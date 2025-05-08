using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using CsvHelper.Configuration;
using System.Globalization;
using CsvHelper;
using System.IO;

public class TrackData_Manager : MonoBehaviour
{
    public static TrackData_Manager instance { get; private set; }
    public List<TrackData> trackDatas;
    [SerializeField] private string csvFileName = "Tracks.csv";
    private async void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            await LoadTrackData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public async Task LoadTrackData()
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = false, // Track.txt does not have a header row
            MissingFieldFound = null, // Handles potentially missing fields gracefully
            Delimiter = "," // Specify the delimiter
        };

        Debug.Log($"[TrackDataLoader] Attempting to load track data from: {csvFileName}");
        List<TrackData> trackEntries = await CSVParser.ParseCSV<TrackData>(
            csvFileName,
            config,
            context => context.RegisterClassMap<TrackDataMap>()
        );
        if (trackEntries != null && trackEntries.Count > 0)
        {
            Debug.Log($"[TrackDataLoader] Successfully loaded {trackEntries.Count} track(s).");
            foreach (var trackEntry in trackEntries)
            {
                trackEntry.ProcessCheckpointStrings();
                Debug.Log($"[TrackDataLoader] Track: {trackEntry.TrackName}, Checkpoints Parsed: {trackEntry.Checkpoints?.Count ?? 0}");
            }
        }
        else
        {
            Debug.LogWarning($"[TrackDataLoader] No data loaded from {csvFileName} or file is empty/not found.");
            trackEntries = new List<TrackData>();
        }
        trackDatas = trackEntries;
    }

    public TrackData GetTrackDataByName(string trackName)
    {
        foreach (var data in trackDatas)
        {
            if (data.TrackName == trackName)
            {
                return data;
            }
        }
        Debug.LogWarning($"TrackData with name '{trackName}' not found.");
        return null;
    }
}
