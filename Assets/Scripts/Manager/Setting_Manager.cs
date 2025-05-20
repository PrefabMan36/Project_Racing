using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Setting_Manager : MonoBehaviour
{
    [SerializeField] private List<GameObject> panels = new List<GameObject>();
    [SerializeField] private int panelIndex = 0;

    [Header("Profile Change")]
    [SerializeField] private ProfileChange ProfilePanel;
    [SerializeField] private int ProfilePanelIndex = 0;
    [SerializeField] private GameObject ProfilePanel_Prefab;
    [SerializeField] private ProfileChange_CheckBox profileChange_Check;
    [SerializeField] private ProfileChange_CheckBox profileChange_Check_Prefab;
    [SerializeField] private GameObject setting_Menu;

    private void Awake()
    {
        Shared.setting_Manager = this;

        DontDestroyOnLoad(this);
    }
    public void SetSettingMenu(GameObject menu)
    {
        setting_Menu = menu;
    }

    public void OnClickProfileChange()
    {
        if (ProfilePanel == null)
        {
            ProfilePanel = Instantiate(ProfilePanel_Prefab, setting_Menu.transform).GetComponent<ProfileChange>();
            ProfilePanel.SetUserName(PlayerPrefs.GetString("userName"));
            panels.Add(ProfilePanel.gameObject);
            ProfilePanelIndex = panelIndex++;

            UIBox tempBox = ProfilePanel.GetComponent<UIBox>();
            tempBox.SetUIType(eUI_TYPE.PROFILESETTING);

        }
        else
        {
            for (int i = 0; i < panels.Count; i++)
            {
                panels[i].SetActive(false);
            }
            panels[ProfilePanelIndex].SetActive(true);
        }
    }
    public void OnClickProfileChangeCheck()
    {
        profileChange_Check = Instantiate(profileChange_Check_Prefab, setting_Menu.transform);
        if(ProfilePanel != null)
            profileChange_Check.SetUserName(ProfilePanel.GetUserName());
    }
}
