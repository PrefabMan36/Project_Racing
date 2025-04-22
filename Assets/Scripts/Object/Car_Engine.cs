using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tiny;
using Unity.VisualScripting;

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
    [SerializeField] protected AudioSource[] engineSound;
    [SerializeField] private GameObject startUpSoundObject;
    private GameObject _tempSoundObject;
    private AudioSource startUpSound;

    [Header("Value TCS")]
    [SerializeField] private bool isTCSEnabled = true; // TCS 사용 여부
    [SerializeField, Range(0.05f, 1.0f)] private float tcsSlipThreshold = 0.25f; // TCS 개입을 시작할 Forward Slip 임계값
    [SerializeField, Range(0.0f, 1.0f)] private float tcsTorqueReductionFactor = 1.0f; // TCS 개입 강도 (1이면 슬립 시 토크 0, 낮을수록 약하게 개입)
    [SerializeField] private float tcsBrake = 0f;
    [SerializeField] private float tcsBrakeFactor = 50f;
    [SerializeField] private float slipFactorTCS;
    [SerializeField] private float baseTorquePerWheel;
    [SerializeField] private float appliedTorque;
    #endregion

    #region Value Engine
    [Header("Engine Value")]
    [SerializeField] private AnimationCurve horsePowerCurve;
    [SerializeField] private AnimationCurve engineTorqueCurve;
    [SerializeField] protected float throttle;
    [SerializeField] private float baseEngineAcceleration, currentDynamicEngineAcceleration;
    [SerializeField] private float maxHorsePower = 0f, rpmAtMaxHorsePower = 0f;
    [SerializeField] private float maxEngineRPM, minEngineRPM, currentEngineRPM, targetRPM, tempWheelRPM, currentWheelRPM;
    [SerializeField] private float currentEngineTorque, currentWheelTorque;
    [SerializeField] private float overSpeed;
    [SerializeField] private bool redLine = false;
    [SerializeField] private float engineLerpValue;
    [SerializeField] private bool isEngineBrakingEnabled = true;
    [SerializeField] private float engineBrakingFactor = 800f;
    [SerializeField] private float engineBrakeEffect;
    #endregion

    #region Function Engine setting
    public void SetEngineCurves(AnimationCurve _horsePowerCurve,  AnimationCurve _engineTorqueCurve)
    {
        horsePowerCurve = _horsePowerCurve;
        engineTorqueCurve = _engineTorqueCurve;
        CalculateOptimalShiftPoints();
    }
    public void SetBaseEngineAcceleration(float _engineAcceleration) { baseEngineAcceleration = _engineAcceleration; }
    public void SetEngineRPMLimit(float _maxEngineRPM, float _minEngineRPM) { maxEngineRPM = _maxEngineRPM; minEngineRPM = _minEngineRPM; }
    public void SetMaxEngineRPM(float _maxRPM) { maxEngineRPM = _maxRPM; }
    public void SetEngineSound(AudioSource[] _engineSound) { engineSound = _engineSound; }
    #endregion

    #region Function Engine
    IEnumerator IgnitionEngine()
    {
        WaitForSeconds wfs = new WaitForSeconds(0.04f);
        engineStartUP = true;
        if (ignition)
        {
            ignition = false;
            foreach (var sound in engineSound)
            {
                if (sound.isPlaying)
                    sound.Stop();
            }
            yield break;
        }
        if (startUpSound == null ) yield break;
        _tempSoundObject = Instantiate(startUpSoundObject);
        startUpSound = _tempSoundObject.GetComponent<AudioSource>();
        _tempSoundObject.transform.position = engineSound[0].transform.position;
        while (true)
        {
            yield return wfs;
            currentEngineRPM = Mathf.Lerp(0f, 1000f, startUpSound.time / startUpSound.clip.length);
            if (!startUpSound.isPlaying)
            {
                Destroy(_tempSoundObject);
                ignition = true;
                foreach (var sound in engineSound)
                {
                    if (!sound.isPlaying)
                        sound.Play();
                }
                yield break;
            }
        }
    }
    protected void CalculateOptimalShiftPoints()
    {
        if (horsePowerCurve == null || horsePowerCurve.length < 2)
        {
            Debug.LogError("Horsepower curve is not set or invalid. Using default shift points.");
            maxHorsePower = 100f;
            rpmAtMaxHorsePower = maxEngineRPM * 0.75f;
            optimalShiftUpRPM = maxEngineRPM * 0.85f;
            optimalShiftDownRPM = maxEngineRPM * 0.3f;
            return;
        }
        maxHorsePower = 0f;
        foreach(var key in horsePowerCurve.keys)
        {
            if(key.value > maxHorsePower)
            {
                maxHorsePower = key.value;
                rpmAtMaxHorsePower = key.time;
            }
        }
        optimalShiftUpRPM = Mathf.Clamp(rpmAtMaxHorsePower * 0.95f, minEngineRPM + 500f, maxEngineRPM - 500f);
        optimalShiftDownRPM = Mathf.Clamp(rpmAtMaxHorsePower * 0.5f, minEngineRPM + 200f, maxEngineRPM);
        Debug.Log($"Optimal Shift Points Calculated: MaxHP={maxHorsePower} at RPM={rpmAtMaxHorsePower}, ShiftUpRPM={optimalShiftUpRPM}, ShiftDownRPM={optimalShiftDownRPM}");
    }
    private float GetPowerFactor(float _RPM)
    {
        if (horsePowerCurve == null || horsePowerCurve.length < 1 || maxHorsePower <= 0)
        {
            return 0.5f; // 커브 없으면 중간값 반환
        }
        // 현재 RPM의 마력을 최대 마력으로 나누어 정규화 (최대값이 1이 되도록)
        float currentHP = horsePowerCurve.Evaluate(currentEngineRPM);
        return Mathf.Clamp01(currentHP / maxHorsePower); // 0~1 사이 값으로 제한
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
            currentDynamicEngineAcceleration = baseEngineAcceleration * Mathf.Max(0.15f, GetPowerFactor(currentEngineRPM));
            overSpeed = (speed > gearSpeedLimit[currentGear]) ? 4f : 1f;
            if (clutch < 0.1f)
            {
                targetRPM = Mathf.Max(minEngineRPM, maxEngineRPM * throttle);
                currentEngineRPM = Mathf.Lerp(currentEngineRPM, targetRPM, Time.deltaTime * currentDynamicEngineAcceleration * 2f);
                currentWheelTorque = 0f;
            }
            else
            {
                CalculateWheelRPM();
                targetRPM = 
                    Mathf.Max
                    (
                        minEngineRPM,
                        Mathf.Abs(currentWheelRPM) * finalDriveRatio * Mathf.Abs(gearRatio[currentGear])
                    );
                currentEngineRPM = Mathf.Lerp
                    (
                        currentEngineRPM,
                        targetRPM,
                        (overSpeed * currentDynamicEngineAcceleration * Time.deltaTime) * Mathf.Abs(gearRatio[currentGear])
                    );
                nitroPowerMultiplier = isNitroActive ? nitroPower : 1f;
                nitroSpeedMultiplier = isNitroActive ? nitroSpeed : 1f;
                if (speed < gearSpeedLimit[currentGear] * nitroSpeed)
                    currentWheelTorque =
                        (
                            engineTorqueCurve.Evaluate(currentEngineRPM) *
                            nitroPowerMultiplier * (gearRatio[currentGear] *finalDriveRatio)
                            * clutch
                        );
                else
                    currentWheelTorque = 0f;
                if
                (
                    isEngineBrakingEnabled &&
                    throttle < 0.05f &&
                    clutch > 0.5f &&
                    currentGear != eGEAR.eGEAR_NEUTURAL &&
                    //currentGear != eGEAR.eGEAR_REVERSE &&
                    currentEngineRPM > minEngineRPM + 500
                )
                {
                    engineBrakeEffect =
                        (currentEngineRPM / maxEngineRPM) *
                        engineBrakingFactor * Mathf.Abs(gearRatio[currentGear]);
                        currentWheelTorque -= engineBrakeEffect;
                }
                if (throttle > 0.1f && currentWheelTorque < 0)
                    currentWheelTorque = 0;
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
        // 엔진 제동이 활성화되어 스로틀이 꺼져 있을 때 음의 토크를 허용합니다.
        // 그렇지 않으면 Mathf.Abs(스로틀) * 휠당 토크를 사용합니다.
        baseTorquePerWheel = (driveWheelsNum > 0) ? currentWheelTorque / driveWheelsNum : 0;
        //if (driveWheelsNum > 0)
        //{ baseTorquePerWheel = Mathf.Abs(throttle) * (currentWheelTorque / driveWheelsNum); }
        //else return;
        for (int i = 0; i < driveWheelsNum; i++)
        {
            //적용 모터토크 = baseTorquePerWheel;
            //기본 토크 결정: 엔진 제동 시 음수, 스로틀에 의해 양의 스케일이 조정되지 않는 경우
            if (baseTorquePerWheel < 0 && throttle < 0.1f && isEngineBrakingEnabled) // Engine braking case
            {
                appliedTorque = baseTorquePerWheel; // 음의 토크를 직접 적용합니다
                driveWheels[i].brakeTorque = 0f; // TCS에서 충돌하는 브레이크가 없는지 확인합니다
            }
            else // 가속 또는 정상적인 코스팅/페달 제동
            {
                appliedTorque = Mathf.Max(0, throttle) * baseTorquePerWheel; // 스로틀에 의해 조정된 비음의 토크를 보장합니다
            }
            // --- TCS 논리 통합 ---
            float tcsBrake = 0f; // 이 휠의 TCS 브레이크 힘 초기화
            if (isTCSEnabled && throttle > 0.1f && appliedTorque > 0 && clutch > 0.5f) // 가속 중 양의 토크에만 TCS 적용
            {
                if (driveWheels[i].GetGroundHit(out WheelHit wheelHitInfo)) // WheelHit에 로컬 변수 사용
                {
                    // 가속도 제어를 위해 전진 슬립을 확인합니다
                    if (Mathf.Abs(wheelHitInfo.forwardSlip) > tcsSlipThreshold)
                    {
                        // 미끄러질 경우 모터 토크를 크게 줄입니다
                        appliedTorque *= (1.0f - tcsTorqueReductionFactor);
                        // 선택 사항: 미끄러짐 휠에 작은 제동력을 가합니다
                        tcsBrake = tcsBrakeFactor * Mathf.Abs(wheelHitInfo.forwardSlip - tcsSlipThreshold); // Scale brake by slip amount
                    }
                }
            }
            //if (isTCSEnabled && clutch >= 0.9f && throttle > 0.1f)
            //{
            //    if (driveWheels[i].GetGroundHit(out wheelHit))
            //    {
            //        if (wheelHit.forwardSlip > tcsSlipThreshold && wheelHit.forwardSlip > Mathf.Abs(wheelHit.sidewaysSlip))
            //        {
            //            slipFactorTCS = Mathf.Clamp01((wheelHit.forwardSlip - tcsSlipThreshold) / (1.0f - tcsSlipThreshold)); // 슬립 초과량 정규화
            //            appliedTorque *= (1.0f - slipFactorTCS * tcsStrength);
            //            appliedTorque = Mathf.Max(0, appliedTorque);
            //        }
            //    }
            //}
            driveWheels[i].motorTorque = appliedTorque;
            if (tcsBrake > 0)
            {
                // TCS 브레이크를 추가하지만 ABS 또는 플레이어 브레이크가 더 강하면 오버라이드하지 마십시오
                // 덮어쓰기 전에 기존 브레이크 토크를 확인합니다(안전한 접근 방식)
                driveWheels[i].brakeTorque = Mathf.Max(driveWheels[i].brakeTorque, tcsBrake);
            }
            // 중요: TCS 브레이크를 적용하지 않을 경우, 휠 브레이크가 ABS/브레이크 기능에 의해 관리되는지 확인합니다
            // 브레이크() 기능은 브레이크 입력이 0일 때 브레이크 토크 재설정을 처리해야 합니다.
        }
    }
    private void EngineSoundUpdate()
    {
        if (engineSound == null)
            return;
        foreach (var sound in engineSound)
        {
            if (sound.isPlaying)
            {
                sound.volume = Mathf.Lerp(0.3f, 0.4f, clutch);
                sound.pitch = Mathf.Lerp(1.0f, 2.2f, currentEngineRPM / maxEngineRPM);
            }
        }
    }
    protected void ForcePlayEngineSound()
    {
        if (engineSound == null)
            return;
        foreach (var sound in engineSound)
        {
            if (!sound.isPlaying)
                sound.Play();
        }
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
    [SerializeField] private float optimalShiftUpRPM = 0f, optimalShiftDownRPM = 0f;
    [SerializeField] private float speedThresholdForUpshift;
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
    protected void ForceChangeGear(eGEAR _gear)
    {
        if(lastGear == eGEAR.eGEAR_FIFTH && _gear == eGEAR.eGEAR_SIXTH) return;
        nextGear = _gear; 
    }
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
        if(!IsGrounded() || currentGear == eGEAR.eGEAR_NEUTURAL || shiftTimer > 0f) return;

        speedThresholdForUpshift = 
            currentGear == eGEAR.eGEAR_FIRST ?
            gearSpeedLimit[eGEAR.eGEAR_FIRST] / 2f :
            throttle > 0.9f ?
            gearSpeedLimit[currentGear] * 0.8f : gearSpeedLimit[currentGear] * 0.6f;

        // Up-Shift 조건: 다음 기어로 변속해도 괜찮은 속도 && 현재 RPM이 마력 기반 Up 시점 도달
        // (속도 조건은 급격한 저속 고 RPM 상태에서 불필요한 변속 방지용으로 추가 가능)
        if (currentGear < lastGear && currentEngineRPM > optimalShiftUpRPM && speed > speedThresholdForUpshift)
        { ChangeGear(true); }

        // Down-Shift 조건: 현재 RPM이 마력 기반 Down 시점 미만 && 첫번째 기어가 아님
        // (브레이크/감속 중인지 확인하는 로직 추가하면 더 자연스러움)
        else if (currentGear > eGEAR.eGEAR_FIRST && currentEngineRPM < optimalShiftDownRPM && speed < perviousMaxSpeed)
        {/*if(throttle <= 0.1f)*/ ChangeGear(false); }

        //자동 중립에서 시작 로직
        if (nextGear == eGEAR.eGEAR_NEUTURAL)
        {
            if (throttle > 0)
                ForceChangeGear(eGEAR.eGEAR_FIRST);
            else if (throttle < 0)
                ForceChangeGear(eGEAR.eGEAR_REVERSE);
        }

        //자동 후진 로직
        if (speed < 1.0f) // 매우 느릴 때만 방향 전환 고려
        {
            if (throttle < -0.1f && currentGear != eGEAR.eGEAR_REVERSE)
                ForceChangeGear(eGEAR.eGEAR_REVERSE); // 후진 입력 시 후진 기어
            else if (throttle > 0.1f && currentGear == eGEAR.eGEAR_REVERSE)
                ForceChangeGear(eGEAR.eGEAR_FIRST); // 후진 중 전진 입력 시 1단 기어
        }
        //if (throttle < 0 && speed < 0.1f && currentGear != eGEAR.eGEAR_REVERSE)
        //    ForceChangeGear(eGEAR.eGEAR_REVERSE);
        //else if(currentGear == eGEAR.eGEAR_REVERSE && throttle > 0 && speed < 10f)
        //    ForceChangeGear(eGEAR.eGEAR_FIRST);
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
