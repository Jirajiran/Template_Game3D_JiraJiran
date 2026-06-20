using FPSGame.Core;
using FPSGame.Weapons;
using UnityEditor;
using UnityEngine;

namespace FPSGame.Editor
{
    /// <summary>
    /// Phase 2: adds PlayerController + CharacterController to Player prefab.
    /// Menu: FPSGame / Setup Phase 2 (Player Controller)
    /// </summary>
    public static class FPSGamePhase2Setup
    {
        private const string PlayerPrefabPath = "Assets/FPSGame/Prefabs/Player/Player.prefab";

        [MenuItem("FPSGame/Setup Phase 2 (Player Controller)", false, 20)]
        public static void RunPhase2Setup()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (prefab == null)
            {
                EditorUtility.DisplayDialog(
                    "FPSGame Phase 2",
                    "Player.prefab not found.\nRun Phase 1 setup first.",
                    "OK");
                return;
            }

            var root = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            UpgradePlayerPrefab(root);
            PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
            PrefabUtility.UnloadPrefabContents(root);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "FPSGame Phase 2",
                "Player prefab upgraded with PlayerController + CharacterController.\n\n" +
                "Play mode: WASD move | Shift sprint | Space jump\n" +
                "Mouse look | LMB fire | RMB aim (slow move) | Scroll / 1-2-3 weapons",
                "OK");

            Debug.Log("[FPSGame] Phase 2 setup complete.");
        }

        internal static void UpgradePlayerPrefab(GameObject player)
        {
            RemoveLegacyPhysics(player);

            var controller = player.GetComponent<CharacterController>();
            if (controller == null)
                controller = player.AddComponent<CharacterController>();

            controller.height = 2f;
            controller.radius = 0.4f;
            controller.center = new Vector3(0f, 1f, 0f);
            controller.slopeLimit = 45f;
            controller.stepOffset = 0.3f;

            var playerController = player.GetComponent<PlayerController>();
            if (playerController == null)
                playerController = player.AddComponent<PlayerController>();

            var camera = player.GetComponentInChildren<Camera>();
            var weaponMount = player.transform.Find("WeaponMount");
            var weaponHandler = weaponMount != null
                ? weaponMount.GetComponent<WeaponHandler>()
                : player.GetComponentInChildren<WeaponHandler>();

            if (weaponHandler != null && weaponMount != null)
            {
                var weaponSo = new SerializedObject(weaponHandler);
                weaponSo.FindProperty("muzzle").objectReferenceValue = weaponMount;
                weaponSo.ApplyModifiedPropertiesWithoutUndo();
            }

            var playerSo = new SerializedObject(playerController);
            playerSo.FindProperty("cameraPivot").objectReferenceValue =
                camera != null ? camera.transform : null;
            playerSo.FindProperty("weaponHandler").objectReferenceValue = weaponHandler;
            playerSo.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void RemoveLegacyPhysics(GameObject player)
        {
            var rigidbody = player.GetComponent<Rigidbody>();
            if (rigidbody != null)
                Object.DestroyImmediate(rigidbody);

            var capsule = player.GetComponent<CapsuleCollider>();
            if (capsule != null)
                Object.DestroyImmediate(capsule);
        }
    }
}
