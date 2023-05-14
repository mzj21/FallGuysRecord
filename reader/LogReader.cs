using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Timers;
using static Player;
using static Squad;

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
    private Timer timer, timerThread, timerCheckLogReset, timerCheckProcess;
    private FileStream fs;
    private int seek; //指针
    private ReadState readState = ReadState.ROUND_INIT;
    private int interval = 1; //计时器运行间隔, 不影响性能
    private bool isRun; //保证线程安全无重复
    private bool isMatchStart; //是否是一场比赛，匹配开始
    private bool isRoundStart; //是否是一场比赛，回合开始
    private Levels level = new Levels(); //当前回合信息
    private TimeSpan timeSpan; //用于计算时间
    private Player player_temp; //零时存贮Player
    private string line_temp;
    private Player player_me; //Player自己
    private int playerId_me; //自己的PlayerId
    private bool isPlayerMEAlive; //自己是否活着
    private bool isFallGuysAlive; //糖豆人进程是否存在,接口只运行一次
    private RoundCompletedEpisodeDto roundCompletedEpisodeDto; //结算奖励
    private RoundCompletedEpisodeDto.Round roundDto; //结算奖励 具体回合信息
    private bool isAnalysisCompletedEpisodeDto; //是否开始解析结算奖励
    private bool isAnalysisRound; //是否开始解析结算奖励的具体回合信息
    private int winstreak; //连胜次数
    private string LogFile_temp;
    private string matchname;
    private int crown; // 皇冠
    private int crownShard; // 皇冠碎片
    private List<string> list_RoundName = new List<string>(); // 保持回合名字
    private List<Squad> list_squad = new List<Squad>(); // 小队积分统计
    private Squad squad_temp;
    private Squad.SquadDetail squadDetail_temp;
    private bool isAnalysisSquad; //是否开始解析小队积分统计
    private bool isCustomShows; //是否是自定义专题

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
        string s = $"{level.matchname} {level.showname}";
        if (list_player.Count > 0)
        {
            s += $" {Util.getResourcesString("Player")}({list_player.Count})";
        }
        logListener.Header(s);
    }

    /// <summary>
    /// 用于外部修改地图信息
    /// </summary>
    public void ChangelevelMap()
    {
        level = Util.GetLevels(level.name);
    }

    /// <summary>
    /// 开启
    /// </summary>
    public void Start()
    {
        fs = new FileStream(Xing.LogFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        LogFile_temp = Xing.LogFile;
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
        if (Xing.LogFile != LogFile_temp) //修改log，用于debug
        {
            Clear();
        }
    }

    /// <summary>
    /// 重置输入流
    /// </summary>
    private void Clear()
    {
        logListener.Clear();
        readerListener.Clear();
        if (Util.FileExists(Xing.LogFile))
        {
            fs = new FileStream(Xing.LogFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        }
        else
        {
            Xing.LogFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "Low", "Mediatonic", "FallGuys_client", "Player.log");
            fs = new FileStream(Xing.LogFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        }
        seek = 0;
        match = 0;
        round = 0;
        win = 0;
        LogFile_temp = Xing.LogFile;
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
            if (isMatchStart) { winstreak = 0; }
            round = 0;
            readState = ReadState.ROUND_INIT;
            logListener.Detail("≡≡≡≡≡≡≡≡≡≡");
            Debug.WriteLine("进程消失，比赛结束");
            readerListener.RoundExit(match, win, winstreak, level.typename + (list_player_Winner.Count > 0 ? $"({list_player_Winner.Count})" : ""));
            isMatchStart = false;
            isRoundStart = false;
            isCustomShows = false;
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
    private string getCount(string p)
    {
        return $"{p}({list_player.FindAll(x => x.platform.Equals(p)).Count}) ";
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
        if (fs != null)
        {
            fs.Seek(seek, SeekOrigin.Begin);
            StreamReader reader = new StreamReader(fs);
            while ((line_temp = reader.ReadLine()) != null)
            {
                seek += line_temp.Length + 1; //+1是换行符
                parseLine(line_temp);
            }
        }
        isRun = false;
    }

    /// <summary>
    /// 处理log，以一行一行的处理
    /// </summary>
    /// <param name="line">log单行</param>
    void parseLine(string line)
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
        if (!isCustomShows && line.Contains("[GameStateMachine] Replacing FGClient.StateMainMenu with FGClient.StatePrivateLobby"))
        {
            isCustomShows = true;
            Debug.WriteLine("切换至自定义模式");
        }
        if (isCustomShows && (line.Contains("to FGClient.StateMainMenu") || line.Contains("with FGClient.StateMainMenu")))
        {
            isCustomShows = false;
            Debug.WriteLine("切换至主界面");
        }
        m = Regex.Match(line, Xing.pattern_Server);
        if (m.Success)
        {
            round = 0; //强制关闭需要重置回合数
            logListener.Detail($"Server IP：{m.Groups[1].Value}");
            Debug.WriteLine($"服务器ip:{m.Groups[1].Value}");
            readState = ReadState.ROUND_INIT;
        }
        if (!isCustomShows && line.Contains("== [CompletedEpisodeDto] =="))
        {
            if (readState != ReadState.ROUND_UPDATED)
            {
                readState = ReadState.ROUND_INIT;
            }
            isAnalysisCompletedEpisodeDto = true;
            Debug.WriteLine("奖励已结算");
        }
        // 开始统计奖励
        CompletedEpisodeDto(line);
        m = Regex.Match(line, Xing.pattern_VictoryScene);
        if (m.Success)
        {
            // 奖励被卡掉BUG的情况暂时未处理，目前看了下不需要处理
            int winnerPlayerId = int.Parse(m.Groups[1].Value);
            if (winnerPlayerId == playerId_me)
            {
                win++;
                winstreak++;
            }
            else
            {
                winstreak = 0;
            }
            Setlist_player_Winner();
            readerListener.RoundExit(match, win, winstreak, level.typename + (list_player_Winner.Count > 0 ? $"({list_player_Winner.Count})" : ""));
            Debug.WriteLine(m.Groups[0].Value);
            Debug.WriteLine("获胜界面");
        }
        if (line.Contains("FG_NetworkManager commencing shutdown") || line.Contains("Client has been disconnected") || line.Contains("[FG_UnityInternetNetworkManager] client quit"))
        {
            if ((isMatchStart && roundCompletedEpisodeDto != null && roundCompletedEpisodeDto.ListRound.Count == 0) || (roundCompletedEpisodeDto != null && roundCompletedEpisodeDto.ListRound.Count > 0 && roundCompletedEpisodeDto.ListRound.Last().Qualified == false))
            {
                winstreak = 0;
            }
            if (isMatchStart && isRoundStart)
            {
                if (readState == ReadState.ROUND_INIT)
                {
                    Setlist_player_Winner();
                    readerListener.RoundExit(match, win, winstreak, level.typename + (list_player_Winner.Count > 0 ? $"({list_player_Winner.Count})" : ""));
                    logListener.Detail("〓〓〓〓〓〓〓〓〓〓");
                    Debug.WriteLine("比赛结束");
                }
                isMatchStart = false;
                isRoundStart = false;
            }
            round = 0;
            readState = ReadState.ROUND_INIT;
            readerListener.RoundExit(match, win, winstreak, level.typename + (list_player_Winner.Count > 0 ? $"({list_player_Winner.Count})" : ""));
            //Debug.WriteLine(roundCompletedEpisodeDto);
            isAnalysisCompletedEpisodeDto = false;
            Debug.WriteLine("中断连接");
        }
        if (line.Contains("Final Squads Positions"))
        {
            list_squad.Clear();
            isAnalysisSquad = true;
            Debug.WriteLine("开始统计小队积分");
        }
        FinalSquadsPositions(line);
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
                    matchname = Util.GetShows(m.Groups[1].Value).showname;
                    Debug.WriteLine($"当前模式为：{matchname}");
                    logListener.Detail(matchname);
                    break;
                }
                m = Regex.Match(line, Xing.pattern_RoundName);
                if (m.Success)
                {
                    Debug.WriteLine($"当前回合载入成功：{m.Groups[1].Value} frame={m.Groups[2].Value}");
                    break;
                }
                m = Regex.Match(line, Xing.pattern_LoadedRound);
                if (m.Success)
                {
                    if (!isMatchStart)
                    {
                        ++match;
                        list_RoundName.Clear();
                    }
                    isMatchStart = true;
                    isRoundStart = true;
                    ++round;
                    roundCompletedEpisodeDto = new RoundCompletedEpisodeDto();

                    list_player.Clear();
                    list_player_QUALIFIED.Clear();
                    list_player_ELIMINATED.Clear();
                    level = Util.GetLevels(m.Groups[1].Value);
                    level.matchname = matchname;
                    if (level.sharecode == String.Empty)
                    {
                        logListener.Detail(level.showname + "(" + round + ")");
                    }
                    else
                    {
                        logListener.Detail(level.showname + " " + level.sharecode + " (" + round + ")");
                    }
                    LogHeader();
                    readerListener.RoundInit(round, level);
                    list_RoundName.Add(level.showname);
                    Debug.WriteLine($"具体地图名为：{m.Groups[1].Value}");
                    break;
                }
                string id = "Requesting spawn of local player, ID=";
                if (line.Contains(id))
                {
                    playerId_me = int.Parse(line.Substring(line.IndexOf(id) + id.Length));
                    break;
                }
                m = Regex.Match(line, Xing.pattern_PlayerSpawn);
                if (m.Success)
                {
                    string name = m.Groups[1].Value;
                    int sep = name.IndexOf("_");
                    if (name.Contains("..."))
                        sep = -1;
                    string playerName = name.Substring(sep + 1);
                    string platform = m.Groups[2].Value;
                    int partyId = string.IsNullOrEmpty(m.Groups[3].Value) ? 0 : int.Parse(m.Groups[3].Value);
                    int squadId = int.Parse(m.Groups[4].Value);
                    int playerId = int.Parse(m.Groups[5].Value);
                    if (playerName.Equals(("...")))
                        playerName = "Player " + playerId;
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
                    readerListener.RoundBalance(level.typename + "(" + list_player.Count + ")");
                    //Debug.WriteLine("载入玩家：" + player);
                    break;
                }

                if (line.Contains("[ClientGameManager] Setting this client as readiness state 'ReadyToPlay'"))
                {
                    Debug.WriteLine($"共计{list_player.Count}个玩家");
                    list_player_RoundAll.Clear();
                    list_player.ForEach(p => list_player_RoundAll.Add(p));
                    logListener.Detail($"{Util.getResourcesString("Player")}({list_player.Count})");
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
                    bool succeeded = "True".Equals(m.Groups[2].Value);
                    m = Regex.Match(line, Xing.pattern_Time);
                    if (m.Success)
                    {
                        tenpTime = new DateTime(tenpTime.Year, tenpTime.Month, tenpTime.Day, int.Parse(m.Groups[1].Value), int.Parse(m.Groups[2].Value), int.Parse(m.Groups[3].Value), int.Parse(m.Groups[4].Value));
                    }
                    timeSpan = tenpTime - roundStartTime;
                    string time_out = timeSpan.ToString(@"mm\:ss\:fff");
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
                                    logListener.Detail(player.playerId == playerId_me ? $"▶#{rank}   {player.ToLog()}  {time_out}" : $"#{rank}   {player.ToLog()}  {time_out}");
                                }
                                else
                                {
                                    logListener.Detail(player.playerId == playerId_me ? $"▶#{rank} {player.ToLog()}  {time_out}" : $"#{rank} {player.ToLog()}  {time_out}");
                                }
                                Debug.WriteLine($"达标了：排名为{rank}  {player}  {time_out}");
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
                                    logListener.Detail($"×    {player.ToLog()}  {time_out}");
                                    Debug.WriteLine($"× {player}  {time_out}");
                                }
                            }
                        }
                    }
                    list_player.Remove(player_temp);
                    readerListener.RoundBalance($"{level.typename}({list_player.Count})");
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
                    string time_out = timeSpan.ToString(@"mm\:ss\:fff");
                    timer.Stop();
                    if (player_me != null)
                    {
                        readerListener.RoundEnd(time_out, player_me.playerState == PlayerState.PLAYING);
                    }
                    foreach (Player player in list_player)
                    {
                        logListener.Detail($"×     {player.ToLog()}");
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
        if (!isAnalysisCompletedEpisodeDto)
        {
            return;
        }
        if (line.Contains("> Kudos: ") && !isAnalysisRound) //总紫币
        {
            roundCompletedEpisodeDto.Kudos = int.Parse(line.Substring("> Kudos: ".Length));
        }
        if (line.Contains("> Fame: ") && !isAnalysisRound) //总经验
        {
            roundCompletedEpisodeDto.Fame = int.Parse(line.Substring("> Fame: ".Length));
        }
        if (line.Contains("> Crowns: ") && !isAnalysisRound) //皇冠
        {
            roundCompletedEpisodeDto.Crowns = int.Parse(line.Substring("> Crowns: ".Length));
            crown += roundCompletedEpisodeDto.Crowns;
        }
        if (line.Contains("> CurrentCrownShards: ") && !isAnalysisRound) //皇冠碎片
        {
            roundCompletedEpisodeDto.CurrentCrownShards = int.Parse(line.Substring("> CurrentCrownShards: ".Length));
            if (roundCompletedEpisodeDto.CurrentCrownShards < crownShard)
            {
                crown++;
                Debug.WriteLine("crown = " + crown);
            }
            crownShard = roundCompletedEpisodeDto.CurrentCrownShards;
            readerListener.RoundCompletedEpisodeDto(crown, crownShard);
        }
        if (line.Contains("[Round ")) //回合
        {
            isAnalysisRound = true;
            roundDto = new RoundCompletedEpisodeDto.Round();
            roundDto.RoundNum = int.Parse(line.Substring("[Round ".Length, 2));
            roundDto.RoundName = line.Substring(line.IndexOf("| ") + 2).Replace("]", "");
            roundDto.RoundShowName = list_RoundName[roundDto.RoundNum];
        }
        if (isAnalysisRound && line.Contains("> Qualified: ")) //是否过关
        {
            roundDto.Qualified = bool.Parse(line.Substring("> Qualified: ".Length));
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

    /// <summary>
    /// 解析小队积分统计
    /// </summary>
    /// <param name="line">单行log</param>
    private void FinalSquadsPositions(string line)
    {
        if (!isAnalysisSquad)
        {
            return;
        }
        m = Regex.Match(line, Xing.pattern_Squad);
        if (m.Success)
        {
            if (squad_temp != null)
            {
                list_squad.Add(squad_temp);
            }
            squad_temp = new Squad();
            squad_temp.SquadId = int.Parse(m.Groups[1].Value);
            squad_temp.TotalPoints = int.Parse(m.Groups[2].Value);
            squad_temp.numPendingScores = int.Parse(m.Groups[3].Value);
        }
        m = Regex.Match(line, Xing.pattern_SquadDetail);
        if (m.Success)
        {
            squadDetail_temp = new Squad.SquadDetail();
            squadDetail_temp.playerId = int.Parse(m.Groups[1].Value);
            squadDetail_temp.points = int.Parse(m.Groups[2].Value);
            squadDetail_temp.isfinal = bool.Parse(m.Groups[3].Value);
            squad_temp.list_SquadDetail.Add(squadDetail_temp);
        }
        if (string.IsNullOrEmpty(line))
        {
            isAnalysisSquad = false;
            squad_temp = null;
            if (list_squad.Count > 0)
            {
                foreach (Squad squad in list_squad)
                {
                    if (squad.SquadId == player_me.squadID)
                    {
                        logListener.Detail($"{Util.getResourcesString("Points")}={squad.TotalPoints}");
                        foreach (SquadDetail squadDetail in squad.list_SquadDetail)
                        {
                            Player player = list_player_RoundAll.Find(p => p.playerId == squadDetail.playerId);
                            if (player != null)
                            {
                                logListener.Detail($"{Util.getResourcesString("Points")}={Util.NumPadRight(squad.TotalPoints.ToString().Length, squadDetail.points.ToString().Length)}{squadDetail.points} {player.playerName}");
                                //logListener.Detail($"{player.playerName} {Util.getResourcesString("Points")}={squadDetail.points} {Util.getResourcesString("Qualify")}={squadDetail.isfinal}");
                            }
                        }
                        logListener.Detail("--------------------");
                    }
                }
            }
        }
    }
}