using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 차량 이름에 해당하는 엔진 데이터 CSV 파일을 읽어
/// 마력과 토크 AnimationCurve를 생성하고 관리합니다.
/// </summary>
public class EngineCurveManager
{
    // 생성된 커브를 저장할 속성
    public AnimationCurve HorsepowerCurve { get; private set; }
    public AnimationCurve TorqueCurve { get; private set; }

    /// <summary>
    /// 지정된 차량 이름으로 엔진 커브 데이터를 비동기적으로 로드합니다.
    /// </summary>
    /// <param name="carName">불러올 차량의 이름 (예: "Super2000")</param>
    public async Task LoadCurves(string carName)
    {
        // CSV 파일은 StreamingAssets 폴더 내의 EngineCurves 폴더에 있다고 가정합니다.
        string subfolder = "EngineCurves";
        string fileName = $"{carName}.csv";
        string fullPath = Path.Combine(Application.streamingAssetsPath, subfolder, fileName);
        string uri = fullPath;

        // 플랫폼별 URI 경로 설정
#if UNITY_ANDROID && !UNITY_EDITOR
        // Android에서는 경로가 그대로 사용됩니다.
#elif UNITY_IOS && !UNITY_EDITOR
        // iOS에서는 경로가 그대로 사용됩니다.
#elif UNITY_STANDALONE || UNITY_EDITOR
        uri = "file:///" + fullPath;
#endif

        Debug.Log($"[EngineCurveManager] 로딩 시도: {uri}");

        using (var www = UnityWebRequest.Get(uri))
        {
            var operation = www.SendWebRequest();

            // 요청이 완료될 때까지 기다립니다.
            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[EngineCurveManager] 파일 로드 에러: {www.error} (URI: {uri})");
                // 에러 발생 시 커브를 비워둡니다.
                HorsepowerCurve = new AnimationCurve();
                TorqueCurve = new AnimationCurve();
                return;
            }

            // 다운로드 받은 텍스트 데이터를 파싱합니다.
            string csvText = www.downloadHandler.text;
            ParseCsvAndCreateCurves(csvText);
        }
    }

    /// <summary>
    /// CSV 텍스트를 파싱하여 AnimationCurve를 생성합니다.
    /// </summary>
    /// <param name="csvText">파싱할 CSV 파일의 전체 텍스트</param>
    private void ParseCsvAndCreateCurves(string csvText)
    {
        // 줄 단위로 텍스트를 나눕니다.
        var lines = csvText.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length < 3)
        {
            Debug.LogError("[EngineCurveManager] CSV 파일은 최소 3줄(RPM, 마력, 토크)이 필요합니다.");
            return;
        }

        try
        {
            // 각 줄을 파싱하여 float 리스트로 변환합니다.
            // 첫 번째 열(헤더 이름)은 건너뜁니다 (Skip(1)).
            var rpmValues = lines[0].Split(',').Skip(1).Select(s => float.Parse(s, CultureInfo.InvariantCulture)).ToList();
            var hpValues = lines[1].Split(',').Skip(1).Select(s => float.Parse(s, CultureInfo.InvariantCulture)).ToList();
            var torqueValues = lines[2].Split(',').Skip(1).Select(s => float.Parse(s, CultureInfo.InvariantCulture)).ToList();

            // 데이터의 개수가 일치하는지 확인합니다.
            if (rpmValues.Count != hpValues.Count || rpmValues.Count != torqueValues.Count)
            {
                Debug.LogError("[EngineCurveManager] RPM, 마력, 토크 데이터의 개수가 일치하지 않습니다.");
                return;
            }

            // Keyframe 리스트를 생성합니다.
            var hpKeyframes = new List<Keyframe>();
            var torqueKeyframes = new List<Keyframe>();

            for (int i = 0; i < rpmValues.Count; i++)
            {
                // Keyframe(time, value) -> Keyframe(RPM, 마력 또는 토크)
                hpKeyframes.Add(new Keyframe(rpmValues[i], hpValues[i]));
                torqueKeyframes.Add(new Keyframe(rpmValues[i], torqueValues[i]));
            }

            // Keyframe 리스트로 AnimationCurve를 생성합니다.
            HorsepowerCurve = new AnimationCurve(hpKeyframes.ToArray());
            TorqueCurve = new AnimationCurve(torqueKeyframes.ToArray());

            // 커브를 부드럽게 만들어줍니다. (선택 사항)
            for (int i = 0; i < HorsepowerCurve.keys.Length; i++)
            {
                HorsepowerCurve.SmoothTangents(i, 0f);
                TorqueCurve.SmoothTangents(i, 0f);
            }

            Debug.Log($"[EngineCurveManager] 커브 생성 완료! 키프레임 {HorsepowerCurve.length}개.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[EngineCurveManager] CSV 파싱 중 에러 발생: {e.Message}");
        }
    }
}