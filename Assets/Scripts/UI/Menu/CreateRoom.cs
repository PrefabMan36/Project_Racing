using System.Collections.Generic;
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
        SelectMap(currentMapIndex);
        sessionInput.text = Server_Data.LobbyName = "session" + Random.Range(0,1000);
        Server_Data.trackIndex = currentMapIndex;
        Server_Data.UserCapacity = 4;
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

    public void SelectMap(int _num)
    {
        if (!Shared.room_Manager.CheckTrack(_num))
        {
            Debug.LogError($"해당하는 맵이 존재 하지 않습니다 {_num}");
            return;
        }
        Server_Data.LobbyID = Random.Range(10000000, 99999999);
        currentMapIndex = _num;
        currentMapImage.sprite = Shared.room_Manager.GetSprite(currentMapIndex);
        Server_Data.serverTrack = Shared.room_Manager.GetTrackByIndex(currentMapIndex);
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
            Shared.lobby_Network_Manager.JoinOrCreateLobby();
            _lobbyIsValid = false;
        }
    }
}
