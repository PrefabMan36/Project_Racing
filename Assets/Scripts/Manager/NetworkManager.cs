using Fusion;
using UnityEngine;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using Fusion.Sockets;
using System;

public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    private NetworkRunner netRunner;
    public NetworkPrefabRef PlayerPrefab;
    void Start()
    {
        netRunner = GetComponent<NetworkRunner>();
        if(netRunner == null )
            netRunner = gameObject.AddComponent<NetworkRunner>();
        netRunner.AddCallbacks(this);
        StartGame(GameMode.Host, "TestSession");
    }
    async void StartGame(GameMode mode, string sessionName)
    {
        Debug.Log($"Starting game in {mode} mode, session: {sessionName}");
        netRunner.ProvideInput = true; //입력을 제공하겠다고 설정
        await netRunner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = sessionName,
            SceneManager = GetComponent<INetworkSceneManager>()// 씬 관리가 필요하면 추가
        });
    }
    public void Onplayerjoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"{player.PlayerId} joined");
        if (runner.Mode == SimulationModes.Host) // 호스트만 차량을 스폰
        {
            runner.Spawn(PlayerPrefab,
                         Vector3.zero, // 스폰 위치 (적절히 조정 필요)
                         Quaternion.identity, // 스폰 회전
                         player); // 이 플레이어가 이 차량의 Input Authority를 가집니다.
        }
    }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    { Debug.Log($"{player.PlayerId} left"); }

    // NetworkRunner가 입력값을 요청할 때 호출 (로컬 플레이어의 입력 수집)
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        //// 로컬 플레이어의 입력값 수집
        input.isAccelerating = Input.GetKey(KeyCode.UpArrow);
        input.isBraking = Input.GetKey(KeyCode.DownArrow);
        input.isSideBraking = Input.GetKey(KeyCode.Space);
        input.isSteering = Input.GetAxis("Horizontal");
        input.isBoosting = Input.GetKey(KeyCode.RightShift);
        // 수집된 입력값을 Fusion에 전달
        //input = data;
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    { throw new NotImplementedException();}
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    { throw new NotImplementedException(); }
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    { throw new NotImplementedException(); }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    { throw new NotImplementedException(); }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    { throw new NotImplementedException(); }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    { throw new NotImplementedException(); }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    { throw new NotImplementedException(); }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    { throw new NotImplementedException(); }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    { throw new NotImplementedException(); }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    { throw new NotImplementedException(); }
    public void OnInput(NetworkRunner runner, Fusion.NetworkInput input)
    { throw new NotImplementedException(); }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, Fusion.NetworkInput input)
    { throw new NotImplementedException(); }
    public void OnConnectedToServer(NetworkRunner runner)
    { throw new NotImplementedException(); }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    { throw new NotImplementedException(); }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    { throw new NotImplementedException(); }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    { throw new NotImplementedException(); }
    public void OnSceneLoadDone(NetworkRunner runner)
    { throw new NotImplementedException(); }
    public void OnSceneLoadStart(NetworkRunner runner)
    { throw new NotImplementedException(); }
}
