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
    { Shared.scene_Manager.ChangeScene(Shared.room_Manager.GetTrackEnum(Shared.game_Manager.trackIndex)); }
}
