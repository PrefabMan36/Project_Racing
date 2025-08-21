using Fusion;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class Car : Object_Movable
{
    protected float speed;

    public bool SetUpFinished { get; set; } = false;

    private bool waitingForRaceStart = false;
    [Networked] private int speedInt { get; set; }
    [SerializeField] protected TextMeshProUGUI speedTextForUI;
    [SerializeField] protected TextMeshProUGUI gearTextForUI;
    [SerializeField] protected Slider nitroBar;
    [SerializeField] protected RPMGauge rpmGauge;

    [SerializeField] protected Driver driver;

    public int GetSpeedNum(){ return (int)speed; }
    public float GetSpeed() { return speed; }
    protected IEnumerator Engine()
    {
        WaitForSeconds waitForSecond = new WaitForSeconds(Shared.frame30);
        while (true)
        {
            yield return waitForSecond;
            if (ignition)
            {
                DetectCounterSteering();
                GearShifting();
                UpdateNitro();
                CalculateTorque();
                forceEngineLerp();
                TorqueToWheel();
                if (autoGear) AutoGear();
            }
            else
            {
                currentEngineRPM = 0f;
                currentWheelTorque = 0f;
                //if (!engineStartUP && throttle > 0)
                //    StartCoroutine(IgnitionEngine());
            }
        }
    }
    protected IEnumerator UpdateWheels()
    {
        WaitForSeconds waitForSecond = new WaitForSeconds(Shared.frame30);
        while (true)
        {
            yield return waitForSecond;
            AntiRollBar();
            SetSlpingAngle();// 슬립 각도를 설정합니다.
            Steering();
            Braking();// 브레이크를 적용합니다.
            UpdatingFriction();
            CalculateDrift();
        }
    }
    protected IEnumerator UpdateBody()
    {
        WaitForSeconds waitForSecond = new WaitForSeconds(Shared.frame30);
        while (true)
        {
            yield return waitForSecond;
            SetCenterMass();
            ApplyStabilizer();
            ApplyAerodynamicDrag();
        }
    }

    protected void PhysicsForNetworkUpdate()
    {
        DetectCounterSteering();
        GearShifting();
        UpdateNitro();
        CalculateTorque();
        forceEngineLerp();
        TorqueToWheel();
        if (autoGear) AutoGear();
        AntiRollBar();
        SetSlpingAngle();// 슬립 각도를 설정합니다.
        Steering();
        Braking();// 브레이크를 적용합니다.
        UpdatingFriction();
        CalculateDrift();
        SetCenterMass();
        ApplyStabilizer();
        ApplyAerodynamicDrag();
    }

    protected IEnumerator UpdateVisual()
    {
        WaitForSeconds waitForSecond = new WaitForSeconds(Shared.frame15);
        while (true)
        {
            yield return waitForSecond;
            EngineSoundUpdate();
            UpdatingWheels();
            UpdateLight();
            ActivateNitro();
            NitroEffect();
            DriftEffect();
        }
    }
    protected IEnumerator UIUpdating()
    {
        WaitForSeconds waitForSecond = new WaitForSeconds(Shared.frame30);
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
            rpmGauge.SetValue(Mathf.Lerp(0f, 0.375f, networkRPM / maxEngineRPM));
        }
        else
            Debug.LogWarning("RPM Gauge is not assigned in the inspector");
        //부스트 게이지 체크 후 부스트 잔량 갱신
        if (nitroBar != null)
            nitroBar.value = currentNitroAmount / maxNitroCapacity;
        //현재 기어를 확인하고 기어를 나타내는 텍스트 변경
        switch (networkGear)
        {
            case 0:
                gearTextForUI.text = "N";
                break;
            case -1:
                gearTextForUI.text = "R";
                break;
            case 1:
                gearTextForUI.text = "1";
                break;
            case 2:
                gearTextForUI.text = "2";
                break;
            case 3:
                gearTextForUI.text = "3";
                break;
            case 4:
                gearTextForUI.text = "4";
                break;
            case 5:
                gearTextForUI.text = "5";
                break;
            case 6:
                gearTextForUI.text = "6";
                break;
        }
    }
    public void EngineStop()
    {
        if(LobbyPlayer.localPlayer.finished)
            ignition = false;
    }
    protected virtual void GetInputData()
    { }
}
