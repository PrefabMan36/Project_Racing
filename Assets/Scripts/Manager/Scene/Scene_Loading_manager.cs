using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class Scene_manager : MonoBehaviour
{
    public eSCENE curScene = eSCENE.eSCENE_TITLE;
    [SerializeField] private int nextScene;
    private bool loading;

    public void ChangeScene(eSCENE _nextScene)
    {
        if (curScene == _nextScene)
            return;
        Shared.ui_Manager.OnClickClose();
        curScene = _nextScene;
        nextScene = (int)_nextScene;
        SceneManager.LoadScene(1);
    }
    public int GetNextScene()
    {
        return nextScene;
    }
}
