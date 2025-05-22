using System.Collections;
using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyMenu : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lobbyName;
    [SerializeField] private Image mapImage;
    [SerializeField] private int currentMapIndex;
    [SerializeField] private Transform scrollContent;

    [SerializeField] private LobbyUserBox lobbyUser_Prefab;

    [SerializeField] private GameObject quitPopup;
    [SerializeField] private GameObject forceStartPopup;
    [SerializeField] private GameObject changeTrackPopup;
    [SerializeField] private GameObject changeCarPopup;

    [SerializeField] private GameObject StartButton;

    public void SetLobby(string _lobbyName, int _mapIndex)
    {
        lobbyName.text = _lobbyName;
        currentMapIndex = _mapIndex;
        mapImage.sprite = Shared.room_Manager.GetSprite(currentMapIndex);
        StartButton.SetActive(false);
    }

    public void OnClickLeaveSession()
    {

    }
    public void OnClickStart()
    {

    }
    public void ForceStart()
    {
        
    }
    public void OnClickReady()
    {

    }
    public void OnClickChangeCar()
    {

    }
}
