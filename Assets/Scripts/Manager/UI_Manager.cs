using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Events;
using System.Threading.Tasks;
using CsvHelper.Configuration;
using System.Globalization;

public class UI_Manager : MonoBehaviour
{
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private Stack<MenuPanel> menus = new Stack<MenuPanel>();
    [SerializeField] private Stack<GameObject> UIs = new Stack<GameObject>();


    [Header("Menus")]
    [SerializeField] private GameObject clickBlocker;
    [SerializeField] private MenuPanel mainMenu;
    [SerializeField] private MenuPanel settingMenu;
    [SerializeField] private MenuPanel hostingMenu;
    [SerializeField] private MenuPanel joinMenu;
    [SerializeField] private MenuPanel lobbyMenu;
    [SerializeField] private GameObject panel;

    [Header("Prefabs")]
    [SerializeField] private GameObject clickBlocker_prefab;
    [SerializeField] private MenuPanel mainMenu_Prefab;
    [SerializeField] private MenuPanel settingMenu_Prefab;
    [SerializeField] private MenuPanel hostingMenu_Prefab;
    [SerializeField] private MenuPanel JoinMenu_Prefab;
    [SerializeField] private MenuPanel lobbyMenu_Prefab;
    [SerializeField] private GameObject panel_Prefab;
    [SerializeField] private GameObject EXITPopUp_Prefab;

    [Header("Temporary Values")]
    [SerializeField] private MenuPanel tempMenu;
    [SerializeField] private GameObject tempMenuObject;

    [Header("Menu state Flags")]
    [SerializeField] private bool isPopMenu = false;
    [SerializeField] private bool isPushMenu = false;
    [SerializeField] public bool isInGame = false;

    [Header("Button Data")]
    [SerializeField] private Dictionary<eUI_TYPE, ButtonData> buttons = new Dictionary<eUI_TYPE, ButtonData>();
    [SerializeField] private Dictionary<string, Sprite> icons = new Dictionary<string, Sprite>();

    private void Awake()
    {
        Shared.ui_Manager = this;
        DontDestroyOnLoad(this);

        if (mainCanvas == null)
        {
            mainCanvas = new GameObject("MainCanvas").AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            mainCanvas.gameObject.AddComponent<CanvasScaler>();
            mainCanvas.gameObject.AddComponent<GraphicRaycaster>();
            DontDestroyOnLoad(mainCanvas);
        }

        if(panel_Prefab != null)
            panel = Instantiate(panel_Prefab, mainCanvas.transform);
        else
            Debug.LogError("Panel Prefab이 할당되지 않았습니다. 메인 패널을 생성할 수 없습니다.");

        StartCoroutine(CallAsyncMethodAsCoroutine(LoadIconsAsync()));
    }
    /// <summary>
    /// 아이콘들을 비동기적으로 로드합니다.
    /// </summary>
    private async Task LoadIconsAsync()
    {
        //특정 아이콘만 지정해서 로드
        string iconsFolderPath = Path.Combine(Application.streamingAssetsPath, "icons");
        string[] iconFilenames = {
            "RacingGameTitleIcon.png",
            "White Gear 2.png",
            "White Power Button.png",
            "White Check.png",
            "White Close 2.png",
            "White Backward 2.png",
            "White Person 2.png",
            "White Flag.png",
            "White Car.png",
            "Free Flat People 1 Icon.png",
            "Free Flat Move In Icon.png",
            "Free Flat Volume 3 Icon.png",
            "icon_achievement.png"
        };

        foreach (string filename in iconFilenames)
        {
            string iconFilePath = Path.Combine(iconsFolderPath, filename);
            using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(iconFilePath))
            {
                await webRequest.SendWebRequest();
                if(webRequest.result != UnityWebRequest.Result.Success)
                    Debug.LogError($"아이콘 로드 실패: {filename} - {webRequest.error}");
                else
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(webRequest);
                    if(texture != null)
                    {
                        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                        icons.Add(filename, sprite);
                        Debug.Log($"아이콘 로드 성공 및 추가: {filename}");
                    }
                    else
                        Debug.LogError($"Texture2D 생성 실패: {filename}");
                }
            }
        }
        Debug.Log("모든 아이콘 로딩 시도 완료.");
        await InitializeButtons();
    }
    /// <summary>
    /// 비동기 Task를 코루틴으로 실행합니다.
    /// </summary>
    /// <param name="task">실행할 비동기 Task</param>
    private IEnumerator CallAsyncMethodAsCoroutine(Task task)
    {
        while (!task.IsCompleted)
        { yield return null; }

        if (task.IsFaulted)
            Debug.LogError($"비동기 작업 중 오류 발생: {task.Exception}");
    }

    private void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.Escape))
            OnClickPrevious();
    }
    /// <summary>
    /// CSV 파일로부터 버튼 데이터를 초기화합니다.
    /// </summary>
    private async Task InitializeButtons()
    {
        Debug.Log("버튼 데이터 초기화 시작");
        var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            Delimiter = ",",
            IgnoreBlankLines = true,
            TrimOptions = TrimOptions.Trim,
        };

        List<ButtonDataEntry> buttonEntries = await CSVParser.ParseCSV<ButtonDataEntry>("ButtonData.csv", csvConfig);

        if (buttonEntries == null || buttonEntries.Count == 0)
        {
            Debug.LogError("ButtonData.txt 파일에서 버튼 데이터를 로드하는 데 실패했습니다. 파일이 없거나 내용이 비어있습니다.");
            return;
        }

        foreach (var entry in buttonEntries)
        {
            // string을 eUI_TYPE으로 변환
            eUI_TYPE buttonType;
            if (System.Enum.TryParse(entry.ButtonType, out buttonType))
            {
                ButtonData buttonData = new ButtonData
                {
                    Name = entry.Name,
                    Description = entry.Description,
                    Icon = GetLoadedIcon(entry.IconFileName)
                };

                // 이미 해당 키가 존재하면 경고 메시지를 출력하고 건너뜁니다.
                if (buttons.ContainsKey(buttonType))
                {
                    Debug.LogWarning($"중복된 ButtonType이 감지되었습니다: {buttonType}. 이 항목은 건너뜁니다.");
                }
                else
                {
                    buttons.Add(buttonType, buttonData);
                    Debug.Log($"버튼 데이터 추가 성공: {buttonType} - {entry.Name}");
                }
            }
            else
            {
                Debug.LogWarning($"알 수 없는 ButtonType: {entry.ButtonType}. 이 항목은 건너뜁니다.");
            }
        }

        if (mainMenu_Prefab != null)
        {
            mainMenu = Instantiate(mainMenu, mainCanvas.transform);
            //mainMenu.SetButtonS_HorizontalSizeUP(50f);
            mainMenu.SetDistribute();
            StartCoroutine(PushMenu(mainMenu));
        }
        else
            Debug.LogError("Main Menu Prefab이 할당되지 않았습니다.");

        //Null타입 버튼만 따로 추가
        ButtonData nullButtonData = new ButtonData();
        nullButtonData.Name = "";
        nullButtonData.Description = "";
        buttons.Add(eUI_TYPE.NULL, nullButtonData);
    }

    /// <summary>
    /// 로드된 아이콘 스프라이트를 반환합니다.
    /// </summary>
    /// <param name="filename">아이콘 파일 이름</param>
    /// <returns>해당 아이콘 스프라이트, 없으면 null</returns>
    public Sprite GetLoadedIcon(string filename)
    {
        if (icons.ContainsKey(filename))
            return icons[filename];

        Debug.LogWarning($"'{filename}' 이름을 가진 아이콘 스프라이트가 로드되지 않았습니다.");
        return null;
    }

    /// <summary>
    /// 메인 캔버스를 반환합니다.
    /// </summary>
    /// <returns>메인 캔버스</returns>
    public Canvas GetMainCanvas()
    { return mainCanvas; }

    /// <summary>
    /// 주어진 버튼 타입에 해당하는 UnityAction을 설정합니다.
    /// </summary>
    /// <param name="buttonType">버튼 타입</param>
    /// <returns>버튼 클릭 시 실행될 UnityAction</returns>
    public UnityAction SetButtonListener(eUI_TYPE buttonType)
    {
        UnityAction action = null;
        switch (buttonType)
        {
            case eUI_TYPE.SETTING:
                action = OnClickOption;
                break;
            case eUI_TYPE.PROFILESETTING:
                action = Shared.setting_Manager.OnClickProfileChange;
                break;
            case eUI_TYPE.AUDIOSETTING:
                action = Shared.setting_Manager.OnClickAudioSetting;
                break;
            case eUI_TYPE.PREVIOUS:
                action = OnClickPrevious;
                break;
            case eUI_TYPE.EXIT:
                action = OnClickExit;
                break;
            case eUI_TYPE.NO:
                action = OnClickNo;
                break;
            case eUI_TYPE.HOST:
                action = OnClickHost;
                break;
            case eUI_TYPE.CREATEROOM:
                action = OnClickCreateRoom;
                break;
            case eUI_TYPE.JOIN:
                action = OnClickJoin;
                break;
            case eUI_TYPE.JOINROOM:
                action = OnClickJoinRoom;
                break;
            case eUI_TYPE.LEAVESESSION:
                action = OnClickToMain;
                break;
            case eUI_TYPE.CARSELECT:
                action = Shared.lobby_Manager.OnClickChangeCar;
                break;
            default:
                Debug.Log($"{buttonType}에 해당하는 버튼 메소드가 정의되지 않았습니다.");
                break;
        }

        if(action != null)
            return action;
        else
        {
            Debug.LogWarning($"{buttonType}에 해당하는 버튼 메소드가 없습니다.");
            return null;
        }
    }
    
    /// <summary>
    /// 주어진 버튼 타입에 해당하는 ButtonData를 반환합니다.
    /// </summary>
    /// <param name="buttonType">버튼 타입</param>
    /// <returns>ButtonData 객체</returns>
    public ButtonData GetButtonData(eUI_TYPE BUTTON_TYPE)
    {
        if(buttons.ContainsKey(BUTTON_TYPE))
            return buttons[BUTTON_TYPE];

        Debug.LogWarning($"'{BUTTON_TYPE}' 타입의 ButtonData를 찾을 수 없습니다.");
        return new ButtonData();
    }

    /// <summary>
    /// 옵션 버튼 클릭 시 호출됩니다.
    /// </summary>
    public void OnClickOption()
    {
        Debug.Log("옵션 버튼 클릭됨");
        if(settingMenu == null)
        {
            settingMenu = Instantiate(settingMenu_Prefab, mainCanvas.transform);
            settingMenu.SetButtonS_HorizontalSizeUP(50f);
            settingMenu.ChangeTopBar(eUI_TYPE.SETTING);
            Shared.setting_Manager.SetSettingMenu(settingMenu.gameObject);
        }
        if(!isPopMenu)
        {
            settingMenu.gameObject.SetActive(false);
            StartCoroutine(PopMenu(settingMenu));
        }
    }

    /// <summary>
    /// 종료 버튼 클릭 시 종료 팝업을 띄웁니다.
    /// </summary>
    public void OnClickExit()
    {
        GameObject exitPopup = Instantiate(EXITPopUp_Prefab, mainCanvas.transform);
        UIs.Push(exitPopup.gameObject);
    }

    /// <summary>
    /// '아니오' 버튼 클릭 시 현재 팝업을 닫습니다.
    /// </summary>
    public void OnClickNo()
    {
        GameObject closePopup;
        if (UIs.Count > 0)
        {
            closePopup = UIs.Pop();
            Destroy(closePopup.gameObject);
        }
        else
        {
            Debug.LogError($"팝업 스택이 비어있습니다. Popup Count: {UIs.Count}");
        }
    }
    
    /// <summary>
    /// 외부에서 팝업을 받아 스택에 추가합니다.
    /// </summary>
    /// <param name="popup">추가할 팝업 GameObject</param>
    public void RecivePopup(GameObject popup)
    { UIs.Push(popup); }

    /// <summary>
    /// 이전 메뉴 버튼 클릭 시 호출됩니다.
    /// </summary>
    public void OnClickPrevious()
    {
        if (menus.Count > 1)
            StartCoroutine(PopOut());
        else
            Debug.LogWarning("메뉴 스택에 이전 메뉴가 없습니다.");
    }

    /// <summary>
    /// 모든 메뉴와 팝업을 닫습니다.
    /// </summary>
    public void OnClickClose()
    {
        Debug.Log("메뉴 닫기 버튼 클릭됨");
        int count;

        //메뉴 스택 비우기
        if(menus.Count > 0)
        {
            count = menus.Count;
            MenuPanel menu;
            for (int i = 0; i < count; i++)
            {
                menu = menus.Pop();
                menu.ForceOut();
            }
        }

        //UI 팝업 스택 비우기
        if(UIs.Count > 0)
        {
            count = UIs.Count;
            GameObject popUp;
            for (int i = 0; i < count; i++)
            {
                popUp = UIs.Pop();
                Destroy(popUp);
            }
        }

        //인게임 상태일 경우 판낼 비활성화
        if (isInGame)
            panel.SetActive(false);
    }

    /// <summary>
    /// 메인 화면으로 돌아가는 버튼 클릭 시 호출됩니다.
    /// </summary>
    public void OnClickToMain()
    {
        if (!isPushMenu)
            StartCoroutine(FadeToMain());
    }

    /// <summary>
    /// 메인 메뉴 에서 방 호스팅(만들기) 버튼 클릭 시 호출됩니다.
    /// </summary>
    public void OnClickHost()
    {
        Debug.Log("방 호스팅 버튼 클릭됨");
        Shared.lobby_Network_Manager.SetCreateLobby();

        if (hostingMenu == null)
        {
            hostingMenu = Instantiate(hostingMenu_Prefab, mainCanvas.transform);
            //hostingMenu.SetButtonS_HorizontalSizeUP(50f);
            hostingMenu.ChangeTopBar(eUI_TYPE.HOST);
        }

        if (!isPopMenu)
        {
            hostingMenu.gameObject.SetActive(false);
            StartCoroutine(PopMenu(hostingMenu));
        }
    }

    /// <summary>
    /// 방 만들기 버튼 클릭 시 호출됩니다.
    /// </summary>
    public void OnClickCreateRoom()
    {
        Debug.Log("방 만들기 버튼 클릭됨");
        if(lobbyMenu == null)
        {
            lobbyMenu = Instantiate(lobbyMenu_Prefab, mainCanvas.transform);
            Lobby_Manager lobby_Manager = lobbyMenu.GetComponent<Lobby_Manager>();
            if (lobby_Manager != null)
                lobby_Manager.SetLobby(Server_Data.LobbyName, Server_Data.LobbyID, Server_Data.trackIndex);
            else
                Debug.LogError("로비 메뉴 프리팹에 Lobby_Manager 컴포넌트가 없습니다.");
        }

        if(!isPushMenu)
        {
            lobbyMenu.gameObject.SetActive(false);
            StartCoroutine(PushMenu(lobbyMenu));
        }
    }

    /// <summary>
    /// 메인 메뉴 에서 방 참여 버튼 클릭 시 호출됩니다.
    /// </summary>
    public void OnClickJoin()
    {
        Debug.Log("방 참여 버튼 클릭됨");
        Shared.lobby_Network_Manager.SetJoinLobby();

        if (joinMenu == null)
        {
            joinMenu = Instantiate(JoinMenu_Prefab, mainCanvas.transform);
            //hostingMenu.SetButtonS_HorizontalSizeUP(50f);
            joinMenu.ChangeTopBar(eUI_TYPE.JOIN);
        }

        if (!isPopMenu)
        {
            joinMenu.gameObject.SetActive(false);
            StartCoroutine(PopMenu(joinMenu));
        }
    }

    /// <summary>
    /// 방 참여하기 버튼 클릭 시 호출됩니다.
    /// </summary>
    public void OnClickJoinRoom()
    {
        Debug.Log("방 참여하기 튼 클릭됨");
        if (lobbyMenu == null)
        {
            lobbyMenu = Instantiate(lobbyMenu_Prefab, mainCanvas.transform);
            Lobby_Manager lobby_Manager = lobbyMenu.GetComponent<Lobby_Manager>();
            if (lobby_Manager != null)
                lobby_Manager.SetLobby(Server_Data.LobbyName, Server_Data.LobbyID, Server_Data.trackIndex);
            else
                Debug.LogError("로비 메뉴 프리팹에 Lobby_Manager 컴포넌트가 없습니다.");
        }

        if (!isPushMenu)
        {
            lobbyMenu.gameObject.SetActive(false);
            StartCoroutine(PushMenu(lobbyMenu));
        }
    }
    /// <summary>
    /// 새 메뉴를 스택에 푸시하고 애니메이션을 처리합니다.
    /// </summary>
    /// <param name="nextMenu">푸시할 다음 메뉴</param>
    private IEnumerator PushMenu(MenuPanel nextMenu)
    {
        panel.SetActive(!isInGame);

        WaitForSeconds waitForSeconds = new WaitForSeconds(Shared.frame15);
        clickBlocker = Instantiate(clickBlocker_prefab, mainCanvas.transform);
        isPushMenu = true;

        if (menus.Count > 0)
        {
            tempMenu = menus.Peek();
            tempMenuObject = tempMenu.gameObject;
            tempMenu.StartAllFadeOut();
        }

        if(tempMenuObject != null)
        {
            while (tempMenuObject.activeSelf)
                yield return waitForSeconds;
        }

        while (nextMenu.GetFading())
            yield return waitForSeconds;

        Destroy(clickBlocker);
        clickBlocker = null;

        menus.Push(nextMenu);
        nextMenu.gameObject.SetActive(true);
        nextMenu.StartAllFadeIn();

        isPushMenu = false;
    }
    /// <summary>
    /// 현재 메뉴 위에 새 메뉴를 팝업으로 띄우고 애니메이션을 처리합니다.
    /// </summary>
    /// <param name="nextMenu">팝업으로 띄울 다음 메뉴</param>
    IEnumerator PopMenu(MenuPanel nextMenu)
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(Shared.frame15);
        clickBlocker = Instantiate(clickBlocker_prefab, mainCanvas.transform);
        isPopMenu = true;

        if (menus.Count > 0)
        {
            tempMenu = menus.Peek();
            tempMenuObject = tempMenu.gameObject;
            tempMenu.StartPopDown();
        }

        if (tempMenuObject != null)
        {
            while (tempMenuObject.activeSelf)
                yield return waitForSeconds;
        }

        while (nextMenu.GetFading())
            yield return waitForSeconds;

        Destroy(clickBlocker);
        clickBlocker = null;

        menus.Push(nextMenu);
        nextMenu.gameObject.SetActive(true);
        nextMenu.StartPopUp();

        isPopMenu = false;
    }

    /// <summary>
    /// 현재 메뉴를 스택에서 팝하고 이전 메뉴를 활성화하는 애니메이션을 처리합니다.
    /// </summary>
    IEnumerator PopOut()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(Shared.frame15);
        clickBlocker = Instantiate(clickBlocker_prefab, mainCanvas.transform);
        isPopMenu = true;

        if (menus.Count > 0)
        {
            tempMenu = menus.Pop();
            tempMenuObject = tempMenu.gameObject;
            tempMenu.StartPopDown();
        }

        while (tempMenuObject.activeSelf)
            yield return waitForSeconds;

        tempMenu = menus.Peek();
        tempMenu.gameObject.SetActive(true);
        tempMenu.StartPopUp();

        while (tempMenu.GetPoping())
            yield return waitForSeconds;

        Destroy(clickBlocker);
        clickBlocker = null;
        isPopMenu = false;
    }
    /// <summary>
    /// 모든 메뉴를 닫고 메인 메뉴로 페이드 인하는 애니메이션을 처리합니다.
    /// </summary>
    IEnumerator FadeToMain()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(Shared.frame15);
        clickBlocker = Instantiate(clickBlocker_prefab, mainCanvas.transform);
        isPushMenu = true;

        tempMenu = menus.Pop();
        tempMenuObject = tempMenu.gameObject;
        tempMenu.StartAllFadeOut();

        while (tempMenuObject.activeSelf)
            yield return waitForSeconds;

        while(menus.Peek() != mainMenu)
            menus.Pop();

        tempMenu = menus.Peek();
        tempMenuObject = tempMenu.gameObject;
        tempMenuObject.SetActive(true);
        tempMenu.StartAllFadeIn();

        while (tempMenu.GetFading())
            yield return waitForSeconds;

        Destroy(clickBlocker);
        clickBlocker = null;

        isPushMenu = false;
    }
}
