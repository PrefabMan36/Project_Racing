using System;
using UnityEngine;
using UnityEngine.UI;

public class SelectMapLobby : MonoBehaviour
{
    [SerializeField] Image currentMapImage;
    [SerializeField] int currentMapIndex;

    [SerializeField] GameObject scrollContent;
    [SerializeField] LobbyMapButton mapButtonPrefab;
    [SerializeField] Button confirmButton;

    private void Awake()
    {
        CreateButtons();

        currentMapIndex = Server_Data.trackIndex;
        currentMapImage.sprite = Shared.room_Manager.GetSprite(currentMapIndex);

        confirmButton.onClick.AddListener(ConfirmTrackChange);
        confirmButton.onClick.AddListener(Shared.ui_Manager.OnClickNo);
    }

    private void CreateButtons()
    {
        LobbyMapButton tempButton;
        TrackSelect tempTrack;
        int mapNum = Shared.room_Manager.GetMapNum();
        Debug.Log(mapNum);
        for (int i = 0; i < mapNum; i++)
        {
            tempTrack = Shared.room_Manager.GetTrackByNum(i);
            tempButton = Instantiate(mapButtonPrefab, scrollContent.transform);
            tempButton.SetMapButton(this, tempTrack.mapID, tempTrack.mapName);
        }
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
    }

    public void ConfirmTrackChange()
    {
        Server_Data.serverTrack = Shared.room_Manager.GetTrackByIndex(currentMapIndex);
        Server_Data.trackIndex = currentMapIndex;
        Shared.game_Manager.trackIndex = currentMapIndex;
    }
}
