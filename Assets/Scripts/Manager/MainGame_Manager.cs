using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class MainGame_Manager : NetworkBehaviour
{
    #region 변수 (Fields)

    #region Core Game & Scene Management
    [Header("Core Game & Scene Management")]
    [SerializeField] private bool gameStart = false;
    [SerializeField] private NetworkRunner networkRunner;
    [Networked] private TickTimer sceneChangeTimer { get; set; }
    [SerializeField] private float sceneChangeDelay = 5f; // 씬 변경 지연 시간 (초)
    #endregion

    #region Player & Car Management
    [Header("Player & Car Management")]
    [SerializeField] private Player_Car playerCar;
    [SerializeField] private Player_Car[] playerCars = new Player_Car[4];
    [SerializeField] private NetworkObject localPlayer;
    [SerializeField] private Player_Car localPlayerCar;
    [SerializeField] private Dictionary<int, NetworkId> playersID = new Dictionary<int, NetworkId>();
    [SerializeField] private byte playerNumber = 0;
    [SerializeField] private Dictionary<int, Player_Car> playerCarPrefabs = new Dictionary<int, Player_Car>();
    [SerializeField] private string[] playerCarPrefabNames;
    [SerializeField] private CarData carData;
    [Networked, SerializeField] private int totalPlayerCount { get; set; } = 0;

    [SerializeField] private AssetBundle carPrefabBundle;
    [SerializeField] private bool areCarPrefabsLoaded = false;
    [SerializeField] private List<AsyncOperationHandle<GameObject>> loadedCarHandles = new List<AsyncOperationHandle<GameObject>>();
    #endregion

    #region Track & Checkpoint Management
    [Header("Track & Checkpoint Management")]
    [SerializeField] private Transform[] spawnPosition = new Transform[4];
    [SerializeField] private float spawnPointSpacing = 2.5f;
    [SerializeField] private float spawnPointVerticalOffset = 1.0f;
    private string trackName = "eSCENE_CITY_NIGHT";
    [SerializeField] private TrackData tracksData;
    [Networked, SerializeField] private int lastCheckPointIndex { get; set; } = 0;
    [SerializeField] private CheckPoint checkPoint_Prefab;
    [SerializeField] private CheckPoint tempCheckPoint;
    [SerializeField] private CheckPoint firstCheckPoint;
    [SerializeField] private CheckPoint lastCheckPoint;
    [SerializeField] private CheckPoint checkPoint;
    [SerializeField] private int maxLaps = 1;
    #endregion

    #region UI Prefabs & References
    [Header("UI Prefabs & References")]
    [SerializeField] private Camera MainCamera_Prefab;
    [SerializeField] private RPMGauge rpmGauge_Prefab;
    [SerializeField] private Slider NitroBar_Prefab;
    [SerializeField] private GameObject Timer_Prefab;
    [SerializeField] private GameObject lapTimeDiff_Prefab;
    [SerializeField] private GameObject localLapTimeDiff_Prefab;
    [SerializeField] private Rank rank_Prefab;
    [SerializeField] private GameObject resultUI_Prefab;
    [SerializeField] private NetworkPrefabRef countDown_Prefab;
    [SerializeField] private GameObject parentObjectForUIPanel_Prefab;
    #endregion

    #region Instantiated UI Elements
    [Header("Instantiated UI Elements")]
    [SerializeField] private Canvas MainCanvas;
    [SerializeField] private Camera MainCamera;
    [SerializeField] private Slider NitroBar;
    [SerializeField] private RPMGauge rpmGauge;
    [SerializeField] private Image timerImage;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Image lapTimeDiffImage;
    [SerializeField] private TextMeshProUGUI lapTimeDiffText;
    [SerializeField] private Image localLapTimeDiffImage;
    [SerializeField] private TextMeshProUGUI localLapTimeDiffText;
    [SerializeField] private GameObject resultUI;
    [SerializeField] private RaceResultBox raceResultBox;
    [SerializeField] private CountDown countDown;
    [SerializeField] private GameObject parentObjectForUIPanel;
    [SerializeField] private Transform[] rankPositons;
    #endregion

    #region Ranking System
    [Header("Ranking System")]
    [SerializeField] private List<Rank_Data> rankData = new List<Rank_Data>();
    [SerializeField] private List<Rank_Data> sortedRankData = new List<Rank_Data>();
    [Networked, Capacity(4), SerializeField] private NetworkDictionary<NetworkId, byte> rank => default;
    [SerializeField] private byte tempRank;
    [SerializeField ]private bool isRankingStart = false;
    [SerializeField] private Dictionary<NetworkId, Rank> rankList = new Dictionary<NetworkId, Rank>();
    [SerializeField] private Vector3[] rankTargetPositions = new Vector3[4];
    #endregion

    #region Rank Colors
    [Header("Rank Colors")]
    private Color tempColor;
    private Color firstPlaceColor = new Color(1.0f, 0.843f, 0.0f, 0.7f); // 1등 색상 (골드)
    private Color secondPlaceColor = new Color(0.769f, 0.769f, 0.769f, 0.7f); // 2등 색상 (실버)
    private Color thirdPlaceColor = new Color(0.815f, 0.486f, 0.222f, 0.7f); // 3등 색상 (브론즈)
    private Color defaultColor = new Color(0.65f, 0.65f, 0.65f, 0.8f); // 그 외 등수 색상 또는 기본 색상
    #endregion

    #region Race Results & Completion
    [Header("Race Results & Completion")]
    [SerializeField] private bool shouldShowResults;
    [Networked, Capacity(4)] private NetworkDictionary<NetworkId, float> finishedPlayerTimes => default;
    [Networked, Capacity(4)] private NetworkDictionary<NetworkId, string> finishedPlayerNames => default;
    private bool isResultPanelActiveLocally = false;
    [Networked, SerializeField] private bool raceEndedByCompletion { get; set; } = false;
    [Networked, SerializeField] private bool raceEndedByTimeout { get; set; } = false;
    #endregion

    #region Countdown & Timers
    [Header("Countdown & Timers")]
    [SerializeField] private float gameTimer;
    [SerializeField] private TimeSpan gameTimeSpan;
    [SerializeField] private DateTime gameTime;
    [Networked] private bool raceFinishCountdownTriggered { get; set; } = false;
    [Networked] private TickTimer didNotFinishTimer { get; set; }
    [SerializeField] private float didNotFinishCountdownDuration = 10f;
    #endregion

    #region Lap Time Difference
    [Header("Lap Time Difference")]
    [SerializeField] bool isLapTimeDiffShowing = false;
    [SerializeField] bool isLocalLapTimeDiffShowing = false;
    [SerializeField] private float lapTimeDiffTimer = 0f;
    [SerializeField] private float localLapTimeDiffTimer = 0f;
    [SerializeField] private float diffTime1;
    [SerializeField] private float diffTime2;
    [SerializeField] private float bestLapTime;
    #endregion

    #endregion

    #region Unity 생명주기 메서드 (Unity Lifecycle Methods)

    public override void Spawned()
    {
        base.Spawned();
        Runner.SetIsSimulated(Object, true);

        StartCoroutine(LoadCarPrefabsCoroutine());

        trackName = SceneManager.GetActiveScene().name;
        //var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        //var sceneInfo = new NetworkSceneInfo();
        //if (scene.IsValid)
        //    sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);

        MainCanvas = Shared.ui_Manager.GetMainCanvas();
        parentObjectForUIPanel = Instantiate(parentObjectForUIPanel_Prefab, MainCanvas.transform);

        for (int i = 0; i < rankPositons.Length; i++)
        {
            rankTargetPositions[i] = Instantiate(rankPositons[i], parentObjectForUIPanel.transform).position;
        }
        if (networkRunner == null)
            networkRunner = GameObject.Find("Session").GetComponent<NetworkRunner>();

        if (HasStateAuthority)
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

        if (Object.HasStateAuthority)
        {
            countDown = Runner.Spawn(countDown_Prefab).GetComponent<CountDown>();
            Debug.Log("CountDown 객체가 호스트에서 스폰되었습니다.");
        }
        StartCoroutine(CheckCountDownSpawned());

        sceneChangeTimer = TickTimer.None;
        raceFinishCountdownTriggered = false;
    }

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
    }

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();

        // DNF Timer Logic
        if (Object.HasStateAuthority && didNotFinishTimer.IsRunning && didNotFinishTimer.Expired(Runner))
        {
            didNotFinishTimer = TickTimer.None;
            RPC_ForceRaceEnd();
            Debug.Log("DNF 타이머 만료. 레이스를 강제 종료합니다.");
        }

        // Result UI Logic
        if (resultUI == null) return;
        if (resultUI != null)
        {
            if (localPlayerCar == null && localPlayer != null)
                localPlayerCar = localPlayer.GetComponent<Player_Car>();

            shouldShowResults = raceEndedByTimeout || raceEndedByCompletion || (localPlayerCar != null && localPlayerCar.GetLap() >= maxLaps);
            resultUI.SetActive(shouldShowResults);

            if (shouldShowResults && raceResultBox != null)
                raceResultBox.UpdateResultDisplay(finishedPlayerTimes, finishedPlayerNames);
        }

        // Scene Change Logic (Host only)
        if (Object.HasStateAuthority)
        {
            if ((raceEndedByCompletion || raceEndedByTimeout) && !sceneChangeTimer.IsRunning && sceneChangeTimer.ExpiredOrNotRunning(Runner))
            {
                sceneChangeTimer = TickTimer.CreateFromSeconds(Runner, sceneChangeDelay);
                Debug.Log($"호스트: 씬 전환 타이머 시작! {sceneChangeDelay}초 후 씬 전환.");
            }

            if (sceneChangeTimer.IsRunning && sceneChangeTimer.Expired(Runner))
            {
                sceneChangeTimer = TickTimer.None;
                foreach (var car in playerCars)
                {
                    if (car != null && car.Object != null && car.Object.IsValid)
                    {
                        car.StopAllCoroutines();
                        Runner.Despawn(car.Object);
                    }
                }

                RPC_CleanupAndReturnToLobby();
            }
        }
    }

    #endregion

    #region 레이스 생명주기 관리 (Race Lifecycle Management)

    public void RaceStart()
    {
        foreach (Player_Car playerCar in playerCars)
        {
            if (playerCar != null)
            {
                playerCar.StartRace();
            }
        }

        isRankingStart = true;
        StartCoroutine(UpdatingRankings());

        if (!gameStart) gameStart = true;
    }

    public void RaceEnd(NetworkId finishedPlayerId, string finishedPlayerName, float finishTime)
    {
        if (!Object.HasStateAuthority) return;

        if (!finishedPlayerTimes.ContainsKey(finishedPlayerId))
        {
            finishedPlayerTimes.Add(finishedPlayerId, finishTime);
            finishedPlayerNames.Add(finishedPlayerId, finishedPlayerName);
            Debug.Log($"플레이어 {finishedPlayerName} (ID: {finishedPlayerId}) 완주 시간 기록: {finishTime}");

            if (finishedPlayerTimes.Count == 1)
            {
                didNotFinishTimer = TickTimer.CreateFromSeconds(Runner, didNotFinishCountdownDuration);
                Debug.Log($"첫 완주자 발생! {didNotFinishCountdownDuration}초 DNF 카운트다운을 시작합니다.");

                if (countDown != null)
                {
                    countDown.StartCountdown((int)didNotFinishCountdownDuration, false);
                }
            }
        }

        if (finishedPlayerTimes.Count >= totalPlayerCount)
        {
            raceEndedByCompletion = true;
            didNotFinishTimer = TickTimer.None;
            Debug.Log("호스트: 모든 플레이어가 완주했습니다.");
        }
    }

    #endregion

    #region 플레이어 및 차량 설정 (Player & Car Setup)

    public void SpawnPlayer(NetworkRunner runner, LobbyPlayer player)
    {
        var index = LobbyPlayer.players.IndexOf(player);
        var point = spawnPosition[index];

        //var profabID = player.carIndex - 1;
        if (!playerCarPrefabs.TryGetValue(player.carIndex, out Player_Car prefab))
        {
            Debug.LogError($"스폰 오류: 차량 번호 {player.carIndex}에 해당하는 프리팹이 로드되지 않았습니다.");
            return;
        }
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

        carData = CarData_Manager.instance.GetCarDataByNumber(playerCar.GetCarNumber());
        playerCar.SetCarMass(carData.Mass);
        playerCar.SetDragCoefficient(carData.dragCoefficient);
        playerCar.SetBaseEngineAcceleration(carData.baseEngineAcceleration);
        playerCar.SetEngineRPMLimit(carData.maxEngineRPM, carData.minEngineRPM);
        switch (carData.lastGear)
        {
            case 1: playerCar.SetLastGear(Car.eGEAR.eGEAR_FIRST); break;
            case 2: playerCar.SetLastGear(Car.eGEAR.eGEAR_SECOND); break;
            case 3: playerCar.SetLastGear(Car.eGEAR.eGEAR_THIRD); break;
            case 4: playerCar.SetLastGear(Car.eGEAR.eGEAR_FOURTH); break;
            case 5: playerCar.SetLastGear(Car.eGEAR.eGEAR_FIFTH); break;
            case 6: playerCar.SetLastGear(Car.eGEAR.eGEAR_SIXTH); break;
            default: Debug.Log("잘못된 lastGear설정입니다."); break;
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
        if (playersID.ContainsKey(playerNumber))
            playersID[playerNumber] = playerCar.GetComponent<NetworkObject>().Id;
        else
            playersID.Add(playerNumber, playerCar.GetComponent<NetworkObject>().Id);

        if (playerNumber < playerCars.Length)
            playerCars[playerNumber] = playerCar;

        if (Object.HasStateAuthority)
            totalPlayerCount = playerNumber + 1;

        playerNumber++;

        SetRank(spawnedCar.Object.Id);
        SetFirstCheckPoint(playerCar);

        rank.Add(playerCar.GetComponent<NetworkObject>().Id, 0);

        playerCar.Init();
    }

    public void OnJoinPlayer(NetworkObject networkPlayerObject)
    {
        playersID[playerNumber] = networkPlayerObject.Id;
        Debug.Log($"Player joined: {networkPlayerObject.Id} at index {playerNumber}");
        rank.Add(networkPlayerObject.Id, 0);
    }

    public void OnLeftPlayer(NetworkObject networkPlayerObject)
    {
        rank.Remove(networkPlayerObject.Id);
        Destroy(rankList[networkPlayerObject.Id].gameObject);
        RemoveRank(networkPlayerObject.Id);
    }

    #endregion

    #region 트랙 및 체크포인트 로직 (Track & Checkpoint Logic)

    private void LoadAndSetupTrack()
    {
        tracksData = TrackData_Manager.instance.GetTrackDataByName(trackName);
        if (tracksData != null)
        {
            bool lastcheck = false;
            lastCheckPointIndex = tracksData.Checkpoints.Count - 1;
            firstCheckPoint = networkRunner.Spawn(checkPoint_Prefab);
            CheckPoint checkPoint = firstCheckPoint;
            checkPoint.SetCheckPointIndex(0 + 1, tracksData.Checkpoints[0].Position, tracksData.Checkpoints[0].Rotation, tracksData.Checkpoints[0].Scale, lastcheck);
            for (int i = 1; i < tracksData.Checkpoints.Count; i++)
            {
                CheckPoint tempCheckPoint = checkPoint;
                checkPoint = networkRunner.Spawn(checkPoint_Prefab);
                tempCheckPoint.SetNextCheckPoint(checkPoint);

                if (i == lastCheckPointIndex)
                { lastcheck = true; }
                else
                { lastcheck = false; }

                checkPoint.SetCheckPointIndex(i + 1, tracksData.Checkpoints[i].Position, tracksData.Checkpoints[i].Rotation, tracksData.Checkpoints[i].Scale, lastcheck);
            }
            GenerateSpawnPointsFromCheckpoint(lastCheckPoint);
            lastCheckPointIndex = lastCheckPoint.GetCheckPointIndex();
            lastCheckPoint.SetNextCheckPoint(firstCheckPoint);
            LobbyPlayer.localPlayer.RPC_ChangeSyncTrackState(true);
        }
        else
        {
            Debug.LogError($"Failed to load {trackName} track data.");
        }
    }

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

    public float CheckPointChecked(Player_Car _playerCar, float _bestTime, float _localBestTime, int checkPointIndex)
    {
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
                bestLapTime = gameTimer;
                _playerCar.SetFinishTime(gameTimer);
                _playerCar.FinishRace();

                if (Object.HasStateAuthority)
                    RaceEnd(_playerCar.Object.Id, _playerCar.GetName(), gameTimer);
                else if (_playerCar.Object.HasInputAuthority)
                    RPC_PlayerFinished(gameTimer);
            }
            else
            {
                SetFirstCheckPoint(_playerCar);
                return bestLapTime;
            }
        }
        return gameTimer;
    }

    public void SetFirstCheckPoint(CheckPoint checkPoint) { firstCheckPoint = checkPoint; }
    public void SetLastCheckPoint(CheckPoint checkPoint) { lastCheckPoint = checkPoint; }
    private void SetFirstCheckPoint(Player_Car _playerCar)
    {
        _playerCar.SetNextCheckPointPosition(firstCheckPoint.transform.position);
    }
    public void SetTimer(float _timer) { gameTimer = _timer; }


    #endregion

    #region 랭킹 로직 (Ranking Logic)

    public void SetRank(NetworkId _id)
    {
        Rank playerRank;
        if (!rankList.TryGetValue(_id, out playerRank))
        {
            playerRank = Instantiate(rank_Prefab, parentObjectForUIPanel.transform);
            rankList.Add(_id, playerRank);
            playerRank.Init(this, _id);
        }

        Player_Car carToUpdate = null;
        foreach (var car in playerCars)
        {
            if (car != null && car.Object.Id == _id)
            {
                carToUpdate = car;
                break;
            }
        }

        if (carToUpdate != null && playerRank != null)
        {
            playerRank.SetPlay(null, carToUpdate.GetName());
        }
    }

    public int GetRank(NetworkId rankPlayer)
    {
        if (Object == null || !Object.IsValid || rank.Count == 0)
        {
            Debug.LogWarning("GetRank 호출 시 Object가 유효하지 않거나 랭킹이 비어 있습니다.");
            return 0;
        }
        if(rank.ContainsKey(rankPlayer))
        {
            //Debug.Log($"GetRank 호출: {rankPlayer}의 랭킹은 {rank[rankPlayer]}입니다.");
            return rank[rankPlayer];
        }
        else
        {
            Debug.LogWarning($"GetRank 호출 시 랭킹에 {rankPlayer}가 없습니다.");
            return 0;
        }
    }

    public void RemoveRank(NetworkId _id)
    {
        if (rankList.ContainsKey(_id))
        {
            Destroy(rankList[_id].gameObject);
            rankList.Remove(_id);
        }
    }

    public Vector3[] GetRankPositions() { return rankTargetPositions; }

    #endregion

    #region RPC (Remote Procedure Calls)

    [Rpc(RpcSources.Proxies, RpcTargets.StateAuthority)]
    public void RPC_PlayerFinished(float finishTime, RpcInfo info = default)
    {
        Player_Car finishedCar = null;
        foreach (var car in playerCars)
        {
            if (car != null && car.Object.InputAuthority == info.Source)
            {
                finishedCar = car;
                break;
            }
        }

        if (finishedCar != null)
        {
            NetworkId playerId = finishedCar.Object.Id;
            string playerName = finishedCar.GetName();
            RaceEnd(playerId, playerName, finishTime);
        }
        else
        {
            Debug.LogWarning($"RPC를 보낸 플레이어(Player {info.Source})의 차를 찾을 수 없습니다.");
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.StateAuthority)]
    public void RPC_ForceRaceEnd()
    {
        Debug.Log("RPC_ForceRaceEnd 호출됨: 레이스가 강제 종료됩니다.");
        foreach (var car in playerCars)
        {
            if (car != null && !finishedPlayerTimes.ContainsKey(car.Object.Id))
            {
                finishedPlayerTimes.Add(car.Object.Id, 9999f);
                finishedPlayerNames.Add(car.Object.Id, car.GetName());
                Debug.Log($"플레이어 {car.GetName()} DNF 처리됨.");
            }
        }

        raceEndedByTimeout = true;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_CleanupAndReturnToLobby()
    {
        Debug.Log($"[{(Object.HasStateAuthority ? "Host" : "Client")}] 씬 전환 및 UI 정리 시작.");
        isRankingStart = false;

        foreach (var handle in loadedCarHandles)
        {
            Addressables.Release(handle);
        }
        loadedCarHandles.Clear();
        playerCarPrefabs.Clear();
        areCarPrefabsLoaded = false;
        Debug.Log("로드했던 모든 차량 프리팹을 메모리에서 해제했습니다.");

        if (Shared.ui_Manager != null)
        {
            Shared.ui_Manager.isInGame = false;
            Shared.ui_Manager.BackToMenu();
        }

        Destroy(parentObjectForUIPanel);

        if (Shared.CurrentAddressableSceneHandle.IsValid())
        {
            Addressables.Release(Shared.CurrentAddressableSceneHandle);
            Debug.Log("이전 트랙 씬을 메모리에서 해제했습니다.");
        }

        if (resultUI != null) { Destroy(resultUI); }

        if (Object.HasStateAuthority)
        {
            LobbyPlayer.localPlayer.Regroup();
            Runner.LoadScene(SceneRef.FromIndex(2));
            Debug.Log("호스트: LobbyScene 로드를 시작합니다.");
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_GameReadyAndStart()
    {
        RaceStart();
        Debug.Log("MainGame_Manager: RPC_GameReadyAndStart 수신됨. 게임 시작 준비 완료!");
    }

    #endregion

    #region 코루틴 (Coroutines)

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

    private IEnumerator WaitingForCheckpoint()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(Shared.frame15);
        while (!LobbyPlayer.players.All(player => player.isSync))
        {
            yield return waitForSeconds;
        }

        while (!areCarPrefabsLoaded)
        {
            Debug.Log("차량 프리팹 로딩을 기다리는 중...");
            yield return null; // 로드가 완료될 때까지 한 프레임 대기
        }

        if (networkRunner.GameMode == GameMode.Host)
        {
            foreach (LobbyPlayer player in LobbyPlayer.players)
                SpawnPlayer(networkRunner, player);

            while (countDown == null)
            {
                yield return waitForSeconds;
            }

            countDown.StartCountdown(3, true);
            Debug.Log("MainGame_Manager: 게임 시작 카운트다운이 시작되었습니다.");
        }
    }

    private IEnumerator UpdatingRankings()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(Shared.frame60);
        while (true)
        {
            yield return waitForSeconds;
            if (!isRankingStart)
                yield break;

            rankData.Clear();
            for (int i = 0; i < playerCars.Length; i++)
            {
                if (!isRankingStart)
                    yield break;
                if (playerCars[i] != null)
                    rankData.Add(playerCars[i].GetRankData());
            }

            sortedRankData = rankData.OrderByDescending(carData => carData.lap)
                .ThenByDescending(carData => carData.currentCheckpointIndex)
                .ThenBy(carData => carData.distanceToCheckPoint)
                .ToList();

            for (int i = 0; i < sortedRankData.Count; i++)
            {
                if (!isRankingStart)
                    yield break;
                tempRank = (byte)(i + 1);
                Debug.Log($"플레이어 {sortedRankData[i].playerId}의 랭킹 업데이트: {tempRank}");
                rank.Set(sortedRankData[i].playerId, tempRank);
            }
        }
    }

    private IEnumerator ShowLapTimeDifference(float _diffTime)
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(Shared.frame15);
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

    private IEnumerator ShowLocalLapTimeDifference(float _diffTime)
    {
        if (Mathf.Abs(_diffTime) > 1000000)
        {
            isLocalLapTimeDiffShowing = false;
            yield break;
        }

        WaitForSeconds waitForSeconds = new WaitForSeconds(Shared.frame15);
        Debug.Log("start diff timer");
        isLocalLapTimeDiffShowing = true;

        localLapTimeDiffImage.color = new Color(0.8f, 0.8f, 0.8f, 1f);
        if (_diffTime < 0)
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

    private IEnumerator LoadCarPrefabsCoroutine()
    {
        areCarPrefabsLoaded = false;
        playerCarPrefabs.Clear();
        loadedCarHandles.Clear();

        var neededCarIndices = new HashSet<int>();
        foreach (var player in LobbyPlayer.players)
        {
            neededCarIndices.Add(player.carIndex);
        }
        Debug.Log($"이번 경기에 필요한 차량 종류: {neededCarIndices.Count}개");

        foreach (int carIndex in neededCarIndices)
        {
            CarData spec = CarData_Manager.instance.GetCarDataByNumber(carIndex);
            if (spec == null)
            {
                Debug.LogError($"CarData_Manager에 차량 번호 {carIndex}에 대한 데이터가 없습니다.");
                continue;
            }

            string address = $"Assets/Prefabs/Cars/ForPlayer/{spec.fileName}.prefab";

            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(address);
            loadedCarHandles.Add(handle);

            yield return handle;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                playerCarPrefabs[carIndex] = handle.Result.GetComponent<Player_Car>();
                Debug.Log($"Addressable 프리팹 '{address}' (차량 번호: {carIndex}) 로드 완료.");
            }
            else
            {
                Debug.LogError($"Addressable 주소 '{address}'에서 프리팹을 로드할 수 없습니다.");
            }
        }

        areCarPrefabsLoaded = true;
        Debug.Log("모든 차량 프리팹 로딩 및 설정이 완료되었습니다.");
    }

    #endregion

    #region 유틸리티 (Utilities)
    private void ExitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
    #endregion
}