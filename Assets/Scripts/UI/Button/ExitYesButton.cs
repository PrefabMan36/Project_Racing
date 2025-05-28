using UnityEngine;
using UnityEngine.UI;

public class ExitYesButton : MonoBehaviour
{
    private void Start()
    {
        UIBox box = GetComponent<UIBox>();
        box.SetUIType(eUI_TYPE.YES);
        gameObject.AddComponent<Button>();
        Button button = GetComponent<Button>();
        button.onClick.AddListener(OnClickQuit);
    }

    public void OnClickQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
