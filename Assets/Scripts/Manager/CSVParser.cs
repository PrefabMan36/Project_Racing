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

        #if UNITY_ANDROID && !UNITY_EDITOR
        #elif UNITY_IOS && !UNITY_EDITOR
        #elif UNITY_STANDALONE || UNITY_EDITOR
                uri = "file:///" + fullPath;
        #endif
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
                HasHeaderRecord = true,
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
                        registerClassMapAction?.Invoke(csv.Context);

                        if (csvConfig.HasHeaderRecord)
                        {
                            csv.Read();
                            csv.ReadHeader();
                        }

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
}