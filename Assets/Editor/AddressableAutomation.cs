using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using System.Collections.Generic;
using System.IO;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;

public class AddressableAutomation
{
    private static readonly List<string> targetFolders = new List<string>
    {
        "Assets/Audio/BGM",
        "Assets/Images/Atlas",
        "Assets/Scenes/Tracks",
        "Assets/Prefabs/Cars/ForPlayer"
    };

    // --- 추가된 부분: 제외할 파일 확장자 및 파일명 리스트 ---
    private static readonly List<string> excludedExtensions = new List<string>
    {
        ".cs", ".dll", ".asmdef"
    };

    private static readonly List<string> excludedFileNames = new List<string>
    {
        "PostProcessLayer.png", // 포스트 프로세싱 관련 파일 예시
        "NavMeshSurface Icon.png"
    };
    // --- 추가된 부분 끝 ---

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
    private static void MakeAssetAddressableWithDependencies(AddressableAssetSettings settings, string assetPath)
    {
        // 프리팹이나 씬인 경우에만 종속성을 검사합니다.
        if (assetPath.EndsWith(".prefab") || assetPath.EndsWith(".unity"))
        {
            string[] dependencies = AssetDatabase.GetDependencies(assetPath, true);
            foreach (string depPath in dependencies)
            {
                // --- 수정된 부분: 제외 로직을 통과하는 에셋만 추가 ---
                if (!IsExcludedAsset(depPath))
                {
                    AddAddressableEntry(settings, depPath);
                }
                // --- 수정된 부분 끝 ---
            }
        }
        else
        {
            // --- 수정된 부분: 단일 에셋도 제외 로직 검사 ---
            if (!IsExcludedAsset(assetPath))
            {
                AddAddressableEntry(settings, assetPath);
            }
            // --- 수정된 부분 끝 ---
        }
    }

    /// <summary>
    /// 실제로 에셋을 Addressable 그룹에 추가하는 함수입니다.
    /// </summary>
    private static void AddAddressableEntry(AddressableAssetSettings settings, string path)
    {
        string guid = AssetDatabase.AssetPathToGUID(path);
        if (string.IsNullOrEmpty(guid))
            return;

        // 이미 어드레서블로 등록되었는지 확인
        if (settings.FindAssetEntry(guid) != null)
            return;

        string groupName = new DirectoryInfo(Path.GetDirectoryName(path)).Name;
        AddressableAssetGroup group = settings.FindGroup(groupName);

        if (group == null)
        {
            group = settings.CreateGroup
            (
                groupName,
                false,
                false,
                true,
                null,
                typeof(BundledAssetGroupSchema),
                typeof(ContentUpdateGroupSchema)
            );
        }

        AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group, false, false);
        entry.address = path;
    }

    // --- 추가된 부분: 제외할 에셋인지 판별하는 함수 ---
    /// <summary>
    /// 이 에셋이 제외 목록에 포함되는지 확인합니다.
    /// </summary>
    /// <param name="path">에셋의 경로</param>
    /// <returns>제외 대상이면 true, 아니면 false</returns>
    private static bool IsExcludedAsset(string path)
    {
        string fileName = Path.GetFileName(path);
        if (excludedFileNames.Contains(fileName))
        {
            // Debug.Log($"제외(파일명): {path}");
            return true;
        }

        string extension = Path.GetExtension(path);
        if (excludedExtensions.Contains(extension))
        {
            // Debug.Log($"제외(확장자): {path}");
            return true;
        }

        return false;
    }
    // --- 추가된 부분 끝 ---
}