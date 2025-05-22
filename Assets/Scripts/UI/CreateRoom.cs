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

    [SerializeField]


    private void Awake()
    {
        CreateButtons();
        SelectMap(currentMapIndex);
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
            Debug.LogError("해당하는 맵이 존재 하지 않습니다");
            return;
        }
        currentMapIndex = _num;
        currentMapImage.sprite = Shared.room_Manager.GetSprite(currentMapIndex);
    }

    public void OnClickCreate()
    {
        
    }
}
