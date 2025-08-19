using CsvHelper.Configuration;
using CsvHelper;
using System.Collections.Generic;
using System.Linq;

public sealed class TrackDataMap : ClassMap<TrackCheckPointData>
{
    public TrackDataMap()
    {
        Map(m => m.TrackName).Index(0);
        Map(m => m.CheckpointStrings).Convert(args =>
        {
            var row = args.Row;
            var record = row.Context.Parser.Record;
            if (record == null) return new List<string>();

            return record.Skip(1).ToList();
        });
    }
}