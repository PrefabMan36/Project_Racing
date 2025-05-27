using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Fusion;
using Fusion.Addons.Physics;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Lobby_Network_Manager : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private Game_Manager gameManager_Prefab;
    [SerializeField] private LobbyPlayer playerPrefab;
    [SerializeField] private GameMode gameMode;

    public static eCONNECTIONSTATUS connectionstatus = eCONNECTIONSTATUS.DISCONNECTED;

    private NetworkRunner networkRunner;
    private Scene_manager sceneManager;

    private void Start()
    {
        Application.runInBackground = true;
        Application.targetFrameRate = (int)Screen.currentResolution.refreshRateRatio.value;
        QualitySettings.vSyncCount = 1;

        DontDestroyOnLoad(this);
        int lobbySceneIndex = (int)eSCENE.eSCENE_LOBBY;
        SceneManager.LoadScene(lobbySceneIndex);
    }

    public void SetCreateLobby() => gameMode = GameMode.Host;

    public void SetJoinLobby() => gameMode = GameMode.Client;

    public void JoinOrCreateLobby()
    {
        SetConnectionStatus(eCONNECTIONSTATUS.CONNECTING);

        if (networkRunner == null)
            QuitSession();

        GameObject sessionObject = new GameObject("Session");
        DontDestroyOnLoad(sessionObject);

        networkRunner = sessionObject.AddComponent<NetworkRunner>();
        var sim3D = sessionObject.AddComponent<RunnerSimulatePhysics3D>();
        sim3D.ClientPhysicsSimulation = ClientPhysicsSimulation.SimulateAlways;

        networkRunner.ProvideInput = gameMode != GameMode.Server;
        networkRunner.AddCallbacks(this);
        Debug.Log($"Created gameobject {sessionObject.name} - starting game");
        networkRunner.StartGame(new StartGameArgs
        {
            GameMode = gameMode,
            SessionName = gameMode == GameMode.Host ? Server_Data.LobbyID.ToString() : Server_Data.LobbyID.ToString(),
            PlayerCount = Server_Data.UserCapacity,
            EnableClientSessionCreation = false
        });
    }

    private void SetConnectionStatus(eCONNECTIONSTATUS status)
    {
        Debug.Log($"Setting connection status to : {status}");
        connectionstatus = status;
        if (!Application.isPlaying)
            return;
        if(status == eCONNECTIONSTATUS.DISCONNECTED || status == eCONNECTIONSTATUS.FAILED)
        {
            SceneManager.LoadScene((int)eSCENE.eSCENE_LOBBY);
        }
    }

    public void QuitSession()
    {
        if(networkRunner != null)
            networkRunner.Shutdown();
        else
            SetConnectionStatus(eCONNECTIONSTATUS.DISCONNECTED);
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        Debug.Log("Connected to server");
        SetConnectionStatus(eCONNECTIONSTATUS.CONNECTED);
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        Debug.Log("Disconnected from server");
        QuitSession();
        SetConnectionStatus(eCONNECTIONSTATUS.DISCONNECTED);
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        if (runner.TryGetSceneInfo(out var scene) && scene.SceneCount > 0)
        {
            Debug.LogWarning($"Refused connection requested by {request.RemoteAddress}");
            request.Refuse();
        }
        else
            request.Accept(); 
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        Debug.Log($"Connect failed {reason}");
        QuitSession();
        SetConnectionStatus(eCONNECTIONSTATUS.FAILED);
        //(string status, string message) = 
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Player joined: {player.PlayerId}");
        if (runner.IsServer)
        {
            if (gameMode == GameMode.Host)
                runner.Spawn(gameManager_Prefab, Vector3.zero, Quaternion.identity);
            var lobbyPlatyer = runner.Spawn(playerPrefab, Vector3.zero, Quaternion.identity, player);
            lobbyPlatyer.gameState = eGAMESTATE.LOBBY;
        }
        SetConnectionStatus(eCONNECTIONSTATUS.CONNECTED);
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Player left: {player.PlayerId}");
        LobbyPlayer.RemovePlayer(runner, player);
        SetConnectionStatus(connectionstatus);
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log($"Runner shutdown: {shutdownReason}");
        SetConnectionStatus(eCONNECTIONSTATUS.DISCONNECTED);

        LobbyPlayer.players.Clear();

        if(networkRunner)
            Destroy(networkRunner.gameObject);

        networkRunner = null;
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    { }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    { }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    { }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    { }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    { }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    { }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    { }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    { }

    public void OnSceneLoadDone(NetworkRunner runner)
    { }

    public void OnSceneLoadStart(NetworkRunner runner)
    { }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    { }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    { }
}
