using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapButton : MonoBehaviour
{
    [SerializeField] private CreateRoom createRoomManager;
    [SerializeField] private TextMeshProUGUI mapName;
    [SerializeField] private int mapNum = 0;
    [SerializeField] Button thisButton;

    private void Awake()
    {
        thisButton = GetComponent<Button>();
        thisButton.onClick.AddListener(OnClickMapSelect);
    }

    public void SetMapButton(CreateRoom _manager ,int _mapNum, string _mapName)
    {
        if (mapNum != 0)
            return;
        createRoomManager = _manager;
        mapName.text = _mapName;
        mapNum = _mapNum;
    }

    public void OnClickMapSelect()
    {
        createRoomManager.SelectMap(mapNum);
    }
}
