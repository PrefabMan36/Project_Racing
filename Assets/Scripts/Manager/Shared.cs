using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Shared
{
    public static float frame60 = 0.01f;
    public static float frame30 = 0.02f;
    public static float frame15 = 0.04f;

    public static Game_Manager game_Manager;
    public static Music_Manager music_Manager;
    public static Scene_manager scene_Manager;
    public static UI_Manager ui_Manager;
    public static Setting_Manager setting_Manager;
    public static Room_Manager room_Manager;
    public static Lobby_Network_Manager lobby_Network_Manager;

    public static float audioVolume;

    public static string CarName;
    public static int CarIndex;
    public static string UserID;
}
