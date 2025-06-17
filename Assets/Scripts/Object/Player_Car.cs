using System.Collections;
using Cinemachine;
using Tiny;
using UnityEngine;
using UnityEngine.UI;
using Fusion;
using TMPro;

//�� Ŭ������ �÷��̾� ������ ��Ÿ����, ������ ������ Ư���� ī�޶�, UI ���� �����մϴ�.
//Car Ŭ������ ��ӹ޾� ������ ������ �����մϴ�.
//�� Ŭ������ Fusion ��Ʈ��ŷ�� ����Ͽ� ��Ƽ�÷��̾� ȯ�濡�� ������ ���¸� ����ȭ�մϴ�.
//�� Ŭ������ ��ӹ��� ������ �Է��� ó���ϰ�, ī�޶� �����ϸ�, ������ ������ Ư���� �ʱ�ȭ�մϴ�.
//�� Ŭ������ ��ӹ��� ������ ���, ����, �극��ũ ���� �����մϴ�.
//�� Ŭ������ ������ ī�޶�� UI�� ������Ʈ�ϴ� �ڷ�ƾ�� �����մϴ�.

public class Player_Car : Car
{
    [SerializeField] private int ID;
    [SerializeField] private NetworkId playerId;
    [Networked] public NetworkString<_16> playerName { get; set; }
    [SerializeField] private bool nameChanged = false;

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
    [Networked, SerializeField] private float distanceToCheckPoint { get; set; }
    [SerializeField]private Transform nextCheckPoint;
    [SerializeField] private float gameTimer = 0;
    [SerializeField] private bool raceStarted = true;

    private bool firstPersonCameraCheck;

    private bool freeLook;
    private float freeLookWaitTime;
    private float fov = 30f;

    public bool braking, sideBraking, up, down, left, right;
    private bool gearUp, gearDown;
    private byte forceGear;
    private float clutching;

    [Networked] public NetworkInputManager inputData { get; set; }
    private bool localPlayer = false;

    //������ �ڵ����� ȣ��Ǵ� �Լ�
    //��ӹ��� Ŭ������ Spawned()�� �������̵��Ͽ� ����մϴ�.
    //�� �Լ��� ��Ʈ��ũ���� ������Ʈ�� ������ �� ȣ��˴ϴ�.
    //�� �Լ����� ī�޶�� UI�� �ʱ�ȭ�ϰ�, ������ ������ Ư���� �����մϴ�.
    //�÷��̾�� NPC�� ������ ������ �ٸ��� �ϱ� ���� override�մϴ�.
    public override void Spawned()
    {
        if(playerName.Value != "")
        { gameObject.name = playerName.Value; }
        Runner.SetIsSimulated(Object, true);// �ùķ��̼��� Ȱ��ȭ�մϴ�.
        gameManager = FindAnyObjectByType<MainGame_Manager>();// ���� �Ŵ����� ã���ϴ�.
        gameManager.CarInit(this, HasInputAuthority);// ���� �Ŵ����� �̿��� ������ �ʱ�ȭ�մϴ�.
        networkObject = GetComponent<NetworkObject>();// ��Ʈ��ũ ������Ʈ�� �����ɴϴ�.
        playerId = networkObject.Id;// �÷��̾� ID�� �����մϴ�.
        rankData.playerId = networkObject.Id;// ��ũ �����Ϳ� �÷��̾� ID�� �����մϴ�.
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
            localPlayer = true;
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
        HeadLightSwitch();// ������Ʈ ����ġ
        ForcePlayEngineSound();// ���� ���� ���� ���
        SetBaseEngineAcceleration(5f);// �⺻ ���� ���ӵ� ����
        SetAutoGear(true);// �ڵ� ��� ����
        SetAntiRoll(35000f);// ��Ƽ�� ����
        SetShiftTiming(0.5f);// ��� ���� Ÿ�̹� ����
        SetBrakePower(3000f);// �극��ũ �Ŀ� ����
        SetDriveAxel(eCAR_DRIVEAXEL.eRWD);// ������ ����
        SetFriction();// ������ ����
        SpawnSmoke();// ���� ���� ����
        CalculateOptimalShiftPoints();// ���� ��� ���� ����Ʈ�� ����մϴ�.
        StartCoroutine(Engine());// ���� �ڷ�ƾ ����
        StartCoroutine(UpdateNitro());// �ν�Ʈ �ڷ�ƾ ����
        if(!nameChanged)
        {
            NameChanged();
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
        if (Input.GetKeyDown(KeyCode.F)) { HeadLightSwitch(); }
        UpdatingWheels();
        if (Input.GetKeyDown(KeyCode.V))
            firstPerson();
        SetRadialBlur();
        if (localPlayer && raceStarted)
        {
            gameTimer += Time.deltaTime;
            gameManager.SetTimer(gameTimer);
        }
    }

    //Braking()�� ȣ���ϱ� ���� �÷��̾� �극��ũ �Է��� ó���մϴ�.
    public override void FixedUpdateNetwork()
    {
        GetInputData();
        if (gearUp)
            ChangeGear(true);// ��� ��
        if (gearDown)
            ChangeGear(false);// ��� �ٿ�
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
        SetSlpingAngle();// ���� ������ �����մϴ�.
        UpdatingFriction();// �������� ������Ʈ�մϴ�.
        Braking();// �극��ũ�� �����մϴ�.
        ApplyAerodynamicDrag();// ���� ���׷��� �����մϴ�.
        EffectDrift();// �帮��Ʈ ȿ���� �����մϴ�.
    }
    // �÷��̾��� �Է��� ó���ϴ� �Լ��Դϴ�.
    // �� �Լ��� ��Ʈ��ũ �Է��� ������ ������ ����, ����, ��� ���� ���� ó���մϴ�.
    // �÷��̾ �ƴ� NPC�� �Է��� ���� �� �ֱ�� �Լ� �̱⿡ override�մϴ�.
    protected override void GetInputData()
    {
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
        }
    }

    public void ResetTimer()
    { gameTimer = 0f; }

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
        speedTextForUI = rpmGauge.transform.Find("Speed").GetComponent<TextMeshProUGUI>();
        gearTextForUI = rpmGauge.transform.Find("GearNum").GetComponent<TextMeshProUGUI>();
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

    public void SetName(string _name)
    {
        playerName = _name;
        NameChanged();
    }

    private void NameChanged()
    {
        gameObject.name = playerName.Value;
        gameManager.SetRank(playerId);
        nameChanged = true;
    }

    public string GetName() { return playerName.Value; }

    public Rank_Data GetRankData()
    {
        rankData.lap = lap;
        rankData.currentCheckpointIndex = currentCheckpointIndex;
        rankData.distanceToCheckPoint = distanceToCheckPoint;
        return rankData;
    }

    public bool GetLocalPlayer() { return localPlayer; }
}
