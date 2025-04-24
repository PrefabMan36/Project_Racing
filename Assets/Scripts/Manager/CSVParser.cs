using CsvHelper;
using CsvHelper.Configuration;
using System.Collections.Generic;
using System.Globalization;
using System.IO; // Path.Combine�� ���� �ʿ�
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public static class CSVParser
{
    /// <summary>
    /// StreamingAssets/CSV ������ �ִ� CSV ������ �񵿱������� �Ľ��Ͽ� ������ Ÿ���� ����Ʈ�� ��ȯ�մϴ�.
    /// UnityWebRequest�� ����Ͽ� �÷��� ȣȯ���� Ȯ���մϴ�.
    /// </summary>
    /// <typeparam name="T">CSV �� ���� ������ Ÿ�� (Ŭ����, ���ڵ�, struct ��)</typeparam>
    /// <param name="csvFileName">StreamingAssets/CSV ���� ���� CSV ���� �̸� (Ȯ���� ����! ��: "MyData.csv")</param>
    /// <param name="config">CSV �Ľ� ���� (���� ����)</param>
    /// <returns>�Ľ̵� �������� ����Ʈ�� ���� Task �Ǵ� ���� �߻� �� �� ����Ʈ�� ���� Task</returns>
    public static async Task<List<T>> ParseCSV<T>(string csvFileName, CsvConfiguration config = null)
        where T : class
    {
        string subfolder = "CSV"; // CSV ������ �ִ� ���� ���� �̸�
        // Application.streamingAssetsPath�� ���� ����, ���� �̸��� �����Ͽ� ���� ��� ����
        // Path.Combine�� �÷����� �´� ��� ������(\ �Ǵ� /)�� ����մϴ�.
        string fullPath = Path.Combine(Application.streamingAssetsPath, subfolder, csvFileName);

        // UnityWebRequest�� Application.streamingAssetsPath�� ������ ��θ� �� ó���մϴ�.
        // Android�� jar:file:// �� PC�� file:/// ���λ縦 �ڵ����� ó���ϴ� ��찡 �����ϴ�.
        string uri = fullPath;

        // *����*: �Ϻ� Android ȯ���̳� Ư�� Unity �������� jar:file:// ���λ� ������ �߻��� �� �ֽ��ϴ�.
        // ���� ������ �߻��ϸ� �Ʒ��� ���� �÷����� ���λ� ������ �߰��ؾ� �� ���� �ֽ��ϴ�.
        // ������ ��κ��� ��� Application.streamingAssetsPath + Path.Combine ��������ε� �۵��մϴ�.
        /*
        string uri;
        if (Application.platform == RuntimePlatform.Android)
        {
             // Android�� ��� apk �� ��δ� jar:file:// ���λ簡 �ʿ��� �� �ֽ��ϴ�.
             // UnityWebRequest�� �ڵ� ó������ ���Ѵٸ� ���⿡ �߰� ���� �ʿ�
             uri = fullPath; // �ϴ� Path.Combine ��� �״�� �õ�
             // ��: uri = "jar:file://" + fullPath; (������ �� ����� Path.Combine ����� ���� �������� �� ����)
        }
        else if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
             // WebGL�� HTTP ��û�̹Ƿ� ���λ� ���ʿ�
             uri = fullPath; // Application.streamingAssetsPath�� WebGL���� URL�� ��ȯ��
        }
        else
        {
             // PC (Editor, Windows, Mac, Linux ��), iOS �� ���� �ý��� ���
             // file:/// ���λ� �ʿ� (Path.Combine ����� ���� ����� ���)
             uri = "file:///" + fullPath; // 3 ������
        }
        */


        using (var www = UnityWebRequest.Get(uri))
        {
            await www.SendWebRequest(); // �񵿱� �ε� ���� �� ���

            if (www.result != UnityWebRequest.Result.Success)
            {
                // �ε� ���� �� ���� �α�
                Debug.LogError($"[CSVParser] StreamingAssets ���� �ε� ����: {www.error}");
                Debug.LogError($"[CSVParser] �ε� �õ� ��� (URI): {uri}");
                return new List<T>();
            }

            // �ε�� �ؽ�Ʈ �Ľ�
            string csvText = www.downloadHandler.text;

            // CsvHelper �Ľ� ���� (������ ����)
            var csvConfig = config ?? new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ",",
                IgnoreBlankLines = true,
                TrimOptions = TrimOptions.Trim,
                // Mode = CsvMode.Standard, // ���� ���� ���� �ذ� ���� ���ŵ�
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
                    // CsvHelper �Ľ� ���� �α� (e.Message ���)
                    Debug.LogError($"[CSVParser] CsvHelper �Ľ� ���� �߻�: {e.Message}");
                    // ���� 33.0.1���� e.Context.Reader.Context.Row/RawRecord ���� ���� �ȵ�
                    return new List<T>();
                }
                catch (System.Exception e)
                {
                    // �Ϲ� ���� �α�
                    Debug.LogError($"[CSVParser] CSV �Ľ� �� ����ġ ���� ���� �߻�: {e.Message}");
                    return new List<T>();
                }
            }
        }
    }
}