using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyMapButton : MonoBehaviour
{
    [SerializeField] private SelectMapLobby mapSelectManager;
    [SerializeField] private TextMeshProUGUI mapName;
    [SerializeField] private int mapNum = 0;
    [SerializeField] Button thisButton;

    private void Awake()
    {
        thisButton = GetComponent<Button>();
        thisButton.onClick.AddListener(OnClickMapSelect);
    }

    public void SetMapButton(SelectMapLobby _manager ,int _mapNum, string _mapName)
    {
        if (mapNum != 0)
            return;
        mapSelectManager = _manager;
        mapName.text = _mapName;
        mapNum = _mapNum;
    }

    public void OnClickMapSelect()
    {
        mapSelectManager.SelectMap(mapNum);
    }
}
