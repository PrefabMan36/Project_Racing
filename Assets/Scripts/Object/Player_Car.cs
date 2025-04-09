using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//void setUpController()
//{
//    carController c = car.GetComponent<carController>();
//    c.gears = new float[7];

//    c.maxRPM = 8000;
//    c.minRPM = 4000;

//    c.DownForceValue = 5;
//    c.dragAmount = 0.015f;
//    c.EngineSmoothTime = 0.5f;
//    c.finalDrive = 3.71f;

//    if (car.tag != "Player") car.tag = "Player";

//}
public class Player_Car : Car
{
    [SerializeField] private Camera MainCamera;
    [SerializeField] private Transform[] personCamPosition;
    public bool braking, sideBraking, up, down, left, right;
    private int curCamPosition;
    private void Start()
    {
        body = gameObject;
        carRB = gameObject.GetComponent<Rigidbody>();
        ignition = true;
        braking = false;
        engineAcceleration = 0.5f;
        minEngineRPM = 800f;
        maxEngineRPM = 9400f;
        curEngineRPM = 1000;
        horsePower = 465;
        autoGear = false;
        //horsePower = 200;
        dragAmount = 0.015f;
        SetGearRatio(eGEAR.eGEAR_NEUTURAL, 0f);
        SetGearSpeedLimit(eGEAR.eGEAR_NEUTURAL, 0f);
        SetGearRatio(eGEAR.eGEAR_REVERSE, -3.154f);
        SetGearSpeedLimit(eGEAR.eGEAR_REVERSE, -38f);
        SetGearRatio(eGEAR.eGEAR_FIRST, 3.154f);
        SetGearSpeedLimit(eGEAR.eGEAR_FIRST, 76f);
        SetGearRatio(eGEAR.eGEAR_SECOND, 2.294f);
        SetGearSpeedLimit(eGEAR.eGEAR_SECOND, 114f);
        SetGearRatio(eGEAR.eGEAR_THIRD, 1.85f);
        SetGearSpeedLimit(eGEAR.eGEAR_THIRD, 148f);
        SetGearRatio(eGEAR.eGEAR_FOURTH, 1.522f);
        SetGearSpeedLimit(eGEAR.eGEAR_FOURTH, 169);
        SetGearRatio(eGEAR.eGEAR_FIFTH, 1.273f);
        SetGearSpeedLimit(eGEAR.eGEAR_FIFTH, 191);
        SetGearRatio(eGEAR.eGEAR_SIXTH, 1.097f);
        SetGearSpeedLimit(eGEAR.eGEAR_SIXTH, 203f);
        lastGear = eGEAR.eGEAR_SIXTH;
        finalDriveRatio = 4.188f;
        brakePower = 50000f;
        curGear = eGEAR.eGEAR_FIRST;
        nextGear = eGEAR.eGEAR_FIRST;
        shiftTimer = 0;
        shiftTiming = 0.5f;
        curCamPosition = 0;
        SetDriveAxel(eCAR_DRIVEAXEL.eRWD);

        StartCoroutine(Controlling());
    }

    private void Update()
    {

        //if (Input.GetKeyDown(KeyCode.V))
        //    changeCameraPosition();
        SetSpeed();
        GearShift();
        SetCenterMass();
        //ShowCenterMass();
        SetSlpingAngle();
        UpdatingWheels();
        if (Input.GetKeyDown(KeyCode.LeftShift))
            ChangeGear(true);
        if (Input.GetKeyDown(KeyCode.LeftControl))
            ChangeGear(false);
    }

    IEnumerator Controlling()
    {
        WaitForSeconds wfs = new WaitForSeconds(0.02f);
        while (true)
        {
            yield return wfs;

            clutch = Input.GetAxis("Vertical") <= 0 ? 0 : Mathf.Lerp(clutch, 1, Time.deltaTime);
            Engine();
            if (ignition)
            {
                throttle = Input.GetAxis("Vertical");
                //throttle = 1f;
                if (slipingAngle < 120f)
                {
                    if (throttle < 0)
                    {
                        brakeInput = Mathf.Abs(throttle);
                        down = true;
                        clutch = 0f;
                    }
                    else
                    {
                        down = false;
                        brakeInput = 0;
                    }
                }
                else
                    brakeInput = 0;
                Braking();
            }
            Steering(Input.GetAxis("Horizontal"));
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
        }
    }
    private void changeCameraPosition()
    {
        curCamPosition++;
        if(curCamPosition >= personCamPosition.Length)
            curCamPosition = 0;
        MainCamera.transform.position = personCamPosition[curCamPosition].position;
        MainCamera.transform.rotation = personCamPosition[curCamPosition].rotation;
        //switch (curCamPosition)
        //{
        //    case 0:
        //        MainCamera.transform.position = personCamPosition[0].position;
        //        MainCamera.transform.rotation = personCamPosition[0].rotation;
        //        break;
        //    case 1:
        //        MainCamera.transform.position = personCamPosition[1].position;
        //        MainCamera.transform.rotation = personCamPosition[1].rotation;
        //        break;
        //}
    }
}
