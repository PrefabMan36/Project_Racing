using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using static Car;

public class NetworkConnect_Manager : SimulationBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private NetworkPrefabRef playerPrefab;
    [SerializeField] private NetworkPrefabRef playerRank_Prefab;
    [SerializeField] private NetworkPrefabRef gameManagerPrefab;
    [SerializeField] private Transform[] spawnPosition;
    private Dictionary<PlayerRef, byte> spawnedPositions = new Dictionary<PlayerRef, byte>();
    private byte playerCount = 0;
    private byte spawnNum = 0;
    [SerializeField] private MainGame_Manager gameManager;
    private NetworkRunner runner;
    private Dictionary<PlayerRef, NetworkObject> _spwawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

    async void StartGame(GameMode mode)
    {
        if (runner == null)
        {
            runner = gameObject.AddComponent<NetworkRunner>();
            runner.ProvideInput = true;

            var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
            var sceneInfo = new NetworkSceneInfo();
            if(scene.IsValid)
                sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);

            await runner.StartGame(new StartGameArgs()
            {
                GameMode = mode,
                SessionName = "TestRoom",
                Scene = scene,
                SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
            });
        }
    }

    private void OnGUI()
    {
        if (runner == null)
        {
            if (GUI.Button(new Rect(0, 0, 200, 40), "Host"))
                StartGame(GameMode.Host);
            if(GUI.Button(new Rect(0,40,200,40), "Join"))
                StartGame(GameMode.Client);
        }
    }

    public void OnEnable()
    {
        if(runner != null)
            runner.AddCallbacks(this);
    }

    public void OnDisable()
    {
        if (runner != null)
            runner.RemoveCallbacks(this);
    }

    // NetworkRunner 콜백 메서드 (INetworkRunnerCallbacks 인터페이스 구현) - 연결 상태 확인에 유용

    public void OnConnectedToServer(NetworkRunner runner)
    {
        Debug.Log("<color=green>Successfully connected to server!</color>");
        // 연결 성공 후 필요한 로직 (예: 플레이어 생성, 로비 진입 등)을 여기에 구현합니다.
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        Debug.LogError($"<color=red>Connection Failed:</color> {reason}");
        // 연결 실패 시 사용자에게 알림 등을 표시할 수 있습니다.
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log($"<color=orange>Network Runner Shutdown:</color> {shutdownReason}");
        // 연결이 종료되거나 러너가 중지될 때 호출됩니다.
    }
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if(runner.IsServer)
        {
            spawnNum = (byte)UnityEngine.Random.Range(0, spawnPosition.Length - playerCount++);
            NetworkObject networkPlayerObject = runner.Spawn(playerPrefab, spawnPosition[spawnNum].position, spawnPosition[spawnNum].rotation, player);
            spawnPosition = SortToBack_Array(spawnPosition, spawnNum);
            spawnedPositions.Add(player, spawnNum);
            gameManager.OnJoinPlayer(networkPlayerObject);
            _spwawnedCharacters.Add(player, networkPlayerObject);
            Debug.Log($"<color=blue>Player Joined:</color> {player.PlayerId}");
        }
    }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if(_spwawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
        {
            gameManager.OnLeftPlayer(networkObject);
            runner.Despawn(networkObject);
            spawnPosition = SortToFront_Array(spawnPosition, spawnedPositions[player]);
            _spwawnedCharacters.Remove(player);
            spawnedPositions.Remove(player);
            playerCount--;
            Debug.Log($"<color=blue>Player Left:</color> {player.PlayerId}");
        }
    }
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var inputData = new NetworkInputManager();
        inputData.direction.x = Input.GetAxis("Horizontal");
        inputData.direction.y = Input.GetAxis("Vertical");
        inputData.direction.z = Input.GetAxis("Clutch");
        inputData.sideBraking = Input.GetAxis("Jump") > 0 ? true : false;
        inputData.boosting = Input.GetKey(KeyCode.RightShift);
        inputData.gearUP = Input.GetKey(KeyCode.LeftShift);
        inputData.gearDOWN = Input.GetKey(KeyCode.LeftControl);
        inputData.forceGear = 0;
        if (Input.GetKey(KeyCode.Keypad0))
            inputData.forceGear = 1;
        if (Input.GetKey(KeyCode.Keypad1))
            inputData.forceGear = 2;
        if (Input.GetKey(KeyCode.Keypad2))
            inputData.forceGear = 3;
        if (Input.GetKey(KeyCode.Keypad3))
            inputData.forceGear = 4;
        if (Input.GetKey(KeyCode.Keypad4))
            inputData.forceGear = 5;
        if (Input.GetKey(KeyCode.Keypad5))
            inputData.forceGear = 6;
        if (Input.GetKey(KeyCode.Keypad6))
            inputData.forceGear = 7;

        input.Set(inputData);
    }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { Debug.Log($"Session list updated. Found {sessionList.Count} sessions."); }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadDone(NetworkRunner runner) { Debug.Log("Scene load done."); }
    public void OnSceneLoadStart(NetworkRunner runner) { Debug.Log("Scene load start."); }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    { Debug.Log($"Object {obj.Id} exited AOI for player {player.PlayerId}"); }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    { Debug.Log($"Object {obj.Id} entered AOI for player {player.PlayerId}"); }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) {}

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) {}

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) {}

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) {}

    private Transform[] SortToBack_Array(Transform[] _array, int changeNum)
    {
        Queue<Transform> newArray = new Queue<Transform>(_array.Length - 1);
        for (int i = 0; i < _array.Length; i++)
        {
            if(i != changeNum)
                newArray.Enqueue(_array[i]);
        }
        newArray.Enqueue(_array[changeNum]);
        return newArray.ToArray();
    }

    private Transform[] SortToFront_Array(Transform[] _array, int changeNum)
    {
        Queue<Transform> newArray = new Queue<Transform>(_array.Length - 1);
        newArray.Enqueue(_array[changeNum]);
        for (int i = 0; i < _array.Length; i++)
        {
            if (i != changeNum)
                newArray.Enqueue(_array[i]);
        }
        return newArray.ToArray();
    }
}