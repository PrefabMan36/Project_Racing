using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AudioSetting : MonoBehaviour
{
    [SerializeField] AudioListener listener;
    [SerializeField] Slider slider;
    [SerializeField] TextMeshProUGUI changeText;
    [SerializeField] float volume;

    [SerializeField] float chagingTime;
    [SerializeField] bool changeing = false;

    private void Awake()
    {
        listener = Camera.main.GetComponent<AudioListener>();
        volume = Shared.audioVolume;
        slider.value = volume;
        changeText.text = "";
    }

    public void OnSliderChange()
    {
        volume = slider.value;
        AudioListener.volume = volume;
    }

    public void OnClickChange()
    {
        volume = slider.value;
        Shared.audioVolume = volume;
        if (!changeing)
        {
            chagingTime = 0f;
            changeText.text = "변경되었습니다.";
            changeText.color = Color.green;
            StartCoroutine(Changing());
        }
    }

    public void OnClickRevert()
    {
        volume = Shared.audioVolume;
        slider.value = volume;
        AudioListener.volume = volume;
        if (!changeing)
        {
            chagingTime = 0f;
            changeText.text = "취소되었습니다.";
            changeText.color = Color.red;
            StartCoroutine(Changing());
        }
    }
    IEnumerator Changing()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(Shared.frame15);
        changeText.gameObject.SetActive(true);
        while (chagingTime < 2)
        {
            yield return waitForSeconds;
            chagingTime += Shared.frame15;
        }
        changeText.gameObject.SetActive(false);
    }
}
