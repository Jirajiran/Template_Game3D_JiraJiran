using System.Collections.Generic;
using System.IO;
using FPSGame.Save;
using UnityEditor;
using UnityEngine;

namespace FPSGame.Editor
{
    /// <summary>
    /// Syncs Editor Build Settings for menu + prototype + campaign level scenes.
    /// </summary>
    public static class FPSGameEditorBuildSettings
    {
        private const string Root = "Assets/FPSGame";
        private const string IntroPath = Root + "/Scenes/Intro.unity";
        private const string ProfilePath = Root + "/Scenes/ProfileSelection.unity";
        private const string MainMenuPath = Root + "/Scenes/MainMenu.unity";
        private const string PrototypePath = Root + "/Scenes/PrototypeGameplay.unity";

        public static void SyncBuildSettings()
        {
            var scenes = new List<EditorBuildSettingsScene>();
            AddSceneIfExists(scenes, IntroPath);
            AddSceneIfExists(scenes, ProfilePath);
            AddSceneIfExists(scenes, MainMenuPath);
            AddSceneIfExists(scenes, PrototypePath);

            for (int campaign = 1; campaign <= CampaignKeys.CampaignCount; campaign++)
            {
                for (int level = 1; level <= CampaignKeys.LevelsPerCampaign; level++)
                    AddSceneIfExists(scenes, CampaignKeys.SceneAssetPathForLevel(campaign, level));
            }

            for (int campaign = 1; campaign <= CampaignKeys.CampaignCount; campaign++)
                AddSceneIfExists(scenes, CampaignKeys.SceneAssetPathForSecret(campaign));

            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void AddSceneIfExists(List<EditorBuildSettingsScene> scenes, string assetPath)
        {
            if (!File.Exists(assetPath))
                return;

            for (int i = 0; i < scenes.Count; i++)
            {
                if (scenes[i].path == assetPath)
                    return;
            }

            scenes.Add(new EditorBuildSettingsScene(assetPath, true));
        }
    }
}
