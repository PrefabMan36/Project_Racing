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
        public ParticleSystem tireSmoke;
        public eAXEL axel;
    }
    protected List<MeshRenderer> wheelTransform;
    protected Quaternion tempWheelRotation;
    protected Vector3 tempWheelPosition;
    private float wheelRadius;

    [SerializeField] private Transform steeringWheel;

    //wheels
    private WheelHit WheelHit; //»Ÿ¡§∫∏
    [SerializeField] protected List<Wheel> wheels;
    protected int wheelCount;
    protected List<WheelCollider> driveWheels = new List<WheelCollider>();
    protected List<WheelCollider> steerWheels = new List<WheelCollider>();
    protected int driveWheelsNum;
    protected int steerWheelsNum;
    protected eCAR_DRIVEAXEL driveAxel;
    private float[] differentialPower;
    [SerializeField] private float differentialPowerValue = 0f;

    //tire
    [Range(0.8f, 1.3f)] private float tireGrip = 1.3f;
    [Range(1f, 2f)] private float forwardValue = 1f;
    [Range(1f, 2f)] private float sideValue = 2f;
    private WheelFrictionCurve forwardFriction, sidewaysFriction;
    [SerializeField] private float[] forwardSlip;
    [SerializeField] private float[] sidewaysSlip;
    [SerializeField] private float[] overallSlip;

    [SerializeField] protected AnimationCurve steeringCurve;
    [SerializeField] private float steeringInput = 0f;
    [SerializeField] private float maxSteerAngle = 30f;
    [SerializeField] protected float curSteerAngle = 0f;
    protected float steerSpeed;
    protected float slipingAngle;
    protected float brakeInput;
    protected float sideBrakeInput;
    protected float brakePower;
    protected float sideBrakePower;

    //Anti Roll Bar
    protected float antiRoll;
    private float antiRollForce;
    private float travelL;
    private float travelR;
    private bool groundedL;
    private bool groundedR;

    protected void SetSteerWheelsCount(int _steerWheelsCount) { steerWheelsNum = _steerWheelsCount; }
    protected void SetDriveWheels()
    {
        wheelCount = wheels.Count;
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
        for (int i = 0; i < wheelCount; i++)
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
        wheelRadius = wheels[0].wheelCollider.radius;
        differentialPower = new float[driveWheelsNum];
    }
    protected void SetFriction()
    {
        forwardSlip = new float[wheelCount];
        sidewaysSlip = new float[wheelCount];
        overallSlip = new float[wheelCount];
        for (int i = 0; i < wheelCount; i++)
        {
            forwardFriction = wheels[i].wheelCollider.forwardFriction;
            sidewaysFriction = wheels[i].wheelCollider.sidewaysFriction;

            //forwardFriction.extremumSlip = 0.4f;
            //forwardFriction.extremumValue = 1.8f;
            //forwardFriction.asymptoteSlip = 1.2f;
            //forwardFriction.asymptoteValue = 1.0f;

            //sidewaysFriction.extremumSlip = 0.7f;
            //sidewaysFriction.extremumValue = 1.2f;
            //sidewaysFriction.asymptoteSlip = 1.5f;
            //sidewaysFriction.asymptoteValue = 1.2f;

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
        if (steeringWheel != null)
            steeringWheel.localRotation = Quaternion.Euler(0, 0, curSteerAngle * 16f);
    }
    protected void Braking()
    {
        for (int i = 0; i < wheelCount; i++)
        {
            wheels[i].wheelCollider.brakeTorque = brakeInput * brakePower;
        }
    }
    protected void SideBrakingDown()
    {
        for (int i = 0; i < wheelCount; i++)
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
        for (int i = 0; i < wheelCount; i++)
        {
            if (wheels[i].wheelCollider.isGrounded)
                return true;
        }
        return false;
    }

    protected void UpdatingFriction()
    {
        for (int i = 0; i < wheelCount; i++)
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

                if (sidewaysSlip[i] > 0.15f || forwardSlip[i] > 0.3f)
                {
                    wheels[i].skidMarks.emitting = true;
                    wheels[i].tireSmoke.Play();
                }
                else
                {
                    wheels[i].skidMarks.emitting = false;
                    wheels[i].tireSmoke.Stop();
                }
            }
        }
        differentialPowerValue = 0f;
        for (int i = 0; i < driveWheelsNum; i++)
        {
            if (wheels[i].wheelCollider.GetGroundHit(out WheelHit))
            {
                if(i % 2 == 0)
                    differentialPowerValue += WheelHit.forwardSlip;
                else
                    differentialPowerValue -= WheelHit.forwardSlip;
            }
        }
        if(differentialPowerValue > 0)
        {
            for (int i = 0; i < driveWheelsNum; i++)
            {
                if (i % 2 == 0)
                    differentialPower[i] = 1f + differentialPowerValue;
                else
                    differentialPower[i] = 1f - differentialPowerValue;
            }
        }
        else
        {
            for (int i = 0; i < driveWheelsNum; i++)
            {
                if (i % 2 == 0)
                    differentialPower[i] = 1f - differentialPowerValue;
                else
                    differentialPower[i] = 1f + differentialPowerValue;
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