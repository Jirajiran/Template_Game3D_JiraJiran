using System;

namespace FPSGame.Save
{
    /// <summary>
    /// Serializable payload stored as profile_1.json … profile_3.json.
    /// </summary>
    [Serializable]
    public class ProfileSaveData
    {
        public int saveVersion = 1;
        public int wallet = 100;

        public string[] unlockedHeroIds = Array.Empty<string>();
        public string[] unlockedWeaponIds = Array.Empty<string>();
        public string[] unlockedLevelKeys = Array.Empty<string>();

        public bool secretStageUnlocked;

        public string selectedHeroId = string.Empty;
        public string[] loadoutWeaponIds = new string[3];
    }
}
