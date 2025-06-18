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

    [SerializeField] private GameObject clickBlocker_prefab;
    [SerializeField] private GameObject clickBlocker;

    [SerializeField] private MenuPanel mainMenu_Prefab;
    [SerializeField] private MenuPanel mainMenu;

    [SerializeField] private MenuPanel settingMenu_Prefab;
    [SerializeField] private MenuPanel settingMenu;

    [SerializeField] private MenuPanel hostingMenu_Prefab;
    [SerializeField] private MenuPanel hostingMenu;

    [SerializeField] private MenuPanel JoinMenu_Prefab;
    [SerializeField] private MenuPanel JoinMenu;

    [SerializeField] private MenuPanel lobbyMenu_Prefab;
    [SerializeField] private MenuPanel lobbyMenu;

    [SerializeField] private GameObject panel_Prefab;
    [SerializeField] private GameObject panel;

    [SerializeField] private MenuPanel tempMenu;
    [SerializeField] private GameObject tempMenuObject;

    [SerializeField] private GameObject EXITPopUp_Prefab;

    [SerializeField] private bool isPopMenu = false;
    [SerializeField] private bool isPushMenu = false;
    [SerializeField] public bool isInGame = false;

    private Dictionary<eUI_TYPE, ButtonData> buttons = new Dictionary<eUI_TYPE, ButtonData>();
    private Dictionary<string, Sprite> icons = new Dictionary<string, Sprite>();
    private void Awake()
    {
        Shared.ui_Manager = this;

        DontDestroyOnLoad(this);

        StartCoroutine(CallAsyncMethodAsCoroutine(LoadIconsAsync()));

        mainCanvas = new GameObject("MainCanvas").AddComponent<Canvas>();
        mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        mainCanvas.gameObject.AddComponent<CanvasScaler>();
        mainCanvas.gameObject.AddComponent<GraphicRaycaster>();
        DontDestroyOnLoad (mainCanvas);

        panel = Instantiate(panel_Prefab, mainCanvas.transform);

    }
    private async Task LoadIconsAsync()
    {
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
    private IEnumerator CallAsyncMethodAsCoroutine(Task task)
    {
        while (!task.IsCompleted)
        {
            yield return null;
        }

        if (task.IsFaulted)
        {
            Debug.LogError($"비동기 작업 중 오류 발생: {task.Exception}");
        }
    }

    private void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            //OnClickClose();
            OnClickPrevious();
        }
    }

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


        mainMenu = Instantiate(mainMenu, mainCanvas.transform);
        //mainMenu.SetButtonS_HorizontalSizeUP(50f);
        mainMenu.SetDistribute();
        StartCoroutine(PushMenu(mainMenu));

        ButtonData nullButtonData = new ButtonData();
        nullButtonData.Name = "";
        nullButtonData.Description = "";
        buttons.Add(eUI_TYPE.NULL, nullButtonData);
    }
    public Sprite GetLoadedIcon(string filename)
    {
        if (icons.ContainsKey(filename))
        {
            return icons[filename];
        }
        Debug.LogWarning($"'{filename}' 이름을 가진 아이콘 스프라이트가 로드되지 않았습니다.");
        return null;
    }
    public Canvas GetMainCanvas()
    {
        return mainCanvas;
    }
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
        }
        if(action != null)
            return action;
        else
        {
            Debug.Log($"{buttonType}에 해당하는 버튼 메소드가 없습니다.");
            return null;
        }
    }

    public ButtonData GetButtonData(eUI_TYPE BUTTON_TYPE)
    {
        if(buttons.ContainsKey(BUTTON_TYPE))
        {
            return buttons[BUTTON_TYPE];
        }
        Debug.LogWarning($"'{BUTTON_TYPE}' 타입의 ButtonData를 찾을 수 없습니다.");
        return new ButtonData();
    }

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

    public void OnClickExit()
    {
        GameObject exitPopup = Instantiate(EXITPopUp_Prefab, mainCanvas.transform);
        UIs.Push(exitPopup.gameObject);
    }

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

    public void RecivePopup(GameObject popup)
    {
        UIs.Push(popup);
    }

    public void OnClickPrevious()
    {
        if (menus.Count > 1)
        {
            //MenuPanel previousMenu = menus.Pop();
            //previousMenu.StartPopDown();
            //if(previousMenu == settingMenu)
            //    Shared.setting_Manager.CloseSetting();
            //menus.Peek().gameObject.SetActive(true);
            //menus.Peek().StartPopUp();
            StartCoroutine(PopOut());
        }
        else
        {
            Debug.LogWarning("메뉴 스택이 비어있습니다.");
        }
    }
    public void OnClickClose()
    {
        Debug.Log("메뉴 닫기 버튼 클릭됨");
        int count;
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
        if (isInGame)
            panel.SetActive(false);
    }

    public void OnClickToMain()
    {
        if (!isPushMenu)
            StartCoroutine(FadeToMain());
    }

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
    public void OnClickCreateRoom()
    {
        Debug.Log("방 만들기 버튼 클릭됨");
        if(lobbyMenu == null)
        {
            Lobby_Manager lobby_Manager;
            lobbyMenu = Instantiate(lobbyMenu_Prefab, mainCanvas.transform);
            lobby_Manager = lobbyMenu.GetComponent<Lobby_Manager>();
            lobby_Manager.SetLobby(Server_Data.LobbyName, Server_Data.LobbyID, Server_Data.trackIndex);
        }
        if(!isPushMenu)
        {
            lobbyMenu.gameObject.SetActive(false);
            StartCoroutine(PushMenu(lobbyMenu));
        }
    }

    public void OnClickJoin()
    {
        Debug.Log("방 참여 버튼 클릭됨");
        Shared.lobby_Network_Manager.SetJoinLobby();
        if (JoinMenu == null)
        {
            JoinMenu = Instantiate(JoinMenu_Prefab, mainCanvas.transform);
            //hostingMenu.SetButtonS_HorizontalSizeUP(50f);
            JoinMenu.ChangeTopBar(eUI_TYPE.JOIN);
        }
        if (!isPopMenu)
        {
            JoinMenu.gameObject.SetActive(false);
            StartCoroutine(PopMenu(JoinMenu));
        }
    }
    public void OnClickJoinRoom()
    {
        Debug.Log("방 참여하기 튼 클릭됨");
        if (lobbyMenu == null)
        {
            Lobby_Manager lobby_Manager;
            lobbyMenu = Instantiate(lobbyMenu_Prefab, mainCanvas.transform);
            lobby_Manager = lobbyMenu.GetComponent<Lobby_Manager>();
            lobby_Manager.SetLobby(Server_Data.LobbyName, Server_Data.LobbyID, Server_Data.trackIndex);
        }
        if (!isPushMenu)
        {
            lobbyMenu.gameObject.SetActive(false);
            StartCoroutine(PushMenu(lobbyMenu));
        }
    }

    IEnumerator PushMenu(MenuPanel nextMenu)
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(Shared.frame15);
        MenuPanel tempNextMenu = nextMenu;
        isPushMenu = true;
        clickBlocker = Instantiate(clickBlocker_prefab, mainCanvas.transform);

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

        while (tempNextMenu.GetFading())
            yield return waitForSeconds;

        Destroy(clickBlocker);
        clickBlocker = null;
        menus.Push(nextMenu);
        nextMenu.gameObject.SetActive(true);
        nextMenu.StartAllFadeIn();
        isPushMenu = false;
    }

    IEnumerator PopMenu(MenuPanel nextMenu)
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(Shared.frame15);
        MenuPanel tempNextMenu = nextMenu;
        isPopMenu = true;
        clickBlocker = Instantiate(clickBlocker_prefab, mainCanvas.transform);

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

        while (tempNextMenu.GetFading())
            yield return waitForSeconds;

        Destroy(clickBlocker);
        clickBlocker = null;

        menus.Push(nextMenu);
        nextMenu.gameObject.SetActive(true);
        nextMenu.StartPopUp();

        isPopMenu = false;
    }

    IEnumerator PopOut()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(Shared.frame15);
        isPopMenu = true;
        clickBlocker = Instantiate(clickBlocker_prefab, mainCanvas.transform);

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
        if(lobbyMenu != null)
        {
            Destroy(lobbyMenu.gameObject);
            lobbyMenu = null;
        }

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
