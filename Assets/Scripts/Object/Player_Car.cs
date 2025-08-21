using System.Collections;
using Cinemachine;
using Tiny;
using UnityEngine;
using UnityEngine.UI;
using Fusion;
using TMPro;
using System;
//using static Fusion.NetworkBehaviour;
//using UnityEngine.Windows;

//이 클래스는 플레이어 차량을 나타내며, 차량의 물리적 특성과 카메라, UI 등을 관리합니다.
//Car 클래스를 상속받아 차량의 동작을 정의합니다.
//이 클래스는 Fusion 네트워킹을 사용하여 멀티플레이어 환경에서 차량의 상태를 동기화합니다.
//이 클래스는 상속받은 차량의 입력을 처리하고, 카메라를 설정하며, 차량의 물리적 특성을 초기화합니다.
//이 클래스는 상속받은 차량의 기어, 엔진, 브레이크 등을 관리합니다.
//이 클래스는 차량의 카메라와 UI를 업데이트하는 코루틴을 포함합니다.

public class Player_Car : Car
{
    private ChangeDetector changeDetector;
    [Header("PlayerSet")]
    [SerializeField] private bool driftCar;
    [SerializeField] private int carNumber;
    [SerializeField] private int ID;
    [SerializeField] private NetworkId playerId;
    [Networked] public NetworkString<_16> playerName { get; set; }
    [SerializeField] private bool nameChanged = false;

    public Vector3 inputCheck;

    public Curve_data _data;
    private MainGame_Manager gameManager;

    [SerializeField] private GameObject[] wheelsModels = new GameObject[4];
    [SerializeField] private bool wheelSet = false;
    [SerializeField] private CarWheelsData wheelData;

    [SerializeField] private GameObject cameraData;
    [SerializeField] private GameObject cameraPositions;
    [SerializeField] private NetworkObject networkObject;
    [SerializeField] private Transform focusPoint;
    [SerializeField] private Transform dynamicLookAtTarget, lookLeftPoint, lookRightPoint;
    [SerializeField] private CinemachineFreeLook raceCamera;
    [SerializeField] private Transform firstPersonCamera;
    [SerializeField] private Transform lookBack;

    Vector3 lookDirection;


    [SerializeField] private float cameraLockSpeedThreshold = 5f;
    [Range(0, 1)]
    [SerializeField] private float cameraLookAtBlend = 0.2f;

    [SerializeField] private Camera mainCamera;
    [SerializeField] private RadialBlur radialBlur;


    private Rank_Data rankData = new Rank_Data();
    [Networked, SerializeField] private byte rank { get; set; } = 0;
    [Networked, SerializeField] private int currentCheckpointIndex { get; set; } = 1;
    [Networked, SerializeField] private short lap { get; set; } = 0;
    [Networked, SerializeField] private float distanceToCheckPoint { get; set; }
    [SerializeField] private Transform nextCheckPoint;
    [SerializeField] private DirectionToNext navigation;
    [SerializeField] private float gameTimer = 0;
    [SerializeField] private float finishedTime = 0f;
    [SerializeField] private bool raceStarted = false;
    [SerializeField] private bool calculateDistance = false;

    private bool firstPersonCameraCheck;
    Vector3 forwardDir;
    Vector3 velocityDir;

    private float fov = 30f;
    private float cameraFollowDamping = 1.0f;

    public bool light, braking, sideBraking, up, down, left, right;
    [SerializeField]private bool turnLeft, turnRight;
    private bool gearUp, gearDown;
    private byte forceGear;
    private float clutching;

    [Networked] public NetworkInputManager inputData { get; set; }
    private bool localPlayer = false;

    [SerializeField] public int carState = 0;

    public override void Spawned()
    {
        if (playerName.Value != "")
        { gameObject.name = playerName.Value; }
        Runner.SetIsSimulated(Object, true);// 시뮬레이션을 활성화합니다.
        gameManager = FindAnyObjectByType<MainGame_Manager>();// 게임 매니저를 찾습니다.
        gameManager.CarInit(this, HasInputAuthority);// 게임 매니저를 이용해 차량을 초기화합니다.
        networkObject = GetComponent<NetworkObject>();// 네트워크 오브젝트를 가져옵니다.
        playerId = networkObject.Id;// 플레이어 ID를 설정합니다.

        changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
    }

    public override void Render()
    {
        // 이 오브젝트의 네트워크 속성 변경을 감지합니다.
        foreach (var change in changeDetector.DetectChanges(this))
        {
            // 변경된 속성의 이름이 'playerName'인지 확인합니다.
            if (change == nameof(playerName))
            {
                // 이름이 변경되었다면, UI를 업데이트하는 메서드를 호출합니다.
                UpdateNameDisplay();
            }
        }
    }

    public void Init()
    {
        if (HasInputAuthority)
        {
            cameraPositions = Instantiate(cameraData, transform);
            dynamicLookAtTarget = cameraPositions.transform.Find("DynamicLookAtTarget");
            raceCamera = cameraPositions.transform.Find("raceCamera").GetComponent<CinemachineFreeLook>();
            firstPersonCamera = transform.transform.Find("FirstPersonCamera");
            lookBack = cameraPositions.transform.Find("LookBackCamera");
            lookLeftPoint = cameraPositions.transform.Find("LookLeftPoint");
            lookRightPoint = cameraPositions.transform.Find("LookRightPoint");

            raceCamera.Follow = this.transform;
            raceCamera.LookAt = dynamicLookAtTarget;

            raceCamera.m_XAxis.Value = 0f;
            raceCamera.enabled = true;
            localPlayer = true;

            Setting_Data.CameraFollowDamping = 2f;
            cameraFollowDamping = Setting_Data.CameraFollowDamping;

            for (int i = 0; i < 3; i++)
            {
                var transposer = raceCamera.GetRig(i).GetCinemachineComponent<CinemachineTransposer>();
                if (transposer != null)
                {
                    // Body Damping 값을 설정하여 카메라의 반응성을 조절합니다.
                    transposer.m_XDamping = 0f;
                    transposer.m_YDamping = 0f;
                    transposer.m_ZDamping = 0f;
                    transposer.m_YawDamping = cameraFollowDamping;
                }
            }

        }
        SetWheels(wheelsModels[0], transform.Find("Wheel_FrontLeft").GetComponent<WheelCollider>(), transform.Find("TrailFrontLeft").GetComponent<TrailRenderer>(), true);
        SetWheels(wheelsModels[1], transform.Find("Wheel_FrontRight").GetComponent<WheelCollider>(), transform.Find("TrailFrontRight").GetComponent<TrailRenderer>(), true);
        SetWheels(wheelsModels[2], transform.Find("Wheel_RearLeft").GetComponent<WheelCollider>(), transform.Find("TrailRearLeft").GetComponent<TrailRenderer>(), false);
        SetWheels(wheelsModels[3], transform.Find("Wheel_RearRight").GetComponent<WheelCollider>(), transform.Find("TrailRearRight").GetComponent<TrailRenderer>(), false);
        _data = gameObject.GetComponent<Curve_data>();
        driver = transform.GetComponentInChildren<Driver>();
        SetSteeringCurve(_data.steer);
        //isTCSEnabled = true;
        //isABSEnabled = true;
        //isEngineBrakingEnabled = true;
        SetNitroInstall(true);
        SetNitroParticles(gameObject.GetComponent<Trail>());
        SetMaxNitroCapacity(100f);
        SetNitroConsumptionRate(40f);
        body = gameObject;
        SetCarRB(gameObject.GetComponent<Rigidbody>());
        ignition = true;
        braking = false;
        //SetEngineSound(transform.Find("EngineSound").GetComponent<AudioSource[]>());

        if(gameManager.GetIsNight())
            ForceLightOn();// 헤드라이트 스위치

        TryGetComponent(out charger);
        ForcePlayEngineSound();// 엔진 사운드 강제 재생
        SetBaseEngineAcceleration(5f);// 기본 엔진 가속도 설정
        SetAutoGear(true);// 자동 기어 설정
        SetAntiRoll(2.0f);// 안티롤 설정
        SetShiftTiming(0.5f);// 기어 변속 타이밍 설정
        //SetDriveAxel(eCAR_DRIVEAXEL.eRWD);// 구동축 설정
        SetDriveWheels();
        SetFriction();// 마찰력 설정
        SetWheelsData(wheelData);
        SpawnSmoke();// 스폰 연기 설정
        CalculateOptimalShiftPoints();// 최적 기어 변속 포인트를 계산합니다.
        CalculateGearSpeedLimits();
        if (centerMass != null)
        {
            originalCenterOfMass = centerMass.transform.localPosition;
            loweredCenterOfMass = originalCenterOfMass + new Vector3(0, -0.2f, 0);
        }
        //if (HasStateAuthority)
        //{
        //    StartCoroutine(Engine());// 엔진 코루틴 시작
        //    StartCoroutine(UpdateWheels());// 브레이크 코루틴 시작
        //    StartCoroutine(UpdateBody());// 스티어링 코루틴 시작
        //    StartCoroutine(UpdateNitro());// 부스트 코루틴 시작
        //}
        if(HasInputAuthority)
        {
            StartCoroutine(UIUpdating());
        }
        StartCoroutine(UpdateVisual());
        ForceChangeGear(eGEAR.eGEAR_FIRST);// 첫 번째 기어로 강제 변경
        SetUpFinished = true;// 차량 설정 완료 상태로 변경
    }

    public void SetCarWheelData(CarWheelsData wheelsData)
    {
        if(!wheelSet)
        {
            wheelSet = true;
            wheelData = wheelsData;
        }
    }

    private void Update()
    {
        SetSpeedToKMH();
        //if (Input.GetKeyDown(KeyCode.G))
        //{
        //    drifting = !drifting;
        //    ChangeFriction(drifting);
        //}
        //if (Input.GetKeyDown(KeyCode.F)) { HeadLightSwitch(); }

        if (Input.GetKeyDown(KeyCode.V))
            firstPerson();
        SetRadialBlur();
        if (localPlayer && raceStarted)
        {
            gameTimer += Time.deltaTime;
            gameManager.SetTimer(gameTimer);
        }
    }

    public override void FixedUpdateNetwork()
    {
        GetInputData();
        if (gearUp)
            ChangeGear(true);// 기어 업
        if (gearDown)
            ChangeGear(false);// 기어 다운
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
            brakeInput = 0f;
            if (throttle < -0.05f)
            {
                if (speed > 1f && GetCurrentGear() != eGEAR.eGEAR_REVERSE)
                {
                    brakeInput = Mathf.Abs(throttle);
                    throttle = 0f;
                }
            }
            else if (throttle > 0.05f)
            {
                if (GetCurrentGear() == eGEAR.eGEAR_REVERSE && speed > 1f)
                {
                    brakeInput = Mathf.Abs(throttle);
                    throttle = 0f;
                }
            }
        }
        else // 시동이 꺼진 경우
        {
            throttle = 0f;
        }

        if (sideBraking)
            SideBrakingDown();
        else
            SideBrakingUp();

        if(HasStateAuthority)
        {
            PhysicsForNetworkUpdate();
        }

        ChangeDriverAnimation();//운전자의 에니메이션 상태를 갱신 합니다.
    }

    private void LateUpdate()
    {
        if (!localPlayer) return;

        CamMoveNew();
    }
    protected override void GetInputData()
    {
        if (GetInput(out NetworkInputManager data))
        {
            //data.direction.Normalize();
            //throttle = MathF.Round(Mathf.Lerp(throttle, data.direction.y, 0.2f),1);
            throttle = data.direction.y;
            SteeringInput(data.direction.x);
            if (data.direction.x > 0)
            {
                turnRight = true;
                turnLeft = false;
            }
            else if (data.direction.x < 0)
            {
                turnLeft = true;
                turnRight = false;
            }
            else
            {
                turnRight = false;
                turnLeft = false;
            }
            sideBraking = data.sideBraking;
            if (data.direction.z > 0)
                nitroInput = true;
            else
                nitroInput = false;
            // = MathF.Round(Mathf.Lerp(clutch, data.direction.z, Time.deltaTime * 4));
            if (data.headLight.IsSet(1))
                HeadLightSwitch();
            clutching = data.clutch;
            forceGear = data.forceGear;
            gearUp = data.gearUP;
            gearDown = data.gearDOWN;
        }
    }

    public void ResetTimer()
    { gameTimer = 0f; }

    public void SetCamera(Camera _camera)
    {
        mainCamera = _camera;
        radialBlur = mainCamera.gameObject.GetComponent<RadialBlur>();
        navigation = mainCamera.GetComponent<DirectionToNext>();
    }
    public void SetNitroBar(Slider _nitroBar) { nitroBar = _nitroBar; }
    public void SetRPMGauge(RPMGauge _rpmGauge)
    {
        rpmGauge = _rpmGauge;
        speedTextForUI = rpmGauge.transform.Find("Speed").GetComponent<TextMeshProUGUI>();
        gearTextForUI = rpmGauge.transform.Find("GearNum").GetComponent<TextMeshProUGUI>();
    }

    private void CamMoveNew()
    {
        if (Input.GetAxis("Horizontal2") > 0)
        {
            raceCamera.enabled = false;
            mainCamera.transform.position = lookLeftPoint.position;
            mainCamera.transform.rotation = lookLeftPoint.rotation;
            return;
        }
        else if (Input.GetAxis("Horizontal2") < 0)
        {
            raceCamera.enabled = false;
            mainCamera.transform.position = lookRightPoint.position;
            mainCamera.transform.rotation = lookRightPoint.rotation;
            return;
        }
        else if (firstPersonCameraCheck)
        {
            if (Input.GetAxis("Vertical2") < 0)
            {
                raceCamera.enabled = false;
                mainCamera.transform.position = lookBack.position;
                mainCamera.transform.rotation = lookBack.rotation;
                return;
            }
            else
            {
                raceCamera.enabled = false;
                mainCamera.transform.position = firstPersonCamera.position;
                mainCamera.transform.rotation = firstPersonCamera.rotation;
            }
        }
        else
        {
            raceCamera.enabled = true;
            raceCamera.m_XAxis.m_MaxSpeed = 0f; // 마우스로 좌우 회전 방지
            raceCamera.m_XAxis.m_Recentering.m_enabled = true;
            raceCamera.m_RecenterToTargetHeading.m_enabled = true;
            raceCamera.m_XAxis.m_Recentering.m_WaitTime = 0f;
            raceCamera.m_XAxis.m_Recentering.m_RecenteringTime = 0f; // 매우 빠르게 복귀
            if(Input.GetAxis("Vertical2") < 0)
            {
                lookDirection = -transform.forward;
                raceCamera.m_Heading.m_Bias = 180f;
            }
            else if (GetSpeed() < cameraLockSpeedThreshold)
            {
                lookDirection = transform.forward;
                raceCamera.m_Heading.m_Bias = 0f;
            }
            else
            {
                if(Vector3.Dot(transform.forward, carRB.velocity) < -1f)
                {
                    forwardDir = -transform.forward;
                    velocityDir = carRB.velocity.normalized;
                    lookDirection = Vector3.Slerp(forwardDir, velocityDir, cameraLookAtBlend);
                    raceCamera.m_Heading.m_Bias = 180f;
                }
                else
                {
                    forwardDir = transform.forward;
                    velocityDir = carRB.velocity.normalized;
                    lookDirection = Vector3.Slerp(forwardDir, velocityDir, cameraLookAtBlend);
                    raceCamera.m_Heading.m_Bias = 0f;
                }
            }
            dynamicLookAtTarget.position = transform.position + lookDirection * 30f; // 20f는 임의의 거리
        }

        if (firstPersonCameraCheck)
            mainCamera.fieldOfView =
                GetIsNitroActive() ?
                Mathf.Lerp(mainCamera.fieldOfView, fov * 2.75f, Time.deltaTime) :
                Mathf.Lerp(mainCamera.fieldOfView, fov * 2f, Time.deltaTime);
        else
            raceCamera.m_Lens.FieldOfView =
                GetIsNitroActive() ?
                Mathf.Lerp(raceCamera.m_Lens.FieldOfView, fov * 3.5f, Time.deltaTime) :
                Mathf.Lerp(raceCamera.m_Lens.FieldOfView, fov * 2f, Time.deltaTime);
    }

    private void firstPerson() { firstPersonCameraCheck = !firstPersonCameraCheck; }

    public int GetCarNumber() { return carNumber; }

    private void SetRadialBlur()
    {
        if (radialBlur != null)
        {
            if (nitroEnabled)
            {
                radialBlur.enabled = true;
                radialBlur.blurStrength = Mathf.Lerp(0f, 1f, GetSpeed() / 200f);
                radialBlur.blurWidth = Mathf.Lerp(0f, 1f, GetSpeed() / 200f) + GetNitroBlurWidth();
            }
            else
                radialBlur.enabled = false;
        }
    }

    public void SetNavigation(DirectionToNext arrow)
    {
        if(navigation == null)
        {
            navigation = arrow;
        }
        else
        {
            Debug.LogWarning("Navigation already set.");
        }
    }

    public void SetCheckPoint(int _checkPoint) { currentCheckpointIndex = _checkPoint; }
    public int GetCheckPointIndex() { return currentCheckpointIndex; }

    public void SetNextCheckPointPosition(CheckPoint _nextCheckPoint)
    {
        if(nextCheckPoint == null)
            nextCheckPoint = new GameObject("NextCheckPoint").transform;
        _nextCheckPoint.SelecteByNaviPoint();
        nextCheckPoint.position = _nextCheckPoint.transform.position;
        if (navigation != null)
        {
            navigation.SetNextCheckPoint(nextCheckPoint);
        }
        if (!calculateDistance)
        {
            calculateDistance = true;
            if (HasInputAuthority)
                StartCoroutine(CalculateDistanceToCheckPoint());
        }
    }
    IEnumerator CalculateDistanceToCheckPoint()
    {
        if (nextCheckPoint == null)
        {
            calculateDistance = false;
            yield break;
        }
        WaitForSeconds waitForSeconds = new WaitForSeconds(Shared.frame60);
        while (true)
        {
            yield return waitForSeconds;
            distanceToCheckPoint = Vector3.Distance(transform.position, nextCheckPoint.position);
        }
    }

    public void SetLap(short _lap) { lap = _lap; }

    public short GetLap() { return lap; }

    public void SetFinishTime(float _time)
    {
        finishedTime = _time;
    }
    public void FinishRace()
    {
        raceStarted = false;
        ForceStop();
    }

    public float GetFinishTime()
    {
        return finishedTime;
    }
    public void SetID(int _id) { ID = _id; }
    public int GetID() { return ID; }

    public void SetName(string _name)
    {
        playerName = _name;
    }

    private void UpdateNameDisplay()
    {
        gameObject.name = playerName.Value;
        if (gameManager != null && Object != null)
            gameManager.SetRank(Object.Id);
    }

    public string GetName() { return playerName.Value; }

    public Rank_Data GetRankData()
    {
        rankData.playerId = playerId;
        rankData.lap = lap;
        rankData.currentCheckpointIndex = currentCheckpointIndex;
        rankData.distanceToCheckPoint = distanceToCheckPoint;
        return rankData;
    }

    public override void StartRace()
    {
        base.StartRace();
        raceStarted = true;
    }

    public bool GetLocalPlayer() { return localPlayer; }

    private void ChangeDriverAnimation()
    {
        if (flipped)
            carState = 6;
        else if (hitted)
            carState = 5;
        //else if (GetCurrentGear() == eGEAR.eGEAR_REVERSE)
        //    carState = 3;
        //else if (isNitroActive)
        //    carState = 4;
        else if (turnRight && turnLeft)
            carState = 0;
        else if (turnRight)
            carState = 2;
        else if (turnLeft)
            carState = 1;
        else
            carState = 0;

        driver.SetState(carState);
    }
}
