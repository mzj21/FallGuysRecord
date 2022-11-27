using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Timers;
using static Player;

/// <summary>
/// 读取log线程
/// </summary>
public class LogReader
{
    private ReaderListener readerListener; //读取反馈接口
    private LogListener logListener; //回合信息接口
    private Match m; //正则
    private List<Player> list_player = new List<Player>();
    private List<Player> list_player_QUALIFIED = new List<Player>();
    private List<Player> list_player_ELIMINATED = new List<Player>();
    private List<Player> list_player_Winner = new List<Player>();
    private List<Player> list_player_RoundAll = new List<Player>();
    private DateTime roundStartTime; //回合开始时间
    private DateTime tenpTime;
    private int match; //比赛场数
    private int round; //第几回合
    private int win; //获胜数量
    private int rank; //回合排名.
    private Boolean isWin; //是否获胜
    private Timer timer, timerThread, timerCheckLogReset, timerCheckProcess;
    private FileStream fs;
    private int seek; //指针
    private ReadState readState = ReadState.ROUND_INIT;
    private int interval = 1; //计时器运行间隔, 不影响性能
    private Boolean isRun; //保证线程安全无重复
    private Boolean isMatchStart; //是否是一场比赛，匹配开始
    private Boolean isRoundStart; //是否是一场比赛，回合开始
    private LevelMap levelMap = new LevelMap(); //当前回合信息
    private TimeSpan timeSpan; //用于计算时间
    private Player player_temp; //零时存贮Player
    private String line_temp;
    private Player player_me; //Player自己
    private int playerId_me; //自己的PlayerId
    private Boolean isPlayerMEAlive; //自己是否活着
    private Boolean isFallGuysAlive; //糖豆人进程是否存在,接口只运行一次
    private RoundCompletedEpisodeDto roundCompletedEpisodeDto; //结算奖励
    private RoundCompletedEpisodeDto.Round roundDto; //结算奖励 具体回合信息
    private Boolean isAnalysisCompletedEpisodeDto; //是否开始解析结算奖励
    private Boolean isAnalysisRound; //是否开始解析结算奖励的具体回合信息
    private int winstreak; //连胜次数

    enum ReadState
    {
        ROUND_INIT, ROUND_START, ROUND_UPDATED, ROUND_END, ROUND_EXIT
    }

    public LogReader(ReaderListener Rlistener, LogListener lListener)
    {
        readerListener = Rlistener;
        logListener = lListener;
    }

    /// <summary>
    /// 回合信息头部修改
    /// </summary>
    private void LogHeader()
    {
        String s = levelMap.showname;
        if (list_player.Count > 0)
        {
            s += " players(" + list_player.Count + ")";
        }
        logListener.Header(s);
    }

    /// <summary>
    /// 用于外部修改地图信息
    /// </summary>
    public void ChangelevelMap()
    {
        levelMap = Util.GetLevelMap(levelMap.name);
    }

    /// <summary>
    /// 开启
    /// </summary>
    public void Start()
    {
        fs = new FileStream(Xing.LogFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        timer = new Timer();
        timer.Interval = interval;
        timerCheckLogReset = new Timer();
        timerCheckLogReset.Interval = interval;
        timerCheckLogReset.Elapsed += CheckLogReset;
        timerCheckLogReset.Start();

        timerCheckProcess = new Timer();
        timerCheckProcess.Interval = interval;
        timerCheckProcess.Elapsed += CheckProcess;
        timerCheckProcess.Start();


        timerThread = new Timer();
        timerThread.Interval = interval;
        if (File.Exists(Xing.LogFile))
        {
            timerThread.Elapsed += ReadLog;
        }
        timerThread.Start();
    }

    /// <summary>
    /// 检测log是否被重置，将seek归零
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CheckLogReset(object sender, ElapsedEventArgs e)
    {
        if (Util.isLogReset(Xing.LogFile) && seek > 0)
        {
            seek = 0;
            Debug.WriteLine("CheckLogReset");
            timerThread.Interval = interval;
            if (File.Exists(Xing.LogFile))
            {
                timerThread.Elapsed += ReadLog;
            }
            timerThread.Start();
        }
    }

    /// <summary>
    /// 检测进程
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CheckProcess(object sender, ElapsedEventArgs e)
    {
        if (isFallGuysAlive && !Util.isFallGuysAlive())
        {
            isFallGuysAlive = false;
            timerThread.Stop();
            if (!isWin) { winstreak = 0; }
            round = 0;
            readState = ReadState.ROUND_INIT;
            logListener.Detail("≡≡≡≡≡≡≡≡≡≡");
            Debug.WriteLine("进程消失，比赛结束");
            readerListener.RoundExit(match, win, levelMap.type + (list_player_Winner.Count > 0 ? "(" + list_player_Winner.Count + ")" : ""));
            isMatchStart = false;
            isRoundStart = false;
        }
        if (Util.isFallGuysAlive())
        {
            isFallGuysAlive = true;
        }
    }

    /// <summary>
    /// 统计平台人数
    /// </summary>
    /// <param name="p">平台类型</param>
    /// <returns></returns>
    private String getCount(String p)
    {
        return p + "(" + list_player.FindAll(x => x.platform.Equals(p)).Count + ") ";
    }

    /// <summary>
    /// 读取log
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ReadLog(object sender, ElapsedEventArgs e)
    {
        if (isRun)
            return;
        isRun = true;
        fs.Seek(seek, SeekOrigin.Begin);
        StreamReader reader = new StreamReader(fs);
        while ((line_temp = reader.ReadLine()) != null)
        {
            seek += line_temp.Length + 1; //+1是换行符
            parseLine(line_temp);
        }
        isRun = false;
    }

    /// <summary>
    /// 处理log，以一行一行的处理
    /// </summary>
    /// <param name="line">log单行</param>
    void parseLine(String line)
    {
        //Debug.WriteLine(line);
        tenpTime = DateTime.Now.ToUniversalTime();
        if (Xing.myName == null && line.Contains("[UserInfo] Player Name: "))
        {
            Xing.myName = line.Substring(line.IndexOf("Name: ") + 6);
            logListener.Detail(Xing.myName);
        }
        if (line.Contains("Client address:"))
        {
            int index = line.IndexOf("RTT: ");
            if (index > 0)
            {
                int msIndex = line.IndexOf("ms", index);
                readerListener.Ping(line.Substring(index + 5, msIndex - index - 5));
            }
        }
        m = Regex.Match(line, Xing.pattern_Server);
        if (m.Success)
        {
            round = 0; //强制关闭需要重置回合数
            logListener.Detail("Server IP：" + m.Groups[1].Value);
            Debug.WriteLine("服务器ip:" + m.Groups[1].Value);
            readState = ReadState.ROUND_INIT;
        }
        if (line.Contains("== [CompletedEpisodeDto] =="))
        {
            readState = ReadState.ROUND_INIT;
            roundCompletedEpisodeDto = new RoundCompletedEpisodeDto();
            isAnalysisCompletedEpisodeDto = true;
            Debug.WriteLine("奖励已结算");
        }
        m = Regex.Match(line, Xing.pattern_VictoryScene);
        if (m.Success)
        {
            // BUG的情况暂时未处理，没有具体log情况
            int winnerPlayerId = int.Parse(m.Groups[1].Value);
            if (winnerPlayerId == playerId_me)
            {
                isWin = true;
                win++;
                winstreak++;
            }
            else
            {
                winstreak = 0;
            }
            Setlist_player_Winner();
            readerListener.RoundExit(match, win, levelMap.type + (list_player_Winner.Count > 0 ? "(" + list_player_Winner.Count + ")" : ""));
            Debug.WriteLine(m.Groups[0].Value);
            Debug.WriteLine("获胜界面");
        }
        CompletedEpisodeDto(line);
        if (line.Contains("FG_NetworkManager commencing shutdown") || line.Contains("Client has been disconnected") || line.Contains("[FG_UnityInternetNetworkManager] client quit"))
        {
            if (isMatchStart && isRoundStart)
            {
                if (readState == ReadState.ROUND_INIT)
                {
                    Setlist_player_Winner();
                    readerListener.RoundExit(match, win, levelMap.type + (list_player_Winner.Count > 0 ? "(" + list_player_Winner.Count + ")" : ""));
                    logListener.Detail("〓〓〓〓〓〓〓〓〓〓");
                    Debug.WriteLine("比赛结束");
                }
                isMatchStart = false;
                isRoundStart = false;
            }
            if (!isWin) { winstreak = 0; }
            round = 0;
            readState = ReadState.ROUND_INIT;
            readerListener.RoundExit(match, win, levelMap.type + (list_player_Winner.Count > 0 ? "(" + list_player_Winner.Count + ")" : ""));
            isAnalysisCompletedEpisodeDto = false;
            Debug.WriteLine("中断连接");
        }
        switch (readState)
        {
            case ReadState.ROUND_EXIT:
                // 此项暂时无用
                readState = ReadState.ROUND_INIT;
                break;
            case ReadState.ROUND_INIT:
                m = Regex.Match(line, Xing.pattern_ShowName);
                if (m.Success)
                {
                    Debug.WriteLine("当前模式为：" + m.Groups[1]);
                    logListener.Detail(m.Groups[1].Value);
                    break;
                }
                m = Regex.Match(line, Xing.pattern_RoundName);
                if (m.Success)
                {
                    if (!isMatchStart) { ++match; }
                    isMatchStart = true;
                    isRoundStart = true;
                    isWin = false;
                    ++round;
                    list_player.Clear();
                    list_player_QUALIFIED.Clear();
                    list_player_ELIMINATED.Clear();
                    levelMap = Util.GetLevelMap(m.Groups[1].Value);
                    logListener.Detail(levelMap.showname + "(" + round + ")");
                    LogHeader();
                    readerListener.RoundInit(round, levelMap);
                    Debug.WriteLine("当前回合载入成功：" + m.Groups[1].Value + " frame=" + m.Groups[2].Value);
                    break;
                }
                m = Regex.Match(line, Xing.pattern_LoadedRound);
                if (m.Success)
                {
                    Debug.WriteLine("具体地图名为：" + m.Groups[1].Value);
                    break;
                }
                String id = "Requesting spawn of local player, ID=";
                if (line.Contains(id))
                {
                    playerId_me = int.Parse(line.Substring(line.IndexOf(id) + id.Length));
                    break;
                }
                m = Regex.Match(line, Xing.pattern_PlayerSpawn);
                if (m.Success)
                {
                    String name = m.Groups[1].Value;
                    int sep = name.IndexOf("(");
                    String playerName = name.Substring(0, sep - 1);
                    String platform = name.Substring(sep + 1).Replace(")", "");
                    int partyId = string.IsNullOrEmpty(m.Groups[2].Value) ? 0 : int.Parse(m.Groups[2].Value);
                    int squadId = int.Parse(m.Groups[3].Value);
                    int playerId = int.Parse(m.Groups[4].Value);
                    Player player = new Player(playerName, name, platform, partyId, squadId, playerId, PlayerState.PLAYING);
                    if (!list_player.Contains(player))
                    {
                        list_player.Add(player);
                    }
                    if (player.playerId == playerId_me)
                    {
                        player_me = player;
                        isPlayerMEAlive = true;
                    }
                    LogHeader();
                    readerListener.RoundBalance(levelMap.type + "(" + list_player.Count + ")");
                    //logListener.Detail("载入玩家：" + player.ToLog());
                    //Debug.WriteLine("载入玩家：" + player);
                    break;
                }

                if (line.Contains("[ClientGameManager] Setting this client as readiness state 'ReadyToPlay'"))
                {
                    Debug.WriteLine("共计" + list_player.Count + "个玩家");
                    list_player_RoundAll.Clear();
                    list_player.ForEach(p => list_player_RoundAll.Add(p));
                    logListener.Detail("Players(" + list_player.Count + ")");
                    logListener.Detail(getCount("bots") + getCount("win") + getCount("switch") + getCount("ps4") + getCount("ps5") + getCount("xsx") + getCount("xb1"));
                    LogHeader();
                    break;
                }
                if (line.Contains("[StateGameLoading] Starting the game"))
                {
                    readState = ReadState.ROUND_START;
                }
                break;
            case ReadState.ROUND_START:
                if (line.Contains("[GameSession] Changing state from Countdown to Playing"))
                {
                    roundStartTime = tenpTime;
                    m = Regex.Match(line, Xing.pattern_Time);
                    if (m.Success)
                    {
                        roundStartTime = new DateTime(tenpTime.Year, tenpTime.Month, tenpTime.Day, int.Parse(m.Groups[1].Value), int.Parse(m.Groups[2].Value), int.Parse(m.Groups[3].Value), int.Parse(m.Groups[4].Value));
                    }
                    readerListener.RoundStart(roundStartTime, isPlayerMEAlive);
                    timer.Interval = interval;
                    timer.Start();
                    Debug.WriteLine("游戏开始：计时开始");
                    list_player_Winner.Clear();
                    rank = 0;
                    readState = ReadState.ROUND_UPDATED;
                }
                break;
            case ReadState.ROUND_UPDATED:
                m = Regex.Match(line, Xing.pattern_PlayerResult);
                if (m.Success)
                {
                    int playerId = int.Parse(m.Groups[1].Value);
                    Boolean succeeded = "True".Equals(m.Groups[2].Value);
                    m = Regex.Match(line, Xing.pattern_Time);
                    if (m.Success)
                    {
                        tenpTime = new DateTime(tenpTime.Year, tenpTime.Month, tenpTime.Day, int.Parse(m.Groups[1].Value), int.Parse(m.Groups[2].Value), int.Parse(m.Groups[3].Value), int.Parse(m.Groups[4].Value));
                    }
                    timeSpan = tenpTime - roundStartTime;
                    String time_out = timeSpan.ToString(@"mm\:ss\:fff");
                    foreach (Player player in list_player)
                    {
                        if (player.playerId == playerId && player.playerState == PlayerState.PLAYING)
                        {
                            player_temp = player;
                            if (succeeded)
                            {
                                player.playerState = PlayerState.QUALIFIED;
                                rank++;
                                if (rank == 1)
                                {
                                    readerListener.RoundUpdateFirst(player, time_out);
                                }
                                if (player.playerId == playerId_me)
                                {
                                    readerListener.RoundUpdateMe(player, time_out, rank);
                                }
                                list_player_QUALIFIED.Add(player);
                                if (rank < 10)
                                {
                                    logListener.Detail("#" + rank + "   " + player.ToLog() + "  " + time_out);
                                }
                                else
                                {
                                    logListener.Detail("#" + rank + " " + player.ToLog() + "  " + time_out);
                                }
                                Debug.WriteLine("达标了：排名为" + rank + "  " + player + "  " + time_out);
                            }
                            else
                            {
                                if (player.playerState == PlayerState.PLAYING)
                                {
                                    if (player.playerId == playerId_me)
                                    {
                                        readerListener.RoundUpdateMe(player, time_out, rank);
                                    }
                                    player.playerState = PlayerState.ELIMINATED;
                                    list_player_ELIMINATED.Add(player);
                                    logListener.Detail("×    " + player.ToLog() + "  " + time_out);
                                    Debug.WriteLine("× " + player + "  " + time_out);
                                }
                            }
                        }
                    }
                    list_player.Remove(player_temp);
                    readerListener.RoundBalance(levelMap.type + "(" + list_player.Count + ")");
                    LogHeader();
                    break;
                }
                if (line.Contains("[ClientGameManager] Server notifying that the round is over."))
                {
                    m = Regex.Match(line, Xing.pattern_Time);
                    if (m.Success)
                    {
                        tenpTime = new DateTime(tenpTime.Year, tenpTime.Month, tenpTime.Day, int.Parse(m.Groups[1].Value), int.Parse(m.Groups[2].Value), int.Parse(m.Groups[3].Value), int.Parse(m.Groups[4].Value));
                    }
                    timeSpan = tenpTime - roundStartTime;
                    String time_out = timeSpan.ToString(@"mm\:ss\:fff");
                    timer.Stop();
                    readerListener.RoundEnd(time_out, player_me.playerState == PlayerState.PLAYING);
                    foreach (Player player in list_player)
                    {
                        logListener.Detail("×     " + player.ToLog());
                    }
                    LogHeader();
                    logListener.Detail("--------------------");
                    isPlayerMEAlive = false;
                    Debug.WriteLine("回合结束");
                    readState = ReadState.ROUND_END;
                }
                break;
            case ReadState.ROUND_END:
                //if (line.Contains("[GlobalGameStateClient] Received instruction that server is ending a round, and to begin ready-up for next round"))
                //{
                //    Debug.WriteLine("Received instruction that server is ending a round, and to begin ready-up for next round");
                //    break;
                //}
                if (line.Contains("[StateQualificationScreen] Reloading for next round"))
                {
                    readState = ReadState.ROUND_INIT;
                    Debug.WriteLine("准备下一回合");
                    break;
                }
                break;
        }
    }

    /// <summary>
    /// 处理奖励
    /// </summary>
    /// <param name="line">log单行</param>
    private void CompletedEpisodeDto(string line)
    {
        if (!isAnalysisCompletedEpisodeDto) { return; }
        if (line
            .Contains("> Kudos: ") && !isAnalysisRound) //总紫币
        {
            roundCompletedEpisodeDto.Kudos = int.Parse(line.Substring("> Kudos: ".Length));
        }
        if (line.Contains("> Fame: ") && !isAnalysisRound) //总经验
        {
            roundCompletedEpisodeDto.Kudos = int.Parse(line.Substring("> Fame: ".Length));
        }
        if (line.Contains("> Crowns: ") && !isAnalysisRound) //皇冠
        {
            roundCompletedEpisodeDto.Kudos = int.Parse(line.Substring("> Crowns: ".Length));
        }
        if (line.Contains("> CurrentCrownShards: ") && !isAnalysisRound) //皇冠碎片
        {
            roundCompletedEpisodeDto.Kudos = int.Parse(line.Substring("> CurrentCrownShards: ".Length));
        }
        if (line.Contains("[Round ")) //回合
        {
            isAnalysisRound = true;
            roundDto = new RoundCompletedEpisodeDto.Round();
            roundDto.RoundNum = int.Parse(line.Substring("[Round ".Length, 2));
            roundDto.RoundName = line.Substring(line.IndexOf("| ") + 2).Replace("]", "");
        }
        if (isAnalysisRound && line.Contains("> Qualified: ")) //是否过关
        {
            roundDto.Qualified = Boolean.Parse(line.Substring("> Qualified: ".Length));
        }
        if (isAnalysisRound && line.Contains("> Position: "))
        {
            roundDto.Position = int.Parse(line.Substring("> Position: ".Length));
        }
        if (isAnalysisRound && line.Contains("> Kudos: ")) //回合紫币
        {
            roundDto.Kudos = int.Parse(line.Substring("> Kudos: ".Length));
        }
        if (isAnalysisRound && line.Contains("> Fame: ")) //回合经验
        {
            roundDto.Fame = int.Parse(line.Substring("> Fame: ".Length));
        }
        if (isAnalysisRound && line.Contains("> Bonus Tier: ")) //额外奖励奖牌类型，0=金，1=银，2=铜，3=粉
        {
            if (!string.IsNullOrEmpty(line.Substring("> Bonus Tier: ".Length)))
            {
                roundDto.BonusTier = int.Parse(line.Substring("> Bonus Tier: ".Length));
            }
        }
        if (isAnalysisRound && line.Contains("> Bonus Kudos: ")) //额外奖励紫币
        {
            roundDto.BonusKudos = int.Parse(line.Substring("> Bonus Kudos: ".Length));
        }
        if (isAnalysisRound && line.Contains("> Bonus Fame: ")) //额外奖励经验
        {
            roundDto.BonusFame = int.Parse(line.Substring("> Bonus Fame: ".Length));
        }
        if (isAnalysisRound && line.Contains("> BadgeId: ")) //回合奖牌名称，与Bonus Tier同步
        {
            if (!string.IsNullOrEmpty(line.Substring("> BadgeId: ".Length)))
            {
                roundDto.BadgeId = line.Substring("> BadgeId: ".Length);
            }
            roundCompletedEpisodeDto.ListRound.Add(roundDto);
            isAnalysisRound = false;
        }
    }

    /// <summary>
    /// 统计获胜人数
    /// </summary>
    private void Setlist_player_Winner()
    {
        list_player_Winner.Clear();
        foreach (Player p in list_player_QUALIFIED)
        {
            list_player_Winner.Add(p);
            foreach (Player p_all in list_player_RoundAll)
            {
                if (p.squadID > 0 && p.squadID == p_all.squadID && !list_player_Winner.Contains(p_all))
                {
                    list_player_Winner.Add(p_all);
                }
            }
            list_player_Winner = list_player_Winner.Distinct().ToList();
        }
    }
}