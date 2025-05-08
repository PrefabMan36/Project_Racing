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
            var row = args.Row; // 현재 행에 접근
            // CsvHelper 버전 및 컨텍스트에 따라 row.Parser.Record 또는 유사한 것을 사용해야 할 수 있음
            // args.Row.Context.Parser.Record 가 더 최신 버전에서 사용될 수 있음
            var record = row.Context.Parser.Record;
            if (record == null) return new List<string>();

            // 첫 번째 필드(TrackName)를 건너뛰고 나머지 필드를 가져옴
            return record.Skip(1).ToList();
        });
        // Checkpoints 속성은 [Ignore] 처리되었으므로 ClassMap에서 명시적으로 무시할 필요는 없음
    }
}