using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuPanel : MonoBehaviour
{
    [SerializeField] private RectTransform uiTransform;
    [SerializeField] private UIBox topBar;
    [SerializeField] private UIBox bottomBar;
    [SerializeField] private UIBox[] buttonL;
    [SerializeField] private eUI_TYPE[] buttonLType;
    [SerializeField] private Vector2[] buttonL_Positon;
    [SerializeField] private UIBox buttonS_Prefab;
    [SerializeField] private List<UIBox> buttonS = new List<UIBox>();
    [SerializeField] private eUI_TYPE[] buttonSType;
    [SerializeField] private Vector2[] buttonS_Positon;

    private int fadeCount = 0;
    private float fadeTime = 0f;
    private bool fadeStarted = false;
    private bool fadeCheck = false;

    private bool PopStarted = false;

    private void Awake()
    {
        uiTransform = GetComponent<RectTransform>();
        if (topBar != null)
        {
            topBar.SetUIType(eUI_TYPE.MAINBAR);
            topBar.SetPosition(Vector2.zero);
        }
        if (bottomBar != null)
        {
            bottomBar.SetUIType(eUI_TYPE.BOTTOMBAR);
            bottomBar.SetPosition(Vector2.zero);
        }
        for (int i = 0; i < buttonL.Length; i++)
        {
            if (buttonL[i] != null)
            {
                buttonL[i].SetPosition(buttonL_Positon[i]);
                buttonL[i].SetUIType(buttonLType[i]);
            }
        }
        for (int i = 0; i < buttonSType.Length; i++)
        {
            if (buttonS_Prefab != null)
            {
                if(buttonS.Count < i+1)
                {
                    UIBox newButton = Instantiate(buttonS_Prefab, transform);
                    newButton.SetPosition(buttonS_Positon[i]);
                    newButton.SetUIType(buttonSType[i]);
                    buttonS.Add(newButton);
                }
                else
                {
                    buttonS[i].SetPosition(buttonS_Positon[i]);
                    buttonS[i].SetUIType(buttonSType[i]);
                }
            }
        }
    }

    public void SetButtonS_HorizontalSizeUP(float size)
    {
        Debug.Log($"사이즈 변경 {buttonS.Count} 개");
        for (int i = 0; i < buttonS.Count; i++)
        {
            if (buttonS[i] != null)
                buttonS[i].SetHorizontalSizeUP(size);
        }
    }

    public void ChangeTopBar(eUI_TYPE uiType)
    {
        if (topBar != null)
        {
            topBar.SetUIType(uiType);
            topBar.SetTopBar();
        }
    }

    public void SetDistribute()
    {
        float UIWidth = uiTransform.rect.width;
        float spacingL = UIWidth / (buttonL.Length + 1);
        float spacingS = UIWidth / (buttonS.Count + 1);
        for (int i = 0; i < buttonL.Length; i++)
        {
            if (buttonL[i] != null)
                buttonL[i].SetPosition(new Vector2(spacingL * (i + 1) - UIWidth * 0.5f, buttonL_Positon[i].y));
        }
        for (int i = 0; i < buttonS.Count; i++)
        {
            if (buttonS[i] != null)
                buttonS[i].SetPosition(new Vector2(spacingS * (i + 1) - UIWidth * 0.5f, buttonS_Positon[i].y));
        }
        Debug.Log($"UI Distribut complete");
    }

    public void StartAllFadeIn()
    {
        fadeCount = 0;
        if(!fadeStarted)
            StartCoroutine(Fade(false));
    }

    public void StartAllFadeOut()
    {
        fadeCount = 2;
        if (!fadeStarted)
            StartCoroutine(Fade(true));
    }
    
    public void StartPopUp()
    {
        if (!PopStarted)
            StartCoroutine(Poping(true));
    }

    public void StartPopDown()
    {
        if (!PopStarted)
            StartCoroutine(Poping(false));
    }

    private IEnumerator Fade(bool fadeOutOrIn)
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(Shared.frame30);
        fadeTime = 0f;
        fadeStarted = true;
        while (fadeCount < 3 && fadeCount >= 0)
        {
            if (fadeTime < 0.2f)
            {
                fadeTime += Shared.frame30;
                yield return waitForSeconds;
            }
            else if (!fadeCheck)
            {
                switch (fadeCount)
                {
                    case 0:
                        if (topBar != null)
                        {
                            if (fadeOutOrIn)
                                topBar.StartFadeOut();
                            else
                                topBar.StartFadeIn();
                        }
                        if (bottomBar != null)
                        {
                            if (fadeOutOrIn)
                                bottomBar.StartFadeOut();
                            else
                                bottomBar.StartFadeIn();
                        }
                        break;
                    case 1:
                        for (int i = 0; i < buttonL.Length; i++)
                        {
                            if (buttonL[i] != null)
                            {
                                if (fadeOutOrIn)
                                    buttonL[i].StartFadeOut();
                                else
                                    buttonL[i].StartFadeIn();
                            }
                        }
                        break;
                    case 2:
                        for (int i = 0; i < buttonS.Count; i++)
                        {
                            if (buttonS[i] != null)
                            {
                                if (fadeOutOrIn)
                                    buttonS[i].StartFadeOut();
                                else
                                    buttonS[i].StartFadeIn();
                            }
                        }
                        break;
                }
                fadeCheck = true;
            }
            else if (fadeTime < 0.15f)
            {
                fadeTime += Shared.frame30;
                yield return waitForSeconds;
            }
            else
            {
                fadeTime = 0f;
                fadeCheck = false;

                if (fadeOutOrIn)
                    fadeCount--;
                else
                    fadeCount++;

                yield return waitForSeconds;
            }
        }
        
        if (fadeOutOrIn)
        {
            while (!topBar.fadeFinish)
                yield return waitForSeconds;
            gameObject.SetActive(false);
        }
        else
        {
            while (!topBar.fadeFinish)
                yield return waitForSeconds;
        }
        fadeStarted = false;
        Debug.Log("Fade Complete");
    }
    private IEnumerator Poping(bool UpOrDown)
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(Shared.frame30);
        PopStarted = true;
        if (UpOrDown)
        {
            if (topBar != null)
                topBar.StartFadeIn();
            if (bottomBar != null)
                bottomBar.StartFadeIn();
            for (int i = 0; i < buttonL.Length; i++)
            {
                if (buttonL[i] != null)
                    buttonL[i].StartFadeIn();
            }
            for (int i = 0; i < buttonS.Count; i++)
            {
                if (buttonS[i] != null)
                    buttonS[i].StartFadeIn();
            }
        }
        else
        {
            if (topBar != null)
                topBar.StartFadeOut();
            if (bottomBar != null)
                bottomBar.StartFadeOut();
            for (int i = 0; i < buttonL.Length; i++)
            {
                if (buttonL[i] != null)
                    buttonL[i].StartFadeOut();
            }
            for (int i = 0; i < buttonS.Count; i++)
            {
                if (buttonS[i] != null)
                    buttonS[i].StartFadeOut();
            }
        }
        
        if (!UpOrDown)
        {
            while (!topBar.fadeFinish)
                yield return waitForSeconds;
            gameObject.SetActive(false);
        }
        else
        {
            while (topBar.fadeFinish)
                yield return waitForSeconds;
        }
        PopStarted = false;
    }
    public void ForceOut()
    {
        if (topBar != null)
            topBar.ForceOut();
        if (bottomBar != null)
            bottomBar.ForceOut();
        for (int i = 0; i < buttonL.Length; i++)
        {
            if (buttonL[i] != null)
                buttonL[i].ForceOut();
        }
        for (int i = 0; i < buttonS.Count; i++)
        {
            if (buttonS[i] != null)
                buttonS[i].ForceOut();
        }
        gameObject.SetActive(false);
    }

    public bool GetFading()
    {
        return fadeStarted;
    }
    public bool GetPoping()
    {
        return PopStarted;
    }
}
