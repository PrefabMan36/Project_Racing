using System.Collections;
using Cinemachine;
using Tiny;
using UnityEngine;
using UnityEngine.UI;
using Fusion;
using TMPro;
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

    [SerializeField] private GameObject cameraData;
    [SerializeField] private GameObject cameraPositions;
    [SerializeField] private NetworkObject networkObject;
    [SerializeField] private Transform focusPoint;
    [SerializeField] private Transform dynamicLookAtTarget, lookLeftPoint, lookRightPoint;
    [SerializeField] private CinemachineFreeLook freeLookCamera;
    [SerializeField] private CinemachineFreeLook sideCamera;
    [SerializeField] private Transform firstPersonCamera;
    [SerializeField] private Transform lookBack;

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
    [SerializeField] private float gameTimer = 0;
    [SerializeField] private float finishedTime = 0f;
    [SerializeField] private bool raceStarted = false;
    [SerializeField] private bool calculateDistance = false;

    private bool firstPersonCameraCheck;

    private bool freeLook;
    private float freeLookWaitTime;
    private float fov = 30f;
    private float cameraFollowDamping = 1.0f;

    public bool braking, sideBraking, up, down, left, right;
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
            freeLookCamera = cameraPositions.transform.Find("FreeLookCamera").GetComponent<CinemachineFreeLook>();
            firstPersonCamera = transform.transform.Find("FirstPersonCamera");
            lookBack = cameraPositions.transform.Find("LookBackCamera");
            lookLeftPoint = cameraPositions.transform.Find("LookLeftPoint");
            lookRightPoint = cameraPositions.transform.Find("LookRightPoint");

            freeLookCamera.Follow = this.transform;
            freeLookCamera.LookAt = dynamicLookAtTarget;

            freeLookCamera.m_XAxis.Value = 0f;
            freeLookWaitTime = 1.0f;
            freeLookCamera.enabled = true;
            localPlayer = true;

            cameraFollowDamping = Setting_Data.CameraFollowDamping;
            cameraFollowDamping = 0f;

            for (int i = 0; i < 3; i++)
            {
                var transposer = freeLookCamera.GetRig(i).GetCinemachineComponent<CinemachineTransposer>();
                if (transposer != null)
                {
                    // Body Damping 값을 설정하여 카메라의 반응성을 조절합니다.
                    transposer.m_XDamping = cameraFollowDamping;
                    transposer.m_YDamping = cameraFollowDamping;
                    transposer.m_ZDamping = cameraFollowDamping;
                }
            }

            StartCoroutine(UIUpdating());
        }
        SetWheels(wheelsModels[0], transform.Find("Wheel_FrontLeft").GetComponent<WheelCollider>(), transform.Find("TrailFrontLeft").GetComponent<TrailRenderer>(), true);
        SetWheels(wheelsModels[1], transform.Find("Wheel_FrontRight").GetComponent<WheelCollider>(), transform.Find("TrailFrontRight").GetComponent<TrailRenderer>(), true);
        SetWheels(wheelsModels[2], transform.Find("Wheel_RearLeft").GetComponent<WheelCollider>(), transform.Find("TrailRearLeft").GetComponent<TrailRenderer>(), false);
        SetWheels(wheelsModels[3], transform.Find("Wheel_RearRight").GetComponent<WheelCollider>(), transform.Find("TrailRearRight").GetComponent<TrailRenderer>(), false);
        _data = gameObject.GetComponent<Curve_data>();
        driver = transform.GetComponentInChildren<Driver>();
        SetEngineCurves(_data.horsePower, _data.torque);
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
        ForceLightOn();// 헤드라이트 스위치
        ForcePlayEngineSound();// 엔진 사운드 강제 재생
        SetBaseEngineAcceleration(5f);// 기본 엔진 가속도 설정
        SetAutoGear(true);// 자동 기어 설정
        SetAntiRoll(2.0f);// 안티롤 설정
        SetShiftTiming(0.5f);// 기어 변속 타이밍 설정
        SetBrakePower(3000f);// 브레이크 파워 설정
        //SetDriveAxel(eCAR_DRIVEAXEL.eRWD);// 구동축 설정
        SetDriveWheels();
        SetFriction();// 마찰력 설정
        SpawnSmoke();// 스폰 연기 설정
        CalculateOptimalShiftPoints();// 최적 기어 변속 포인트를 계산합니다.
        SetDriftMode(driftCar);// 드리프트 모드 설정

        StartCoroutine(Engine());// 엔진 코루틴 시작
        StartCoroutine(UpdateNitro());// 부스트 코루틴 시작
        ForceChangeGear(eGEAR.eGEAR_FIRST);// 첫 번째 기어로 강제 변경
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

        //ChangeMode(sideBraking);//드리프트 모드 진입

        UpdatingWheels();
        AntiRollBar();
        SetSlpingAngle();// 슬립 각도를 설정합니다.
        UpdatingFriction();// 마찰력을 업데이트합니다.
        Braking();// 브레이크를 적용합니다.
        ApplyAerodynamicDrag();// 공기 저항력을 적용합니다.
        EffectDrift();// 드리프트 효과를 적용합니다.
        ChangeStatForAnimation();//운전자의 에니메이션 상태를 갱신 합니다.
    }

    private void LateUpdate()
    {
        if (!localPlayer) return;

        if(Input.GetAxis("Vertical2") < 0)
        {
            freeLookCamera.enabled = false;
            mainCamera.transform.position = lookBack.position;
            mainCamera.transform.rotation = lookBack.rotation;
            return;
        }
        else if (firstPersonCameraCheck)
        {
            freeLookCamera.enabled = false;
            mainCamera.transform.position = firstPersonCamera.position;
            mainCamera.transform.rotation = firstPersonCamera.rotation;
        }
        else if (Input.GetAxis("Horizontal2") > 0)
        {
            freeLookCamera.enabled = false;
            mainCamera.transform.position = lookRightPoint.position;
            mainCamera.transform.rotation = lookRightPoint.rotation;
            return;
        }
        else if (Input.GetAxis("Horizontal2") < 0)
        {
            freeLookCamera.enabled = false;
            mainCamera.transform.position = lookRightPoint.position;
            mainCamera.transform.rotation = lookRightPoint.rotation;
            return;
        }

        if (!freeLookCamera.enabled)
        {
            freeLookCamera.enabled = true;
        }

        if (GetSpeed() < cameraLockSpeedThreshold)
        {
            // 정지 상태: 자유롭게 둘러보기 (Recenter 비활성화)
            freeLookCamera.m_XAxis.m_MaxSpeed = 300f; // 마우스 감도
            freeLookCamera.m_YAxis.m_MaxSpeed = 2f;
            freeLookCamera.m_XAxis.m_Recentering.m_enabled = false;
        }
        else
        {
            // 주행 상태: 카메라를 차량 뒤에 고정 (Recenter 강제 활성화)
            freeLookCamera.m_XAxis.m_MaxSpeed = 0f; // 마우스로 좌우 회전 방지
            freeLookCamera.m_XAxis.m_Recentering.m_enabled = true;
            freeLookCamera.m_XAxis.m_Recentering.m_WaitTime = 0f;
            freeLookCamera.m_XAxis.m_Recentering.m_RecenteringTime = 0.1f; // 매우 빠르게 복귀
        }

        Vector3 lookDirection;
        if (carRB.velocity.magnitude < 0.1f)
        {
            lookDirection = transform.forward;
        }
        else
        {
            // 차량의 앞 방향과 실제 운동 방향을 Slerp로 부드럽게 보간
            Vector3 forwardDir = transform.forward;
            Vector3 velocityDir = carRB.velocity.normalized;
            lookDirection = Vector3.Slerp(forwardDir, velocityDir, cameraLookAtBlend);
        }

        dynamicLookAtTarget.position = transform.position + lookDirection * 20f; // 20f는 임의의 거리

        if (firstPersonCameraCheck)
            freeLookCamera.m_Lens.FieldOfView = fov * 2f;
        else
            freeLookCamera.m_Lens.FieldOfView =
                GetIsNitroActive() ?
                Mathf.Lerp(freeLookCamera.m_Lens.FieldOfView, fov * 3.5f, Time.deltaTime) :
                Mathf.Lerp(freeLookCamera.m_Lens.FieldOfView, fov * 2f, Time.deltaTime);
    }
    protected override void GetInputData()
    {
        if (GetInput(out NetworkInputManager data))
        {
            //data.direction.Normalize();
            throttle = data.direction.y;
            Steering(data.direction.x);
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
            ActivateNitro(data.boosting);
            clutching = data.direction.z;
            forceGear = data.forceGear;
            gearUp = data.gearUP;
            gearDown = data.gearDOWN;
        }
    }

    public void ResetTimer()
    { gameTimer = 0f; }

    public void ChangeMode(bool _driftMode) { ChangeFriction(_driftMode); }
    public void SetDriftMode(bool _driftMode) { ChangeFriction(_driftMode); }

    public void SetCamera(Camera _camera)
    {
        mainCamera = _camera;
        radialBlur = mainCamera.gameObject.GetComponent<RadialBlur>();
    }
    public void SetNitroBar(Slider _nitroBar) { nitroBar = _nitroBar; }
    public void SetRPMGauge(RPMGauge _rpmGauge)
    {
        rpmGauge = _rpmGauge;
        speedTextForUI = rpmGauge.transform.Find("Speed").GetComponent<TextMeshProUGUI>();
        gearTextForUI = rpmGauge.transform.Find("GearNum").GetComponent<TextMeshProUGUI>();
    }

    IEnumerator CameraUpdate()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(Shared.frame60);
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

    public int GetCarNumber() { return carNumber; }

    private void SetRadialBlur()
    {
        if (radialBlur != null)
        {
            if (isNitroActive)
            {
                radialBlur.enabled = true;
                radialBlur.blurStrength = Mathf.Lerp(0f, 1f, GetSpeed() / 200f);
                radialBlur.blurWidth = Mathf.Lerp(0f, 1f, GetSpeed() / 200f) + GetNitroBlurWidth();
            }
            else
                radialBlur.enabled = false;
        }
    }
    public void SetCheckPoint(int _checkPoint) { currentCheckpointIndex = _checkPoint; }
    public int GetCheckPoint() { return currentCheckpointIndex; }

    public void SetNextCheckPointPosition(CheckPoint _nextCheckPoint)
    {
        nextCheckPoint = _nextCheckPoint.transform;
        if (!calculateDistance)
        {
            calculateDistance = true;
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

    private void ChangeStatForAnimation()
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
