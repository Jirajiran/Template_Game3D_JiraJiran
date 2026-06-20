namespace FPSGame.UI
{
    /// <summary>
    /// Remembers the last campaign level launched from the menu (for pause → menu restore).
    /// </summary>
    public static class CampaignSessionState
    {
        public static int ActiveCampaign { get; private set; }
        public static string ActiveLevelKey { get; private set; }
        public static bool IsSecretStage { get; private set; }

        public static bool HasActiveCampaignContext => ActiveCampaign >= 1;

        public static void BeginCampaignLevel(int campaign, int level)
        {
            ActiveCampaign = campaign;
            ActiveLevelKey = FPSGame.Save.CampaignKeys.Level(campaign, level);
            IsSecretStage = false;
        }

        public static void BeginSecretStage(int campaign)
        {
            ActiveCampaign = campaign;
            ActiveLevelKey = FPSGame.Save.CampaignKeys.SecretStage(campaign);
            IsSecretStage = true;
        }

        public static void Clear()
        {
            ActiveCampaign = 0;
            ActiveLevelKey = string.Empty;
            IsSecretStage = false;
        }
    }
}
