using System;
using System.Collections;
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

    #region Value Steer
    [Header("Steer Value")]
    [SerializeField] protected float brakeInput;
    [SerializeField] private AnimationCurve steeringCurve;
    [SerializeField] private Transform steeringHandle;
    [SerializeField] private float maxSteerAngle = 30f;
    [SerializeField] private float curSteerAngle = 0f;
    [SerializeField] private float steerSpeed;
    [SerializeField] private float slipingAngle;
    [SerializeField] private float sideBrakeInput;
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
    [SerializeField] private float differentialPowerValue = 0f;
    #endregion

    #region Value Tire
    [Header("Tire Value")]
    [Range(0.8f, 1.3f), SerializeField] private float forwardTireGrip = 1.8f;
    [Range(0.8f, 1.3f), SerializeField] private float sidewaysTireGrip = 2.2f;
    [Range(1f, 2f), SerializeField] private float forwardValue = 1f;
    [Range(1f, 2f), SerializeField] private float sideValue = 2f;
    [SerializeField] private WheelFrictionCurve forwardFriction, sidewaysFriction;
    [SerializeField] private float[] forwardSlip;
    [SerializeField] private float[] sidewaysSlip;
    [SerializeField] private float[] overallSlip;
    #endregion

    #region Value AntiRoll
    [SerializeField] private float antiRoll;
    [SerializeField] private float antiRollForce;
    [SerializeField] private float travelL;
    [SerializeField] private float travelR;
    [SerializeField] private bool groundedL;
    [SerializeField] private bool groundedR;
    #endregion

    #region Value Drift
    [Header("Drift Value")]
    [Range(0.0f, 0.5f), SerializeField] private float slipLimit = 0.3f;
    [SerializeField] private GameObject smokePrefab;
    [SerializeField] private GameObject[] smokes;
    [SerializeField] private ParticleSystem[] smokeParticles;
    #endregion

    #region Function Steer Setting
    public void SetSteeringCurve(AnimationCurve _steeringCurve) { steeringCurve  = _steeringCurve; }
    protected void SetSteerWheelsCount(int _steerWheelsCount) { steerWheelsNum = _steerWheelsCount; }
    #endregion

    #region Fuction Wheels Setting
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
    [SerializeField] private float sideBrakePower;
    [SerializeField] private float targetBrakeTorque;
    [SerializeField] private float appliedBrakeTorque;
    [SerializeField] private float slipFactorABS;
    [SerializeField] private WheelCollider tempWheelColliderForBrake;
    [SerializeField] private bool isABSEnabled = true; // ABS 사용 여부
    [SerializeField, Range(0.1f, 1.0f)] private float absSlipThreshold = 0.35f; // ABS 개입을 시작할 Forward Slip 임계값 (음수)
    [SerializeField, Range(0.1f, 1.0f)] private float absBrakeReleaseFactor = 0.3f; // ABS 개입 강도 (1이면 슬립 시 브레이크 0, 낮을수록 약하게 개입)
    #endregion
    protected void SetFriction()
    {
        forwardSlip = new float[wheelNum];
        sidewaysSlip = new float[wheelNum];
        overallSlip = new float[wheelNum];
        for (int i = 0; i < wheelNum; i++)
        {
            forwardFriction = wheels[i].wheelCollider.forwardFriction;
            sidewaysFriction = wheels[i].wheelCollider.sidewaysFriction;

            forwardFriction.extremumSlip = 0.4f;
            forwardFriction.extremumValue = 2.0f;
            forwardFriction.asymptoteSlip = 0.8f;
            forwardFriction.asymptoteValue = 1.5f;

            sidewaysFriction.extremumSlip = 0.2f;
            sidewaysFriction.extremumValue = 2.5f;
            sidewaysFriction.asymptoteSlip = 0.7f;
            sidewaysFriction.asymptoteValue = 1.8f;

            wheels[i].wheelCollider.forwardFriction = forwardFriction;
            wheels[i].wheelCollider.sidewaysFriction = sidewaysFriction;
        }
        Braking();
    }

    protected void ChangeFriction(bool _mode)
    {
        for (int i = 0; i < wheelNum; i++)
        {
            forwardFriction = wheels[i].wheelCollider.forwardFriction;
            sidewaysFriction = wheels[i].wheelCollider.sidewaysFriction;

            if(_mode)//Drift mode
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
            }



            wheels[i].wheelCollider.forwardFriction = forwardFriction;
            wheels[i].wheelCollider.sidewaysFriction = sidewaysFriction;
        }
    }

    public void SetDriveAxel(eCAR_DRIVEAXEL _driveAxel)
    {
        driveAxel = _driveAxel;
        SetDriveWheels();
    }
    public void SetBrakePower(float _brakePower) { brakePower = _brakePower; }
    public void SetWheelMesh(Material _wheelMesh)
    {
        for (int i = 0; i < wheels.Count; i++)
        {
            MeshRenderer curWheel = wheels[i].wheelModel.GetComponent<MeshRenderer>();
            curWheel.material = _wheelMesh;
        }
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
    protected void Steering(float _input)
    {
        float input = _input;
        curSteerAngle = Mathf.Lerp(curSteerAngle, steeringCurve.Evaluate(speed) * input, maxSteerAngle > curSteerAngle ? Time.deltaTime * 2f : Time.deltaTime * 10f);//steeringCurve.Evaluate(speed);
        for (int i = 0; i < steerWheelsNum; i++)
            steerWheels[i].steerAngle = curSteerAngle;
        if (steeringHandle != null)
            steeringHandle.localRotation = Quaternion.Euler(0, 0, curSteerAngle * 16f);
    }
    protected void Braking() // FixedUpdate에서 호출하는 것이 최적입니다.
    {
        // 플레이어 입력에 의해 요청된 최대 브레이크 토크를 계산합니다.
        float requestedBrakeTorque = brakeInput * brakePower; // [출처:2] [출처:4] brakeInput는 Player_Car의 Update에서 업데이트됩니다.
        bool isBrakingIntent = brakeInput > 0.05f; // 플레이어가 브레이크를 의도했는지 확인합니다.

        TailLampSwitch(isBrakingIntent); // 브레이크 의도에 따라 테일 램프를 업데이트합니다. [출처:49]

        // ABS 로직을 적용하기 전에 마찰/슬립 값을 업데이트합니다.
        // UpdatingFriction이 FixedUpdate에서 다른 곳에서 호출되었다면 이 라인은 중복일 수 있습니다.
        // UpdatingFriction()이 FixedUpdate에서 브레이크를 호출하기 전에 한 번 실행되도록 해야 합니다.
        // UpdatingFriction(); // --> Braking() 전에 확실히 호출되도록 설정하세요.

        for (int i = 0; i < wheelNum; i++)
        {
            float finalBrakeTorque = 0f; // 이 휠에 대한 브레이크 토크를 0으로 시작합니다.

            if (isBrakingIntent) // 플레이어가 브레이크를 요청한 경우에만 브레이크를 적용합니다.
            {
                if (isABSEnabled && wheels[i].wheelCollider.isGrounded) // ABS가 활성화되어 있고 휠이 지면에 닿아있는지 확인합니다. [출처:53 관련]
                {
                    // overallSlip[i]는 이 함수가 실행되기 전에 UpdatingFriction()에 의해 업데이트되어야 합니다. [출처:55]
                    if (overallSlip[i] > absSlipThreshold)
                    {
                        // 휠이 미끄러짐 (잠금 상태)이 발생한 경우, 브레이크 압력을 줄입니다.
                        finalBrakeTorque = requestedBrakeTorque * absBrakeReleaseFactor;
                    }
                    else
                    {
                        // 휠이 과도하게 미끄러지지 않은 경우, 요청된 브레이크 토크를 완전히 적용합니다.
                        finalBrakeTorque = requestedBrakeTorque;
                    }
                }
                else if (wheels[i].wheelCollider.isGrounded) // ABS 비활성화 또는 휠이 지면에 닿아 있는 경우 정상적으로 적용
                {
                    finalBrakeTorque = requestedBrakeTorque;
                }
                // 지면에 닿아 있지 않고 브레이크를 작동하면 finalBrakeTorque는 0으로 유지됩니다(또는 요청된 브레이크를 적용? 동작 테스트 필요).
            }

            // TCS가 브레이크 힘을 적용하는지 확인합니다. (TorqueToWheel 수정에서 발생)
            // TCS 브레이크가 활성화된 경우, 혼합하거나 우선 순위를 지정할 가능성이 있습니다.
            // 간단하게 하기 위해, 페달 브레이크가 TCS 브레이크보다 우선하도록 설정합니다.
            // TCS가 ABS가 활성화된 상태에서 브레이크를 적용하려면 더 복잡한 로직이 필요합니다.
            // brakeInput > 0일 때 페달/ABS가 우선한다고 가정합니다.

            // 이 휠에 대해 최종 계산된 브레이크 토크를 적용합니다.
            wheels[i].wheelCollider.brakeTorque = finalBrakeTorque;
        }
    }
    
    public void SetAntiRoll(float _antiRoll) { antiRoll = _antiRoll; }
    protected void SideBrakingDown()
    {
        for (int i = 0; i < wheelNum; i++)
        {
            if (wheels[i].axel == eAXEL.eAXEL_BACK)
                wheels[i].wheelCollider.brakeTorque = Mathf.Infinity;
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

    private bool IsGrounded()
    {
        for (int i = 0; i < wheelNum; i++)
        {
            if (wheels[i].wheelCollider.isGrounded)
                return true;
        }
        return false;
    }

    protected void UpdatingFriction()
    {
        for (int i = 0; i < wheelNum; i++)
        {
            if (wheels[i].wheelCollider.GetGroundHit(out wheelHit))
            {
                overallSlip[i] = Mathf.Abs(wheelHit.forwardSlip + wheelHit.sidewaysSlip);

                forwardFriction = wheels[i].wheelCollider.forwardFriction;
                forwardFriction.stiffness = forwardTireGrip - overallSlip[i] / forwardValue;
                wheels[i].wheelCollider.forwardFriction = forwardFriction;

                sidewaysFriction = wheels[i].wheelCollider.sidewaysFriction;
                sidewaysFriction.stiffness = sidewaysTireGrip - overallSlip[i] / sideValue;
                wheels[i].wheelCollider.sidewaysFriction = sidewaysFriction;

                forwardSlip[i] = wheelHit.forwardSlip;
                sidewaysSlip[i] = wheelHit.sidewaysSlip;
            }
        }
        Braking();
    }

    protected void EffectDrift()
    {
        for(int i = 0; i < wheelNum; i++)
        {
            if (wheels[i].wheelCollider.GetGroundHit(out wheelHit))
            {
                if (Mathf.Abs(wheelHit.sidewaysSlip) >= 0.15f || Mathf.Abs(wheelHit.forwardSlip) >= 0.3f && IsGrounded())
                {
                    wheels[i].skidMarks.emitting = true;
                    if (!smokeParticles[i].isPlaying)
                        smokeParticles[i].Play();
                    //Debug.Log(WheelHit.sidewaysSlip);
                    //Debug.Log(WheelHit.forwardSlip);
                }
                else
                {
                    wheels[i].skidMarks.emitting = false;
                    if (!smokeParticles[i].isStopped)
                        smokeParticles[i].Stop();
                }
            }
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
                smokes[i].transform.parent = wheels[i].skidMarks.transform;
                smokes[i].transform.position = wheels[i].skidMarks.transform.position;
                smokes[i].transform.rotation = Quaternion.identity;
                smokes[i].transform.localScale = Vector3.one;
            }
        }
    }
    protected void AntiRollBar()
    {
        travelL = 1.0f;
        travelR = 1.0f;
        groundedL = steerWheels[0].GetGroundHit(out wheelHit) ? true : false;
        groundedR = steerWheels[1].GetGroundHit(out wheelHit) ? true : false;
        if (groundedL)
        {
            travelL = (-steerWheels[0].transform.InverseTransformPoint(wheelHit.point).y - steerWheels[0].radius) / steerWheels[0].suspensionDistance;
        }
        if (groundedR)
        {
            travelR = (-steerWheels[1].transform.InverseTransformPoint(wheelHit.point).y - steerWheels[1].radius) / steerWheels[1].suspensionDistance;
        }
        antiRollForce = (travelL - travelR) * antiRoll;
        if (groundedL)
            carRB.AddForceAtPosition(steerWheels[0].transform.up * -antiRollForce, steerWheels[0].transform.position);
        if (groundedR)
            carRB.AddForceAtPosition(steerWheels[1].transform.up * antiRollForce, steerWheels[1].transform.position);
    }
}