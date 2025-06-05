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
        Shared.scene_Manager.ChangeScene(Shared.room_Manager.GetTrackEnum(Shared.game_Manager.trackIndex));
        Shared.lobby_Network_Manager.OnStartRace();
    }
}
