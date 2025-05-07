using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MainGame_Manager : NetworkBehaviour
{
    [SerializeField] private bool gameStart = false;

    [SerializeField] private Player_Car playerCar;
    [SerializeField] private Player_Car[] playerCars = new Player_Car[4];
    [SerializeField] private NetworkObject localPlayer;
    [SerializeField] private Dictionary<int, NetworkId> playersID = new Dictionary<int, NetworkId>();
    [SerializeField] private byte playerNumber = 0;

    [Networked, SerializeField] private float gameTimer { get; set; } = 0;

    // 변경: Car_data 대신 CarData 클래스 사용
    [SerializeField] private CarData carData;

    [SerializeField] private Camera MainCamera_Prefab;
    [SerializeField] private RPMGauge rpmGauge_Prefab;
    [SerializeField] private Slider NitroBar_Prefab;

    [SerializeField] private Canvas MainCanvas;
    [SerializeField] private Camera MainCamera;
    [SerializeField] private Slider NitroBar;
    [SerializeField] private RPMGauge rpmGauge;

    [SerializeField] private GameObject Timer_Prefab;
    [SerializeField] private GameObject lapTimeDiff_Prefab;

    [SerializeField] private Image timerImage;
    [SerializeField] private Text timerText;
    [SerializeField] private Image lapTimeDiffImage;
    [SerializeField] private Text lapTimeDiffText;
    [SerializeField] bool isLapTimeDiffShowing = false;
    [SerializeField] private float lapTimeDiffTimer = 0f;
    [SerializeField] private float finalLapTime;

    [SerializeField] private int lastCheckPoint = 0;
    [SerializeField] private CheckPoint checkPoint_Prefab;
    [SerializeField] private CheckPoint tempCheckPoint;
    [SerializeField] private CheckPoint firstCheckPoint;
    [SerializeField] private CheckPoint checkPoint;
    [SerializeField] private int maxLaps = 1;

    [SerializeField] private List<Rank_Data> rankData = new List<Rank_Data>();
    [SerializeField] private List<Rank_Data> sortedRankData = new List<Rank_Data>();
    [Networked, Capacity(4), SerializeField] private NetworkDictionary<NetworkId, byte> rank => default;
    private byte tempRank;
    private bool isRankingStart = false;

    [SerializeField] private Rank rank_Prefab;
    [SerializeField] private Dictionary<NetworkId, Rank> rankList = new Dictionary<NetworkId, Rank>();
    [SerializeField] private Transform[] rankPositons;
    [SerializeField] private Vector3[] rankTargetPositions = new Vector3[4];


    [SerializeField]private bool GameStart = false;

    private void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            ExitGame();
        }
        if(gameStart)
            timerText.text = string.Format("{0:0.00}", gameTimer);
    }

    private void Awake()
    {
        for (int i = 0; i < rankPositons.Length; i++)
        {
            rankTargetPositions[i] = rankPositons[i].position;
        }
        lastCheckPoint = 4;
        checkPoint = Instantiate(checkPoint_Prefab);
        checkPoint.SetCheckPointIndex(this, 1, new Vector3(-288f, 8f, -5f), new Vector3(0, -45, 0), new Vector3(35, 5, 35));
        tempCheckPoint = checkPoint;
        firstCheckPoint = checkPoint;
        checkPoint = Instantiate(checkPoint_Prefab);
        checkPoint.SetCheckPointIndex(this, 2, new Vector3(-280f, 10f, -170f), new Vector3(0, -135, 0), new Vector3(35, 5, 35));
        tempCheckPoint.SetNextCheckPoint(checkPoint);
        tempCheckPoint = checkPoint;
        checkPoint = Instantiate(checkPoint_Prefab);
        checkPoint.SetCheckPointIndex(this, 3, new Vector3(245f, 7f, -85f), new Vector3(0, 135, 0), new Vector3(35, 5, 35));
        tempCheckPoint.SetNextCheckPoint(checkPoint);
        tempCheckPoint = checkPoint;
        checkPoint = Instantiate(checkPoint_Prefab);
        checkPoint.SetCheckPointIndex(this, 4, new Vector3(-36f, 3f, 26f), new Vector3(0, 0, 0), new Vector3(35, 5, 35));
        tempCheckPoint.SetNextCheckPoint(checkPoint);
    }


    public override void FixedUpdateNetwork()
    {
        if (Runner.IsServer)
        {
            gameTimer += Runner.DeltaTime;
        }
    }

    public void CarInit(Player_Car spawnedCar, bool localPlayer)
    {
        if(!GameStart) gameStart = true;
        playerCar = spawnedCar;
        if (localPlayer)
        {
            this.localPlayer = playerCar.GetComponent<NetworkObject>();

            timerImage = Instantiate(Timer_Prefab, MainCanvas.transform).GetComponent<Image>();
            lapTimeDiffImage = Instantiate(lapTimeDiff_Prefab, MainCanvas.transform).GetComponent<Image>();
            timerText = timerImage.GetComponentInChildren<Text>();
            lapTimeDiffText = lapTimeDiffImage.GetComponentInChildren<Text>();
            lapTimeDiffImage.gameObject.SetActive(false);
        }
        carData = CarData_Manager.instance.GetCarDataByName("Super2000");
        if (MainCamera == null)
        {
            MainCamera = Instantiate(MainCamera_Prefab);
            playerCar.SetCamera(MainCamera);
        }
        if (NitroBar == null)
        {
            NitroBar = Instantiate(NitroBar_Prefab, MainCanvas.transform);
            playerCar.SetNitroBar(NitroBar);
        }
        if (rpmGauge == null)
        {
            rpmGauge = Instantiate(rpmGauge_Prefab, MainCanvas.transform);
            playerCar.SetRPMGauge(rpmGauge);
        }
        playerCar.SetCarMass(carData.Mass);
        playerCar.SetDragCoefficient(carData.dragCoefficient);
        playerCar.SetBaseEngineAcceleration(carData.baseEngineAcceleration);
        playerCar.SetEngineRPMLimit(carData.maxEngineRPM, carData.minEngineRPM);
        switch (carData.lastGear)
        {
            case 1:
                playerCar.SetLastGear(Car.eGEAR.eGEAR_FIRST);
                break;
            case 2:
                playerCar.SetLastGear(Car.eGEAR.eGEAR_SECOND);
                break;
            case 3:
                playerCar.SetLastGear(Car.eGEAR.eGEAR_THIRD);
                break;
            case 4:
                playerCar.SetLastGear(Car.eGEAR.eGEAR_FOURTH);
                break;
            case 5:
                playerCar.SetLastGear(Car.eGEAR.eGEAR_FIFTH);
                break;
            case 6:
                playerCar.SetLastGear(Car.eGEAR.eGEAR_SIXTH);
                break;
            default:
                Debug.Log("잘못된 lastGear설정입니다.");
                break;
        }
        playerCar.SetGearRatio(Car.eGEAR.eGEAR_NEUTURAL, 0f);
        playerCar.SetGearRatio(Car.eGEAR.eGEAR_REVERSE, carData.gearRatio_eGEAR_REVERSE);
        playerCar.SetGearRatio(Car.eGEAR.eGEAR_FIRST, carData.gearRatio_eGEAR_FIRST);
        playerCar.SetGearRatio(Car.eGEAR.eGEAR_SECOND, carData.gearRatio_eGEAR_SECOND);
        playerCar.SetGearRatio(Car.eGEAR.eGEAR_THIRD, carData.gearRatio_eGEAR_THIRD);
        playerCar.SetGearRatio(Car.eGEAR.eGEAR_FOURTH, carData.gearRatio_eGEAR_FOURTH);
        playerCar.SetGearRatio(Car.eGEAR.eGEAR_FIFTH, carData.gearRatio_eGEAR_FIFTH);
        playerCar.SetGearRatio(Car.eGEAR.eGEAR_SIXTH, carData.gearRatio_eGEAR_SIXTH);
        playerCar.SetFinalDriveRatio(carData.finalDriveRatio);
        playerCar.SetGearSpeedLimit(Car.eGEAR.eGEAR_NEUTURAL, 0f);
        playerCar.SetGearSpeedLimit(Car.eGEAR.eGEAR_REVERSE, carData.gearSpeedLimit_eGEAR_REVERSE);
        playerCar.SetGearSpeedLimit(Car.eGEAR.eGEAR_FIRST, carData.gearSpeedLimite_GEAR_FIRST);
        playerCar.SetGearSpeedLimit(Car.eGEAR.eGEAR_SECOND, carData.gearSpeedLimit_eGEAR_SECOND);
        playerCar.SetGearSpeedLimit(Car.eGEAR.eGEAR_THIRD, carData.gearSpeedLimit_eGEAR_THIRD);
        playerCar.SetGearSpeedLimit(Car.eGEAR.eGEAR_FOURTH, carData.gearSpeedLimit_eGEAR_FOURTH);
        playerCar.SetGearSpeedLimit(Car.eGEAR.eGEAR_FIFTH, carData.gearSpeedLimit_eGEAR_FIFTH);
        playerCar.SetGearSpeedLimit(Car.eGEAR.eGEAR_SIXTH, carData.gearSpeedLimit_eGEAR_SIXTH);
        playerCar.SetID(playerNumber);
        if(playersID.ContainsKey(playerNumber))
            playersID[playerNumber] = playerCar.GetComponent<NetworkObject>().Id;
        else
            playersID.Add(playerNumber, playerCar.GetComponent<NetworkObject>().Id);
        rankList.Add(playersID[playerNumber], Instantiate(rank_Prefab, MainCanvas.transform));
        if (localPlayer == playerCar.GetComponent<NetworkObject>())
            rankList[playersID[playerNumber]].SetPlay(null, "YOU");
        else
            rankList[playersID[playerNumber]].SetPlay(null, playerCar.GetName() != null ? playerCar.GetName() : playersID[playerNumber].ToString());
        rankList[playersID[playerNumber]].SetTargets(rankTargetPositions);
        rankList[playersID[playerNumber]].Rpc_SetPosition(playerNumber);
        playerCars[playerNumber++] = playerCar;
        SetFirstCheckPoint(playerCar);
        if (!isRankingStart && playerNumber > 1)
        {
            isRankingStart = true;
            StartCoroutine(UpdatingRankings());
        }
        playerCar.Init();
    }

    public void OnJoinPlayer(NetworkObject networkPlayerObject)
    {
        playersID[playerNumber] = networkPlayerObject.Id;
        rank.Add(networkPlayerObject.Id, 0);
    }
    public void OnLeftPlayer(NetworkObject networkPlayerObject)
    {
        rank.Remove(networkPlayerObject.Id);
        Destroy(rankList[networkPlayerObject.Id].gameObject);
        rankList.Remove(networkPlayerObject.Id);
        //Queue<Rank> rankQueue = new Queue<Rank>();
        //Queue<NetworkId> idQueue = new Queue<NetworkId>();
        //for (int i = 0; i < playerNumber; i++)
        //{
        //    if (playersID.TryGetValue(i, out NetworkId currentId))
        //    {
        //        if (currentId != networkPlayerObject.Id)
        //        {
        //            idQueue.Enqueue(currentId);
        //            if (rankList.TryGetValue(currentId, out Rank playerRank))
        //                rankQueue.Enqueue(playerRank);
        //        }
        //    }
        //}
        //rankList.Clear();
        //playersID.Clear();
        //int newIndex = 0;
        //while (idQueue.Count > 0)
        //{
        //    NetworkId playerId = idQueue.Dequeue();
        //    Rank playerRank = rankQueue.Dequeue();
        //    playersID.Add(newIndex, playerId);
        //    rankList.Add(playerId, playerRank);
        //    newIndex++;
        //}
        //playerNumber = (byte)newIndex;
    }

    private void ExitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public float CheckPointChecked(Player_Car _playerCar, float _bestTime, int checkPointIndex)
    {
        if (_bestTime != 0 && _playerCar.GetComponent<NetworkObject>().Id == localPlayer.Id)
        {
            float diffTime = gameTimer - _bestTime;
            if (!isLapTimeDiffShowing && diffTime > 0)
                StartCoroutine(Showlaptimedifference(diffTime));
        }
        if (lastCheckPoint == checkPointIndex)
        {
            _playerCar.SetCheckPoint(1);
            short tempLap = _playerCar.GetLap();
            tempLap++;
            _playerCar.SetLap(tempLap);
            Debug.Log("Lap " + tempLap + " CheckPoint " + checkPointIndex + " Entered by " + _playerCar.name);
            finalLapTime = gameTimer;
            gameTimer = 0f;
            if (_playerCar.GetLap() >= maxLaps)
            {
                finalLapTime = gameTimer;
                ExitGame();
            }
            else
            {
                SetFirstCheckPoint(_playerCar);
            }
        }
        if (gameTimer < _bestTime)
            return gameTimer;
        else
            return _bestTime;
    }

    IEnumerator UpdatingRankings()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.1f);
        while(true)
        {
            rankData.Clear();
            for (int i = 0; i < playerCars.Length; i++)
            {
                if (playerCars[i] != null)
                    rankData.Add(playerCars[i].GetRankData());
            }
            sortedRankData = rankData.OrderByDescending(carData => carData.lap)
                .ThenByDescending(carData => carData.currentCheckpointIndex)
                .ThenBy(carData => carData.distanceToCheckPoint)
                .ToList();
            for (int i = 0; i < sortedRankData.Count; i++)
            {
                tempRank = (byte)(i + 1);
                //Debug.Log("sortedRankData[" + i + "] : " + sortedRankData[i].playerId + " " + sortedRankData[i].lap + " " + sortedRankData[i].currentCheckpointIndex + " " + sortedRankData[i].distanceToCheckPoint);
                rank.Set(sortedRankData[i].playerId, tempRank);
                if(tempRank - 1 < 4)
                {
                    if (rankList.TryGetValue(sortedRankData[i].playerId, out Rank playerRank))
                    {
                        playerRank.Rpc_SetPosition(tempRank - 1);
                    }
                }
            }
            yield return waitForSeconds;
        }
    }

    private void SetFirstCheckPoint(Player_Car _playerCar)
    {
        _playerCar.SetNextCheckPointPosition(firstCheckPoint);
    }

    IEnumerator Showlaptimedifference(float _diffTime)
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.04f);
        Debug.Log("start diff timer");
        isLapTimeDiffShowing = true;
        lapTimeDiffImage.color = new Color(0.8f, 0.8f, 0.8f, 1f);
        lapTimeDiffText.color = new Color(1f, 0f, 0f, 1f);
        lapTimeDiffText.text = '+' + string.Format("{0:0.00}", _diffTime);
        lapTimeDiffImage.gameObject.SetActive(true);
        while (true)
        {
            yield return waitForSeconds;
            lapTimeDiffTimer += 0.04f;
            if (lapTimeDiffTimer > 3f)
            {
                lapTimeDiffImage.gameObject.SetActive(false);
                lapTimeDiffTimer = 0f;
                isLapTimeDiffShowing = false;
                yield break;
            }
            else if (lapTimeDiffTimer > 2f)
            {
                lapTimeDiffText.color = new Color(1f, 0f, 0f, Mathf.Lerp(1f, 0f, lapTimeDiffTimer - 2f));
                lapTimeDiffImage.color = new Color(0.8f, 0.8f, 0.8f, Mathf.Lerp(1f, 0f, lapTimeDiffTimer - 2f));
            }
        }
    }
}