using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfileChange : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputBox;
    [SerializeField] private Button confirmButton;

    void Awake()
    {
        inputBox = GetComponentInChildren<TMP_InputField>();
        confirmButton = GetComponentInChildren<Button>();
        confirmButton.onClick.AddListener(Shared.setting_Manager.OnClickProfileChangeCheck);
    }

    public string GetUserName()
    {
        return inputBox.text;
    }
    public void SetUserName(string name)
    {
        inputBox.text = name;
    }
}
