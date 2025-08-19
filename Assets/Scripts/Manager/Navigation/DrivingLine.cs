using UnityEngine;
using UnityEngine.AI;

// 주행 라인을 동적으로 그리고, 속도에 따라 색상을 변경하는 스크립트
[RequireComponent(typeof(LineRenderer))]
public class DrivingLine : MonoBehaviour
{
    public WaypointPath path; // 따라갈 경로
    public Transform car;     // 플레이어 차량
    public Rigidbody carRigidbody; // 차량의 Rigidbody 컴포넌트

    [Header("Line Settings")]
    public int lineLength = 50; // 라인을 그릴 웨이포인트 개수
    public Color safeColor = Color.green;    // 안전 속도 색상
    public Color cautionColor = Color.yellow; // 주의 속도 색상
    public Color dangerColor = Color.red;    // 위험 속도 색상

    [Header("Path Recalculation")]
    public Pathfinder pathfinder; // 경로 재탐색을 위해 Pathfinder 참조
    public float offPathThreshold = 5f; // 이 거리(m) 이상 벗어나면 경로 이탈로 간주
    public float recalculationCooldown = 2.0f; // 경로 재탐색 주기 (초)

    private float timeSinceLastRecalc = 0f;

    private LineRenderer lineRenderer;

    void Start()
    {
        path = GameObject.FindAnyObjectByType<WaypointPath>();
        pathfinder = GameObject.FindAnyObjectByType<Pathfinder>();
        car = transform;
        carRigidbody = car.GetComponent<Rigidbody>();
        lineRenderer = GetComponent<LineRenderer>();
        // 라인 렌더러의 월드 공간 좌표 사용 설정
        lineRenderer.useWorldSpace = true;
    }

    void Update()
    {
        timeSinceLastRecalc += Time.deltaTime;

        if (path == null || path.waypoints.Count < 2)
        {
            lineRenderer.positionCount = 0;
            return;
        }

        int closestIndex = GetClosestWaypointIndex();

        // 경로 이탈 감지 및 재탐색 로직
        CheckForOffPathAndRecalculate(closestIndex);

        DrawDrivingLine(closestIndex);
        UpdateLineColor(closestIndex);
    }

    private void CheckForOffPathAndRecalculate(int closestIndex)
    {
        // 쿨다운 시간이 아직 안됐거나, pathfinder가 없으면 실행 안함
        if (timeSinceLastRecalc < recalculationCooldown || pathfinder == null)
        {
            return;
        }

        float distanceToPath = Vector3.Distance(car.position, path.waypoints[closestIndex].position);

        if (distanceToPath > offPathThreshold)
        {
            Debug.Log("경로 이탈 감지! 경로를 재탐색합니다.");

            // 현재 위치에서 경로의 최종 목적지까지 가는 길을 다시 탐색
            Vector3 destination = path.waypoints[path.waypoints.Count - 1].position;
            pathfinder.UpdatePath(car.position, destination);

            // 쿨다운 타이머 초기화
            timeSinceLastRecalc = 0f;
        }
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
        lineRenderer.positionCount = Mathf.Max(Mathf.Min(lineLength, path.waypoints.Count - startIndex),0);

        Vector3[] linePoints = new Vector3[lineRenderer.positionCount];
        for (int i = 0; i < lineRenderer.positionCount; i++)
        {
            // 경로가 순환하도록 인덱스 계산
            int waypointIndex = (startIndex + i) % path.waypoints.Count;
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