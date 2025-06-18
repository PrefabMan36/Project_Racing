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

    public void SetPlay(Image _playerIcon, string _playerID, MainGame_Manager _gameManager, NetworkId _id)
    {
        if (_playerID.ToString().Length < 1)
        {
            _gameManager.RemoveRank(_id);
            return;
        }
        backGround = gameObject.GetComponent<Image>();
        playerID.text = _playerID.ToString();
        if(_playerIcon != null)
            playerIcon.sprite = _playerIcon.sprite;
    }

    public void SetTargets(Vector3[] _targets)
    {
        targets = new Vector3[_targets.Length];
        for (int i = 0; i < _targets.Length; i++)
        {
            targets[i] = _targets[i];
        }
    }
    public void Rpc_SetPosition(int _targetNum, Color _placeColor)
    {
        startPosition = transform.position;
        targetNum = _targetNum;
        if(backGround != null)
            backGround.color = _placeColor;
        if (targets[_targetNum] != null)
            targetPosition = targets[_targetNum];
        if (!PositionChanging)
        {
            time = 0f;
            PositionChanging = true;
            StartCoroutine(ChangePosition());
        }
    }

    IEnumerator ChangePosition()
    {
        WaitForSeconds wfs = new WaitForSeconds(0.04f);
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
