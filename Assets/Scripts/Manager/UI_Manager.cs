using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class UI_Manager : MonoBehaviour
{
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private MenuPanel mainMenu_Prefab;
    [SerializeField] private MenuPanel mainMenu;
    [SerializeField] private MenuPanel settingMenu_Prefab;
    [SerializeField] private MenuPanel settingMenu;
    [SerializeField] private Stack<MenuPanel> menus = new Stack<MenuPanel>();
    [SerializeField] private GameObject panel_Prefab;
    [SerializeField] private GameObject panel;
    [SerializeField] private MenuPanel tempMenu;
    [SerializeField] private GameObject tempMenuObject;
    [SerializeField] private bool isPushMenu = false;
    [SerializeField] private bool isInGame = false;

    private Dictionary<eUI_TYPE, ButtonData> buttons = new Dictionary<eUI_TYPE, ButtonData>();
    private Dictionary<string, Sprite> icons = new Dictionary<string, Sprite>();
    private void Awake()
    {
        Shared.ui_Manager = this;

        DontDestroyOnLoad(this);

        StartCoroutine(LoadIconsCoroutine());

        mainCanvas = new GameObject("MainCanvas").AddComponent<Canvas>();
        mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        mainCanvas.gameObject.AddComponent<CanvasScaler>();
        mainCanvas.gameObject.AddComponent<GraphicRaycaster>();
        panel = Instantiate(panel_Prefab, mainCanvas.transform);

    }
    private IEnumerator LoadIconsCoroutine()
    {
        string iconsFolderPath = Path.Combine(Application.streamingAssetsPath, "icons");
        string[] iconFilenames = {
            "RacingGameTitleIcon.png",
            "White Gear 2.png",
            "White Power Button.png",
            "White Check.png",
            "White Close 2.png",
            "White Backward 2.png",
            "Free Flat People 1 Icon.png",
            "Free Flat Move In Icon.png"
        };
        foreach (string filename in iconFilenames)
        {
            string iconFilePath = Path.Combine(iconsFolderPath, filename);
            using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(iconFilePath))
            {
                yield return webRequest.SendWebRequest();
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
        InitializeButtons();
    }

    private void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            onClickClose();
        }
        if (Input.GetKey(KeyCode.Backspace))
        {
            OnClickOption();
        }
    }

    private void InitializeButtons()
    {
        ButtonData buttonData = new ButtonData();
        buttonData.Name = "����";
        buttonData.Description = "���� ȯ�� �� ������ �����մϴ�.";
        buttonData.Icon = GetLoadedIcon("White Gear 2.png");
        buttons.Add(eUI_TYPE.SETTING, buttonData);
        buttonData = new ButtonData();
        buttonData.Name = "����";
        buttonData.Description = "������ �����մϴ�.";
        buttonData.Icon = GetLoadedIcon("White Power Button.png");
        buttons.Add(eUI_TYPE.EXIT, buttonData);
        buttonData = new ButtonData();
        buttonData.Name = "����";
        buttonData.Description = "���� ȭ������ ���ư��ϴ�.";
        buttonData.Icon = GetLoadedIcon("White Backward 2.png");
        buttons.Add(eUI_TYPE.PREVIOUS, buttonData);
        buttonData = new ButtonData();
        buttonData.Name = "�� �����";
        buttonData.Description = "�ٸ� �÷��̾ ������ �� �ִ� ���ο� ������ ����ϴ�.";
        buttonData.Icon = GetLoadedIcon("Free Flat People 1 Icon.png");
        buttons.Add(eUI_TYPE.HOST, buttonData);
        buttonData = new ButtonData();
        buttonData.Name = "�� �����ϱ�";
        buttonData.Description = "�ٸ� �÷��̾ ���� ���ӿ� �����մϴ�.";
        buttonData.Icon = GetLoadedIcon("Free Flat Move In Icon.png");
        buttons.Add(eUI_TYPE.JOIN, buttonData);
        buttonData = new ButtonData();
        buttonData.Name = "��";
        buttonData.Description = "���� �����մϴ�.";
        buttonData.Icon = GetLoadedIcon("White Check.png");
        buttons.Add(eUI_TYPE.YES, buttonData);
        buttonData = new ButtonData();
        buttonData.Name = "�ƴϿ�";
        buttonData.Description = "�ƴϿ��� �����մϴ�.";
        buttonData.Icon = GetLoadedIcon("White Close 2.png");
        buttons.Add(eUI_TYPE.NO, buttonData);
        buttonData = new ButtonData();
        buttonData.Name = "���� �޴�";
        buttonData.Description = "���ϴ� �޴��� �����ϼ���.";
        buttonData.Icon = GetLoadedIcon("RacingGameTitleIcon.png");
        buttons.Add(eUI_TYPE.MAINBAR, buttonData);

        mainMenu = Instantiate(mainMenu, mainCanvas.transform);
        mainMenu.SetButtonS_HorizontalSizeUP(50f);
        mainMenu.SetDistribute();
        mainMenu.StartAllFadeIn();
        menus.Push(mainMenu);
    }
    public Sprite GetLoadedIcon(string filename)
    {
        if (icons.ContainsKey(filename))
        {
            return icons[filename];
        }
        Debug.LogWarning($"'{filename}' �̸��� ���� ������ ��������Ʈ�� �ε���� �ʾҽ��ϴ�.");
        return null;
    }
    public ButtonData GetButtonData(eUI_TYPE BUTTON_TYPE)
    {
        if(buttons.ContainsKey(BUTTON_TYPE))
        {
            return buttons[BUTTON_TYPE];
        }
        Debug.LogWarning($"'{BUTTON_TYPE}' Ÿ���� ButtonData�� ã�� �� �����ϴ�.");
        return new ButtonData();
    }

    public void OnClickOption()
    {
        Debug.Log("�ɼ� ��ư Ŭ����");
        if(settingMenu == null)
        {
            settingMenu = Instantiate(settingMenu_Prefab, mainCanvas.transform);
            settingMenu.SetButtonS_HorizontalSizeUP(50f);
            settingMenu.ChangeTopBar(eUI_TYPE.SETTING);
        }
        if(!isPushMenu)
        {
            settingMenu.gameObject.SetActive(false);
            StartCoroutine(PushMenu(settingMenu));
        }
    }

    public void OnClickExit()
    {

    }
    public void OnClickPrevious()
    {
        if (menus.Count > 0)
        {
            MenuPanel previousMenu = menus.Pop();
            previousMenu.StartAllFadeOut();
            menus.Peek().gameObject.SetActive(true);
            menus.Peek().StartAllFadeIn();
        }
        else
        {
            Debug.LogWarning("�޴� ������ ����ֽ��ϴ�.");
        }
    }
    public void onClickClose()
    {
        Debug.Log("���� ���� ��ư Ŭ����");
        int count = menus.Count;
        MenuPanel menu;
        for (int i = 0; i < count; i++)
        {
            menu = menus.Pop();
            menu.ForceOut();
        }
        if(isInGame)
            panel.SetActive(false);
    }
    public void OnClickHost()
    {
        Debug.Log("�� ����� ��ư Ŭ����");
    }

    IEnumerator PushMenu(MenuPanel nextMenu)
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(Shared.frame15);
        isPushMenu = true;
        if (menus.Count > 0)
        {
            tempMenu = menus.Peek();
            tempMenuObject = tempMenu.gameObject;
            tempMenu.StartAllFadeOut();
        }

        while (tempMenuObject.activeSelf)
            yield return waitForSeconds;

        menus.Push(nextMenu);
        nextMenu.gameObject.SetActive(true);
        nextMenu.StartAllFadeIn();
    }
}
