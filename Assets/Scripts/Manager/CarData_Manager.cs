using UnityEngine;
using System.Collections.Generic;
using System.IO; // �߰�
using System.Globalization; // �߰�
using CsvHelper; // �߰�
using CsvHelper.Configuration; // �߰�
// using CSVToolKit; // ����

public class CarData_Manager : MonoBehaviour
{
    public static CarData_Manager instance { get; private set; }
    public List<CarData> carDatas;// = new List<CarData>();
    private string csvRelativePathInStreamingAssets = "CSV"; // StreamingAssets ������ ��� ���
    private string csvFileName = "Car_spec"; // ���� �̸�

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadCarData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadCarData()
    {
       carDatas = CSVParser.ParseCSV<CarData>(csvFileName);
        if(carDatas.Count > 0)
        {
            Debug.Log($"[CSVLoaderExample] '{csvFileName}.csv' ���Ͽ��� �� {carDatas.Count}���� ������ ���ڵ带 �ε��߽��ϴ�.");
        }
        else
        {
            Debug.LogWarning($"[CSVLoaderExample] '{csvFileName}.csv' ���� �ε忡 �����߰ų� �����Ͱ� �����ϴ�.");
        }
    }

    public CarData GetCarDataByName(string carName)
    {
        foreach (var data in carDatas)
        {
            if (data.Name == carName)
            {
                return data;
            }
        }
        Debug.LogWarning($"CarData with name '{carName}' not found.");
        return null;
    }
}