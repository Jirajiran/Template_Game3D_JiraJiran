using FPSGame.Core;
using FPSGame.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace FPSGame.Editor
{
    /// <summary>
    /// Phase 4: Gameplay HUD under HUD_Canvas + PlayerWallet on player.
    /// Menu: FPSGame / Setup Phase 4 (HUD)
    /// </summary>
    public static class FPSGamePhase4Setup
    {
        private const string ScenePath = "Assets/FPSGame/Scenes/PrototypeGameplay.unity";
        private const string PlayerPrefabPath = "Assets/FPSGame/Prefabs/Player/Player.prefab";
        private const string HudPrefabPath = "Assets/FPSGame/UI/HUD/GameplayHUD.prefab";

        [MenuItem("FPSGame/Setup Phase 4 (HUD)", false, 40)]
        public static void RunPhase4Setup()
        {
            FPSGameProjectSetup.RunPhase0SetupSilent();

            AddWalletToPlayerPrefab();
            var hudPrefab = CreateOrLoadGameplayHudPrefab();
            AddHudToScene(hudPrefab);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "FPSGame Phase 4",
                "Gameplay HUD ready under HUD_Canvas.\n\n" +
                "Play mode: HP bars (dual layer), weapon slots 1-3, wallet top-right.",
                "OK");

            Debug.Log("[FPSGame] Phase 4 setup complete.");
        }

        private static void AddWalletToPlayerPrefab()
        {
            if (!System.IO.File.Exists(PlayerPrefabPath))
            {
                Debug.LogWarning("[FPSGame] Player.prefab missing — run Phase 1 first.");
                return;
            }

            var root = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            if (root.GetComponent<PlayerWallet>() == null)
                root.AddComponent<PlayerWallet>();

            PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
            PrefabUtility.UnloadPrefabContents(root);
        }

        private static GameObject CreateOrLoadGameplayHudPrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(HudPrefabPath);
            if (existing != null)
                return existing;

            var hudRoot = BuildGameplayHudHierarchy();
            var prefab = PrefabUtility.SaveAsPrefabAsset(hudRoot, HudPrefabPath);
            Object.DestroyImmediate(hudRoot);
            return prefab;
        }

        private static void AddHudToScene(GameObject hudPrefab)
        {
            if (hudPrefab == null || !System.IO.File.Exists(ScenePath))
                return;

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var canvas = GameObject.Find("HUD_Canvas");
            if (canvas == null)
            {
                Debug.LogWarning("[FPSGame] HUD_Canvas not found — run Phase 0.");
                return;
            }

            var existing = canvas.transform.Find("GameplayHUD");
            if (existing != null)
                Object.DestroyImmediate(existing.gameObject);

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(hudPrefab, scene);
            instance.transform.SetParent(canvas.transform, false);
            instance.name = "GameplayHUD";

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static GameObject BuildGameplayHudHierarchy()
        {
            var root = new GameObject("GameplayHUD", typeof(RectTransform));
            var manager = root.AddComponent<GameplayHudManager>();

            var healthPanel = CreatePanel(root.transform, "HealthPanel",
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(24f, 24f), new Vector2(280f, 48f));

            var delayFill = CreateFilledImage(healthPanel.transform, "DelayFill",
                new Color(0.55f, 0.08f, 0.08f, 0.85f));
            var primaryFill = CreateFilledImage(healthPanel.transform, "PrimaryFill",
                new Color(0.9f, 0.15f, 0.15f, 1f));

            var weaponPanel = CreatePanel(root.transform, "WeaponPanel",
                new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-24f, 24f), new Vector2(320f, 120f));
            weaponPanel.GetComponent<RectTransform>().pivot = new Vector2(1f, 0f);

            var slotViews = new WeaponSlotHudUI.SlotView[3];
            for (int i = 0; i < 3; i++)
            {
                float y = i * 36f;
                var slot = CreatePanel(weaponPanel.transform, $"Slot{i + 1}",
                    new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, y), new Vector2(0f, 32f));
                var slotRect = slot.GetComponent<RectTransform>();
                slotRect.anchorMin = new Vector2(0f, 0f);
                slotRect.anchorMax = new Vector2(1f, 0f);
                slotRect.offsetMin = new Vector2(0f, y);
                slotRect.offsetMax = new Vector2(0f, y + 32f);

                var bg = slot.GetComponent<Image>();
                var label = CreateText(slot.transform, "Label", $"{i + 1}: —", TextAnchor.MiddleLeft, 14);
                var labelRect = label.GetComponent<RectTransform>();
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.one;
                labelRect.offsetMin = new Vector2(8f, 0f);
                labelRect.offsetMax = new Vector2(-8f, 0f);

                slotViews[i] = new WeaponSlotHudUI.SlotView { background = bg, label = label };
            }

            var walletPanel = CreatePanel(root.transform, "WalletPanel",
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-24f, -24f), new Vector2(160f, 36f));
            walletPanel.GetComponent<RectTransform>().pivot = Vector2.one;
            var walletText = CreateText(walletPanel.transform, "Value", "$ 100", TextAnchor.MiddleRight, 18);

            var healthBar = healthPanel.AddComponent<DualLayerHealthBarUI>();
            AssignHealthBar(healthBar, primaryFill, delayFill);

            var weaponHud = weaponPanel.AddComponent<WeaponSlotHudUI>();
            AssignWeaponSlots(weaponHud, slotViews);

            var walletHud = walletPanel.AddComponent<WalletHudUI>();
            AssignWalletHud(walletHud, walletText);

            AssignHudManager(manager, healthBar, weaponHud, walletHud);

            return root;
        }

        private static GameObject CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = anchorMin;
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = size;

            var image = go.GetComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.35f);
            return go;
        }

        private static Image CreateFilledImage(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var image = go.GetComponent<Image>();
            image.color = color;
            image.type = Image.Type.Filled;
            image.fillMethod = Image.FillMethod.Horizontal;
            image.fillOrigin = (int)Image.OriginHorizontal.Left;
            image.fillAmount = 1f;
            return image;
        }

        private static Text CreateText(Transform parent, string name, string content, TextAnchor anchor, int fontSize)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var text = go.GetComponent<Text>();
            text.text = content;
            text.alignment = anchor;
            text.fontSize = fontSize;
            text.color = Color.white;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return text;
        }

        private static void AssignHealthBar(DualLayerHealthBarUI healthBar, Image primary, Image delay)
        {
            var so = new SerializedObject(healthBar);
            so.FindProperty("primaryFill").objectReferenceValue = primary;
            so.FindProperty("delayFill").objectReferenceValue = delay;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AssignWeaponSlots(WeaponSlotHudUI weaponHud, WeaponSlotHudUI.SlotView[] slots)
        {
            var so = new SerializedObject(weaponHud);
            var array = so.FindProperty("slots");
            array.arraySize = slots.Length;
            for (int i = 0; i < slots.Length; i++)
            {
                var element = array.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("background").objectReferenceValue = slots[i].background;
                element.FindPropertyRelative("label").objectReferenceValue = slots[i].label;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AssignWalletHud(WalletHudUI walletHud, Text valueText)
        {
            var so = new SerializedObject(walletHud);
            so.FindProperty("valueText").objectReferenceValue = valueText;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AssignHudManager(GameplayHudManager manager, DualLayerHealthBarUI health, WeaponSlotHudUI weapons, WalletHudUI wallet)
        {
            var so = new SerializedObject(manager);
            so.FindProperty("healthBar").objectReferenceValue = health;
            so.FindProperty("weaponSlotHud").objectReferenceValue = weapons;
            so.FindProperty("walletHud").objectReferenceValue = wallet;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
