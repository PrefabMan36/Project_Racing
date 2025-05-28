using TMPro;
using UnityEngine;

public class ProfileChange_CheckBox : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI boxText;
    [SerializeField] private string userName;

    public void SetUserName(string _userName)
    {
        userName = _userName;
        boxText.text = $"�̸��� **{userName}**(��)�� �����Ͻðڽ��ϱ�?";
    }

    public void OnClickYes()
    {
        if(PlayerPrefs.HasKey("Client_Username"))
            PlayerPrefs.SetString("Client_Username", userName);
        Debug.Log($"User name {userName} changed");
        Debug.Log(PlayerPrefs.GetString("Client_Username"));
        Destroy(gameObject);
    }
    public void OnClickNo()
    {
        Destroy(gameObject);
    }
}
