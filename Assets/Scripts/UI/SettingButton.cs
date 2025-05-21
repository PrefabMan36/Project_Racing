using UnityEngine;

public class SettingButton : MonoBehaviour
{
    private void Start()
    {
        UIBox box = GetComponent<UIBox>();
        box.SetUIType(Shared.setting_Manager.StartSetting());
    }
}
