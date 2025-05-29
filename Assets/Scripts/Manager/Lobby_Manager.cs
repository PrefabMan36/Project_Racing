using System.Collections.Generic;
using System.Linq;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Lobby_Manager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI lobbyNumberText;
    [SerializeField] private Canvas mainCanvas;

    [Header("Map Info")]
    [SerializeField] private Image trackImage;
    [SerializeField] private TextMeshProUGUI trackName;
    private int currentMapIndex;
    private TrackSelect selectedTrack;

    [Header("Player List")]
    [SerializeField] private Transform scrollContent;
    [SerializeField] private GameObject lobbyUser_Prefab;

    [Header("Popups")]
    [SerializeField] private GameObject quitPopup;
    [SerializeField] private GameObject forceStartPopup;
    [SerializeField] private GameObject changeTrackPopup;
    [SerializeField] private GameObject changeCarPopup;

    [Header("Buttons")]
    [SerializeField] private Button StartButton;
    [SerializeField] private Button readyButton;
    [SerializeField] private Button changeTrackButton;
    [SerializeField] private Button quitButton;

    [SerializeField] private static readonly Dictionary<LobbyPlayer, LobbyItem> playerList = new Dictionary<LobbyPlayer, LobbyItem>();
    [SerializeField] private static bool isSubscrribed;

    private void Awake()
    {
        Game_Manager.OnLobbyUpdated += OnLobbyUpdate;

        LobbyPlayer.PlayerChanged += (player) =>
        {
            var isHost = LobbyPlayer.localPlayer.isHost;
            StartButton.gameObject.SetActive(isHost);
            changeTrackButton.gameObject.SetActive(isHost);
            changeTrackButton.onClick.AddListener(OnClickChangeTrack);
        };
    }

    private void Start()
    {
        quitButton.onClick.AddListener(Shared.lobby_Network_Manager.QuitSession);
    }

    private void OnLobbyUpdate(Game_Manager manager)
    {
        lobbyNameText.text = "방 이름 : " + manager.lobbyName.Value;
        lobbyNumberText.text = "방 번호 : " + manager.lobbyID.ToString();
        SetMapInfoUI(manager.trackIndex);
    }

    public void SetLobby(string _lobbyName, int _lobbyID , int _mapIndex)
    {
        if (isSubscrribed) return;
        lobbyNameText.text = _lobbyName;
        lobbyNumberText.text = _lobbyID.ToString();
        mainCanvas = Shared.ui_Manager.GetMainCanvas();
        SetMapInfoUI(_mapIndex);
        
        LobbyPlayer.playerJoined += OnPlayerJoined;
        LobbyPlayer.playerLeft += OnPlayerLeft;
        LobbyPlayer.PlayerChanged += OnPlayerReadyChanged;

        readyButton.onClick.AddListener(OnClickReady);

        isSubscrribed = true;
    }

    private void OnDestroy()
    {
        if(!isSubscrribed) return;

        LobbyPlayer.playerJoined -= OnPlayerJoined;
        LobbyPlayer.playerLeft -= OnPlayerLeft;

        readyButton.onClick.RemoveListener(OnClickReady);
        isSubscrribed = false;
    }

    private void OnPlayerJoined(LobbyPlayer player)
    {
        if(playerList.ContainsKey(player))
        {
            var removePlayer = playerList[player];
            Destroy(removePlayer.gameObject);
            playerList.Remove(player);
        }

        var playerInLobby = Instantiate(lobbyUser_Prefab, scrollContent).GetComponent<LobbyItem>();
        playerInLobby.SetPlayer(player);
        playerList.Add(player, playerInLobby);
        OnLobbyUpdate(Shared.game_Manager);
    }

    private void OnPlayerLeft(LobbyPlayer player)
    {
        if (!playerList.ContainsKey(player)) return;
        
        var playerInLobby = playerList[player];
        if(playerInLobby != null)
        {
            Destroy(playerInLobby.gameObject);
            playerList.Remove(player);
        }
    }

    private void OnClickReady()
    {
        var localPlayer = LobbyPlayer.localPlayer;
        if(localPlayer && localPlayer.Object && localPlayer.Object.IsValid)
            localPlayer.RPC_ChangeReadyState(!localPlayer.isReady);
    }

    private void OnPlayerReadyChanged(LobbyPlayer lobbyPlayer)
    {
        if (!LobbyPlayer.localPlayer.isHost) return;

        if(IsAllReady())
        {
            Shared.scene_Manager.ChangeScene(Shared.room_Manager.GetTrackEnum(Shared.game_Manager.trackIndex));
        }
    }

    private static bool IsAllReady()
    {
        if (LobbyPlayer.players.Count > 0 && LobbyPlayer.players.All(player => player.isReady))
            return true;
        else
            return false;
    }

    private void SetMapInfoUI(int _mapIndex)
    {
        currentMapIndex = _mapIndex;
        selectedTrack = Shared.room_Manager.GetTrackByIndex(currentMapIndex);
        trackImage.sprite = selectedTrack.mapImage;
        trackName.text = selectedTrack.mapName;
    }
    public void OnClickLeaveSession()
    { Shared.ui_Manager.RecivePopup(Instantiate(quitPopup, mainCanvas.transform)); }
    public void ForceStart()
    { Shared.ui_Manager.RecivePopup(Instantiate(forceStartPopup, mainCanvas.transform)); }
    public void OnClickChangeCar()
    { Shared.ui_Manager.RecivePopup(Instantiate(changeCarPopup, mainCanvas.transform)); }
    public void OnClickChangeTrack()
    { Shared.ui_Manager.RecivePopup(Instantiate(changeTrackPopup, mainCanvas.transform)); }
    public void OnClickStart()
    {
    }
}
