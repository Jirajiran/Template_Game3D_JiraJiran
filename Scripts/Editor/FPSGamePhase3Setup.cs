using FPSGame.Core;
using FPSGame.Weapons;
using UnityEditor;
using UnityEngine;

namespace FPSGame.Editor
{
    /// <summary>
    /// Phase 3: weapon ScriptableObjects, bullet hole placeholder, test dummy, prefab wiring.
    /// Menu: FPSGame / Setup Phase 3 (Weapons)
    /// </summary>
    public static class FPSGamePhase3Setup
    {
        private const string Root = "Assets/FPSGame";
        private const string PistolPath = Root + "/Data/Weapons/pistol_hitscan.asset";
        private const string KnifePath = Root + "/Data/Weapons/knife_melee.asset";
        private const string EnemyStatsPath = Root + "/Data/Characters/Enemy_Default.asset";
        private const string BulletHolePath = Root + "/Prefabs/Weapons/bullet_hole_placeholder.prefab";
        private const string TestDummyPath = Root + "/Prefabs/Player/TestDummy_Enemy.prefab";
        private const string PlayerPrefabPath = Root + "/Prefabs/Player/Player.prefab";
        private const string ScenePath = Root + "/Scenes/PrototypeGameplay.unity";

        [MenuItem("FPSGame/Setup Phase 3 (Weapons)", false, 30)]
        public static void RunPhase3Setup()
        {
            FPSGameProjectSetup.RunPhase0SetupSilent();

            var bulletHole = CreateOrLoadBulletHolePrefab();
            var pistol = CreateOrLoadPistol(bulletHole);
            var knife = CreateOrLoadKnife();
            var enemyStats = CreateOrLoadEnemyStats();
            var testDummy = CreateOrLoadTestDummy(enemyStats);

            WirePlayerWeapons(pistol, knife);
            PlaceTestDummyInScene(testDummy);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "FPSGame Phase 3",
                "Weapons ready.\n\n" +
                "LMB hitscan | R reload (3 phases) | melee slot 2\n" +
                "Test dummy placed at (5, 1, 0) for damage tests.",
                "OK");

            Debug.Log("[FPSGame] Phase 3 setup complete.");
        }

        private static HitscanWeaponData CreateOrLoadPistol(GameObject bulletHole)
        {
            var existing = AssetDatabase.LoadAssetAtPath<HitscanWeaponData>(PistolPath);
            if (existing != null)
                return existing;

            var asset = ScriptableObject.CreateInstance<HitscanWeaponData>();
            asset.weaponId = "pistol_01";
            asset.displayName = "Pistol";
            asset.category = WeaponCategory.Hitscan;
            asset.damage = 25f;
            asset.attackCooldown = 0.25f;
            asset.clipSize = 12;
            asset.magazineCount = 3;
            asset.spreadDegrees = 1.5f;
            asset.reloadPhaseDuration = 0.2f;
            asset.baseHitDelay = 0.01f;
            asset.extraDelayPer100m = 0.01f;
            asset.referenceRangeMeters = 100f;
            asset.maxRange = 200f;
            asset.penetration = 0;
            asset.bulletHolePrefab = bulletHole;

            AssetDatabase.CreateAsset(asset, PistolPath);
            return asset;
        }

        private static MeleeWeaponData CreateOrLoadKnife()
        {
            var existing = AssetDatabase.LoadAssetAtPath<MeleeWeaponData>(KnifePath);
            if (existing != null)
                return existing;

            var asset = ScriptableObject.CreateInstance<MeleeWeaponData>();
            asset.weaponId = "knife_01";
            asset.displayName = "Knife";
            asset.category = WeaponCategory.Melee;
            asset.damage = 50f;
            asset.attackCooldown = 0.6f;
            asset.activeTime = 0.01f;
            asset.hitboxSize = new Vector3(0.8f, 0.8f, 1.2f);
            asset.hitboxOffset = new Vector3(0f, 0f, 0.6f);

            AssetDatabase.CreateAsset(asset, KnifePath);
            return asset;
        }

        private static CharacterStats CreateOrLoadEnemyStats()
        {
            var existing = AssetDatabase.LoadAssetAtPath<CharacterStats>(EnemyStatsPath);
            if (existing != null)
                return existing;

            var asset = ScriptableObject.CreateInstance<CharacterStats>();
            asset.characterId = "enemy_default";
            asset.maxHealth = 100f;
            asset.faction = Faction.Enemy;

            AssetDatabase.CreateAsset(asset, EnemyStatsPath);
            return asset;
        }

        private static GameObject CreateOrLoadBulletHolePrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(BulletHolePath);
            if (existing != null)
                return existing;

            var hole = GameObject.CreatePrimitive(PrimitiveType.Quad);
            hole.name = "bullet_hole_placeholder";
            Object.DestroyImmediate(hole.GetComponent<Collider>());
            hole.transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);

            var prefab = PrefabUtility.SaveAsPrefabAsset(hole, BulletHolePath);
            Object.DestroyImmediate(hole);
            return prefab;
        }

        private static GameObject CreateOrLoadTestDummy(CharacterStats enemyStats)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(TestDummyPath);
            if (existing != null)
                return existing;

            var enemyLayer = LayerMask.NameToLayer("Enemy");
            if (enemyLayer < 0)
                enemyLayer = 0;

            var dummy = new GameObject("TestDummy_Enemy");
            dummy.tag = "Enemy";
            dummy.layer = enemyLayer;

            var character = dummy.AddComponent<CharacterBase>();
            var so = new SerializedObject(character);
            so.FindProperty("stats").objectReferenceValue = enemyStats;
            so.ApplyModifiedPropertiesWithoutUndo();

            var capsule = dummy.AddComponent<CapsuleCollider>();
            capsule.height = 2f;
            capsule.radius = 0.4f;
            capsule.center = new Vector3(0f, 1f, 0f);

            var prefab = PrefabUtility.SaveAsPrefabAsset(dummy, TestDummyPath);
            Object.DestroyImmediate(dummy);
            return prefab;
        }

        private static void WirePlayerWeapons(HitscanWeaponData pistol, MeleeWeaponData knife)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (prefab == null)
            {
                Debug.LogWarning("[FPSGame] Player.prefab missing — run Phase 1 first.");
                return;
            }

            var root = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            var weaponHandler = root.GetComponentInChildren<WeaponHandler>();
            if (weaponHandler != null)
            {
                var weaponSo = new SerializedObject(weaponHandler);
                var slots = weaponSo.FindProperty("weaponSlots");
                slots.arraySize = 3;
                slots.GetArrayElementAtIndex(0).objectReferenceValue = pistol;
                slots.GetArrayElementAtIndex(1).objectReferenceValue = knife;
                slots.GetArrayElementAtIndex(2).objectReferenceValue = null;
                weaponSo.ApplyModifiedPropertiesWithoutUndo();
            }

            PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
            PrefabUtility.UnloadPrefabContents(root);
        }

        private static void PlaceTestDummyInScene(GameObject testDummyPrefab)
        {
            if (testDummyPrefab == null || !System.IO.File.Exists(ScenePath))
                return;

            var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(ScenePath);
            var existing = GameObject.Find("TestDummy_Enemy");
            if (existing != null)
                Object.DestroyImmediate(existing);

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(testDummyPrefab, scene);
            instance.transform.position = new Vector3(5f, 1f, 0f);
            instance.name = "TestDummy_Enemy";

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
        }
    }
}
