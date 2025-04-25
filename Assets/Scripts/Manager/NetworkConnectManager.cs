using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

public class NetworkConnectManager : MonoBehaviour, INetworkRunnerCallbacks
{
    private NetworkRunner _runner;

    void Awake()
    {
        // NetworkRunner가 아직 없으면 추가합니다.
        if (_runner == null)
        {
            _runner = gameObject.AddComponent<NetworkRunner>();
        }
        _runner.AddCallbacks(this); // 콜백 등록

        Debug.Log("NetworkConnectManager Initialized. Ready to connect.");
    }

    // 씬 로드 시 자동으로 연결을 시작하고 싶다면 이 부분을 사용하세요.
    // 하지만 UI 버튼으로 연결하는 경우 Start에서는 초기화만 하는 것이 좋습니다.
    /*
    async void Start()
    {
        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.AddCallbacks(this);
        await StartHost(); // 예시: 씬 시작 시 바로 호스트로 시작
    }
    */

    // 호스트로 게임 시작
    public async void StartHost()
    {
        if (_runner == null)
        {
            Debug.LogError("NetworkRunner is not initialized!");
            return;
        }
        if (_runner.IsRunning) // 이미 실행 중인지 확인
        {
            Debug.LogWarning("NetworkRunner is already running.");
            return;
        }

        Debug.Log("Attempting to Start Host...");

        var result = await _runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Host, // 호스트 모드
            SessionName = "MyTestSession", // 세션 이름 (참여할 클라이언트와 동일해야 함)
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>() // 씬 매니저 추가 (없다면)
            // 기본 씬 로딩 처리를 위해 NetworkSceneManagerDefault를 사용합니다.
            // 프로젝트 설정에 따라 다른 씬 매니저가 필요할 수 있습니다.
        });

        if (result.Ok)
        {
            // OnConnectedToServer 콜백에서 성공 로그가 출력될 것입니다.
        }
        else
        {
            Debug.LogError($"Failed to start Host: {result.ShutdownReason}");
        }
    }

    // 클라이언트로 게임 참여
    public async void JoinClient()
    {
        if (_runner == null)
        {
            Debug.LogError("NetworkRunner is not initialized!");
            return;
        }
        if (_runner.IsRunning) // 이미 실행 중인지 확인
        {
            Debug.LogWarning("NetworkRunner is already running.");
            return;
        }

        Debug.Log("Attempting to Join Client...");

        var result = await _runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Client, // 클라이언트 모드
            SessionName = "MyTestSession", // 참여할 세션 이름 (호스트와 동일해야 함)
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>() // 씬 매니저 추가 (없다면)
        });

        if (result.Ok)
        {
            // OnConnectedToServer 콜백에서 성공 로그가 출력될 것입니다.
        }
        else
        {
            Debug.LogError($"Failed to join Client: {result.ShutdownReason}");
        }
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

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { Debug.Log($"<color=blue>Player Joined:</color> {player.PlayerId}"); }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { Debug.Log($"<color=blue>Player Left:</color> {player.PlayerId}"); }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { Debug.Log($"Session list updated. Found {sessionList.Count} sessions."); }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadDone(NetworkRunner runner) { Debug.Log("Scene load done."); }
    public void OnSceneLoadStart(NetworkRunner runner) { Debug.Log("Scene load start."); }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        // Debug.Log($"Object {obj.Id} exited AOI for player {player.PlayerId}"); // AOI 관련 로그는 필요에 따라 활성화
    }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        // Debug.Log($"Object {obj.Id} entered AOI for player {player.PlayerId}"); // AOI 관련 로그는 필요에 따라 활성화
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        throw new NotImplementedException();
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        throw new NotImplementedException();
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
        throw new NotImplementedException();
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
        throw new NotImplementedException();
    }

    public void OnInput(NetworkRunner runner, Fusion.NetworkInput input)
    {
        throw new NotImplementedException();
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, Fusion.NetworkInput input)
    {
        throw new NotImplementedException();
    }
}