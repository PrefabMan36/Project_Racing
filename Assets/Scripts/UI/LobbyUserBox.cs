using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUserBox : NetworkBehaviour
{
    [SerializeField] Image playerIcon;
    [SerializeField] TextMeshProUGUI playerName;
    [SerializeField] GameObject readyCheck;

    private void Awake()
    {
        playerIcon.sprite = Shared.ui_Manager.GetLoadedIcon("White Person 2.png");
    }

    public void SetUserName(string userName)
    {
        if (!string.IsNullOrEmpty(userName))
            playerName.text = userName;
    }
}
