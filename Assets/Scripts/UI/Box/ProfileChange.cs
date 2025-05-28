using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfileChange : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputBox;
    [SerializeField] private Button confirmButton;

    void Start()
    {
        if (inputBox != null)
        {
            inputBox = GetComponentInChildren<TMP_InputField>();
            if (PlayerPrefs.HasKey("Client_Username"))
                Client_Data.Username = PlayerPrefs.GetString("Client_Username");
            if (Client_Data.Username != string.Empty)
                inputBox.text = Client_Data.Username;
            Debug.Log("profile change" + GetUserName());
        }
        if(confirmButton == null)
        {
            confirmButton = GetComponentInChildren<Button>();
            confirmButton.onClick.AddListener(Shared.setting_Manager.OnClickProfileChangeCheck);
        }
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
