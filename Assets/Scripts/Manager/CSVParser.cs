using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public static class CSVParser
{
    public static async Task<List<T>> ParseCSV<T>(
        string csvFileName,
        CsvConfiguration config = null,
        Action<CsvContext> registerClassMapAction = null) // ClassMap 등록을 위한 Action 추가
        where T : class
    {
        string subfolder = "CSV";
        string fullPath = Path.Combine(Application.streamingAssetsPath, subfolder, csvFileName);
        string uri = fullPath;

        // 플랫폼별 URI 처리 로직 (기존 코드 유지)
        // ... UnityWebRequest 이전 부분 ...
        // 예시: Android에서 jar:file:// 접두사가 필요한 경우를 위한 주석 처리된 로직
        // if (Application.platform == RuntimePlatform.Android) { /* ... */ }
        // else if (Application.platform == RuntimePlatform.WebGLPlayer) { /* ... */ }
        // else { uri = "file:///" + fullPath; } // PC 등

#if UNITY_ANDROID && !UNITY_EDITOR
        // Android 실 디바이스에서는 StreamingAssets 경로에 특별한 처리가 필요 없을 수 있음
        // UnityWebRequest가 내부적으로 jar:file:// 처리를 할 수 있음.
        // 문제가 발생하면 uri = "jar:file://" + fullPath; 와 같은 명시적 처리가 필요할 수 있음.
#elif UNITY_IOS && !UNITY_EDITOR
        // iOS도 마찬가지
#elif UNITY_STANDALONE || UNITY_EDITOR
        uri = "file:///" + fullPath; // Editor 및 PC 빌드에서는 file:/// 접두사 사용
#endif
        // WebGL의 경우 Application.streamingAssetsPath 자체가 URL이므로 추가 접두사 불필요

        Debug.Log($"[CSVParser] Attempting to load CSV from URI: {uri}");

        using (var www = UnityWebRequest.Get(uri))
        {
            await www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[CSVParser] StreamingAssets file load error: {www.error}");
                Debug.LogError($"[CSVParser] Attempted URI: {uri}");
                return new List<T>();
            }

            string csvText = www.downloadHandler.text;
            var csvConfig = config ?? new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true, // 기본값은 true, TrackDataLoader에서 false로 오버라이드
                Delimiter = ",",
                IgnoreBlankLines = true,
                TrimOptions = TrimOptions.Trim,
            };

            using (var reader = new StringReader(csvText))
            {
                try
                {
                    using (var csv = new CsvReader(reader, csvConfig))
                    {
                        // Action을 통해 ClassMap 등록
                        registerClassMapAction?.Invoke(csv.Context);

                        // HasHeaderRecord 설정에 따라 헤더를 읽거나 읽지 않음
                        if (csvConfig.HasHeaderRecord)
                        {
                            csv.Read(); // 첫 줄을 읽고
                            csv.ReadHeader(); // 헤더로 처리
                        }
                        // HasHeaderRecord가 false이면, 첫 줄부터 데이터로 간주 (csv.Read()는 GetRecords<T> 내부에서 호출됨)

                        return csv.GetRecords<T>().ToList();
                    }
                }
                catch (CsvHelperException e)
                {
                    Debug.LogError($"[CSVParser] CsvHelper parsing error: {e.Message} for file {csvFileName}. Raw CsvHelper Context: {e.Context?.ToString() ?? "N/A"}");
                    return new List<T>();
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[CSVParser] Unexpected error during CSV parsing: {e.Message} for file {csvFileName}");
                    return new List<T>();
                }
            }
        }
    }
    /// <summary>
    /// StreamingAssets/CSV 폴더에 있는 CSV 파일을 비동기적으로 파싱하여 지정된 타입의 리스트로 반환합니다.
    /// UnityWebRequest를 사용하여 플랫폼 호환성을 확보합니다.
    /// </summary>
    /// <typeparam name="T">CSV 각 행을 매핑할 타입 (클래스, 레코드, struct 등)</typeparam>
    /// <param name="csvFileName">StreamingAssets/CSV 폴더 내의 CSV 파일 이름 (확장자 포함! 예: "MyData.csv")</param>
    /// <param name="config">CSV 파싱 설정 (선택 사항)</param>
    /// <returns>파싱된 데이터의 리스트를 담은 Task 또는 오류 발생 시 빈 리스트를 담은 Task</returns>
}