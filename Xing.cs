using System;
using System.Collections.Generic;
using System.IO;

public class Xing
{
    public static String LogFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "Low", "Mediatonic", "FallGuys_client", "Player.log");
    public static String LogFile_prev = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "Low", "Mediatonic", "FallGuys_client", "Player-prev.log");
    public static String myName { get; set; }
    public static String pattern_Server = "\\[StateConnectToGame\\] We're connected to the server! Host = ([^:]+)";
    public static String pattern_ShowName = "\\[HandleSuccessfulLogin\\] Selected show is ([^\\s]+)";
    public static String pattern_RoundName = "\\[StateGameLoading\\] Loading game level scene ([^\\s]+) - frame (\\d+)";
    public static String pattern_LoadedRound = "\\[StateGameLoading\\] Finished loading game level, assumed to be ([^.]+)\\.";
    public static String pattern_PlayerSpawn = "\\[CameraDirector\\] Adding Spectator target (.+) with Party ID: (\\d*)  Squad ID: (\\d+) and playerID: (\\d+)";
    public static String pattern_PlayerObjectId = "\\[ClientGameManager\\] Handling bootstrap for [^ ]+ player FallGuy \\[(\\d+)\\].+, playerID = (\\d+)";
    public static String pattern_PlayerResult = "ClientGameManager::HandleServerPlayerProgress PlayerId=(\\d+) is succeeded=([^\\s]+)";
    public static String pattern_PlayerResult2 = "-playerId:(\\d+) points:(\\d+) isfinal:([^\\s]+) name:";
    public static String pattern_Time = "(\\d{2}):(\\d{2}):(\\d{2}).(\\d{3})";
    public static String pattern_VictoryScene = "VictoryScene::winnerPlayerId:(\\d*) name:([^\\s]+) squadId:(\\d*) teamId:(-?\\d*)";
    public static List<LevelMap> list_LevelMap;
}