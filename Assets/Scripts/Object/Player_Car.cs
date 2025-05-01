using System.Collections;
using Cinemachine;
using Tiny;
using UnityEngine;
using UnityEngine.UI;
using Fusion;

public class Player_Car : Car
{
    public Vector3 inputCheck;

    public Curve_data _data;
    [SerializeField] private CinemachineFreeLook freeLookCamera;
    [SerializeField] private CinemachineFreeLook sideCamera;
    private Transform firstPersonCamera;
    private Transform lookBack;
    [SerializeField] private Camera MainCamera;
    [SerializeField] private RadialBlur radialBlur;

    private bool firstPersonCameraCheck = false;
    private GameObject windowF, windowL, windowR;

    private bool freeLook;
    private float freeLookWaitTime;
    private float fov = 30f;

    public bool braking, sideBraking, up, down, left, right;
    private bool gearUp, gearDown;
    private byte forceGear;
    private float clutching;

    [Networked] public NetworkInputManager inputData { get; set; }

    public void Init()
    {
        _data = gameObject.GetComponent<Curve_data>();
        SetEngineCurves(_data.horsePower, _data.torque);
        SetSteeringCurve(_data.steer);

        //if(gameObject.transform.Find("WindowFront").gameObject != null)
        //    windowF = gameObject.transform.Find("WindowFront").gameObject;
        //if(gameObject.transform.Find("WindowLeft").gameObject != null)
        //    windowL = gameObject.transform.Find("WindowLeft").gameObject;
        //if(gameObject.transform.Find("WindowRight").gameObject != null)
        //    windowR = gameObject.transform.Find("WindowRight").gameObject;

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
        HeadLightSwitch();
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
        StartCoroutine(Engine());
        StartCoroutine(Controlling());
        StartCoroutine(UpdateNitro());
        StartCoroutine(UIUpdating());
    }

    public void CamInit()
    {
        freeLookCamera = gameObject.transform.Find("FreeLookCamera").GetComponent<CinemachineFreeLook>();
        sideCamera = gameObject.transform.Find("ForceSideCamera").GetComponent<CinemachineFreeLook>();
        firstPersonCamera = gameObject.transform.Find("FirstPersonCamera");
        lookBack = gameObject.transform.Find("LookBackCamera");
        freeLookCamera.Follow = gameObject.transform;
        freeLookCamera.LookAt = gameObject.transform.Find("FocusPoint").transform;
        sideCamera.Follow = gameObject.transform;
        sideCamera.LookAt = gameObject.transform.Find("FocusPoint").transform;
        radialBlur = MainCamera.GetComponent<RadialBlur>();
    }

    private void Update()
    {
        SetSpeedToKMH();
        //if (Input.GetKeyDown(KeyCode.G))
        //{
        //    drifting = !drifting;
        //    ChangeFriction(drifting);
        //}
        if(Input.GetKeyDown(KeyCode.F)) { HeadLightSwitch(); }
        UpdatingWheels();
        if (Input.GetKeyDown(KeyCode.V))
            firstPerson();
    }

    public override void FixedUpdateNetwork()
    {
        //AntiRollBar();
        //Braking()�� ȣ���ϱ� ���� �÷��̾� �극��ũ �Է��� ó���մϴ�.
        if (GetInput(out NetworkInputManager data))
        {
            data.direction.Normalize();
            throttle = data.direction.y;
            Steering(data.direction.x);
            sideBraking = data.sideBraking;
            ActivateNitro(data.boosting);
            clutching = data.direction.z;
            forceGear = data.forceGear;
            gearUp = data.gearUP;
            gearDown = data.gearDOWN;
            inputCheck = data.direction;
        }
        if (gearUp)
            ChangeGear(true);
        if (gearDown)
            ChangeGear(false);
        if (sideBraking)
            SideBrakingDown();
        else
            SideBrakingUp();
        if (GetCurrentGear() != eGEAR.eGEAR_NEUTURAL)
            clutch = Mathf.Lerp(1, 0, clutching);
        switch(forceGear)
        {
            case 1:
                ForceChangeGear(eGEAR.eGEAR_REVERSE);
                break;
            case 2:
                ForceChangeGear(eGEAR.eGEAR_FIRST);
                break;
            case 3:
                ForceChangeGear(eGEAR.eGEAR_SECOND);
                break;
            case 4:
                ForceChangeGear(eGEAR.eGEAR_THIRD);
                break;
            case 5:
                ForceChangeGear(eGEAR.eGEAR_FOURTH);
                break;
            case 6:
                ForceChangeGear(eGEAR.eGEAR_FIFTH);
                break;
            case 7:
                ForceChangeGear(eGEAR.eGEAR_SIXTH);
                break;
        }
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

            // ����: �� ������ �Է� ������ TorqueToWheel �Լ���
            // ���� throttle�� ���� ���� ��ũ�� �ùٸ��� �ؼ��� �� �־�� �۵��մϴ�.
            // ���� TorqueToWheel�� Mathf.Max(0, throttle) �κ��� �̴�ζ��
            // ���� throttle�� 0���� ����� �˴ϴ�.
            // ���� TorqueToWheel �Լ������� ���� ��� ������ ����
            // ���� throttle�� �޾� ���� ��ũ�� �����ϴ� ���� ������ �ʿ��� �� �ֽ��ϴ�.
        }
        else // �õ��� ���� ���
        {
            throttle = 0f;
        }
        // ���� ��� �� �Է� ó�� �� FixedUpdate���� Braking() ȣ��
    }

    IEnumerator Controlling()
    {
        WaitForSeconds wfs = new WaitForSeconds(0.01f);
        while (true)
        {
            yield return wfs;
            CameraUpdate();
            SetSlpingAngle();
            //EngineForUpdate();
            ApplyAerodynamicDrag();
            UpdatingFriction();
            EffectDrift();
            SetRadialBlur();
        }
    }
    public void ChangeMode(bool _driftMode) { ChangeFriction(_driftMode); }
    public void SetCamAndUI(Camera _cam, Slider _NitroBar, RPMGauge _rpmGauge)
    {
        MainCamera = _cam;
        NitroBar = _NitroBar;
        rpmGauge = _rpmGauge;
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
