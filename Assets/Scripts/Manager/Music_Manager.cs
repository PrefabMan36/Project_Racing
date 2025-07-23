using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Music_Manager : Manager
{
    [SerializeField] private List<AudioClip> BGM;
    [SerializeField] private AudioSource MusicPlayer;

    private void Awake()
    {
        if(Shared.music_Manager == null)
        {
            Shared.music_Manager = this;
            OnStart();
        }
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        
    }
}
