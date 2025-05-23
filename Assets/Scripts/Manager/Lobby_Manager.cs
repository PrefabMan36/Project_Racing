using System.Collections;
using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Lobby_Manager : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private Canvas mainCanvas;

    [Header("Map Info")]
    [SerializeField] private Image trackImage;
    [SerializeField] private TextMeshProUGUI trackName;
    //[Networked, OnChanged = nameof(OnMapChanged)] // 맵 인덱스 네트워크 동기화 및 변경 감지
    private int currentMapIndex;
    private TrackSelect selectedTrack;

    [Header("Player List")]
    [SerializeField] private Transform scrollContent;
    [SerializeField] private LobbyUserBox lobbyUser_Prefab;

    [Header("Popups")]
    [SerializeField] private GameObject quitPopup;
    [SerializeField] private GameObject forceStartPopup;
    [SerializeField] private GameObject changeTrackPopup;
    [SerializeField] private GameObject changeCarPopup;

    [Header("Buttons")]
    [SerializeField] private GameObject StartButton;
    [SerializeField] private GameObject readyButton;
    [SerializeField] private GameObject changeTrackButton;

    public void SetLobby(string _lobbyName, int _mapIndex)
    {
        lobbyNameText.text = _lobbyName;
        mainCanvas = Shared.ui_Manager.GetMainCanvas();
        SetMapInfoUI(_mapIndex);
        if (!Runner.IsServer)
        {
            currentMapIndex = _mapIndex;
            StartButton.SetActive(true);
            changeTrackButton.SetActive(true);
        }
        else
        {
            StartButton.SetActive(false);
            changeTrackButton.SetActive(false);
        }
    }

    private void OnMapChanged()
    {
    }

    private void SetMapInfoUI(int _mapIndex)
    {
        selectedTrack = Shared.room_Manager.GetTrackSelect(currentMapIndex);
        trackImage.sprite = selectedTrack.mapImage;
        trackName.text = selectedTrack.mapName;
    }

    public override void Spawned()
    {
        base.Spawned();

        UpdatePlayerList();
    }

    private void UpdatePlayerList()
    {
        //foreach (var entry in playerListEntries.Values)
        //{
        //    Destroy(entry.gameObject);
        //}
        //playerListEntries.Clear();

        //if (Runner != null && Runner.ActivePlayers != null)
        //{
        //    foreach(PlayerRef player in Runner.ActivePlayers)
        //    {
        //        AddPlayerToList(player);
        //    }
        //}
    }

    //private void AddPlayerToList(PlayerRef player)
    //{
    //    if (!playerListEntries.ContainsKey(player))
    //    {
    //        LobbyUserBox entry = Instantiate(lobbyUser_Prefab, scrollContent);
    //        entry.gameObject.SetActive(true);
    //        playerListEntries.Add(player, entry);
    //    }
    //}
    //private void RemovePlayerFromList(PlayerRef player)
    //{
    //    if (playerListEntries.TryGetValue(player, out LobbyUserBox entry))
    //    {
    //        Destroy(entry.gameObject);
    //        playerListEntries.Remove(player);
    //    }
    //}
    //public override void OnPlayerJoined(PlayerRef player)
    //{
    //    base.OnPlayerJoined(player);
    //    AddPlayerToList(player);
    //}
    //public override void OnPlayerLeft(PlayerRef player)
    //{
    //    base.OnPlayerLeft(player);
    //    RemovePlayerFromList(player);
    //}

    public void OnClickLeaveSession()
    { Shared.ui_Manager.RecivePopup(Instantiate(quitPopup, mainCanvas.transform)); }
    public void ForceStart()
    { Shared.ui_Manager.RecivePopup(Instantiate(forceStartPopup, mainCanvas.transform)); }
    public void OnClickReady()
    {
        if (Runner.IsServer)
            Shared.ui_Manager.RecivePopup(Instantiate(changeTrackPopup, mainCanvas.transform));
        else
            Debug.LogWarning("오직 호스트만 트랙을 변경할 수 있습니다.");
    }
    public void OnClickChangeCar()
    { Shared.ui_Manager.RecivePopup(Instantiate(changeCarPopup, mainCanvas.transform)); }
    //public void OnClickStart()
    //{
    //    if (!Runner.IsServer) return;
    //    bool allReady = true;
    //    foreach (PlayerRef playerRef in Runner.ActivePlayers)
    //    {
    //        NetworkObject playerObj = Runner.GetPlayerObject(playerRef);
    //        if (playerObj != null && playerObj.TryGetBehaviour<PlayerLobbyData>(out var playerData))
    //        {
    //            if (!playerData.IsReady)
    //            {
    //                allReady = false;
    //                break;
    //            }
    //        }
    //    }
    //    if (!allReady && showForceStartPopupIfNeeded)
    //    { // 강제 시작 팝업 로직 [cite: 9]
    //        Shared.ui_Manager.RecivePopup(Instantiate(forceStartPopup, mainCanvas.transform)); [cite: 9]
    //          return;
    //    }
    //    Runner.SetActiveScene(selectedTrack.sceneBuildIndex);
    //}
}
