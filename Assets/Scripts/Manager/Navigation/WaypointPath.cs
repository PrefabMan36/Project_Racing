using UnityEngine;
using System.Collections.Generic;

// 트랙의 웨이포인트 경로를 관리하는 스크립트
public class WaypointPath : MonoBehaviour
{
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
        if (waypoints.Count < 3) return;

        // 예시: 곡률이 클수록(코너가 급할수록) 권장 속도를 낮게 설정
        for (int i = 0; i < waypoints.Count; i++)
        {
            Vector3 prev = waypoints[i == 0 ? waypoints.Count - 1 : i - 1].position;
            Vector3 current = waypoints[i].position;
            Vector3 next = waypoints[(i + 1) % waypoints.Count].position;

            Vector3 toPrev = (prev - current).normalized;
            Vector3 toNext = (next - current).normalized;

            // 두 벡터의 내적을 이용해 각도를 구하고, 이를 곡률로 사용
            float dot = Vector3.Dot(toPrev, toNext);
            float curvature = 1.0f - dot; // 값이 클수록 급커브

            // 곡률에 따라 최고 속도와 최저 속도 사이에서 권장 속도를 정함
            float maxSpeed = 300f; // 최대 권장 속도 (km/h)
            float minSpeed = 30f;  // 최소 권장 속도 (km/h)

            // Lerp를 이용해 곡률(0~2)에 따라 속도를 매핑
            recommendedSpeeds.Add(Mathf.Lerp(maxSpeed, minSpeed, curvature / 2.0f));
        }
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
        refinedPoints.Add(newPathCorners[0]); // 시작점은 무조건 추가

        // 원본 경로의 각 코너 사이를 순회
        for (int i = 0; i < newPathCorners.Length - 1; i++)
        {
            Vector3 startPoint = newPathCorners[i];
            Vector3 endPoint = newPathCorners[i + 1];
            float segmentDistance = Vector3.Distance(startPoint, endPoint);

            // 두 점 사이의 거리가 resolution보다 길면, 그 사이를 잘게 나눔
            if (segmentDistance > segmentResolution)
            {
                int divisionCount = Mathf.FloorToInt(segmentDistance / segmentResolution);
                for (int j = 1; j <= divisionCount; j++)
                {
                    float t = (float)j / (divisionCount + 1); // 0과 1 사이의 비율
                    Vector3 interpolatedPoint = Vector3.Lerp(startPoint, endPoint, t);
                    refinedPoints.Add(interpolatedPoint);
                }
            }
            refinedPoints.Add(endPoint); // 각 세그먼트의 끝점 추가
        }

        // 세분화된 경로로 웨이포인트 GameObject 생성
        for (int i = 0; i < refinedPoints.Count; i++)
        {
            GameObject wp = new GameObject($"Waypoint {i}");
            wp.transform.position = refinedPoints[i];
            wp.transform.parent = this.transform;
            waypoints.Add(wp.transform);
        }

        // 새로운 경로에 맞춰 권장 속도 재계산
        CalculateRecommendedSpeeds();
    }
}