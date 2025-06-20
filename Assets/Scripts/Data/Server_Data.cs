using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Server_Data
{
    public static int UserCapacity = 4;
    public static string LobbyName;
    public static int LobbyID;
    public static TrackSelect serverTrack;// = Shared.room_Manager.GetTrackByIndex(trackIndex);
    public static bool GameStart;
    public static int trackIndex
    { 
        get => PlayerPrefs.GetInt("TrackIndex", 6);
        set => PlayerPrefs.SetInt("TrackIndex", value); 
    }
}
