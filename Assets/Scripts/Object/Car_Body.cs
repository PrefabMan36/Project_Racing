using Fusion;
using System.Collections;
using UnityEngine;

public partial class Car
{
    #region Value Body
    [Header("Body Value")]
    [SerializeField] protected GameObject centerMass;
    [SerializeField] protected Rigidbody carRB;
    [SerializeField] protected bool flipped = false;
    [SerializeField] protected bool hitted = false;
    #endregion

    #region Value Stabilizer
    [Header("Stabilizer Value")]
    [SerializeField] private float instabilityThresholdAngle = 10f;
    [SerializeField] protected Vector3 loweredCenterOfMass;
    [SerializeField] private float comLerpSpeed = 5f;
    [SerializeField] protected Vector3 originalCenterOfMass;
    [SerializeField] Vector3 targetCoM { get; set; }
    [SerializeField] float tiltAngle;

    [SerializeField] Quaternion currentRotation;
    [SerializeField] Vector3 eulerAngles;
    [SerializeField] float normalizedX;
    [SerializeField] float normalizedZ;
    [SerializeField] float clampedX;
    [SerializeField] float clampedZ;
    [SerializeField] Quaternion newRotation;
    #endregion

    #region Function Body
    public void SetCarMass(float _mass) { carRB.mass = _mass; }
    public void SetCarRB(Rigidbody _carRB) { carRB = _carRB; }
    public void SetCenterMass() { carRB.centerOfMass = Vector3.Lerp(carRB.centerOfMass, targetCoM, Time.fixedDeltaTime * comLerpSpeed); }
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

    #region Function Stabilizer
    protected void ApplyStabilizer()
    {
        tiltAngle = Vector3.Angle(transform.up, Vector3.up);

        if (tiltAngle > instabilityThresholdAngle)
        {
            targetCoM = loweredCenterOfMass;
            //carRB.velocity *= 0.998f;
        }
        else
        {
            // 안정적인 상태이면 원래 무게 중심으로 설정합니다.
            targetCoM = originalCenterOfMass;
        }
    }
    private float NormalizeAngle(float angle)
    {
        if (angle > 180)
            angle -= 360;
        return angle;
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
    [SerializeField] protected Light_Car headLights;
    [SerializeField] protected Light_Car tailLamps;
    [Networked, SerializeField] private NetworkBool headLightSwitch { get; set; } = false;
    [Networked, SerializeField] private NetworkBool tailLampSwitch { get; set; } = false;
    protected void HeadLightSwitch() { headLightSwitch = !headLightSwitch; }
    protected void ForceLightOn() { headLightSwitch = true; }

    private void UpdateLight()
    {
        if (isBrakingIntent != tailLampSwitch)
            tailLampSwitch = isBrakingIntent;
        if (headLights != null)
        {
            headLights.lightOn = headLightSwitch;
        }
        if (tailLamps != null)
        {
            tailLamps.lightOn = tailLampSwitch;
        }
    }

    #endregion
}
