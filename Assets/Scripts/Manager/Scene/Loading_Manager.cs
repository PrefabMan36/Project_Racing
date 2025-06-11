using System.Collections;
using System.Linq;
using Fusion;
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
                    if(LobbyPlayer.localPlayer != null)
                        LobbyPlayer.localPlayer.isReadyToPlay = true;
                    if (Shared.lobby_Network_Manager.GetNetRunner() != null)
                    {
                        if (LobbyPlayer.players.Count > 0 && LobbyPlayer.players.All(player => player.isReadyToPlay))
                        {
                            if (Shared.lobby_Network_Manager.GetNetRunner().IsSceneAuthority)
                            {
                                NetworkRunner tempNetRunner = Shared.lobby_Network_Manager.GetNetRunner();
                                tempNetRunner.LoadScene(SceneRef.FromIndex(nextScene));
                            }
                        }    
                    }
                    else
                    {
                        Shared.scene_Manager.SetCurrentScene((eSCENE)nextScene);
                    }
                    yield break;
                }
            }
        }
    }
}
