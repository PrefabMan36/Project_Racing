using TMPro;
using UnityEngine;

public class CreateUserName : MonoBehaviour
{
    [SerializeField] private TMP_InputField userNameInputField;
    [SerializeField] private Title_Manager titleManager;

    public void SetTitleManager(Title_Manager titleManager)
        { this.titleManager = titleManager; }

    public void OnClickYes()
    {
        PlayerPrefs.SetString("Client_Username", userNameInputField.text);
        Debug.Log($"User name {userNameInputField.text} created");
        Debug.Log(PlayerPrefs.GetString("Client_Username"));
        titleManager.OnClickStart();
        Destroy(gameObject);
    }
    public void OnClickNo()
    {
        PlayerPrefs.DeleteKey("Client_Username");
        Debug.Log("User name deleted");
        Destroy(gameObject);
    }
}
