using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CineMachine_Car_Manager : MonoBehaviour
{
    private CinemachineFreeLook carCamera;
    private CinemachineVirtualCamera virCamera;

    private void Awake()
    {
        carCamera = gameObject.GetComponent<CinemachineFreeLook>();
        carCamera.Follow = GameObject.FindGameObjectWithTag("Player").transform;
        carCamera.LookAt = carCamera.Follow.transform.Find("FocusPoint").transform;
    }
}
