using CsvHelper.Configuration;
using CsvHelper;
using System.Collections.Generic;
using System.Linq;

public sealed class TrackDataMap : ClassMap<TrackData>
{
    public TrackDataMap()
    {
        Map(m => m.TrackName).Index(0);
        Map(m => m.CheckpointStrings).Convert(args =>
        {
            var row = args.Row; // ���� �࿡ ����
            // CsvHelper ���� �� ���ؽ�Ʈ�� ���� row.Parser.Record �Ǵ� ������ ���� ����ؾ� �� �� ����
            // args.Row.Context.Parser.Record �� �� �ֽ� �������� ���� �� ����
            var record = row.Context.Parser.Record;
            if (record == null) return new List<string>();

            // ù ��° �ʵ�(TrackName)�� �ǳʶٰ� ������ �ʵ带 ������
            return record.Skip(1).ToList();
        });
        // Checkpoints �Ӽ��� [Ignore] ó���Ǿ����Ƿ� ClassMap���� ��������� ������ �ʿ�� ����
    }
}