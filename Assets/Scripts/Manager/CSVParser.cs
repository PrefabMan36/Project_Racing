using CsvHelper;
using CsvHelper.Configuration;
using System.Collections.Generic;
using System.Globalization;
using System.IO; // Path.Combine을 위해 필요
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public static class CSVParser
{
    /// <summary>
    /// StreamingAssets/CSV 폴더에 있는 CSV 파일을 비동기적으로 파싱하여 지정된 타입의 리스트로 반환합니다.
    /// UnityWebRequest를 사용하여 플랫폼 호환성을 확보합니다.
    /// </summary>
    /// <typeparam name="T">CSV 각 행을 매핑할 타입 (클래스, 레코드, struct 등)</typeparam>
    /// <param name="csvFileName">StreamingAssets/CSV 폴더 내의 CSV 파일 이름 (확장자 포함! 예: "MyData.csv")</param>
    /// <param name="config">CSV 파싱 설정 (선택 사항)</param>
    /// <returns>파싱된 데이터의 리스트를 담은 Task 또는 오류 발생 시 빈 리스트를 담은 Task</returns>
    public static async Task<List<T>> ParseCSV<T>(string csvFileName, CsvConfiguration config = null)
        where T : class
    {
        string subfolder = "CSV"; // CSV 파일이 있는 하위 폴더 이름
        // Application.streamingAssetsPath와 하위 폴더, 파일 이름을 결합하여 최종 경로 생성
        // Path.Combine은 플랫폼에 맞는 경로 구분자(\ 또는 /)를 사용합니다.
        string fullPath = Path.Combine(Application.streamingAssetsPath, subfolder, csvFileName);

        // UnityWebRequest는 Application.streamingAssetsPath로 생성된 경로를 잘 처리합니다.
        // Android의 jar:file:// 나 PC의 file:/// 접두사를 자동으로 처리하는 경우가 많습니다.
        string uri = fullPath;

        // *주의*: 일부 Android 환경이나 특정 Unity 버전에서 jar:file:// 접두사 문제가 발생할 수 있습니다.
        // 만약 문제가 발생하면 아래와 같은 플랫폼별 접두사 로직을 추가해야 할 수도 있습니다.
        // 하지만 대부분의 경우 Application.streamingAssetsPath + Path.Combine 결과만으로도 작동합니다.
        /*
        string uri;
        if (Application.platform == RuntimePlatform.Android)
        {
             // Android의 경우 apk 내 경로는 jar:file:// 접두사가 필요할 수 있습니다.
             // UnityWebRequest가 자동 처리하지 못한다면 여기에 추가 로직 필요
             uri = fullPath; // 일단 Path.Combine 결과 그대로 시도
             // 예: uri = "jar:file://" + fullPath; (하지만 이 방식은 Path.Combine 결과에 따라 복잡해질 수 있음)
        }
        else if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
             // WebGL은 HTTP 요청이므로 접두사 불필요
             uri = fullPath; // Application.streamingAssetsPath는 WebGL에서 URL을 반환함
        }
        else
        {
             // PC (Editor, Windows, Mac, Linux 등), iOS 등 파일 시스템 경로
             // file:/// 접두사 필요 (Path.Combine 결과가 절대 경로인 경우)
             uri = "file:///" + fullPath; // 3 슬래시
        }
        */


        using (var www = UnityWebRequest.Get(uri))
        {
            await www.SendWebRequest(); // 비동기 로드 시작 및 대기

            if (www.result != UnityWebRequest.Result.Success)
            {
                // 로드 실패 시 오류 로깅
                Debug.LogError($"[CSVParser] StreamingAssets 파일 로드 오류: {www.error}");
                Debug.LogError($"[CSVParser] 로드 시도 경로 (URI): {uri}");
                return new List<T>();
            }

            // 로드된 텍스트 파싱
            string csvText = www.downloadHandler.text;

            // CsvHelper 파싱 설정 (이전과 동일)
            var csvConfig = config ?? new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ",",
                IgnoreBlankLines = true,
                TrimOptions = TrimOptions.Trim,
                // Mode = CsvMode.Standard, // 이전 버전 오류 해결 위해 제거됨
            };

            using (var reader = new StringReader(csvText))
            {
                try
                {
                    using (var csv = new CsvReader(reader, csvConfig))
                    {
                        csv.Read();
                        csv.ReadHeader();
                        return csv.GetRecords<T>().ToList();
                    }
                }
                catch (CsvHelperException e)
                {
                    // CsvHelper 파싱 오류 로깅 (e.Message 사용)
                    Debug.LogError($"[CSVParser] CsvHelper 파싱 오류 발생: {e.Message}");
                    // 버전 33.0.1에서 e.Context.Reader.Context.Row/RawRecord 직접 접근 안됨
                    return new List<T>();
                }
                catch (System.Exception e)
                {
                    // 일반 오류 로깅
                    Debug.LogError($"[CSVParser] CSV 파싱 중 예상치 못한 오류 발생: {e.Message}");
                    return new List<T>();
                }
            }
        }
    }
}