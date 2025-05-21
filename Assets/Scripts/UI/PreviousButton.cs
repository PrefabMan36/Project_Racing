
using UnityEngine;

public class PreviousButton : MonoBehaviour
{
    void Start()
    {
        UIBox uIBox = GetComponent<UIBox>();
        uIBox.SetUIType(eUI_TYPE.PREVIOUS);
    }
}
