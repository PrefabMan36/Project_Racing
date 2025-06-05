using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceStarter : MonoBehaviour
{
    private void Awake()
    {
        Shared.lobby_Network_Manager.OnStartRace();
        Destroy(gameObject);
    }
}
