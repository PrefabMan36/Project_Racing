using UnityEngine;
using System.Collections.Generic;
using System.IO; // 파일 경로 및 읽기를 위해 추가
using System.Threading.Tasks; // 비동기 작업을 위해 추가

/// <summary>
/// CSV 파일로부터 웨이포인트 데이터를 읽고, 모든 경로를 관리하는 중앙 관리자 클래스입니다.
/// </summary>
public class WaypointManager : Manager
{
    // Unity 에디터에서 CSV 파일을 할당할 수 있도록 public으로 선언합니다.
    public TextAsset waypointCsvFile;
    private string currentTrackName = "";
    private eTRACKTYPE currentTrackType;

    private WaypointPath waypointPath;

    // 경로 ID(int)를 키로, 해당 경로의 Vector3 좌표 리스트를 값으로 갖는 딕셔너리
    private Dictionary<string, Dictionary<int, List<Vector3>>> allRouteData = new Dictionary<string, Dictionary<int, List<Vector3>>>();
    private Dictionary<int, List<Vector3>> currentRouteData;
    private string waypointFolderPath;
    public bool isLoaded = false;

    private GameObject waypoints;

    private async void Awake()
    {
        OnStart();
        if (Shared.waypointManager == null)
        {
            Shared.waypointManager = this;
            waypointFolderPath = Path.Combine(Application.streamingAssetsPath, "CSV", "Waypoints");
            await LoadAllWaypoints();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void SetCurrentTrackName(string trackName, eTRACKTYPE TRACKTYPE)
    {
        currentTrackName = trackName;
        currentTrackType = TRACKTYPE;
        currentRouteData = allRouteData.ContainsKey(trackName) ? allRouteData[trackName] : null;
        waypoints = new GameObject("Waypoints");
        foreach (Transform child in waypoints.transform) Destroy(child.gameObject);
        foreach (var routs in currentRouteData)
        {
            int count = 0;
            foreach (Vector3 positions in routs.Value)
            {
                GameObject wp = new GameObject($"Waypoint {routs.Key} {count++}");
                wp.transform.position = positions;
                wp.transform.parent = waypoints.transform;
            }
            count = 0;
        }
    }

    public eTRACKTYPE GetTracktype()
    { return currentTrackType; }

    public void SetWaypointPath(WaypointPath path)
    {
        waypointPath = path;
    }

    /// <summary>
    /// 지정된 폴더의 모든 웨이포인트 CSV 파일을 로드하여 딕셔너리에 저장합니다.
    /// </summary>
    private async Task LoadAllWaypoints()
    {
        if (!Directory.Exists(waypointFolderPath))
        {
            Debug.LogError($"[WaypointManager] 웨이포인트 폴더를 찾을 수 없습니다: {waypointFolderPath}");
            isLoaded = true;
            return;
        }

        // 폴더 내의 모든 .csv 파일을 가져옵니다.
        string[] files = Directory.GetFiles(waypointFolderPath, "*.csv");
        Debug.Log($"[WaypointManager] 총 {files.Length}개의 웨이포인트 파일을 로드합니다.");

        foreach (string filePath in files)
        {
            string trackName = Path.GetFileNameWithoutExtension(filePath);
            string fileContents = await File.ReadAllTextAsync(filePath);

            Dictionary<int, List<Vector3>> routeData = new Dictionary<int, List<Vector3>>();
            string[] lines = fileContents.Split('\n');

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                string[] parts = line.Split(new char[] { ',' }, 2);
                if (parts.Length < 2) continue;

                if (!int.TryParse(parts[0].Trim(), out int routeId)) continue;

                List<Vector3> waypoints = new List<Vector3>();
                // 쉼표로 각 웨이포인트 문자열을 분리합니다.
                string[] waypointStrings = parts[1].Split(',');

                foreach (string wpStr in waypointStrings)
                {
                    // 세미콜론으로 x, y, z 좌표를 분리합니다.
                    string[] coords = wpStr.Trim().Split(';');

                    if (coords.Length == 3 &&
                        float.TryParse(coords[0], out float x) &&
                        float.TryParse(coords[1], out float y) &&
                        float.TryParse(coords[2], out float z))
                    {
                        waypoints.Add(new Vector3(x, y, z));
                    }
                }

                if (!routeData.ContainsKey(routeId))
                {
                    routeData.Add(routeId, waypoints);
                    Debug.Log($"[WaypointManager] '{trackName}' 트랙에 경로 ID '{routeId}' 추가. 웨이포인트 수: {waypoints.Count}");
                }
            }
            allRouteData[trackName] = routeData;
            Debug.Log($"[WaypointManager] '{trackName}' 트랙의 웨이포인트 데이터 로드 완료. 총 {routeData.Count}개의 경로 포함.");
        }

        Debug.Log($"[WaypointManager] 모든 웨이포인트 데이터 로드 완료!");
        isLoaded = true;
    }

    /// <summary>
    /// 특정 트랙의 특정 경로에 대한 데이터를 반환합니다.
    /// </summary>
    public List<Vector3> GetRouteData(string trackName, int routeId)
    {
        if (allRouteData.TryGetValue(trackName, out var trackRoutes) && trackRoutes.TryGetValue(routeId, out var routePoints))
        {
            return routePoints;
        }
        Debug.LogWarning($"[WaypointManager] '{trackName}' 트랙의 경로 ID '{routeId}' 데이터를 찾을 수 없습니다.");
        return null;
    }

    public void ClearWaypoints()
    {
        currentTrackName = "";
        currentRouteData = null;
        if(waypoints != null)
            Destroy(waypoints);
    }

    /// <summary>
    /// 특정 트랙의 특정 위치에서 가장 가까운 경로의 ID를 반환합니다.
    /// </summary>
    public int GetClosestRouteID(string trackName, Vector3 position)
    {
        if (!allRouteData.ContainsKey(trackName)) return -1;

        float minDistance = float.MaxValue;
        int closestRouteId = -1;

        foreach (var route in allRouteData[trackName])
        {
            foreach (var point in route.Value)
            {
                float distance = Vector3.Distance(position, point);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestRouteId = route.Key;
                }
            }
        }
        return closestRouteId;
    }

    public int GetClosestRouteID(int routeNum, Vector3 position)
    {
        if (currentRouteData == null) return -1;

        float minDistance = float.MaxValue;
        int closestRouteId = -1;

        foreach (var route in currentRouteData)
        {
            foreach (var point in route.Value)
            {
                float distance = Vector3.Distance(position, point);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestRouteId = route.Key;
                }
            }
        }
        return closestRouteId;
    }

    /// <summary>
    /// 경로 ID를 받아 해당 경로의 웨이포인트 데이터 리스트를 반환합니다.
    /// </summary>
    /// <param name="routeId">요청할 경로의 ID</param>
    /// <returns>Vector3 리스트 형태의 경로 데이터. 해당 ID의 경로가 없으면 null을 반환합니다.</returns>
    public List<Vector3> GetRouteData(int routeId)
    {
        if (currentRouteData.ContainsKey(routeId))
        {
            if(currentTrackType == eTRACKTYPE.eTRACK_TYPE_CIRCUIT)
                return new List<Vector3>(currentRouteData[routeId]) { currentRouteData[routeId][0] };
            else
                return currentRouteData[routeId];
        }
        return null;
    }
}