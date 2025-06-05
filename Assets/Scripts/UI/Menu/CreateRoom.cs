using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateRoom : MonoBehaviour
{
    [SerializeField] Image currentMapImage;
    [SerializeField] TMP_InputField sessionInput;
    [SerializeField] int currentMapIndex = 6;

    [SerializeField] GameObject scrollContent;
    [SerializeField] MapButton mapButtonPrefab;

    private void Awake()
    {
        CreateButtons();
        Server_Data.trackIndex = currentMapIndex;
        Server_Data.UserCapacity = 4;
        RandomizeSession();
        SelectMap(currentMapIndex);
    }

    private void CreateButtons()
    {
        MapButton tempButton;
        TrackSelect tempTrack;
        int mapNum = Shared.room_Manager.GetMapNum();
        Debug.Log(mapNum);
        for(int i = 0; i < mapNum; i++)
        {
            tempTrack = Shared.room_Manager.GetTrackByNum(i);
            tempButton = Instantiate(mapButtonPrefab, scrollContent.transform);
            tempButton.SetMapButton(this, tempTrack.mapID , tempTrack.mapName);
        }
    }

    private void OnEnable()
    {
        RandomizeSession();
    }
    private void OnDisable()
    {
        sessionInput.text = string.Empty;
    }

    public void SelectMap(int _num)
    {
        if (!Shared.room_Manager.CheckTrack(_num))
        {
            Debug.LogError($"해당하는 맵이 존재 하지 않습니다 {_num}");
            return;
        }
        currentMapIndex = _num;
        currentMapImage.sprite = Shared.room_Manager.GetSprite(currentMapIndex);
        Server_Data.serverTrack = Shared.room_Manager.GetTrackByIndex(currentMapIndex);
        Server_Data.trackIndex = _num;
    }

    private bool _lobbyIsValid;
    public void ValidateLobby()
    {
        _lobbyIsValid = string.IsNullOrEmpty(Server_Data.LobbyName) == false;
        Debug.Log(_lobbyIsValid);
    }

    public void OnClickCreate()
    {
        if (_lobbyIsValid)
        {
            Server_Data.LobbyName = sessionInput.text;
            Shared.lobby_Network_Manager.JoinOrCreateLobby();
            _lobbyIsValid = false;
        }
    }
    private void RandomizeSession()
    {
        if(sessionInput.text == string.Empty)
            sessionInput.text = Server_Data.LobbyName = "session" + Random.Range(0, 1000);
        Server_Data.LobbyID = Random.Range(10000000, 99999999);
    }
}
