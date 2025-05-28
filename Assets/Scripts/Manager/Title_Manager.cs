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

    [SerializeField] private GameObject fadeOut;

    private void Awake()
    {
        mainCanvas = FindAnyObjectByType<Canvas>();
    }

    public void OnClickLogin()
    {
        if (PlayerPrefs.HasKey("Client_Username"))
        {
            Debug.Log("Profile exist: ");
            Client_Data.Username = PlayerPrefs.GetString("Client_Username");
            CreateUserName createUserName = Instantiate(CreateUserMenu, mainCanvas.transform).GetComponent<CreateUserName>();
            createUserName.SetTitleManager(this);
            //if (Check_ID(LoginID.text))
            //    Shared.mgr_Scene.ChangeScene(eSCENE.eSCENE_MAINMENU);
        }
        else
        {
            Debug.Log("Profile not exist: ");
            CreateUserName createUserName = Instantiate(CreateUserMenu, mainCanvas.transform).GetComponent<CreateUserName>();
            createUserName.SetTitleManager(this);
        }
    }

    public bool Check_ID(string tempID)
    {
        if (tempID != null)
            return true;
        return false;
    }

    public void RaceStart(eSCENE targetMap)
    {
        Debug.Log("Load Scene: " + targetMap);
    }

    public void OnClickStart()
    {
        fadeOut.SetActive(true);
    }
}
