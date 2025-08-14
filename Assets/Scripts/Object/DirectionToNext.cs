using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionToNext : MonoBehaviour
{
    [SerializeField] private Transform arrow;
    [SerializeField] private Transform nextCheckpoint;

    [SerializeField] Vector3 direction;
    [SerializeField] Quaternion targetRotation;

    public void SetNextCheckPoint(Transform checkPoint)
    {
        if (checkPoint == null)
        {
            Debug.LogError("Next checkpoint is null.");
            return;
        }
        nextCheckpoint = checkPoint;
    }

    private void Update()
    {
        if (nextCheckpoint == null)
        {
            Debug.LogWarning("Next checkpoint is not set.");
            return;
        }
        direction = (nextCheckpoint.position - transform.position).normalized;
        targetRotation = Quaternion.LookRotation(direction);

        arrow.rotation = Quaternion.Slerp(arrow.rotation, targetRotation, Time.deltaTime * 5f);
    }
}
