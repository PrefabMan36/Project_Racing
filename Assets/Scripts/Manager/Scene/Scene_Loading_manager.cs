using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class Scene_manager : MonoBehaviour
{
    public eSCENE curScene = eSCENE.eSCENE_TITLE;
    [SerializeField] private string nextScene;
    private bool loading;

    public void ChangeScene(eSCENE _e)
    {
        if (curScene == _e)
            return;
        switch (_e)
        {
            case eSCENE.eSCENE_TITLE:
                nextScene = "TitleMenu";
                curScene = eSCENE.eSCENE_TITLE;
                break;
            case eSCENE.eSCENE_MAINMENU:
                nextScene = "MainMenu";
                curScene = eSCENE.eSCENE_MAINMENU;
                break;
            case eSCENE.eSCENE_MAINGAME:
                nextScene = "MainGame";
                curScene = eSCENE.eSCENE_MAINGAME;
                break;
            case eSCENE.eSCENE_RESULT:
                nextScene = "Result";
                curScene = eSCENE.eSCENE_RESULT;
                break;
        }
        SceneManager.LoadScene("Loading");
    }
    public string GetNextScene()
    {
        return nextScene;
    }
}
