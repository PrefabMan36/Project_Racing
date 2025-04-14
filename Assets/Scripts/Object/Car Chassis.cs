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
        public eAXEL axel;
    }
    protected List<MeshRenderer> wheelTransform;
    protected Quaternion tempWheelRotation;
    protected Vector3 tempWheelPosition;

    //wheels
    private WheelHit WheelHit; //»Ÿ¡§∫∏
    [SerializeField] protected List<Wheel> wheels;
    protected int wheelCount;
    protected List<WheelCollider> driveWheels = new List<WheelCollider>();
    protected List<WheelCollider> steerWheels = new List<WheelCollider>();
    protected int driveWheelsNum;
    protected int steerWheelsNum;
    protected eCAR_DRIVEAXEL driveAxel;
    [SerializeField] private float[] differentialPower;

    //tire
    [Range(0.8f, 1.3f)] private float tireGrip = 1.0f;
    [Range(0.8f, 1.3f)] private float coreTireGrip = 1.0f;
    [Range(1f, 2f)] private float forwardValue = 1f;
    [Range(1f, 2f)] private float sideValue = 2f;
    private WheelFrictionCurve forwardFriction, sidewaysFriction;
    [SerializeField] private float[] forwardSlip;
    [SerializeField] private float[] sidewaysSlip;
    [SerializeField] private float[] overallSlip;

    [SerializeField] protected AnimationCurve steeringCurve;
    protected float steeringInput;
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

    public void SetTireGrip(float _grip) { coreTireGrip = _grip; }
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

            forwardFriction.extremumSlip = 0.7f;
            forwardFriction.extremumValue = 1.8f;
            forwardFriction.asymptoteSlip = 1.2f;
            forwardFriction.asymptoteValue = 0.5f;

            wheels[i].wheelCollider.forwardFriction = forwardFriction;

            sidewaysFriction = wheels[i].wheelCollider.sidewaysFriction;

            sidewaysFriction.extremumSlip = 1.0f;
            sidewaysFriction.extremumValue = 2.2f;
            sidewaysFriction.asymptoteSlip = 1.5f;
            sidewaysFriction.asymptoteValue = 0.6f;

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
        }
    }
    protected void Steering(float _input)
    {
        steeringInput = _input;
        curSteerAngle = steeringInput * steeringCurve.Evaluate(speed);
        for (int i = 0; i < steerWheelsNum; i++)
            steerWheels[i].steerAngle = curSteerAngle;
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
        for (int i = 0; i < driveWheelsNum; i++)
        {
            driveWheels[i].brakeTorque = Mathf.Infinity;
        }
    }
    protected void SideBrakingUp()
    {
        for (int i = 0; i < driveWheelsNum; i++)
        {
            driveWheels[i].brakeTorque = 0;
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
            }
        }
        for (int i = 0; i < driveWheelsNum; i++)
        {
            if (wheels[i].wheelCollider.GetGroundHit(out WheelHit))
            {
                differentialPower[i] = WheelHit.force;
            }
        }
    }

    protected void AntiRollBar()
    {
        travelL = 1.0f;
        travelR = 1.0f;
        groundedL = steerWheels[0].GetGroundHit(out WheelHit);
        groundedR = steerWheels[1].GetGroundHit(out WheelHit);
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