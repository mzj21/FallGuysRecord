public class Player
{
    public string playerName { get; set; }
    public string name { get; set; }
    public string platform { get; set; }
    public int partyId { get; set; }
    public int squadID { get; set; }
    public int playerId { get; set; }
    public PlayerState playerState { get; set; }
    public enum PlayerState
    {
        QUALIFIED, PLAYING, ELIMINATED
    }

    public Player(string playerName, string name, string platform, int partyId, int squadID, int playerId, PlayerState playerState)
    {
        this.playerName = playerName;
        this.name = name;
        this.platform = platform;
        this.partyId = partyId;
        this.squadID = squadID;
        this.playerId = playerId;
        this.playerState = playerState;
    }

    public override string ToString()
    {
        return $"{{{nameof(playerName)}={playerName}, {nameof(name)}={name}, {nameof(platform)}={platform}, {nameof(partyId)}={partyId.ToString()}, {nameof(squadID)}={squadID.ToString()}, {nameof(playerId)}={playerId.ToString()}, {nameof(playerState)}={playerState.ToString()}}}";
    }

    public string ToLog()
    {
        string r = playerName + " (" + platform + ") ";
        if (partyId > 0)
        {
            r += " partyId=" + partyId;
            //r += " P(" + partyId + ")";
        }
        if (squadID > 0)
        {
            r += " squadID=" + squadID ;
            //r += " S(" + squadID + ")";
        }
        return r;
    }
}