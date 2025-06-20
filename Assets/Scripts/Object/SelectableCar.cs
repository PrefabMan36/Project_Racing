using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectableCar : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private Quaternion initialRotation;
    [SerializeField] private bool rotating;

    public void SetInitialRotation()
    {
        initialRotation = transform.rotation;
    }

    /// <summary>
    /// 이 차량이 선택되었을때 호출됩니다,
    /// </summary>
    public void OnSelected()
    {
        if (!rotating)
        {
            rotating = true;
            StartCoroutine(ObjectRotate());
        }
    }

    /// <summary>
    /// 이 차량의 선택이 해제되었을 때 호출됩니다.
    /// </summary>
    public void OnDeselected()
    {
        rotating = false;
    }

    IEnumerator ObjectRotate()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(Shared.frame30);
        while(rotating)
        {
            yield return waitForSeconds;
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
        }
        transform.rotation = initialRotation;
    }
}
