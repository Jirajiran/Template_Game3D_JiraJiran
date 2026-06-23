using FPSGame.AI;
using FPSGame.Core;
using FPSGame.Weapons;
using UnityEditor;
using UnityEditor.AI;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

namespace FPSGame.Editor
{
    /// <summary>
    /// Phase 7: Enemy AI prefab, patrol points, NavMesh bake, player noise emitter.
    /// Menu: FPSGame / Setup Phase 7 (AI Enemy)
    /// </summary>
    public static class FPSGamePhase7Setup
    {
        private const string Root = "Assets/FPSGame";
        private const string PerceptionPath = Root + "/Data/AI/EnemyPerception_Default.asset";
        private const string EnemyPrefabPath = Root + "/Prefabs/Enemy/Enemy_AI.prefab";
        private const string EnemyStatsPath = Root + "/Data/Characters/Enemy_Default.asset";
        private const string PistolPath = Root + "/Data/Weapons/pistol_projectile.asset";
        private const string KnifePath = Root + "/Data/Weapons/knife_melee.asset";
        private const string PlayerPrefabPath = Root + "/Prefabs/Player/Player.prefab";
        private const string ScenePath = Root + "/Scenes/PrototypeGameplay.unity";

        [MenuItem("FPSGame/Setup Phase 7 (AI Enemy)", false, 70)]
        public static void RunPhase7Setup()
        {
            FPSGameProjectSetup.RunPhase0SetupSilent();

            var perception = CreateOrLoadPerceptionConfig();
            var enemyStats = AssetDatabase.LoadAssetAtPath<CharacterStats>(EnemyStatsPath);
            var pistol = AssetDatabase.LoadAssetAtPath<ProjectileWeaponData>(PistolPath);
            var knife = AssetDatabase.LoadAssetAtPath<MeleeWeaponData>(KnifePath);

            if (enemyStats == null || pistol == null || knife == null)
            {
                EditorUtility.DisplayDialog(
                    "FPSGame Phase 7",
                    "Missing Phase 3 assets.\nRun Phase 3 (Weapons) first.",
                    "OK");
                return;
            }

            AddPlayerNoiseEmitter();
            var enemyPrefab = CreateOrLoadEnemyPrefab(enemyStats, perception, pistol, knife);
            RewireEnemyPrefabWeapons(enemyPrefab, pistol, knife);
            SetupScene(enemyPrefab, perception);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "FPSGame Phase 7",
                "Enemy AI ready.\n\n" +
                "NavMesh baked. Enemy_AI @ (8,0,8) with patrol points.\n" +
                "Sprint or shoot to trigger hearing.",
                "OK");

            Debug.Log("[FPSGame] Phase 7 setup complete.");
        }

        private static AiPerceptionConfig CreateOrLoadPerceptionConfig()
        {
            var existing = AssetDatabase.LoadAssetAtPath<AiPerceptionConfig>(PerceptionPath);
            if (existing != null)
                return existing;

            var asset = ScriptableObject.CreateInstance<AiPerceptionConfig>();
            AssetDatabase.CreateAsset(asset, PerceptionPath);
            return asset;
        }

        private static void AddPlayerNoiseEmitter()
        {
            if (!System.IO.File.Exists(PlayerPrefabPath))
                return;

            var root = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            if (root.GetComponent<PlayerNoiseEmitter>() == null)
                root.AddComponent<PlayerNoiseEmitter>();

            PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
            PrefabUtility.UnloadPrefabContents(root);
        }

        private static GameObject CreateOrLoadEnemyPrefab(
            CharacterStats enemyStats,
            AiPerceptionConfig perception,
            ProjectileWeaponData pistol,
            MeleeWeaponData knife)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(EnemyPrefabPath);
            if (existing != null)
                return existing;

            int enemyLayer = LayerMask.NameToLayer("Enemy");
            if (enemyLayer < 0)
                enemyLayer = 0;

            var enemy = new GameObject("Enemy_AI");
            enemy.tag = "Enemy";
            enemy.layer = enemyLayer;

            var character = enemy.AddComponent<CharacterBase>();
            var charSo = new SerializedObject(character);
            charSo.FindProperty("stats").objectReferenceValue = enemyStats;
            charSo.ApplyModifiedPropertiesWithoutUndo();

            var capsule = enemy.AddComponent<CapsuleCollider>();
            capsule.height = 2f;
            capsule.radius = 0.4f;
            capsule.center = new Vector3(0f, 1f, 0f);

            var agent = enemy.AddComponent<NavMeshAgent>();
            agent.height = 2f;
            agent.radius = 0.4f;
            agent.speed = enemyStats.walkSpeed;
            agent.stoppingDistance = 1.5f;
            agent.angularSpeed = 360f;
            agent.acceleration = 12f;

            var perceptionComponent = enemy.AddComponent<AiPerception>();
            var perceptionSo = new SerializedObject(perceptionComponent);
            perceptionSo.FindProperty("config").objectReferenceValue = perception;
            perceptionSo.ApplyModifiedPropertiesWithoutUndo();

            enemy.AddComponent<AIController>();

            var weaponMount = new GameObject("WeaponMount");
            weaponMount.transform.SetParent(enemy.transform, false);
            weaponMount.transform.localPosition = new Vector3(0f, 1.4f, 0.4f);
            var weaponHandler = weaponMount.AddComponent<WeaponHandler>();

            var weaponSo = new SerializedObject(weaponHandler);
            weaponSo.FindProperty("muzzle").objectReferenceValue = weaponMount.transform;
            var slots = weaponSo.FindProperty("weaponSlots");
            slots.arraySize = 3;
            slots.GetArrayElementAtIndex(0).objectReferenceValue = pistol;
            slots.GetArrayElementAtIndex(1).objectReferenceValue = knife;
            weaponSo.ApplyModifiedPropertiesWithoutUndo();

            var prefab = PrefabUtility.SaveAsPrefabAsset(enemy, EnemyPrefabPath);
            Object.DestroyImmediate(enemy);
            return prefab;
        }

        private static void RewireEnemyPrefabWeapons(
            GameObject enemyPrefab,
            ProjectileWeaponData pistol,
            MeleeWeaponData knife)
        {
            if (enemyPrefab == null || pistol == null || knife == null)
                return;

            var path = AssetDatabase.GetAssetPath(enemyPrefab);
            var root = PrefabUtility.LoadPrefabContents(path);
            var weaponHandler = root.GetComponentInChildren<WeaponHandler>();
            if (weaponHandler != null)
            {
                var weaponSo = new SerializedObject(weaponHandler);
                var slots = weaponSo.FindProperty("weaponSlots");
                slots.arraySize = 3;
                slots.GetArrayElementAtIndex(0).objectReferenceValue = pistol;
                slots.GetArrayElementAtIndex(1).objectReferenceValue = knife;
                weaponSo.ApplyModifiedPropertiesWithoutUndo();
            }

            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
        }

        private static void SetupScene(GameObject enemyPrefab, AiPerceptionConfig perception)
        {
            if (!System.IO.File.Exists(ScenePath))
                return;

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            RemoveOldEnemies();
            var patrolRoot = GetOrCreatePatrolRoot();
            var pointA = GetOrCreatePatrolPoint(patrolRoot.transform, "PatrolPoint_A", new Vector3(6f, 0f, 6f));
            var pointB = GetOrCreatePatrolPoint(patrolRoot.transform, "PatrolPoint_B", new Vector3(10f, 0f, 10f));

            var enemy = (GameObject)PrefabUtility.InstantiatePrefab(enemyPrefab, scene);
            enemy.transform.position = new Vector3(8f, 0f, 8f);
            enemy.name = "Enemy_AI";

            var controller = enemy.GetComponent<AIController>();
            var aiSo = new SerializedObject(controller);
            var patrolArray = aiSo.FindProperty("patrolPoints");
            patrolArray.arraySize = 2;
            patrolArray.GetArrayElementAtIndex(0).objectReferenceValue = pointA.transform;
            patrolArray.GetArrayElementAtIndex(1).objectReferenceValue = pointB.transform;
            aiSo.ApplyModifiedPropertiesWithoutUndo();

            var perceptionComp = enemy.GetComponent<AiPerception>();
            var perceptionSo = new SerializedObject(perceptionComp);
            perceptionSo.FindProperty("config").objectReferenceValue = perception;
            perceptionSo.ApplyModifiedPropertiesWithoutUndo();

            NavMeshBuilder.BuildNavMesh();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void RemoveOldEnemies()
        {
            var dummy = GameObject.Find("TestDummy_Enemy");
            if (dummy != null)
                Object.DestroyImmediate(dummy);

            var existing = GameObject.Find("Enemy_AI");
            if (existing != null)
                Object.DestroyImmediate(existing);
        }

        private static GameObject GetOrCreatePatrolRoot()
        {
            var root = GameObject.Find("PatrolPoints");
            if (root != null)
                return root;

            return new GameObject("PatrolPoints");
        }

        private static GameObject GetOrCreatePatrolPoint(Transform parent, string name, Vector3 position)
        {
            var existing = parent.Find(name);
            if (existing != null)
            {
                existing.position = position;
                return existing.gameObject;
            }

            var point = new GameObject(name);
            point.transform.SetParent(parent, false);
            point.transform.position = position;
            return point;
        }
    }
}
