using FPSGame.AI;
using UnityEditor;
using UnityEditor.AI;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace FPSGame.Editor
{
    /// <summary>
    /// Phase 8: wave sequence SO, spawn point, trigger zone. Scene starts with no active enemies.
    /// Menu: FPSGame / Setup Phase 8 (Spawn System)
    /// </summary>
    public static class FPSGamePhase8Setup
    {
        private const string Root = "Assets/FPSGame";
        private const string WavePath = Root + "/Data/AI/Wave_Prototype_01.asset";
        private const string EnemyPrefabPath = Root + "/Prefabs/Enemy/Enemy_AI.prefab";
        private const string SpawnPointPrefabPath = Root + "/Prefabs/Enemy/EnemySpawnPoint.prefab";
        private const string ScenePath = Root + "/Scenes/PrototypeGameplay.unity";

        [MenuItem("FPSGame/Setup Phase 8 (Spawn System)", false, 80)]
        public static void RunPhase8Setup()
        {
            FPSGameProjectSetup.RunPhase0SetupSilent();

            var enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(EnemyPrefabPath);
            if (enemyPrefab == null)
            {
                EditorUtility.DisplayDialog(
                    "FPSGame Phase 8",
                    "Enemy_AI.prefab not found.\nRun Phase 7 first.",
                    "OK");
                return;
            }

            var wave = CreateOrLoadWaveSequence(enemyPrefab);
            CreateOrLoadSpawnPointPrefab();
            SetupScene(wave);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "FPSGame Phase 8",
                "Spawn system ready.\n\n" +
                "Scene has no enemies until player enters SpawnTrigger_01.\n" +
                "Walk toward the orange zone past spawn.",
                "OK");

            Debug.Log("[FPSGame] Phase 8 setup complete.");
        }

        private static EnemyWaveSequence CreateOrLoadWaveSequence(GameObject enemyPrefab)
        {
            var existing = AssetDatabase.LoadAssetAtPath<EnemyWaveSequence>(WavePath);
            if (existing != null)
                return existing;

            var wave = ScriptableObject.CreateInstance<EnemyWaveSequence>();
            wave.runOncePerSpawnPoint = true;
            wave.steps = new[]
            {
                new EnemySpawnStep
                {
                    enemyPrefab = enemyPrefab,
                    count = 1,
                    delayAfterPrevious = 0f
                },
                new EnemySpawnStep
                {
                    enemyPrefab = enemyPrefab,
                    count = 1,
                    delayAfterPrevious = 2f
                }
            };

            AssetDatabase.CreateAsset(wave, WavePath);
            return wave;
        }

        private static void CreateOrLoadSpawnPointPrefab()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(SpawnPointPrefabPath) != null)
                return;

            var root = new GameObject("EnemySpawnPoint");
            root.AddComponent<EnemySpawnPoint>();

            CreatePatrolChild(root.transform, "PatrolPoint_A", new Vector3(-2f, 0f, 0f));
            CreatePatrolChild(root.transform, "PatrolPoint_B", new Vector3(2f, 0f, 2f));

            PrefabUtility.SaveAsPrefabAsset(root, SpawnPointPrefabPath);
            Object.DestroyImmediate(root);
        }

        private static void SetupScene(EnemyWaveSequence wave)
        {
            if (!System.IO.File.Exists(ScenePath))
                return;

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            RemoveLegacySceneEnemies();

            var spawnPointPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SpawnPointPrefabPath);
            var spawnPointGo = (GameObject)PrefabUtility.InstantiatePrefab(spawnPointPrefab, scene);
            spawnPointGo.transform.position = new Vector3(8f, 0f, 8f);
            spawnPointGo.name = "SpawnPoint_01";

            var spawnPoint = spawnPointGo.GetComponent<EnemySpawnPoint>();
            var spawnSo = new SerializedObject(spawnPoint);
            spawnSo.FindProperty("sequence").objectReferenceValue = wave;
            spawnSo.ApplyModifiedPropertiesWithoutUndo();

            var triggerGo = GameObject.Find("SpawnTrigger_01");
            if (triggerGo == null)
            {
                triggerGo = new GameObject("SpawnTrigger_01");
                triggerGo.transform.position = new Vector3(4f, 1f, 4f);
            }

            var box = triggerGo.GetComponent<BoxCollider>();
            if (box == null)
                box = triggerGo.AddComponent<BoxCollider>();

            box.isTrigger = true;
            box.size = new Vector3(8f, 3f, 8f);
            box.center = Vector3.zero;

            var trigger = triggerGo.GetComponent<EnemySpawnTrigger>();
            if (trigger == null)
                trigger = triggerGo.AddComponent<EnemySpawnTrigger>();

            var triggerSo = new SerializedObject(trigger);
            var points = triggerSo.FindProperty("spawnPoints");
            points.arraySize = 1;
            points.GetArrayElementAtIndex(0).objectReferenceValue = spawnPoint;
            triggerSo.ApplyModifiedPropertiesWithoutUndo();

            NavMeshBuilder.BuildNavMesh();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void RemoveLegacySceneEnemies()
        {
            var enemy = GameObject.Find("Enemy_AI");
            if (enemy != null)
                Object.DestroyImmediate(enemy);

            var dummy = GameObject.Find("TestDummy_Enemy");
            if (dummy != null)
                Object.DestroyImmediate(dummy);

            var patrolRoot = GameObject.Find("PatrolPoints");
            if (patrolRoot != null)
                Object.DestroyImmediate(patrolRoot);
        }

        private static void CreatePatrolChild(Transform parent, string name, Vector3 localPos)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent, false);
            child.transform.localPosition = localPos;
        }
    }
}
