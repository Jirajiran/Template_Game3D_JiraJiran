using FPSGame.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace FPSGame.Editor
{
    /// <summary>
    /// Wires campaign win detection + victory UI into gameplay scenes.
    /// Menu: FPSGame / Setup Campaign Win (Level Complete)
    /// </summary>
    public static class FPSGameCampaignWinSetup
    {
        public const string PrototypeScenePath = "Assets/FPSGame/Scenes/PrototypeGameplay.unity";
        public const string VictoryPrefabPath = "Assets/FPSGame/UI/HUD/VictoryMenu.prefab";

        [MenuItem("FPSGame/Setup Campaign Win (Level Complete)", false, 125)]
        public static void RunCampaignWinSetup()
        {
            FPSGameProjectSetup.RunPhase0SetupSilent();

            var victoryPrefab = CreateOrLoadVictoryPrefab();
            SetupGameplayScene(PrototypeScenePath, victoryPrefab);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "FPSGame Campaign Win",
                "Level complete flow ready on PrototypeGameplay.\n\n" +
                "Kill all enemies after spawn waves finish → unlock next level.\n" +
                "Run Setup Campaign Level Scenes to copy to per-level scenes.\n" +
                "Debug: F7 force win in gameplay.",
                "OK");

            Debug.Log("[FPSGame] Campaign win setup complete.");
        }

        public static GameObject CreateOrLoadVictoryPrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(VictoryPrefabPath);
            if (existing != null)
                return existing;

            var root = BuildVictoryHierarchy();
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, VictoryPrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        public static void SetupGameplayScene(string scenePath, GameObject victoryPrefab)
        {
            if (victoryPrefab == null || !System.IO.File.Exists(scenePath))
                return;

            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            EnsureVictoryInOpenScene(victoryPrefab);
            WireWinControllerInOpenScene();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }

        private static void EnsureVictoryInOpenScene(GameObject victoryPrefab)
        {
            var canvas = GameObject.Find("HUD_Canvas");
            if (canvas == null)
            {
                Debug.LogWarning("[FPSGame] HUD_Canvas not found — run Phase 0/4.");
                return;
            }

            if (canvas.transform.Find("VictoryMenu") != null)
                return;

            var scene = EditorSceneManager.GetActiveScene();
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(victoryPrefab, scene);
            instance.transform.SetParent(canvas.transform, false);
            instance.name = "VictoryMenu";
        }

        private static void WireWinControllerInOpenScene()
        {
            var victoryUi = Object.FindObjectOfType<GameplayVictoryUI>();
            if (victoryUi == null)
            {
                Debug.LogWarning("[FPSGame] VictoryMenu not found in scene.");
                return;
            }

            var saveSystem = GameObject.Find("SaveSystem");
            GameObject host = saveSystem != null ? saveSystem : new GameObject("LevelSystem");
            if (saveSystem == null)
                host.name = "LevelSystem";

            var controller = host.GetComponent<CampaignLevelWinController>();
            if (controller == null)
                controller = host.AddComponent<CampaignLevelWinController>();

            var so = new SerializedObject(controller);
            so.FindProperty("victoryUi").objectReferenceValue = victoryUi;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject BuildVictoryHierarchy()
        {
            var root = new GameObject("VictoryMenu", typeof(RectTransform));
            var victoryUi = root.AddComponent<GameplayVictoryUI>();

            var panel = CreatePanel(root.transform, "Panel",
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                new Color(0f, 0f, 0f, 0.72f));
            panel.SetActive(false);

            var layout = panel.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 16f;
            layout.padding = new RectOffset(32, 32, 32, 32);
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var title = CreateText(panel.transform, "Title", "Mission Complete!", TextAnchor.MiddleCenter, 28);
            var titleLayout = title.gameObject.AddComponent<LayoutElement>();
            titleLayout.preferredHeight = 40f;

            var message = CreateText(panel.transform, "Message", "Level 1 complete!", TextAnchor.MiddleCenter, 18);
            var messageLayout = message.gameObject.AddComponent<LayoutElement>();
            messageLayout.preferredHeight = 56f;

            var backButton = CreateButton(panel.transform, "BackToCampaignButton", "Back to Campaign",
                new Vector2(280f, 44f));

            AssignVictoryUi(victoryUi, panel, message, backButton);
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

        private static Button CreateButton(Transform parent, string name, string label, Vector2 sizeDelta)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = sizeDelta;

            go.GetComponent<Image>().color = new Color(0.2f, 0.32f, 0.2f, 0.95f);

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

        private static void AssignVictoryUi(GameplayVictoryUI victoryUi, GameObject panel, Text message, Button backButton)
        {
            var so = new SerializedObject(victoryUi);
            so.FindProperty("panelRoot").objectReferenceValue = panel;
            so.FindProperty("messageText").objectReferenceValue = message;
            so.FindProperty("backToCampaignButton").objectReferenceValue = backButton;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
