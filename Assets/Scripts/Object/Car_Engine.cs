using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tiny;

public partial class Car
{
    //현재 표기방식은 구식 표현 현재는 굳이 e를 붙이지 않는다 이경우 대문자이름이 enum이다.
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

    #region Value Others
    [Header("Other Value")]
    protected bool ignition = true;
    private bool engineStartUP = false;
    protected float dragAmount;
    [SerializeField] protected AudioSource engineSound;
    [SerializeField] private GameObject startUpSoundObject;
    private GameObject _tempSoundObject;
    private AudioSource startUpSound;
    #endregion

    #region Value Engine
    [Header("Engine Value")]
    [SerializeField] private AnimationCurve horsePowerCurve;
    [SerializeField] private AnimationCurve engineTorqueCurve;
    [SerializeField] protected float throttle;
    [SerializeField] private float engineAcceleration;
    [SerializeField] private float maxEngineRPM, minEngineRPM, currentEngineRPM, tempWheelRPM, currentWheelRPM;
    [SerializeField] private float currentEngineTorque, currentWheelTorque;
    [SerializeField] private float overSpeed;
    [SerializeField] private bool redLine = false;
    [SerializeField] private float engineLerpValue;
    #endregion

    #region Function Engine setting
    public void SetEngineCurves(AnimationCurve _horsePowerCurve,  AnimationCurve _engineTorqueCurve) { horsePowerCurve = _horsePowerCurve; engineTorqueCurve = _engineTorqueCurve; }
    public void SetEngineAcceleration(float _engineAcceleration) { engineAcceleration = _engineAcceleration; }
    public void SetEngineRPMLimit(float _maxEngineRPM, float _minEngineRPM) { maxEngineRPM = _maxEngineRPM; minEngineRPM = _minEngineRPM; }
    public void SetMaxEngineRPM(float _maxRPM) { maxEngineRPM = _maxRPM; }
    public void SetEngineSound(AudioSource _engineSound) { engineSound = _engineSound; }
    #endregion

    #region Function Engine
    IEnumerator IgnitionEngine()
    {
        WaitForSeconds wfs = new WaitForSeconds(0.04f);
        engineStartUP = true;
        if (ignition)
        {
            ignition = false;
            engineSound.Stop();
            yield break;
        }
        if (startUpSound == null ) yield break;
        _tempSoundObject = Instantiate(startUpSoundObject);
        startUpSound = _tempSoundObject.GetComponent<AudioSource>();
        _tempSoundObject.transform.position = engineSound.transform.position;
        while (true)
        {
            yield return wfs;
            currentEngineRPM = Mathf.Lerp(0f, 1000f, startUpSound.time / startUpSound.clip.length);
            if (!startUpSound.isPlaying)
            {
                Destroy(_tempSoundObject);
                ignition = true;
                if (!engineSound.isPlaying)
                    engineSound.Play();
                yield break;
            }
        }
    }
    private void CalculateWheelRPM()
    {
        tempWheelRPM = 0f;
        for (int i = 0; i < driveWheelsNum; i++)
        {
            tempWheelRPM += driveWheels[i].rpm;
        }
        currentWheelRPM = tempWheelRPM / driveWheelsNum;
        if (tempWheelRPM > 0f)
            reverse = false;
        else
            reverse = true;
    }
    private void CalculateTorque()
    {
        if (currentEngineRPM >= maxEngineRPM) SetEngineLerp(maxEngineRPM - 1000f);
        if(!redLine)
        {
            if (speed > gearSpeedLimit[currentGear])
                overSpeed = 4f;
            else
                overSpeed = 1f;
            if (clutch < 0.1f)
                currentEngineRPM = Mathf.Lerp(currentEngineRPM, Mathf.Max(minEngineRPM, maxEngineRPM * throttle), Time.deltaTime * 3.5f);
            else
            {
                currentEngineRPM = Mathf.Lerp
                    (
                        currentEngineRPM,
                        Mathf.Max(minEngineRPM, Mathf.Abs(currentWheelRPM) * finalDriveRatio * Mathf.Abs(gearRatio[currentGear])),
                        (overSpeed * engineAcceleration * Time.deltaTime) * Mathf.Abs(gearRatio[currentGear])
                    );
                nitroPowerMultiplier = isNitroActive ? nitroPower : 1f;
                nitroSpeedMultiplier = isNitroActive ? nitroSpeed : 1f;
                if (speed < gearSpeedLimit[currentGear] * nitroSpeed)
                    currentWheelTorque = (engineTorqueCurve.Evaluate(currentEngineRPM) * nitroPowerMultiplier * (gearRatio[currentGear] * finalDriveRatio) * clutch);
                else
                    currentWheelTorque = 0f;
            }
        }
        //엔진 토크(Engine Torque) 계산법
        //엔진 토크(Nm) = (엔진 출력(kW) × 9549) / 엔진 회전수(RPM)
        //엔진 토크(Nm) = (엔진 출력(hp) x 7121) / 엔진 회전수(RPM)
        //바퀴 토크(Wheel Torque) 계산법
        //바퀴 토크 = 엔진 토크(Engine Torque) × 변속기 기어비(Gear ratio) × 차동기어비(Differential ratio)
    }
    protected void SetEngineLerp(float _num)//레드라인 도달했을때 동작
    {
        redLine = true;
        engineLerpValue = _num;
    }
    private void forceEngineLerp()
    {
        if(redLine)
        {
            //curEngineTorque = 0f;
            currentEngineRPM = Mathf.Lerp(currentEngineRPM,engineLerpValue,20 * Time.deltaTime);
            redLine = currentEngineRPM <= engineLerpValue + 100 ? false : true;
        }
    }
    private void TorqueToWheel()
    {
        for (int i = 0; i < driveWheelsNum; i++)
        {
            driveWheels[i].motorTorque = Mathf.Abs(throttle) * (currentWheelTorque / driveWheelsNum);
        }
    }
    private void EngineSoundUpdate()
    {
        if (engineSound == null)
            return;
        engineSound.volume = Mathf.Lerp(0.3f, 0.4f, clutch);
        engineSound.pitch = Mathf.Lerp(0.1f, 1.4f, currentEngineRPM / maxEngineRPM);
    }
    #endregion

    #region Value Gear
    [Header("Gear Value")]
    [SerializeField] private eGEAR currentGear = eGEAR.eGEAR_NEUTURAL;
    [SerializeField] private eGEAR nextGear = eGEAR.eGEAR_NEUTURAL;
    [SerializeField] protected float clutch;
    [SerializeField] protected bool reverse;
    [SerializeField] private Dictionary<eGEAR, float> gearRatio = new Dictionary<eGEAR, float>();
    [SerializeField] private Dictionary<eGEAR, float> gearSpeedLimit = new Dictionary<eGEAR, float>();
    [SerializeField] private float perviousMaxSpeed = 0;
    [SerializeField] private float differentialRatio;
    [SerializeField] private float finalDriveRatio;
    [SerializeField] private eGEAR lastGear;
    [SerializeField] private bool autoGear;
    [SerializeField] private float shiftTiming;
    [SerializeField] private float shiftTimer = 0f;
    #endregion

    #region Function Gear setting
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
    public void SetDifferentialRatio(float _ratio) { differentialRatio = _ratio; }
    public void SetFinalDriveRatio(float  _ratio) {  finalDriveRatio = _ratio; }
    public void SetLastGear(eGEAR _lastGearName) { lastGear = _lastGearName; }
    public void SetAutoGear(bool _autoGear) { autoGear = _autoGear; }
    public void SetShiftTiming(float _shiftTiming) { shiftTiming = _shiftTiming; }
    #endregion

    #region Function Gear
    protected void ChangeGear(bool _up)
    {
        if (_up)
        {
            if(currentGear == lastGear /*|| (currentGear == eGEAR.eGEAR_NEUTURAL && currentGear != nextGear)*/) { return; }
            switch(currentGear)
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
            switch (currentGear)
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
    protected void ForceChangeGear(eGEAR _gear) { nextGear = _gear; }
    protected void GearShifting()
    {
        if (currentGear == nextGear)
            return;
        if (autoGear)
            shiftTiming = 0.25f;
        if (shiftTimer < shiftTiming)
        {
            currentGear = eGEAR.eGEAR_NEUTURAL;
            if(throttle > 0)
                throttle = 0f;
            clutch = 0f;
            shiftTimer += Time.deltaTime;
            //curEngineRPM = Mathf.Lerp(curEngineRPM, maxEngineRPM / 3 + minEngineRPM, Time.deltaTime * 5f);
        }
        else
        {
            currentGear = nextGear;
            currentEngineRPM = maxEngineRPM / 2 + minEngineRPM;
            shiftTimer = 0;
            switch (currentGear)
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
        if(speed > gearSpeedLimit[currentGear] -10f && currentEngineRPM >= maxEngineRPM - minEngineRPM - 500f) { ChangeGear(true); }
        else if(speed < perviousMaxSpeed + 10f && currentGear != eGEAR.eGEAR_FIRST && currentEngineRPM < maxEngineRPM / 3 + minEngineRPM) { ChangeGear(false); }
        else if (nextGear == eGEAR.eGEAR_NEUTURAL)
        {
            if (throttle > 0)
                ForceChangeGear(eGEAR.eGEAR_FIRST);
            else if (throttle < 0)
                ForceChangeGear(eGEAR.eGEAR_REVERSE);
        }
        if (throttle < 0 && speed < 0.1f && currentGear != eGEAR.eGEAR_REVERSE)
            ForceChangeGear(eGEAR.eGEAR_REVERSE);
        else if(currentGear == eGEAR.eGEAR_REVERSE && throttle > 0 && speed < 10f)
            ForceChangeGear(eGEAR.eGEAR_FIRST);
    }
    public eGEAR GetCurrentGear() { return currentGear; }
    #endregion

    #region Value Nitro
    [Header("Nitro Value")]
    [SerializeField] private AudioSource nitroSoundEffect;
    [SerializeField] private Trail nitroParticles;
    [SerializeField] private bool isNitroInstalled;
    [SerializeField] private bool powerMode = false;
    [SerializeField] private float maxNitroCapacity = 100f;
    [SerializeField] private float currentNitroAmount;
    [SerializeField] private float nitroConsumptionRate = 10f;
    [SerializeField] private float nitroRechargeRate = 5f;
    [SerializeField] private float nitroRechargeDelay = 2f;
    [SerializeField] private float nitroRechargeDelayTimer = 0f;
    [SerializeField] private float nitroPower = 1.5f;
    [SerializeField] private float nitroSpeed = 1.2f;
    [SerializeField] private float nitroPowerMultiplier;
    [SerializeField] private float nitroSpeedMultiplier;
    [SerializeField] private float nitroRechargeAmount = 5f;
    [SerializeField] private float nitroAdjustBlurWidth = 0.0f;//블러효과 조정용 최대값 1f;
    [SerializeField] private float nitroDuration = 1f;//파워모드 지속시간
    //[SerializeField] private float nitroDurationTimer = 0f;//파워모드 지속시간 타이머
    [SerializeField] private bool nitroPowerReady = true;//파워모드 지속시간 타이머 최대값
    [SerializeField] private bool isNitroActive = false;
    #endregion

    #region Function Nitro setting
    public void SetNitroParticles(Trail _trail) { nitroParticles = _trail; }
    public void SetNitroInstall(bool _isNitroInstalled)
    {
        isNitroInstalled = _isNitroInstalled;
        NitroBar.enabled = _isNitroInstalled;
    }
    public void SetNitroPowerMode(bool _powerMode) { powerMode = _powerMode; }
    public void SetMaxNitroCapacity(float _maxNitroCapacity) { maxNitroCapacity = _maxNitroCapacity; currentNitroAmount = maxNitroCapacity; }
    public void SetNitroConsumptionRate(float _nitroConsumptionRate) { nitroConsumptionRate = _nitroConsumptionRate; }
    public void SetNitroRechargeRate(float _nitroRechargeRate) { nitroRechargeRate = _nitroRechargeRate; }
    public void SetNitroRechargeDelay(float _nitroRechargeDelay) { nitroRechargeDelay = _nitroRechargeDelay; }
    public void SetNitroPowerMultiplier(float _nitroPowerMultiplier) { nitroPower = _nitroPowerMultiplier; }
    public void SetNitroRechargeAmount(float _nitroRechargeAmount) { nitroRechargeAmount = _nitroRechargeAmount; }
    public void SetNitroDuration(float _nitroDuration) { nitroDuration = _nitroDuration; }
    #endregion

    #region Function Nitro
    protected void ActivateNitro(bool _activate)
    {
        if(!isNitroInstalled) return;
        if(powerMode && !nitroPowerReady) return;
        if (_activate && currentNitroAmount > 0f)
        {
            isNitroActive = true;
            if (nitroSoundEffect != null && !nitroSoundEffect.isPlaying)
            { nitroSoundEffect.Play(); }
            if (nitroParticles != null && !nitroParticles.enabled)
            { nitroParticles.enabled = true; }
        }
        else
        {
            isNitroActive = false;
            if (nitroSoundEffect != null && nitroSoundEffect.isPlaying)
            { nitroSoundEffect.Stop(); }
            if (nitroParticles != null && nitroParticles.enabled)
            { nitroParticles.enabled = false; }
        }
    }
    public float GetCurrentNitroAmount() { return currentNitroAmount; }
    public float GetMaxNitroAmount() { return maxNitroCapacity; }
    public float GetNitroBlurWidth() { return nitroAdjustBlurWidth; }
    public bool GetPowerMode() { return powerMode; }
    public bool GetIsNitroActive() { return isNitroActive; }
    public void RefillNitro(float _amount)
    {
        currentNitroAmount += _amount;
        currentNitroAmount = Mathf.Clamp(currentNitroAmount, 0f, maxNitroCapacity);
    }
    #endregion
}
