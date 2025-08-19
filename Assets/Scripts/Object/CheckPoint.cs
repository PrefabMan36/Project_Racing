using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using Fusion;
using UnityEngine;
using UnityEngine.UIElements;

public class CheckPoint : NetworkBehaviour
{
    [SerializeField] private MainGame_Manager mainGameManager;
    [SerializeField] private BoxCollider checkPointCollider;

    [SerializeField] private Pathfinder pathfinder;

    [SerializeField] private Transform navigationPoint;
    [SerializeField] private Vector3 navigationPointSize = new Vector3(1f, 1f, 1f);

    [SerializeField] private Player_Car EnteredPlayer;
    [Networked, Capacity(16), SerializeField] private NetworkDictionary<short, float> fastestCheckPointTime => default;
    [SerializeField] private float localCheckPointTime = 9999999f;
    [SerializeField] private short currentLap = 0;
    [SerializeField] private float tempTimer;
    [SerializeField] private bool isNavigationPoint = false;

    [Networked] Vector3 position { get; set; }
    [Networked] Vector3 rotation { get; set; }
    [Networked] Vector3 boxSize { get; set; }
    [Networked] float circleSize { get; set; }
    [Networked] bool last { get; set; } = false;
    [Networked, SerializeField] private int checkPointIndex { get; set; } = 0;

    [SerializeField] private CheckPoint nextCheckPoint;
    [Networked, SerializeField] private Vector3 nextCheckPointValue { get; set; } = Vector3.zero;
    [SerializeField] private GameObject[] circles;

    public override void Spawned()
    {
        base.Spawned();
        checkPointCollider = gameObject.GetComponent<BoxCollider>();
        localCheckPointTime = 9999999f;
        currentLap = 0;
        tempTimer = 0;
        navigationPointSize = navigationPoint.localScale;
        mainGameManager = GameObject.FindAnyObjectByType<MainGame_Manager>();
        if(!HasStateAuthority)
            InitCheckPointForClient();
    }

    public void SelecteByNaviPoint()
    {
        isNavigationPoint = true;
        if(navigationPoint != null)
        {
            isNavigationPoint = false;
            navigationPoint.localScale = navigationPointSize * 2;
        }   
    }

    public void SetCheckPointIndex(int index, Vector3 _position, Vector3 _rotation, Vector3 _boxSize, bool _last)
    {
        last = _last;
        if (checkPointIndex == 0)
            checkPointIndex = index;
        if (checkPointIndex == 1)
            mainGameManager.SetFirstCheckPoint(this);
        else if(last)
            mainGameManager.SetLastCheckPoint(this);
        position = _position;
        transform.position = position;
        rotation = _rotation;
        transform.rotation = Quaternion.Euler(rotation);
        boxSize = _boxSize;
        checkPointCollider.size = boxSize;
        circleSize = boxSize.y > boxSize.x ? boxSize.y : boxSize.x;
        Vector3 circleSizeVector = new Vector3(circleSize, circleSize, 1);
        for (int i = 0; i < circles.Length; i++)
        {
            circles[i].transform.localScale = circleSizeVector;
        }
        pathfinder = mainGameManager.pathfinder;
    }

    private void InitCheckPointForClient()
    {
        if (checkPointIndex == 1)
            mainGameManager.SetFirstCheckPoint(this);
        else if (last)
            mainGameManager.SetLastCheckPoint(this);
        transform.position = position;
        transform.rotation = Quaternion.Euler(rotation);
        checkPointCollider.size = boxSize;
        circleSize = boxSize.y > boxSize.x ? boxSize.y : boxSize.x;
        Vector3 circleSizeVector = new Vector3(circleSize, circleSize, 1);
        for (int i = 0; i < circles.Length; i++)
        {
            circles[i].transform.localScale = circleSizeVector;
        }
        if(last)
            LobbyPlayer.localPlayer.RPC_ChangeSyncTrackState(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            EnteredPlayer = other.gameObject.GetComponent<Player_Car>();
            
            if (EnteredPlayer != null)
            {
                if (EnteredPlayer.Object == null || !EnteredPlayer.Object.IsValid)
                    return;
                currentLap = EnteredPlayer.GetLap();
                if (!fastestCheckPointTime.ContainsKey(currentLap))
                    fastestCheckPointTime.Add(currentLap, 9999999f);
                if (EnteredPlayer.GetCheckPointIndex() == checkPointIndex)
                {
                    navigationPoint.localScale = navigationPointSize;
                    EnteredPlayer.SetCheckPoint(checkPointIndex + 1);
                    EnteredPlayer.SetNextCheckPointPosition(nextCheckPoint);

                    pathfinder.UpdatePath(transform.position, nextCheckPointValue);

                    tempTimer = mainGameManager.CheckPointChecked(EnteredPlayer, fastestCheckPointTime[currentLap], localCheckPointTime, checkPointIndex);
                    fastestCheckPointTime.Set(currentLap, fastestCheckPointTime[currentLap] > tempTimer ? tempTimer : fastestCheckPointTime[currentLap]);
                    if(EnteredPlayer.GetLocalPlayer() && tempTimer < localCheckPointTime)
                        localCheckPointTime = tempTimer;
                    Debug.Log("CheckPoint " + checkPointIndex + " Entered by " + EnteredPlayer.name + " in " + tempTimer.ToString("0.00"));
                }
            }
            else
                Debug.LogWarning("EnteredPlayer is null in CheckPoint.OnTriggerEnter");
        }
    }

    public void SetNextCheckPoint(CheckPoint _nextCheckPoint)
    {
        nextCheckPoint = _nextCheckPoint;
        nextCheckPointValue = _nextCheckPoint.transform.position;
    }
    public CheckPoint GetNextCheckPoint()
    { return nextCheckPoint; }

    public int GetCheckPointIndex()
    { return checkPointIndex; }
}
