using CsvHelper;
using CsvHelper.Configuration;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine; // Debug.LogError를 위해 필요

public static class CSVParser
{
    /// <summary>
    /// Resources 폴더에 있는 CSV 파일을 파싱하여 지정된 타입의 리스트로 반환합니다.
    /// CSVHelper의 자동 매핑 기능을 사용합니다.
    /// </summary>
    /// <typeparam name="T">CSV 각 행을 매핑할 타입 (클래스, 레코드, struct 등)</typeparam>
    /// <param name="resourceFileName">Resources 폴더 내의 CSV 파일 이름 (확장자 제외)</param>
    /// <param name="config">CSV 파싱 설정 (선택 사항)</param>
    /// <returns>파싱된 데이터의 리스트 또는 오류 발생 시 빈 리스트</returns>
    public static List<T> ParseCSV<T>(string resourceFileName, CsvConfiguration config = null)
    {
        TextAsset csvTextAsset = Resources.Load<TextAsset>(resourceFileName);

        if (csvTextAsset == null)
        {
            Debug.LogError($"[CSVParser] CSV 파일 '{resourceFileName}.csv'를 Resources 폴더에서 찾을 수 없습니다.");
            return new List<T>(); // 파일을 찾지 못한 경우 빈 리스트 반환
        }

        using (var reader = new StringReader(csvTextAsset.text))
        {
            // 기본 설정 (RFC4180 표준, 쉼표 구분, 헤더 있음 가정)
            var csvConfig = config ?? new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,       // 첫 줄을 헤더로 간주
                Delimiter = ",",              // 쉼표 구분자
                IgnoreBlankLines = true,      // 빈 줄 무시
                TrimOptions = TrimOptions.Trim, // 공백 제거
                // 다양한 OS의 줄바꿈 문자 (\r\n, \n, \r)를 자동으로 처리합니다. StringReader가 기본적으로 잘 처리합니다.
                //Mode = CsvMode.Standard,      // 표준 CSV 모드
            };

            try
            {
                using (var csv = new CsvReader(reader, csvConfig))
                {
                    // CsvHelper는 기본적으로 클래스 속성 이름과 헤더 이름을 매핑합니다.
                    // 만약 매핑 규칙이 복잡하다면 CsvClassMap<T>를 구현하여 사용해야 합니다.
                    // 예: csv.Context.RegisterClassMap<YourCustomMap>();

                    // 헤더를 읽어 컬럼 정보를 준비
                    csv.Read();
                    csv.ReadHeader();

                    // 모든 레코드를 지정된 타입 T의 리스트로 읽어옴
                    return csv.GetRecords<T>().ToList();
                }
            }
            catch (CsvHelperException e)
            {
                Debug.LogError($"[CSVParser] CSV 파싱 중 CsvHelper 오류 발생: {e.Message}");
                Debug.LogError($"[CSVParser] 오류 발생 라인: {e.Context?.Parser?.Row}, 텍스트: {e.Context?.Parser?.RawRecord}");
                return new List<T>(); // 파싱 오류 발생 시 빈 리스트 반환
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[CSVParser] CSV 파싱 중 예상치 못한 오류 발생: {e.Message}");
                return new List<T>(); // 기타 오류 발생 시 빈 리스트 반환
            }
        }
    }
}