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
    //Engine엔진
    protected bool ignition;
    protected float throttleInput;
    [SerializeField] protected AnimationCurve torqueCurve;
    protected float maxEngineTorque;
    [SerializeField] protected float maxEngineRPM;
    [SerializeField]protected float minEngineRPM;
    [SerializeField]protected float curEngineRPM;
    [SerializeField]protected float targetEngineRPM;
    protected float normalizedRPM;
    [SerializeField] protected float curPower;
    protected AudioSource engineSound;
    
    //Engine stat setting 엔진 스텟 설정
    public void SetEngineTorque(float _torque) { maxEngineTorque = _torque; }
    public void SetMaxEngineRPM(float _maxRPM) { maxEngineRPM = _maxRPM; }
    public void SetEngineSound(AudioSource _engineSound) { engineSound = _engineSound; }
    //Gear기어
    [SerializeField] protected eGEAR curGear;
    [SerializeField] protected eGEAR nextGear;
    protected Dictionary<eGEAR, float> gearRatio = new Dictionary<eGEAR, float>();
    protected int maxGear;
    protected bool menualGear;
    [SerializeField] protected float wheelPower;
    protected float shiftTimer;
    protected float shiftTiming;
    public void SetGearRatio(eGEAR _gearName, float _gearRatio) { gearRatio.Add(_gearName, _gearRatio); }
    protected float GetPower()
    {
        normalizedRPM = Mathf.Clamp01(curEngineRPM / maxEngineRPM);
        curPower = torqueCurve.Evaluate(normalizedRPM) * maxEngineTorque;
        wheelPower = curPower * gearRatio[curGear];
        return wheelPower;
    }
    protected void InputThrottle()
    {
        float throttleInput = Mathf.Abs(Input.GetAxis("Vertical"));
        targetEngineRPM = Mathf.Lerp(minEngineRPM, maxEngineRPM, throttleInput);
        curEngineRPM = Mathf.Lerp(curEngineRPM, targetEngineRPM, Time.deltaTime);
        curEngineRPM = Mathf.Clamp(curEngineRPM, minEngineRPM, maxEngineRPM);
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
