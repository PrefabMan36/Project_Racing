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
    private MainGame_Manager gameManager;

    [SerializeField] private GameObject cameraData;
    [SerializeField] private GameObject cameraPositions;
    [SerializeField] private Transform focusPoint;
    [SerializeField] private CinemachineFreeLook freeLookCamera;
    [SerializeField] private CinemachineFreeLook sideCamera;
    [SerializeField] private Transform firstPersonCamera;
    [SerializeField] private Transform lookBack;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private RadialBlur radialBlur;

    private bool firstPersonCameraCheck;

    private bool freeLook;
    private float freeLookWaitTime;
    private float fov = 30f;

    public bool braking, sideBraking, up, down, left, right;
    private bool gearUp, gearDown;
    private byte forceGear;
    private float clutching;

    [Networked] public NetworkInputManager inputData { get; set; }

    public override void Spawned()
    {
        Runner.SetIsSimulated(Object, true);
        gameManager = FindAnyObjectByType<MainGame_Manager>();
        gameManager.Init(this);
    }
    public void Init()
    {
        if (HasInputAuthority)
        {
            cameraPositions = Instantiate(cameraData, transform);
            freeLookCamera = cameraPositions.transform.Find("FreeLookCamera").GetComponent<CinemachineFreeLook>();
            sideCamera = cameraPositions.transform.Find("ForceSideCamera").GetComponent<CinemachineFreeLook>();
            firstPersonCamera = cameraPositions.transform.Find("FirstPersonCamera");
            lookBack = cameraPositions.transform.Find("LookBackCamera");

            freeLookCamera.Follow = this.transform;
            freeLookCamera.LookAt = focusPoint;
            sideCamera.Follow = this.transform;
            sideCamera.LookAt = focusPoint;

            freeLookCamera.m_XAxis.Value = 0f;
            freeLookWaitTime = 1.0f;
            freeLookCamera.enabled = true;
            StartCoroutine(CameraUpdate());
            StartCoroutine(UIUpdating());
        }
        _data = gameObject.GetComponent<Curve_data>();
        SetEngineCurves(_data.horsePower, _data.torque);
        SetSteeringCurve(_data.steer);

        SetNitroInstall(true);
        SetNitroParticles(gameObject.GetComponent<Trail>());
        SetMaxNitroCapacity(100f);
        SetNitroConsumptionRate(40f);
        body = gameObject;
        SetCarRB(gameObject.GetComponent<Rigidbody>());
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
        SetRadialBlur();
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
            SetSlpingAngle();
            //EngineForUpdate();
            ApplyAerodynamicDrag();
            UpdatingFriction();
            EffectDrift();
        }
    }
    public void ChangeMode(bool _driftMode) { ChangeFriction(_driftMode); }

    public void SetCamera(Camera _camera)
    {
        mainCamera = _camera;
        radialBlur = mainCamera.gameObject.GetComponent<RadialBlur>();
    }
    public void SetNitroBar(Slider _nitroBar) { nitroBar = _nitroBar; }
    public void SetRPMGauge(RPMGauge _rpmGauge)
    {
        rpmGauge = _rpmGauge;
        speedTextForUI = rpmGauge.transform.Find("Speed").GetComponent<Text>();
        gearTextForUI = rpmGauge.transform.Find("GearNum").GetComponent<Text>();
    }

    IEnumerator CameraUpdate()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.01f);
        while (true)
        {
            yield return waitForSeconds;
            if (Input.GetAxis("Vertical2") < 0)
            {
                freeLookCamera.enabled = false;
                sideCamera.enabled = false;
                mainCamera.fieldOfView = fov * 2f;
                mainCamera.transform.position = lookBack.position;
                mainCamera.transform.rotation = lookBack.rotation;
            }
            else if (firstPersonCameraCheck)
            {
                freeLookCamera.enabled = false;
                sideCamera.enabled = false;
                mainCamera.transform.position = firstPersonCamera.position;
                mainCamera.transform.rotation = firstPersonCamera.rotation;
            }
            else if (Input.GetAxis("Horizontal2") + Input.GetAxis("Vertical2") == 0)
            {
                freeLookCamera.enabled = true;
                sideCamera.enabled = false;
                up = false;
                down = false;
                left = false;
                right = false;
            }
            else if (Input.GetAxis("Horizontal2") != 0)
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
