using FPSGame.Core;
using FPSGame.Weapons;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace FPSGame.Editor
{
    /// <summary>
    /// Phase 1: Hero_Default.asset, Player prefab, optional scene placement.
    /// Menu: FPSGame / Setup Phase 1 (Character + Player Prefab)
    /// </summary>
    public static class FPSGamePhase1Setup
    {
        private const string Root = "Assets/FPSGame";
        private const string HeroAssetPath = Root + "/Data/Characters/Hero_Default.asset";
        private const string PlayerPrefabPath = Root + "/Prefabs/Player/Player.prefab";
        private const string ScenePath = Root + "/Scenes/PrototypeGameplay.unity";

        [MenuItem("FPSGame/Setup Phase 1 (Character + Player Prefab)", false, 10)]
        public static void RunPhase1Setup()
        {
            if (!EditorUtility.DisplayDialog(
                    "FPSGame Phase 1",
                    "Creates Hero_Default.asset and Player.prefab.\n\n" +
                    "Requires Phase 0 folders. Run Phase 0 first if missing.",
                    "Run Setup",
                    "Cancel"))
            {
                return;
            }

            FPSGameProjectSetup.RunPhase0SetupSilent();
            FPSGameProjectSetup.CreatePrototypeSceneIfMissing();

            var heroStats = CreateOrLoadHeroDefault();
            var prefab = CreateOrUpdatePlayerPrefab(heroStats);
            PlacePlayerInPrototypeScene(prefab);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "FPSGame Phase 1",
                "Phase 1 complete.\n\n" +
                $"Hero: {HeroAssetPath}\n" +
                $"Prefab: {PlayerPrefabPath}\n\n" +
                "Play mode test:\n" +
                "K = damage 10 | H = heal 10 | Y = revive when dead",
                "OK");

            Debug.Log("[FPSGame] Phase 1 setup complete.");
        }

        private static CharacterStats CreateOrLoadHeroDefault()
        {
            var existing = AssetDatabase.LoadAssetAtPath<CharacterStats>(HeroAssetPath);
            if (existing != null)
            {
                return existing;
            }

            var asset = ScriptableObject.CreateInstance<CharacterStats>();
            asset.characterId = "hero_default";
            asset.maxHealth = 100f;
            asset.walkSpeed = 5f;
            asset.sprintSpeed = 8f;
            asset.jumpHeight = 1.5f;
            asset.jumpLimit = 1;
            asset.turnSpeed = 10f;
            asset.gravity = -20f;
            asset.faction = Faction.Friendly;

            AssetDatabase.CreateAsset(asset, HeroAssetPath);
            Debug.Log("[FPSGame] Created " + HeroAssetPath);
            return asset;
        }

        private static GameObject CreateOrUpdatePlayerPrefab(CharacterStats heroStats)
        {
            var root = BuildPlayerHierarchy(heroStats);

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
            Object.DestroyImmediate(root);

            Debug.Log("[FPSGame] Saved " + PlayerPrefabPath);
            return prefab;
        }

        private static GameObject BuildPlayerHierarchy(CharacterStats heroStats)
        {
            var playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer < 0)
            {
                playerLayer = 0;
            }

            var player = new GameObject("Player");
            player.tag = "Player";
            player.layer = playerLayer;

            var character = player.AddComponent<CharacterBase>();
            AssignStats(character, heroStats);

            player.AddComponent<CharacterDamageTester>();

            var cameraGo = new GameObject("Camera");
            cameraGo.transform.SetParent(player.transform, false);
            cameraGo.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            cameraGo.tag = "MainCamera";
            cameraGo.AddComponent<Camera>();

            var weaponMount = new GameObject("WeaponMount");
            weaponMount.transform.SetParent(player.transform, false);
            weaponMount.transform.localPosition = new Vector3(0f, 1.4f, 0.4f);
            weaponMount.AddComponent<WeaponHandler>();

            FPSGamePhase2Setup.UpgradePlayerPrefab(player);

            return player;
        }

        private static void AssignStats(CharacterBase character, CharacterStats heroStats)
        {
            var serialized = new SerializedObject(character);
            serialized.FindProperty("stats").objectReferenceValue = heroStats;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void PlacePlayerInPrototypeScene(GameObject prefab)
        {
            if (prefab == null || !System.IO.File.Exists(ScenePath))
            {
                return;
            }

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            RemoveExistingPlayers();

            var spawn = GameObject.Find("PlayerSpawn");
            var position = spawn != null ? spawn.transform.position : new Vector3(0f, 1f, 0f);
            var rotation = spawn != null ? spawn.transform.rotation : Quaternion.identity;

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            instance.transform.SetPositionAndRotation(position, rotation);
            instance.name = "Player";

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void RemoveExistingPlayers()
        {
            var existing = GameObject.FindGameObjectsWithTag("Player");
            foreach (var go in existing)
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
