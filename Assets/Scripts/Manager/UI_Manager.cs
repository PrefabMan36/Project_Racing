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
            Debug.LogError("Panel Prefab�� �Ҵ���� �ʾҽ��ϴ�. ���� �г��� ������ �� �����ϴ�.");

        StartCoroutine(CallAsyncMethodAsCoroutine(LoadIconsAsync()));
    }
    /// <summary>
    /// �����ܵ��� �񵿱������� �ε��մϴ�.
    /// </summary>
    private async Task LoadIconsAsync()
    {
        //Ư�� �����ܸ� �����ؼ� �ε�
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
                    Debug.LogError($"������ �ε� ����: {filename} - {webRequest.error}");
                else
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(webRequest);
                    if(texture != null)
                    {
                        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                        icons.Add(filename, sprite);
                        Debug.Log($"������ �ε� ���� �� �߰�: {filename}");
                    }
                    else
                        Debug.LogError($"Texture2D ���� ����: {filename}");
                }
            }
        }
        Debug.Log("��� ������ �ε� �õ� �Ϸ�.");
        await InitializeButtons();
    }
    /// <summary>
    /// �񵿱� Task�� �ڷ�ƾ���� �����մϴ�.
    /// </summary>
    /// <param name="task">������ �񵿱� Task</param>
    private IEnumerator CallAsyncMethodAsCoroutine(Task task)
    {
        while (!task.IsCompleted)
        { yield return null; }

        if (task.IsFaulted)
            Debug.LogError($"�񵿱� �۾� �� ���� �߻�: {task.Exception}");
    }

    private void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.Escape))
            OnClickPrevious();
    }
    /// <summary>
    /// CSV ���Ϸκ��� ��ư �����͸� �ʱ�ȭ�մϴ�.
    /// </summary>
    private async Task InitializeButtons()
    {
        Debug.Log("��ư ������ �ʱ�ȭ ����");
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
            Debug.LogError("ButtonData.txt ���Ͽ��� ��ư �����͸� �ε��ϴ� �� �����߽��ϴ�. ������ ���ų� ������ ����ֽ��ϴ�.");
            return;
        }

        foreach (var entry in buttonEntries)
        {
            // string�� eUI_TYPE���� ��ȯ
            eUI_TYPE buttonType;
            if (System.Enum.TryParse(entry.ButtonType, out buttonType))
            {
                ButtonData buttonData = new ButtonData
                {
                    Name = entry.Name,
                    Description = entry.Description,
                    Icon = GetLoadedIcon(entry.IconFileName)
                };

                // �̹� �ش� Ű�� �����ϸ� ��� �޽����� ����ϰ� �ǳʶݴϴ�.
                if (buttons.ContainsKey(buttonType))
                {
                    Debug.LogWarning($"�ߺ��� ButtonType�� �����Ǿ����ϴ�: {buttonType}. �� �׸��� �ǳʶݴϴ�.");
                }
                else
                {
                    buttons.Add(buttonType, buttonData);
                    Debug.Log($"��ư ������ �߰� ����: {buttonType} - {entry.Name}");
                }
            }
            else
            {
                Debug.LogWarning($"�� �� ���� ButtonType: {entry.ButtonType}. �� �׸��� �ǳʶݴϴ�.");
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
            Debug.LogError("Main Menu Prefab�� �Ҵ���� �ʾҽ��ϴ�.");

        //NullŸ�� ��ư�� ���� �߰�
        ButtonData nullButtonData = new ButtonData();
        nullButtonData.Name = "";
        nullButtonData.Description = "";
        buttons.Add(eUI_TYPE.NULL, nullButtonData);
    }

    /// <summary>
    /// �ε�� ������ ��������Ʈ�� ��ȯ�մϴ�.
    /// </summary>
    /// <param name="filename">������ ���� �̸�</param>
    /// <returns>�ش� ������ ��������Ʈ, ������ null</returns>
    public Sprite GetLoadedIcon(string filename)
    {
        if (icons.ContainsKey(filename))
            return icons[filename];

        Debug.LogWarning($"'{filename}' �̸��� ���� ������ ��������Ʈ�� �ε���� �ʾҽ��ϴ�.");
        return null;
    }

    /// <summary>
    /// ���� ĵ������ ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ĵ����</returns>
    public Canvas GetMainCanvas()
    { return mainCanvas; }

    /// <summary>
    /// �־��� ��ư Ÿ�Կ� �ش��ϴ� UnityAction�� �����մϴ�.
    /// </summary>
    /// <param name="buttonType">��ư Ÿ��</param>
    /// <returns>��ư Ŭ�� �� ����� UnityAction</returns>
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
                Debug.Log($"{buttonType}�� �ش��ϴ� ��ư �޼ҵ尡 ���ǵ��� �ʾҽ��ϴ�.");
                break;
        }

        if(action != null)
            return action;
        else
        {
            Debug.LogWarning($"{buttonType}�� �ش��ϴ� ��ư �޼ҵ尡 �����ϴ�.");
            return null;
        }
    }
    
    /// <summary>
    /// �־��� ��ư Ÿ�Կ� �ش��ϴ� ButtonData�� ��ȯ�մϴ�.
    /// </summary>
    /// <param name="buttonType">��ư Ÿ��</param>
    /// <returns>ButtonData ��ü</returns>
    public ButtonData GetButtonData(eUI_TYPE BUTTON_TYPE)
    {
        if(buttons.ContainsKey(BUTTON_TYPE))
            return buttons[BUTTON_TYPE];

        Debug.LogWarning($"'{BUTTON_TYPE}' Ÿ���� ButtonData�� ã�� �� �����ϴ�.");
        return new ButtonData();
    }

    /// <summary>
    /// �ɼ� ��ư Ŭ�� �� ȣ��˴ϴ�.
    /// </summary>
    public void OnClickOption()
    {
        Debug.Log("�ɼ� ��ư Ŭ����");
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
    /// ���� ��ư Ŭ�� �� ���� �˾��� ���ϴ�.
    /// </summary>
    public void OnClickExit()
    {
        GameObject exitPopup = Instantiate(EXITPopUp_Prefab, mainCanvas.transform);
        UIs.Push(exitPopup.gameObject);
    }

    /// <summary>
    /// '�ƴϿ�' ��ư Ŭ�� �� ���� �˾��� �ݽ��ϴ�.
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
            Debug.LogError($"�˾� ������ ����ֽ��ϴ�. Popup Count: {UIs.Count}");
        }
    }
    
    /// <summary>
    /// �ܺο��� �˾��� �޾� ���ÿ� �߰��մϴ�.
    /// </summary>
    /// <param name="popup">�߰��� �˾� GameObject</param>
    public void RecivePopup(GameObject popup)
    { UIs.Push(popup); }

    /// <summary>
    /// ���� �޴� ��ư Ŭ�� �� ȣ��˴ϴ�.
    /// </summary>
    public void OnClickPrevious()
    {
        if (menus.Count > 1)
            StartCoroutine(PopOut());
        else
            Debug.LogWarning("�޴� ���ÿ� ���� �޴��� �����ϴ�.");
    }

    /// <summary>
    /// ��� �޴��� �˾��� �ݽ��ϴ�.
    /// </summary>
    public void OnClickClose()
    {
        Debug.Log("�޴� �ݱ� ��ư Ŭ����");
        int count;

        //�޴� ���� ����
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

        //UI �˾� ���� ����
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

        //�ΰ��� ������ ��� �ǳ� ��Ȱ��ȭ
        if (isInGame)
            panel.SetActive(false);
    }

    /// <summary>
    /// ���� ȭ������ ���ư��� ��ư Ŭ�� �� ȣ��˴ϴ�.
    /// </summary>
    public void OnClickToMain()
    {
        if (!isPushMenu)
            StartCoroutine(FadeToMain());
    }

    /// <summary>
    /// ���� �޴� ���� �� ȣ����(�����) ��ư Ŭ�� �� ȣ��˴ϴ�.
    /// </summary>
    public void OnClickHost()
    {
        Debug.Log("�� ȣ���� ��ư Ŭ����");
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
    /// �� ����� ��ư Ŭ�� �� ȣ��˴ϴ�.
    /// </summary>
    public void OnClickCreateRoom()
    {
        Debug.Log("�� ����� ��ư Ŭ����");
        if(lobbyMenu == null)
        {
            lobbyMenu = Instantiate(lobbyMenu_Prefab, mainCanvas.transform);
            Lobby_Manager lobby_Manager = lobbyMenu.GetComponent<Lobby_Manager>();
            if (lobby_Manager != null)
                lobby_Manager.SetLobby(Server_Data.LobbyName, Server_Data.LobbyID, Server_Data.trackIndex);
            else
                Debug.LogError("�κ� �޴� �����տ� Lobby_Manager ������Ʈ�� �����ϴ�.");
        }

        if(!isPushMenu)
        {
            lobbyMenu.gameObject.SetActive(false);
            StartCoroutine(PushMenu(lobbyMenu));
        }
    }

    /// <summary>
    /// ���� �޴� ���� �� ���� ��ư Ŭ�� �� ȣ��˴ϴ�.
    /// </summary>
    public void OnClickJoin()
    {
        Debug.Log("�� ���� ��ư Ŭ����");
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
    /// �� �����ϱ� ��ư Ŭ�� �� ȣ��˴ϴ�.
    /// </summary>
    public void OnClickJoinRoom()
    {
        Debug.Log("�� �����ϱ� ư Ŭ����");
        if (lobbyMenu == null)
        {
            lobbyMenu = Instantiate(lobbyMenu_Prefab, mainCanvas.transform);
            Lobby_Manager lobby_Manager = lobbyMenu.GetComponent<Lobby_Manager>();
            if (lobby_Manager != null)
                lobby_Manager.SetLobby(Server_Data.LobbyName, Server_Data.LobbyID, Server_Data.trackIndex);
            else
                Debug.LogError("�κ� �޴� �����տ� Lobby_Manager ������Ʈ�� �����ϴ�.");
        }

        if (!isPushMenu)
        {
            lobbyMenu.gameObject.SetActive(false);
            StartCoroutine(PushMenu(lobbyMenu));
        }
    }
    /// <summary>
    /// �� �޴��� ���ÿ� Ǫ���ϰ� �ִϸ��̼��� ó���մϴ�.
    /// </summary>
    /// <param name="nextMenu">Ǫ���� ���� �޴�</param>
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
    /// ���� �޴� ���� �� �޴��� �˾����� ���� �ִϸ��̼��� ó���մϴ�.
    /// </summary>
    /// <param name="nextMenu">�˾����� ��� ���� �޴�</param>
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
    /// ���� �޴��� ���ÿ��� ���ϰ� ���� �޴��� Ȱ��ȭ�ϴ� �ִϸ��̼��� ó���մϴ�.
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
    /// ��� �޴��� �ݰ� ���� �޴��� ���̵� ���ϴ� �ִϸ��̼��� ó���մϴ�.
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
