using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyItem : MonoBehaviour
{
    public TextMeshProUGUI playerNameText;
    public Image readyImage;
    public Image hostImage;

    private LobbyPlayer _player;

    public void SetPlayer(LobbyPlayer player)
    {
        _player = player;
    }

    private void Update()
    {
        if(_player.Object != null && _player.Object.IsValid)
        {
            playerNameText.text = _player.playerName.Value;
            readyImage.gameObject.SetActive(_player.isReady);
        }
    }
}
