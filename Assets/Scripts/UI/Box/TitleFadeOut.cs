using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleFadeOut : MonoBehaviour
{
    [SerializeField] Image panel;
    [SerializeField] float timer;


    private void OnEnable()
    {
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(Shared.frame15);
        timer = 0;
        while (timer < 1)
        {
            yield return waitForSeconds;
            panel.color = new Color(0, 0, 0, Mathf.Lerp(0, 1, timer));
            timer += 0.04f;
        }
        SceneManager.LoadScene(2);
    }
}
