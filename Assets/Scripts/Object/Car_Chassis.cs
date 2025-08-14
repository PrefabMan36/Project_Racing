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
        public eAXEL axel;
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
    public float counterSteerMultiplier = 2.5f; // 카운터 스티어시 마찰 증가 배수
    public float counterSteerSumer = 0.0f;
    public float frictionChangeSpeed = 5f;
    private bool isCounterSteering = false;
    #endregion

    #region Value Wheels
    [Header("Fake Wheels")]
    [SerializeField] private List<MeshRenderer> wheelTransform;
    [SerializeField] private Quaternion tempWheelRotation;
    [SerializeField] private Vector3 tempWheelPosition;
    [SerializeField] private float wheelRadius;

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
    [SerializeField] private bool isDrift;
    [SerializeField] private float forwardTireGrip_FG = 3.0f;
    [SerializeField] private float forwardTireGrip_RG = 3.0f;
    [SerializeField] private float sidewaysTireGrip_FG = 3.5f;
    [SerializeField] private float sidewaysTireGrip_RG = 3.5f;

    [SerializeField] private float forwardTireGrip_FD = 2.5f;
    [SerializeField] private float forwardTireGrip_RD = 2.5f;
    [SerializeField] private float sidewaysTireGrip_FD = 2.0f;
    [SerializeField] private float sidewaysTireGrip_RD = 2.0f;

    [SerializeField] private float forwardTireGrip;
    [SerializeField] private float sidewaysTireGrip;

    [SerializeField] private float forwardValue = 1f;
    [SerializeField] private float sideValue = 1f;
    [SerializeField] private WheelFrictionCurve forwardFriction, sidewaysFriction;
    [SerializeField] private JointSpring suspension;
    [SerializeField] private float[] forwardSlip;
    [SerializeField] private float[] sidewaysSlip;
    [SerializeField] private float[] overallSlip;
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
    [Range(0.0f, 0.5f), SerializeField] private float slipLimit = 0.3f;
    [SerializeField] private AudioSource slipSound;
    [SerializeField] private float currentMaxSlip;
    [SerializeField] private float sumSlip;
    [SerializeField] private float finalMaxSlip;
    [SerializeField] private float slipVolume;
    [SerializeField] private GameObject smokePrefab;
    [SerializeField] private GameObject[] smokes;
    [SerializeField] private ParticleSystem[] smokeParticles;
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
        wheelRadius = wheels[0].wheelCollider.radius;
        differentialPower = new float[driveWheelsNum];
    }
    #endregion

    #region Value Brake
    [Header("Value Brake")]
    [SerializeField] private float brakePower;
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
    protected void ChangeFriction(bool _mode)
    {
        isDrift = _mode;
        for (int i = 0; i < wheelNum; i++)
        {
            forwardFriction = wheels[i].wheelCollider.forwardFriction;
            sidewaysFriction = wheels[i].wheelCollider.sidewaysFriction;
            suspension = wheels[i].wheelCollider.suspensionSpring;

            /*if(_mode)//Drift mode
            {
                forwardFriction.extremumSlip = 0.7f;
                forwardFriction.extremumValue = 1.8f;
                forwardFriction.asymptoteSlip = 1.2f;
                forwardFriction.asymptoteValue = 1.0f;
                sidewaysFriction.extremumSlip = 1.0f;
                sidewaysFriction.extremumValue = 2.2f;
                sidewaysFriction.asymptoteSlip = 1.5f;
                sidewaysFriction.asymptoteValue = 1.2f;
            }
            else
            {
                forwardFriction.extremumSlip = 0.4f;
                forwardFriction.extremumValue = 2.0f;
                forwardFriction.asymptoteSlip = 0.8f;
                forwardFriction.asymptoteValue = 1.5f;
                sidewaysFriction.extremumSlip = 0.2f;
                sidewaysFriction.extremumValue = 2.5f;
                sidewaysFriction.asymptoteSlip = 0.7f;
                sidewaysFriction.asymptoteValue = 1.8f;
            }*/

            if (_mode)
            {
                if (wheels[i].axel == eAXEL.eAXEL_FRONT)
                {
                    forwardFriction.extremumSlip = 0.4f;
                    forwardFriction.extremumValue = 1.0f;
                    forwardFriction.asymptoteSlip = 0.8f;
                    forwardFriction.asymptoteValue = 0.5f;

                    sidewaysFriction.extremumSlip = 0.2f;
                    sidewaysFriction.extremumValue = 1.0f;
                    sidewaysFriction.asymptoteSlip = 0.5f;
                    sidewaysFriction.asymptoteValue = 0.8f;

                    suspension.spring = 30000f;
                    suspension.damper = 2500f;
                }
                else
                {
                    forwardFriction.extremumSlip = 0.4f;
                    forwardFriction.extremumValue = 1.0f;
                    forwardFriction.asymptoteSlip = 0.8f;
                    forwardFriction.asymptoteValue = 0.5f;

                    sidewaysFriction.extremumSlip = 0.3f;
                    sidewaysFriction.extremumValue = 1.0f;
                    sidewaysFriction.asymptoteSlip = 0.6f;
                    sidewaysFriction.asymptoteValue = 0.85f;

                    suspension.spring = 22000f;
                    suspension.damper = 1800f;
                }
            }
            else
            {
                forwardFriction.extremumSlip = 0.4f;
                forwardFriction.extremumValue = 1.0f;
                forwardFriction.asymptoteSlip = 0.6f;
                forwardFriction.asymptoteValue = 0.8f;

                sidewaysFriction.extremumSlip = 0.4f;
                sidewaysFriction.extremumValue = 1.0f;
                sidewaysFriction.asymptoteSlip = 0.8f;
                sidewaysFriction.asymptoteValue = 1.9f;

                if (wheels[i].axel == eAXEL.eAXEL_FRONT)
                {
                    suspension.spring = 45000f;
                    suspension.damper = 5000f;
                }
                else
                {
                    suspension.spring = 48000f;
                    suspension.damper = 5500f;
                }
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

        // 차량이 오른쪽으로 회전하는데 왼쪽으로 스티어링하는 경우
        if (angularVelocityY > counterSteerThreshold && steerInput < -counterSteerThreshold)
        {
            isCounterSteering = true;
        }
        // 차량이 왼쪽으로 회전하는데 오른쪽으로 스티어링하는 경우
        else if (angularVelocityY < -counterSteerThreshold && steerInput > counterSteerThreshold)
        {
            isCounterSteering = true;
        }
        // 측면 미끄러짐을 이용한 추가 감지
        else if (Mathf.Abs(sidewaysVelocity) > 2f && Mathf.Sign(sidewaysVelocity) != Mathf.Sign(steerInput) && Mathf.Abs(steerInput) > 0.3f)
        {
            isCounterSteering = true;
        }
        else
        {
            isCounterSteering = false;
        }
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
        for (int i = 0; i < wheelNum; i++)
        {
            if (wheels[i].wheelCollider.GetGroundHit(out wheelHit))
            {
                //overallSlip[i] = Mathf.Abs(wheelHit.forwardSlip + wheelHit.sidewaysSlip);

                if(isDrift)
                {
                    if (wheels[i].axel == eAXEL.eAXEL_FRONT)
                    {
                        forwardTireGrip = forwardTireGrip_FD;
                        sidewaysTireGrip = sidewaysTireGrip_FD;
                    }
                    else
                    {
                        forwardTireGrip = forwardTireGrip_RD;
                        sidewaysTireGrip = sidewaysTireGrip_RD;
                    }
                }
                else
                {
                    if (wheels[i].axel == eAXEL.eAXEL_FRONT)
                    {
                        forwardTireGrip = forwardTireGrip_FG;
                        sidewaysTireGrip = sidewaysTireGrip_FG;
                    }
                    else
                    {
                        forwardTireGrip = forwardTireGrip_RG;
                        sidewaysTireGrip = sidewaysTireGrip_RG;
                    }
                }

                    forwardFriction = wheels[i].wheelCollider.forwardFriction;
                forwardFriction.stiffness = forwardTireGrip - overallSlip[i] / forwardValue;
                wheels[i].wheelCollider.forwardFriction = forwardFriction;

                sidewaysFriction = wheels[i].wheelCollider.sidewaysFriction;
                sidewaysFriction.stiffness = sidewaysTireGrip - overallSlip[i] / sideValue;
                wheels[i].wheelCollider.sidewaysFriction = sidewaysFriction;

                forwardSlip[i] = wheelHit.forwardSlip;
                sidewaysSlip[i] = wheelHit.sidewaysSlip;

                overallSlip[i] = Mathf.Sqrt(forwardSlip[i] * forwardSlip[i] + sidewaysSlip[i] * sidewaysSlip[i]);
            }
        }
    }
    #endregion

    #region Funtion Wheels Controll
    protected void Steering(float _input)
    {
        currentInput = Mathf.Abs(_input);
        steerInput = _input;
        curSteerAngle = Mathf.Lerp(0, steeringCurve.Evaluate(speed) * _input, currentInput);//steeringCurve.Evaluate(speed);
        for (int i = 0; i < steerWheelsNum; i++)
            steerWheels[i].steerAngle = curSteerAngle;
        if (steeringHandle != null)
            steeringHandle.localRotation = Quaternion.Euler(0, 0, curSteerAngle * 16f);
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
        
        TailLampSwitch(isBrakingIntent);

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
            wheels[i].wheelCollider.brakeTorque = finalBrakeTorque;
        }
    }
    protected void SideBrakingUp()
    {
        for (int i = 0; i < driveWheelsNum; i++)
        {
            if (wheels[i].axel == eAXEL.eAXEL_BACK)
                wheels[i].wheelCollider.brakeTorque = 0f;
        }
    }
    protected void SideBrakingDown()
    {
        for (int i = 0; i < wheelNum; i++)
        {
            if (wheels[i].axel == eAXEL.eAXEL_BACK)
                wheels[i].wheelCollider.brakeTorque = Mathf.Infinity;
        }
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
    protected void EffectDrift()
    {
        finalMaxSlip = 0f;
        sumSlip = 0f;
        for(int i = 0; i < wheelNum; i++)
        {
            if (wheels[i].wheelCollider.GetGroundHit(out wheelHit))
            {
                if (Mathf.Abs(wheelHit.sidewaysSlip) > 0.15f || Mathf.Abs(wheelHit.forwardSlip) > 0.3f)
                {
                    wheels[i].skidMarks.emitting = true;

                    var emission = smokeParticles[i].emission;
                    emission.enabled = true;

                    currentMaxSlip = MathF.Max(wheelHit.sidewaysSlip, wheelHit.forwardSlip);
                    if(finalMaxSlip < currentMaxSlip)
                    {
                        finalMaxSlip = currentMaxSlip;
                        sumSlip = wheelHit.sidewaysSlip + wheelHit.forwardSlip;
                    }
                }
                else
                {
                    wheels[i].skidMarks.emitting = false;

                    var emission = smokeParticles[i].emission;
                    emission.enabled = false;
                }
            }
            else
            {
                wheels[i].skidMarks.emitting = false;

                var emission = smokeParticles[i].emission;
                emission.enabled = false;
            }
        }
        if(finalMaxSlip > 0.15f)
        {
            if (slipSound != null && !slipSound.isPlaying)
                slipSound.Play();

            slipSound.volume = Mathf.Clamp01(finalMaxSlip * 2);
            slipSound.pitch = Mathf.Clamp(finalMaxSlip, 0.75f, 1f);
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
            }
        }
    }
    #endregion
}