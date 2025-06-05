using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class JoinRoom : MonoBehaviour
{
    [SerializeField] private TMP_InputField sessionIDInput;

    private void OnEnable()
    {
        sessionIDInput.text = Client_Data.LobbyID.ToString();
        SetLobbyID(sessionIDInput.text);
    }
    private void Start()
    {
        sessionIDInput.onValueChanged.AddListener(SetLobbyID);
        sessionIDInput.text = Client_Data.LobbyID.ToString();
    }

    private void SetLobbyID(string _lobbyID)
    {
        int lobbyNumber = int.Parse(_lobbyID);
        Client_Data.LobbyID = lobbyNumber;
    }
}
