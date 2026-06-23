using UnityEngine;

namespace FPSGame.Save
{
    [CreateAssetMenu(fileName = "SaveStartPack", menuName = "FPSGame/Save/Start Pack")]
    public class SaveStartPack : ScriptableObject
    {
        [Header("Starter unlocks")]
        public string[] starterHeroIds = { "hero_default" };
        public string[] starterWeaponIds = { "pistol_01", "knife_01", "ak_01", "sniper_01", "rpg_raycast" };
        public string[] starterLevelKeys = { "c1_l1" };
        public int starterWallet = 100;

        [Header("Default loadout")]
        public string defaultHeroId = "hero_default";
        public string[] defaultLoadoutWeaponIds = { "pistol_01", "knife_01", string.Empty };

        public ProfileSaveData CreateNewProfile()
        {
            var data = new ProfileSaveData
            {
                saveVersion = 1,
                wallet = starterWallet,
                unlockedHeroIds = CloneArray(starterHeroIds),
                unlockedWeaponIds = CloneArray(starterWeaponIds),
                unlockedLevelKeys = CloneArray(starterLevelKeys),
                secretStageUnlocked = false,
                selectedHeroId = defaultHeroId,
                loadoutWeaponIds = NormalizeLoadout(defaultLoadoutWeaponIds)
            };

            return data;
        }

        private static string[] CloneArray(string[] source)
        {
            if (source == null || source.Length == 0)
                return System.Array.Empty<string>();

            var copy = new string[source.Length];
            for (int i = 0; i < source.Length; i++)
                copy[i] = source[i] ?? string.Empty;

            return copy;
        }

        private static string[] NormalizeLoadout(string[] source)
        {
            var result = new string[3];
            for (int i = 0; i < result.Length; i++)
                result[i] = source != null && i < source.Length ? source[i] ?? string.Empty : string.Empty;

            return result;
        }
    }
}
