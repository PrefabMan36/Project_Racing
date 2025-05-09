using TMPro;
using UnityEngine;

public class CreateUserName : MonoBehaviour
{
    [SerializeField] private TMP_InputField userNameInputField;
    //[SerializeField]
    public void OnClickYes()
    {
        PlayerPrefs.SetString("userName", userNameInputField.text);
        Debug.Log($"User name {userNameInputField.text} created");
        Debug.Log(PlayerPrefs.GetString("userName"));
        Destroy(gameObject);
    }
    public void OnClickNo()
    {
        PlayerPrefs.DeleteKey("userName");
        Debug.Log("User name deleted");
        Destroy(gameObject);
    }
}
