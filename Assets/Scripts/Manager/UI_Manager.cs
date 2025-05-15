using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class UI_Manager : MonoBehaviour
{
    private Dictionary<eUI_TYPE, ButtonData> buttons = new Dictionary<eUI_TYPE, ButtonData>();
    private Dictionary<string, Sprite> icons = new Dictionary<string, Sprite>();
    private void Awake()
    {
        Shared.ui_Manager = this;

        DontDestroyOnLoad(this);
    }
    private void Start()
    {
        StartCoroutine(LoadIconsCoroutine());
    }
    private IEnumerator LoadIconsCoroutine()
    {
        string iconsFolderPath = Path.Combine(Application.streamingAssetsPath, "icon");
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
    private void LoadIconsFromStreamingAssets()
    {
        icons.Clear();
    }
}
