using System.Collections;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class Car : Object_Movable
{
    private float speed;
    [Networked] private int speedInt { get; set; }
    [SerializeField] protected TextMeshProUGUI speedTextForUI;
    [SerializeField] protected TextMeshProUGUI gearTextForUI;
    [SerializeField] protected Slider nitroBar;
    [SerializeField] protected RPMGauge rpmGauge;

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
                CalculateTorque();
                forceEngineLerp();
                TorqueToWheel();
                if (autoGear) AutoGear();
                EngineSoundUpdate();
            }
            else
            {
                currentEngineRPM = 0f;
                currentWheelTorque = 0f;
                if (!engineStartUP && throttle > 0)
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
                    currentNitroAmount += nitroRechargeRate * nitroRechargeAmount * Time.deltaTime;
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
        WaitForSeconds waitForSecond = new WaitForSeconds(Shared.frame15);
        while(true)
        {
            yield return waitForSecond;
            SetUI();
        }
    }
    public void SetUI()
    {
        //RPM게이지가 있는지 체크 후 속도와 RPM 갱신
        if(rpmGauge != null)
        {
            speedInt = (int)speed;
            speedTextForUI.text = speedInt.ToString();
            rpmGauge.SetValue(Mathf.Lerp(0f, 0.375f, currentEngineRPM / maxEngineRPM));
        }
        else
            Debug.LogWarning("RPM Gauge is not assigned in the inspector");
        //부스트 게이지 체크 후 부스트 잔량 갱신
        if (nitroBar != null)
            nitroBar.value = currentNitroAmount / maxNitroCapacity;
        //현재 기어를 확인하고 기어를 나타내는 텍스트 변경
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

    protected virtual void GetInputData()
    { }
}
