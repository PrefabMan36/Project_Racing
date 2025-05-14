using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBox : MonoBehaviour
{
    [SerializeField] GameObject panel_prefab;
    [SerializeField] GameObject panel;
    [SerializeField] RectTransform panel_size;
    [SerializeField] Vector2 anchoredPosition;
    [SerializeField] Vector2 size;
    [SerializeField] Vector2 pivot;
    [SerializeField] Vector2 anchorMin;
    [SerializeField] Vector2 anchorMax;
    [SerializeField] Image forColorSettig;
    private void Awake()
    {
        
    }

    public void SetPanelSize()
    {

    }
}
