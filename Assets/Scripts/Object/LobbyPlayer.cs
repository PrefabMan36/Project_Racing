using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;

public class LobbyPlayer : NetworkBehaviour
{
    public static readonly List<LobbyPlayer> players = new List<LobbyPlayer>();

    [SerializeField] public int playerCount;

    public static Action<LobbyPlayer> playerJoined;
    public static Action<LobbyPlayer> playerLeft;
    public static Action<LobbyPlayer> PlayerChanged;

    public static LobbyPlayer localPlayer;

    [Networked] public NetworkBool isReady { get; set; }
    [Networked] public NetworkBool isReadyToPlay { get; set; }
    [Networked] public NetworkBool isSync { get; set; }
    [Networked] public NetworkString<_16> playerName { get; set; }
    [Networked] public NetworkBool finished { get; set; }
    [Networked] public Player_Car car { get; set; }
    [Networked] public eGAMESTATE gameState { get; set; }
    [Networked] public int carIndex { get; set; }

    public bool isHost => Object != null && Object.IsValid && Object.HasStateAuthority;

    private ChangeDetector changeDetector;

    public override void Spawned()
    {
        base.Spawned();

        changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);

        if (Object.HasInputAuthority)
        {
            localPlayer = this;

            PlayerChanged?.Invoke(this);
            RPC_SetPlayerState(Client_Data.Username, Client_Data.CarID);
        }
        players.Add(this);
        playerJoined?.Invoke(this);
        localPlayer.playerCount = players.Count;
        DontDestroyOnLoad(gameObject);
    }

    public override void Render()
    {
        foreach(var change in changeDetector.DetectChanges(this))
        {
            switch(change)
            {
                case nameof(isReady):
                case nameof(playerName):
                    //OnstateChagnge(this);
                    break;
            }
        }
    }

    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
    private void RPC_SetPlayerState(NetworkString<_16> username, int carID)
    {
        playerName = username;
        carIndex = carID;
    }

    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
    public void RPC_SetCarIndex(int id)
    {
        carIndex = id;
    }
    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
    public void RPC_ChangeReadyState(NetworkBool ready)
    {
        Debug.Log($"RPC_ChangeReadyState: {ready}");
        isReady = ready;
    }
    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
    public void RPC_ChangeLoadingState(NetworkBool readyToPlay)
    {
        Debug.Log($"RPC_ChangeReadyState: {readyToPlay}");
        isReadyToPlay = readyToPlay;
    }
    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
    public void RPC_ChangeSyncTrackState(NetworkBool spawnedOver)
    {
        Debug.Log($"RPC_ChangeSyncTrackState: {spawnedOver}");
        isSync = spawnedOver;
    }

    private void OnDisable()
    {
        playerLeft?.Invoke(this);
        players.Remove(this);
    }

    private static void OnStateChanged(LobbyPlayer changed) => PlayerChanged?.Invoke(changed);

    public static void RemovePlayer(NetworkRunner runner, PlayerRef player)
    {
        var lobbyPlayer = players.FirstOrDefault(p => p.Object.InputAuthority == player);
        if (lobbyPlayer != null)
        {
            if (lobbyPlayer.car != null)
                runner.Despawn(lobbyPlayer.car.Object);
            players.Remove(lobbyPlayer);
            runner.Despawn(lobbyPlayer.Object);
            localPlayer.playerCount = players.Count;
        }
    }
}
