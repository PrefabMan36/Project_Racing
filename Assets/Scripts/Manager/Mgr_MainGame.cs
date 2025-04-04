using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Mgr_MainGame : MonoBehaviour
{
    [SerializeField] private List<GameObject> Cars;

    [SerializeField] private GameObject player;
    [SerializeField] private GameObject playerCar;
    private void Start()
    {
        player = FindAnyObjectByType<Player_Car>().gameObject;

        string carFileName = Shared.CarName + ".prefab";

        //playerCar = Instantiate((GameObject)AssetDatabase.LoadAssetAtPath($"Assets/Prefabs/Colliders/" + carFileName, typeof(GameObject)));
        //playerCar.transform.position = player.transform.position;
        //playerCar.transform.rotation = player.transform.rotation;
        //playerCar.transform.parent = player.transform;
    }
}
