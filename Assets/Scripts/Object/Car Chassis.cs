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
    [SerializeField] private AnimationCurve steeringCurve;
    [SerializeField] private Transform steeringHandle;
    [SerializeField] private float maxSteerAngle = 30f;
    [SerializeField] private float curSteerAngle = 0f;
    [SerializeField] private float steerSpeed;
    [SerializeField] private float slipingAngle;
    [SerializeField] private float brakeInput;
    [SerializeField] private float sideBrakeInput;
    [SerializeField] private float brakePower;
    [SerializeField] private float sideBrakePower;
    #endregion

    #region Value Wheels
    [Header("Fake Wheels")]
    [SerializeField] private List<MeshRenderer> wheelTransform;
    [SerializeField] private Quaternion tempWheelRotation;
    [SerializeField] private Vector3 tempWheelPosition;
    [SerializeField] private float wheelRadius;

    [Header("Real Wheels")]
    [SerializeField] private WheelHit WheelHit; //»Ÿ¡§∫∏
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
    [Range(0.8f, 1.3f), SerializeField] private float tireGrip = 1.3f;
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
    protected void SetFriction()
    {
        forwardSlip = new float[wheelNum];
        sidewaysSlip = new float[wheelNum];
        overallSlip = new float[wheelNum];
        for (int i = 0; i < wheelNum; i++)
        {
            forwardFriction = wheels[i].wheelCollider.forwardFriction;
            sidewaysFriction = wheels[i].wheelCollider.sidewaysFriction;

            forwardFriction.extremumSlip = 0.065f;
            forwardFriction.extremumValue = 1.8f;
            forwardFriction.asymptoteSlip = 1.2f;
            forwardFriction.asymptoteValue = 1.8f;

            sidewaysFriction.extremumSlip = 0.065f;
            sidewaysFriction.extremumValue = 2.2f;
            sidewaysFriction.asymptoteSlip = 1.6f;
            sidewaysFriction.asymptoteValue = 2.0f;

            wheels[i].wheelCollider.forwardFriction = forwardFriction;
            wheels[i].wheelCollider.sidewaysFriction = sidewaysFriction;
        }
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
                forwardFriction.extremumSlip = 0.065f;
                forwardFriction.extremumValue = 2.0f;
                forwardFriction.asymptoteSlip = 1.2f;
                forwardFriction.asymptoteValue = 2.0f;
                sidewaysFriction.extremumSlip = 0.065f;
                sidewaysFriction.extremumValue = 2.2f;
                sidewaysFriction.asymptoteSlip = 1.6f;
                sidewaysFriction.asymptoteValue = 2.4f;
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
    protected void Braking()
    {
        for (int i = 0; i < wheelNum; i++)
        {
            wheels[i].wheelCollider.brakeTorque = brakeInput * brakePower;
        }
        TailLampSwitch(brakeInput > 0 ? true : false);
    }
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
            if (wheels[i].wheelCollider.GetGroundHit(out WheelHit))
            {
                overallSlip[i] = Mathf.Abs(WheelHit.forwardSlip + WheelHit.sidewaysSlip);

                forwardFriction = wheels[i].wheelCollider.forwardFriction;
                forwardFriction.stiffness = tireGrip - (overallSlip[i] / 2) / forwardValue;
                wheels[i].wheelCollider.forwardFriction = forwardFriction;

                sidewaysFriction = wheels[i].wheelCollider.sidewaysFriction;
                sidewaysFriction.stiffness = tireGrip - overallSlip[i] / sideValue;
                wheels[i].wheelCollider.sidewaysFriction = sidewaysFriction;

                forwardSlip[i] = WheelHit.forwardSlip;
                sidewaysSlip[i] = WheelHit.sidewaysSlip;
            }
        }
    }

    protected void EffectDrift()
    {
        for(int i = 0; i < wheelNum; i++)
        {
            if (wheels[i].wheelCollider.GetGroundHit(out WheelHit))
            {
                if (Mathf.Abs(WheelHit.sidewaysSlip) >= 0.15f || Mathf.Abs(WheelHit.forwardSlip) >= 0.3f && IsGrounded())
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
        groundedL = steerWheels[0].GetGroundHit(out WheelHit) ? true : false;
        groundedR = steerWheels[1].GetGroundHit(out WheelHit) ? true : false;
        if (groundedL)
        {
            travelL = (-steerWheels[0].transform.InverseTransformPoint(WheelHit.point).y - steerWheels[0].radius) / steerWheels[0].suspensionDistance;
        }
        if (groundedR)
        {
            travelR = (-steerWheels[1].transform.InverseTransformPoint(WheelHit.point).y - steerWheels[1].radius) / steerWheels[1].suspensionDistance;
        }
        antiRollForce = (travelL - travelR) * antiRoll;
        if (groundedL)
            carRB.AddForceAtPosition(steerWheels[0].transform.up * -antiRollForce, steerWheels[0].transform.position);
        if (groundedR)
            carRB.AddForceAtPosition(steerWheels[1].transform.up * antiRollForce, steerWheels[1].transform.position);
    }
}