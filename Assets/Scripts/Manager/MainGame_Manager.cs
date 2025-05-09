using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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
    [SerializeField] private TimeSpan gameTimeSpan;
    [SerializeField] private DateTime gameTime;

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
    [SerializeField] private GameObject localLapTimeDiff_Prefab;

    [SerializeField] private Image timerImage;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Image lapTimeDiffImage;
    [SerializeField] private Image localLapTimeDiffImage;
    [SerializeField] private TextMeshProUGUI lapTimeDiffText;
    [SerializeField] private TextMeshProUGUI localLapTimeDiffText;
    [SerializeField] bool isLapTimeDiffShowing = false;
    [SerializeField] bool isLocalLapTimeDiffShowing = false;
    [SerializeField] private float lapTimeDiffTimer = 0f;
    [SerializeField] private float localLapTimeDiffTimer = 0f;
    [SerializeField] private float diffTime1;
    [SerializeField] private float diffTime2;
    [SerializeField] private float bestLapTime;

    private string trackName = "City_Night";
    [SerializeField] private TrackData tracksData;
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
    [Header("Rank Colors")]
    private Color tempColor;
    private Color firstPlaceColor = new Color(1.0f, 0.843f, 0.0f, 0.7f); // 1등 색상 (골드)
    private Color secondPlaceColor = new Color(0.769f, 0.769f, 0.769f, 0.7f);  // 2등 색상 (실버)
    private Color thirdPlaceColor = new Color(0.815f, 0.486f, 0.222f, 0.7f); // 3등 색상 (브론즈)
    private Color defaultColor = new Color(0.65f, 0.65f, 0.65f, 0.8f); // 그 외 등수 색상 또는 기본 색상

    [SerializeField]private bool GameStart = false;

    private void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            ExitGame();
        }
        if(gameStart)
        {
            gameTimeSpan = TimeSpan.FromSeconds(gameTimer);
            gameTime = DateTime.Today.Add(gameTimeSpan);
            timerText.text = gameTime.ToString("mm':'ss'.'ff");
            //timerText.text = string.Format("{0:0.00}", gameTimer);
        }
    }

    private void Start()
    {
        for (int i = 0; i < rankPositons.Length; i++)
        {
            rankTargetPositions[i] = rankPositons[i].position;
        }
        LoadAndSetupTrack();
    }


    public override void FixedUpdateNetwork()
    {
        if (Runner.IsServer)
        {
            gameTimer += Runner.DeltaTime;
        }
    }

    private void LoadAndSetupTrack()
    {
        tracksData = TrackData_Manager.instance.GetTrackDataByName(trackName);
        if (tracksData != null)
        {
            for (int i = 0; i < tracksData.Checkpoints.Count; i++)
            {
                CheckPoint checkPoint = Instantiate(checkPoint_Prefab);
                checkPoint.SetCheckPointIndex(this, i + 1, tracksData.Checkpoints[i].Position, tracksData.Checkpoints[i].Rotation, tracksData.Checkpoints[i].Scale);
                if (i > 0)
                {
                    tempCheckPoint.SetNextCheckPoint(checkPoint);
                }
                tempCheckPoint = checkPoint;
                if(i == 0)
                    firstCheckPoint = checkPoint;
                lastCheckPoint = i + 1;
            }
        }
        else
        {
            Debug.LogError("Failed to load track data.");
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
            timerText = timerImage.GetComponentInChildren<TextMeshProUGUI>();
            lapTimeDiffText = lapTimeDiffImage.GetComponentInChildren<TextMeshProUGUI>();
            lapTimeDiffImage.gameObject.SetActive(false);
            localLapTimeDiffImage = Instantiate(localLapTimeDiff_Prefab, MainCanvas.transform).GetComponent<Image>();
            localLapTimeDiffText = localLapTimeDiffImage.GetComponentInChildren<TextMeshProUGUI>();
            localLapTimeDiffImage.gameObject.SetActive(false);
            playerCar.SetName("YOU");
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
        rankList[playersID[playerNumber]].Rpc_SetPosition(playerNumber, defaultColor);
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
    }

    private void ExitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public float CheckPointChecked(Player_Car _playerCar, float _bestTime, float _localBestTime, int checkPointIndex)
    {
        //체크 포인트의 기록과 비교후 느릴 경우 표시한다, 개인 기록은 항상 표시한다.
        if (_bestTime != 0 && _playerCar.GetComponent<NetworkObject>().Id == localPlayer.Id)
        {
            diffTime1 = gameTimer - _bestTime;
            if (!isLapTimeDiffShowing && diffTime1 > 0)
                StartCoroutine(ShowLapTimeDifference(diffTime1));
            diffTime2 = gameTimer - _localBestTime;
            if (!isLocalLapTimeDiffShowing)
                StartCoroutine(ShowLocalLapTimeDifference(diffTime2));
        }
        if (lastCheckPoint == checkPointIndex)
        {
            _playerCar.SetCheckPoint(1);
            short tempLap = _playerCar.GetLap();
            tempLap++;
            _playerCar.SetLap(tempLap);
            Debug.Log("Lap " + tempLap + " CheckPoint " + checkPointIndex + " Entered by " + _playerCar.name);
            bestLapTime = gameTimer;
            gameTimer = 0f;
            if (_playerCar.GetLap() >= maxLaps)
            {
                bestLapTime = gameTimer;
                ExitGame();
            }
            else
            {
                SetFirstCheckPoint(_playerCar);
                return bestLapTime;
            }
        }
        return gameTimer;
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
                    switch (tempRank - 1)
                    {
                        case 0:
                            tempColor = firstPlaceColor;
                            break;
                        case 1:
                            tempColor = secondPlaceColor;
                            break;
                        case 2:
                            tempColor = thirdPlaceColor;
                            break;
                        default:
                            tempColor = defaultColor;
                            break;
                    }
                    if (rankList.TryGetValue(sortedRankData[i].playerId, out Rank playerRank))
                    {
                        playerRank.Rpc_SetPosition(tempRank - 1, tempColor);
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

    IEnumerator ShowLapTimeDifference(float _diffTime)
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
    IEnumerator ShowLocalLapTimeDifference(float _diffTime)
    {
        if (Mathf.Abs(_diffTime) > 1000000)
        {
            isLocalLapTimeDiffShowing = false;
            yield break;
        }
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.04f);
        Debug.Log("start diff timer");
        isLocalLapTimeDiffShowing = true;
        localLapTimeDiffImage.color = new Color(0.8f, 0.8f, 0.8f, 1f);
        if(_diffTime < 0)
            localLapTimeDiffText.color = new Color(1f, 0f, 0f, 1f);
        else
            localLapTimeDiffText.color = new Color(0f, 1f, 0f, 1f);
        localLapTimeDiffText.text = '+' + string.Format("{0:0.00}", _diffTime);
        localLapTimeDiffImage.gameObject.SetActive(true);
        while (true)
        {
            yield return waitForSeconds;
            localLapTimeDiffTimer += 0.04f;
            if (localLapTimeDiffTimer > 3f)
            {
                localLapTimeDiffImage.gameObject.SetActive(false);
                localLapTimeDiffTimer = 0f;
                isLocalLapTimeDiffShowing = false;
                yield break;
            }
            else if (localLapTimeDiffTimer > 2f)
            {
                if (_diffTime < 0)
                    localLapTimeDiffText.color = new Color(1f, 0f, 0f, Mathf.Lerp(1f, 0f, lapTimeDiffTimer - 2f));
                else
                    localLapTimeDiffText.color = new Color(0f, 1f, 0f, Mathf.Lerp(1f, 0f, lapTimeDiffTimer - 2f));
                localLapTimeDiffImage.color = new Color(0.8f, 0.8f, 0.8f, Mathf.Lerp(1f, 0f, lapTimeDiffTimer - 2f));
            }
        }
    }
}