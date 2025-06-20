using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using System.Threading.Tasks;

public class CarData_Manager : MonoBehaviour
{
    public static CarData_Manager instance { get; private set; }
    public List<CarData> carDatas;// = new List<CarData>();
    public List<Curve_data> curve_Datas;
    private string csvFileName = "Car_spec.csv"; // ���� �̸�

    private async void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            await LoadCarData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private async Task LoadCarData()
    {
        Debug.Log($"[CSVLoaderExample] StreamingAssets���� '{csvFileName}' �ε� ����...");
        carDatas = await CSVParser.ParseCSV<CarData>(csvFileName);
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

    public CarData GetCarDataByNumber(int carNumber)
    {
        foreach (var data in carDatas)
        {
            if (data.Num == carNumber)
            {
                return data;
            }
        }
        Debug.LogWarning($"CarData with name '{carNumber}' not found.");
        return null;
    }
}