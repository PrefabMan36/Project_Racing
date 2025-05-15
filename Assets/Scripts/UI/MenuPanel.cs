using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuPanel : MonoBehaviour
{
    [SerializeField] private UIBox topBar;
    [SerializeField] private UIBox bottomBar;
    [SerializeField] private UIBox[] buttonL;
    [SerializeField] private eUI_TYPE[] buttonLType;
    [SerializeField] private Vector2[] buttonL_Positon;
    [SerializeField] private UIBox buttonS_Prefab;
    [SerializeField] private List<UIBox> buttonS = new List<UIBox>();
    [SerializeField] private eUI_TYPE[] buttonSType;
    [SerializeField] private Vector2[] buttonS_Positon;

    private void Start()
    {
        if (topBar != null)
            topBar.SetButtonType(eUI_TYPE.TOPBAR);
        if (bottomBar != null)
            bottomBar.SetButtonType(eUI_TYPE.BOTTOMBAR);
        for (int i = 0; i < buttonL.Length; i++)
        {
            if (buttonL[i] != null)
            {
                buttonL[i].SetButtonType(buttonLType[i]);
                buttonL[i].SetPosition(buttonL_Positon[i]);
            }
        }
        for (int i = 0; i < buttonSType.Length; i++)
        {
            if (buttonS_Prefab != null)
            {
                UIBox newButton = Instantiate(buttonS_Prefab, transform);
                newButton.SetButtonType(buttonSType[i]);
                newButton.SetPosition(buttonS_Positon[i]);
                buttonS.Add(newButton);
            }
        }
    }
}
