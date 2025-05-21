using UnityEngine;

public class NoButton : MonoBehaviour
{
    void Start()
    {
        UIBox box = GetComponent<UIBox>();
        box.SetUIType(eUI_TYPE.NO);
    }
}
