using System;

public interface ReaderListener
{
    /// <summary>
    /// 回合初始化
    /// </summary>
    /// <param name="num">回合数</param>
    /// <param name="levelMap">当前回合信息</param>
    void RoundInit(int num, LevelMap levelMap);
    /// <summary>
    /// 回合开始
    /// </summary>
    void RoundStart();
    /// <summary>
    /// 回合更新(第一)
    /// </summary>
    /// <param name="player">玩家信息</param>
    /// <param name="time">时间</param>
    void RoundUpdateFirst(Player player, String time);
    /// <summary>
    /// 回合更新(自己)
    /// </summary>
    /// <param name="player">玩家信息</param>
    /// <param name="time">时间</param>
    void RoundUpdateMe(Player player, String time);
    /// <summary>
    /// 回合计时器
    /// </summary>
    /// <param name="time">时间</param>
    void RoundUpdateTotal(String time);
    /// <summary>
    /// 回合结束
    /// </summary>
    /// <param name="endtime">结束时间</param>
    void RoundEnd(String endtime);
    /// <summary>
    /// 回合中途掉线、退出
    /// </summary>
    /// <param name="match">比赛场次</param>
    /// <param name="win">获胜数</param>
    /// <param name="wins">获胜人数</param>
    void RoundExit(int match, int win, String wins);
    /// <summary>
    /// 更新PING
    /// </summary>
    /// <param name="ping"></param>
    void Ping(String ping);
}