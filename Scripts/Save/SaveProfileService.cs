using System;
using System.Collections.Generic;
using System.IO;
using FPSGame.Core;
using UnityEngine;

namespace FPSGame.Save
{
    /// <summary>
    /// Loads/saves profile_1.json … profile_3.json under persistentDataPath (or custom root).
    /// </summary>
    public static class SaveProfileService
    {
        public const int MinProfileIndex = 1;
        public const int MaxProfileIndex = 3;
        public const int LoadoutSlotCount = 3;

        private static SaveStartPack startPack;
        private static string saveRootOverride;

        public static int CurrentProfileIndex { get; private set; }
        public static ProfileSaveData Data { get; private set; } = new ProfileSaveData();
        public static bool HasActiveProfile => CurrentProfileIndex >= MinProfileIndex;

        public static string SaveDirectory => GetSaveDirectory();

        public static event Action<int> OnProfileLoaded;
        public static event Action<int> OnProfileSaved;

        public static void Configure(SaveStartPack pack) => startPack = pack;

        public static void SetSaveRoot(string folder)
        {
            saveRootOverride = string.IsNullOrWhiteSpace(folder) ? null : folder.Trim();
        }

        public static string GetProfilePath(int profileIndex)
        {
            profileIndex = ClampProfileIndex(profileIndex);
            return Path.Combine(GetSaveDirectory(), $"profile_{profileIndex}.json");
        }

        public static bool ProfileFileExists(int profileIndex) =>
            File.Exists(GetProfilePath(profileIndex));

        public static void LoadOrCreate(int profileIndex)
        {
            profileIndex = ClampProfileIndex(profileIndex);
            CurrentProfileIndex = profileIndex;

            if (TryLoadFromDisk(profileIndex))
            {
                NormalizeData();
                OnProfileLoaded?.Invoke(profileIndex);
                return;
            }

            Data = CreateDefaultProfile();
            NormalizeData();
            Save();
            OnProfileLoaded?.Invoke(profileIndex);
        }

        public static void Save()
        {
            if (!HasActiveProfile)
                return;

            NormalizeData();
            string path = GetProfilePath(CurrentProfileIndex);

            try
            {
                string json = JsonUtility.ToJson(Data, true);
                File.WriteAllText(path, json);
                OnProfileSaved?.Invoke(CurrentProfileIndex);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveProfileService] Failed to write {path}: {e.Message}");
            }
        }

        public static void DeleteProfile(int profileIndex)
        {
            profileIndex = ClampProfileIndex(profileIndex);
            string path = GetProfilePath(profileIndex);
            if (File.Exists(path))
                File.Delete(path);

            if (CurrentProfileIndex != profileIndex)
                return;

            Data = new ProfileSaveData();
            CurrentProfileIndex = 0;
        }

        public static bool IsHeroUnlocked(string heroId) => ContainsId(Data?.unlockedHeroIds, heroId);

        public static bool IsWeaponUnlocked(string weaponId) => ContainsId(Data?.unlockedWeaponIds, weaponId);

        public static bool IsLevelUnlocked(string levelKey)
        {
            if (string.IsNullOrEmpty(levelKey))
                return true;

            return ContainsId(Data?.unlockedLevelKeys, levelKey);
        }

        public static bool IsSecretStageUnlocked => Data != null && Data.secretStageUnlocked;

        public static bool IsSecretStageAvailable(int campaign) =>
            IsSecretStageUnlocked && IsLevelUnlocked(CampaignKeys.SecretStage(campaign));

        public static void UnlockSecretStage(bool saveImmediately = true)
        {
            if (Data == null)
                return;

            Data.secretStageUnlocked = true;

            for (int campaign = 1; campaign <= CampaignKeys.CampaignCount; campaign++)
                UnlockLevel(CampaignKeys.SecretStage(campaign), saveImmediately: false);

            if (saveImmediately)
                Save();
        }

        public static void UnlockHero(string heroId, bool saveImmediately = true)
        {
            AddUniqueId(ref Data.unlockedHeroIds, heroId);
            if (saveImmediately)
                Save();
        }

        public static void UnlockWeapon(string weaponId, bool saveImmediately = true)
        {
            AddUniqueId(ref Data.unlockedWeaponIds, weaponId);
            if (saveImmediately)
                Save();
        }

        public static void UnlockLevel(string levelKey, bool saveImmediately = true)
        {
            if (string.IsNullOrEmpty(levelKey))
                return;

            AddUniqueId(ref Data.unlockedLevelKeys, levelKey);
            if (saveImmediately)
                Save();
        }

        public static void UnlockNextLevelAfterWin(int campaign, int completedLevel, bool saveImmediately = true)
        {
            UnlockLevel(CampaignKeys.Level(campaign, completedLevel), saveImmediately: false);

            int nextLevel = completedLevel + 1;
            if (nextLevel <= CampaignKeys.LevelsPerCampaign)
                UnlockLevel(CampaignKeys.Level(campaign, nextLevel), saveImmediately: false);
            else if (campaign < CampaignKeys.CampaignCount)
                UnlockLevel(CampaignKeys.Level(campaign + 1, 1), saveImmediately: false);

            if (saveImmediately)
                Save();
        }

        public static void SetSelectedHero(string heroId, bool saveImmediately = true)
        {
            if (Data == null)
                return;

            Data.selectedHeroId = heroId ?? string.Empty;
            if (saveImmediately)
                Save();
        }

        public static void SetLoadoutWeapon(int slot, string weaponId, bool saveImmediately = true)
        {
            if (Data == null)
                return;

            NormalizeData();
            if (slot < 0 || slot >= LoadoutSlotCount)
                return;

            Data.loadoutWeaponIds[slot] = weaponId ?? string.Empty;
            if (saveImmediately)
                Save();
        }

        public static string GetLoadoutWeaponId(int slot)
        {
            if (Data?.loadoutWeaponIds == null || slot < 0 || slot >= Data.loadoutWeaponIds.Length)
                return string.Empty;

            return Data.loadoutWeaponIds[slot] ?? string.Empty;
        }

        public static void ApplyWalletTo(PlayerWallet wallet)
        {
            if (wallet == null || Data == null)
                return;

            wallet.SetBalance(Mathf.Max(0, Data.wallet));
        }

        public static void CaptureWalletFrom(PlayerWallet wallet)
        {
            if (wallet == null || Data == null)
                return;

            Data.wallet = Mathf.Max(0, wallet.Balance);
        }

        private static ProfileSaveData CreateDefaultProfile()
        {
            if (startPack != null)
                return startPack.CreateNewProfile();

            return new ProfileSaveData
            {
                unlockedHeroIds = new[] { "hero_default" },
                unlockedWeaponIds = new[] { "pistol_01", "knife_01", "ak_01", "sniper_01", "rpg_raycast" },
                unlockedLevelKeys = new[] { CampaignKeys.Level(1, 1) },
                selectedHeroId = "hero_default",
                loadoutWeaponIds = new[] { "pistol_01", "knife_01", string.Empty }
            };
        }

        private static bool TryLoadFromDisk(int profileIndex)
        {
            string path = GetProfilePath(profileIndex);
            if (!File.Exists(path))
                return false;

            try
            {
                string json = File.ReadAllText(path);
                Data = string.IsNullOrWhiteSpace(json)
                    ? new ProfileSaveData()
                    : JsonUtility.FromJson<ProfileSaveData>(json) ?? new ProfileSaveData();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveProfileService] Failed to read {path}: {e.Message}");
                Data = new ProfileSaveData();
            }

            return true;
        }

        private static void NormalizeData()
        {
            if (Data == null)
                Data = new ProfileSaveData();

            Data.unlockedHeroIds ??= Array.Empty<string>();
            Data.unlockedWeaponIds ??= Array.Empty<string>();
            Data.unlockedLevelKeys ??= Array.Empty<string>();

            if (Data.loadoutWeaponIds == null || Data.loadoutWeaponIds.Length != LoadoutSlotCount)
            {
                var normalized = new string[LoadoutSlotCount];
                if (Data.loadoutWeaponIds != null)
                {
                    for (int i = 0; i < normalized.Length && i < Data.loadoutWeaponIds.Length; i++)
                        normalized[i] = Data.loadoutWeaponIds[i] ?? string.Empty;
                }

                Data.loadoutWeaponIds = normalized;
            }
        }

        private static bool ContainsId(string[] list, string id)
        {
            if (string.IsNullOrEmpty(id) || list == null)
                return false;

            for (int i = 0; i < list.Length; i++)
            {
                if (list[i] == id)
                    return true;
            }

            return false;
        }

        private static void AddUniqueId(ref string[] list, string id)
        {
            if (string.IsNullOrEmpty(id))
                return;

            if (Data == null)
                Data = new ProfileSaveData();

            NormalizeData();
            if (ContainsId(list, id))
                return;

            var merged = new List<string>(list ?? Array.Empty<string>()) { id };
            list = merged.ToArray();
        }

        private static string GetSaveDirectory()
        {
            string root = string.IsNullOrEmpty(saveRootOverride)
                ? Application.persistentDataPath
                : saveRootOverride;

            if (!Directory.Exists(root))
                Directory.CreateDirectory(root);

            return root;
        }

        private static int ClampProfileIndex(int profileIndex) =>
            Mathf.Clamp(profileIndex, MinProfileIndex, MaxProfileIndex);
    }
}
