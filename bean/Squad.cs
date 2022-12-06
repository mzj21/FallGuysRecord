using System.Collections.Generic;

public class Squad
{
    public int SquadId { get; set; }
    public int TotalPoints { get; set; }
    public int numPendingScores { get; set; }
    public List<SquadDetail> list_SquadDetail { get; set; }

    public class SquadDetail
    {
        public int playerId { get; set; }
        public int points { get; set; }
        public bool isfinal { get; set; }

        public SquadDetail()
        {
            playerId = 0;
            points = 0;
            isfinal = false;
        }
    }

    public Squad()
    {
        SquadId = 0;
        TotalPoints = 0;
        numPendingScores = 0;
        list_SquadDetail = new List<SquadDetail>();
    }
}