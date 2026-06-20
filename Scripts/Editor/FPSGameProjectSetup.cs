using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FPSGame.Editor
{
    /// <summary>
    /// Phase 0 bootstrap: folders, tags, layers, and PrototypeGameplay scene.
    /// Menu: FPSGame / Setup Phase 0 (Project Bootstrap)
    /// </summary>
    public static class FPSGameProjectSetup
    {
        private const string Root = "Assets/FPSGame";
        private const string ScenePath = Root + "/Scenes/PrototypeGameplay.unity";

        private static readonly string[] Tags = { "Player", "Enemy" };
        private static readonly string[] Layers = { "Ground", "Player", "Enemy", "Hitbox" };

        private static readonly string[] FolderPaths =
        {
            Root,
            Root + "/Scripts",
            Root + "/Scripts/Core",
            Root + "/Scripts/Weapons",
            Root + "/Scripts/Editor",
            Root + "/Data",
            Root + "/Data/Characters",
            Root + "/Data/Weapons",
            Root + "/Prefabs",
            Root + "/Prefabs/Player",
            Root + "/Prefabs/Weapons",
            Root + "/Scenes",
            Root + "/UI",
            Root + "/UI/HUD",
            Root + "/Animation",
        };

        [MenuItem("FPSGame/Setup Phase 0 (Project Bootstrap)", false, 0)]
        public static void RunPhase0Setup()
        {
            if (!EditorUtility.DisplayDialog(
                    "FPSGame Phase 0",
                    "Creates folder structure, Tags, Layers, and PrototypeGameplay scene.\n\n" +
                    "Copy vault template/Scripts/ into Assets/FPSGame/Scripts/ before or after this step.",
                    "Run Setup",
                    "Cancel"))
            {
                return;
            }

            RunPhase0SetupSilent();
            CreatePrototypeSceneIfMissing();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "FPSGame Phase 0",
                "Setup complete.\n\n" +
                "Next:\n" +
                "1. Copy template/Scripts/ → Assets/FPSGame/Scripts/\n" +
                "2. Open PrototypeGameplay scene\n" +
                "3. Continue with Phase 1 (Character) in UnitySetup.md",
                "OK");

            Debug.Log("[FPSGame] Phase 0 setup complete. Scene: " + ScenePath);
        }

        /// <summary>Folders, tags, layers only — no scene overwrite.</summary>
        public static void RunPhase0SetupSilent()
        {
            CreateFolders();
            EnsureTags();
            EnsureLayers();
        }

        public static void CreatePrototypeSceneIfMissing()
        {
            if (File.Exists(ScenePath))
            {
                return;
            }

            CreatePrototypeScene();
        }

        [MenuItem("FPSGame/Open PrototypeGameplay Scene", false, 1)]
        public static void OpenPrototypeScene()
        {
            if (!File.Exists(ScenePath))
            {
                EditorUtility.DisplayDialog("FPSGame", "Scene not found. Run Phase 0 setup first.", "OK");
                return;
            }

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(ScenePath);
            }
        }

        private static void CreateFolders()
        {
            foreach (var path in FolderPaths)
            {
                CreateFolderPath(path);
            }
        }

        private static void CreateFolderPath(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            var folderName = Path.GetFileName(path);

            if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(folderName))
            {
                return;
            }

            if (!AssetDatabase.IsValidFolder(parent))
            {
                CreateFolderPath(parent);
            }

            AssetDatabase.CreateFolder(parent, folderName);
        }

        private static void EnsureTags()
        {
            foreach (var tag in Tags)
            {
                AddTag(tag);
            }
        }

        private static void AddTag(string tag)
        {
            if (TagExists(tag))
            {
                return;
            }

            UnityEditorInternal.InternalEditorUtility.AddTag(tag);
            Debug.Log("[FPSGame] Added tag: " + tag);
        }

        private static bool TagExists(string tag)
        {
            foreach (var existing in UnityEditorInternal.InternalEditorUtility.tags)
            {
                if (existing == tag)
                {
                    return true;
                }
            }

            return false;
        }

        private static void EnsureLayers()
        {
            foreach (var layer in Layers)
            {
                AddLayer(layer);
            }
        }

        private static void AddLayer(string layer)
        {
            if (LayerExists(layer))
            {
                return;
            }

            UnityEditorInternal.InternalEditorUtility.AddLayer(layer);
            Debug.Log("[FPSGame] Added layer: " + layer);
        }

        private static bool LayerExists(string layer)
        {
            foreach (var existing in UnityEditorInternal.InternalEditorUtility.layers)
            {
                if (existing == layer)
                {
                    return true;
                }
            }

            return false;
        }

        private static int GetLayerIndex(string layerName)
        {
            var index = LayerMask.NameToLayer(layerName);
            return index >= 0 ? index : 0;
        }

        private static void CreatePrototypeScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "PrototypeGameplay";

            CreateGround();
            CreateLighting();
            CreateEventSystemAndCanvas();
            CreateSpawnMarker();

            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static void CreateGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(2f, 1f, 2f);
            ground.layer = GetLayerIndex("Ground");

            var renderer = ground.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");
            }
        }

        private static void CreateLighting()
        {
            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1f;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private static void CreateEventSystemAndCanvas()
        {
            var eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<EventSystem>();
            eventSystemGo.AddComponent<StandaloneInputModule>();

            var canvasGo = new GameObject("HUD_Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();
        }

        private static void CreateSpawnMarker()
        {
            var spawn = new GameObject("PlayerSpawn");
            spawn.transform.position = new Vector3(0f, 1f, 0f);
        }
    }

    /// <summary>Draws PlayerSpawn in Scene view without a runtime component.</summary>
    public static class SpawnMarkerGizmoDrawer
    {
        [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.Active)]
        private static void DrawSpawnMarker(Transform transform, GizmoType gizmoType)
        {
            if (transform.gameObject.name != "PlayerSpawn")
            {
                return;
            }

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 1.8f);
        }
    }
}
