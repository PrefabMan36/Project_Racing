using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class Scene_manager : MonoBehaviour
{
    public eSCENE curScene = eSCENE.eSCENE_TITLE;
    [SerializeField] private int nextScene;

    public void ChangeScene(eSCENE _nextScene)
    {
        if (curScene == _nextScene)
            return;
        Shared.ui_Manager.OnClickClose();
        nextScene = (int)_nextScene;
        SceneManager.LoadScene(1);
    }
    public int GetNextScene()
    {
        return nextScene;
    }
    public void SetCurrentScene(eSCENE _currentScene)
    {
        curScene = _currentScene;
    }
    public bool GetSceneChangedCheck()
    {
        return curScene == (eSCENE)nextScene ? true : false;
    }
}
