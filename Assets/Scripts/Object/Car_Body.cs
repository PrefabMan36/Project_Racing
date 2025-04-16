using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Car
{
    //Body��ü
    [SerializeField] GameObject centerMass;
    [SerializeField] protected Rigidbody carRB;

    private float dragCoefficient = 0.4f; // Cd ��
    private float frontalArea = 2.0f; // A ��
    private float airDensity = 1.225f; // rho ��
    private float sqrSpeed;
    private Vector3 dragDirection;
    private float dragMagnitude;
    private Vector3 dragForce;

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
        //carRB.drag = dragAmount + (speed / 6000);
        //curDownforce = -(Mathf.Lerp(0, 1, speed / gearSpeedLimit[lastGear]) * downForce);
        //carRB.AddForce(Vector3.up * curDownforce);
    }

    protected void ApplyAerodynamicDrag()
    {
        sqrSpeed = speed * speed;
        if (speed < 0.1f) return;
        dragDirection = -carRB.velocity.normalized;
        // ���׷��� ũ�� ��� (���� ����)
        // Fd = 0.5 * rho * v^2 * Cd * A
        dragMagnitude = 0.5f * airDensity * sqrSpeed * dragCoefficient * frontalArea;
        dragForce = dragDirection * dragMagnitude;
        carRB.AddForce(dragForce, ForceMode.Force);
    }
}
