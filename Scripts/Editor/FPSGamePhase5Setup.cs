using FPSGame.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace FPSGame.Editor
{
    /// <summary>
    /// Phase 5: Pause menu UI + Build Settings for scene flow.
    /// Menu: FPSGame / Setup Phase 5 (Pause)
    /// </summary>
    public static class FPSGamePhase5Setup
    {
        private const string ScenePath = "Assets/FPSGame/Scenes/PrototypeGameplay.unity";
        private const string PausePrefabPath = "Assets/FPSGame/UI/HUD/PauseMenu.prefab";

        [MenuItem("FPSGame/Setup Phase 5 (Pause)", false, 50)]
        public static void RunPhase5Setup()
        {
            FPSGameProjectSetup.RunPhase0SetupSilent();

            var pausePrefab = CreateOrLoadPausePrefab();
            AddPauseToScene(pausePrefab);
            EnsurePrototypeSceneInBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "FPSGame Phase 5",
                "Pause menu ready.\n\n" +
                "Escape or Pause button → Resume / Restart / Back to Menu\n" +
                "PrototypeGameplay added to Build Settings.",
                "OK");

            Debug.Log("[FPSGame] Phase 5 setup complete.");
        }

        private static GameObject CreateOrLoadPausePrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(PausePrefabPath);
            if (existing != null)
                return existing;

            var root = BuildPauseHierarchy();
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PausePrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static void AddPauseToScene(GameObject pausePrefab)
        {
            if (pausePrefab == null || !System.IO.File.Exists(ScenePath))
                return;

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var canvas = GameObject.Find("HUD_Canvas");
            if (canvas == null)
            {
                Debug.LogWarning("[FPSGame] HUD_Canvas not found — run Phase 0/4.");
                return;
            }

            var existing = canvas.transform.Find("PauseMenu");
            if (existing != null)
                Object.DestroyImmediate(existing.gameObject);

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(pausePrefab, scene);
            instance.transform.SetParent(canvas.transform, false);
            instance.name = "PauseMenu";

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void EnsurePrototypeSceneInBuildSettings()
        {
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(
                EditorBuildSettings.scenes);

            bool hasPrototype = false;
            foreach (var entry in scenes)
            {
                if (entry.path == ScenePath)
                {
                    hasPrototype = true;
                    break;
                }
            }

            if (!hasPrototype)
                scenes.Add(new EditorBuildSettingsScene(ScenePath, true));

            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static GameObject BuildPauseHierarchy()
        {
            var root = new GameObject("PauseMenu", typeof(RectTransform));
            var pauseUi = root.AddComponent<GameplayPauseUI>();

            var pauseButton = CreateButton(root.transform, "PauseButton", "||",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -16f), new Vector2(48f, 32f));

            var panel = CreatePanel(root.transform, "Panel",
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                new Color(0f, 0f, 0f, 0.65f));
            panel.SetActive(false);

            var layout = panel.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 12f;
            layout.padding = new RectOffset(24, 24, 24, 24);
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var resume = CreateButton(panel.transform, "ResumeButton", "Resume",
                Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(240f, 40f));
            var restart = CreateButton(panel.transform, "RestartButton", "Restart",
                Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(240f, 40f));
            var exit = CreateButton(panel.transform, "ExitButton", "Back to Menu",
                Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(240f, 40f));

            AssignPauseUi(pauseUi, panel, pauseButton, resume, restart, exit);
            return root;
        }

        private static GameObject CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 anchoredPos, Vector2 sizeDelta, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = sizeDelta;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            go.GetComponent<Image>().color = color;
            return go;
        }

        private static Button CreateButton(Transform parent, string name, string label,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = anchorMin;
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = sizeDelta;

            var image = go.GetComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.2f, 0.95f);

            var text = CreateText(go.transform, "Text", label, TextAnchor.MiddleCenter, 16);
            var textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return go.GetComponent<Button>();
        }

        private static Text CreateText(Transform parent, string name, string content, TextAnchor anchor, int fontSize)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);

            var text = go.GetComponent<Text>();
            text.text = content;
            text.alignment = anchor;
            text.fontSize = fontSize;
            text.color = Color.white;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return text;
        }

        private static void AssignPauseUi(GameplayPauseUI pauseUi, GameObject panel, Button pauseBtn,
            Button resume, Button restart, Button exit)
        {
            var so = new SerializedObject(pauseUi);
            so.FindProperty("panelRoot").objectReferenceValue = panel;
            so.FindProperty("pauseToggleButton").objectReferenceValue = pauseBtn;
            so.FindProperty("resumeButton").objectReferenceValue = resume;
            so.FindProperty("restartButton").objectReferenceValue = restart;
            so.FindProperty("exitToMenuButton").objectReferenceValue = exit;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
