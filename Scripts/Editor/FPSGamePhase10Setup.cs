using FPSGame.Save;
using FPSGame.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FPSGame.Editor
{
    /// <summary>
    /// Phase 10: Intro, ProfileSelection, MainMenu scenes + build settings.
    /// Menu: FPSGame / Setup Phase 10 (Scene Flow)
    /// </summary>
    public static class FPSGamePhase10Setup
    {
        private const string Root = "Assets/FPSGame";
        private const string IntroPath = Root + "/Scenes/Intro.unity";
        private const string ProfilePath = Root + "/Scenes/ProfileSelection.unity";
        private const string MainMenuPath = Root + "/Scenes/MainMenu.unity";
        private const string GameplayPath = Root + "/Scenes/PrototypeGameplay.unity";
        private const string StartPackPath = Root + "/Data/Save/StartPack_Default.asset";

        [MenuItem("FPSGame/Setup Phase 10 (Scene Flow)", false, 100)]
        public static void RunPhase10Setup()
        {
            FPSGameProjectSetup.RunPhase0SetupSilent();

            CreateIntroScene();
            CreateProfileSelectionScene();
            CreateMainMenuScene();
            ConfigureBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "FPSGame Phase 10",
                "Scene flow ready.\n\n" +
                "Build order: Intro → ProfileSelection → MainMenu → PrototypeGameplay\n" +
                "Play from Intro scene.",
                "OK");

            Debug.Log("[FPSGame] Phase 10 setup complete.");
        }

        private static void CreateIntroScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EnsureEventSystem();
            var canvas = CreateCanvas("IntroCanvas");

            CreateText(canvas.transform, "Title", "FPS Game Template", 36, TextAnchor.UpperCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(600f, 60f));
            CreateText(canvas.transform, "Hint", "Press any key to continue", 18, TextAnchor.LowerCenter,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 40f), new Vector2(400f, 40f));

            canvas.AddComponent<IntroSceneController>();
            EditorSceneManager.SaveScene(scene, IntroPath);
        }

        private static void CreateProfileSelectionScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EnsureEventSystem();
            var canvas = CreateCanvas("ProfileCanvas");

            CreateText(canvas.transform, "Header", "Select Profile", 28, TextAnchor.UpperCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(500f, 50f));

            var startPack = AssetDatabase.LoadAssetAtPath<SaveStartPack>(StartPackPath);
            var controller = canvas.AddComponent<ProfileSelectionController>();

            var b1 = CreateMenuButton(canvas.transform, "Profile1", "Profile 1", new Vector2(0f, 40f));
            var b2 = CreateMenuButton(canvas.transform, "Profile2", "Profile 2", new Vector2(0f, -10f));
            var b3 = CreateMenuButton(canvas.transform, "Profile3", "Profile 3", new Vector2(0f, -60f));

            var so = new SerializedObject(controller);
            so.FindProperty("startPack").objectReferenceValue = startPack;
            so.FindProperty("profile1Button").objectReferenceValue = b1;
            so.FindProperty("profile2Button").objectReferenceValue = b2;
            so.FindProperty("profile3Button").objectReferenceValue = b3;
            so.FindProperty("profile1Label").objectReferenceValue = b1.GetComponentInChildren<Text>();
            so.FindProperty("profile2Label").objectReferenceValue = b2.GetComponentInChildren<Text>();
            so.FindProperty("profile3Label").objectReferenceValue = b3.GetComponentInChildren<Text>();
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, ProfilePath);
        }

        private static void CreateMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EnsureEventSystem();
            var canvas = CreateCanvas("MenuCanvas");

            var mainPanel = CreatePanel(canvas.transform, "MainPanel", true);
            var playPanel = CreatePanel(canvas.transform, "PlayPanel", false);
            var settingsPanel = CreatePanel(canvas.transform, "SettingsPanel", false);
            var creditsPanel = CreatePanel(canvas.transform, "CreditsPanel", false);
            var quitPanel = CreatePanel(canvas.transform, "QuitConfirmPanel", false);
            var loadoutPanel = CreatePanel(canvas.transform, "LoadoutPanel", false);
            var campaignPanel = CreatePanel(canvas.transform, "CampaignPanel", false);

            CreateText(mainPanel.transform, "Title", "Main Menu", 32, TextAnchor.UpperCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(500f, 50f));

            var playBtn = CreateMenuButton(mainPanel.transform, "PlayBtn", "Play", new Vector2(0f, 60f));
            var settingsBtn = CreateMenuButton(mainPanel.transform, "SettingsBtn", "Settings", new Vector2(0f, 10f));
            var creditsBtn = CreateMenuButton(mainPanel.transform, "CreditsBtn", "Credits", new Vector2(0f, -40f));
            var quitBtn = CreateMenuButton(mainPanel.transform, "QuitBtn", "Quit", new Vector2(0f, -90f));

            CreateText(playPanel.transform, "PlayTitle", "Play", 28, TextAnchor.UpperCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(400f, 40f));
            var loadoutBtn = CreateMenuButton(playPanel.transform, "LoadoutBtn", "Loadout (Phase 11)", new Vector2(0f, 30f));
            var campaignBtn = CreateMenuButton(playPanel.transform, "CampaignBtn", "Campaign (Phase 12)", new Vector2(0f, -20f));
            var protoBtn = CreateMenuButton(playPanel.transform, "PrototypeBtn", "Prototype Gameplay", new Vector2(0f, -70f));

            CreateText(settingsPanel.transform, "SettingsText", "Settings — (placeholder)", 20, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            CreateText(creditsPanel.transform, "CreditsText", "Credits — FPS Game Template", 20, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            CreateText(loadoutPanel.transform, "LoadoutText", "Loadout panel — Phase 11", 20, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            CreateText(campaignPanel.transform, "CampaignText", "Campaign panel — Phase 12", 20, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            CreateText(quitPanel.transform, "QuitText", "Quit game?", 22, TextAnchor.UpperCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(400f, 40f));
            var confirmQuit = CreateMenuButton(quitPanel.transform, "ConfirmQuit", "Yes, Quit", new Vector2(0f, 10f));
            var cancelQuit = CreateMenuButton(quitPanel.transform, "CancelQuit", "Cancel", new Vector2(0f, -40f));

            var backBtn = CreateMenuButton(canvas.transform, "GlobalBack", "Back", new Vector2(-320f, 220f));
            backBtn.gameObject.SetActive(false);

            var navigator = canvas.AddComponent<MenuNavigator>();
            var menuController = canvas.AddComponent<MainMenuController>();

            var navSo = new SerializedObject(navigator);
            navSo.FindProperty("mainPanel").objectReferenceValue = mainPanel;
            navSo.FindProperty("playPanel").objectReferenceValue = playPanel;
            navSo.FindProperty("settingsPanel").objectReferenceValue = settingsPanel;
            navSo.FindProperty("creditsPanel").objectReferenceValue = creditsPanel;
            navSo.FindProperty("quitConfirmPanel").objectReferenceValue = quitPanel;
            navSo.FindProperty("loadoutPanel").objectReferenceValue = loadoutPanel;
            navSo.FindProperty("campaignPanel").objectReferenceValue = campaignPanel;
            navSo.FindProperty("globalBackButton").objectReferenceValue = backBtn;
            navSo.ApplyModifiedPropertiesWithoutUndo();

            var menuSo = new SerializedObject(menuController);
            menuSo.FindProperty("navigator").objectReferenceValue = navigator;
            menuSo.FindProperty("playButton").objectReferenceValue = playBtn;
            menuSo.FindProperty("settingsButton").objectReferenceValue = settingsBtn;
            menuSo.FindProperty("creditsButton").objectReferenceValue = creditsBtn;
            menuSo.FindProperty("quitButton").objectReferenceValue = quitBtn;
            menuSo.FindProperty("loadoutButton").objectReferenceValue = loadoutBtn;
            menuSo.FindProperty("campaignButton").objectReferenceValue = campaignBtn;
            menuSo.FindProperty("prototypePlayButton").objectReferenceValue = protoBtn;
            menuSo.FindProperty("confirmQuitButton").objectReferenceValue = confirmQuit;
            menuSo.FindProperty("cancelQuitButton").objectReferenceValue = cancelQuit;
            menuSo.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, MainMenuPath);
        }

        private static void ConfigureBuildSettings()
        {
            var scenes = new[]
            {
                new EditorBuildSettingsScene(IntroPath, true),
                new EditorBuildSettingsScene(ProfilePath, true),
                new EditorBuildSettingsScene(MainMenuPath, true),
                new EditorBuildSettingsScene(GameplayPath, true)
            };

            EditorBuildSettings.scenes = scenes;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null)
                return;

            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        private static GameObject CreateCanvas(string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            return go;
        }

        private static GameObject CreatePanel(Transform parent, string name, bool active)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            go.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.25f);
            go.SetActive(active);
            return go;
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

            var text = CreateText(go.transform, "Text", label, 18, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            return go.GetComponent<Button>();
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
