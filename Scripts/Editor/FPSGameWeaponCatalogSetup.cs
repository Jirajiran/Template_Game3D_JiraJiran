using FPSGame.Save;
using FPSGame.Weapons;
using UnityEditor;
using UnityEngine;

namespace FPSGame.Editor
{
    /// <summary>
    /// Shared weapon asset paths, creation, GameContentRegistry and StartPack wiring.
    /// </summary>
    public static class FPSGameWeaponCatalogSetup
    {
        public const string PistolPath = "Assets/FPSGame/Data/Weapons/pistol_projectile.asset";
        public const string AkPath = "Assets/FPSGame/Data/Weapons/ak_projectile.asset";
        public const string SniperPath = "Assets/FPSGame/Data/Weapons/sniper_hitscan.asset";
        public const string RpgPath = "Assets/FPSGame/Data/Weapons/rpg_raycast.asset";
        public const string KnifePath = "Assets/FPSGame/Data/Weapons/knife_melee.asset";
        public const string LegacyPistolHitscanPath = "Assets/FPSGame/Data/Weapons/pistol_hitscan.asset";
        public const string StartPackPath = "Assets/FPSGame/Data/Save/StartPack_Default.asset";
        public const string RegistryPath = "Assets/FPSGame/Data/Save/GameContentRegistry.asset";

        public static HitscanWeaponData EnsureSniper(GameObject bulletHole)
        {
            var existing = AssetDatabase.LoadAssetAtPath<HitscanWeaponData>(SniperPath);
            if (existing == null)
            {
                existing = ScriptableObject.CreateInstance<HitscanWeaponData>();
                AssetDatabase.CreateAsset(existing, SniperPath);
            }

            existing.weaponId = "sniper_01";
            existing.displayName = "Sniper";
            existing.category = WeaponCategory.Hitscan;
            existing.damage = 85f;
            existing.attackCooldown = 1.2f;
            existing.clipSize = 5;
            existing.magazineCount = 3;
            existing.spreadDegrees = 0.25f;
            existing.reloadPhaseDuration = 0.25f;
            existing.baseHitDelay = 0.01f;
            existing.extraDelayPer100m = 0.01f;
            existing.referenceRangeMeters = 100f;
            existing.maxRange = 500f;
            existing.penetration = 0;
            existing.bulletHolePrefab = bulletHole;

            EditorUtility.SetDirty(existing);
            return existing;
        }

        public static ProjectileWeaponData EnsureAk(GameObject bulletPrefab)
        {
            var existing = AssetDatabase.LoadAssetAtPath<ProjectileWeaponData>(AkPath);
            if (existing == null)
            {
                existing = ScriptableObject.CreateInstance<ProjectileWeaponData>();
                AssetDatabase.CreateAsset(existing, AkPath);
            }

            existing.weaponId = "ak_01";
            existing.displayName = "AK";
            existing.category = WeaponCategory.Projectile;
            existing.damage = 25f;
            existing.attackCooldown = 0.1f;
            existing.clipSize = 30;
            existing.magazineCount = 3;
            existing.spreadDegrees = 2f;
            existing.reloadPhaseDuration = 0.2f;
            existing.projectilePrefab = bulletPrefab;
            existing.projectileSpeed = 90f;
            existing.projectileGravity = 0f;
            existing.projectileMaxLifetime = 3f;
            existing.explosionRadius = 0f;

            EditorUtility.SetDirty(existing);
            return existing;
        }

        public static ProjectileWeaponData EnsureRpg(GameObject bulletPrefab)
        {
            var existing = AssetDatabase.LoadAssetAtPath<ProjectileWeaponData>(RpgPath);
            if (existing == null)
            {
                existing = ScriptableObject.CreateInstance<ProjectileWeaponData>();
                AssetDatabase.CreateAsset(existing, RpgPath);
            }

            existing.weaponId = "rpg_raycast";
            existing.displayName = "RPG";
            existing.category = WeaponCategory.Projectile;
            existing.damage = 45f;
            existing.attackCooldown = 0.8f;
            existing.clipSize = 1;
            existing.magazineCount = 3;
            existing.spreadDegrees = 0.5f;
            existing.projectilePrefab = bulletPrefab;
            existing.projectileSpeed = 80f;
            existing.projectileGravity = 9.81f;
            existing.projectileMaxLifetime = 5f;
            existing.explosionRadius = 0f;

            EditorUtility.SetDirty(existing);
            return existing;
        }

        public static void RemoveLegacyPistolHitscanAsset()
        {
            if (AssetDatabase.LoadAssetAtPath<WeaponData>(LegacyPistolHitscanPath) == null)
                return;

            if (AssetDatabase.DeleteAsset(LegacyPistolHitscanPath))
                Debug.Log("[FPSGame] Removed legacy pistol_hitscan.asset");
        }

        public static void ApplyStartPackDefaults(SaveStartPack pack)
        {
            if (pack == null)
                return;

            pack.starterWeaponIds = new[] { "pistol_01", "knife_01", "ak_01", "sniper_01", "rpg_raycast" };
            pack.defaultLoadoutWeaponIds = new[] { "pistol_01", "knife_01", string.Empty };
            EditorUtility.SetDirty(pack);
        }

        public static void ApplyRegistryWeapons(GameContentRegistry registry)
        {
            if (registry == null)
                return;

            var pistol = AssetDatabase.LoadAssetAtPath<WeaponData>(PistolPath);
            var ak = AssetDatabase.LoadAssetAtPath<WeaponData>(AkPath);
            var sniper = AssetDatabase.LoadAssetAtPath<WeaponData>(SniperPath);
            var rpg = AssetDatabase.LoadAssetAtPath<WeaponData>(RpgPath);
            var knife = AssetDatabase.LoadAssetAtPath<WeaponData>(KnifePath);

            var list = new System.Collections.Generic.List<WeaponData>();
            if (pistol != null) list.Add(pistol);
            if (ak != null) list.Add(ak);
            if (sniper != null) list.Add(sniper);
            if (knife != null) list.Add(knife);
            if (rpg != null) list.Add(rpg);

            var so = new SerializedObject(registry);
            var weapons = so.FindProperty("weapons");
            weapons.arraySize = list.Count;
            for (int i = 0; i < list.Count; i++)
                weapons.GetArrayElementAtIndex(i).objectReferenceValue = list[i];

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(registry);
        }
    }
}
