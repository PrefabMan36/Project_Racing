using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMChager : MonoBehaviour
{
    [SerializeField] protected eSCENE_TYPE currentSceneType;
    private void Awake()
    {
        Shared.audio_Manager.PlayBGM(currentSceneType);
    }
}
