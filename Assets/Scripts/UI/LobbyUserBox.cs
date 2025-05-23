using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUserBox : NetworkBehaviour
{
    [SerializeField] Image playerIcon;
    [SerializeField] TextMeshProUGUI playerName;
    [SerializeField] GameObject readyCheck;
    [SerializeField, Networked] bool ready { get; set; }

    public override void Spawned()
    {
        base.Spawned();
        playerIcon.sprite = Shared.ui_Manager.GetLoadedIcon("White Person 2.png");
        ready = false;
    }

    public void SetUserName(string userName)
    {
        if (!string.IsNullOrEmpty(userName))
            playerName.text = userName;
    }

    public void SetReady()
    {
        ready = !ready;
    }

    public void SetForceUnReady()
    {
        ready = false;
    }

    public override void FixedUpdateNetwork()
    {
        readyCheck.SetActive(ready);
    }
}
