using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Car
{
    //BodyÂ÷Ã¼
    [SerializeField] GameObject centerMass;
    [SerializeField] protected Rigidbody carRB;
    public float curDownforce;
    private float downForce = 50f;
    public void SetCenterMass() { carRB.centerOfMass = centerMass.transform.localPosition; }
    public void ShowCenterMass() { centerMass.transform.position = carRB.centerOfMass; }
    public void SetCarRB(Rigidbody _carRB) { carRB = _carRB; }
    public void SetCarColor(Material[] _CarColor)
    {
        MeshRenderer carMesh = body.gameObject.GetComponent<MeshRenderer>();
        carMesh.materials = _CarColor;
    }
    protected void SetSpeed()
    {
        speed = carRB.velocity.magnitude * 3.6f;//m/s to km/h
        carRB.drag = dragAmount + (speed / 4000);
        curDownforce = -Mathf.Lerp(0, 1, speed / 300f) * downForce;
        carRB.AddForce(Vector3.up * curDownforce);
        tireGrip = coreTireGrip + Mathf.Lerp(0, 1, speed / 300f);
    }
}
