using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Mgr_Music : MonoBehaviour
{
    [SerializeField] private List<AudioClip> BGM;
    [SerializeField] private AudioSource MusicPlayer;

    private void Awake()
    {
        if(Shared.mgr_Music == null)
        {
            Shared.mgr_Music = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        MusicPlayer = GetComponent<AudioSource>();

        string[] Audio_Path = System.IO.Directory.GetFiles("Assets/Audio/BGM", "*.wav");
        for (int i = 0; i < Audio_Path.Length; i++)
            BGM.Add((AudioClip)AssetDatabase.LoadAssetAtPath(Audio_Path[i], typeof(AudioClip)));

        MusicPlayer.clip = BGM[1];
        MusicPlayer.Play();
    }
}
