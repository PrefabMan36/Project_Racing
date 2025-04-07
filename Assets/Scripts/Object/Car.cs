using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Car : Object_Movable
{
    [SerializeField] protected float speed;
    protected string speedTextForUI;
    [SerializeField] protected ParticleSystem wheelParticle;
    protected void SetSlpingAngle(){ slipingAngle = Vector3.Angle(transform.forward, carRB.velocity - transform.forward); }
    public int GetSpeedNum(){ return (int)speed; }
    public string GetSpeedString()
    {
        speedTextForUI = speed.ToString();
        return speedTextForUI;
    }
}
