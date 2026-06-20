using FPSGame.Save;
using FPSGame.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace FPSGame.Editor
{
    /// <summary>
    /// Phase 11: Loadout panel UI in MainMenu + LoadoutItem prefab.
    /// Menu: FPSGame / Setup Phase 11 (Loadout Panel)
    /// </summary>
    public static class FPSGamePhase11Setup
    {
        private const string Root = "Assets/FPSGame";
        private const string MainMenuPath = Root + "/Scenes/MainMenu.unity";
        private const string StartPackPath = Root + "/Data/Save/StartPack_Default.asset";
        private const string RegistryPath = Root + "/Data/Save/GameContentRegistry.asset";
        private const string LoadoutItemPrefabPath = Root + "/UI/Menu/LoadoutItem.prefab";

        [MenuItem("FPSGame/Setup Phase 11 (Loadout Panel)", false, 110)]
        public static void RunPhase11Setup()
        {
            FPSGameProjectSetup.RunPhase0SetupSilent();

            var itemPrefab = CreateOrLoadLoadoutItemPrefab();
            UpgradeMainMenuLoadoutPanel(itemPrefab);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "FPSGame Phase 11",
                "Loadout panel ready in MainMenu.\n\n" +
                "Play flow: Intro → Profile → Main Menu → Play → Loadout\n" +
                "Drag weapons to slots; click hero to select. Saves to JSON.",
                "OK");

            Debug.Log("[FPSGame] Phase 11 setup complete.");
        }

        private static LoadoutItemView CreateOrLoadLoadoutItemPrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(LoadoutItemPrefabPath);
            if (existing != null)
                return existing.GetComponent<LoadoutItemView>();

            EnsureFolder(Root + "/UI/Menu");

            var temp = new GameObject("LoadoutItem", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rect = temp.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(280f, 38f);

            var bg = temp.GetComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.2f, 0.95f);

            var label = CreateText(temp.transform, "Label", "Item", 16, TextAnchor.MiddleLeft,
                Vector2.zero, Vector2.one, new Vector2(12f, 0f), Vector2.zero);

            var view = temp.AddComponent<LoadoutItemView>();
            var so = new SerializedObject(view);
            so.FindProperty("label").objectReferenceValue = label;
            so.FindProperty("background").objectReferenceValue = bg;
            so.ApplyModifiedPropertiesWithoutUndo();

            var prefab = PrefabUtility.SaveAsPrefabAsset(temp, LoadoutItemPrefabPath);
            Object.DestroyImmediate(temp);
            return prefab.GetComponent<LoadoutItemView>();
        }

        private static void UpgradeMainMenuLoadoutPanel(LoadoutItemView itemPrefab)
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
                Debug.LogWarning("[FPSGame] MenuCanvas not found in MainMenu.");
                return;
            }

            var loadoutPanel = FindChild(canvas.transform, "LoadoutPanel");
            if (loadoutPanel == null)
            {
                Debug.LogWarning("[FPSGame] LoadoutPanel not found — run Phase 10 first.");
                return;
            }

            ClearChildren(loadoutPanel.transform);

            var startPack = AssetDatabase.LoadAssetAtPath<SaveStartPack>(StartPackPath);
            var registry = AssetDatabase.LoadAssetAtPath<GameContentRegistry>(RegistryPath);

            CreateText(loadoutPanel.transform, "Header", "Loadout", 28, TextAnchor.UpperCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(500f, 40f));

            var statusText = CreateText(loadoutPanel.transform, "StatusText", string.Empty, 16,
                TextAnchor.LowerCenter, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 16f), new Vector2(900f, 30f));

            var content = CreateStretchPanel(loadoutPanel.transform, "Content", new Vector2(24f, 56f), new Vector2(-24f, -48f));
            content.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.1f);

            var heroScroll = CreateScrollColumn(content.transform, "HeroesColumn",
                new Vector2(0f, 0f), new Vector2(0.28f, 1f), "Heroes (click to select)", out Transform heroContent);
            var center = CreateStretchPanel(content.transform, "CenterColumn", Vector2.zero, Vector2.zero);
            StretchAnchors(center, new Vector2(0.29f, 0f), new Vector2(0.71f, 1f));
            center.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.08f);

            var weaponScroll = CreateScrollColumn(content.transform, "WeaponsColumn",
                new Vector2(0.72f, 0f), new Vector2(1f, 1f), "Weapons (drag to slot)", out Transform weaponContent);

            var selectedHeroLabel = CreateText(center.transform, "SelectedHeroLabel", "Selected hero: —", 18,
                TextAnchor.UpperCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -36f), new Vector2(520f, 32f));

            CreateText(center.transform, "SlotsHeader", "Equipped weapons", 16, TextAnchor.UpperCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -72f), new Vector2(400f, 24f));

            var slotViews = new LoadoutItemView[3];
            var slotDrops = new LoadoutDropTarget[3];
            for (int i = 0; i < 3; i++)
            {
                float y = 20f - i * 48f;
                var slotRoot = CreateSlotRoot(center.transform, $"WeaponSlot{i + 1}", new Vector2(0f, y));
                slotViews[i] = (LoadoutItemView)PrefabUtility.InstantiatePrefab(itemPrefab.gameObject, slotRoot.transform);
                slotViews[i].name = $"SlotView{i + 1}";
                Stretch(slotViews[i].GetComponent<RectTransform>());

                slotDrops[i] = slotRoot.AddComponent<LoadoutDropTarget>();
            }

            var unequipArea = CreateStretchPanel(center.transform, "UnequipPool", Vector2.zero, Vector2.zero);
            StretchAnchors(unequipArea, new Vector2(0.1f, 0f), new Vector2(0.9f, 0f));
            var unequipRect = unequipArea.GetComponent<RectTransform>();
            unequipRect.pivot = new Vector2(0.5f, 0f);
            unequipRect.sizeDelta = new Vector2(0f, 56f);
            unequipRect.anchoredPosition = new Vector2(0f, 24f);
            unequipArea.GetComponent<Image>().color = new Color(0.35f, 0.12f, 0.12f, 0.55f);
            CreateText(unequipArea.transform, "Label", "Drop here to unequip", 15, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var unequipDrop = unequipArea.AddComponent<LoadoutDropTarget>();

            var controller = loadoutPanel.GetComponent<LoadoutPanelController>();
            if (controller == null)
                controller = loadoutPanel.AddComponent<LoadoutPanelController>();

            var ctrlSo = new SerializedObject(controller);
            ctrlSo.FindProperty("startPack").objectReferenceValue = startPack;
            ctrlSo.FindProperty("contentRegistry").objectReferenceValue = registry;
            ctrlSo.FindProperty("heroListContent").objectReferenceValue = heroContent;
            ctrlSo.FindProperty("weaponListContent").objectReferenceValue = weaponContent;
            ctrlSo.FindProperty("loadoutItemPrefab").objectReferenceValue = itemPrefab;
            ctrlSo.FindProperty("selectedHeroLabel").objectReferenceValue = selectedHeroLabel;
            ctrlSo.FindProperty("statusText").objectReferenceValue = statusText;

            var slotsProp = ctrlSo.FindProperty("weaponSlotViews");
            slotsProp.arraySize = 3;
            for (int i = 0; i < 3; i++)
                slotsProp.GetArrayElementAtIndex(i).objectReferenceValue = slotViews[i];

            var dropsProp = ctrlSo.FindProperty("weaponSlotDropTargets");
            dropsProp.arraySize = 3;
            for (int i = 0; i < 3; i++)
                dropsProp.GetArrayElementAtIndex(i).objectReferenceValue = slotDrops[i];

            ctrlSo.FindProperty("unequipDropTarget").objectReferenceValue = unequipDrop;
            ctrlSo.ApplyModifiedPropertiesWithoutUndo();

            UpdateLoadoutButtonLabel(canvas.transform);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void UpdateLoadoutButtonLabel(Transform canvas)
        {
            var playPanel = FindChild(canvas, "PlayPanel");
            if (playPanel == null)
                return;

            var loadoutBtn = FindChild(playPanel, "LoadoutBtn");
            if (loadoutBtn == null)
                return;

            var text = loadoutBtn.GetComponentInChildren<Text>();
            if (text != null)
                text.text = "Loadout";
        }

        private static GameObject CreateScrollColumn(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
            string header, out Transform content)
        {
            var column = CreateStretchPanel(parent, name, Vector2.zero, Vector2.zero);
            StretchAnchors(column, anchorMin, anchorMax);
            column.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.08f);

            CreateText(column.transform, "Header", header, 15, TextAnchor.UpperCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -8f), new Vector2(300f, 24f));

            var scrollGo = new GameObject("Scroll", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
            scrollGo.transform.SetParent(column.transform, false);
            StretchAnchors(scrollGo, new Vector2(0f, 0f), new Vector2(1f, 1f));
            var scrollRect = scrollGo.GetComponent<RectTransform>();
            scrollRect.offsetMin = new Vector2(8f, 8f);
            scrollRect.offsetMax = new Vector2(-8f, -36f);
            scrollGo.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.15f);

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
            layout.spacing = 6f;
            layout.padding = new RectOffset(4, 4, 4, 4);
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var fitter = contentGo.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = scrollGo.GetComponent<ScrollRect>();
            scroll.viewport = viewport.GetComponent<RectTransform>();
            scroll.content = contentRect;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;

            content = contentGo.transform;
            return column;
        }

        private static GameObject CreateSlotRoot(Transform parent, string name, Vector2 anchoredPos)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(360f, 42f);
            rect.anchoredPosition = anchoredPos;
            go.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f, 0.65f);
            return go;
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
