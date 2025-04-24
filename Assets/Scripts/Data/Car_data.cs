using System.Collections.Generic;
using CsvHelper.Configuration.Attributes; // [Name] �Ӽ� ��� ���� �߰�

// struct ��� class�� �����ϰ� CsvHelper ���� �Ӽ� �߰�
public class CarData // Ŭ���� �̸� ���� (������������ ����)
{
    // CSV ����� �̸��� ������ ��� [Name] ���ʿ�
    public int Num { get; set; } // Num �� �߰� (CSV�� �����Ƿ�)
    public string Name { get; set; }
    public string fileName { get; set; }
    public float Mass { get; set; } // CSV�� ����(Mass)���� float���� �ޱ� ����

    [Name("dragCoefficient")] // ��Ȯ���� ���� �߰� (������)
    public float dragCoefficient { get; set; }

    [Name("baseEngineAcceleration")]
    public float baseEngineAcceleration { get; set; }

    public float maxEngineRPM { get; set; }
    public float minEngineRPM { get; set; }
    public short lastGear { get; set; }

    // gearRatio�� gearSpeedLimit�� CSV�� ����Ʈ ���°� �ƴϹǷ�
    // ���� �����ϱ� ��ƽ��ϴ�. ���� CarSpecRecordó�� ���� �Ӽ����� �ްų�,
    // �ε� �� ������ ����Ʈ�� �����ؾ� �մϴ�.
    // ���⼭�� CarSpecRecord�� �����ϰ� ���� �Ӽ����� �ްڽ��ϴ�.
    // ���� Car_data ����ü�� List<float> ���¸� �����ϰ� �ʹٸ�,
    // LoadCarData���� CarSpecRecord�� ���� �� Car_data ����Ʈ�� ��ȯ�ؾ� �մϴ�.

    [Name("gearRatio.eGEAR_REVERSE")]
    public float gearRatio_eGEAR_REVERSE { get; set; }

    [Name("gearRatio.eGEAR_FIRST")]
    public float gearRatio_eGEAR_FIRST { get; set; }

    [Name("gearRatio.eGEAR_SECOND")]
    public float gearRatio_eGEAR_SECOND { get; set; }

    [Name("gearRatio.eGEAR_THIRD")]
    public float gearRatio_eGEAR_THIRD { get; set; }

    [Name("gearRatio.eGEAR_FOURTH")]
    public float gearRatio_eGEAR_FOURTH { get; set; }

    [Name("gearRatio.eGEAR_FIFTH")]
    public float gearRatio_eGEAR_FIFTH { get; set; }

    [Name("gearRatio.eGEAR_SIXTH")]
    public float gearRatio_eGEAR_SIXTH { get; set; }

    public float finalDriveRatio { get; set; }

    [Name("gearSpeedLimit.eGEAR_REVERSE")]
    public float gearSpeedLimit_eGEAR_REVERSE { get; set; }

    // 'gearSpeedLimite.GEAR_FIRST' ��Ÿ ����! CSV ���� ���� ��� Ȯ�� �ʿ�
    [Name("gearSpeedLimite.GEAR_FIRST")]
    public float gearSpeedLimite_GEAR_FIRST { get; set; }

    [Name("gearSpeedLimit.eGEAR_SECOND")]
    public float gearSpeedLimit_eGEAR_SECOND { get; set; }

    [Name("gearSpeedLimit.eGEAR_THIRD")]
    public float gearSpeedLimit_eGEAR_THIRD { get; set; }

    [Name("gearSpeedLimit.eGEAR_FOURTH")]
    public float gearSpeedLimit_eGEAR_FOURTH { get; set; }

    [Name("gearSpeedLimit.eGEAR_FIFTH")]
    public float gearSpeedLimit_eGEAR_FIFTH { get; set; }

    [Name("gearSpeedLimit.eGEAR_SIXTH")]
    public float gearSpeedLimit_eGEAR_SIXTH { get; set; }

    // ���� ���� Car_data ����ü�� List<float> ���¸� �� ����ؾ� �Ѵٸ�,
    // �� Ŭ������ ������� �ʰ�, ���� �亯�� CarSpecRecord�� ���� ��
    // CarData_Manager���� Car_data ����Ʈ�� ��ȯ�ϴ� ������ �߰��ؾ� �մϴ�.
    // ��:
    // public List<float> gearRatio { get; set; }
    // public List<float> gearSpeedLimit { get; set; }
    // �� �ʵ���� CsvHelper�� �ڵ����� ä�� �� �����Ƿ� [Ignore] �Ӽ� �ʿ�
}