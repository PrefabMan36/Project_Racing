using Fusion;
using UnityEngine;

public struct NetworkInputManager : INetworkInput
{
    public Vector3 direction;
    public float clutch;
    public bool sideBraking;
    public bool boosting;
    public bool gearUP;
    public bool gearDOWN;

    public NetworkButtons headLight;
    public byte forceGear;
}
