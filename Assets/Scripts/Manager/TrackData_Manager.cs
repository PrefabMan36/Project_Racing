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
    public List<TrackCheckPointData> trackCheckpointDatas;
    public List<TrackStateData> trackStateDatas;
    [SerializeField] private string trackCheckpointFileName = "Tracks_Checkpoint.csv";
    [SerializeField] private string trackStateFileName = "Tracks_State.csv";
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

        Debug.Log($"[TrackDataLoader] Attempting to load track data from: {trackCheckpointFileName}");
        List<TrackCheckPointData> trackEntries = await CSVParser.ParseCSV<TrackCheckPointData>(
            trackCheckpointFileName,
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
            Debug.LogWarning($"[TrackDataLoader] No data loaded from {trackCheckpointFileName} or file is empty/not found.");
            trackEntries = new List<TrackCheckPointData>();
        }

        Debug.Log($"[CSVLoader] StreamingAssets에서 '{trackStateFileName}' 로딩 시작...");
        List<TrackStateData> trackStateEntries = await CSVParser.ParseCSV<TrackStateData>(trackStateFileName);
        if (trackStateEntries != null && trackStateEntries.Count > 0)
        {
            Debug.Log($"[CSVLoader] '{trackStateFileName}' 파일에서 총 {trackStateEntries.Count}개의 데이터 레코드를 로드했습니다.");
        }
        else
        {
            Debug.LogWarning($"[CSVLoader] '{trackStateFileName}' 파일 로드에 실패했거나 데이터가 없습니다.");
        }
        trackStateDatas = trackStateEntries;
        trackCheckpointDatas = trackEntries;
    }

    public TrackCheckPointData GetTrackCheckpointDataByName(string trackName)
    {
        foreach (var data in trackCheckpointDatas)
        {
            if (data.TrackName == trackName)
            {
                return data;
            }
        }
        Debug.LogWarning($"TrackData with name '{trackName}' not found.");
        return null;
    }

    public TrackStateData GetTrackStateDataByName(string trackName)
    {
        foreach (var data in trackStateDatas)
        {
            if (data.Name == trackName)
            {
                return data;
            }
        }
        Debug.LogWarning($"TrackStateData with name '{trackName}' not found.");
        return null;
    }
}
