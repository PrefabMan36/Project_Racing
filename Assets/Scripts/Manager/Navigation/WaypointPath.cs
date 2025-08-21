using UnityEngine;
using System.Collections.Generic;

// 트랙의 웨이포인트 경로를 관리하는 스크립트
public class WaypointPath : MonoBehaviour
{
    [Header("Racing Line Settings")]
    [Tooltip("라인을 코너 안쪽으로 얼마나 밀어낼지 결정합니다. (미터 단위)")]
    public float racingLineOffset = 3.5f;

    [Tooltip("이 값보다 곡률이 높아야 코너로 인식하고 라인을 수정합니다.")]
    [Range(0, 2)]
    public float cornerCurvatureThreshold = 0.05f;

    public List<Transform> waypoints = new List<Transform>();
    public List<float> recommendedSpeeds = new List<float>();

    void Awake()
    {
        // 자식 오브젝트들을 순서대로 웨이포인트로 등록
        GetWaypoints();
    }

    // 자식 Transform을 기반으로 웨이포인트를 자동으로 찾아 리스트에 추가
    private void GetWaypoints()
    {
        waypoints.Clear();
        // 자신의 자식 개수만큼 반복
        for (int i = 0; i < transform.childCount; i++)
        {
            waypoints.Add(transform.GetChild(i));
        }

        // 경로의 곡률을 기반으로 각 지점의 권장 속도를 계산 (예시)
        CalculateRecommendedSpeeds();
    }

    // 각 웨이포인트 지점의 권장 속도를 계산하는 함수 (고급 기능)
    public void CalculateRecommendedSpeeds()
    {
        recommendedSpeeds.Clear();
        if (waypoints.Count < 3)
        {
            // 웨이포인트가 부족하면 모두 최고 속도로 설정
            for (int i = 0; i < waypoints.Count; i++) recommendedSpeeds.Add(100f);
            return;
        }

        float maxSpeed = 120f; // 최대 권장 속도 (km/h)
        float minSpeed = 40f;  // 최소 권장 속도 (km/h)

        // 첫 번째 웨이포인트는 직선으로 간주
        recommendedSpeeds.Add(maxSpeed);

        for (int i = 1; i < waypoints.Count - 1; i++)
        {
            Vector3 prev = waypoints[i - 1].position;
            Vector3 current = waypoints[i].position;
            Vector3 next = waypoints[i + 1].position;

            // 올바른 벡터 계산: (이전->현재) 와 (현재->다음)
            Vector3 incomingVec = (current - prev).normalized;
            Vector3 outgoingVec = (next - current).normalized;

            // 두 벡터의 내적(dot product)은 두 벡터가 얼마나 같은 방향을 향하는지 나타냄
            // 직선에 가까울수록 1, 90도일 때 0, 180도 꺾이면 -1
            float dot = Vector3.Dot(incomingVec, outgoingVec);

            // 곡률 계산: (1.0 - 내적 값) -> 직선일 때 0, 급커브일수록 2에 가까워짐
            float curvature = 1.0f - dot;

            // 곡률(0~2 범위)을 이용해 권장 속도 계산
            // Mathf.Clamp01로 값을 0~1 사이로 제한하여 안정성 추가
            recommendedSpeeds.Add(Mathf.Lerp(maxSpeed, minSpeed, Mathf.Clamp01(curvature / 1.5f))); // 1.5f로 나누어 민감도 조절
        }

        // 마지막 웨이포인트는 직선으로 간주
        recommendedSpeeds.Add(maxSpeed);
    }

    // 외부에서 웨이포인트 배열을 직접 설정할 수 있도록 하는 함수
    public void SetPath(Vector3[] newPath)
    {
        // 기존 자식 웨이포인트 삭제
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        waypoints.Clear();

        // 새로운 경로점들로 웨이포인트 생성
        for (int i = 0; i < newPath.Length; i++)
        {
            GameObject wp = new GameObject($"Waypoint {i}");
            wp.transform.position = newPath[i];
            wp.transform.parent = this.transform;
            waypoints.Add(wp.transform);
        }

        // 새로운 경로에 맞춰 권장 속도 재계산
        CalculateRecommendedSpeeds();
    }

    /// <summary>
    /// 현재 웨이포인트 경로를 아웃-인-아웃 스타일의 레이싱 라인으로 변형합니다.
    /// </summary>
    private void GenerateRacingLine()
    {
        if (waypoints.Count < 3) return;

        // 계산 시 원본 위치를 참조하기 위해 현재 위치들을 복사해둡니다.
        // (수정된 포인트가 다음 포인트 계산에 영향을 주는 것을 방지)
        Vector3[] originalPositions = new Vector3[waypoints.Count];
        for (int i = 0; i < waypoints.Count; i++)
        {
            originalPositions[i] = waypoints[i].position;
        }

        // 첫 포인트와 마지막 포인트는 제외하고 순회
        for (int i = 1; i < waypoints.Count - 1; i++)
        {
            Vector3 prevPos = originalPositions[i - 1];
            Vector3 currentPos = originalPositions[i];
            Vector3 nextPos = originalPositions[i + 1];

            Vector3 incomingVec = (currentPos - prevPos).normalized;
            Vector3 outgoingVec = (nextPos - currentPos).normalized;

            float dot = Vector3.Dot(incomingVec, outgoingVec);
            float curvature = 1.0f - dot; // 0: 직선, 2: 헤어핀

            // 곡률이 설정한 임계값보다 낮으면 직선으로 간주하고 건너뜁니다.
            if (curvature < cornerCurvatureThreshold)
            {
                continue;
            }

            // 코너 방향 결정: Cross 결과의 y값 부호로 좌/우회전 판별
            // 양수(+) = 좌회전, 음수(-) = 우회전
            float turnDirection = Mathf.Sign(Vector3.Cross(incomingVec, outgoingVec).y);

            // 경로 진행 방향의 수직 벡터 (경로의 오른쪽 방향 벡터)
            Vector3 forward = (nextPos - prevPos).normalized;
            Vector3 perpendicular = Vector3.Cross(forward, Vector3.up);

            // 곡률을 0~1 범위로 정규화하여 push 강도로 사용
            // InverseLerp(최소, 최대, 현재값) -> 현재값이 최소~최대 사이에서 어느 지점인지 0~1로 반환
            float pushIntensity = Mathf.InverseLerp(cornerCurvatureThreshold, 2.0f, curvature);

            // 코너 안쪽으로 밀어낼 방향 계산
            // 우회전(turnDirection = -1)일 때: 안쪽은 오른쪽(perpendicular)
            // 좌회전(turnDirection = 1)일 때: 안쪽은 왼쪽(-perpendicular)
            // 즉, (-perpendicular * turnDirection) 벡터가 항상 코너의 안쪽을 향하게 됩니다.
            Vector3 offsetDirection = -perpendicular * turnDirection;

            // 최종적으로 웨이포인트 위치 이동
            // 곡률이 셀수록(코너가 급할수록) 더 강하게 안쪽으로 밀어냅니다.
            waypoints[i].position += offsetDirection * racingLineOffset * pushIntensity;
        }
    }

    /// <summary>
    /// 새로운 경로를 설정하고, 설정된 경로를 부드럽게 보간합니다.
    /// </summary>
    /// <param name="newPathCorners">NavMesh로부터 받은 원본 경로 코너들</param>
    /// <param name="segmentResolution">경로를 얼마나 잘게 쪼갤지에 대한 값 (m 단위)</param>
    public void SetAndRefinePath(Vector3[] newPathCorners, float segmentResolution = 2.0f)
    {
        if (newPathCorners.Length < 2) return;

        // 기존 웨이포인트 정리
        foreach (Transform child in transform) Destroy(child.gameObject);
        waypoints.Clear();

        // 보간된 포인트를 저장할 새 리스트
        List<Vector3> refinedPoints = new List<Vector3>();

        List<Vector3> controlPoints = new List<Vector3>(newPathCorners);
        controlPoints.Insert(0, newPathCorners[0]);
        controlPoints.Add(newPathCorners[newPathCorners.Length - 1]);

        for (int i = 1; i < controlPoints.Count - 2; i++)
        {
            Vector3 p0 = controlPoints[i - 1];
            Vector3 p1 = controlPoints[i];
            Vector3 p2 = controlPoints[i + 1];
            Vector3 p3 = controlPoints[i + 2];

            float segmentDistance = Vector3.Distance(p1, p2);
            int divisions = Mathf.Max(1, Mathf.CeilToInt(segmentDistance / segmentResolution));

            for (int j = 0; j < divisions; j++)
            {
                float t = (float)j / divisions;

                // 캣멀롬 스플라인 공식
                Vector3 pointOnCurve = 0.5f * (
                    (2f * p1) +
                    (-p0 + p2) * t +
                    (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t +
                    (-p0 + 3f * p1 - 3f * p2 + p3) * t * t * t
                );

                refinedPoints.Add(pointOnCurve);
            }
        }
        refinedPoints.Add(controlPoints[controlPoints.Count - 2]);

        // 세분화된 경로로 웨이포인트 GameObject 생성
        for (int i = 0; i < refinedPoints.Count; i++)
        {
            GameObject wp = new GameObject($"Waypoint {i}");
            wp.transform.position = refinedPoints[i];
            wp.transform.parent = this.transform;
            waypoints.Add(wp.transform);
        }

        CalculateRecommendedSpeeds();
        GenerateRacingLine();
    }
}