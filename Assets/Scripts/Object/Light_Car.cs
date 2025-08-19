using System.Collections;
using UnityEngine;

public class Light_Car : MonoBehaviour
{
    [SerializeField] private Transform retractable;
    [SerializeField] private Vector3 rotation;
    [SerializeField] private float targetAngle, currentAngle, maxAngle, minAngle;
    [SerializeField] private float retractSpeed = 1.0f;
    [SerializeField] private float retractState = 0f;
    [SerializeField] private bool coroutine = false;

    [SerializeField] private Light[] lights;

    [SerializeField] private TrailRenderer[] trailRenderer;
    [SerializeField] private Rigidbody parantRB;
    [SerializeField] private Vector3 forwardVelocity, sidewaysVelocity;
    [SerializeField] private float sidewaysSpeed;
    [SerializeField] private float minDriftSpeed = 10f;

    [SerializeField] private bool isOn = false;
    [SerializeField] public bool lightOn
    {
        get => isOn;
        set
        {
            if(value != isOn)
            {
                isOn = value;
                if(retractable != null)
                {
                    targetAngle = isOn ? maxAngle : minAngle;
                    if (!coroutine)
                    {
                        coroutine = true;
                        StartCoroutine(ToggleRetractable());
                    }
                }
                else
                {
                    SetLight(isOn);
                }
            }
        }
    }

    private void Awake()
    {
        parantRB = transform.GetComponentInParent<Rigidbody>();
        if (lights != null)
        {
            foreach (Light light in lights)
            {
                light.enabled = false;
            }
        }
        if (trailRenderer != null)
            StartCoroutine(DriftLighting());
    }

    private void SetLight(bool _isOn)
    {
        if (lights != null)
        {
            foreach (Light light in lights)
            {
                if (light.enabled != _isOn)
                    light.enabled = _isOn;
            }
        }
    }

    private IEnumerator ToggleRetractable()
    {
        WaitForSeconds wait = new WaitForSeconds(Shared.frame30);
        while(coroutine)
        {
            yield return wait;
            SetLight(false);

            if (isOn)
                retractState += Shared.frame30 * retractSpeed;
            else
                retractState -= Shared.frame30 * retractSpeed;

            if(retractState < 0f)
                retractState = 0f;
            if (retractState > 1f)
                retractState = 1f;

            currentAngle = Mathf.Lerp(minAngle, maxAngle, retractState);
            rotation = Vector3.zero;
            rotation.x = currentAngle;
            retractable.localRotation = Quaternion.Euler(rotation);

            if(retractState == 1f || retractState == 0f)
                coroutine = false;
        }
        if(targetAngle == maxAngle)
            SetLight(true);
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    private IEnumerator DriftLighting()
    {
        WaitForSeconds wait = new WaitForSeconds(Shared.frame15);
        while (true)
        {
            yield return wait;
            forwardVelocity = Vector3.Dot(parantRB.velocity, transform.forward) * transform.forward;
            sidewaysVelocity = parantRB.velocity - forwardVelocity;

            // 옆방향 속도의 크기(magnitude)를 측정
            sidewaysSpeed = sidewaysVelocity.magnitude;

            // 옆방향 속도가 설정된 값보다 클 때
            if (sidewaysSpeed > minDriftSpeed)
            {
                for (int i = 0; i < trailRenderer.Length; i++)
                {
                    trailRenderer[i].emitting = true;
                }
            }
            else
            {
                for (int i = 0; i < trailRenderer.Length; i++)
                {
                    trailRenderer[i].emitting = false;
                }
            }
        }
    }
}
