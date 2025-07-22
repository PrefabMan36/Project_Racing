using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceStarter : MonoBehaviour
{
    private void Start()
    {
        if(!Shared.mainGameManagerSpawned)
        {
            Shared.lobby_Network_Manager.OnStartRace();
            Shared.mainGameManagerSpawned = true;
        }
        Destroy(gameObject);
    }
}
