using UnityEngine;
using UnityEngine.AI;

// NavMesh를 이용해 두 지점 사이의 경로를 찾는 스크립트
public class Pathfinder : MonoBehaviour
{
    public Transform startPoint;  // 출발 지점
    public Transform endPoint;    // 도착 지점
    public WaypointPath drivingLinePath; // 경로를 전달할 WaypointPath 스크립트
    private void Awake()
    {
        startPoint = new GameObject("StartPoint").transform;
        endPoint = new GameObject("EndPoint").transform;
        drivingLinePath = GameObject.FindObjectOfType<WaypointPath>();
    }

    // 특정 지점들 사이의 경로를 찾아 주행 라인을 업데이트하는 함수
    public void UpdatePath(Vector3 start, Vector3 end)
    {
        startPoint.position = start;
        endPoint.position = end;
        FindAndDrawPath();
    }

    // 경로를 찾고 라인을 그리는 메인 함수
    private void FindAndDrawPath()
    {
        NavMeshHit startHit;
        NavMeshHit endHit;

        // 1. 시작점 근처에서 가장 가까운 NavMesh 포인트를 찾음 (검색 반경 5.0f)
        bool startFound = NavMesh.SamplePosition(startPoint.position, out startHit, 5.0f, NavMesh.AllAreas);
        // 2. 끝점 근처에서 가장 가까운 NavMesh 포인트를 찾음 (검색 반경 5.0f)
        bool endFound = NavMesh.SamplePosition(endPoint.position, out endHit, 5.0f, NavMesh.AllAreas);

        // 3. 두 지점 모두 NavMesh 위에서 유효한 위치를 찾았는지 확인
        if (!startFound || !endFound)
        {
            Debug.LogError("시작점 또는 끝점이 NavMesh 근처에 없습니다! 위치를 확인해주세요.");
            return; // 경로 탐색 중단
        }

        // 4. 찾은 유효한 위치(startHit.position, endHit.position)를 사용해 경로 계산
        NavMeshPath navMeshPath = new NavMeshPath();
        if (NavMesh.CalculatePath(startHit.position, endHit.position, NavMesh.AllAreas, navMeshPath))
        {
            if (navMeshPath.status == NavMeshPathStatus.PathComplete)
            {
                Debug.Log("경로 탐색 성공!");
                drivingLinePath.SetAndRefinePath(navMeshPath.corners, 2.0f);
            }
            else
            {
                Debug.LogError("경로 탐색 실패: 목적지에 도달할 수 없습니다. (Path Invalid or Partial)");
            }
        }
        else
        {
            // 이 로그는 이제 거의 볼 수 없게 됩니다.
            Debug.LogError("NavMesh.CalculatePath 함수 호출 실패");
        }
    }
}