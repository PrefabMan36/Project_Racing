using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Car
{
    public enum eGEAR
    {
        eGEAR_NEUTURAL,
        eGEAR_REVERSE,
        eGEAR_FIRST,
        eGEAR_SECOND,
        eGEAR_THIRD,
        eGEAR_FOURTH,
        eGEAR_FIFTH,
        eGEAR_SIXTH
    }
    //Engine����
    //���� ��ũ(Engine Torque) ����
    //���� ��ũ(Nm) = (���� ���(kW) �� 9549) / ���� ȸ����(RPM)
    //���� ��ũ(Nm) = (���� ���(hp) x 7121) / ���� ȸ����(RPM)
    //���� ��ũ(Wheel Torque) ����
    //���� ��ũ = ���� ��ũ(Engine Torque) �� ���ӱ� ����(Gear ratio) �� ��������(Differential ratio)
    protected bool ignition;
    protected float throttle;
    [SerializeField] protected AnimationCurve horsePowerCurve;
    protected float horsePower;
    protected float maxEngineRPM, minEngineRPM, curEngineRPM, tempWheelRPM, curWheelRPM;
    protected float curEngineTorque, curWheelTorque;
    protected float dragAmount;
    protected AudioSource engineSound;

    //Engine stat setting ���� ���� ����
    public void SetHorsePower(float _horsePower) { horsePower = _horsePower; }
    public void SetMaxEngineRPM(float _maxRPM) { maxEngineRPM = _maxRPM; }
    public void SetEngineSound(AudioSource _engineSound) { engineSound = _engineSound; }

    //Gear���
    [SerializeField] protected eGEAR curGear;
    [SerializeField] protected eGEAR nextGear;
    protected Dictionary<eGEAR, float> gearRatio = new Dictionary<eGEAR, float>();
    [SerializeField] protected float differentialRatio;
    [SerializeField] protected float clutch;
    protected int maxGear;
    //protected bool menualGear;
    protected float shiftTimer;
    protected float shiftTiming;
    public void SetGearRatio(eGEAR _gearName, float _gearRatio) { gearRatio.Add(_gearName, _gearRatio); }

    //Engine Operation ���� ����
    protected void CalculateTorque()
    {
        if (ignition)
        {
            if (clutch < 0.1f)
            {
                curEngineRPM = Mathf.Lerp(curEngineRPM, Mathf.Max(minEngineRPM, maxEngineRPM * throttle) + Random.Range(-50, 50), Time.deltaTime);
            }
            else
            {
                curEngineRPM = Mathf.Lerp(curEngineRPM, Mathf.Max(minEngineRPM - 100, curWheelRPM / gearRatio[curGear] * clutch), Time.deltaTime);
                curWheelTorque = (horsePowerCurve.Evaluate(curEngineRPM / maxEngineRPM) * ((horsePower * 7121) / curEngineRPM)) * gearRatio[curGear] * differentialRatio * clutch;
            }
        }
        else
        {
            curWheelTorque = 0f;
        }
    }
    protected void Moving()
    {
        CalculateWheelRPM();
        CalculateTorque();
        for (int i = 0; i < driveWheelsNum; i++)
        {
            driveWheels[i].motorTorque = throttle * (curWheelTorque / driveWheelsNum);
        }
        carRB.drag = dragAmount + (speed / 40000);
        //for (int i = 0; i < driveWheelsNum; i++)
        //    driveWheels[i].motorTorque = throttle * curWheelTorque;
    }
    protected void CalculateWheelRPM()
    {
        tempWheelRPM = 0f;
        for (int i = 0; i < driveWheelsNum; i++)
        {
            tempWheelRPM += driveWheels[i].rpm;
        }
        tempWheelRPM = Mathf.Abs(tempWheelRPM / driveWheelsNum);
        curWheelRPM = tempWheelRPM;
    }
    protected void ChangeGear(bool _up)
    {
        if (_up)
        {
            switch(curGear)
            {
                case eGEAR.eGEAR_REVERSE:
                    nextGear = eGEAR.eGEAR_FIRST;
                    break;
                case eGEAR.eGEAR_FIRST:
                    nextGear = eGEAR.eGEAR_SECOND;
                    break;
                case eGEAR.eGEAR_SECOND:
                    nextGear = eGEAR.eGEAR_THIRD;
                    break;
                case eGEAR.eGEAR_THIRD:
                    nextGear = eGEAR.eGEAR_FOURTH;
                    break;
                case eGEAR.eGEAR_FOURTH:
                    nextGear = eGEAR.eGEAR_FIFTH;
                    break;
                case eGEAR.eGEAR_FIFTH:
                    nextGear = eGEAR.eGEAR_SIXTH;
                    break;
            }
        }
        else
        {
            switch (curGear)
            {
                case eGEAR.eGEAR_SIXTH:
                    nextGear = eGEAR.eGEAR_FIFTH;
                    break;
                case eGEAR.eGEAR_FIFTH:
                    nextGear = eGEAR.eGEAR_FOURTH;
                    break;
                case eGEAR.eGEAR_FOURTH:
                    nextGear = eGEAR.eGEAR_THIRD;
                    break;
                case eGEAR.eGEAR_THIRD:
                    nextGear = eGEAR.eGEAR_SECOND;
                    break;
                case eGEAR.eGEAR_SECOND:
                    nextGear = eGEAR.eGEAR_FIRST;
                    break;
                case eGEAR.eGEAR_FIRST:
                    nextGear = eGEAR.eGEAR_REVERSE;
                    break;
            }
        }
    }
    protected void GearShift()
    {
        if (curGear == nextGear)
            return;
        if (shiftTimer < shiftTiming)
        {
            curGear = eGEAR.eGEAR_NEUTURAL;
            clutch = 0f;
            shiftTimer += Time.deltaTime;
        }
        else
        {
            curGear = nextGear;
            curEngineRPM = minEngineRPM;
            shiftTimer = 0;
        }
    }
}
