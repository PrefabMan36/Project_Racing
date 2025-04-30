using Fusion;
using UnityEngine;

public struct NetworkInputManager : INetworkInput
{
    public Vector3 direction;
    public bool sideBraking;
    public bool boosting;
    public bool gearUP;
    public bool gearDOWN;

    public bool headLight;
    public byte forceGear;
}
