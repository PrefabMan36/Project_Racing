using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateRoom : MonoBehaviour
{
    [SerializeField] Image currentMapImage;
    [SerializeField] TMP_InputField sessionInput;
    [SerializeField] int currentMapIndex = 5;

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
        for(int i = 0; i < mapNum; i++)
        {
            tempTrack = Shared.room_Manager.GetTrackSelect(i);
            tempButton = Instantiate(mapButtonPrefab, scrollContent.transform);
            tempButton.SetMapButton(this, tempTrack.mapID , tempTrack.mapName);
        }
    }

    public void SelectMap(int _num)
    {
        if (!Shared.room_Manager.CheckTrack(_num))
        {
            Debug.LogError("�ش��ϴ� ���� ���� ���� �ʽ��ϴ�");
            return;
        }
        currentMapIndex = _num;
        currentMapImage.sprite = Shared.room_Manager.GetSprite(currentMapIndex);
    }

    private bool _lobbyIsValid;
    public void ValidateLobby()
    { _lobbyIsValid = string.IsNullOrEmpty(Server_Data.LobbyName) == false; }

    public void OnClickCreate(Lobby_Network_Manager lobby_Network)
    {
        if (_lobbyIsValid)
        {
            Server_Data.LobbyID = Random.Range(0, 100000);
            Server_Data.serverTrack = Shared.room_Manager.GetTrackSelect(currentMapIndex);
            //lobby_Network.CreateLobby()
            _lobbyIsValid = false;
        }
    }
}
