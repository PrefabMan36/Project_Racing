using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Tiny;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Fusion;

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
        _data = FindAnyObjectByType<Curve_data>();
        SetEngineCurves(_data.horsePower, _data.torque);
        SetSteeringCurve(_data.steer);
        Destroy(_data.gameObject);

        rpmGauge = FindAnyObjectByType<RPMGauge>();
        NitroBar = FindAnyObjectByType<Slider>();

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
        SetNitroParticles(gameObject.GetComponent<Trail>());
        SetMaxNitroCapacity(100f);
        SetNitroConsumptionRate(40f);
        speedTextForUI = rpmGauge.transform.Find("Speed").GetComponent<Text>();
        gearTextForUI = rpmGauge.transform.Find("GearNum").GetComponent<Text>();

        freeLookCamera.m_XAxis.Value = 0f;
        freeLookWaitTime = 1.0f;
        MainCamera = FindAnyObjectByType<Camera>();
        body = gameObject;
        SetCarRB(gameObject.GetComponent<Rigidbody>());
        freeLookCamera.enabled = true;
        ignition = true;
        braking = false;
        //SetEngineSound(transform.Find("EngineSound").GetComponent<AudioSource[]>());
        ForcePlayEngineSound();
        SetBaseEngineAcceleration(5f);
        SetAutoGear(false);
        SetAntiRoll(35000f);
        SetShiftTiming(0.5f);
        SetBrakePower(3000f);
        SetDriveAxel(eCAR_DRIVEAXEL.eRWD);
        SetFriction();
        SpawnSmoke();
        CalculateOptimalShiftPoints();
        StartCoroutine(Controlling());
        //StartCoroutine(Engine());
        StartCoroutine(UpdateNitro());
        StartCoroutine(UIUpdating());
    }

    private void Update()
    {
        SetSpeedToKMH();
        Steering(Input.GetAxis("Horizontal"));
        SetSlpingAngle();
        CameraUpdate();
        if (GetCurrentGear() != eGEAR.eGEAR_NEUTURAL)
            clutch = Input.GetKey(KeyCode.C) == true ? 0 : Mathf.Lerp(clutch, 1, Time.deltaTime);
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
        if (Input.GetKeyDown(KeyCode.Keypad0))
            ForceChangeGear(eGEAR.eGEAR_REVERSE);
        if (Input.GetKeyDown(KeyCode.Keypad1))
            ForceChangeGear(eGEAR.eGEAR_FIRST);
        if (Input.GetKeyDown(KeyCode.Keypad2))
            ForceChangeGear(eGEAR.eGEAR_SECOND);
        if (Input.GetKeyDown(KeyCode.Keypad3))
            ForceChangeGear(eGEAR.eGEAR_THIRD);
        if(Input.GetKeyDown(KeyCode.Keypad4))
            ForceChangeGear(eGEAR.eGEAR_FOURTH);
        if (Input.GetKeyDown(KeyCode.Keypad5))
            ForceChangeGear(eGEAR.eGEAR_FIFTH);
        if (Input.GetKeyDown(KeyCode.Keypad6))
            ForceChangeGear(eGEAR.eGEAR_SIXTH);
    }

    private void FixedUpdate()
    {
        EngineForUpdate();
        ApplyAerodynamicDrag();
        UpdatingFriction();
        //AntiRollBar();
        // Braking()를 호출하기 전에 플레이어 브레이크 입력을 처리합니다.
        // (Update의 논리를 복제하므로 입력 처리를 통합하는 것을 고려하십시오.)
        throttle = Input.GetAxis("Vertical");

        if (ignition)
        {
            if (GetCurrentGear() == eGEAR.eGEAR_REVERSE)
            {
                if (throttle > 0.05f)
                {
                    brakeInput = Mathf.Abs(throttle);
                    throttle = 0;
                }
                else if (throttle < -0.05f)
                {
                    brakeInput = 0f;
                }
                else
                {
                    brakeInput = 0f;
                    throttle = 0f;
                }
            }
            else
            {
                if (throttle < -0.05f)
                {
                    brakeInput = Mathf.Abs(throttle);
                    throttle = 0;
                }
                else if (throttle > 0.05f)
                {
                    brakeInput = 0f;
                }
                else
                {
                    brakeInput = 0f;
                    throttle = 0f;
                }
            }

            // 주의: 이 수정된 입력 로직은 TorqueToWheel 함수가
            // 음수 throttle을 후진 가속 토크로 올바르게 해석할 수 있어야 작동합니다.
            // 현재 TorqueToWheel의 Mathf.Max(0, throttle) 부분은 이대로라면
            // 음수 throttle을 0으로 만들게 됩니다.
            // 따라서 TorqueToWheel 함수에서도 후진 기어 상태일 때는
            // 음수 throttle을 받아 후진 토크를 적용하는 로직 수정이 필요할 수 있습니다.
        }
        else // 시동이 꺼진 경우
        {
            throttle = 0f;
        }

        // 슬립 계산 및 입력 처리 후 FixedUpdate에서 Braking() 호출
    }

    IEnumerator Controlling()
    {
        WaitForSeconds wfs = new WaitForSeconds(0.02f);
        while (true)
        {
            yield return wfs;

            
            
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
