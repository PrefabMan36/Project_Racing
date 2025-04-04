using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Car : Object_Movable
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
    //chassis¼¨½Ã
    [SerializeField] protected List<Wheel> wheels;
    protected int wheelCount;
    protected List<Wheel> driveWheels = new List<Wheel>();
    protected List<Wheel> steerWheels = new List<Wheel>();
    protected int driveWheelsNum;
    protected eCAR_DRIVEAXEL driveAxel;
    protected float maxAngle;
    protected Vector3 curAngle;
    protected float steerSpeed;
    protected float brakePower;
    protected float sideBrakePower;
    protected float brakeAcceleration;
    protected float sideBrakeAcceleration;
    public void SetMaxSteerAngle(float _maxAngle) { maxAngle = _maxAngle; }
    private void SetDriveWheels()
    {
        wheelCount = wheels.Count;
        switch(driveAxel)
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
                steerWheels.Add(wheels[i]);
                switch (driveAxel)
            {
                case eCAR_DRIVEAXEL.eFWD:
                    if (wheels[i].axel == eAXEL.eAXEL_FRONT)
                        driveWheels.Add(wheels[i]);
                    break;
                case eCAR_DRIVEAXEL.eRWD:
                    if (wheels[i].axel == eAXEL.eAXEL_BACK)
                        driveWheels.Add(wheels[i]);
                    break;
                case eCAR_DRIVEAXEL.e4WD:
                    driveWheels.Add(wheels[i]);
                    break;
            }
        }
    }
    public void SetDriveAxel(eCAR_DRIVEAXEL _driveAxel)
    {
        driveAxel = _driveAxel;
        SetDriveWheels();
    }
    public void SetBrakePower(float _brakePower) { brakeAcceleration = _brakePower; }
    public void SetWheelMesh(Material _wheelMesh)
    {
        for (int i = 0; i < wheels.Count; i++)
        {
            MeshRenderer curWheel = wheels[i].wheelModel.GetComponent<MeshRenderer>();
            curWheel.material = _wheelMesh;
        }
    }

    //BodyÂ÷Ã¼
    protected GameObject carBody;
    protected Vector3 centerMass;
    [SerializeField] protected Rigidbody carRB;
    public void SetCenterMass(Vector3 _centerMass) { centerMass = _centerMass; }
    public void SetCarRB(Rigidbody _carRB) { carRB = _carRB; }
    public void SetCarColor(Material[] _CarColor)
    {
        MeshRenderer carMesh = carBody.gameObject.GetComponent<MeshRenderer>();
        carMesh.materials = _CarColor;
    }

    protected void MoveForward()
    {
        for(int i = 0;i < driveWheelsNum; i++)
        {
            if (carRB.velocity.magnitude < 0.01f && curGear == eGEAR.eGEAR_REVERSE)
                BrakingDown();
            else
            {
                driveWheels[i].wheelCollider.motorTorque = GetPower();
            }
        }
    }
    protected void MoveBackward()
    {
        for (int i = 0; i < driveWheelsNum; i++)
        {
            if (carRB.velocity.magnitude < 0.01f && curGear != eGEAR.eGEAR_REVERSE)
                BrakingDown();
            else
            {
                curGear = eGEAR.eGEAR_REVERSE;
                driveWheels[i].wheelCollider.motorTorque = GetPower();
            }
        }
    }
    protected void MoveStop()
    {
        for (int i = 0; i < driveWheelsNum; i++)
        {
            driveWheels[i].wheelCollider.motorTorque = 0;
        }
    }
    protected void BrakingDown()
    {
        for (int i = 0; i < 2; i++)
        {
            steerWheels[i].wheelCollider.brakeTorque = 2000;
        }
    }
    protected void BrakingUp()
    {
        for (int i = 0; i < driveWheelsNum; i++)
        {
            steerWheels[i].wheelCollider.brakeTorque = 0;
        }
    }
    protected void SideBrakingDown()
    {
        for (int i = 0; i < driveWheelsNum; i++)
        {
            driveWheels[i].wheelCollider.brakeTorque = 2000;
        }
    }
    protected void SideBrakingUp()
    {
        for (int i = 0; i < driveWheelsNum; i++)
        {
            driveWheels[i].wheelCollider.brakeTorque = 0;
        }
    }
    protected void SteerNone()
    {
        for (int i = 0; i < 2; i++)
        {
            steerWheels[i].wheelCollider.steerAngle = 0;
        }
    }
    protected void SteerRight()
    {
        for (int i = 0; i < 2 ; i++)
        {
            steerWheels[i].wheelCollider.steerAngle = 60;
        }
    }
    protected void SteerLeft()
    {
        for (int i = 0; i < 2; i++)
        {
            steerWheels[i].wheelCollider.steerAngle = -60;
        }
    }
}
