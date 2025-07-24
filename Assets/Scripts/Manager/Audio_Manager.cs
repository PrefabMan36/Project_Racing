using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets; // Addressables 사용을 위해 추가
using UnityEngine.ResourceManagement.AsyncOperations; // Addressables 사용을 위해 추가

public class Audio_Manager : Manager
{
    private AudioSource bgmAudioSource;
    private Dictionary<eSCENE_TYPE, List<BGMData>> bgmDataDictionary = new Dictionary<eSCENE_TYPE, List<BGMData>>();

    private AsyncOperationHandle<AudioClip> currentBgmHandle;

    private const string BGM_ADDRESSABLE_PATH = "Assets/Audio/BGM/";

    private void Awake()
    {
        OnStart();
        if (Shared.audio_Manager == null)
            Shared.audio_Manager = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        bgmAudioSource = gameObject.AddComponent<AudioSource>();
        bgmAudioSource.playOnAwake = false;
        bgmAudioSource.loop = true;

        LoadBGMData();
    }

    private void OnDestroy()
    {
        if (currentBgmHandle.IsValid())
        {
            Addressables.Release(currentBgmHandle);
            Debug.Log("[AudioManager] OnDestroy에서 BGM 리소스를 해제했습니다.");
        }
    }

    private async void LoadBGMData()
    {
        var bgmList = await CSVParser.ParseCSV<BGMData>(
            "BGMList.csv",
            registerClassMapAction: context => context.RegisterClassMap<BGMDataMap>()
        );
        if (bgmList == null || bgmList.Count == 0)
        {
            Debug.LogError("[AudioManager] BGM 데이터를 로드하지 못했습니다.");
            return;
        }

        foreach (var bgmData in bgmList)
        {
            if (!bgmDataDictionary.ContainsKey(bgmData.State))
            {
                bgmDataDictionary[bgmData.State] = new List<BGMData>();
            }
            bgmDataDictionary[bgmData.State].Add(bgmData);
        }

        Debug.Log("[AudioManager] BGM 데이터 로드 완료.");

        PlayBGM(eSCENE_TYPE.eSCENE_TITLE);
    }

    public void PlayBGM(eSCENE_TYPE sceneType)
    {
        if (bgmDataDictionary.TryGetValue(sceneType, out List<BGMData> bgmList) && bgmList.Count > 0)
        {
            var bgmData = bgmList[Random.Range(0, bgmList.Count)];
            StartCoroutine(LoadAndPlayAudioAddressable(bgmData.FileName));
        }
        else
        {
            Debug.LogWarning($"[AudioManager] {sceneType}에 해당하는 BGM을 찾을 수 없습니다.");
            StopBGM();
        }
    }

    public void StopBGM()
    {
        bgmAudioSource.Stop();
        if (currentBgmHandle.IsValid())
        {
            Addressables.Release(currentBgmHandle);
        }
    }

    /// <summary>
    /// Addressables를 사용해 오디오 클립을 로드하고 재생하는 코루틴
    /// </summary>
    private IEnumerator LoadAndPlayAudioAddressable(string audioFileName)
    {
        if (currentBgmHandle.IsValid())
        {
            Addressables.Release(currentBgmHandle);
        }

        string audioAddress = BGM_ADDRESSABLE_PATH + audioFileName;

        AsyncOperationHandle<AudioClip> loadHandle = Addressables.LoadAssetAsync<AudioClip>(audioAddress);

        currentBgmHandle = loadHandle;

        yield return loadHandle;

        if (loadHandle.Status == AsyncOperationStatus.Succeeded)
        {
            AudioClip clip = loadHandle.Result;
            bgmAudioSource.clip = clip;
            bgmAudioSource.Play();
            Debug.Log($"[AudioManager] BGM 재생: {audioFileName}");
        }
        else
        {
            Debug.LogError($"[AudioManager] Addressable 오디오 파일 로드 실패: {audioAddress}");
        }
    }
}