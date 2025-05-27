using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room_Manager : MonoBehaviour
{
    [SerializeField] Dictionary<int, TrackSelect> trackList = new Dictionary<int, TrackSelect>();
    [SerializeField] int[] mapEnum;
    [SerializeField] Sprite[] mapImages;
    [SerializeField] string sessionID;

    private void Awake()
    {
        Shared.room_Manager = this;
        DontDestroyOnLoad(this);

        InitailzeTrackList();
    }

    private void InitailzeTrackList()
    {
        trackList.Clear();
        if (mapEnum == null || mapImages == null)
        {
            Debug.LogError("mapEnum혹은 mapImages 배열이 Inspactor에서 할당되지 않았습니다.");
            return;
        }

        if (mapEnum.Length != mapImages.Length)
        {
            Debug.LogError("mapEnum 배열과 mapImages 배열의 길이가 일치하지 않습니다.");
            return;
        }

        TrackSelect tempTrack;
        for (int i = 0; i < mapEnum.Length; i++)
        {
            tempTrack = new TrackSelect();
            int enumValue = mapEnum[i];
            tempTrack.mapEnum = (eSCENE)enumValue;
            tempTrack.mapID = mapEnum[i];
            tempTrack.mapImage = mapImages[i];
            tempTrack.mapName = mapImages[i].name;
            trackList.Add(mapEnum[i], tempTrack);
        }
    }

    public int GetMapNum()
    {
        return trackList.Count;
    }

    public TrackSelect GetTrackSelect(int _trackNum)
    {
        return trackList[mapEnum[_trackNum]];
    }
    public eSCENE GetTrackEnum(int _trackNum)
    {
        return trackList[_trackNum].mapEnum;
    }

    public bool CheckTrack(int _trackNum)
    {
        if(trackList.ContainsKey(_trackNum))
            return true;
        return false;
    }

    public Sprite GetSprite(int _trackNum)
    {
        return trackList[_trackNum].mapImage;
    }
}
