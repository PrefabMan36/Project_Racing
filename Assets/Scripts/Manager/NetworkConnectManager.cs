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
        // NetworkRunner�� ���� ������ �߰��մϴ�.
        if (_runner == null)
        {
            _runner = gameObject.AddComponent<NetworkRunner>();
        }
        _runner.AddCallbacks(this); // �ݹ� ���

        Debug.Log("NetworkConnectManager Initialized. Ready to connect.");
    }

    // �� �ε� �� �ڵ����� ������ �����ϰ� �ʹٸ� �� �κ��� ����ϼ���.
    // ������ UI ��ư���� �����ϴ� ��� Start������ �ʱ�ȭ�� �ϴ� ���� �����ϴ�.
    /*
    async void Start()
    {
        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.AddCallbacks(this);
        await StartHost(); // ����: �� ���� �� �ٷ� ȣ��Ʈ�� ����
    }
    */

    // ȣ��Ʈ�� ���� ����
    public async void StartHost()
    {
        if (_runner == null)
        {
            Debug.LogError("NetworkRunner is not initialized!");
            return;
        }
        if (_runner.IsRunning) // �̹� ���� ������ Ȯ��
        {
            Debug.LogWarning("NetworkRunner is already running.");
            return;
        }

        Debug.Log("Attempting to Start Host...");

        var result = await _runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Host, // ȣ��Ʈ ���
            SessionName = "MyTestSession", // ���� �̸� (������ Ŭ���̾�Ʈ�� �����ؾ� ��)
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>() // �� �Ŵ��� �߰� (���ٸ�)
            // �⺻ �� �ε� ó���� ���� NetworkSceneManagerDefault�� ����մϴ�.
            // ������Ʈ ������ ���� �ٸ� �� �Ŵ����� �ʿ��� �� �ֽ��ϴ�.
        });

        if (result.Ok)
        {
            // OnConnectedToServer �ݹ鿡�� ���� �αװ� ��µ� ���Դϴ�.
        }
        else
        {
            Debug.LogError($"Failed to start Host: {result.ShutdownReason}");
        }
    }

    // Ŭ���̾�Ʈ�� ���� ����
    public async void JoinClient()
    {
        if (_runner == null)
        {
            Debug.LogError("NetworkRunner is not initialized!");
            return;
        }
        if (_runner.IsRunning) // �̹� ���� ������ Ȯ��
        {
            Debug.LogWarning("NetworkRunner is already running.");
            return;
        }

        Debug.Log("Attempting to Join Client...");

        var result = await _runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Client, // Ŭ���̾�Ʈ ���
            SessionName = "MyTestSession", // ������ ���� �̸� (ȣ��Ʈ�� �����ؾ� ��)
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>() // �� �Ŵ��� �߰� (���ٸ�)
        });

        if (result.Ok)
        {
            // OnConnectedToServer �ݹ鿡�� ���� �αװ� ��µ� ���Դϴ�.
        }
        else
        {
            Debug.LogError($"Failed to join Client: {result.ShutdownReason}");
        }
    }

    // NetworkRunner �ݹ� �޼��� (INetworkRunnerCallbacks �������̽� ����) - ���� ���� Ȯ�ο� ����

    public void OnConnectedToServer(NetworkRunner runner)
    {
        Debug.Log("<color=green>Successfully connected to server!</color>");
        // ���� ���� �� �ʿ��� ���� (��: �÷��̾� ����, �κ� ���� ��)�� ���⿡ �����մϴ�.
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        Debug.LogError($"<color=red>Connection Failed:</color> {reason}");
        // ���� ���� �� ����ڿ��� �˸� ���� ǥ���� �� �ֽ��ϴ�.
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log($"<color=orange>Network Runner Shutdown:</color> {shutdownReason}");
        // ������ ����ǰų� ���ʰ� ������ �� ȣ��˴ϴ�.
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
        // Debug.Log($"Object {obj.Id} exited AOI for player {player.PlayerId}"); // AOI ���� �α״� �ʿ信 ���� Ȱ��ȭ
    }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        // Debug.Log($"Object {obj.Id} entered AOI for player {player.PlayerId}"); // AOI ���� �α״� �ʿ信 ���� Ȱ��ȭ
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