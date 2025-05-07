using Fusion;
using UnityEngine;

public struct Rank_Data
{
    [Header("Ranking_Data")]
    public NetworkId playerId;
    public short lap;
    public int currentCheckpointIndex;
    public float distanceToCheckPoint;
}
