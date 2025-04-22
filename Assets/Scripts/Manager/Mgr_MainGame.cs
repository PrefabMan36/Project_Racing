using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Mgr_MainGame : MonoBehaviour
{
    [SerializeField] private Player_Car playerCar;
    [SerializeField] private Car_data carData;
    private void Awake()
    {
        playerCar = GameObject.FindWithTag("Player").GetComponent<Player_Car>();
        carData = (Car_data)CarData_Manager.instance.GetCarDataByName("Super2000");
        //playerCar.set
    }
}
