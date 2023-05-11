public class Levels
{
    public string matchname { get; set; }
    public string name { get; set; }
    public string showname { get; set; }
    //archetype_final #最终回合0, archetype_hunt #捕猎1, archetype_invisibeans #隐形糖豆2,
    //archetype_logic #动脑3, archetype_race #竞速4, archetype_survival #生存5, archetype_team #合作6,
    //archetype_timeattack #计时赛7, else #其他-1
    public int type { get; set; }
    public string typename { get; set; }

    public Levels()
    {
        this.matchname = "";
        this.name = "";
        this.showname = "";
        this.type = -1;
        this.typename = "";
    }
}