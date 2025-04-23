using System.Collections;
using System.Collections.Generic;

public struct Car_data
{
    public string Name;
    public string fileName;
    public float mass;
    public float dragCoefficient;
    public float baseEngineAcceleration;
    public float maxEngineRPM, minEngineRPM;
    public int lastGear;
    public List<float> gearRatio;
    public float finalDriveRatio;
    public List<float> gearSpeedLimit;
    //public bool TCS, ABS, Nitro;
}
