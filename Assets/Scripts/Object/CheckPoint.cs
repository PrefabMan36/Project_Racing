using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using Fusion;
using UnityEngine;
using UnityEngine.UIElements;

public class CheckPoint : NetworkBehaviour
{
    [SerializeField] private MainGame_Manager mainGameManager;
    [SerializeField] private BoxCollider checkPointCollider;

    [SerializeField] private Player_Car EnteredPlayer;
    [Networked, Capacity(16), SerializeField] private NetworkDictionary<short, float> fastestCheckPointTime => default;
    [SerializeField] private float localCheckPointTime = 9999999f;
    [SerializeField] private short currentLap = 0;
    [SerializeField] private float tempTimer;

    [Networked] Vector3 position { get; set; }
    [Networked] Vector3 rotation { get; set; }
    [Networked] Vector3 boxSize { get; set; }
    [Networked] float circleSize { get; set; }
    [Networked] bool last { get; set; } = false;
    [Networked, SerializeField] private int checkPointIndex { get; set; } = 0;

    [SerializeField] private CheckPoint nextCheckPoint;
    [SerializeField] private GameObject[] circles;

    public override void Spawned()
    {
        base.Spawned();
        checkPointCollider = gameObject.GetComponent<BoxCollider>();
        localCheckPointTime = 9999999f;
        currentLap = 0;
        tempTimer = 0;
        mainGameManager = GameObject.FindAnyObjectByType<MainGame_Manager>();
        if(!HasStateAuthority)
            InitCheckPointForClient();
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
                currentLap = EnteredPlayer.GetLap();
                if (!fastestCheckPointTime.ContainsKey(currentLap))
                    fastestCheckPointTime.Add(currentLap, 9999999f);
                if (EnteredPlayer.GetCheckPoint() == checkPointIndex)
                {
                    EnteredPlayer.SetCheckPoint(checkPointIndex + 1);
                    tempTimer = mainGameManager.CheckPointChecked(EnteredPlayer, fastestCheckPointTime[currentLap], localCheckPointTime, checkPointIndex);
                    fastestCheckPointTime.Set(currentLap, fastestCheckPointTime[currentLap] > tempTimer ? tempTimer : fastestCheckPointTime[currentLap]);
                    if(EnteredPlayer.GetLocalPlayer() && tempTimer < localCheckPointTime)
                        localCheckPointTime = tempTimer;
                    Debug.Log("CheckPoint " + checkPointIndex + " Entered by " + EnteredPlayer.name + " in " + tempTimer.ToString("0.00"));
                }
            }
        }
    }

    public void SetNextCheckPoint(CheckPoint nextCheckPoint)
    {
        this.nextCheckPoint = nextCheckPoint;
    }
}
