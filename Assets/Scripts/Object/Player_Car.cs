using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

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
    private Curve_data _data;
    private CinemachineFreeLook freeLookCamera;
    private CinemachineFreeLook sideCamera;
    private Transform firstPersonCamera;
    private Transform lookBack;
    private Camera MainCamera;
    [SerializeField] private RadialBlur radialBlur;

    private bool firstPersonCameraCheck = false;
    private GameObject windowF, windowL, windowR;

    private bool freeLook;
    private float freeLookWaitTime;
    private float fov = 30f;

    public bool braking, sideBraking, up, down, left, right;

    private void Awake()
    {
        
    }
    private void Start()
    {
        _data = gameObject.transform.Find("Champion_curve").GetComponent<Curve_data>();
        horsePowerCurve = _data.horsePower;
        engineTorqueCurve = _data.torque;

        rpmGauge = FindAnyObjectByType<RPMGauge>();

        freeLookCamera = gameObject.transform.Find("FreeLookCamera").GetComponent<CinemachineFreeLook>();
        sideCamera = gameObject.transform.Find("ForceSideCamera").GetComponent<CinemachineFreeLook>();
        firstPersonCamera = gameObject.transform.Find("FirstPersonCamera");
        lookBack = gameObject.transform.Find("LookBackCamera");
        freeLookCamera.Follow = gameObject.transform;
        freeLookCamera.LookAt = gameObject.transform.Find("FocusPoint").transform;
        sideCamera.Follow = gameObject.transform;
        sideCamera.LookAt = gameObject.transform.Find("FocusPoint").transform;
        MainCamera = FindAnyObjectByType<Camera>();

        windowF = gameObject.transform.Find("WindowFront").gameObject;
        windowL = gameObject.transform.Find("WindowLeft").gameObject;
        windowR = gameObject.transform.Find("WindowRight").gameObject;

        radialBlur = MainCamera.GetComponent<RadialBlur>();
        //SetCenterMass();

        speedTextForUI = rpmGauge.transform.Find("Speed").GetComponent<Text>();
        gearTextForUI = rpmGauge.transform.Find("GearNum").GetComponent<Text>();
        freeLookCamera.m_XAxis.Value = 0f;
        freeLookWaitTime = 1.0f;
        MainCamera = FindAnyObjectByType<Camera>();
        body = gameObject;
        carRB = gameObject.GetComponent<Rigidbody>();
        freeLookCamera.enabled = true;
        ignition = true;
        braking = false;
        engineAcceleration = 0.5f;
        minEngineRPM = 800f;
        maxEngineRPM = 9400f;
        curEngineRPM = 1000;
        horsePower = 465;
        autoGear = true;
        antiRoll = 5000f;
        horsePower = 200;
        dragAmount = 0.015f;
        SetGearRatio(eGEAR.eGEAR_NEUTURAL, 0f);
        SetGearSpeedLimit(eGEAR.eGEAR_NEUTURAL, 0f);
        SetGearRatio(eGEAR.eGEAR_REVERSE, -3.44f);
        SetGearSpeedLimit(eGEAR.eGEAR_REVERSE, 99f);
        SetGearRatio(eGEAR.eGEAR_FIRST, 3.75f);
        SetGearSpeedLimit(eGEAR.eGEAR_FIRST, 82f);
        SetGearRatio(eGEAR.eGEAR_SECOND, 2.38f);
        SetGearSpeedLimit(eGEAR.eGEAR_SECOND, 130f);
        SetGearRatio(eGEAR.eGEAR_THIRD, 1.72f);
        SetGearSpeedLimit(eGEAR.eGEAR_THIRD, 179f);
        SetGearRatio(eGEAR.eGEAR_FOURTH, 1.34f);
        SetGearSpeedLimit(eGEAR.eGEAR_FOURTH, 230);
        SetGearRatio(eGEAR.eGEAR_FIFTH, 1.08f);
        SetGearSpeedLimit(eGEAR.eGEAR_FIFTH, 286);
        SetGearRatio(eGEAR.eGEAR_SIXTH, 0.88f);
        SetGearSpeedLimit(eGEAR.eGEAR_SIXTH, 351f);
        lastGear = eGEAR.eGEAR_SIXTH;
        finalDriveRatio = 3.97f;
        brakePower = 50000f;
        curGear = eGEAR.eGEAR_FIRST;
        nextGear = eGEAR.eGEAR_FIRST;
        shiftTimer = 0;
        shiftTiming = 0.5f;
        SetDriveAxel(eCAR_DRIVEAXEL.eRWD);
        SetFriction();
        StartCoroutine(Controlling());
    }

    private void Update()
    {

        //if (Input.GetKeyDown(KeyCode.V))
        //    changeCameraPosition();
        SetSpeed();
        SetUI();
        Steering(Input.GetAxis("Horizontal"));
        SetSlpingAngle();
        CameraUpdate();
        UpdatingWheels();
        if (Input.GetKeyDown(KeyCode.V))
            firstPerson();
        if (Input.GetKeyDown(KeyCode.LeftShift))
            ChangeGear(true);
        if (Input.GetKeyDown(KeyCode.LeftControl))
            ChangeGear(false);
        if (Input.GetKeyDown(KeyCode.Alpha1))
            SetGear(eGEAR.eGEAR_FIRST);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            SetGear(eGEAR.eGEAR_SECOND);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            SetGear(eGEAR.eGEAR_THIRD);
        if(Input.GetKeyDown(KeyCode.Alpha4))
            SetGear(eGEAR.eGEAR_FOURTH);
    }

    private void FixedUpdate()
    {
        UpdatingFriction();
        AntiRollBar();
    }

    IEnumerator Controlling()
    {
        WaitForSeconds wfs = new WaitForSeconds(0.02f);
        while (true)
        {
            yield return wfs;

            if(curGear != eGEAR.eGEAR_NEUTURAL)
                clutch = Input.GetAxis("Vertical") <= 0 ? 0 : Mathf.Lerp(clutch, 1, Time.deltaTime);
            if(Input.GetKey(KeyCode.C))
                clutch = 0f;
            Engine();
            if (ignition)
            {
                if (curGear != eGEAR.eGEAR_NEUTURAL)
                    throttle = Input.GetAxis("Vertical");
                if(throttle < 0)
                {
                    clutch = 0f;
                    brakeInput = Mathf.Abs(throttle);
                }
                else
                    brakeInput = 0f;
                //throttle = 1f;
                //if (slipingAngle < 120f)
                //{
                //    if (throttle < 0)
                //    {
                //        brakeInput = Mathf.Abs(throttle);
                //        down = true;
                //        clutch = 0f;
                //    }
                //    else
                //    {
                //        down = false;
                //        brakeInput = 0;
                //    }
                //}
                //else
                //    brakeInput = 0;
                Braking();
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
            SetRadialBlur();
        }
    }
    private void CameraUpdate()
    {
        if (Input.GetAxis("Vertical2") < 0)
        {
            freeLookCamera.enabled = false;
            sideCamera.enabled = false;
            MainCamera.fieldOfView = fov * 2f;
            MainCamera.transform.position = lookBack.position;
            MainCamera.transform.rotation = lookBack.rotation;
        }
        else if (firstPersonCameraCheck)
        {
            windowF.SetActive(true);
            windowL.SetActive(true);
            windowR.SetActive(true);
            freeLookCamera.enabled = false;
            sideCamera.enabled = false;
            MainCamera.transform.position = firstPersonCamera.position;
            MainCamera.transform.rotation = firstPersonCamera.rotation;
        }
        else if (Input.GetAxis("Horizontal2") + Input.GetAxis("Vertical2") == 0)
        {
            windowF.SetActive(false);
            windowL.SetActive(false);
            windowR.SetActive(false);
            freeLookCamera.enabled = true;
            sideCamera.enabled = false;
            up = false;
            down = false;
            left = false;
            right = false;
        }
        else if(Input.GetAxis("Horizontal2") != 0)
        {
            freeLookCamera.enabled = false;
            sideCamera.enabled = true;
            if (Input.GetAxis("Horizontal2") > 0)
                right = true;
            else
                left = true;
        }
            //freeLookCamera.m_Lens.FieldOfView = Mathf.Lerp(30f, 65f, GetSpeed()/200f);
        if(firstPersonCameraCheck)
            freeLookCamera.m_Lens.FieldOfView = fov * 2f;
        else
            freeLookCamera.m_Lens.FieldOfView = fov * 2f;
    }
    private void firstPerson() { firstPersonCameraCheck = !firstPersonCameraCheck; }
    private void FreeLookCheck()
    {
        if (Input.GetAxis("Mouse X") + Input.GetAxis("Mouse Y") != 0)
        {
            freeLookWaitTime = 1.0f;
            freeLook = true;
        }
        if(freeLook)
        {
            if (freeLookWaitTime > 0f)
                freeLookWaitTime -= Time.deltaTime;
            else
                freeLook = false;
        }
    }

    private void SetRadialBlur()
    {
        if (radialBlur != null)
        {
            radialBlur.blurStrength = Mathf.Lerp(0f, 2.2f, GetSpeed() / LastGearLimit());
            radialBlur.blurWidth = Mathf.Lerp(0f, 1f, GetSpeed() / LastGearLimit());
        }
    }
}
