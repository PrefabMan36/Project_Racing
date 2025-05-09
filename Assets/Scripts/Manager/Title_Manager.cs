using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Title_Manager : MonoBehaviour
{
    [SerializeField] private GameObject CreateUserMenu;
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private Text LoginID;

    private void Awake()
    {
        mainCanvas = FindAnyObjectByType<Canvas>();
    }

    public void OnClickLogin()
    {
        if (PlayerPrefs.HasKey("userName"))
        {
            Debug.Log("Profile exist: ");
            //if (Check_ID(LoginID.text))
            //    Shared.mgr_Scene.ChangeScene(eSCENE.eSCENE_MAINMENU);
        }
        else
        {
            Debug.Log("Profile not exist: ");
            Instantiate(CreateUserMenu, mainCanvas.transform);
        }
    }

    public bool Check_ID(string tempID)
    {
        if (tempID != null)
            return true;
        return false;
    }
}
