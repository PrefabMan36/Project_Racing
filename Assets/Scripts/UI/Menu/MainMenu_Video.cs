using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class MainMenu_Video : MonoBehaviour
{
    [SerializeField] private VideoPlayer video;

    private void Awake()
    {
        video = GetComponent<VideoPlayer>();
        video.time = 1;
        StartCoroutine(CheckStoped());
    }

    private IEnumerator CheckStoped()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(Shared.frame15);
        yield return waitForSeconds;
        if (!video.isPlaying)
        {
            video.Play();
        }
    }
}
