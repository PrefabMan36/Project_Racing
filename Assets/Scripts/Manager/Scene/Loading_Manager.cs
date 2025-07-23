using System.Collections;
using System.Linq;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Loading_Manager : MonoBehaviour
{
    [SerializeField] Slider progressBar;
    [SerializeField] private Image loadingImage;
    [SerializeField] TextMeshProUGUI statusText;
    
    private int nextScene;
    private LobbyPlayer localPlayer;
    private NetworkRunner runner;

    void Start()
    {
        localPlayer = LobbyPlayer.localPlayer;
        runner = Shared.lobby_Network_Manager.GetNetRunner();

        nextScene = Shared.scene_Manager.GetNextScene();
        loadingImage.sprite = Shared.room_Manager.GetSprite(nextScene);

        Shared.ui_Manager.isInGame = true;
        Shared.ui_Manager.OnLoadInGame();

        StartCoroutine(LoadScene());
    }
    IEnumerator LoadScene()
    {
        if (statusText != null)
            statusText.text = "로딩 중...";

        var op = SceneManager.LoadSceneAsync(nextScene);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
        {
            progressBar.value = Mathf.Lerp(progressBar.value, op.progress, Time.deltaTime * 5f);
            yield return null;
        }

        progressBar.value = 0.9f;

        localPlayer = LobbyPlayer.localPlayer;
        if (localPlayer && localPlayer.Object && localPlayer.Object.IsValid && localPlayer.HasInputAuthority)
        {
            localPlayer.RPC_ChangeLoadingState(true);
        }

        if (statusText != null)
            statusText.text = "다른 플레이어 대기 중...";

        if (runner != null && runner.IsServer)
        {
            yield return new WaitUntil(() => LobbyPlayer.players.All(p => p.isReadyToPlay));
            Debug.LogWarning("로딩 대기중(서버)");
            op.allowSceneActivation = true;
            Shared.audio_Manager.StopBGM();
            runner.LoadScene(SceneRef.FromIndex(nextScene), LoadSceneMode.Single);
            Debug.LogWarning("씬넘기기");
        }
        else
        {
            op.allowSceneActivation = true;

            while (!op.isDone)
            {
                progressBar.value = Mathf.Lerp(progressBar.value, 1f, Time.deltaTime * 5f);
                Debug.LogWarning("로딩 대기중(싱글/클라이언트)");
                Shared.audio_Manager.StopBGM();
                yield return null;
            }

            progressBar.value = 1f;
            if(Shared.lobby_Network_Manager.GetNetRunner() == null)
                Shared.scene_Manager.SetCurrentScene((eSCENE)nextScene);
        }
    }
}
