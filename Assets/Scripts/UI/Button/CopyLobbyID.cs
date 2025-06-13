using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyLobbyID : MonoBehaviour
{
    public void OnClickCopyID()
    {
        GUIUtility.systemCopyBuffer = Server_Data.LobbyID.ToString();
    }
}
