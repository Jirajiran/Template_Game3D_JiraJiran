namespace FPSGame.Save
{
    /// <summary>Level key helpers: c{campaign}_l{level}, e.g. c1_l1.</summary>
    public static class CampaignKeys
    {
        public const int CampaignCount = 4;
        public const int LevelsPerCampaign = 4;

        public static string Level(int campaign, int level) => $"c{campaign}_l{level}";

        public static string SecretStage(int campaign) => $"secret_c{campaign}";
    }
}
