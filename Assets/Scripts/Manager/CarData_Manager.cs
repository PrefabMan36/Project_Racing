using UnityEngine;
using System.Collections.Generic;
using System.IO; // 추가
using System.Globalization; // 추가
using CsvHelper; // 추가
using CsvHelper.Configuration; // 추가
// using CSVToolKit; // 제거

public class CarData_Manager : MonoBehaviour
{
    public static CarData_Manager instance { get; private set; }
    public List<CarData> carDatas;// = new List<CarData>();
    private string csvRelativePathInStreamingAssets = "CSV"; // StreamingAssets 내부의 상대 경로
    private string csvFileName = "Car_spec"; // 파일 이름

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
}