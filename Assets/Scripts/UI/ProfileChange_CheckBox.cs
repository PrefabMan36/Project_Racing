using TMPro;
using UnityEngine;

public class ProfileChange_CheckBox : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI boxText;
    [SerializeField] private string userName;

    public void SetUserName(string _userName)
    {
        userName = _userName;
        boxText.text = $"이름을 **{_userName}**(으)로 변경하시겠습니까?";
    }

    public void OnClickYes()
    {
        if(PlayerPrefs.HasKey("userName"))
            PlayerPrefs.SetString("userName", userName);
        Debug.Log($"User name {userName} changed");
        Debug.Log(PlayerPrefs.GetString("userName"));
        Destroy(gameObject);
    }
    public void OnClickNo()
    {
        Destroy(gameObject);
    }
}
