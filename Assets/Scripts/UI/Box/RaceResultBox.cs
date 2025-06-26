using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RaceResultBox : MonoBehaviour
{
    [SerializeField] private Transform resultEntryParent;
    [SerializeField] private GameObject resultEntryPrefab;

    [SerializeField] private Dictionary<NetworkId, GameObject> activeResultEntries = new Dictionary<NetworkId, GameObject>();

    /// <summary>
    /// 네트워크를 통해 동기화된 완주 플레이어 데이터를 받아 결과 UI를 업데이트합니다.
    /// 이 메서드는 MainGame_Manager의 FixedUpdateNetwork에서 호출됩니다.
    /// </summary>
    /// <param name="finishedTimes">완주한 플레이어의 ID와 시간 딕셔너리</param>
    /// <param name="playerNames">완주한 플레이어의 ID와 이름 딕셔너리</param>
    public void UpdateResultDisplay(NetworkDictionary<NetworkId, float> finishedTimes, NetworkDictionary<NetworkId, string> playerNames)
    {
        if(resultEntryParent == null || resultEntryPrefab == null)
        {
            Debug.LogError("ResultUIManager: resultEntryParent 또는 resultEntryPrefab이 할당되지 않았습니다.");
            return;
        }

        // 현재 표시된 항목 중, finishedTimes에 없는 항목은 제거합니다.
        // 이는 플레이어가 도중에 나가거나, 데이터가 변경되었을 때 UI를 동기화하기 위함입니다.
        List<NetworkId> idsToRemove = new List<NetworkId>();
        foreach (var entryId in activeResultEntries.Keys)
        {
            if (!finishedTimes.ContainsKey(entryId))
            {
                idsToRemove.Add(entryId);
            }
        }
        foreach (var id in idsToRemove)
        {
            Destroy(activeResultEntries[id]);
            activeResultEntries.Remove(id);
        }

        // 완주한 플레이어들을 시간 기준으로 정렬하여 랭킹 생성
        var rankedPlayers = finishedTimes.OrderBy(pair => pair.Value).ToList();

        int rankNum = 1;
        foreach (var entry in rankedPlayers)
        {
            NetworkId playerId = entry.Key;
            float finishTime = entry.Value;
            string playerName = playerNames.ContainsKey(playerId) ? playerNames[playerId] : "Unknown Player";

            GameObject resultEntryGO;

            // 이미 존재하는 항목이면 업데이트, 없으면 새로 생성
            if (activeResultEntries.ContainsKey(playerId))
            {
                resultEntryGO = activeResultEntries[playerId];
            }
            else
            {
                resultEntryGO = Instantiate(resultEntryPrefab, resultEntryParent);
                activeResultEntries.Add(playerId, resultEntryGO);
            }

            // ResultEntryPrefab에 TextMeshProUGUI 컴포넌트들이 있어야 합니다.
            TextMeshProUGUI rankText = resultEntryGO.transform.Find("RankText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI nameText = resultEntryGO.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI timeText = resultEntryGO.transform.Find("TimeText")?.GetComponent<TextMeshProUGUI>();

            if (rankText != null) rankText.text = $"{rankNum}위";
            if (nameText != null) nameText.text = playerName;
            // 시간 표시 형식: 분:초.밀리초 (예: 01:23.45)
            if (timeText != null) timeText.text = System.TimeSpan.FromSeconds(finishTime).ToString("mm':'ss'.'fff");

            rankNum++;
        }
    }
}
