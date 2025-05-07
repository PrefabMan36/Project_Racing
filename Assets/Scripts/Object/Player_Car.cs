using System.Collections;
using Cinemachine;
using Tiny;
using UnityEngine;
using UnityEngine.UI;
using Fusion;

public class Player_Car : Car
{
    private int ID;

    public Vector3 inputCheck;

    public Curve_data _data;
    private MainGame_Manager gameManager;

    [SerializeField] private GameObject cameraData;
    [SerializeField] private GameObject cameraPositions;
    [SerializeField] private NetworkObject networkObject;
    [SerializeField] private Transform focusPoint;
    [SerializeField] private CinemachineFreeLook freeLookCamera;
    [SerializeField] private CinemachineFreeLook sideCamera;
    [SerializeField] private Transform firstPersonCamera;
    [SerializeField] private Transform lookBack;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private RadialBlur radialBlur;


    private Rank_Data rankData = new Rank_Data();
    [Networked, SerializeField] private byte rank { get; set; } = 0;
    [Networked, SerializeField] private int currentCheckpointIndex { get; set; } = 1;
    [Networked, SerializeField] private short lap { get; set; } = 0;
    private Transform nextCheckPoint;
    private float distanceToCheckPoint;

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
        gameManager.CarInit(this, HasInputAuthority);
        networkObject = GetComponent<NetworkObject>();
        rankData.playerId = networkObject.Id;
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
        SetWheels(transform.Find("Tire_LF").gameObject, transform.Find("Wheel_FrontLeft").GetComponent<WheelCollider>(), transform.Find("TrailFrontLeft").GetComponent<TrailRenderer>(), true);
        SetWheels(transform.Find("Tire_RF").gameObject, transform.Find("Wheel_FrontRight").GetComponent<WheelCollider>(), transform.Find("TrailFrontRight").GetComponent<TrailRenderer>(), true);
        SetWheels(transform.Find("Tire_LR").gameObject, transform.Find("Wheel_RearLeft").GetComponent<WheelCollider>(), transform.Find("TrailRearLeft").GetComponent<TrailRenderer>(), false);
        SetWheels(transform.Find("Tire_RR").gameObject, transform.Find("Wheel_RearRight").GetComponent<WheelCollider>(), transform.Find("TrailRearRight").GetComponent<TrailRenderer>(), false);
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
        if (Input.GetKeyDown(KeyCode.F)) { HeadLightSwitch(); }
        UpdatingWheels();
        if (Input.GetKeyDown(KeyCode.V))
            firstPerson();
        SetRadialBlur();
    }

    public override void FixedUpdateNetwork()
    {
        //AntiRollBar();
        //Braking()를 호출하기 전에 플레이어 브레이크 입력을 처리합니다.
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
        switch (forceGear)
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
        if (freeLook)
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
            radialBlur.blurStrength = Mathf.Lerp(0f, 1f, GetSpeed() / 200f);
            radialBlur.blurWidth = Mathf.Lerp(0f, 1f, GetSpeed() / 200f) + GetNitroBlurWidth();
        }
    }
    public void SetCheckPoint(int _checkPoint) { currentCheckpointIndex = _checkPoint; }
    public int GetCheckPoint() { return currentCheckpointIndex; }

    public void SetNextCheckPointPosition(CheckPoint _nextCheckPoint)
    {
        nextCheckPoint = _nextCheckPoint.transform;
    }
    public float GetDistanceToCheckPoint()
    {
        if (nextCheckPoint != null)
            distanceToCheckPoint = Vector3.Distance(transform.position, nextCheckPoint.position);
        return distanceToCheckPoint;
    }

    public void SetLap(short _lap) { lap = _lap; }

    public short GetLap() { return lap; }

    public void SetID(int _id) { ID = _id; }
    public int GetID() { return ID; }

    public Rank_Data GetRankData()
    {
        rankData.lap = lap;
        rankData.currentCheckpointIndex = currentCheckpointIndex;
        rankData.distanceToCheckPoint = distanceToCheckPoint;
        return rankData;
    }
}
