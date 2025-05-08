using UnityEngine;
public class CheckpointData
{
    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }
    public Vector3 Scale { get; set; }
    public CheckpointData(string checkpointString)
    {
        string[] parts = checkpointString.Split(';');
        if (parts.Length == 3)
        {
            Position = ParseVector3(parts[0]);
            Rotation = ParseVector3(parts[1]);
            Scale = ParseVector3(parts[2]);
        }
        else
        {
            Debug.LogError($"Invalid checkpoint string format: {checkpointString}");
            Position = Vector3.zero;
            Rotation = Vector3.zero;
            Scale = Vector3.zero;
        }
    }

    private Vector3 ParseVector3(string vectorString)
    {
        float x = 0, y = 0, z = 0;
        string[] coords = vectorString.Split(new char[] { 'x', 'y', 'z' }, System.StringSplitOptions.RemoveEmptyEntries);
        try
        {
            var xMatch = System.Text.RegularExpressions.Regex.Match(vectorString, @"x:([-\d\.]+)");
            var yMatch = System.Text.RegularExpressions.Regex.Match(vectorString, @"y:([-\d\.]+)");
            var zMatch = System.Text.RegularExpressions.Regex.Match(vectorString, @"z:([-\d\.]+)");

            if (xMatch.Success) float.TryParse(xMatch.Groups[1].Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out x);
            if (yMatch.Success) float.TryParse(yMatch.Groups[1].Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out y);
            if (zMatch.Success) float.TryParse(zMatch.Groups[1].Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out z);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[CheckpointData] Error parsing vector string '{vectorString}': {ex.Message}");
        }
        return new Vector3(x, y, z);
    }
}