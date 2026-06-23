using System.IO;
using FPSGame.Save;
using UnityEditor;
using UnityEngine;

namespace FPSGame.Editor
{
    /// <summary>
    /// Creates per-level gameplay scenes from PrototypeGameplay and registers them in Build Settings.
    /// Menu: FPSGame / Setup Campaign Level Scenes
    /// </summary>
    public static class FPSGameCampaignScenesSetup
    {
        private const string PrototypePath = "Assets/FPSGame/Scenes/PrototypeGameplay.unity";

        [MenuItem("FPSGame/Setup Campaign Level Scenes", false, 126)]
        public static void RunCampaignScenesSetup()
        {
            FPSGameProjectSetup.RunPhase0SetupSilent();

            if (!File.Exists(PrototypePath))
            {
                EditorUtility.DisplayDialog(
                    "FPSGame Campaign Scenes",
                    "PrototypeGameplay.unity not found.\nRun Phase 0 first.",
                    "OK");
                return;
            }

            EnsureFolder(CampaignKeys.LevelsSceneFolder);
            EnsureFolder(CampaignKeys.SecretSceneFolder);

            int created = 0;
            int skipped = 0;

            for (int campaign = 1; campaign <= CampaignKeys.CampaignCount; campaign++)
            {
                for (int level = 1; level <= CampaignKeys.LevelsPerCampaign; level++)
                {
                    if (CopySceneIfMissing(
                            PrototypePath,
                            CampaignKeys.SceneAssetPathForLevel(campaign, level)))
                        created++;
                    else
                        skipped++;
                }

                if (CopySceneIfMissing(
                        PrototypePath,
                        CampaignKeys.SceneAssetPathForSecret(campaign)))
                    created++;
                else
                    skipped++;
            }

            FPSGameEditorBuildSettings.SyncBuildSettings();

            var victoryPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(FPSGameCampaignWinSetup.VictoryPrefabPath);
            int wired = 0;
            if (victoryPrefab != null)
                wired = WireWinOnAllCampaignScenes(victoryPrefab);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "FPSGame Campaign Scenes",
                $"Level scenes ready.\n\n" +
                $"Created: {created}\n" +
                $"Skipped (already exist): {skipped}\n" +
                $"Win UI wired: {wired} scenes\n\n" +
                "Campaign loads Level_cX_lY.unity per level.\n" +
                "Edit scenes under Scenes/Levels/ and Scenes/Secret/.",
                "OK");

            Debug.Log("[FPSGame] Campaign level scenes setup complete.");
        }

        private static bool CopySceneIfMissing(string sourcePath, string destinationPath)
        {
            if (File.Exists(destinationPath))
                return false;

            if (!AssetDatabase.CopyAsset(sourcePath, destinationPath))
            {
                Debug.LogWarning($"[FPSGame] Failed to copy scene to {destinationPath}");
                return false;
            }

            return true;
        }

        private static int WireWinOnAllCampaignScenes(GameObject victoryPrefab)
        {
            int wired = 0;

            for (int campaign = 1; campaign <= CampaignKeys.CampaignCount; campaign++)
            {
                for (int level = 1; level <= CampaignKeys.LevelsPerCampaign; level++)
                {
                    string path = CampaignKeys.SceneAssetPathForLevel(campaign, level);
                    if (!File.Exists(path))
                        continue;

                    FPSGameCampaignWinSetup.SetupGameplayScene(path, victoryPrefab);
                    wired++;
                }

                string secretPath = CampaignKeys.SceneAssetPathForSecret(campaign);
                if (!File.Exists(secretPath))
                    continue;

                FPSGameCampaignWinSetup.SetupGameplayScene(secretPath, victoryPrefab);
                wired++;
            }

            return wired;
        }

        private static void EnsureFolder(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath))
                return;

            string parent = Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
            string folderName = Path.GetFileName(assetPath);
            if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(folderName))
                return;

            if (!AssetDatabase.IsValidFolder(parent))
                EnsureFolder(parent);

            AssetDatabase.CreateFolder(parent, folderName);
        }
    }
}
