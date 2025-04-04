using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Mgr_Title : MonoBehaviour
{
    [SerializeField] private Text LoginID;
    public void OnClickLogin()
    {
        if(Check_ID(LoginID.text))
            Shared.mgr_Scene.ChangeScene(eSCENE.eSCENE_MAINMENU);
    }

    public bool Check_ID(string tempID)
    {
        if (tempID != null)
            return true;
        return false;
    }
}
