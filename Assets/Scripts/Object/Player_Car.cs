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

    private bool drifting = false;

    private bool freeLook;
    private float freeLookWaitTime;
    private float fov = 30f;

    public bool braking, sideBraking, up, down, left, right;
    private void Start()
    {
        _data = gameObject.transform.Find("Champion_curve").GetComponent<Curve_data>();
        SetCurves(_data.horsePower, _data.torque);
        steeringCurve = _data.steer;
        Destroy(_data.gameObject);

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

        //if(gameObject.transform.Find("WindowFront").gameObject != null)
        //    windowF = gameObject.transform.Find("WindowFront").gameObject;
        //if(gameObject.transform.Find("WindowLeft").gameObject != null)
        //    windowL = gameObject.transform.Find("WindowLeft").gameObject;
        //if(gameObject.transform.Find("WindowRight").gameObject != null)
        //    windowR = gameObject.transform.Find("WindowRight").gameObject;

        radialBlur = MainCamera.GetComponent<RadialBlur>();
        //SetCenterMass();

        SetNitroInstall(true);
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
        engineSound.Play();
        SetEngineAcceleration(0.5f);
        SetEngineRPMLimit(9400f, 800f);
        SetAutoGear(true);
        antiRoll = 5000f;
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
        SetLastGear(eGEAR.eGEAR_SIXTH);
        SetFinalDriveRatio(3.97f);
        SetShiftTiming(0.5f);
        brakePower = 50000f;
        SetDriveAxel(eCAR_DRIVEAXEL.eRWD);
        SetFriction();
        SpawnSmoke();
        StartCoroutine(Controlling());
        StartCoroutine(Engine());
        StartCoroutine(UpdateNitro());
        StartCoroutine(UIUpdating());
    }

    private void Update()
    {

        //if (Input.GetKeyDown(KeyCode.V))
        //    changeCameraPosition();
        //Engine();
        SetSpeed();
        Steering(Input.GetAxis("Horizontal"));
        SetSlpingAngle();
        CameraUpdate();
        if (Input.GetKeyDown(KeyCode.RightShift))
        { ActivateNitro(true); }
        if(Input.GetKeyUp(KeyCode.RightShift) && !GetPowerMode())
        { ActivateNitro(false); }
        if (Input.GetKeyDown(KeyCode.G))
        {
            drifting = !drifting;
            ChangeFriction(drifting);
        }
        if(Input.GetKeyDown(KeyCode.F)) { HeadLightSwitch(); }
        UpdatingWheels();
        if (Input.GetKeyDown(KeyCode.V))
            firstPerson();
        if (Input.GetKeyDown(KeyCode.LeftShift))
            ChangeGear(true);
        if (Input.GetKeyDown(KeyCode.LeftControl))
            ChangeGear(false);
        if (Input.GetKeyDown(KeyCode.Alpha1))
            ForceChangeGear(eGEAR.eGEAR_FIRST);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            ForceChangeGear(eGEAR.eGEAR_SECOND);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            ForceChangeGear(eGEAR.eGEAR_THIRD);
        if(Input.GetKeyDown(KeyCode.Alpha4))
            ForceChangeGear(eGEAR.eGEAR_FOURTH);
    }

    private void FixedUpdate()
    {
        ApplyAerodynamicDrag();
        //UpdatingFriction();
        AntiRollBar();
    }

    IEnumerator Controlling()
    {
        WaitForSeconds wfs = new WaitForSeconds(0.02f);
        while (true)
        {
            yield return wfs;

            if(GetCurrentGear() != eGEAR.eGEAR_NEUTURAL)
                clutch = Input.GetAxis("Vertical") == 0 ? 0 : Mathf.Lerp(clutch, 1, Time.deltaTime);
            if(Input.GetKey(KeyCode.C))
                clutch = 0f;
            if (ignition)
            {
                if (GetCurrentGear() != eGEAR.eGEAR_NEUTURAL)
                    throttle = Input.GetAxis("Vertical");
                if (throttle > 0)
                {
                    if (GetCurrentGear() == eGEAR.eGEAR_REVERSE)
                    {
                        clutch *= throttle;
                        brakeInput = Mathf.Abs(throttle);
                    }
                    else
                        brakeInput = 0f;
                }
                else if (throttle < 0)
                {
                    if (GetCurrentGear() != eGEAR.eGEAR_REVERSE)
                    {
                        clutch *= throttle;
                        brakeInput = Mathf.Abs(throttle);
                    }
                    else
                        brakeInput = 0f;
                }
                else
                    brakeInput = 0f;
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
            EffectDrift();
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
            if (windowF != null)
            {
                windowF.SetActive(true);
                windowL.SetActive(true);
                windowR.SetActive(true);
            }
            freeLookCamera.enabled = false;
            sideCamera.enabled = false;
            MainCamera.transform.position = firstPersonCamera.position;
            MainCamera.transform.rotation = firstPersonCamera.rotation;
        }
        else if (Input.GetAxis("Horizontal2") + Input.GetAxis("Vertical2") == 0)
        {
            if(windowF != null)
            {
                windowL.SetActive(false);
                windowF.SetActive(false);
                windowR.SetActive(false);
            }
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
        if (firstPersonCameraCheck)
            freeLookCamera.m_Lens.FieldOfView = fov * 2f;
        else
            freeLookCamera.m_Lens.FieldOfView = 
                GetIsNitroActive() ?
                Mathf.Lerp(freeLookCamera.m_Lens.FieldOfView, fov * 2.5f, Time.deltaTime) : 
                Mathf.Lerp(freeLookCamera.m_Lens.FieldOfView, fov * 2f, Time.deltaTime);
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
            radialBlur.blurStrength = Mathf.Lerp(0f, 2.2f, GetSpeed() / 200f);
            radialBlur.blurWidth = Mathf.Lerp(0f, 1f, GetSpeed() / 200f) + GetNitroBlurWidth();
        }
    }
}
