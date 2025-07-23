using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Audio_Manager : Manager
{
    private AudioSource bgmAudioSource;
    private Dictionary<eSCENE_TYPE, List<BGMData>> bgmDataDictionary = new Dictionary<eSCENE_TYPE, List<BGMData>>();
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
            var bgmData = bgmList[Random.Range(0,bgmList.Count)];
            StartCoroutine(LoadAndPlayAudio(bgmData.FileName));
        }
        else
        {
            Debug.LogWarning($"[AudioManager] {sceneType}에 해당하는 BGM을 찾을 수 없습니다.");
            bgmAudioSource.Stop();
        }
    }

    public void StopBGM()
    {
        bgmAudioSource.Stop();
    }

    private System.Collections.IEnumerator LoadAndPlayAudio(string audioFileName)
    {
        string audioPath = Path.Combine(Application.streamingAssetsPath, "Audio", "BGM", audioFileName);
        string uri = "file:///" + audioPath;

#if UNITY_ANDROID && !UNITY_EDITOR
            uri = audioPath;
#endif

        using (var www = UnityEngine.Networking.UnityWebRequestMultimedia.GetAudioClip(uri, AudioType.OGGVORBIS))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                AudioClip clip = UnityEngine.Networking.DownloadHandlerAudioClip.GetContent(www);
                bgmAudioSource.clip = clip;
                bgmAudioSource.Play();
                Debug.Log($"[AudioManager] BGM 재생: {audioFileName}");
            }
            else
            {
                Debug.LogError($"[AudioManager] 오디오 파일 로드 실패: {www.error}. Path: {uri}");
            }
        }
    }
}
