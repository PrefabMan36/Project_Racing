using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using System.Collections.Generic;
using System.IO;

public class AddressableAutomation
{
    private static readonly List<string> targetFolders = new List<string>
    {
        "Assets/Audio/BGM",
        "Assets/Images/Atlas"
    };

    /// <summary>
    /// 에디터 메뉴에서 이 함수를 호출하여 Addressables를 업데이트합니다.
    /// </summary>
    [MenuItem("Tools/Addressables/Update All Addressables")]
    public static void UpdateAllAddressables()
    {
        Debug.Log("Addressable 자동 등록을 시작합니다...");

        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("Addressable Asset 설정을 찾을 수 없습니다. Window > Asset Management > Addressables > Groups 에서 설정을 먼저 생성해주세요.");
            return;
        }

        int processedAssetCount = 0;

        foreach (string folder in targetFolders)
        {
            if (!AssetDatabase.IsValidFolder(folder))
            {
                Debug.LogWarning($"폴더를 찾을 수 없습니다: {folder}");
                continue;
            }

            string[] guids = AssetDatabase.FindAssets("t:Object", new[] { folder });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                if (AssetDatabase.IsValidFolder(path))
                {
                    continue;
                }

                MakeAssetAddressableWithDependencies(settings, path);
                processedAssetCount++;
            }
        }

        Debug.Log($"총 {processedAssetCount}개의 원본 에셋과 그 종속성들을 Addressable로 처리했습니다. 성공적으로 완료되었습니다!");
    }

    /// <summary>
    /// 지정된 경로의 에셋과 그 모든 종속성을 Addressable로 만듭니다.
    /// </summary>
    /// <param name="settings">Addressable 설정</param>
    /// <param name="assetPath">에셋의 경로</param>
    private static void MakeAssetAddressableWithDependencies(AddressableAssetSettings settings, string assetPath)
    {
        // 프리팹인 경우에만 종속성을 검사합니다.
        // 만약 다른 에셋 타입(예: Scene)도 종속성 처리를 하고 싶다면 아래 조건을 수정하면 됩니다.
        if (assetPath.EndsWith(".prefab"))
        {
            string[] dependencies = AssetDatabase.GetDependencies(assetPath, true);
            foreach (string depPath in dependencies)
            {
                if (!depPath.EndsWith(".cs"))
                {
                    AddAddressableEntry(settings, depPath);
                }
            }
        }
        else
        {
            AddAddressableEntry(settings, assetPath);
        }
    }

    /// <summary>
    /// 실제로 에셋을 Addressable 그룹에 추가하는 함수입니다.
    /// </summary>
    /// <param name="settings">Addressable 설정</param>
    /// <param name="path">에셋의 경로</param>
    private static void AddAddressableEntry(AddressableAssetSettings settings, string path)
    {
        string guid = AssetDatabase.AssetPathToGUID(path);

        string groupName = new DirectoryInfo(Path.GetDirectoryName(path)).Name;

        AddressableAssetGroup group = settings.FindGroup(groupName) ?? settings.CreateGroup(groupName, false, false, true, null);

        AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group, false, false);

        entry.address = path;
    }
}