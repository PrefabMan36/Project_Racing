using System.Collections;
using UnityEngine;
using static Fusion.Editor.FusionHubWindow;

public class Light_Car : MonoBehaviour
{
    [SerializeField] private Transform retractable;
    [SerializeField] private Vector3 rotation;
    [SerializeField] private float targetAngle, currentAngle, maxAngle, minAngle;
    [SerializeField] private float retractSpeed = 1.0f;
    [SerializeField] private float retractState = 0f;
    [SerializeField] private bool coroutine = false;

    [SerializeField] private Light[] lights;

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
        if (lights != null)
        {
            foreach (Light light in lights)
            {
                light.enabled = false;
            }
        }
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
}
