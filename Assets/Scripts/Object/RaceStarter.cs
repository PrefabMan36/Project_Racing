using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceStarter : MonoBehaviour
{
    [SerializeField] GameObject PathManager;
    private void Start()
    {
        Instantiate(PathManager, Vector3.zero, Quaternion.identity);
        if (!Shared.mainGameManagerSpawned)
        {
            Shared.lobby_Network_Manager.OnStartRace();
            Shared.mainGameManagerSpawned = true;
        }
        Destroy(gameObject);
    }
}
