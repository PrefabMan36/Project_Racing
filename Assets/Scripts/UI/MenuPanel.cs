using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class MenuPanel : MonoBehaviour
{
    [SerializeField] protected GameObject topBar_prefab;
    [SerializeField] protected GameObject topBar;
    [SerializeField] protected ButtonBox button_Prefab;
}
