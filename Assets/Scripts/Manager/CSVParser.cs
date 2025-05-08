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
        Action<CsvContext> registerClassMapAction = null) // ClassMap ����� ���� Action �߰�
        where T : class
    {
        string subfolder = "CSV";
        string fullPath = Path.Combine(Application.streamingAssetsPath, subfolder, csvFileName);
        string uri = fullPath;

        // �÷����� URI ó�� ���� (���� �ڵ� ����)
        // ... UnityWebRequest ���� �κ� ...
        // ����: Android���� jar:file:// ���λ簡 �ʿ��� ��츦 ���� �ּ� ó���� ����
        // if (Application.platform == RuntimePlatform.Android) { /* ... */ }
        // else if (Application.platform == RuntimePlatform.WebGLPlayer) { /* ... */ }
        // else { uri = "file:///" + fullPath; } // PC ��

#if UNITY_ANDROID && !UNITY_EDITOR
        // Android �� ����̽������� StreamingAssets ��ο� Ư���� ó���� �ʿ� ���� �� ����
        // UnityWebRequest�� ���������� jar:file:// ó���� �� �� ����.
        // ������ �߻��ϸ� uri = "jar:file://" + fullPath; �� ���� ����� ó���� �ʿ��� �� ����.
#elif UNITY_IOS && !UNITY_EDITOR
        // iOS�� ��������
#elif UNITY_STANDALONE || UNITY_EDITOR
        uri = "file:///" + fullPath; // Editor �� PC ���忡���� file:/// ���λ� ���
#endif
        // WebGL�� ��� Application.streamingAssetsPath ��ü�� URL�̹Ƿ� �߰� ���λ� ���ʿ�

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
                HasHeaderRecord = true, // �⺻���� true, TrackDataLoader���� false�� �������̵�
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
                        // Action�� ���� ClassMap ���
                        registerClassMapAction?.Invoke(csv.Context);

                        // HasHeaderRecord ������ ���� ����� �аų� ���� ����
                        if (csvConfig.HasHeaderRecord)
                        {
                            csv.Read(); // ù ���� �а�
                            csv.ReadHeader(); // ����� ó��
                        }
                        // HasHeaderRecord�� false�̸�, ù �ٺ��� �����ͷ� ���� (csv.Read()�� GetRecords<T> ���ο��� ȣ���)

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
    /// StreamingAssets/CSV ������ �ִ� CSV ������ �񵿱������� �Ľ��Ͽ� ������ Ÿ���� ����Ʈ�� ��ȯ�մϴ�.
    /// UnityWebRequest�� ����Ͽ� �÷��� ȣȯ���� Ȯ���մϴ�.
    /// </summary>
    /// <typeparam name="T">CSV �� ���� ������ Ÿ�� (Ŭ����, ���ڵ�, struct ��)</typeparam>
    /// <param name="csvFileName">StreamingAssets/CSV ���� ���� CSV ���� �̸� (Ȯ���� ����! ��: "MyData.csv")</param>
    /// <param name="config">CSV �Ľ� ���� (���� ����)</param>
    /// <returns>�Ľ̵� �������� ����Ʈ�� ���� Task �Ǵ� ���� �߻� �� �� ����Ʈ�� ���� Task</returns>
}