using CsvHelper;
using CsvHelper.Configuration;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine; // Debug.LogError�� ���� �ʿ�

public static class CSVParser
{
    /// <summary>
    /// Resources ������ �ִ� CSV ������ �Ľ��Ͽ� ������ Ÿ���� ����Ʈ�� ��ȯ�մϴ�.
    /// CSVHelper�� �ڵ� ���� ����� ����մϴ�.
    /// </summary>
    /// <typeparam name="T">CSV �� ���� ������ Ÿ�� (Ŭ����, ���ڵ�, struct ��)</typeparam>
    /// <param name="resourceFileName">Resources ���� ���� CSV ���� �̸� (Ȯ���� ����)</param>
    /// <param name="config">CSV �Ľ� ���� (���� ����)</param>
    /// <returns>�Ľ̵� �������� ����Ʈ �Ǵ� ���� �߻� �� �� ����Ʈ</returns>
    public static List<T> ParseCSV<T>(string resourceFileName, CsvConfiguration config = null)
    {
        TextAsset csvTextAsset = Resources.Load<TextAsset>(resourceFileName);

        if (csvTextAsset == null)
        {
            Debug.LogError($"[CSVParser] CSV ���� '{resourceFileName}.csv'�� Resources �������� ã�� �� �����ϴ�.");
            return new List<T>(); // ������ ã�� ���� ��� �� ����Ʈ ��ȯ
        }

        using (var reader = new StringReader(csvTextAsset.text))
        {
            // �⺻ ���� (RFC4180 ǥ��, ��ǥ ����, ��� ���� ����)
            var csvConfig = config ?? new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,       // ù ���� ����� ����
                Delimiter = ",",              // ��ǥ ������
                IgnoreBlankLines = true,      // �� �� ����
                TrimOptions = TrimOptions.Trim, // ���� ����
                // �پ��� OS�� �ٹٲ� ���� (\r\n, \n, \r)�� �ڵ����� ó���մϴ�. StringReader�� �⺻������ �� ó���մϴ�.
                //Mode = CsvMode.Standard,      // ǥ�� CSV ���
            };

            try
            {
                using (var csv = new CsvReader(reader, csvConfig))
                {
                    // CsvHelper�� �⺻������ Ŭ���� �Ӽ� �̸��� ��� �̸��� �����մϴ�.
                    // ���� ���� ��Ģ�� �����ϴٸ� CsvClassMap<T>�� �����Ͽ� ����ؾ� �մϴ�.
                    // ��: csv.Context.RegisterClassMap<YourCustomMap>();

                    // ����� �о� �÷� ������ �غ�
                    csv.Read();
                    csv.ReadHeader();

                    // ��� ���ڵ带 ������ Ÿ�� T�� ����Ʈ�� �о��
                    return csv.GetRecords<T>().ToList();
                }
            }
            catch (CsvHelperException e)
            {
                Debug.LogError($"[CSVParser] CSV �Ľ� �� CsvHelper ���� �߻�: {e.Message}");
                Debug.LogError($"[CSVParser] ���� �߻� ����: {e.Context?.Parser?.Row}, �ؽ�Ʈ: {e.Context?.Parser?.RawRecord}");
                return new List<T>(); // �Ľ� ���� �߻� �� �� ����Ʈ ��ȯ
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[CSVParser] CSV �Ľ� �� ����ġ ���� ���� �߻�: {e.Message}");
                return new List<T>(); // ��Ÿ ���� �߻� �� �� ����Ʈ ��ȯ
            }
        }
    }
}