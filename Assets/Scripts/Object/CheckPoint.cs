using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using Fusion;
using UnityEngine;

public class CheckPoint : NetworkBehaviour
{
    [SerializeField] private MainGame_Manager mainGameManager;
    [SerializeField] private BoxCollider checkPointCollider;

    [SerializeField] private Player_Car EnteredPlayer;
    [Networked, Capacity(16), SerializeField] private NetworkDictionary<short, float> fastestCheckPointTime => default;
    [SerializeField] private float localCheckPointTime = 9999999f;
    [SerializeField] private short currentLap = 0;
    [SerializeField] private float tempTimer;

    [SerializeField] private int checkPointIndex = 0;

    [SerializeField] private CheckPoint nextCheckPoint;
    [SerializeField] private GameObject[] circles;

    public void SetCheckPointIndex(MainGame_Manager gameManager, int index, Vector3 position, Vector3 rotation, Vector3 boxSize)
    {
        checkPointCollider = gameObject.GetComponent<BoxCollider>();
        mainGameManager = gameManager;
        if (checkPointIndex == 0)
            checkPointIndex = index;
        transform.position = position;
        transform.rotation = Quaternion.Euler(rotation);
        checkPointCollider.size = boxSize;
        float circleSize = boxSize.y > boxSize.x ? boxSize.y : boxSize.x;
        Vector3 circleSizeVector = new Vector3(circleSize, circleSize, 1);
        for (int i = 0; i < circles.Length; i++)
        {
            circles[i].transform.localScale = circleSizeVector;
        }
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
