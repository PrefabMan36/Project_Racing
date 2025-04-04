using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Car : Car
{
    [SerializeField] private Camera MainCamera;
    [SerializeField] private Transform[] personCamPosition;
    public bool braking;
    public bool sideBraking;
    public bool up;
    public bool down;
    public bool left;
    public bool right;
    private int curCamPosition;
    private void Start()
    {
        carRB = gameObject.GetComponent<Rigidbody>();
        braking = false;
        curEngineRPM = 1000;
        maxEngineRPM = 9000;
        maxEngineTorque = 465;
        SetGearRatio(eGEAR.eGEAR_NEUTURAL, 0f);
        SetGearRatio(eGEAR.eGEAR_REVERSE, -1.28f);
        SetGearRatio(eGEAR.eGEAR_FIRST, 3.154f);
        SetGearRatio(eGEAR.eGEAR_SECOND, 2.294f);
        SetGearRatio(eGEAR.eGEAR_THIRD, 1.85f);
        SetGearRatio(eGEAR.eGEAR_FOURTH, 1.522f);
        SetGearRatio(eGEAR.eGEAR_FIFTH, 1.273f);
        SetGearRatio(eGEAR.eGEAR_SIXTH, 1.097f);
        curGear = eGEAR.eGEAR_FIRST;
        nextGear = eGEAR.eGEAR_FIRST;
        shiftTimer = 0;
        shiftTiming = 1.0f;
        curCamPosition = 0;
        SetDriveAxel(eCAR_DRIVEAXEL.eRWD);

        StartCoroutine(Moving());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
            changeCameraPosition();
    }

    IEnumerator Moving()
    {
        WaitForSeconds wfs = new WaitForSeconds(0.001f);
        while (true)
        {
            yield return wfs;
            InputThrottle();
            GearShift();
            switch(Input.GetAxisRaw("Vertical"))
            {
                case 0:
                    MoveStop();
                    BrakingUp();
                    up = false;
                    down = false;
                    break;
                case 1:
                    MoveForward();
                    up = true;
                    break;
                case -1:
                    MoveBackward();
                    down = true;
                    break;
            }
            switch(Input.GetAxisRaw("Horizontal"))
            {
                case 0:
                    SteerNone();
                    right = false;
                    left = false;
                    break;
                case 1:
                    SteerRight();
                    right = true;
                    break;
                case -1:
                    SteerLeft();
                    left = true;
                    break;
            }
            if (Input.GetKey(KeyCode.Space))
            {
                SideBrakingDown();
                sideBraking = true;
            }
            else
            {
                SideBrakingUp();
                sideBraking = false;
            }
            if (Input.GetKeyDown(KeyCode.LeftShift))
                ChangeGear(true);
            if (Input.GetKeyDown(KeyCode.LeftControl))
                ChangeGear(false);
        }
    }
    private void changeCameraPosition()
    {
        curCamPosition++;
        if(curCamPosition >= personCamPosition.Length)
            curCamPosition = 0;
        switch(curCamPosition)
        {
            case 0:
                MainCamera.transform.position = personCamPosition[0].position;
                MainCamera.transform.rotation = personCamPosition[0].rotation;
                break;
            case 1:
                MainCamera.transform.position = personCamPosition[1].position;
                MainCamera.transform.rotation = personCamPosition[1].rotation;
                break;
        }
    }
}
