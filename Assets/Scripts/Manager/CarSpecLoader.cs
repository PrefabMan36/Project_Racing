using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;

// CarSpecRecord 클래스 정의는 동일 ...
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
    [Name("gearSpeedLimite.GEAR_FIRST")] // CSV 헤더 확인 필요
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


// *** 클래스 이름 변경: CarSpecLoader ***
public class CarSpecLoader : MonoBehaviour
{
    // StreamingAssets 폴더 사용 권장
    public string csvFileName = "Car_spec.csv";
    public List<CarSpecRecord> records;

    void Start()
    {
        ReadCsvFile();
    }

    void ReadCsvFile()
    {
        // StreamingAssets 경로 사용
        string filePath = Path.Combine(Application.streamingAssetsPath, csvFileName);

        try
        {
            using (var reader = new StreamReader(filePath))
            // 클래스 이름이 다르므로 CsvHelper. 명시 불필요 (해도 무방)
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                records = new List<CarSpecRecord>(csv.GetRecords<CarSpecRecord>());
                Debug.Log($"{csvFileName} 파일 읽기 완료 (경로: {filePath}). 총 {records.Count}개의 레코드.");
                if (records.Count > 0)
                {
                    var record = records[0];
                    Debug.Log($"첫 레코드 확인 - Name: {record.Name}, Mass: {record.Mass}, gearRatio.eGEAR_FIRST: {record.gearRatio_eGEAR_FIRST}");
                }
            }
        }
        catch (FileNotFoundException)
        {
            Debug.LogError($"CSV 파일을 찾을 수 없습니다: {filePath}. StreamingAssets 폴더에 파일이 있는지 확인하세요.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"CSV 파일 처리 중 오류 발생: {e.Message}");
            if (e is HeaderValidationException headerEx)
            {
                Debug.LogError("CSV 헤더와 CarSpecRecord 클래스의 속성 매핑을 확인하세요.");
            }
        }
    }
}