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
    [SerializeField] protected ParticleSystem wheelParticle;
    protected void SetSlpingAngle(){ slipingAngle = Vector3.Angle(transform.forward, carRB.velocity - transform.forward); }
    public int GetSpeedNum(){ return (int)speed; }
    public float GetSpeed() { return speed; }
    public void SetUI()
    {
        speedInt = (int)speed;
        speedTextForUI.text = speedInt.ToString();
        switch(curGear)
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
