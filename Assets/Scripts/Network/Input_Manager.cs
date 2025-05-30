using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

public class Input_Manager : MonoBehaviour, INetworkRunnerCallbacks
{

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
    void INetworkRunnerCallbacks.OnConnectedToServer(NetworkRunner runner) { }

    void INetworkRunnerCallbacks.OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }

    void INetworkRunnerCallbacks.OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }

    void INetworkRunnerCallbacks.OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

    void INetworkRunnerCallbacks.OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }

    void INetworkRunnerCallbacks.OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

    void INetworkRunnerCallbacks.OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

    void INetworkRunnerCallbacks.OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

    void INetworkRunnerCallbacks.OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

    void INetworkRunnerCallbacks.OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }

    void INetworkRunnerCallbacks.OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }

    void INetworkRunnerCallbacks.OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }

    void INetworkRunnerCallbacks.OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }

    void INetworkRunnerCallbacks.OnSceneLoadDone(NetworkRunner runner) { }

    void INetworkRunnerCallbacks.OnSceneLoadStart(NetworkRunner runner) { }

    void INetworkRunnerCallbacks.OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }

    void INetworkRunnerCallbacks.OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }

    void INetworkRunnerCallbacks.OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
}
