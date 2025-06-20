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
    private string csvFileName = "Car_spec.csv"; // 파일 이름

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
        Debug.Log($"[CSVLoaderExample] StreamingAssets에서 '{csvFileName}' 로딩 시작...");
        carDatas = await CSVParser.ParseCSV<CarData>(csvFileName);
        if(carDatas.Count > 0)
        {
            Debug.Log($"[CSVLoaderExample] '{csvFileName}.csv' 파일에서 총 {carDatas.Count}개의 데이터 레코드를 로드했습니다.");
        }
        else
        {
            Debug.LogWarning($"[CSVLoaderExample] '{csvFileName}.csv' 파일 로드에 실패했거나 데이터가 없습니다.");
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