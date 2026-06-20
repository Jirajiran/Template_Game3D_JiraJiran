using FPSGame.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace FPSGame.Editor
{
    /// <summary>
    /// Phase 12: Campaign + level select UI in MainMenu.
    /// Menu: FPSGame / Setup Phase 12 (Campaign Select)
    /// </summary>
    public static class FPSGamePhase12Setup
    {
        private const string Root = "Assets/FPSGame";
        private const string MainMenuPath = Root + "/Scenes/MainMenu.unity";
        private const string LevelButtonPrefabPath = Root + "/UI/Menu/CampaignLevelButton.prefab";

        [MenuItem("FPSGame/Setup Phase 12 (Campaign Select)", false, 120)]
        public static void RunPhase12Setup()
        {
            FPSGameProjectSetup.RunPhase0SetupSilent();

            var levelButtonPrefab = CreateOrLoadLevelButtonPrefab();
            UpgradeMainMenuCampaignPanel(levelButtonPrefab);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "FPSGame Phase 12",
                "Campaign select ready in MainMenu.\n\n" +
                "Play → Campaign → pick campaign → unlocked levels\n" +
                "Debug: F6 unlocks secret stage in gameplay.",
                "OK");

            Debug.Log("[FPSGame] Phase 12 setup complete.");
        }

        private static Button CreateOrLoadLevelButtonPrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(LevelButtonPrefabPath);
            if (existing != null)
                return existing.GetComponent<Button>();

            EnsureFolder(Root + "/UI/Menu");

            var temp = new GameObject("CampaignLevelButton",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            var rect = temp.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(320f, 42f);
            temp.GetComponent<Image>().color = new Color(0.2f, 0.28f, 0.2f, 0.95f);

            CreateText(temp.transform, "Text", "Level 1", 17, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            temp.SetActive(false);
            var prefab = PrefabUtility.SaveAsPrefabAsset(temp, LevelButtonPrefabPath);
            Object.DestroyImmediate(temp);
            return prefab.GetComponent<Button>();
        }

        private static void UpgradeMainMenuCampaignPanel(Button levelButtonPrefab)
        {
            if (!System.IO.File.Exists(MainMenuPath))
            {
                Debug.LogWarning("[FPSGame] MainMenu scene missing — run Phase 10 first.");
                return;
            }

            var scene = EditorSceneManager.OpenScene(MainMenuPath, OpenSceneMode.Single);
            var canvas = GameObject.Find("MenuCanvas");
            if (canvas == null)
            {
                Debug.LogWarning("[FPSGame] MenuCanvas not found.");
                return;
            }

            var campaignPanel = FindChild(canvas.transform, "CampaignPanel");
            if (campaignPanel == null)
            {
                Debug.LogWarning("[FPSGame] CampaignPanel not found — run Phase 10 first.");
                return;
            }

            ClearChildren(campaignPanel.transform);

            CreateText(campaignPanel.transform, "Header", "Campaign", 28, TextAnchor.UpperCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(500f, 40f));

            var statusText = CreateText(campaignPanel.transform, "StatusText", string.Empty, 16,
                TextAnchor.LowerCenter, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 16f), new Vector2(900f, 30f));

            var campaignListView = CreateStretchPanel(campaignPanel.transform, "CampaignListView",
                new Vector2(40f, 56f), new Vector2(-40f, -48f));
            campaignListView.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.08f);

            var campaignListTitle = CreateText(campaignListView.transform, "Title", "Select Campaign", 20,
                TextAnchor.UpperCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -12f), new Vector2(400f, 30f));

            var campaignButtons = new Button[4];
            for (int i = 0; i < 4; i++)
            {
                float y = 70f - i * 52f;
                campaignButtons[i] = CreateMenuButton(campaignListView.transform, $"Campaign{i + 1}Btn",
                    $"Campaign {i + 1}", new Vector2(0f, y));
            }

            var levelListView = CreateStretchPanel(campaignPanel.transform, "LevelListView",
                new Vector2(40f, 56f), new Vector2(-40f, -48f));
            levelListView.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.08f);
            levelListView.SetActive(false);

            var levelListTitle = CreateText(levelListView.transform, "Title", "Campaign 1 — Levels", 20,
                TextAnchor.UpperCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -12f), new Vector2(500f, 30f));

            var levelListBackButton = CreateMenuButton(levelListView.transform, "LevelListBack", "Back to Campaigns",
                new Vector2(0f, 150f));

            CreateScrollList(levelListView.transform, out Transform levelListContent);

            var controller = campaignPanel.GetComponent<CampaignPanelController>();
            if (controller == null)
                controller = campaignPanel.AddComponent<CampaignPanelController>();

            var ctrlSo = new SerializedObject(controller);
            ctrlSo.FindProperty("campaignListView").objectReferenceValue = campaignListView;
            ctrlSo.FindProperty("levelListView").objectReferenceValue = levelListView;
            ctrlSo.FindProperty("campaignListTitle").objectReferenceValue = campaignListTitle;
            ctrlSo.FindProperty("levelListTitle").objectReferenceValue = levelListTitle;
            ctrlSo.FindProperty("statusText").objectReferenceValue = statusText;
            ctrlSo.FindProperty("levelListBackButton").objectReferenceValue = levelListBackButton;
            ctrlSo.FindProperty("levelListContent").objectReferenceValue = levelListContent;
            ctrlSo.FindProperty("levelButtonPrefab").objectReferenceValue = levelButtonPrefab;

            var campaignButtonsProp = ctrlSo.FindProperty("campaignButtons");
            campaignButtonsProp.arraySize = 4;
            for (int i = 0; i < 4; i++)
                campaignButtonsProp.GetArrayElementAtIndex(i).objectReferenceValue = campaignButtons[i];

            ctrlSo.ApplyModifiedPropertiesWithoutUndo();

            var navigator = canvas.GetComponent<MenuNavigator>();
            if (navigator != null)
            {
                var navSo = new SerializedObject(navigator);
                navSo.FindProperty("campaignPanelController").objectReferenceValue = controller;
                navSo.ApplyModifiedPropertiesWithoutUndo();
            }

            UpdateCampaignButtonLabel(canvas.transform);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static GameObject CreateScrollList(Transform parent, out Transform content)
        {
            var scrollGo = new GameObject("LevelScroll", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
            scrollGo.transform.SetParent(parent, false);
            StretchAnchors(scrollGo, new Vector2(0.15f, 0f), new Vector2(0.85f, 1f));
            var scrollRect = scrollGo.GetComponent<RectTransform>();
            scrollRect.offsetMin = new Vector2(0f, 24f);
            scrollRect.offsetMax = new Vector2(0f, -48f);
            scrollGo.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.12f);

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(scrollGo.transform, false);
            Stretch(viewport.GetComponent<RectTransform>());
            viewport.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.02f);
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            var contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentGo.transform.SetParent(viewport.transform, false);
            var contentRect = contentGo.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0f, 0f);

            var layout = contentGo.GetComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.spacing = 8f;
            layout.padding = new RectOffset(4, 4, 4, 4);
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            contentGo.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = scrollGo.GetComponent<ScrollRect>();
            scroll.viewport = viewport.GetComponent<RectTransform>();
            scroll.content = contentRect;
            scroll.horizontal = false;
            scroll.vertical = true;

            content = contentGo.transform;
            return scrollGo;
        }

        private static void UpdateCampaignButtonLabel(Transform canvas)
        {
            var playPanel = FindChild(canvas, "PlayPanel");
            var campaignBtn = playPanel != null ? FindChild(playPanel, "CampaignBtn") : null;
            if (campaignBtn == null)
                return;

            var text = campaignBtn.GetComponentInChildren<Text>();
            if (text != null)
                text.text = "Campaign";
        }

        private static Button CreateMenuButton(Transform parent, string name, string label, Vector2 anchoredPos)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(320f, 44f);
            rect.anchoredPosition = anchoredPos;
            go.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.95f);

            CreateText(go.transform, "Text", label, 18, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            return go.GetComponent<Button>();
        }

        private static GameObject CreateStretchPanel(Transform parent, string name, Vector2 offsetMin, Vector2 offsetMax)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            StretchAnchors(go, Vector2.zero, Vector2.one);
            var rect = go.GetComponent<RectTransform>();
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            return go;
        }

        private static void StretchAnchors(GameObject go, Vector2 anchorMin, Vector2 anchorMax)
        {
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static Transform FindChild(Transform parent, string name)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (child.name == name)
                    return child;

                var nested = FindChild(child, name);
                if (nested != null)
                    return nested;
            }

            return null;
        }

        private static void ClearChildren(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(parent.GetChild(i).gameObject);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            var parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        private static Text CreateText(Transform parent, string name, string content, int fontSize,
            TextAnchor anchor, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = sizeDelta;

            var text = go.GetComponent<Text>();
            text.text = content;
            text.fontSize = fontSize;
            text.alignment = anchor;
            text.color = Color.white;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return text;
        }
    }
}
