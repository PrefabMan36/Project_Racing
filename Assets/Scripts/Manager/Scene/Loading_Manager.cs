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
    private LobbyPlayer localPlayer;
    void Start()
    {
        nextScene = Shared.scene_Manager.GetNextScene();
        loadingImage.sprite = Shared.room_Manager.GetSprite(nextScene);
        StartCoroutine(LoadScene());
    }
    IEnumerator LoadScene()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(Shared.frame15);
        op = SceneManager.LoadSceneAsync(nextScene);
        op.allowSceneActivation = false;
        Shared.ui_Manager.isInGame = true;
        Shared.ui_Manager.OnClickClose();
        while (op.progress < 0.9f)
        {
            progressBar.value = op.progress;
            yield return waitForSeconds;
        }
        progressBar.value = 1f;
        localPlayer = LobbyPlayer.localPlayer;
        if (localPlayer && localPlayer.Object && localPlayer.Object.IsValid && localPlayer.HasInputAuthority)
        {
            localPlayer.RPC_ChangeLoadingState(true);
        }
        if (Shared.lobby_Network_Manager.GetNetRunner() != null)
        {
            while (!LobbyPlayer.players.All(player => player.isReadyToPlay))
            {
                yield return waitForSeconds;
            }
            op.allowSceneActivation = true;

            //if (Shared.lobby_Network_Manager.GetNetRunner().IsSceneAuthority)
            //{
            //    op.allowSceneActivation = true;
            //    NetworkRunner tempNetRunner = Shared.lobby_Network_Manager.GetNetRunner();
            //    tempNetRunner.LoadScene(SceneRef.FromIndex(nextScene));
            //}
        }
        else
        {
            op.allowSceneActivation = true;
            Shared.scene_Manager.SetCurrentScene((eSCENE)nextScene);
        }
    }
}
