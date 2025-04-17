using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class Car : Object_Movable
{
    private float speed;
    private int speedInt;
    protected Text speedTextForUI;
    protected Text gearTextForUI;
    protected RPMGauge rpmGauge;

    protected void SetSlpingAngle(){ slipingAngle = Vector3.Angle(transform.forward, carRB.velocity - transform.forward); }
    public int GetSpeedNum(){ return (int)speed; }
    public float GetSpeed() { return speed; }
    protected IEnumerator Engine()
    {
        WaitForSeconds waitForSecond = new WaitForSeconds(0.01f);
        while (true)
        {
            yield return waitForSecond;
            if (ignition)
            {
                GearShifting();
                CalculateWheelRPM();
                CalculateTorque();
                forceEngineLerp();
                TorqueToWheel();
                if (autoGear) AutoGear();
                EngineSoundUpdate();
            }
            else
            {
                engineSound.Stop();
                currentEngineRPM = 0f;
                currentWheelTorque = 0f;
                if (!engineStartUP)
                    StartCoroutine(IgnitionEngine());
            }
        }
    }
    protected IEnumerator UpdateNitro()
    {
        WaitForSeconds wfs = new WaitForSeconds(0.02f);
        if (!isNitroInstalled) yield break;
        while (true)
        {
            yield return wfs;
            if (isNitroActive)
            {
                currentNitroAmount -= nitroConsumptionRate * Time.deltaTime;
                currentNitroAmount = Mathf.Max(0f, currentNitroAmount);
                nitroRechargeDelayTimer = 0f;
                nitroAdjustBlurWidth = 1f;
                if (currentNitroAmount <= 0f)
                {
                    ActivateNitro(false);
                    if (powerMode)
                        nitroPowerReady = false;
                }
            }
            else
            {
                nitroAdjustBlurWidth = 0f;
                if (nitroRechargeDelayTimer < nitroRechargeDelay)
                { nitroRechargeDelayTimer += Time.deltaTime; }
                else
                {
                    currentNitroAmount += nitroRechargeRate * Time.deltaTime;
                    currentNitroAmount = Mathf.Min(maxNitroCapacity, currentNitroAmount);
                    if (currentNitroAmount >= maxNitroCapacity)
                    {
                        currentNitroAmount = maxNitroCapacity;
                        nitroRechargeDelayTimer = 0f;
                        if (powerMode)
                            nitroPowerReady = true;
                    }
                }
            }
        }
    }
    protected IEnumerator UIUpdating()
    {
        WaitForSeconds waitForSecond = new WaitForSeconds(0.04f);
        while(true)
        {
            yield return waitForSecond;
            SetUI();
        }
    }
    public void SetUI()
    {
        speedInt = (int)speed;
        speedTextForUI.text = speedInt.ToString();
        rpmGauge.SetValue(Mathf.Lerp(0f, 0.375f, currentEngineRPM / maxEngineRPM));
        switch (currentGear)
        {
            case eGEAR.eGEAR_NEUTURAL:
                gearTextForUI.text = "N";
                break;
            case eGEAR.eGEAR_REVERSE:
                gearTextForUI.text = "R";
                break;
            case eGEAR.eGEAR_FIRST:
                gearTextForUI.text = "1";
                break;
            case eGEAR.eGEAR_SECOND:
                gearTextForUI.text = "2";
                break;
            case eGEAR.eGEAR_THIRD:
                gearTextForUI.text = "3";
                break;
            case eGEAR.eGEAR_FOURTH:
                gearTextForUI.text = "4";
                break;
            case eGEAR.eGEAR_FIFTH:
                gearTextForUI.text = "5";
                break;
            case eGEAR.eGEAR_SIXTH:
                gearTextForUI.text = "6";
                break;
        }
    }
}
