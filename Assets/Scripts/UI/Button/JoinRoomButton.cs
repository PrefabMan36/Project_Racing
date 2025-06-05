using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JoinRoomButton : MonoBehaviour
{
    private void Start()
    {
        Button joinButton = GetComponent<Button>();
        if (joinButton != null)
        {
            joinButton.onClick.AddListener(Shared.lobby_Network_Manager.JoinOrCreateLobby);
            Debug.Log("Join Room Button listener added.");
        }
        else
        {
            Debug.LogError("Join Room Button component not found.");
        }
    }
}
