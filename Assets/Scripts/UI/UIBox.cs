using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIBox : MonoBehaviour
{
    [SerializeField] private ButtonData data;
    [SerializeField] private eUI_TYPE uiType;
    [SerializeField] private RectTransform uiPosition;
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI uiName;
    [SerializeField] private TextMeshProUGUI uiDescription;
    [SerializeField] private bool fading = false;
    [SerializeField] private bool Vertical = false;
    [SerializeField] private float fadeTime = 0f;
    [SerializeField] private Vector2 startPositon;
    [SerializeField] private Vector2 endPositon;
    [SerializeField] private float originX;
    [SerializeField] private float originY;

    public void SetButtonType(eUI_TYPE _type)
    {
        uiType = _type;
        SetButton();
    }
    private void SetButton()
    {
        data = Shared.ui_Manager.GetButtonData(uiType);
        if (data.Icon != null)
        { 
            if(icon != null)
                icon.sprite = data.Icon;
        }
        else
        {
            if (icon != null)
                icon.color = new Color(0, 0, 0, 0);
        }
        if (uiName != null)
            uiName.text = data.Name;
        if(uiDescription != null)
            uiDescription.text = data.Description;
        if(uiType == eUI_TYPE.TOPBAR || uiType == eUI_TYPE.BOTTOMBAR)
            Vertical = true;
    }

    public void SetPosition(Vector2 positon)
    {
        if (uiPosition == null)
            uiPosition = this.GetComponent<RectTransform>();
        uiPosition.anchoredPosition = positon;
        originX = positon.x;
        originY = positon.y;
    }

    public void StartFadeIn()
    {
        if(!fading)
        {
            fading = true;
            StartCoroutine(Fade(false));
        }
        Debug.Log("startIn");
    }

    public void StartFadeOut()
    {
        if(!fading)
        {
            fading = true;
            StartCoroutine(Fade(true));
        }
        Debug.Log("startOut");
    }

    private IEnumerator Fade(bool fadeOutOrIn)
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(Shared.frame30);
        startPositon = uiPosition.anchoredPosition;
        if(fadeOutOrIn)
        {
            if(Vertical)
            {
                if (uiPosition.anchoredPosition.y < 0)
                    endPositon = new Vector2(uiPosition.anchoredPosition.x, -Screen.height);
                else
                    endPositon = new Vector2(uiPosition.anchoredPosition.x, Screen.height);
            }
            else
            {
                if (uiPosition.anchoredPosition.x < 0)
                    endPositon = new Vector2(-Screen.width, uiPosition.anchoredPosition.y);
                else
                    endPositon = new Vector2(Screen.width, uiPosition.anchoredPosition.y);
            }
        }
        else
        {
            if (Vertical)
                endPositon = new Vector2(uiPosition.anchoredPosition.x, originY);
            else
                endPositon = new Vector2(originX, uiPosition.anchoredPosition.y);
        }
        while (true)
        {
            yield return waitForSeconds;
            uiPosition.anchoredPosition = Vector2.Lerp(startPositon, endPositon, fadeTime / 0.45f);
            fadeTime += Shared.frame30;
            if (fadeTime > 0.45f)
            {
                fading = false;
                yield break;
            }
        }
    }
}
