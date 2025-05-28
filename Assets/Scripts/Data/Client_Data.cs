using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Client_Data
{
    public static string Username
    {
        get => PlayerPrefs.GetString("Client_Username", string.Empty);
        set => PlayerPrefs.SetString("Client_Username", value);
    }

    public static int CarID
    {
        get => PlayerPrefs.GetInt("Client_CarID", 0);
        set => PlayerPrefs.SetInt("Client_CarID", value);
    }

    public static int LobbyID
    {
        get => PlayerPrefs.GetInt("Client_LastLobbyID", 0);
        set => PlayerPrefs.SetInt("Client_LastLobbyID", value);
    }
}
