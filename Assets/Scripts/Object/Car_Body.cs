using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Car
{
    #region Value Body
    [Header("Body Value")]
    [SerializeField] private GameObject centerMass;
    [SerializeField] private Rigidbody carRB;
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
        carRB.AddForce(dragForce * Time.deltaTime, ForceMode.Force);
    }
    #endregion

    #region Lights
    [Header("Lights Values")]
    [SerializeField] private Light[] headLight_Object;
    [SerializeField] private Light[] headLight_SpotLight;
    [SerializeField] private Light[] tailLamp;
    [SerializeField] private bool headLightSwitch = true;
    private void SetLight()
    {
        headLight_Object = new Light[2];
        headLight_SpotLight = new Light[2];
        tailLamp = new Light[2];
    }
    protected void SetLights(Transform _gameObject)
    {
        SetLight();
        headLight_Object[0] = _gameObject.Find("HeadLight_Left").GetComponent<Light>();
        headLight_Object[1] = _gameObject.Find("HeadLight_Right").GetComponent<Light>();
        headLight_SpotLight[0] = _gameObject.Find("HeadLight_Left_Spot").GetComponent<Light>();
        headLight_SpotLight[1] = _gameObject.Find("HeadLight_Right_Spot").GetComponent<Light>();
        tailLamp[0] = _gameObject.Find("TailLamp_Left").GetComponent<Light>();
        tailLamp[1] = _gameObject.Find("TailLamp_Right").GetComponent<Light>();
    }
    protected void HeadLightSwitch()
    {
        if(headLight_Object[0] != null)
        {
            headLightSwitch = !headLightSwitch;
            foreach (Light light in headLight_Object)
                light.enabled = headLightSwitch;
            foreach (Light light in headLight_SpotLight)
                light.enabled = headLightSwitch;
        }
    }
    private void TailLampSwitch(bool _switch)
    {
        if(tailLamp[0] != null)
        {
            foreach(Light light in tailLamp)
            light.enabled = _switch;
        }
    }
    #endregion
}
