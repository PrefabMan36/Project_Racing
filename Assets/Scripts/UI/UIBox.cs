using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIBox : MonoBehaviour
{
    [SerializeField] private ButtonData data;
    [SerializeField] private eUI_TYPE uiType;
    [SerializeField] private RectTransform uiTransform;
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI uiName;
    [SerializeField] private TextMeshProUGUI uiDescription;
    [SerializeField] private bool fading = false;
    [SerializeField] public bool fadeFinish = false;
    [SerializeField] private bool Vertical = false;
    [SerializeField] private float fadeTime = 0f;
    [SerializeField] private Vector2 startPositon;
    [SerializeField] private Vector2 endPositon;
    [SerializeField] private float originX;
    [SerializeField] private float originY;

    [SerializeField] private Button thisButton;

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
        if(uiType == eUI_TYPE.MAINBAR || uiType == eUI_TYPE.BOTTOMBAR)
            Vertical = true;
        Debug.Log($"positons{uiTransform.anchoredPosition}");
    }

    public void SetTopBar()
    {
        uiType = eUI_TYPE.MAINBAR;
        icon.sprite = Shared.ui_Manager.GetLoadedIcon("RacingGameTitleIcon");
    }

    public void SetPosition(Vector2 positon)
    {
        if (uiTransform == null)
            uiTransform = this.GetComponent<RectTransform>();
        uiTransform.anchoredPosition = positon;
        originX = positon.x;
        originY = positon.y;
        ForceOut();
    }

    public void ForceOut()
    {
        if (uiTransform == null)
            uiTransform = this.GetComponent<RectTransform>();
        if (Vertical)
        {
            if (uiTransform.anchoredPosition.y < 0)
                uiTransform.anchoredPosition = new Vector2(uiTransform.anchoredPosition.x, -Screen.height);
            else
                uiTransform.anchoredPosition = new Vector2(uiTransform.anchoredPosition.x, Screen.height);
        }
        else
        {
            if (uiTransform.anchoredPosition.x < 0)
                uiTransform.anchoredPosition = new Vector2(-Screen.width, uiTransform.anchoredPosition.y);
            else
                uiTransform.anchoredPosition = new Vector2(Screen.width, uiTransform.anchoredPosition.y);
        }
    }

    public void SetHorizontalSizeUP(float size)
    {
        if (uiTransform == null)
            uiTransform = this.GetComponent<RectTransform>();
        if (uiTransform != null)
        {
            Debug.Log($"이전 사이즈 {uiTransform.sizeDelta}");
            uiTransform.sizeDelta = new Vector2(uiTransform.sizeDelta.x + size, uiTransform.sizeDelta.y);
            Debug.Log($"바뀐 사이즈 {uiTransform.sizeDelta}");
        }
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
        startPositon = uiTransform.anchoredPosition;
        fadeFinish = false;
        if (fadeOutOrIn)
        {
            if(Vertical)
            {
                if (uiTransform.anchoredPosition.y < 0)
                    endPositon = new Vector2(uiTransform.anchoredPosition.x, -Screen.height);
                else
                    endPositon = new Vector2(uiTransform.anchoredPosition.x, Screen.height);
            }
            else
            {
                if (uiTransform.anchoredPosition.x < 0)
                    endPositon = new Vector2(-Screen.width, uiTransform.anchoredPosition.y);
                else
                    endPositon = new Vector2(Screen.width, uiTransform.anchoredPosition.y);
            }
        }
        else
        {
            if (Vertical)
                endPositon = new Vector2(uiTransform.anchoredPosition.x, originY);
            else
                endPositon = new Vector2(originX, uiTransform.anchoredPosition.y);
        }
        while (fadeTime < 0.45f)
        {
            yield return waitForSeconds;
            uiTransform.anchoredPosition = Vector2.Lerp(startPositon, endPositon, fadeTime / 0.45f);
            fadeTime += Shared.frame30;
        }
        fading = false;
        fadeFinish = true;
        fadeTime = 0f;
        Debug.Log($"fade complete {fadeOutOrIn}");
    }

    public void OnClickButton()
    {
        if (thisButton != null)
        {

        }
    }
}
