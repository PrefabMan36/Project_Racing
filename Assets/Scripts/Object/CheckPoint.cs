using Fusion;
using UnityEngine;

public class CheckPoint : NetworkBehaviour
{
    [SerializeField] private MainGame_Manager mainGameManager;
    [SerializeField] private BoxCollider checkPointCollider;

    [SerializeField] private Player_Car EnteredPlayer;
    [Networked, SerializeField] private float fastestCheckPointTime { get; set; } = 9999999f;
    [SerializeField] private float tempTimer;

    [SerializeField] private int checkPointIndex = 0;

    [SerializeField] private CheckPoint nextCheckPoint;

    public void SetCheckPointIndex(MainGame_Manager gameManager, int index, Vector3 position, Vector3 rotation, Vector3 boxSize)
    {
        checkPointCollider = gameObject.GetComponent<BoxCollider>();
        mainGameManager = gameManager;
        if (checkPointIndex == 0)
            checkPointIndex = index;
        transform.position = position;
        transform.rotation = Quaternion.Euler(rotation);
        checkPointCollider.size = boxSize;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            EnteredPlayer = other.gameObject.GetComponent<Player_Car>();
            if (EnteredPlayer != null)
            {
                if(EnteredPlayer.GetCheckPoint() == checkPointIndex)
                {
                    EnteredPlayer.SetCheckPoint(checkPointIndex + 1);
                    Debug.Log("CheckPoint " + checkPointIndex + " Entered by " + EnteredPlayer.name);
                    tempTimer = mainGameManager.CheckPointChecked(EnteredPlayer, fastestCheckPointTime, checkPointIndex);
                    fastestCheckPointTime = fastestCheckPointTime < tempTimer ? tempTimer : fastestCheckPointTime;
                }
            }
        }
    }

    public void SetNextCheckPoint(CheckPoint nextCheckPoint)
    {
        this.nextCheckPoint = nextCheckPoint;
    }
}
