using System.Collections;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Rank : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerID;
    [SerializeField] private Image playerIcon;
    [SerializeField] private Image backGround;

    [SerializeField] private bool PositionChanging = false;
    [SerializeField] private Vector3 startPosition;
    [SerializeField] private Vector3[] targets;
    [SerializeField] private Vector3 targetPosition;
    [SerializeField] public int targetNum = 0;
    [SerializeField] private float time = 0f;

    private Color[] placeColors = new Color[4]
    {
        new Color(1f, 0.8f, 0.2f, 1f), // Gold
        new Color(0.8f, 0.8f, 0.8f, 1f), // Silver
        new Color(0.6f, 0.4f, 0.2f, 1f), // Bronze
        new Color(1f, 1f, 1f, 1f) // Default color for other ranks
    };

    private MainGame_Manager gameManager;
    private NetworkId myPlayerId;
    [SerializeField] private int currentRank = -1;

    public void Init(MainGame_Manager manager, NetworkId playerId)
    {
        this.gameManager = manager;
        this.myPlayerId = playerId;

        targets = gameManager.GetRankPositions();

        Debug.Log($"{this.myPlayerId.ToString()}의 랭크UI 가 스폰되었습니다.");

        StartCoroutine(CheckRankContinuously());
    }

    private void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager = null;
        }
        StopAllCoroutines();
        Debug.Log($"{myPlayerId.ToString()}의 랭크UI 가 제거되었습니다.");
    }

    public void SetPlay(Image _playerIcon, string _playerID)
    {
        backGround = gameObject.GetComponent<Image>();
        playerID.text = _playerID;
        Debug.Log(_playerID + "의 랭크UI 가 스폰되었습니다.");
        if (_playerIcon != null)
            playerIcon.sprite = _playerIcon.sprite;
    }
    private void PositioningRank()
    {
        startPosition = transform.position;
        targetNum = currentRank;

        if (backGround != null)
            backGround.color = targetNum > 3 ? placeColors[3] : placeColors[targetNum];
        if (targets[targetNum] != null)
            targetPosition = targets[targetNum];
        if (!PositionChanging)
        {
            time = 0f;
            PositionChanging = true;
            StartCoroutine(ChangePosition());
        }
    }

    IEnumerator CheckRankContinuously()
    {
        WaitForSeconds wfs = new WaitForSeconds(Shared.frame15);
        while (true)
        {
            if (gameManager != null && gameManager.GetRank(myPlayerId)-1 != currentRank)
            {
                currentRank = gameManager.GetRank(myPlayerId)-1;
                PositioningRank();
            }
            yield return wfs;
        }
    }

    IEnumerator ChangePosition()
    {
        WaitForSeconds wfs = new WaitForSeconds(Shared.frame30);
        while(true)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, time);
            time += 0.04f;
            if (time >= 1f)
            {
                PositionChanging = false;
                yield break;
            }
            yield return wfs;
        }
    }
}
