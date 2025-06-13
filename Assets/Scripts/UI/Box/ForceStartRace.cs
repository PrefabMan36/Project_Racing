using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ForceStartRace : MonoBehaviour
{
    [SerializeField] Button yesButton;

    private void Start()
    {
        yesButton.onClick.AddListener(Shared.lobby_Manager.ForceStart);
    }
}
