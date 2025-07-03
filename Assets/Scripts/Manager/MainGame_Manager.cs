using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainGame_Manager : NetworkBehaviour
{
    [SerializeField] private bool gameStart = false;
    [SerializeField] NetworkRunner networkRunner;

    [SerializeField] private Player_Car playerCar;
    [SerializeField] private Player_Car[] playerCars = new Player_Car[4];
    [SerializeField] private NetworkObject localPlayer;
    [SerializeField] private Player_Car localPlayerCar;
    [SerializeField] private Dictionary<int, NetworkId> playersID = new Dictionary<int, NetworkId>();
    [SerializeField] private byte playerNumber = 0;

    [SerializeField] private Transform[] spawnPosition = new Transform[4];
    [SerializeField] private float spawnPointSpacing = 2.5f;
    [SerializeField] private float spawnPointVerticalOffset = 1.0f;
    [SerializeField] private Player_Car[] playerCarPrefab;
    [SerializeField] private string[] playerCarPrefabNames;

    [SerializeField] private float gameTimer;
    [SerializeField] private TimeSpan gameTimeSpan;
    [SerializeField] private DateTime gameTime;

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

    private string trackName = "eSCENE_CITY_NIGHT";
    [SerializeField] private TrackData tracksData;
    [Networked, SerializeField] private int lastCheckPointIndex { get; set; } = 0;
    [SerializeField] private CheckPoint checkPoint_Prefab;
    [SerializeField] private CheckPoint tempCheckPoint;
    [SerializeField] private CheckPoint firstCheckPoint;
    [SerializeField] private CheckPoint lastCheckPoint;
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

    [Header("Result UI")]
    [SerializeField] private GameObject resultUI_Prefab;
    [SerializeField] private GameObject resultUI;
    [SerializeField] private RaceResultBox raceResultBox;

    [Networked, Capacity(4)]
    private NetworkDictionary<NetworkId, float> finishedPlayerTimes => default;
    [Networked, Capacity(4)]
    private NetworkDictionary<NetworkId, string> finishedPlayerNames => default;

    private bool isResultPanelActiveLocally = false;

    [Networked] private int totalPlayerCount { get; set; } = 0;
    [Networked] private bool raceEndedByCompletion { get; set; } = false;
    [Networked] private bool raceEndedByTimeout { get; set; } = false;
    [Networked] private TickTimer sceneChangeTimer { get; set; }
    [SerializeField] private float sceneChangeDelay = 5f; // 씬 변경 지연 시간 (초)

    [Header("Countdown")]
    [SerializeField] private NetworkPrefabRef countDown_Prefab;
    [SerializeField] private CountDown countDown;
    [Networked] private bool raceFinishCountdownTriggered { get; set; } = false;
    //[Networked] private NetworkBool countDownSpawned { get; set; } = false;

    [SerializeField] private GameObject parentObjectForUIPanel_Prefab;
    [SerializeField] private GameObject parentObjectForUIPanel;

    private void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            ExitGame();
        }
        if (gameStart)
        {
            gameTimeSpan = TimeSpan.FromSeconds(gameTimer);
            gameTime = DateTime.Today.Add(gameTimeSpan);
            timerText.text = gameTime.ToString("mm':'ss'.'ff");
        }
        //if(Input.GetKey(KeyCode.Return))
        //{
        //    if (Runner.GameMode == GameMode.Host)
        //    {
        //        foreach (LobbyPlayer player in LobbyPlayer.players)
        //            SpawnPlayer(Runner, player);
        //    }
        //}
    }

    public override void Spawned()
    {
        base.Spawned();

        trackName = SceneManager.GetActiveScene().name;
        var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        var sceneInfo = new NetworkSceneInfo();
        if (scene.IsValid)
            sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);

        MainCanvas = Shared.ui_Manager.GetMainCanvas();
        parentObjectForUIPanel = Instantiate(parentObjectForUIPanel_Prefab, MainCanvas.transform);

        for (int i = 0; i < rankPositons.Length; i++)
        {
            rankTargetPositions[i] = Instantiate(rankPositons[i], parentObjectForUIPanel.transform).position;
        }
        if (networkRunner == null)
            networkRunner = GameObject.Find("Session").GetComponent<NetworkRunner>();
        if(HasStateAuthority)
            LoadAndSetupTrack();
        StartCoroutine(WaitingForCheckpoint());

        if (resultUI_Prefab != null)
        {
            resultUI = Instantiate(resultUI_Prefab, parentObjectForUIPanel.transform);
            raceResultBox = resultUI.GetComponent<RaceResultBox>();

            if (raceResultBox == null)
                Debug.LogError("RaceResultBox 컴포넌트가 resultUI에 없습니다. 추가해주세요.");
            resultUI.SetActive(false);
        }
        else
            Debug.LogError("resultUI가 할당되지 않았습니다. UI를 확인해주세요.");

        if(Object.HasStateAuthority)
        {
            countDown = Runner.Spawn(countDown_Prefab).GetComponent<CountDown>();
            //countDownSpawned = true;
            Debug.Log("CountDown 객체가 호스트에서 스폰되었습니다.");
        }
        StartCoroutine(CheckCountDownSpawned());

        sceneChangeTimer = TickTimer.None;
        raceFinishCountdownTriggered = false;
    }

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();

        if(resultUI != null && localPlayer != null)
        {
            if(localPlayerCar == null)
                localPlayerCar = localPlayer.GetComponent<Player_Car>();
            if(localPlayerCar != null)
            {
                isResultPanelActiveLocally = (localPlayerCar.GetLap() >= maxLaps) || raceEndedByTimeout;
                resultUI.SetActive(isResultPanelActiveLocally);
            }
        }
        else if(resultUI != null && localPlayer == null)
            resultUI.SetActive(false);

        if (isResultPanelActiveLocally && resultUI != null)
            raceResultBox.UpdateResultDisplay(finishedPlayerTimes, finishedPlayerNames);

        if (Object.HasStateAuthority)
        {
            if((raceEndedByCompletion || raceEndedByTimeout) && !sceneChangeTimer.IsRunning && sceneChangeTimer.ExpiredOrNotRunning(Runner))
            {
                sceneChangeTimer = TickTimer.CreateFromSeconds(Runner, sceneChangeDelay);
                Debug.Log($"호스트: 씬 전환 타이머 시작! {sceneChangeDelay}초 후 씬 전환.");
            }

            if (sceneChangeTimer.IsRunning && sceneChangeTimer.Expired(Runner))
            {
                Shared.ui_Manager.isInGame = false;
                Shared.ui_Manager.BackToMenu();
                isRankingStart = false;
                removeUIs();

                LobbyPlayer.localPlayer.Regroup();

                Runner.LoadScene(SceneRef.FromIndex(2));
                Destroy(resultUI);
                Debug.Log("호스트: 씬이 LobbyScene으로 전환됩니다.");
            }
        }
    }

    public void RaceStart()
    { 
        if(Object.HasStateAuthority)
        {
            Debug.Log("MainGame_Manager: RaceStarted 호출됨. 모든 차량에 StartRace 실행.");
            foreach (Player_Car playerCar in playerCars)
            {
                if (playerCar != null)
                {
                    playerCar.StartRace();
                }
            }
        }

        if (!gameStart) gameStart = true;
    }
    public void RaceEnd()
    {
        if (Object.HasStateAuthority)
        {
            if (gameStart)
            {
                //gameStart = false;
                Debug.Log("게임 종료! 첫 번째 플레이어 완주.");
                foreach (var playerCarEntry in playerCars)
                {
                    if (playerCarEntry != null && playerCarEntry.GetLap() >= maxLaps) // 완주 조건 확인
                    {
                        NetworkId playerId = playerCarEntry.Object.Id;
                        if (!finishedPlayerTimes.ContainsKey(playerId))
                        {
                            finishedPlayerTimes.Add(playerId, playerCarEntry.GetFinishTime());
                            finishedPlayerNames.Add(playerId, playerCarEntry.GetName()); // 이름도 저장 (RPC를 통해 설정된 이름)
                            Debug.Log($"플레이어 {playerCarEntry.GetName()} (ID: {playerId}) 완주 시간 기록: {playerCarEntry.GetFinishTime()}");
                        }
                    }
                }
            }
        }
        // 모든 플레이어가 완주했는지 확인 (호스트에서만)
        if (finishedPlayerTimes.Count >= totalPlayerCount) // totalPlayerCount와 같거나 많으면 (혹시 모를 오류 대비 >=)
        {
            raceEndedByCompletion = true;
            Debug.Log("호스트: 모든 플레이어가 완주했습니다. 씬 전환 준비.");
        }

        if (countDown != null && !raceFinishCountdownTriggered)
        {
            countDown.StartCountdown(0, false); // 숫자 없이 바로 "Race Finish" 스프라이트 표시
            raceFinishCountdownTriggered = true; // 플래그 설정
            Debug.Log("MainGame_Manager: 레이스 종료 카운트다운이 트리거되었습니다.");
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.StateAuthority)]
    public void RPC_ForceRaceEnd()
    {
        Debug.Log("RPC_ForceRaceEnd 호출됨: 레이스가 강제 종료됩니다.");
        raceEndedByTimeout = true;
        // 강제 종료 시, 모든 플레이어를 finishedPlayerTimes에 추가 (완주 시간은 0으로 처리하거나 적절한 값 설정)
        // 이 부분은 필요에 따라 구현. 현재는 강제 종료 시 결과창만 띄우고 씬 전환.
        // 예를 들어, 모든 플레이어를 강제로 완주 처리하고 싶다면:
        // if (Object.HasStateAuthority)
        // {
        //     foreach (var car in playerCars)
        //     {
        //         if (car != null && !finishedPlayerTimes.ContainsKey(car.Object.Id))
        //         {
        //             finishedPlayerTimes.Add(car.Object.Id, -1f); // 강제 종료된 플레이어 시간 (음수 또는 특정 값)
        //             finishedPlayerNames.Add(car.Object.Id, car.GetName());
        //         }
        //     }
        // }
    }

    private void removeUIs()
    {
        Destroy(parentObjectForUIPanel);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_GameReadyAndStart()
    {
        RaceStart();
        Debug.Log("MainGame_Manager: RPC_GameReadyAndStart 수신됨. 게임 시작 준비 완료!");
    }

    public void SpawnPlayer(NetworkRunner runner, LobbyPlayer player)
    {
        var index = LobbyPlayer.players.IndexOf(player);
        var point = spawnPosition[index];

        var profabID = player.carIndex-1;
        var prefab = playerCarPrefab[profabID];

        var entity = runner.Spawn(
            prefab,
            point.position,
            point.rotation,
            player.Object.InputAuthority
            );

        player.gameState = eGAMESTATE.GAMEREADY;
        player.car = entity;
        entity.GetComponent<Player_Car>().SetName(player.playerName.Value);
    }

    private IEnumerator CheckCountDownSpawned()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(Shared.frame15);
        while (FindObjectOfType<CountDown>() == null)
        {
            yield return waitForSeconds;
        }
        countDown = FindObjectOfType<CountDown>().GetComponent<CountDown>();
        countDown.transform.SetParent(parentObjectForUIPanel.transform, false);
        countDown.SetMainGameManager(this);
        Debug.Log("카운트 다운이 시작됩니다.");
    }

    private void LoadAndSetupTrack()
    {
        tracksData = TrackData_Manager.instance.GetTrackDataByName(trackName);
        if (tracksData != null)
        {
            bool lastcheck = false;
            lastCheckPointIndex = tracksData.Checkpoints.Count-1;
            for (int i = 0; i < tracksData.Checkpoints.Count; i++)
            {
                CheckPoint checkPoint = networkRunner.Spawn(checkPoint_Prefab);

                if(i == lastCheckPointIndex)
                { lastcheck = true; }
                else
                { lastcheck = false; }

                checkPoint.SetCheckPointIndex(i + 1, tracksData.Checkpoints[i].Position, tracksData.Checkpoints[i].Rotation, tracksData.Checkpoints[i].Scale, lastcheck);
            }
            GenerateSpawnPointsFromCheckpoint(lastCheckPoint);
            lastCheckPointIndex = lastCheckPoint.GetCheckPointIndex();
            LobbyPlayer.localPlayer.RPC_ChangeSyncTrackState(true);
        }
        else
        {
            Debug.LogError($"Failed to load {trackName} track data.");
        }
    }
    public void SetFirstCheckPoint(CheckPoint checkPoint)
    { firstCheckPoint = checkPoint; }
    public void SetLastCheckPoint(CheckPoint checkPoint)
    { lastCheckPoint = checkPoint; }

    private void GenerateSpawnPointsFromCheckpoint(CheckPoint referenceCheckpoint)
    {
        if (referenceCheckpoint == null)
        {
            Debug.LogError("Cannot generate spawn points: referenceCheckpoint is null.");
            return;
        }
        Transform checkpointTransform = referenceCheckpoint.transform;
        float initialOffset = -((spawnPosition.Length - 1) * spawnPointSpacing) / 2.0f;
        for (int i = 0; i < spawnPosition.Length; i++)
        {
            Vector3 horizontalOffset = checkpointTransform.right * (initialOffset + (i * spawnPointSpacing));
            Vector3 verticalOffsetVector = checkpointTransform.up * spawnPointVerticalOffset;

            Vector3 spawnPos = checkpointTransform.position + horizontalOffset + verticalOffsetVector;
            Quaternion spawnRot = checkpointTransform.rotation;

            GameObject spGO = new GameObject($"DynamicSpawnPoint_{i}");
            spGO.transform.position = spawnPos;
            spGO.transform.rotation = spawnRot;
            spGO.transform.SetParent(this.transform);

            spawnPosition[i] = spGO.transform;
        }
        Debug.Log($"Generated {spawnPosition.Length} spawn points from the first checkpoint.");
    }

    public void CarInit(Player_Car spawnedCar, bool localPlayer)
    {
        playerCar = spawnedCar;
        if (localPlayer)
        {
            this.localPlayer = playerCar.GetComponent<NetworkObject>();

            timerImage = Instantiate(Timer_Prefab, parentObjectForUIPanel.transform).GetComponent<Image>();
            lapTimeDiffImage = Instantiate(lapTimeDiff_Prefab, parentObjectForUIPanel.transform).GetComponent<Image>();
            timerText = timerImage.GetComponentInChildren<TextMeshProUGUI>();
            lapTimeDiffText = lapTimeDiffImage.GetComponentInChildren<TextMeshProUGUI>();
            lapTimeDiffImage.gameObject.SetActive(false);
            localLapTimeDiffImage = Instantiate(localLapTimeDiff_Prefab, parentObjectForUIPanel.transform).GetComponent<Image>();
            localLapTimeDiffText = localLapTimeDiffImage.GetComponentInChildren<TextMeshProUGUI>();
            localLapTimeDiffImage.gameObject.SetActive(false);
            if (MainCamera == null)
            {
                MainCamera = Instantiate(MainCamera_Prefab);
                playerCar.SetCamera(MainCamera);
            }
            if (NitroBar == null)
            {
                NitroBar = Instantiate(NitroBar_Prefab, parentObjectForUIPanel.transform);
                playerCar.SetNitroBar(NitroBar);
            }
            if (rpmGauge == null)
            {
                rpmGauge = Instantiate(rpmGauge_Prefab, parentObjectForUIPanel.transform);
                playerCar.SetRPMGauge(rpmGauge);
            }
        }
        //playerCar.SetName();
        carData = CarData_Manager.instance.GetCarDataByNumber(playerCar.GetCarNumber());
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
        if(playerNumber < playerCars.Length)
            playerCars[playerNumber] = playerCar;
        // 호스트에서만 총 플레이어 수 업데이트
        if (Object.HasStateAuthority)
            totalPlayerCount = playerNumber + 1;

        playerNumber++;

        SetFirstCheckPoint(playerCar);

        if (Object.HasStateAuthority && !isRankingStart)
        {
            isRankingStart = true;
            StartCoroutine(UpdatingRankings());
        }

        playerCar.Init();
    }
    public void SetRank(NetworkId _id)
    {
        if(!rankList.ContainsKey(_id))
            rankList.Add(_id, Instantiate(rank_Prefab, parentObjectForUIPanel.transform));
        rankList[_id].SetTargets(rankTargetPositions);
        rankList[_id].Rpc_SetPosition(0, defaultColor);
        rankList[_id].SetPlay(null, playerCar.GetName() != null ? playerCar.GetName() : playersID[playerNumber].ToString(), this, _id);
    }
    public void RemoveRank(NetworkId _id)
    {
        if (rankList.ContainsKey(_id))
        {
            Destroy(rankList[_id].gameObject);
            rankList.Remove(_id);
        }
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
        RemoveRank(networkPlayerObject.Id);
    }

    private void ExitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
    IEnumerator WaitingForCheckpoint()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(Shared.frame15);
        while (!LobbyPlayer.players.All(player => player.isSync))
        {
            yield return waitForSeconds;
        }
        if (networkRunner.GameMode == GameMode.Host)
        {
            foreach (LobbyPlayer player in LobbyPlayer.players)
                SpawnPlayer(networkRunner, player);

            while(countDown == null)
            {
                yield return waitForSeconds;
            }

            countDown.StartCountdown(3, true);
            Debug.Log("MainGame_Manager: 게임 시작 카운트다운이 시작되었습니다.");
        }
    }
    public void SetTimer(float _timer)
    { gameTimer = _timer; }
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
        if (lastCheckPointIndex == checkPointIndex)
        {
            _playerCar.SetCheckPoint(1);
            short tempLap = _playerCar.GetLap();
            tempLap++;
            _playerCar.SetLap(tempLap);
            Debug.Log("Lap " + tempLap + " CheckPoint " + checkPointIndex + " Entered by " + _playerCar.name);
            bestLapTime = gameTimer;
            _playerCar.ResetTimer();
            if (_playerCar.GetLap() >= maxLaps)
            {
                // 완주 시 GameEnd 호출 및 최종 완주 시간 전달
                _playerCar.SetFinishTime(gameTimer); // Player_Car에 완주 시간 저장 함수 추가 필요
                _playerCar.FinishRace();
                RaceEnd(); // 게임 종료 및 결과창 활성화 트리거
                bestLapTime = gameTimer;
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
        //코루틴 갱신 간격 설정(초당60프레임 정도로)
        WaitForSeconds waitForSeconds = new WaitForSeconds(Shared.frame60);
        while(true)
        {
            yield return waitForSeconds;
            if(!isRankingStart)
                yield break;
            //랭크데이터 초기화
            rankData.Clear();
            for (int i = 0; i < playerCars.Length; i++)
            {
                if (!isRankingStart)
                    yield break;
                if (playerCars[i] != null)
                    rankData.Add(playerCars[i].GetRankData());
            }
            //초기화된 랭크데이터를 바퀴수,체크포인트상황,체크포인트와의 거리 순으로 정렬
            sortedRankData = rankData.OrderByDescending(carData => carData.lap)
                .ThenByDescending(carData => carData.currentCheckpointIndex)
                .ThenBy(carData => carData.distanceToCheckPoint)
                .ToList();
            //정렬된 랭크데이터에 따라 랭크가 표시될 위치와 색 변경
            for (int i = 0; i < sortedRankData.Count; i++)
            {
                if (!isRankingStart)
                    yield break;
                tempRank = (byte)(i + 1);
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
        }
    }

    private void SetFirstCheckPoint(Player_Car _playerCar)
    {
        _playerCar.SetNextCheckPointPosition(firstCheckPoint);
    }
    //체크포인트에 기록된 최고기록을 받아와서 코루틴 실행
    IEnumerator ShowLapTimeDifference(float _diffTime)
    {
        //코루틴 갱신 간격 설정(초당 15프레임정도)
        WaitForSeconds waitForSeconds = new WaitForSeconds(Shared.frame15);
        Debug.Log("start diff timer");
        //표시될 시간의 초기화와 오브젝트 활성화
        isLapTimeDiffShowing = true;
        lapTimeDiffImage.color = new Color(0.8f, 0.8f, 0.8f, 1f);
        lapTimeDiffText.color = new Color(1f, 0f, 0f, 1f);
        lapTimeDiffText.text = '+' + string.Format("{0:0.00}", _diffTime);
        lapTimeDiffImage.gameObject.SetActive(true);
        while (true)
        {
            yield return waitForSeconds;
            lapTimeDiffTimer += 0.04f;
            //3초뒤 비활성화 코루틴 종료
            if (lapTimeDiffTimer > 3f)
            {
                lapTimeDiffImage.gameObject.SetActive(false);
                lapTimeDiffTimer = 0f;
                isLapTimeDiffShowing = false;
                yield break;
            }
            //2초뒤 3초 천천히 사라지기 시작
            else if (lapTimeDiffTimer > 2f)
            {
                lapTimeDiffText.color = new Color(1f, 0f, 0f, Mathf.Lerp(1f, 0f, lapTimeDiffTimer - 2f));
                lapTimeDiffImage.color = new Color(0.8f, 0.8f, 0.8f, Mathf.Lerp(1f, 0f, lapTimeDiffTimer - 2f));
            }
        }
    }
    //체크포인트에 기록된 개인최고기록을 받아와서 코루틴 실행
    IEnumerator ShowLocalLapTimeDifference(float _diffTime)
    {
        //개인 최고기록이 없을경우(초기값이 1000000이 넘음) 표시되지 않음
        if (Mathf.Abs(_diffTime) > 1000000)
        {
            isLocalLapTimeDiffShowing = false;
            yield break;
        }
        //코루틴 갱신 간격 설정(초당 15프레임정도)
        WaitForSeconds waitForSeconds = new WaitForSeconds(Shared.frame15);
        Debug.Log("start diff timer");
        isLocalLapTimeDiffShowing = true;
        //표시될 시간의 배경과 텍스트 색 초기화, 비교후 느리면 붉은색 빠르면 초록색
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
            //이하 내용은 최고기록 비교와 같음
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