using System.Collections.Generic;
using CsvHelper.Configuration.Attributes;

public class TrackData
{
    [Index(0)]
    public string TrackName { get; set; }
    public List<string> CheckpointStrings { get; set; }

    [Ignore]
    public List<CheckpointData> Checkpoints { get; private set; }

    public void ProcessCheckpointStrings()
    {
        Checkpoints = new List<CheckpointData>();
        if (CheckpointStrings != null)
        {
            foreach (var checkpointStr in CheckpointStrings)
            {
                if (!string.IsNullOrWhiteSpace(checkpointStr))
                {
                    Checkpoints.Add(new CheckpointData(checkpointStr));
                }
            }
        }
    }
}