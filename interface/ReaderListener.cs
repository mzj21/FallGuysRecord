public interface ReaderListener
{
    /// <summary>
    /// 回合初始化
    /// </summary>
    /// <param name="num">回合数</param>
    /// <param name="levelMap">当前回合信息</param>
    void RoundInit(int num, Levels levelMap);
    /// <summary>
    /// 回合开始
    /// <param name="roundStartTime">回合开始时间</param>
    /// <param name="isPlayerMEAlive">自己是否活着</param>
    /// </summary>
    void RoundStart(System.DateTime roundStartTime, bool isPlayerMEAlive);
    /// <summary>
    /// 回合更新(第一)
    /// </summary>
    /// <param name="player">玩家信息</param>
    /// <param name="time">时间</param>
    void RoundUpdateFirst(Player player, string time);
    /// <summary>
    /// 回合更新(自己)
    /// </summary>
    /// <param name="player">玩家信息</param>
    /// <param name="time">时间</param>
    /// <param name="rank">排名</param>
    void RoundUpdateMe(Player player, string time, int rank);
    /// <summary>
    /// 回合计时器
    /// </summary>
    /// <param name="time">时间</param>
    void RoundUpdateTotal(string time);
    /// <summary>
    /// 回合剩余人数
    /// </summary>
    /// <param name="balance">回合剩余人数</param>
    void RoundBalance(string balance);
    /// <summary>
    /// 回合结束
    /// </summary>
    /// <param name="endtime">结束时间</param>
    /// <param name="isPlaying">是否游戏中</param>
    void RoundEnd(string endtime, bool isPlaying);
    /// <summary>
    /// 回合中途掉线、退出
    /// </summary>
    /// <param name="match">比赛场次</param>
    /// <param name="win">获胜数</param>
    /// <param name="winstreak">连胜数</param>
    /// <param name="wins">获胜人数</param>
    void RoundExit(int match, int win, int winstreak, string wins);
   /// <summary>
   /// 奖励结算
   /// </summary>
   /// <param name="crown">皇冠</param>
   /// <param name="crownShard">皇冠碎片</param>
    void RoundCompletedEpisodeDto(int crown, int crownShard);
    /// <summary>
    /// 个人记录
    /// </summary>
    /// <param name="levelMap">当前回合信息</param>
    /// <param name="time">PB时间</param>
    void RoundPB(Levels levelMap, string time);
    /// <summary>
    /// 更新PING
    /// </summary>
    /// <param name="ping"></param>
    void Ping(string ping);
    /// <summary>
    /// 还原初始状态
    /// </summary>
    void Clear();
}