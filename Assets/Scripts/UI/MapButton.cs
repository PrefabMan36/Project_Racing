using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI mapName;
    [SerializeField] Button thisButton;

    private void Awake()
    {
        thisButton = GetComponent<Button>();
    }
}
