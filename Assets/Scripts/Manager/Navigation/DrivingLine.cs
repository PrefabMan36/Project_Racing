using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AI;

// 주행 라인을 동적으로 그리고, 속도에 따라 색상을 변경하는 스크립트
[RequireComponent(typeof(LineRenderer))]
public class DrivingLine : MonoBehaviour
{
    public WaypointPath path; // 따라갈 경로
    public Transform car;     // 플레이어 차량
    public Rigidbody carRigidbody; // 차량의 Rigidbody 컴포넌트

    // RaceManager로부터 트랙 타입을 전달받을 변수
    [HideInInspector]
    public eTRACKTYPE currentTrackType;

    [Header("Line Settings")]
    public int lineLength = 50; // 라인을 그릴 웨이포인트 개수
    public Color safeColor = Color.green;    // 안전 속도 색상
    public Color cautionColor = Color.yellow; // 주의 속도 색상
    public Color dangerColor = Color.red;    // 위험 속도 색상

    [Header("Path Recalculation")]
    private WaypointManager waypointManager;
    private int currentDisplayingRouteId = -1;

    private LineRenderer lineRenderer;

    private int currentRouteId = -1;

    void Start()
    {
        waypointManager = FindObjectOfType<WaypointManager>();
        path = FindObjectOfType<WaypointPath>();

        car = transform.parent;
        carRigidbody = car.GetComponent<Rigidbody>();
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;

        if (waypointManager == null) Debug.LogError("씬에 WaypointManager가 없습니다!");
        if (path == null) Debug.LogError("씬에 WaypointPath가 없습니다!");
        currentTrackType = waypointManager.GetTracktype();
    }

    private void Update()
    {
        if (waypointManager == null || path == null || car == null) return;

        // 1. 자동차 위치를 기반으로 현재 가장 가까운 경로 ID를 찾습니다.
        int detectedRouteId = waypointManager.GetClosestRouteID(currentRouteId, car.position);

        // 2. 이전에 그리던 경로와 다른 경로가 감지되면, 경로를 새로 설정합니다.
        if (detectedRouteId != -1 && detectedRouteId != currentDisplayingRouteId)
        {
            Debug.Log($"새로운 경로(ID: {detectedRouteId}) 감지! 주행 라인을 업데이트합니다.");
            currentDisplayingRouteId = detectedRouteId;

            // WaypointManager로부터 새로운 경로 데이터를 가져옵니다.
            List<Vector3> routePoints = waypointManager.GetRouteData(currentDisplayingRouteId);
            if (routePoints != null && routePoints.Count > 0)
            {
                // WaypointPath에 새로운 경로를 설정하여 웨이포인트들을 생성하고 라인을 다듬도록 합니다.
                path.SetAndRefinePath(routePoints, currentTrackType == eTRACKTYPE.eTRACK_TYPE_CIRCUIT);
            }
        }

        if (path.waypoints.Count < 2)
        {
            lineRenderer.positionCount = 0;
            return;
        }

        // 3. 가장 가까운 웨이포인트를 찾아 라인을 그리고 색상을 업데이트합니다. (기존 로직 재사용)
        int closestIndex = GetClosestWaypointIndex();
        DrawDrivingLine(closestIndex);
        UpdateLineColor(closestIndex);
    }

    // 가장 가까운 웨이포인트의 인덱스를 찾는 함수
    private int GetClosestWaypointIndex()
    {
        int closestIndex = 0;
        float minDistance = float.MaxValue;

        for (int i = 0; i < path.waypoints.Count; i++)
        {
            float distance = Vector3.Distance(car.position, path.waypoints[i].position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestIndex = i;
            }
        }
        return closestIndex;
    }

    // 주행 라인을 그리는 함수
    private void DrawDrivingLine(int startIndex)
    {
        lineRenderer.positionCount = Mathf.Max(Mathf.Min(lineLength, path.waypoints.Count), 0);
        if (currentTrackType != eTRACKTYPE.eTRACK_TYPE_CIRCUIT)
        {
            lineRenderer.positionCount = Mathf.Max(Mathf.Min(lineLength, path.waypoints.Count - startIndex), 0);
        }

        Vector3[] linePoints = new Vector3[lineRenderer.positionCount];
        for (int i = 0; i < lineRenderer.positionCount; i++)
        {
            int waypointIndex;
            if (currentTrackType == eTRACKTYPE.eTRACK_TYPE_CIRCUIT)
            {
                waypointIndex = (startIndex + i) % path.waypoints.Count;
            }
            else
            {
                waypointIndex = startIndex + i;
                if (waypointIndex >= path.waypoints.Count)
                {
                    lineRenderer.positionCount = i;
                    break;
                }
            }
            linePoints[i] = path.waypoints[waypointIndex].position;
        }
        lineRenderer.SetPositions(linePoints);
    }

    // 라인 색상을 업데이트하는 함수
    private void UpdateLineColor(int startIndex)
    {
        // 현재 차량 속도 (m/s -> km/h 변환)
        float currentSpeed = carRigidbody.velocity.magnitude * 3.6f;

        // 앞으로 다가올 코너의 평균 권장 속도 계산
        float avgRecommendedSpeed = 0;
        int samples = Mathf.Min(lineLength, path.recommendedSpeeds.Count - startIndex);
        if (samples == 0) return;

        for (int i = 0; i < samples; i++)
        {
            int waypointIndex = (startIndex + i) % path.recommendedSpeeds.Count;
            avgRecommendedSpeed += path.recommendedSpeeds[waypointIndex];
        }
        avgRecommendedSpeed /= samples;

        // 속도 비율 계산 (현재 속도 / 권장 속도)
        float speedRatio = currentSpeed / avgRecommendedSpeed;

        // 비율에 따라 색상 보간
        Color targetColor;
        if (speedRatio < 1.0f) // 안전
        {
            targetColor = Color.Lerp(safeColor, cautionColor, speedRatio);
        }
        else // 위험
        {
            // 1.0(주의) ~ 1.5(위험) 사이의 값을 0~1로 정규화
            targetColor = Color.Lerp(cautionColor, dangerColor, Mathf.Clamp01((speedRatio - 1.0f) * 2.0f));
        }

        lineRenderer.startColor = targetColor;
        lineRenderer.endColor = targetColor;
    }
}