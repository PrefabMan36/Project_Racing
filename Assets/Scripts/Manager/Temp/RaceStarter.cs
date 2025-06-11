using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceStarter : MonoBehaviour
{
    private void Start()
    {
        Shared.lobby_Network_Manager.OnStartRace();
        Destroy(gameObject);
    }
}
