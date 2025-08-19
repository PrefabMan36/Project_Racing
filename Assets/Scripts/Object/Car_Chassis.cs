using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;

public partial class Car
{
    public enum eAXEL
    {
        eAXEL_FRONT,
        eAXEL_BACK
    }

    [Serializable]
    public struct Wheel
    {
        public GameObject wheelModel;
        public WheelCollider wheelCollider;
        public TrailRenderer skidMarks;
        public eTIRETYPE tireType;
        public eAXEL axel;
        public bool isLeft;
    }

    //-------------------------VALUE-----------------------------

    #region Value Steer
    [Header("Steer Value")]
    [SerializeField] protected float brakeInput;
    [SerializeField] private AnimationCurve steeringCurve;
    [SerializeField] private Transform steeringHandle;
    [SerializeField] private float maxSteerAngle = 30f;
    [Networked, SerializeField] private float curSteerAngle { get; set; } = 0f;
    [SerializeField] private float currentInput;
    [SerializeField] private float steerInput;
    [SerializeField] private float steerSpeed;
    [SerializeField] private float slipingAngle;
    [SerializeField] private float sideBrakeInput;

    [Header("Counter Steer Settings")]
    public float counterSteerThreshold = 0.3f; // 카운터 스티어 감지 임계값
    public float counterSteerMultiplier = 3.5f; // 카운터 스티어시 마찰 증가 배수
    public float currentCounterSteerMultiplier = 1.0f;
    public float frictionChangeSpeed = 8f;
    private bool isCounterSteering = false;
    #endregion

    #region Value Wheels
    [Header("Fake Wheels")]
    [SerializeField] private List<MeshRenderer> wheelTransform;
    [SerializeField] private Quaternion tempWheelRotation;
    [SerializeField] private Vector3 tempWheelPosition;

    [Header("Real Wheels")]
    [SerializeField] private WheelHit wheelHit; //휠정보
    [SerializeField] private List<Wheel> wheels;
    [SerializeField] private int wheelNum;
    [SerializeField] private List<WheelCollider> driveWheels = new List<WheelCollider>();
    [SerializeField] private List<WheelCollider> steerWheels = new List<WheelCollider>();
    [SerializeField] private int driveWheelsNum;
    [SerializeField] private int steerWheelsNum;
    [SerializeField] private eCAR_DRIVEAXEL driveAxel;
    [SerializeField] private float[] differentialPower;
    //[SerializeField] private float differentialPowerValue = 0f;
    #endregion

    #region Value Tire
    [Header("Tire Value")]
    [SerializeField] private float wheelMass;
    [SerializeField] private float wheelRadius;
    [SerializeField] private eTIRETYPE tireType;
    [SerializeField] private float suspensionDistanceFront;
    [SerializeField] private float suspensionDistanceRear;
    [SerializeField] private float forceAppPointDistanceFront;
    [SerializeField] private float forceAppPointDistanceRear;

    [SerializeField] private float forwardextremumSlipFront;
    [SerializeField] private float forwardextremumValueFront;
    [SerializeField] private float forwardsasymptoteSlipFront;
    [SerializeField] private float forwardsasymptoteValueFront;

    [SerializeField] private float sidewayextremumSlipFront;
    [SerializeField] private float sidewayextremumValueFront;
    [SerializeField] private float sidewaysasymptoteSlipFront;
    [SerializeField] private float sidewaysasymptoteValueFront;

    [SerializeField] private float forwardextremumSlipRear;
    [SerializeField] private float forwardextremumValueRear;
    [SerializeField] private float forwardsasymptoteSlipRaer;
    [SerializeField] private float forwardsasymptoteValueRaer;

    [SerializeField] private float sidewayextremumSlipRear;
    [SerializeField] private float sidewayextremumValueRear;
    [SerializeField] private float sidewaysasymptoteSlipRear;
    [SerializeField] private float sidewaysasymptoteValueRear;

    [SerializeField] private float forwardTireGripFront;
    [SerializeField] private float forwardTireGripRear;
    [SerializeField] private float sidewaysTireGripFront;
    [SerializeField] private float sidewaysTireGripRear;

    [SerializeField] private float suspensionSpringFront;
    [SerializeField] private float suspensionSpringRear;
    [SerializeField] private float suspensionDamperFront;
    [SerializeField] private float suspensionDamperRear;
    [SerializeField] private float suspensionPositionFront;
    [SerializeField] private float suspensionPositionRear;

    [SerializeField] private float baseForwardTireGrip;
    [SerializeField] private float baseSidewaysTireGrip;

    //[SerializeField] private float forwardValue = 1f;
    //[SerializeField] private float sideValue = 1f;
    [SerializeField] private WheelFrictionCurve forwardFriction, sidewaysFriction;
    [SerializeField] private JointSpring suspension;
    [SerializeField] private float[] forwardSlip;
    [SerializeField] private float[] sidewaysSlip;
    [SerializeField] private float[] overallSlip;
    #endregion

    #region Dynamic Friction Settings
    [Header("Dynamic Friction Settings")]
    [Tooltip("가속/감속 시 앞/뒤 타이어 그립 변화량")]
    [SerializeField] private float longitudinalLoadFactor;
    [Tooltip("코너링 시 좌/우 타이어 그립 변화량")]
    [SerializeField] private float lateralLoadFactor;
    [Tooltip("타이어가 미끄러질 때 전반적인 그립 감소량")]
    [SerializeField] private float slipGripReductionFactor;
    [Tooltip("각 바퀴에 가해지는 현재 하중 (디버깅용)")]
    [SerializeField] private float[] wheelLoads;
    [SerializeField] private float forwardFrictionMin;
    [SerializeField] private float sidewaysFrictionMin;
    private Vector3 lastVelocity;
    #endregion

    #region Value AntiRoll
    [SerializeField] private float antiRoll;
    [SerializeField] private WheelHit wheelHitForAntiRoll;
    [SerializeField] private float antiRollForce;
    [SerializeField] private float travelL;
    [SerializeField] private float travelR;
    [SerializeField] private bool groundedL;
    [SerializeField] private bool groundedR;
    #endregion

    #region Value Drift
    [Header("Drift Value")]
    [SerializeField] private AudioSource slipSound;
    [SerializeField] private float currentMaxSlip;
    [SerializeField] private float sumSlip;
    [SerializeField] private float finalMaxSlip;
    [SerializeField] private float slipVolume;
    [SerializeField] private GameObject smokePrefab;
    [SerializeField] private GameObject[] smokes;
    [SerializeField] private ParticleSystem[] smokeParticles;
    [Networked, Capacity(4), SerializeField] private NetworkArray<NetworkBool> isDrifting => default;
    [Networked, SerializeField] private float resultSlip { get; set; } = 0f;
    #endregion

    #region Function Steer Setting
    public void SetSteeringCurve(AnimationCurve _steeringCurve) { steeringCurve  = _steeringCurve; }
    protected void SetSteerWheelsCount(int _steerWheelsCount) { steerWheelsNum = _steerWheelsCount; }
    #endregion

    #region Fuction Wheels Setting
    protected void SetWheels(GameObject _wheelModel, WheelCollider _wheelCollider, TrailRenderer _skidMarks, bool front)
    {
        if(wheels == null)
            wheels = new List<Wheel>();

        Wheel wheel = new Wheel();
        wheel.wheelModel = _wheelModel;
        wheel.wheelCollider = _wheelCollider;
        wheel.skidMarks = _skidMarks;
        wheel.axel = front ? eAXEL.eAXEL_FRONT : eAXEL.eAXEL_BACK;
        wheels.Add(wheel);
    }
    protected void SetDriveWheels()
    {
        wheelNum = wheels.Count;
        wheelLoads = new float[wheelNum];
        SetSteerWheelsCount(2);
        switch (driveAxel)
        {
            case eCAR_DRIVEAXEL.eFWD:
            case eCAR_DRIVEAXEL.eRWD:
                driveWheelsNum = 2;
                break;
            case eCAR_DRIVEAXEL.e4WD:
                driveWheelsNum = 4;
                break;
        }
        for (int i = 0; i < wheelNum; i++)
        {
            if (wheels[i].axel == eAXEL.eAXEL_FRONT)
                steerWheels.Add(wheels[i].wheelCollider);
            if (wheels[i].wheelModel.transform.localPosition.x < 0)
            {
                var wheel = wheels[i];
                wheel.isLeft = true;
                wheels[i] = wheel;
            }
            switch (driveAxel)
            {
                case eCAR_DRIVEAXEL.eFWD:
                    if (wheels[i].axel == eAXEL.eAXEL_FRONT)
                        driveWheels.Add(wheels[i].wheelCollider);
                    break;
                case eCAR_DRIVEAXEL.eRWD:
                    if (wheels[i].axel == eAXEL.eAXEL_BACK)
                        driveWheels.Add(wheels[i].wheelCollider);
                    break;
                case eCAR_DRIVEAXEL.e4WD:
                    driveWheels.Add(wheels[i].wheelCollider);
                    break;
            }
        }
        smokes = new GameObject[wheelNum];
        smokeParticles = new ParticleSystem[wheelNum];
        differentialPower = new float[driveWheelsNum];
    }
    #endregion

    #region Value Brake
    [Header("Value Brake")]
    [SerializeField] private float brakePower;
    [SerializeField] private float brakeBias;
    [SerializeField] private float requestedBrakeTorque;
    [SerializeField] private float sideBrakePower;
    [SerializeField] private float targetBrakeTorque;
    [SerializeField] private float appliedBrakeTorque;
    [SerializeField] private float slipFactorABS;
    [SerializeField] private WheelCollider tempWheelColliderForBrake;
    [SerializeField] protected bool isABSEnabled = true; // ABS 사용 여부
    [SerializeField] private bool isBrakingIntent = false; // 브레이크 의도 여부
    [SerializeField] private bool forceBrake = false; // 강제 브레이크 여부
    [SerializeField, Range(0.1f, 1.0f)] private float absSlipThreshold = 0.35f; // ABS 개입을 시작할 Forward Slip 임계값 (음수)
    [SerializeField, Range(0.1f, 1.0f)] private float absBrakeReleaseFactor = 0.3f; // ABS 개입 강도 (1이면 슬립 시 브레이크 0, 낮을수록 약하게 개입)
    #endregion

    //-------------------------FUNCTION-----------------------------

    #region Fuction Wheels
    public void SetWheelMesh(Material _wheelMesh)
    {
        for (int i = 0; i < wheels.Count; i++)
        {
            MeshRenderer curWheel = wheels[i].wheelModel.GetComponent<MeshRenderer>();
            curWheel.material = _wheelMesh;
        }
    }
    protected void SetFriction()
    {
        forwardSlip = new float[wheelNum];
        sidewaysSlip = new float[wheelNum];
        overallSlip = new float[wheelNum];
    }
    protected void SetWheelsData(CarWheelsData wheelsData)
    {
        wheelMass = wheelsData.wheelMass;
        wheelRadius = wheelsData.wheelRadius;

        brakePower = wheelsData.brakePower;
        brakeBias = wheelsData.brakeBias;

        forwardFrictionMin = wheelsData.forwardFrictionMin;
        sidewaysFrictionMin = wheelsData.sidewaysFrictionMin;

        longitudinalLoadFactor = wheelsData.longitudinalLoadFactor;
        lateralLoadFactor = wheelsData.lateralLoadFactor;
        slipGripReductionFactor = wheelsData.slipGripReductionFactor;

        if (System.Enum.TryParse(wheelsData.Type, out eTIRETYPE tireTypeEnum))
            tireType = tireTypeEnum;
        else
        {
            Debug.LogError($"Invalid tire type: {wheelsData.Type}. Using default value.");
            tireType = eTIRETYPE.eTIRETYPE_BALANCE; // 기본값 설정
        }

        suspensionDistanceFront = wheelsData.suspensionDistanceFront;
        suspensionDistanceRear = wheelsData.suspensionDistanceRear;

        forceAppPointDistanceFront = wheelsData.forceAppPointDistanceFront;
        forceAppPointDistanceRear = wheelsData.forceAppPointDistanceRear;

        forwardextremumSlipFront = wheelsData.forwardextremumSlipFront;
        forwardextremumValueFront = wheelsData.forwardextremumValueFront;
        forwardsasymptoteSlipFront = wheelsData.forwardsasymptoteSlipFront;
        forwardsasymptoteValueFront = wheelsData.forwardsasymptoteValueFront;

        sidewayextremumSlipFront = wheelsData.sidewayextremumSlipFront;
        sidewayextremumValueFront = wheelsData.sidewayextremumValueFront;
        sidewaysasymptoteSlipFront = wheelsData.sidewaysasymptoteSlipFront;
        sidewaysasymptoteValueFront = wheelsData.sidewaysasymptoteValueFront;

        forwardextremumSlipRear = wheelsData.forwardextremumSlipRear;
        forwardextremumValueRear = wheelsData.forwardextremumValueRear;
        forwardsasymptoteSlipRaer = wheelsData.forwardsasymptoteSlipRear;
        forwardsasymptoteValueRaer = wheelsData.forwardsasymptoteValueRear;

        sidewayextremumSlipRear = wheelsData.sidewayextremumSlipRear;
        sidewayextremumValueRear = wheelsData.sidewayextremumValueRear;
        sidewaysasymptoteSlipRear = wheelsData.sidewaysasymptoteSlipRear;
        sidewaysasymptoteValueRear = wheelsData.sidewaysasymptoteValueRear;

        forwardTireGripFront = wheelsData.forwardTireGripFront;
        forwardTireGripRear = wheelsData.forwardTireGripRear;
        sidewaysTireGripFront = wheelsData.sidewaysTireGripFront;
        sidewaysTireGripRear = wheelsData.sidewaysTireGripRear;

        suspensionSpringFront = wheelsData.suspensionSpringFront;
        suspensionSpringRear = wheelsData.suspensionSpringRear;
        suspensionDamperFront = wheelsData.suspensionDamperFront;
        suspensionDamperRear = wheelsData.suspensionDamperRear;
        suspensionPositionFront = wheelsData.suspensionPositionFront;
        suspensionPositionRear = wheelsData.suspensionPositionRear;

        ChangeWheelsData();
    }
    private void ChangeWheelsData()
    {
        for (int i = 0; i < wheelNum; i++)
        {
            forwardFriction = wheels[i].wheelCollider.forwardFriction;
            sidewaysFriction = wheels[i].wheelCollider.sidewaysFriction;
            suspension = wheels[i].wheelCollider.suspensionSpring;

            wheels[i].wheelCollider.mass = wheelMass;
            wheels[i].wheelCollider.radius = wheelRadius;

            if (wheels[i].axel == eAXEL.eAXEL_FRONT)
            {
                wheels[i].wheelCollider.suspensionDistance = suspensionDistanceFront;
                wheels[i].wheelCollider.forceAppPointDistance = forceAppPointDistanceFront;

                forwardFriction.extremumSlip = forwardextremumSlipFront;
                forwardFriction.extremumValue = forwardextremumValueFront;
                forwardFriction.asymptoteSlip = forwardsasymptoteSlipFront;
                forwardFriction.asymptoteValue = forwardsasymptoteValueFront;

                sidewaysFriction.extremumSlip = sidewayextremumSlipFront;
                sidewaysFriction.extremumValue = sidewayextremumValueFront;
                sidewaysFriction.asymptoteSlip = sidewaysasymptoteSlipFront;
                sidewaysFriction.asymptoteValue = sidewaysasymptoteValueFront;

                suspension.spring = suspensionSpringFront;
                suspension.damper = suspensionDamperFront;
                suspension.targetPosition = suspensionPositionFront;
            }
            else
            {
                wheels[i].wheelCollider.suspensionDistance = suspensionDistanceRear;
                wheels[i].wheelCollider.forceAppPointDistance = forceAppPointDistanceRear;

                forwardFriction.extremumSlip = forwardextremumSlipRear;
                forwardFriction.extremumValue = forwardextremumValueRear;
                forwardFriction.asymptoteSlip = forwardsasymptoteSlipRaer;
                forwardFriction.asymptoteValue = forwardsasymptoteValueRaer;

                sidewaysFriction.extremumSlip = sidewayextremumSlipRear;
                sidewaysFriction.extremumValue = sidewayextremumValueRear;
                sidewaysFriction.asymptoteSlip = sidewaysasymptoteSlipRear;
                sidewaysFriction.asymptoteValue = sidewaysasymptoteValueRear;

                suspension.spring = suspensionSpringRear;
                suspension.damper = suspensionDamperRear;
                suspension.targetPosition = suspensionPositionRear;
            }

            wheels[i].wheelCollider.forwardFriction = forwardFriction;
            wheels[i].wheelCollider.sidewaysFriction = sidewaysFriction;
            wheels[i].wheelCollider.suspensionSpring = suspension;
        }
    }

    private void DetectCounterSteering()
    {
        // 차량의 각속도 (Y축 회전)
        float angularVelocityY = carRB.angularVelocity.y;

        // 차량의 측면 속도
        Vector3 localVelocity = transform.InverseTransformDirection(carRB.velocity);
        float sidewaysVelocity = localVelocity.x;

        // 카운터 스티어링 감지 로직
        bool wasCounterSteering = isCounterSteering;

        // 카운터 스티어링 감지 로직
        // 차량이 오른쪽으로 미끄러지는데 왼쪽으로 스티어링하는 경우 (혹은 그 반대)
        if ((angularVelocityY > counterSteerThreshold && steerInput < -counterSteerThreshold) ||
            (angularVelocityY < -counterSteerThreshold && steerInput > counterSteerThreshold) ||
            (Mathf.Abs(sidewaysVelocity) > 2f && Mathf.Sign(sidewaysVelocity) != Mathf.Sign(steerInput) && Mathf.Abs(steerInput) > 0.1f))
        {
            isCounterSteering = true;
        }
        else
        {
            isCounterSteering = false;
        }
        float targetMultiplier = isCounterSteering ? counterSteerMultiplier : 1.0f;

        // 현재 카운터 스티어 배수를 목표 배수까지 부드럽게 변경
        currentCounterSteerMultiplier = Mathf.Lerp(currentCounterSteerMultiplier, targetMultiplier, Time.fixedDeltaTime * frictionChangeSpeed);
    }

    public void SetDriveAxel(eCAR_DRIVEAXEL _driveAxel)
    {
        driveAxel = _driveAxel;
        SetDriveWheels();
    }
    protected void UpdatingWheels()
    {
        for (int i = 0; i < wheels.Count; i++)
        {
            wheels[i].wheelCollider.GetWorldPose(out tempWheelPosition, out tempWheelRotation);
            wheels[i].wheelModel.transform.position = tempWheelPosition;
            wheels[i].wheelModel.transform.rotation = tempWheelRotation;
            wheels[i].skidMarks.transform.position = tempWheelPosition - (Vector3.up * wheelRadius);
        }
    }
    protected void UpdatingFriction()
    {
        // 1. 차량의 하중 이동 계산
        // 차량의 지역 가속도 계산 (전후, 좌우)
        Vector3 localAcceleration = transform.InverseTransformDirection((carRB.velocity - lastVelocity) / Time.fixedDeltaTime);
        lastVelocity = carRB.velocity;

        // 전후 하중 이동 계산 (가속 시 뒤로, 감속 시 앞으로)
        float longitudinalLoad = localAcceleration.z * longitudinalLoadFactor;
        // 좌우 하중 이동 계산 (코너링 원심력)
        float lateralLoad = localAcceleration.x * lateralLoadFactor;

        // 2. 각 바퀴의 마찰력 업데이트
        for (int i = 0; i < wheelNum; i++)
        {
            if (wheels[i].wheelCollider.GetGroundHit(out wheelHit))
            {
                // 기본 마찰력 설정
                baseForwardTireGrip = wheels[i].axel == eAXEL.eAXEL_FRONT ? forwardTireGripFront : forwardTireGripRear;

                baseSidewaysTireGrip = wheels[i].axel == eAXEL.eAXEL_FRONT ? sidewaysTireGripFront : sidewaysTireGripRear;

                // 3. 각 바퀴에 가해지는 최종 하중 계산
                float currentWheelLoad = 1.425f; // 기본 하중은 1.0

                // 전/후 하중에 따른 변화 적용
                if (wheels[i].axel == eAXEL.eAXEL_FRONT)
                    currentWheelLoad -= longitudinalLoad; // 감속 시 앞바퀴 하중 증가
                else
                    currentWheelLoad += longitudinalLoad; // 가속 시 뒷바퀴 하중 증가

                // 좌/우 하중에 따른 변화 적용
                if (wheels[i].isLeft)
                    currentWheelLoad -= lateralLoad; // 우회전 시 왼쪽(안쪽) 바퀴 하중 감소
                else
                    currentWheelLoad += lateralLoad; // 우회전 시 오른쪽(바깥쪽) 바퀴 하중 증가

                wheelLoads[i] = Mathf.Max(0.1f, currentWheelLoad); // 하중이 0 이하로 떨어지지 않도록 함 (디버깅용)

                // 4. 미끄러짐(Slip)에 따른 마찰력 감소 계산
                overallSlip[i] = Mathf.Sqrt(Mathf.Pow(wheelHit.forwardSlip, 2) + Mathf.Pow(wheelHit.sidewaysSlip, 2));
                float slipReduction = overallSlip[i] * slipGripReductionFactor;

                // 5. 최종 마찰력(Stiffness) 적용
                // 기본 마찰력에 (하중 * 미끄러짐 감소)를 적용
                forwardFriction = wheels[i].wheelCollider.forwardFriction;
                forwardFriction.stiffness = baseForwardTireGrip * Mathf.Clamp(currentWheelLoad - slipReduction, forwardFrictionMin, 2.0f);
                wheels[i].wheelCollider.forwardFriction = forwardFriction;

                sidewaysFriction = wheels[i].wheelCollider.sidewaysFriction;
                sidewaysFriction.stiffness = baseSidewaysTireGrip * Mathf.Clamp(currentWheelLoad - slipReduction, sidewaysFrictionMin, 2.0f);
                wheels[i].wheelCollider.sidewaysFriction = sidewaysFriction;

                // 슬립 값 저장 (다른 로직에서 사용될 수 있음)
                forwardSlip[i] = wheelHit.forwardSlip;
                sidewaysSlip[i] = wheelHit.sidewaysSlip;
            }
        }
    }
    #endregion

    #region Funtion Wheels Controll
    private void Steering()
    {
        curSteerAngle = Mathf.Lerp(0, steeringCurve.Evaluate(speed) * steerInput, currentInput);
        for (int i = 0; i < steerWheelsNum; i++)
            steerWheels[i].steerAngle = curSteerAngle;
        if (steeringHandle != null)
            steeringHandle.localRotation = Quaternion.Euler(0, 0, curSteerAngle * 16f);
    }
    protected void SteeringInput(float _input)
    {
        currentInput = Mathf.Abs(_input);
        steerInput = _input;
    }

    protected void ForceStop()
    {
        if(waitingForRaceStart)
            forceBrake = true;
    }

    protected void Braking()
    {
        requestedBrakeTorque = brakeInput * brakePower;
        isBrakingIntent = brakeInput > 0.05f;

        for (int i = 0; i < wheelNum; i++)
        {
            float finalBrakeTorque = 0f;

            if (isBrakingIntent)
            {
                if (isABSEnabled && wheels[i].wheelCollider.isGrounded)
                {
                    if (overallSlip[i] > absSlipThreshold)
                    {
                        finalBrakeTorque = requestedBrakeTorque * absBrakeReleaseFactor;
                    }
                    else
                    {
                        finalBrakeTorque = requestedBrakeTorque;
                    }
                }
                else if (wheels[i].wheelCollider.isGrounded)
                {
                    finalBrakeTorque = requestedBrakeTorque;
                }
            }
            if (forceBrake) finalBrakeTorque = Mathf.Infinity;
            if (wheels[i].axel == eAXEL.eAXEL_BACK)
            {
                if (sideBrakeInput > 0f)
                {
                    finalBrakeTorque += sideBrakePower * sideBrakeInput;
                }
            }
            if(wheels[i].axel == eAXEL.eAXEL_FRONT)
            {
                finalBrakeTorque *= brakeBias;
            }
            else
            {
                finalBrakeTorque *= (1f - brakeBias);
            }
            wheels[i].wheelCollider.brakeTorque = finalBrakeTorque;
        }
        
    }
    protected void SideBrakingUp()
    {
        sideBrakeInput = 0f;
    }
    protected void SideBrakingDown()
    {
        sideBrakeInput = 1f;
    }
    public void SetBrakePower(float _brakePower) { brakePower = _brakePower; }
    #endregion

    #region Fuction Other
    public void SetAntiRoll(float _antiRoll) { antiRoll = _antiRoll; }
    protected void AntiRollBar()
    {
        for(int i = 0; i < wheelNum; i += 2)
        {
            WheelCollider wheelL = wheels[i].wheelCollider;
            WheelCollider wheelR = wheels[i + 1].wheelCollider;

            travelL = 1.0f;
            travelR = 1.0f;

            groundedL = wheelL.GetGroundHit(out wheelHitForAntiRoll);
            groundedR = wheelR.GetGroundHit(out wheelHitForAntiRoll);

            if (groundedL)
            {
                travelL = (-wheelL.transform.InverseTransformPoint(wheelHit.point).y - wheelL.radius) / wheelL.suspensionDistance;
            }
            if (groundedR)
            {
                travelR = (-wheelR.transform.InverseTransformPoint(wheelHit.point).y - wheelR.radius) / wheelR.suspensionDistance;
            }

            antiRollForce = (travelL - travelR) * antiRoll;

            //if (groundedL)
            //    carRB.AddForceAtPosition(wheelL.transform.up * -antiRollForce, steerWheels[0].transform.position);
            //if (groundedR)
            //    carRB.AddForceAtPosition(wheelR.transform.up * antiRollForce, steerWheels[1].transform.position);
        }

        
        
    }
    private bool IsGrounded()
    {
        for (int i = 0; i < wheelNum; i++)
        {
            if (wheels[i].wheelCollider.isGrounded)
                return true;
        }
        return false;
    }
    protected void CalculateDrift()
    {
        finalMaxSlip = 0f;
        sumSlip = 0f;
        for(int i = 0; i < wheelNum; i++)
        {
            if (wheels[i].wheelCollider.GetGroundHit(out wheelHit))
            {
                if (Mathf.Abs(wheelHit.sidewaysSlip) > 0.15f || Mathf.Abs(wheelHit.forwardSlip) > 0.15f)
                {
                    isDrifting.Set(i, true);

                    currentMaxSlip = MathF.Max(wheelHit.sidewaysSlip, wheelHit.forwardSlip);
                    if(finalMaxSlip < currentMaxSlip)
                    {
                        finalMaxSlip = currentMaxSlip;
                        sumSlip = wheelHit.sidewaysSlip + wheelHit.forwardSlip;
                    }
                }
                else
                {
                    isDrifting.Set(i, false);
                }
            }
            else
            {
                isDrifting.Set(i, false);
            }
        }
        resultSlip = finalMaxSlip;
    }

    protected void DriftEffect()
    {
        for (int i = 0; i < wheelNum; i++)
        {
            if (isDrifting.Get(i))
            {
                wheels[i].skidMarks.emitting = true;
                if (smokes[i] == null)
                {   
                    SpawnSmoke();
                }
                else
                {
                    var emission = smokeParticles[i].emission;
                    emission.enabled = true;
                }
            }
            else
            {
                wheels[i].skidMarks.emitting = false;
                if (smokes[i] != null)
                {
                    var emission = smokeParticles[i].emission;
                    emission.enabled = false;
                }
            }
        }
        if (resultSlip > 0.15f)
        {
            if (slipSound != null && !slipSound.isPlaying)
                slipSound.Play();

            slipSound.volume = Mathf.Clamp01(resultSlip * 2);
            slipSound.pitch = Mathf.Clamp(resultSlip, 0.75f, 1f);
        }
        else
        {
            if (slipSound != null && slipSound.isPlaying)
                slipSound.Stop();
        }
    }

    protected void SpawnSmoke()
    {
        for (int i = 0; i < wheelNum; i++)
        {
            if (smokes != null)
            {
                smokes[i] = Instantiate(smokePrefab);
                smokeParticles[i] = smokes[i].GetComponent<ParticleSystem>();
                smokeParticles[i].Play();
                smokes[i].transform.parent = wheels[i].skidMarks.transform;
                smokes[i].transform.position = wheels[i].skidMarks.transform.position;
                smokes[i].transform.rotation = Quaternion.identity;
                smokes[i].transform.localScale = Vector3.one;
                var emission = smokeParticles[i].emission;
                emission.enabled = false;
            }
        }
    }
    #endregion
}