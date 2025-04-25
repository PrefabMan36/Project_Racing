using Fusion;
using UnityEngine;

public struct NetworkInput : INetworkInput
{
    public bool isAccelerating;
    public bool isBraking;
    public bool isSideBraking;
    public float isSteering;
    public bool isBoosting;
}
