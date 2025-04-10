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
    private float acceleration;
    protected float engineAcceleration;
    [SerializeField] protected AnimationCurve horsePowerCurve;
    protected float horsePower;
    protected float maxEngineRPM, minEngineRPM, curEngineRPM, tempWheelRPM, curWheelRPM;
    protected float curEngineTorque, curWheelTorque;

    //forceRPMChange
    [SerializeField] private bool redLine = false;
    private float engineLerpValue;
    //others
    protected float dragAmount;
    [SerializeField] protected AudioSource engineSound;

    //Engine stat setting ���� ���� ����
    public void SetHorsePower(float _horsePower) { horsePower = _horsePower; }
    public void SetMaxEngineRPM(float _maxRPM) { maxEngineRPM = _maxRPM; }
    public void SetEngineSound(AudioSource _engineSound) { engineSound = _engineSound; }

    //Gear���
    [SerializeField] protected eGEAR curGear;
    [SerializeField] protected eGEAR nextGear;
    protected Dictionary<eGEAR, float> gearRatio = new Dictionary<eGEAR, float>();
    protected Dictionary<eGEAR, float> gearSpeedLimit = new Dictionary<eGEAR, float>();
    protected float differentialRatio;
    protected float finalDriveRatio;
    protected float clutch;
    private bool reverse;
    protected eGEAR lastGear;
    [SerializeField] protected bool autoGear;
    protected float shiftTimer;
    protected float shiftTiming;
    public void SetGearRatio(eGEAR _gearName, float _gearRatio)
    {
        if (gearRatio.ContainsKey(_gearName))
            gearRatio[_gearName] = _gearRatio;
        else
            gearRatio.Add(_gearName, _gearRatio);
    }
    public void SetGearSpeedLimit(eGEAR _gearName, float _gearSpeed)
    {
        if(gearSpeedLimit.ContainsKey(_gearName))
            gearSpeedLimit[_gearName] = _gearSpeed;
        else
            gearSpeedLimit.Add(_gearName, _gearSpeed);
    }

    //Engine Operation ���� ����
    protected void Engine()
    {
        if (ignition)
        {
            forceEngineLerp();
            CalculateWheelRPM();
            CalculateTorque();
            TorqueToWheel();
            if(autoGear) AutoGear();
            EngineSoundUpdate();
        }
        else
        {
            curEngineRPM = 0f;
            curWheelTorque = 0f;
            if(throttle > 0f)
                ignition = true;
        }
    }
    private void TorqueToWheel()
    {
        for (int i = 0; i < driveWheelsNum; i++)
        {
            driveWheels[i].motorTorque = throttle * (curWheelTorque / driveWheelsNum);
        }
        carRB.drag = dragAmount + (speed / 40000);
    }
    private void CalculateWheelRPM()
    {
        tempWheelRPM = 0f;
        for (int i = 0; i < driveWheelsNum; i++)
        {
            tempWheelRPM += driveWheels[i].rpm;
        }
        tempWheelRPM = tempWheelRPM / driveWheelsNum;
        if (tempWheelRPM > 0f)
            reverse = false;
        else
            reverse = true;
            curWheelRPM = tempWheelRPM;
    }
    private void CalculateTorque()
    {
        if (curEngineRPM >= maxEngineRPM) SetEngineLerp(maxEngineRPM - 1000f);
        if(!redLine)
        {
            if(clutch < 0.1f)
                curEngineRPM = Mathf.Lerp(curEngineRPM, Mathf.Max(minEngineRPM, maxEngineRPM * throttle), (engineAcceleration) * Time.deltaTime * 3f);
            else
            {
                curEngineRPM = Mathf.Lerp
                    (
                        curEngineRPM,
                        Mathf.Max(minEngineRPM, curWheelRPM * finalDriveRatio * gearRatio[curGear]),
                        Time.deltaTime * 0.5f
                    );
                if (speed < gearSpeedLimit[curGear])
                    curWheelTorque = (horsePowerCurve.Evaluate(curEngineRPM / maxEngineRPM) * (horsePower /* 7121f*/)) * (gearRatio[curGear] * finalDriveRatio) * clutch;
                else
                    curWheelTorque = 0f;
            }
        }
    }
    protected void SetEngineLerp(float _num)
    {
        redLine = true;
        engineLerpValue = _num;
    }
    private void forceEngineLerp()
    {
        if(redLine)
        {
            curEngineTorque = 0f;
            curEngineRPM = Mathf.Lerp(curEngineRPM,engineLerpValue,20 * Time.deltaTime);
            redLine = curEngineRPM <= engineLerpValue + 100 ? false : true;
        }
    }
    protected void ChangeGear(bool _up)
    {
        if (_up)
        {
            if(curGear == lastGear) { return; }
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
    protected void SetGear(eGEAR _gear) { nextGear = _gear; }
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
            curEngineRPM = maxEngineRPM / 2 + 1000f;
            shiftTimer = 0;
        }
    }
    private void AutoGear()
    {
        if(!IsGrounded()) return;

        if (curEngineRPM > maxEngineRPM - 100f && curGear != lastGear && !reverse && speed > 40f)
            ChangeGear(true);
        if (curEngineRPM < maxEngineRPM / 2 && curGear != eGEAR.eGEAR_FIRST)
            ChangeGear(false);
    }

    private void EngineSoundUpdate()
    {
        if(engineSound == null)
            return;
        engineSound.pitch = Mathf.Lerp(-2, 2, curEngineRPM / maxEngineRPM);
    }
}
