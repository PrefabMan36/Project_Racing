using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Car
{
    #region Value Body
    [Header("Body Value")]
    [SerializeField] private GameObject centerMass;
    [SerializeField] protected Rigidbody carRB;
    [SerializeField] protected bool flipped = false;
    [SerializeField] protected bool hitted = false;
    #endregion

    #region Function Body
    public void SetCarMass(float _mass) { carRB.mass = _mass; }
    public void SetCarRB(Rigidbody _carRB) { carRB = _carRB; }
    public void SetCenterMass() { carRB.centerOfMass = centerMass.transform.localPosition; }
    public void ShowCenterMass() { centerMass.transform.position = carRB.centerOfMass; }
    public void SetCarColor(Material[] _CarColor)
    {
        MeshRenderer carMesh = body.gameObject.GetComponent<MeshRenderer>();
        carMesh.materials = _CarColor;
    }
    protected void SetSpeedToKMH() { speed = carRB.velocity.magnitude * 3.6f; }

    protected void CheckFlipped()
    {
        if (carRB.transform.up.y < 0.5f)
        {
            flipped = true;
            //carRB.isKinematic = true; // 차량이 뒤집혔을 때 물리 엔진을 일시 중지
        }
        else
        {
            flipped = false;
            //carRB.isKinematic = false; // 차량이 정상 상태로 돌아오면 물리 엔진을 재개
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (carRB != null)
        {
            if (carRB.velocity.magnitude > 1f)
            {
                if (driver != null)
                {
                    driver.Hitted();
                }
            }
        }
    }
    #endregion

    #region AeroDynamicPhysics
    [Header("AeroDynamic Physics Value")]
    [SerializeField] private float dragCoefficient = 0.4f; // Cd 값
    [SerializeField] private float frontalArea = 2.0f; // A 값
    [SerializeField] private float airDensity = 1.225f; // rho 값
    [SerializeField] private float sqrSpeed;
    [SerializeField] private Vector3 dragDirection;
    [SerializeField] private float dragMagnitude;
    [SerializeField] private Vector3 dragForce;

    protected void SetSlpingAngle() { slipingAngle = Vector3.Angle(transform.forward, carRB.velocity - transform.forward); }
    public void SetDragCoefficient(float _Cd) { dragCoefficient = _Cd; }
    protected void ApplyAerodynamicDrag()
    {
        sqrSpeed = speed * speed;
        if (speed < 0.1f) return;
        dragDirection = -carRB.velocity.normalized;
        // 저항력의 크기 계산 (공식 적용)
        // Fd = 0.5 * rho * v^2 * Cd * A
        dragMagnitude = 0.5f * airDensity * sqrSpeed * dragCoefficient * frontalArea;
        dragForce = dragDirection * dragMagnitude;
        carRB.AddForce(dragForce * Runner.DeltaTime, ForceMode.Force);
    }
    #endregion

    #region Lights
    [Header("Lights Values")]
    [SerializeField] private Light_Car headLights;
    [SerializeField] private Light_Car tailLamps;
    [SerializeField] private bool headLightSwitch = false;
    protected void HeadLightSwitch()
    {
        if(headLights != null)
        {
            headLightSwitch = !headLightSwitch; // 토글 스위치
            headLights.lightOn = headLightSwitch; // 라이트 상태 변경
        }
    }

    protected void ForceLightOn()
    {
        if(headLights != null)
        {
            headLightSwitch = true; // 강제로 라이트 켜기
            headLights.lightOn = headLightSwitch; // 라이트 상태 변경
        }
    }

    private void TailLampSwitch(bool _switch)
    {
        if(tailLamps != null)
        {
            tailLamps.lightOn = _switch; // 테일램프 상태 변경
        }
    }
    #endregion
}
