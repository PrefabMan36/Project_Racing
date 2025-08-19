using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Navigation_Camera : MonoBehaviour
{
    [SerializeField] private GameObject followCamera;
    [SerializeField] private Vector3 baseFollowCameraPosition;

    [SerializeField] private GameObject topDownCamera;
    [SerializeField] private Vector3 baseTopDownCameraPosition;

    [SerializeField] private Player_Car car;
    [SerializeField] private Transform basePosition;
    [SerializeField] private Vector3 rotation = new Vector3(0, 0, 0);
    [SerializeField] private bool isRotate = false;
    [SerializeField] private bool rotateState = false;
    [SerializeField] private bool isTopDown = false;
    [SerializeField] private float topDownHeight = 200f;

    private void Start()
    {
        car = transform.GetComponentInParent<Player_Car>();
        baseFollowCameraPosition = followCamera.transform.localPosition;
        baseTopDownCameraPosition = topDownCamera.transform.localPosition;

        basePosition = car.transform;
        transform.SetParent(null);
        transform.Rotate(Vector3.zero,Space.World);
        StartCoroutine(Navigation());
    }

    private void IsRotateCheck()
    {
        if(rotateState != isRotate)
        {
            rotateState = isRotate;
            if(isRotate)
            {
                topDownCamera.transform.localPosition = new Vector3(baseTopDownCameraPosition.x, topDownHeight, 100f);
            }
            else
            {
                transform.eulerAngles = Vector3.zero;
                topDownCamera.transform.localPosition = baseTopDownCameraPosition;
            }
        }
    }

    private IEnumerator Navigation()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(Shared.frame15);
        while (true)
        {
            yield return waitForSeconds;
            IsRotateCheck();
            if (car != null)
            {
                transform.position = basePosition.position;
                if(isRotate)
                {
                    rotation.y = basePosition.eulerAngles.y;
                    transform.eulerAngles = rotation;
                }
            }
            if(isTopDown)
            {
                topDownCamera.SetActive(true);
                followCamera.SetActive(false);
            }
            else
            {
                topDownCamera.SetActive(false);
                followCamera.SetActive(true);
                rotation.y = basePosition.eulerAngles.y;
                transform.eulerAngles = rotation;
            }
        }
    }
}
