using FPSGame.Core;
using FPSGame.Save;
using FPSGame.Weapons;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace FPSGame.Editor
{
    /// <summary>
    /// Phase 9: SaveStartPack, GameContentRegistry, SaveSystem in scene.
    /// Menu: FPSGame / Setup Phase 9 (Save System)
    /// </summary>
    public static class FPSGamePhase9Setup
    {
        private const string Root = "Assets/FPSGame";
        private const string StartPackPath = Root + "/Data/Save/StartPack_Default.asset";
        private const string RegistryPath = Root + "/Data/Save/GameContentRegistry.asset";
        private const string HeroPath = Root + "/Data/Characters/Hero_Default.asset";
        private const string EnemyPath = Root + "/Data/Characters/Enemy_Default.asset";
        private const string PistolPath = Root + "/Data/Weapons/pistol_hitscan.asset";
        private const string KnifePath = Root + "/Data/Weapons/knife_melee.asset";
        private const string ScenePath = Root + "/Scenes/PrototypeGameplay.unity";

        [MenuItem("FPSGame/Setup Phase 9 (Save System)", false, 90)]
        public static void RunPhase9Setup()
        {
            FPSGameProjectSetup.RunPhase0SetupSilent();

            var startPack = CreateOrLoadStartPack();
            var registry = CreateOrLoadRegistry();
            AddSaveSystemToScene(startPack, registry);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "FPSGame Phase 9",
                "Save system ready.\n\n" +
                "JSON: persistentDataPath/profile_1.json … profile_3.json\n" +
                "Debug: F1–F3 switch profile, F5 save.",
                "OK");

            Debug.Log("[FPSGame] Phase 9 setup complete.");
        }

        private static SaveStartPack CreateOrLoadStartPack()
        {
            var existing = AssetDatabase.LoadAssetAtPath<SaveStartPack>(StartPackPath);
            if (existing != null)
                return existing;

            var asset = ScriptableObject.CreateInstance<SaveStartPack>();
            AssetDatabase.CreateAsset(asset, StartPackPath);
            return asset;
        }

        private static GameContentRegistry CreateOrLoadRegistry()
        {
            var hero = AssetDatabase.LoadAssetAtPath<CharacterStats>(HeroPath);
            var enemy = AssetDatabase.LoadAssetAtPath<CharacterStats>(EnemyPath);
            var pistol = AssetDatabase.LoadAssetAtPath<HitscanWeaponData>(PistolPath);
            var knife = AssetDatabase.LoadAssetAtPath<MeleeWeaponData>(KnifePath);

            var existing = AssetDatabase.LoadAssetAtPath<GameContentRegistry>(RegistryPath);
            if (existing == null)
            {
                existing = ScriptableObject.CreateInstance<GameContentRegistry>();
                AssetDatabase.CreateAsset(existing, RegistryPath);
            }

            var so = new SerializedObject(existing);
            var heroes = so.FindProperty("heroes");
            heroes.arraySize = hero != null ? 1 : 0;
            if (hero != null)
                heroes.GetArrayElementAtIndex(0).objectReferenceValue = hero;

            var weapons = so.FindProperty("weapons");
            int weaponCount = (pistol != null ? 1 : 0) + (knife != null ? 1 : 0);
            weapons.arraySize = weaponCount;
            int w = 0;
            if (pistol != null)
                weapons.GetArrayElementAtIndex(w++).objectReferenceValue = pistol;
            if (knife != null)
                weapons.GetArrayElementAtIndex(w).objectReferenceValue = knife;

            so.ApplyModifiedPropertiesWithoutUndo();
            return existing;
        }

        private static void AddSaveSystemToScene(SaveStartPack startPack, GameContentRegistry registry)
        {
            if (!System.IO.File.Exists(ScenePath))
                return;

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var existing = GameObject.Find("SaveSystem");
            if (existing != null)
                Object.DestroyImmediate(existing);

            var go = new GameObject("SaveSystem");
            var bootstrap = go.AddComponent<GameplayProfileBootstrap>();
            var debug = go.AddComponent<SaveProfileDebug>();

            var bootstrapSo = new SerializedObject(bootstrap);
            bootstrapSo.FindProperty("startPack").objectReferenceValue = startPack;
            bootstrapSo.FindProperty("contentRegistry").objectReferenceValue = registry;
            bootstrapSo.FindProperty("profileIndexOnStart").intValue = 1;
            bootstrapSo.ApplyModifiedPropertiesWithoutUndo();

            var debugSo = new SerializedObject(debug);
            debugSo.FindProperty("bootstrap").objectReferenceValue = bootstrap;
            debugSo.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
    }
}
