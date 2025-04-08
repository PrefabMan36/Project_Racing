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

    //chassis¼¨½Ã
    [SerializeField] protected List<Wheel> wheels;
    protected int wheelCount;
    protected List<WheelCollider> driveWheels = new List<WheelCollider>();
    protected List<WheelCollider> steerWheels = new List<WheelCollider>();
    protected int driveWheelsNum;
    protected int steerWheelsNum;
    protected eCAR_DRIVEAXEL driveAxel;
    [SerializeField] protected AnimationCurve steeringCurve;
    protected float steeringInput;
    [SerializeField] protected float curSteerAngle;
    protected float steerSpeed;
    protected float slipingAngle;
    protected float brakeInput;
    protected float sideBrakeInput;
    protected float brakePower;
    protected float sideBrakePower;

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
            if (wheels[i].axel == eAXEL.eAXEL_BACK)
                wheels[i].wheelCollider.brakeTorque = brakeInput * brakePower * 2;
            else
                wheels[i].wheelCollider.brakeTorque = brakeInput * brakePower;
        }
    }
    protected void SideBrakingDown()
    {
        for (int i = 0; i < driveWheelsNum; i++)
        {
            driveWheels[i].brakeTorque += sideBrakePower;
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
        for(int i = 0;i < wheelCount;i++)
        {
            if (wheels[i].wheelCollider.isGrounded)
                return true;
        }
        return false;
    }
}
