using FPSGame.Weapons;
using UnityEditor;
using UnityEngine;

namespace FPSGame.Editor
{
    /// <summary>
    /// RaycastProjectile prefab helper. RPG weapon asset is created by Phase 3 / FPSGameWeaponCatalogSetup.
    /// </summary>
    public static class FPSGameProjectileSetup
    {
        public const string RaycastBulletPrefabPath = "Assets/FPSGame/Prefabs/Projectiles/RaycastBullet.prefab";

        /// <summary>Default bullet prefab used by pistol, AK, and RPG.</summary>
        public static GameObject CreateOrLoadRaycastBulletPrefab(GameObject impactEffectPrefab = null)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(RaycastBulletPrefabPath);
            if (existing != null)
            {
                if (impactEffectPrefab != null)
                    SetImpactEffect(existing, impactEffectPrefab);

                return existing;
            }

            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "RaycastBullet";
            go.transform.localScale = Vector3.one * 0.08f;

            var col = go.GetComponent<Collider>();
            if (col != null)
                Object.DestroyImmediate(col);

            var projectile = go.AddComponent<RaycastProjectile>();
            if (impactEffectPrefab != null)
                SetImpactEffectOnComponent(projectile, impactEffectPrefab);

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, RaycastBulletPrefabPath);
            Object.DestroyImmediate(go);
            return prefab;
        }

        [MenuItem("FPSGame/Refresh Weapon Catalog Assets", false, 49)]
        public static void RefreshWeaponCatalog()
        {
            FPSGameProjectSetup.RunPhase0SetupSilent();

            var bulletHole = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/FPSGame/Prefabs/Weapons/bullet_hole_placeholder.prefab");
            var bulletPrefab = CreateOrLoadRaycastBulletPrefab(bulletHole);

            FPSGameWeaponCatalogSetup.EnsureSniper(bulletHole);
            FPSGameWeaponCatalogSetup.EnsureAk(bulletPrefab);
            FPSGameWeaponCatalogSetup.EnsureRpg(bulletPrefab);
            FPSGameWeaponCatalogSetup.RemoveLegacyPistolHitscanAsset();

            var registry = AssetDatabase.LoadAssetAtPath<Save.GameContentRegistry>(
                FPSGameWeaponCatalogSetup.RegistryPath);
            if (registry != null)
                FPSGameWeaponCatalogSetup.ApplyRegistryWeapons(registry);

            var startPack = AssetDatabase.LoadAssetAtPath<Save.SaveStartPack>(
                FPSGameWeaponCatalogSetup.StartPackPath);
            if (startPack != null)
                FPSGameWeaponCatalogSetup.ApplyStartPackDefaults(startPack);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "FPSGame",
                "Weapon catalog refreshed (AK, Sniper, RPG).\nRe-run Phase 9 if SaveSystem not set up yet.",
                "OK");
        }

        private static void SetImpactEffect(GameObject prefabRoot, GameObject impactEffectPrefab)
        {
            var projectile = prefabRoot.GetComponent<RaycastProjectile>();
            if (projectile == null)
                return;

            SetImpactEffectOnComponent(projectile, impactEffectPrefab);
            EditorUtility.SetDirty(prefabRoot);
        }

        private static void SetImpactEffectOnComponent(RaycastProjectile projectile, GameObject impactEffectPrefab)
        {
            var so = new SerializedObject(projectile);
            so.FindProperty("impactEffectPrefab").objectReferenceValue = impactEffectPrefab;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
