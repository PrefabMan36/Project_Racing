using System.Collections;
using System.Linq;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
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

    private string nextSceneAddress;
    private AsyncOperationHandle<SceneInstance> opHandle;

    void Start()
    {
        localPlayer = LobbyPlayer.localPlayer;
        runner = Shared.lobby_Network_Manager.GetNetRunner();

        nextScene = Shared.scene_Manager.GetNextScene();
        Debug.Log(Server_Data.serverTrack.mapName);
        string sceneName = Server_Data.serverTrack.mapName;
        Debug.Log(nextScene);
        nextSceneAddress = $"Assets/Scenes/Tracks/{sceneName}/{sceneName}.unity";
        loadingImage.sprite = Shared.room_Manager.GetSprite(nextScene);

        Shared.ui_Manager.isInGame = true;
        Shared.ui_Manager.OnLoadInGame();

        StartCoroutine(LoadScene());
    }
    IEnumerator LoadScene()
    {
        if (statusText != null)
            statusText.text = "로딩 중...";

        opHandle = Addressables.LoadSceneAsync(nextSceneAddress, LoadSceneMode.Single, false);
        Shared.CurrentAddressableSceneHandle = opHandle;

        while (!opHandle.IsDone)
        {
            progressBar.value = Mathf.Lerp(progressBar.value, opHandle.PercentComplete, Time.deltaTime * 5f);
            yield return null;
        }

        //var op = SceneManager.LoadSceneAsync(nextScene);
        //op.allowSceneActivation = false;

        //while (op.progress < 0.9f)
        //{
        //    progressBar.value = Mathf.Lerp(progressBar.value, op.progress, Time.deltaTime * 5f);
        //    yield return null;
        //}

        //progressBar.value = 0.9f;
        if (opHandle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
        {
            localPlayer = LobbyPlayer.localPlayer;
            if (localPlayer && localPlayer.Object && localPlayer.Object.IsValid && localPlayer.HasInputAuthority)
            {
                localPlayer.RPC_ChangeLoadingState(true);
            }

            if (statusText != null)
                statusText.text = "다른 플레이어 대기 중...";

            if (runner != null && runner.IsServer)
            {
                Debug.LogWarning("로딩 대기중(서버)");
                yield return new WaitUntil(() => LobbyPlayer.players.All(p => p.isReadyToPlay));
                //op.allowSceneActivation = true;

                var activationOperation = opHandle.Result.ActivateAsync();
                while (!activationOperation.isDone)
                {
                    yield return null;
                }

                Shared.audio_Manager.StopBGM();
                //runner.LoadScene(SceneRef.FromIndex(nextScene), LoadSceneMode.Single);
                Debug.LogWarning("씬넘기기 완료 (서버)");
            }
            else
            {
                Debug.LogWarning("로딩 대기중(클라이언트/싱글)");

                var activationOperation = opHandle.Result.ActivateAsync();

                while (!activationOperation.isDone)
                {
                    // 0.9에서 1.0 사이를 채우도록 progress 값을 보정합니다.
                    float progress = Mathf.Clamp01(activationOperation.progress / 0.9f);
                    progressBar.value = Mathf.Lerp(progressBar.value, progress, Time.deltaTime * 5f);
                    yield return null;
                }

                Shared.audio_Manager.StopBGM();
                progressBar.value = 1f;
                //opHandle.allowSceneActivation = true;
                //op.allowSceneActivation = true;

                //while (!op.isDone)
                //{
                //    progressBar.value = Mathf.Lerp(progressBar.value, 1f, Time.deltaTime * 5f);
                //    Debug.LogWarning("로딩 대기중(싱글/클라이언트)");
                //    Shared.audio_Manager.StopBGM();
                //    yield return null;
                //}

                progressBar.value = 1f;
                if (Shared.lobby_Network_Manager.GetNetRunner() == null)
                    Shared.scene_Manager.SetCurrentScene((eSCENE)nextScene);
            }
        }
        else
        {
            Debug.LogError($"씬 로딩 실패: {nextSceneAddress}");
            if (statusText != null)
                statusText.text = "로딩 실패";
        }
    }
}
