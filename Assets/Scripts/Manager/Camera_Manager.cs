using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Manager : MonoBehaviour
{
    private enum eCAMERA_POSITION
    {
        eCAMERA_FRONT,
        eCAMERA_BACK,
        eCAMERA_LEFT,
        eCAMERA_RIGHT
    }
    private enum eCAMERA_HIGHT
    {
        eCAMERA_HIGH,
        eCAMERA_LOW
    }
    private enum eCAMERA_DISTANCE
    {
        eCAMERA_FAR,
        eCAMERA_NEAR
    }

    [SerializeField] private GameObject atachedObject;
    private bool following;
    private Rigidbody ObjectRB;
    private Player_Car controllerRef;
    private Camera mainCamera;
    private float speed;

    private float fieldOfViewFirstPerson = 60;
    private float fieldOfViewThirdPerson = 80;

    private GameObject focusPoint;
    [SerializeField] private Transform targetTransform;
    [SerializeField] private Vector3 cameraTargetPosition;
    private Vector3 tempPosition;
    private float tempPositionY, tempPositionZ;
    [SerializeField] private Transform cameraCurrentTransform;

    [SerializeField] private eCAMERA_POSITION curCamPosition;
    [SerializeField] private eCAMERA_HIGHT curCamHight;
    [SerializeField] private eCAMERA_DISTANCE curCamDistance;
    private int camPosIndex;
    private float highPosition,lowPosition;
    private float farPosition, nearPosition;

    private void Start()
    {
        following = true;
        mainCamera = gameObject.GetComponent<Camera>();
        cameraCurrentTransform = gameObject.transform;
        ObjectRB = atachedObject.GetComponent<Rigidbody>();
        focusPoint = atachedObject.transform.Find("FocusPoint").gameObject;
        targetTransform = focusPoint.transform;

        mainCamera.usePhysicalProperties = true;
        mainCamera.fieldOfView = fieldOfViewThirdPerson;

        SetHighCamPosition(2f);
        SetLowCamPosition(1f);
        SetFarCamPosition(-5f);
        SetNearCamPosition(-3f);
        speed = 1.5f;
        camPosIndex = 1;
        curCamPosition = eCAMERA_POSITION.eCAMERA_BACK;
        curCamHight = eCAMERA_HIGHT.eCAMERA_HIGH;
        curCamDistance = eCAMERA_DISTANCE.eCAMERA_FAR;
    }

    private void Update()
    {
        FollowCamUpdate();
    }

    private void FollowCamUpdate()
    {
        if (!following) return;
        SetCameraPosition(curCamPosition);
        transform.LookAt(targetTransform);
        cameraCurrentTransform.position = Vector3.Lerp(cameraCurrentTransform.position, cameraTargetPosition, Time.deltaTime * speed);
    }
    private void ChangeCamPosition()
    {
        camPosIndex++;
        if (camPosIndex > 4)
            camPosIndex = 1;
        switch(camPosIndex)
        {
            case 0:
                curCamHight = eCAMERA_HIGHT.eCAMERA_HIGH;
                curCamDistance = eCAMERA_DISTANCE.eCAMERA_FAR;
                break;
            case 1:
                curCamHight = eCAMERA_HIGHT.eCAMERA_HIGH;
                curCamDistance = eCAMERA_DISTANCE.eCAMERA_NEAR;
                break;
            case 3:
                curCamHight = eCAMERA_HIGHT.eCAMERA_LOW;
                curCamDistance = eCAMERA_DISTANCE.eCAMERA_FAR;
                break;
            case 4:
                curCamHight = eCAMERA_HIGHT.eCAMERA_LOW;
                curCamDistance = eCAMERA_DISTANCE.eCAMERA_NEAR;
                break;
        }
    }
    private void ChangeCamPosition(eCAMERA_HIGHT _hight, eCAMERA_DISTANCE _distance)
    {
        camPosIndex = 0;
        curCamHight = _hight;
        curCamDistance = _distance;
    }
    private void SetCameraPosition(eCAMERA_POSITION _POSITION)
    {
        if(curCamPosition == eCAMERA_POSITION.eCAMERA_BACK)
        {
            switch (curCamHight)
            {
                case eCAMERA_HIGHT.eCAMERA_HIGH:
                    tempPosition.y = highPosition;
                    break;
                case eCAMERA_HIGHT.eCAMERA_LOW:
                    tempPosition.y = lowPosition;
                    break;
            }
            switch (curCamDistance)
            {
                case eCAMERA_DISTANCE.eCAMERA_FAR:
                    tempPosition.z = farPosition;
                    break;
                case eCAMERA_DISTANCE.eCAMERA_NEAR:
                    tempPosition.z = nearPosition;
                    break;
            }
            cameraTargetPosition = targetTransform.position + (targetTransform.up * tempPosition.y) + (targetTransform.forward * tempPosition.z);
        }
    }

    public void SetFirstPersonFOV(float _fov) { fieldOfViewFirstPerson = _fov; }
    public void SetThirdPersonFOV(float _fov) { fieldOfViewThirdPerson = _fov; }
    public void SetHighCamPosition(float _positionY) { highPosition = _positionY; }
    public void SetLowCamPosition(float _positionY) {  lowPosition = _positionY; }
    public void SetNearCamPosition(float _positonZ) { nearPosition = _positonZ; }
    public void SetFarCamPosition(float _positonZ) { farPosition = _positonZ; }
    public void SetFollowing(bool _following) { following = _following; }
}
