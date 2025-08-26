using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

public static class Shared
{
    public static float frame60 = 0.01f;
    public static float frame30 = 0.02f;
    public static float frame15 = 0.04f;

    public static Game_Manager game_Manager;
    public static Audio_Manager audio_Manager;
    public static Scene_manager scene_Manager;
    public static UI_Manager ui_Manager;
    public static Setting_Manager setting_Manager;
    public static Room_Manager room_Manager;
    public static Lobby_Manager lobby_Manager;
    public static Lobby_Network_Manager lobby_Network_Manager;
    public static CarSelect_Manager CarSelect_Manager;
    public static WaypointManager waypointManager;

    public static AsyncOperationHandle<SceneInstance> CurrentAddressableSceneHandle;

    public static float audioVolume;

    public static bool mainGameManagerSpawned;

    public static string CarName;
    public static int CarIndex;
    public static string UserID;
}
