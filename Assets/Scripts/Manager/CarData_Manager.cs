using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using CSVToolKit;
using UnityEngine;

public class CarData_Manager : MonoBehaviour
{
    public static CarData_Manager instance {  get; private set; }

    public List<Car_data> carDatas = new List<Car_data>();
    private string csvFilePath = "Assets/CSV/";
    private string csvFileName = "Car_spec.csv";

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadCarData();
        }
        else
            Destroy(gameObject);
    }
    private void LoadCarData()
    {
        List<List<string>> rawData = CSVParser.Instance.ReadData(csvFilePath, csvFileName);
        if(rawData == null || rawData.Count <= 1)
        {
            Debug.LogError("Car spec data could not be loaded or is empty.");
            return;
        }

        for(int i = 1; i < rawData.Count; i++)
        {
            List<string> row = rawData[i];
            // ������ ���� �ʵ� ���� ������� Ȯ�� (���� ����)
            // ����Ǵ� �ּ� �ʵ� �� (��: finalDriveRatio �ε��� + 1)
            if (row.Count <= 16 )
            {
                Debug.LogWarning($"Skipping row {i + 1} due to insufficient data fields: {row.Count} fields found.");
                continue; // �ʵ� ���� �����ϸ� �ش� �� �ǳʶٱ�
            }
            // �� �� �ǳʶٱ� (��: ������ �ٹٲ����� ���� �� ���� ����� ���)
            if(row.Count == 1 && string.IsNullOrEmpty(row[0]) )
                continue;

            Car_data data = new Car_data();

            try
            {
                data.Name = row[1];
                data.fileName = row[2];
                // Mass: "1,120" ���� ���� ó��
                string massString = row[3].Replace("/", "").Replace(",", "");// ����ǥ�� ��ǥ ����
                if (float.TryParse(massString, NumberStyles.Any, CultureInfo.InvariantCulture, out float mass))
                    data.mass = mass;
                else
                    Debug.LogWarning($"Failed to parse Mass for row {i + 1}: '{row[3]}'");
                // �⺻�� �Ǵ� ���� ó�� ���� �߰� ����

                if (float.TryParse(row[4], NumberStyles.Any, CultureInfo.InvariantCulture, out float dragCoeff))
                    data.dragCoefficient = dragCoeff;
                else
                    Debug.LogWarning($"Failed to parse dragCoefficient for row {i + 1}: '{row[4]}'");
                if (float.TryParse(row[5], NumberStyles.Any, CultureInfo.InvariantCulture, out float baseAccel))
                    data.baseEngineAcceleration = baseAccel;
                else
                    Debug.LogWarning($"Failed to parse baseEngineAcceleration for row {i + 1}: '{row[5]}'");
                if (float.TryParse(row[6], NumberStyles.Any, CultureInfo.InvariantCulture, out float maxRPM))
                    data.maxEngineRPM = maxRPM;
                else
                    Debug.LogWarning($"Failed to parse maxEngineRPM for row {i + 1}: '{row[6]}'");
                if (float.TryParse(row[7], NumberStyles.Any, CultureInfo.InvariantCulture, out float minRPM))
                    data.minEngineRPM = minRPM;
                else
                    Debug.LogWarning($"Failed to parse minEngineRPM for row {i + 1}: '{row[7]}'");
                if (int.TryParse(row[8], out int lastGear))
                    data.lastGear = lastGear;
                else
                    Debug.LogWarning($"Failed to parse lastGear for row {i + 1}: '{row[8]}'");

                // GearRatio (���� ��� ����)
                data.gearRatio = new List<float>();
                int gearRatioStartIndex = 9;
                int gearRatioCount = data.lastGear + 1; // ���� ��� ���� ����
                if (row.Count >= gearRatioStartIndex + gearRatioCount)
                {
                    for(int j = 0; j < gearRatioCount; j++)
                    {
                        if (float.TryParse(row[gearRatioStartIndex + j], NumberStyles.Any, CultureInfo.InvariantCulture, out float ratio))
                            data.gearRatio.Add(ratio);
                        else
                        {
                            Debug.LogWarning($"Failed to parse gearRatio {j} for row {i + 1}: '{row[gearRatioStartIndex + j]}'");
                            data.gearRatio.Add(0f); // �Ľ� ���� �� �⺻�� �߰�
                        }
                    }
                }
                else
                    Debug.LogWarning($"Insufficient data for gearRatio for row {i + 1}");

                //finalDriveRatio
                int finalDriveRatioIndex = gearRatioStartIndex + gearRatioCount;
                if (row.Count > finalDriveRatioIndex && float.TryParse(row[finalDriveRatioIndex], NumberStyles.Any, CultureInfo.InvariantCulture, out float finalDrive))
                    data.finalDriveRatio = finalDrive;
                else
                    Debug.LogWarning($"Failed to parse or find finalDriveRatio for row {i + 1}");

                //GearSpeedLimit (���� ��� ����)
                data.gearSpeedLimit = new List<float>();
                int speedLimitSatrtIndex = finalDriveRatioIndex + 1;
                int speedLimitCount = data.lastGear + 1;
                if(row.Count >= speedLimitSatrtIndex + speedLimitCount)
                {
                    for (int j = 0; j < speedLimitCount; j++)
                    {
                        if (float.TryParse(row[j], NumberStyles.Any, CultureInfo.InvariantCulture, out float limit))
                            data.gearSpeedLimit.Add(limit);
                        else
                        {
                            Debug.LogWarning($"Failed to parse gearSpeedLimit {j} for row {i + 1}: '{row[speedLimitSatrtIndex + j]}'");
                            data.gearSpeedLimit.Add(0f); // �Ľ� ���� �� �⺻�� �߰�
                        }

                    }
                }
                else
                    Debug.LogWarning($"Insufficient data for gearSpeedLimit for row {i + 1}");

                carDatas.Add(data);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error parsing row {i + 1}: {e.Message}\nRow Data: {string.Join(",", row)}");
            }
        }
        Debug.Log($"Successfully loaded {carDatas.Count} car data entries.");
    }
    public Car_data? GetCarDataByName(string carName)
    {
        foreach (var data in carDatas)
        {
            if(data.Name == carName)
                { return data; }
        }
        return null; // �ش� �̸��� �����Ͱ� ���� ��� null ��ȯ
    }
}
