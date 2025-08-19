[System.Serializable]
public class CarWheelsData
{
    public int Num { get; set; }
    public string Name { get; set; }
    public string fileName { get; set; }
    public string Type { get; set; } // enum eTIRETYPE 정의 필요

    public float brakePower { get; set; }
    public float brakeBias { get; set; }

    public float forwardFrictionMin { get; set; }
    public float sidewaysFrictionMin { get; set; }
    public float longitudinalLoadFactor { get; set; }
    public float lateralLoadFactor { get; set; }
    public float slipGripReductionFactor { get; set; }

    public float suspensionSpringFront { get; set; }
    public float suspensionSpringRear { get; set; }

    public float suspensionDamperFront { get; set; }
    public float suspensionDamperRear { get; set; }

    public float suspensionPositionFront { get; set; }
    public float suspensionPositionRear { get; set; }

    public float forwardTireGripFront { get; set; }
    public float forwardTireGripRear { get; set; }
    public float sidewaysTireGripFront { get; set; }
    public float sidewaysTireGripRear { get; set; }

    public float wheelMass { get; set; }
    public float wheelRadius { get; set; }

    public float suspensionDistanceFront { get; set; }
    public float suspensionDistanceRear { get; set; }

    public float forceAppPointDistanceFront { get; set; }
    public float forceAppPointDistanceRear { get; set; }

    public float forwardextremumSlipFront { get; set; }
    public float forwardextremumValueFront { get; set; }
    public float forwardsasymptoteSlipFront { get; set; }
    public float forwardsasymptoteValueFront { get; set; }

    public float sidewayextremumSlipFront { get; set; }
    public float sidewayextremumValueFront { get; set; }
    public float sidewaysasymptoteSlipFront { get; set; }
    public float sidewaysasymptoteValueFront { get; set; }

    public float forwardextremumSlipRear { get; set; }
    public float forwardextremumValueRear { get; set; }
    public float forwardsasymptoteSlipRear { get; set; }
    public float forwardsasymptoteValueRear { get; set; }

    public float sidewayextremumSlipRear { get; set; }
    public float sidewayextremumValueRear { get; set; }
    public float sidewaysasymptoteSlipRear { get; set; }
    public float sidewaysasymptoteValueRear { get; set; }
}