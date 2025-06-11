using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ForceStartRace : MonoBehaviour
{
    [SerializeField] Button yesButton;

    private void Start()
    {
        yesButton.onClick.AddListener(ForceStart);
    }
    public void ForceStart()
    {
        Shared.ui_Manager.isInGame = true;
        Shared.ui_Manager.OnClickClose();
        Shared.game_Manager.gameStart = true;
    }
}
