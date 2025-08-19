using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using UnityEngine.Networking;
using System.Globalization;
using System.Linq;

public class CarData_Manager : MonoBehaviour
{
    public static CarData_Manager instance { get; private set; }

    public List<CarData> carDatas;
    public List<CarWheelsData> carWheelsDatas;
    public List<EngineCurveData> engineCurveDatas;

    private string carSpecCsvFileName = "Car_spec.csv"; // 파일 이름
    private string carWheelsCsvFileName = "Car_Wheels.csv"; // 파일 이름

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
        Debug.Log($"[CSVLoaderExample] StreamingAssets에서 '{carSpecCsvFileName}' 로딩 시작...");
        carDatas = await CSVParser.ParseCSV<CarData>(carSpecCsvFileName);
        if(carDatas.Count > 0)
        {
            Debug.Log($"[CSVLoaderExample] '{carSpecCsvFileName}' 파일에서 총 {carDatas.Count}개의 데이터 레코드를 로드했습니다.");
        }
        else
        {
            Debug.LogWarning($"[CSVLoaderExample] '{carSpecCsvFileName}.csv' 파일 로드에 실패했거나 데이터가 없습니다.");
        }

        Debug.Log($"[CSVLoader] StreamingAssets에서 '{carWheelsCsvFileName}' 로딩 시작...");
        carWheelsDatas = await CSVParser.ParseCSV<CarWheelsData>(carWheelsCsvFileName);
        if (carWheelsDatas != null && carWheelsDatas.Count > 0)
        {
            Debug.Log($"[CSVLoader] '{carWheelsCsvFileName}' 파일에서 총 {carWheelsDatas.Count}개의 데이터 레코드를 로드했습니다.");
        }
        else
        {
            Debug.LogWarning($"[CSVLoader] '{carWheelsCsvFileName}' 파일 로드에 실패했거나 데이터가 없습니다.");
        }
        await LoadAllEngineCurves();
    }

    private async Task LoadAllEngineCurves()
    {
        engineCurveDatas = new List<EngineCurveData>();
        Debug.Log("[EngineCurveLoader] 모든 차량의 엔진 커브 데이터 로드를 시작합니다.");

        if (carDatas == null || carDatas.Count == 0)
        {
            Debug.LogError("[EngineCurveLoader] 엔진 커브를 로드하기 전에 Car_spec 데이터가 먼저 로드되어야 합니다.");
            return;
        }

        // carDatas에 있는 모든 차량에 대해 반복
        foreach (var carData in carDatas)
        {
            string carName = carData.Name;
            string subfolder = "CSV/EngineCurves"; // CSV 파일이 있는 하위 폴더
            string fileName = $"{carName}.csv";
            string fullPath = Path.Combine(Application.streamingAssetsPath, subfolder, fileName);
            string uri = fullPath;

#if UNITY_STANDALONE || UNITY_EDITOR
            uri = "file:///" + fullPath;
#endif

            using (var www = UnityWebRequest.Get(uri))
            {
                var operation = www.SendWebRequest();
                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                if (www.result == UnityWebRequest.Result.Success)
                {
                    string csvText = www.downloadHandler.text;
                    var curveData = ParseCsvAndCreateCurves(carName, csvText);
                    if (curveData != null)
                    {
                        engineCurveDatas.Add(curveData);
                        Debug.Log($"[EngineCurveLoader] '{carName}'의 커브 데이터 로드 성공.");
                    }
                }
                else
                {
                    Debug.LogWarning($"[EngineCurveLoader] '{fileName}' 파일을 찾을 수 없습니다. (URI: {uri})");
                }
            }
        }
        Debug.Log($"[EngineCurveLoader] 총 {engineCurveDatas.Count}개의 차량 엔진 커브 데이터 로드 완료.");
    }

    private EngineCurveData ParseCsvAndCreateCurves(string carName, string csvText)
    {
        var lines = csvText.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 3) return null;

        try
        {
            var rpmValues = lines[0].Split(',').Skip(1).Select(s => float.Parse(s, CultureInfo.InvariantCulture)).ToList();
            var hpValues = lines[1].Split(',').Skip(1).Select(s => float.Parse(s, CultureInfo.InvariantCulture)).ToList();
            var torqueValues = lines[2].Split(',').Skip(1).Select(s => float.Parse(s, CultureInfo.InvariantCulture)).ToList();

            if (rpmValues.Count != hpValues.Count || rpmValues.Count != torqueValues.Count) return null;

            var hpKeyframes = new List<Keyframe>();
            var torqueKeyframes = new List<Keyframe>();
            for (int i = 0; i < rpmValues.Count; i++)
            {
                hpKeyframes.Add(new Keyframe(rpmValues[i], hpValues[i]));
                torqueKeyframes.Add(new Keyframe(rpmValues[i], torqueValues[i]));
            }

            var hpCurve = new AnimationCurve(hpKeyframes.ToArray());
            var torqueCurve = new AnimationCurve(torqueKeyframes.ToArray());

            for (int i = 0; i < hpCurve.keys.Length; i++)
            {
                hpCurve.SmoothTangents(i, 0f);
                torqueCurve.SmoothTangents(i, 0f);
            }

            return new EngineCurveData { Name = carName, HorsepowerCurve = hpCurve, TorqueCurve = torqueCurve };
        }
        catch { return null; }
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

    public CarWheelsData GetCarWheelsDataByName(string carName)
    {
        foreach (var data in carWheelsDatas)
        {
            if (data.Name == carName)
            {
                return data;
            }
        }
        Debug.LogWarning($"CarWheelsData with name '{carName}' not found.");
        return null;
    }
    public CarWheelsData GetCarWheelsDataByNumber(int carNumber)
    {
        foreach (var data in carWheelsDatas)
        {
            if (data.Num == carNumber)
            {
                return data;
            }
        }
        Debug.LogWarning($"CarWheelsData with number '{carNumber}' not found.");
        return null;
    }
    public EngineCurveData GetEngineCurveDataByName(string carName)
    {
        var foundData = engineCurveDatas.Find(data => data.Name == carName);
        if (foundData == null)
        {
            Debug.LogWarning($"EngineCurveData with name '{carName}' not found.");
        }
        return foundData;
    }
    public EngineCurveData GetEngineCurveDataByNumber(int carNumber)
    {
        string carName = string.Empty;
        foreach (var data in carWheelsDatas)
        {
            if (data.Num == carNumber)
            {
                carName = data.Name;
            }
        }
        var foundData = engineCurveDatas.Find(data => data.Name == carName);
        if (foundData == null)
        {
            Debug.LogWarning($"EngineCurveData with number '{carNumber}' not found.");
        }
        return foundData;
    }
}