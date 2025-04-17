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
    [SerializeField]protected float throttle;
    private float acceleration;
    private float overSpeed;
    protected float engineAcceleration;
    [SerializeField] protected AnimationCurve horsePowerCurve;
    [SerializeField] protected AnimationCurve engineTorqueCurve;
    protected float horsePower;
    [SerializeField]protected float maxEngineRPM, minEngineRPM, curEngineRPM, tempWheelRPM, curWheelRPM;
    [SerializeField]protected float curEngineTorque, curWheelTorque;

    protected RPMGauge rpmGauge;

    //forceRPMChange
    private bool redLine = false;
    private float engineLerpValue;
    //others
    protected float dragAmount;
    [SerializeField] protected AudioSource engineSound;

    //Engine stat setting ���� ���� ����
    public void SetHorsePower(float _horsePower) { horsePower = _horsePower; }
    public void SetMaxEngineRPM(float _maxRPM) { maxEngineRPM = _maxRPM; }
    public void SetEngineSound(AudioSource _engineSound) { engineSound = _engineSound; }

    //Gear���
    [SerializeField] protected eGEAR curGear = eGEAR.eGEAR_NEUTURAL;
    [SerializeField] protected eGEAR nextGear = eGEAR.eGEAR_NEUTURAL;
    private Dictionary<eGEAR, float> gearRatio = new Dictionary<eGEAR, float>();
    private Dictionary<eGEAR, float> gearSpeedLimit = new Dictionary<eGEAR, float>();
    private float perviousMaxSpeed = 0;
    protected float differentialRatio;
    protected float finalDriveRatio;
    [SerializeField] protected float clutch;
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
            if(engineSound.isPlaying == false)
                engineSound.Play();
            GearShift();
            forceEngineLerp();
            CalculateWheelRPM();
            CalculateTorque();
            TorqueToWheel();
            if (autoGear) AutoGear();
            EngineSoundUpdate();
            UpdateRPMGauge();
        }
        else
        {
            engineSound.Stop();
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
    }
    private void CalculateWheelRPM()
    {
        tempWheelRPM = 0f;
        for (int i = 0; i < driveWheelsNum; i++)
        {
            tempWheelRPM += driveWheels[i].rpm;
        }
        curWheelRPM = tempWheelRPM / driveWheelsNum;
        if (tempWheelRPM > 0f)
            reverse = false;
        else
            reverse = true;
    }
    private void CalculateTorque()
    {
        if (curEngineRPM >= maxEngineRPM) SetEngineLerp(maxEngineRPM - 1000f);
        if(!redLine)
        {
            if (speed > gearSpeedLimit[curGear])
                overSpeed = 4f;
            else
                overSpeed = 1f;
            if (clutch < 0.1f)
                curEngineRPM = Mathf.Lerp(curEngineRPM, Mathf.Max(minEngineRPM, maxEngineRPM * throttle), Time.deltaTime * 3f);
            else
            {
                curEngineRPM = Mathf.Lerp
                    (
                        curEngineRPM,
                        Mathf.Max(minEngineRPM, curWheelRPM * finalDriveRatio * gearRatio[curGear]),
                        (overSpeed * engineAcceleration * Time.deltaTime) * gearRatio[curGear]
                    );
                if (speed < gearSpeedLimit[curGear])
                    curWheelTorque = (engineTorqueCurve.Evaluate(curEngineRPM) /* (horsePowerCurve.Evaluate(curEngineRPM)))*/ * (gearRatio[curGear] * finalDriveRatio) * clutch);
                else
                    curWheelTorque = 0f;
            }
        }
    }
    protected float LastGearLimit() { return gearSpeedLimit[lastGear]; }
    protected void SetEngineLerp(float _num)
    {
        redLine = true;
        engineLerpValue = _num;
    }
    private void forceEngineLerp()
    {
        if(redLine)
        {
            //curEngineTorque = 0f;
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
        if (autoGear)
            shiftTiming = 0.25f;
        if (shiftTimer < shiftTiming)
        {
            curGear = eGEAR.eGEAR_NEUTURAL;
            throttle = 0f;
            clutch = 0f;
            shiftTimer += Time.deltaTime;
            //curEngineRPM = Mathf.Lerp(curEngineRPM, maxEngineRPM / 3 + minEngineRPM, Time.deltaTime * 5f);
        }
        else
        {
            curGear = nextGear;
            curEngineRPM = maxEngineRPM / 2 + minEngineRPM;
            shiftTimer = 0;
            switch (curGear)
            {
                case eGEAR.eGEAR_NEUTURAL:
                case eGEAR.eGEAR_REVERSE:
                    perviousMaxSpeed = gearSpeedLimit[eGEAR.eGEAR_NEUTURAL];
                    break;
                case eGEAR.eGEAR_FIRST:
                    perviousMaxSpeed = gearSpeedLimit[eGEAR.eGEAR_NEUTURAL];
                    break;
                case eGEAR.eGEAR_SECOND:
                    perviousMaxSpeed = gearSpeedLimit[eGEAR.eGEAR_FIRST];
                    break;
                case eGEAR.eGEAR_THIRD:
                    perviousMaxSpeed = gearSpeedLimit[eGEAR.eGEAR_SECOND];
                    break;
                case eGEAR.eGEAR_FOURTH:
                    perviousMaxSpeed = gearSpeedLimit[eGEAR.eGEAR_THIRD];
                    break;
                case eGEAR.eGEAR_FIFTH:
                    perviousMaxSpeed = gearSpeedLimit[eGEAR.eGEAR_FOURTH];
                    break;
                case eGEAR.eGEAR_SIXTH:
                    perviousMaxSpeed = gearSpeedLimit[eGEAR.eGEAR_FIFTH];
                    break;
            }
        }
    }
    private void AutoGear()
    {
        if(!IsGrounded()) return;
        if(speed > gearSpeedLimit[curGear] -10f && curEngineRPM >= maxEngineRPM - minEngineRPM - 500f) { ChangeGear(true); }
        if(speed < perviousMaxSpeed + 10f && curGear != eGEAR.eGEAR_FIRST && curEngineRPM < maxEngineRPM / 3 + minEngineRPM) { ChangeGear(false); }
    }

    private void EngineSoundUpdate()
    {
        if(engineSound == null)
            return;
        engineSound.volume = Mathf.Lerp(0.3f, 0.4f, clutch);
        engineSound.pitch = Mathf.Lerp(0.1f, 1.4f, curEngineRPM / maxEngineRPM);
    }

    private void UpdateRPMGauge()
    {
        rpmGauge.SetValue(Mathf.Lerp(0f, 0.375f, curEngineRPM / maxEngineRPM));
    }
}
