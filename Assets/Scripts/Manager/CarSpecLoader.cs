using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;

// CarSpecRecord Ŭ���� ���Ǵ� ���� ...
public class CarSpecRecord
{
    public int Num { get; set; }
    public string Name { get; set; }
    public string fileName { get; set; }
    public int Mass { get; set; }
    public float dragCoefficient { get; set; }
    public float baseEngineAcceleration { get; set; }
    public int maxEngineRPM { get; set; }
    public int minEngineRPM { get; set; }
    public int lastGear { get; set; }
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
    [Name("gearSpeedLimite.GEAR_FIRST")] // CSV ��� Ȯ�� �ʿ�
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
}


// *** Ŭ���� �̸� ����: CarSpecLoader ***
public class CarSpecLoader : MonoBehaviour
{
    // StreamingAssets ���� ��� ����
    public string csvFileName = "Car_spec.csv";
    public List<CarSpecRecord> records;

    void Start()
    {
        ReadCsvFile();
    }

    void ReadCsvFile()
    {
        // StreamingAssets ��� ���
        string filePath = Path.Combine(Application.streamingAssetsPath, csvFileName);

        try
        {
            using (var reader = new StreamReader(filePath))
            // Ŭ���� �̸��� �ٸ��Ƿ� CsvHelper. ��� ���ʿ� (�ص� ����)
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                records = new List<CarSpecRecord>(csv.GetRecords<CarSpecRecord>());
                Debug.Log($"{csvFileName} ���� �б� �Ϸ� (���: {filePath}). �� {records.Count}���� ���ڵ�.");
                if (records.Count > 0)
                {
                    var record = records[0];
                    Debug.Log($"ù ���ڵ� Ȯ�� - Name: {record.Name}, Mass: {record.Mass}, gearRatio.eGEAR_FIRST: {record.gearRatio_eGEAR_FIRST}");
                }
            }
        }
        catch (FileNotFoundException)
        {
            Debug.LogError($"CSV ������ ã�� �� �����ϴ�: {filePath}. StreamingAssets ������ ������ �ִ��� Ȯ���ϼ���.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"CSV ���� ó�� �� ���� �߻�: {e.Message}");
            if (e is HeaderValidationException headerEx)
            {
                Debug.LogError("CSV ����� CarSpecRecord Ŭ������ �Ӽ� ������ Ȯ���ϼ���.");
            }
        }
    }
}