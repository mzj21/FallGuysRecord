using FallGuysRecord.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Timers;
using static Player;
using static System.Windows.Forms.AxHost;

public class LogReader
{
    private ReaderListener readerListener;
    private LogListener logListener;
    private Match m;
    private List<Player> list_player = new List<Player>();
    private List<Player> list_player_QUALIFIED = new List<Player>();
    private List<Player> list_player_ELIMINATED = new List<Player>();
    private DateTime roundStartTime; //回合开始时间
    private int match; //比赛场数
    private int round; //第几回合
    private int win; //获胜数量
    private int rank; //回合排名
    private Boolean isWin; //是否获胜
    private Boolean isFinal; //用于判断平冠
    private Timer timer, timerThread, timerProcess;
    private FileStream fs;
    private int seek;
    private ReadState readState = ReadState.ROUND_EXIT;
    private int interval = 1; //计时器运行间隔, 不影响性能
    private Boolean isRun; //保证线程安全无重复
    private Boolean isMatchStart; //是否是一场比赛
    private LevelMap levelMap = new LevelMap(); //当前回合信息
    private TimeSpan timeSpan;
    private Player player_temp;

    enum ReadState
    {
        ROUND_INIT, ROUND_START, ROUND_UPDATED, ROUND_END, ROUND_EXIT
    }

    public LogReader(ReaderListener Rlistener, LogListener lListener)
    {
        readerListener = Rlistener;
        logListener = lListener;
    }

    private void LogHeader()
    {
        String s = levelMap.showname;
        if (list_player.Count > 0)
        {
            s += " players(" + list_player.Count + ")";
        }
        logListener.Header(s);
    }

    public void Start()
    {
        fs = new FileStream(Xing.LogFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        timer = new Timer();
        timer.Interval = interval;
        timerProcess = new Timer();
        timerProcess.Interval = interval;
        timerProcess.Elapsed += CheckLogReset;
        timerProcess.Start();
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
        }
    }

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
        fs.Seek(seek, SeekOrigin.Begin);
        StreamReader reader = new StreamReader(fs);
        isRun = true;
        String line;
        while ((line = reader.ReadLine()) != null)
        {
            seek += line.Length;
            parseLine(line);
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
        if (Xing.myName == null && line.Contains("[UserInfo] Player Name:"))
        {
            char[] c = "[UserInfo] Player Name:".ToCharArray();
            string[] sp = line.Split(c);
            Xing.myName = sp[sp.Length - 1];
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
        if (line.Contains("Client has been disconnected"))
        {
            if (isMatchStart)
            {
                round = 0;
                ++match;
                if (isWin) { win++; }
                if (isFinal && readState != ReadState.ROUND_UPDATED)
                {
                    readerListener.RoundExit(match, win, levelMap.type + "(" + list_player_QUALIFIED.Count + ")");
                }
                else
                {
                    readerListener.RoundExit(match, win, "");
                }
                logListener.Detail("〓〓〓〓〓〓〓〓〓〓");
                Debug.WriteLine("与服务器连接中断，是一场比赛");
                isMatchStart = false;
            }
            readState = ReadState.ROUND_EXIT;
        }
        switch (readState)
        {
            case ReadState.ROUND_EXIT:
                m = Regex.Match(line, Xing.pattern_Server);
                if (m.Success)
                {
                    logListener.Detail("IP：" + m.Groups[1].Value);
                    Debug.WriteLine("服务器ip:" + m.Groups[1].Value);
                    isWin = false;
                    readState = ReadState.ROUND_INIT;
                }
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
                    isMatchStart = true;
                    list_player.Clear();
                    list_player_QUALIFIED.Clear();
                    list_player_ELIMINATED.Clear();
                    levelMap = Util.GetLevelMap(m.Groups[1].Value);
                    logListener.Detail(levelMap.showname);
                    LogHeader();
                    readerListener.RoundInit(++round, levelMap);
                    Debug.WriteLine("当前回合载入成功：" + m.Groups[1].Value + " frame=" + m.Groups[2].Value);
                    break;
                }
                m = Regex.Match(line, Xing.pattern_LoadedRound);
                if (m.Success)
                {
                    Debug.WriteLine("具体地图名为：" + m.Groups[1].Value);
                    break;
                }
                m = Regex.Match(line, Xing.pattern_PlayerSpawn);
                if (m.Success)
                {
                    String name = m.Groups[1].Value;
                    int sep = name.IndexOf("_");
                    String platform = name.Substring(0, sep);
                    String playerName = name.Substring(sep + 1).Replace(" (" + platform + ")", "");
                    int partyId = string.IsNullOrEmpty(m.Groups[2].Value) ? 0 : int.Parse(m.Groups[2].Value);
                    int squadId = int.Parse(m.Groups[3].Value);
                    int playerId = int.Parse(m.Groups[4].Value);
                    Player player = new Player(playerName, name, platform, partyId, squadId, playerId, PlayerState.PLAYING);
                    list_player.Add(player);
                    LogHeader();
                    //logListener.Detail("载入玩家：" + player.ToLog());
                    //Debug.WriteLine("载入玩家：" + player);
                    break;
                }
                
                if (line.Contains("Setting this client as readiness state 'ObjectsSpawned'")) {
                    Debug.WriteLine("共计" + list_player.Count + "个玩家");
                    logListener.Detail("Players(" + list_player.Count + ")");
                    logListener.Detail(getCount("bots") + getCount("win") + getCount("switch") + getCount("ps4") + getCount("ps5") + getCount("xsx") + getCount("xb1"));
                    LogHeader();
                    break ;
                }
                if (line.Contains("[StateGameLoading] Starting the game"))
                {
                    readState = ReadState.ROUND_START;
                }
                break;
            case ReadState.ROUND_START:
                if (line.Contains("[GameSession] Changing state from Countdown to Playing"))
                {
                    readerListener.RoundStart();
                    roundStartTime = DateTime.Now;
                    timer.Interval = interval;
                    timer.Start();
                    Debug.WriteLine("游戏开始：计时开始");
                    rank = 0;
                    readState = ReadState.ROUND_UPDATED;
                }
                break;
            case ReadState.ROUND_UPDATED:
                m = Regex.Match(line, Xing.pattern_PlayerResult);
                if (m.Success)
                {
                    isFinal = false;
                    int playerId = int.Parse(m.Groups[1].Value);
                    Boolean succeeded = "True".Equals(m.Groups[2].Value);
                    timeSpan = DateTime.Now - roundStartTime;
                    String time_out = timeSpan.ToString(@"mm\:ss\:ff");
                    foreach (Player player in list_player)
                    {
                        if (player.playerId == playerId && player.playerState == PlayerState.PLAYING)
                        {
                            player_temp = player;
                            if (succeeded)
                            {
                                isFinal = true;
                                player.playerState = PlayerState.QUALIFIED;
                                rank++;
                                if (rank == 1)
                                {
                                    readerListener.RoundUpdateFirst(player, time_out);
                                }
                                if (player.playerName.Equals(Xing.myName))
                                {
                                    isWin = true;
                                    readerListener.RoundUpdateMe(player, time_out);
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
                                    if (player.playerName.Equals(Xing.myName))
                                    {
                                        readerListener.RoundUpdateMe(player, time_out);
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
                    LogHeader();
                    break;
                }
                if (line.Contains("[GameSession] Changing state from Playing to GameOver"))
                {
                    timeSpan = DateTime.Now - roundStartTime;
                    String time_out = timeSpan.ToString(@"mm\:ss\:ff");
                    timer.Stop();
                    readerListener.RoundEnd(time_out);
                    foreach (Player player in list_player)
                    {
                        logListener.Detail("×     " + player.ToLog());
                    }
                    LogHeader();
                    logListener.Detail("--------------------");
                    Debug.WriteLine("回合结束");
                    readState = ReadState.ROUND_END;
                }
                break;
            case ReadState.ROUND_END:
                if (line.Contains("[GlobalGameStateClient] Received instruction that server is ending a round, and to begin ready-up for next round"))
                {
                    Debug.WriteLine("Received instruction that server is ending a round, and to begin ready-up for next round");
                    break;
                }
                if (line.Contains("[StateQualificationScreen] Reloading for next round"))
                {
                    readState = ReadState.ROUND_INIT;
                    break;
                }
                if (line.Contains("== [CompletedEpisodeDto] =="))
                {
                    Debug.WriteLine("你的比赛结束了，可以观战");
                    readState = ReadState.ROUND_INIT;
                }
                break;
        }
    }
}