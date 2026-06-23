namespace FPSGame.Save
{
    /// <summary>Level key helpers: c{campaign}_l{level}, e.g. c1_l1.</summary>
    public static class CampaignKeys
    {
        public const int CampaignCount = 4;
        public const int LevelsPerCampaign = 4;

        public const string LevelsSceneFolder = "Assets/FPSGame/Scenes/Levels";
        public const string SecretSceneFolder = "Assets/FPSGame/Scenes/Secret";

        public static string Level(int campaign, int level) => $"c{campaign}_l{level}";

        public static string SecretStage(int campaign) => $"secret_c{campaign}";

        public static string SceneNameForLevel(int campaign, int level) => $"Level_c{campaign}_l{level}";

        public static string SceneNameForSecret(int campaign) => $"Secret_c{campaign}";

        public static string SceneAssetPathForLevel(int campaign, int level) =>
            $"{LevelsSceneFolder}/Level_c{campaign}_l{level}.unity";

        public static string SceneAssetPathForSecret(int campaign) =>
            $"{SecretSceneFolder}/Secret_c{campaign}.unity";

        public static bool TryGetSceneNameForLevelKey(string levelKey, out string sceneName)
        {
            sceneName = string.Empty;
            if (string.IsNullOrEmpty(levelKey))
                return false;

            if (levelKey.StartsWith("secret_c") && int.TryParse(levelKey.Substring(8), out int secretCampaign))
            {
                sceneName = SceneNameForSecret(secretCampaign);
                return true;
            }

            if (!levelKey.StartsWith("c") || levelKey.Length < 5)
                return false;

            int underscore = levelKey.IndexOf('_');
            if (underscore <= 1 || underscore >= levelKey.Length - 2)
                return false;

            if (!int.TryParse(levelKey.Substring(1, underscore - 1), out int campaign))
                return false;

            if (!levelKey.Substring(underscore + 1).StartsWith("l"))
                return false;

            if (!int.TryParse(levelKey.Substring(underscore + 2), out int level))
                return false;

            sceneName = SceneNameForLevel(campaign, level);
            return true;
        }
    }
}
