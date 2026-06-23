using UnityEngine;
using FPSGame.Save;
using UnityEngine.SceneManagement;

namespace FPSGame.Core
{
    /// <summary>
    /// Scene names and transitions for the full game flow.
    /// </summary>
    public static class GameSceneFlow
    {
        public const string IntroScene = "Intro";
        public const string ProfileSelectionScene = "ProfileSelection";
        public const string MainMenuScene = "MainMenu";
        public const string PrototypeGameplayScene = "PrototypeGameplay";

        public static void LoadIntro()
        {
            GameplayPauseService.ForceResume();
            LoadScene(IntroScene);
        }

        public static void LoadProfileSelection()
        {
            GameplayPauseService.ForceResume();
            LoadScene(ProfileSelectionScene);
        }

        public static void LoadMainMenu()
        {
            GameplayPauseService.ForceResume();
            LoadScene(MainMenuScene);
        }

        public static void RestartCurrentScene()
        {
            GameplayPauseService.ForceResume();

            var active = SceneManager.GetActiveScene();
            if (!active.IsValid())
                return;

            SceneManager.LoadScene(active.buildIndex);
        }

        public static void LoadPrototypeGameplay()
        {
            GameplayPauseService.ForceResume();
            LoadScene(PrototypeGameplayScene);
        }

        public static void LoadCampaignLevel(int campaign, int level)
        {
            GameplayPauseService.ForceResume();
            LoadSceneWithFallback(CampaignKeys.SceneNameForLevel(campaign, level), PrototypeGameplayScene);
        }

        public static void LoadSecretStage(int campaign)
        {
            GameplayPauseService.ForceResume();
            LoadSceneWithFallback(CampaignKeys.SceneNameForSecret(campaign), PrototypeGameplayScene);
        }

        public static void LoadLevelByKey(string levelKey)
        {
            GameplayPauseService.ForceResume();

            if (CampaignKeys.TryGetSceneNameForLevelKey(levelKey, out string sceneName))
                LoadSceneWithFallback(sceneName, PrototypeGameplayScene);
            else
                LoadPrototypeGameplay();
        }

        private static void LoadSceneWithFallback(string sceneName, string fallbackSceneName)
        {
            if (CanLoadScene(sceneName))
            {
                LoadScene(sceneName);
                return;
            }

            if (!string.IsNullOrEmpty(fallbackSceneName) && CanLoadScene(fallbackSceneName))
            {
                Debug.LogWarning(
                    $"[FPSGame] Scene '{sceneName}' not in Build Settings — loading fallback '{fallbackSceneName}'. " +
                    "Run FPSGame → Setup Campaign Level Scenes.");
                LoadScene(fallbackSceneName);
                return;
            }

            Debug.LogWarning($"[FPSGame] Scene '{sceneName}' is not in Build Settings.");
        }

        private static void LoadScene(string sceneName)
        {
            if (CanLoadScene(sceneName))
            {
                SceneManager.LoadScene(sceneName);
                return;
            }

            Debug.LogWarning($"[FPSGame] Scene '{sceneName}' is not in Build Settings.");
        }

        private static bool CanLoadScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
                return false;

            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(i);
                if (path.EndsWith(sceneName + ".unity"))
                    return true;
            }

            return false;
        }
    }
}
