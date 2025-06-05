using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
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

    [SerializeField] private bool UISetting = false;

    [SerializeField] private Button thisButton;

    private void Awake()
    {
        thisButton = GetComponent<Button>();
        if (uiTransform == null)
            uiTransform = this.GetComponent<RectTransform>();
    }

    private void Start()
    {
        if (!UISetting)
            SetUIType(uiType);
    }

    public void SetOnClickAction()
    {
        if (thisButton != null && uiType != eUI_TYPE.NULL)
        {
            //thisButton.onClick.RemoveAllListeners();
            if(Shared.ui_Manager.SetButtonListener(uiType) != null)
                thisButton.onClick.AddListener(Shared.ui_Manager.SetButtonListener(uiType));
            Debug.Log($"{uiType} 의 버튼이 설정되었습니다.");
        }
        else
            Debug.LogWarning($"버튼이 없습니다. UI타입: {uiType}");
    }
    public void SetOnClickAction(UnityAction action)
    {
        if (thisButton != null)
        {
            thisButton.onClick.RemoveAllListeners();
            thisButton.onClick.AddListener(action);
        }
        else
            Debug.LogWarning("버튼이 없습니다.");
    }

    public void SetUIType(eUI_TYPE _type)
    {
        uiType = _type;
        UISetting = true;
        SetUI();
        SetOnClickAction();
    }
    private void SetUI()
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
        icon.sprite = Shared.ui_Manager.GetLoadedIcon("RacingGameTitleIcon.png");
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
            if (uiType == eUI_TYPE.BOTTOMBAR)
                uiTransform.anchoredPosition = new Vector2(uiTransform.anchoredPosition.x, -Screen.height);
            else
                uiTransform.anchoredPosition = new Vector2(uiTransform.anchoredPosition.x, Screen.height);
        }
        else
        {
            if (uiTransform.localPosition.x < 0)
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
            uiTransform.sizeDelta = new Vector2(uiTransform.sizeDelta.x + size, uiTransform.sizeDelta.y);
        }
    }

    public void StartFadeIn()
    {
        if(!fading)
        {
            fading = true;
            StartCoroutine(Fade(false));
        }
        //Debug.Log("startIn");
    }

    public void StartFadeOut()
    {
        if(!fading)
        {
            fading = true;
            StartCoroutine(Fade(true));
        }
        //Debug.Log("startOut");
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
                if (uiType == eUI_TYPE.BOTTOMBAR)
                    endPositon = new Vector2(uiTransform.anchoredPosition.x, -Screen.height);
                else
                    endPositon = new Vector2(uiTransform.anchoredPosition.x, Screen.height);
            }
            else
            {
                if (uiTransform.localPosition.x < 0)
                    endPositon = new Vector2(-Screen.width, uiTransform.anchoredPosition.y);
                else
                    endPositon = new Vector2(Screen.width, uiTransform.anchoredPosition.y);
            }
        }
        else
        {
            if (Vertical)
                endPositon = new Vector2(uiTransform.anchoredPosition.x, originY - 5);
            else
                endPositon = new Vector2(originX, uiTransform.anchoredPosition.y);
        }
        while (fadeTime < 0.25f)
        {
            yield return waitForSeconds;
            uiTransform.anchoredPosition = Vector2.Lerp(startPositon, endPositon, fadeTime / 0.25f);
            fadeTime += Shared.frame30;
        }
        fading = false;
        fadeFinish = true;
        fadeTime = 0f;
    }
}
