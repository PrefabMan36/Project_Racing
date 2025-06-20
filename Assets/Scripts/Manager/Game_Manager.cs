using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class Game_Manager : NetworkBehaviour
{
    public static event Action<Game_Manager> OnLobbyUpdated;

    [SerializeField, Layer] private int groundLayer;
    public static int GroundLayer => Shared.game_Manager.groundLayer;
    [SerializeField, Layer] private int carLayer;
    public static int CarLayer => Shared.game_Manager.carLayer;

    public Camera Camera;

    public string trackName;

    [Networked] public NetworkString<_32> lobbyName { get; set; }
    [Networked] public int lobbyID { get; set; }
    [Networked, SerializeField] public int trackIndex { get; set; }
    [Networked] public int MaxUsers { get; set; }
    [Networked] public bool gameStart { get; set; }

    private static void OnLobbyChangedCallback(Game_Manager manager)
    { OnLobbyUpdated?.Invoke(manager); }

    private ChangeDetector changeDetector;

    private void Awake()
    {
        if(Shared.game_Manager != null)
        {
            Destroy(gameObject);
            return;
        }
        Shared.game_Manager = this;
        DontDestroyOnLoad(gameObject);
        StartCoroutine(RaceStart());
    }

    public override void Spawned()
    {
        base.Spawned();

        changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        if(Object.HasStateAuthority)
        {
            lobbyName = Server_Data.LobbyName;
            lobbyID = Server_Data.LobbyID;
            trackIndex = Server_Data.trackIndex;
            MaxUsers = Server_Data.UserCapacity;
            gameStart = Server_Data.GameStart;
        }
    }

    public override void Render()
    {
        foreach(var chage in changeDetector.DetectChanges(this))
        {
            switch (chage)
            {
                case nameof(lobbyName):
                case nameof(trackIndex):
                case nameof(MaxUsers):
                    OnLobbyChangedCallback(this);
                    break;
            }
        }
    }
    IEnumerator RaceStart()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(Shared.frame15);
        while (true)
        {
            yield return waitForSeconds;
            Shared.scene_Manager.SetNextScene(trackIndex);
            if(gameStart)
            {
                Shared.scene_Manager.ChangeScene(Shared.room_Manager.GetTrackEnum(trackIndex));
                gameStart = false;
            }
        }
    }
}
