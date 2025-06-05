using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Loading_Manager : MonoBehaviour
{
    private float timer;
    private int nextScene;
    [SerializeField] private Image loadingImage;
    [SerializeField] AsyncOperation op;
    [SerializeField] Slider progressBar;
    void Start()
    {
        nextScene = Shared.scene_Manager.GetNextScene();
        loadingImage.sprite = Shared.room_Manager.GetSprite(nextScene);
        StartCoroutine(LoadScene());
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
                    Shared.scene_Manager.SetCurrentScene((eSCENE)nextScene);
                    yield break;
                }
            }
        }
    }
}
