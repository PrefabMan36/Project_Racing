using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Mgr_Loading : MonoBehaviour
{
    private float timer;
    private string nextScene;
    [SerializeField] AsyncOperation op;
    [SerializeField] Slider progressBar;
    void Start()
    {
        nextScene = Shared.mgr_Scene.GetNextScene();
        StartCoroutine("LoadScene");
    }
    IEnumerator LoadScene()
    {
        op = SceneManager.LoadSceneAsync(nextScene);
        op.allowSceneActivation = false;
        timer = 0f;
        while (!op.isDone)
        {
            yield return null;
            timer += Time.deltaTime;
            if (op.progress < 0.9f)
            {
                progressBar.value = Mathf.Lerp(progressBar.value, op.progress, timer);
                if (progressBar.value >= op.progress)
                    timer = 0f;
            }
            else
            {
                progressBar.value = Mathf.Lerp(progressBar.value, 1f, timer);
                if (progressBar.value >= 1.0f)
                {
                    op.allowSceneActivation = true;
                    yield break;
                }
            }
        }
    }
}
