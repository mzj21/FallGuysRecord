using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;
using static Player;

public class LogReader
{
    private ReaderListener readerListener;
    private LogListener logListener;
    private Match m;
    private List<Player> list_player = new List<Player>();
    private List<Player> list_player_QUALIFIED = new List<Player>();
    private List<Player> list_player_ELIMINATED = new List<Player>();
    private DateTime roundStartTime;
    private String mode;
    private int match;
    private int round;
    private int win;
    private int rank;
    private Boolean isWin;
    private Boolean isFinal;
    private Timer timer, timerThread;
    private int seek;
    private ReadState readState = ReadState.ROUND_EXIT;
    private String roundName;
    private int interval = 1; //计时器运行间隔, 不影响性能
    private Boolean isRun; //保证线程安全无重复
    private FileStream fs;

    enum ReadState
    {
        ROUND_INIT, ROUND_START, ROUND_UPDATED, ROUND_END, ROUND_EXIT
    }

    public LogReader(ReaderListener Rlistener, LogListener lListener)
    {
        readerListener = Rlistener;
        logListener = lListener;
        list_player = new List<Player>();
    }

    private void LogHeader(string head)
    {
        Xing.LogFileHeader = mode + " " + head;
        logListener.Header(head);
    }

    private void LogDetail(string detail)
    {
        detail = Util.ToDBC(detail);
        Xing.LogFileDetail += detail + Environment.NewLine;
        logListener.Detail(detail);
    }

    public void Start()
    {
        fs = new FileStream(Xing.LogFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        timer = new Timer();
        timer.Interval = interval;
        timerThread = new Timer();
        timerThread.Interval = interval;
        if (File.Exists(Xing.LogFile))
        {
            timerThread.Elapsed += ReadLog;
        }
        timerThread.Start();
    }

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

    void parseLine(String line)
    {
        //Debug.Write(line + Environment.NewLine);
        if (Xing.myName == null && line.Contains("[UserInfo] Player Name:"))
        {
            char[] c = "[UserInfo] Player Name:".ToCharArray();
            string[] sp = line.Split(c);
            Xing.myName = sp[sp.Length - 1];
            LogDetail(Xing.myName);
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
            round = 0;
            ++match;
            if (isWin) { win++; }
            if (isFinal && readState != ReadState.ROUND_UPDATED)
            {
                readerListener.RoundExit(match, win, Util.GetLevelMap(roundName).showname + "(" + list_player_QUALIFIED.Count + ")");
            }
            else
            {
                readerListener.RoundExit(match, win, "");
            }
            //LogDetail("××××××××××××××××××××");
            Debug.Write("与服务器连接中断" + Environment.NewLine);
            readState = ReadState.ROUND_EXIT;
        }
        switch (readState)
        {
            case ReadState.ROUND_EXIT:
                m = Regex.Match(line, Xing.pattern_Server);
                if (m.Success)
                {
                    LogDetail("≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡");
                    LogDetail("IP：" + m.Groups[1]);
                    Debug.Write("服务器：ip:" + m.Groups[1] + Environment.NewLine);
                    readState = ReadState.ROUND_INIT;
                }
                break;
            case ReadState.ROUND_INIT:
                m = Regex.Match(line, Xing.pattern_ShowName);
                if (m.Success)
                {
                    mode = m.Groups[1].Value;
                    LogDetail(mode);
                    Debug.Write("当前模式为：" + mode + Environment.NewLine);
                    break;
                }
                m = Regex.Match(line, Xing.pattern_RoundName);
                if (m.Success)
                {
                    LogDetail(Util.GetLevelMap(m.Groups[1].Value).showname);
                    Debug.Write("当前回合载入成功：" + m.Groups[1].Value + " frame=" + m.Groups[2].Value + Environment.NewLine);
                    LogHeader(Util.GetLevelMap(m.Groups[1].Value).showname);
                    roundName = m.Groups[1].Value;
                    readerListener.RoundInit(++round, roundName);
                    list_player.Clear();
                    list_player_QUALIFIED.Clear();
                    list_player_ELIMINATED.Clear();
                    break;
                }
                m = Regex.Match(line, Xing.pattern_LoadedRound);
                if (m.Success)
                {
                    Debug.Write("具体地图名为：" + m.Groups[1].Value + Environment.NewLine);
                    break;
                }
                m = Regex.Match(line, Xing.pattern_PlayerSpawn);
                if (m.Success)
                {
                    String name = m.Groups[1].Value;
                    int sep = name.IndexOf("_");
                    String platform = name.Substring(0, sep);
                    String playerName = name.Substring(sep + 1).Replace("(.+)$", "").Replace(" (" + platform + ")", "");
                    int partyId = string.IsNullOrEmpty(m.Groups[2].Value) ? 0 : int.Parse(m.Groups[2].Value);
                    int squadId = int.Parse(m.Groups[3].Value);
                    int playerId = int.Parse(m.Groups[4].Value);
                    Player player = new Player(playerName, name, platform, partyId, squadId, playerId);
                    list_player.Add(player);
                    //LogDetail("载入玩家：" + player.ToLog());
                    //Debug.Write("载入玩家：" + player + Environment.NewLine);
                    break;
                }
                if (line.Contains("[StateGameLoading] Starting the game"))
                {
                    Debug.Write("游戏开始：可以转镜头了" + Environment.NewLine);
                    Debug.Write("共计" + list_player.Count + "个玩家" + Environment.NewLine);
                    LogDetail("Players(" + list_player.Count + ")");
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
                    Debug.Write("游戏开始：计时开始" + Environment.NewLine);
                    rank = 0;
                    readState = ReadState.ROUND_UPDATED;
                }
                break;
            case ReadState.ROUND_UPDATED:
                m = Regex.Match(line, Xing.pattern_PlayerResult);
                if (m.Success)
                {
                    isFinal = false;
                    isWin = false;
                    int playerId = int.Parse(m.Groups[1].Value);
                    Boolean succeeded = "True".Equals(m.Groups[2].Value);
                    TimeSpan timeSpan = DateTime.Now - roundStartTime;
                    String time_out = timeSpan.ToString(@"mm\:ss\:ff");
                    foreach (Player player in list_player)
                    {
                        if (player.playerId == playerId)
                        {
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
                                    LogDetail("#" + rank + "   " + player.ToLog() + "  " + time_out);
                                }
                                else
                                {
                                    LogDetail("#" + rank + " " + player.ToLog() + "  " + time_out);
                                }
                                Debug.Write("达标了：排名为" + rank + "  " + player + "  " + time_out + Environment.NewLine);
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
                                    LogDetail("×    " + player.ToLog() + "  " + time_out);
                                    Debug.Write("× " + player + "  " + time_out + Environment.NewLine);
                                }
                            }
                        }
                    }
                    break;
                }
                if (line.Contains("[GameSession] Changing state from Playing to GameOver"))
                {
                    TimeSpan timeSpan = DateTime.Now - roundStartTime;
                    String time_out = timeSpan.ToString(@"mm\:ss\:ff");
                    timer.Stop();
                    readerListener.RoundEnd(time_out);
                    foreach (Player player in list_player)
                    {
                        if (player.playerState == PlayerState.PLAYING)
                        {
                            LogDetail("×     " + player.ToLog());
                        }
                    }
                    LogDetail("--------------------");
                    Debug.Write("回合结束" + Environment.NewLine);
                    readState = ReadState.ROUND_END;
                }
                break;
            case ReadState.ROUND_END:
                if (line.Contains("[GlobalGameStateClient] Received instruction that server is ending a round, and to begin ready-up for next round"))
                {
                    Debug.Write("Received instruction that server is ending a round, and to begin ready-up for next round" + Environment.NewLine);
                    break;
                }
                if (line.Contains("[StateQualificationScreen] Reloading for next round"))
                {
                    readState = ReadState.ROUND_INIT;
                    break;
                }
                if (line.Contains("== [CompletedEpisodeDto] =="))
                {
                    LogDetail("==============================");
                    Debug.Write("整场比赛结束" + Environment.NewLine);
                }
                break;
        }
    }
}