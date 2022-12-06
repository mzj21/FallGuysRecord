using System;
using System.Collections.Generic;
using System.IO;

public class Xing
{
    public static string LogFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "Low", "Mediatonic", "FallGuys_client", "Player.log");
    public static string LogFile_prev = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "Low", "Mediatonic", "FallGuys_client", "Player-prev.log");
    public static string myName { get; set; }
    public static string pattern_Server = "\\[StateConnectToGame\\] We're connected to the server! Host = ([^:]+)";
    public static string pattern_ShowName = "\\[HandleSuccessfulLogin\\] Selected show is ([^\\s]+)";
    public static string pattern_RoundName = "\\[StateGameLoading\\] Loading game level scene ([^\\s]+) - frame (\\d+)";
    public static string pattern_LoadedRound = "\\[StateGameLoading\\] Finished loading game level, assumed to be ([^.]+)\\.";
    public static string pattern_PlayerSpawn = "\\[CameraDirector\\] Adding Spectator target (.+) \\((.+)\\) with Party ID: (\\d*)  Squad ID: (\\d+) and playerID: (\\d+)";
    public static string pattern_PlayerObjectId = "\\[ClientGameManager\\] Handling bootstrap for [^ ]+ player FallGuy \\[(\\d+)\\].+, playerID = (\\d+)";
    public static string pattern_PlayerResult = "ClientGameManager::HandleServerPlayerProgress PlayerId=(\\d+) is succeeded=([^\\s]+)";
    public static string pattern_PlayerResult2 = "-playerId:(\\d+) points:(\\d+) isfinal:([^\\s]+) name:";
    public static string pattern_Time = "(\\d{2}):(\\d{2}):(\\d{2}).(\\d{3})";
    public static string pattern_VictoryScene = "VictoryScene::winnerPlayerId:(\\d*) name:([^\\s]+) squadId:(\\d*) teamId:(-?\\d*)";
    public static string pattern_PrivateLobby = "\\[GameStateMachine\\] Replacing FGClient.StateMainMenu with FGClient.StatePrivateLobby";
    public static string pattern_Squad = "SquadId:(\\d*) TotalPoints:(\\d*) numPendingScores:(\\d*)";
    public static string pattern_SquadDetail = "-playerId:(\\d*) points:(\\d*) isfinal:([^\\s]+)";
    public static List<Levels> list_Levels;
    public static List<Shows> list_Shows;
}