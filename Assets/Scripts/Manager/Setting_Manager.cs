using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Setting_Manager : MonoBehaviour
{
    [SerializeField] private List<GameObject> panels = new List<GameObject>();
    [SerializeField] private int panelIndex = 0;

    [Header("Profile Change")]
    [SerializeField] private ProfileChange ProfilePanel;
    [SerializeField] private int profilePanelIndex = 0;
    [SerializeField] private GameObject ProfilePanel_Prefab;
    [SerializeField] private ProfileChange_CheckBox profileChange_Check;
    [SerializeField] private ProfileChange_CheckBox profileChange_Check_Prefab;
    [SerializeField] private GameObject setting_Menu;

    [Header("Volume Change")]
    [SerializeField] private AudioSetting audioSettingPanel;
    [SerializeField] private int audioPanelIndex = 0;
    [SerializeField] private GameObject audioSettingPanel_Prefab;
    [SerializeField] private float volume;

    [SerializeField] private eUI_TYPE[] buttons_TYPE;
    [SerializeField] private int size;
    [SerializeField] private int buttonTypeIndex = 0;

    private void Awake()
    {
        Shared.setting_Manager = this;

        DontDestroyOnLoad(this);
        
        if (PlayerPrefs.HasKey("AudioVolume"))
            volume = PlayerPrefs.GetFloat("AudioVolume");
        else
        {
            PlayerPrefs.SetFloat("AudioVolume", 0.5f);
            volume = 0.5f;
        }
        Shared.audioVolume = volume;
        AudioListener.volume = volume;
    }

    public void SetSettingMenu(GameObject menu)
    {
        setting_Menu = menu;
    }

    public void CloseSetting()
    {
        for (int i = 0; i < panels.Count; i++)
        {
            panels[i].SetActive(false);
        }
    }

    public eUI_TYPE StartSetting()
    {
        size = buttons_TYPE.Length;
        if (buttonTypeIndex < buttons_TYPE.Length)
            return buttons_TYPE[buttonTypeIndex++];
        else return eUI_TYPE.NULL;
    }

    private void PanelOpen(int index)
    {
        for (int i = 0; i < panels.Count; i++)
        {
            panels[i].SetActive(false);
        }
        panels[index].SetActive(true);
    }

    public void OnClickProfileChange()
    {
        if (ProfilePanel == null)
        {
            ProfilePanel = Instantiate(ProfilePanel_Prefab, setting_Menu.transform).GetComponent<ProfileChange>();
            ProfilePanel.SetUserName(PlayerPrefs.GetString("userName"));
            panels.Add(ProfilePanel.gameObject);
            profilePanelIndex = panelIndex++;

            UIBox tempBox = ProfilePanel.GetComponent<UIBox>();
            tempBox.SetUIType(eUI_TYPE.PROFILESETTING);

        }
        PanelOpen(profilePanelIndex);
    }
    public void OnClickProfileChangeCheck()
    {
        profileChange_Check = Instantiate(profileChange_Check_Prefab, setting_Menu.transform);
        if(ProfilePanel != null)
            profileChange_Check.SetUserName(ProfilePanel.GetUserName());
    }

    public void OnClickAudioSetting()
    {
        if(audioSettingPanel == null)
        {
            audioSettingPanel = Instantiate(audioSettingPanel_Prefab, setting_Menu.transform).GetComponent<AudioSetting>();
            panels.Add(audioSettingPanel.gameObject);
            audioPanelIndex = panelIndex++;

            UIBox tempBox = audioSettingPanel.GetComponent<UIBox>();
            tempBox.SetUIType(eUI_TYPE.AUDIOSETTING);
        }
        PanelOpen(audioPanelIndex);
    }
}
