public class Levels
{
    public string matchname { get; set; }
    public string name { get; set; }
    public string showname { get; set; }
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