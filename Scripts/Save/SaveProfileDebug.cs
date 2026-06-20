using UnityEngine;

namespace FPSGame.Save
{
    /// <summary>
    /// Prototype debug: F1–F3 load profile, F5 save, F6 unlock secret stage. Remove before shipping.
    /// </summary>
    public class SaveProfileDebug : MonoBehaviour
    {
        [SerializeField] private GameplayProfileBootstrap bootstrap;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
                LoadProfile(1);
            if (Input.GetKeyDown(KeyCode.F2))
                LoadProfile(2);
            if (Input.GetKeyDown(KeyCode.F3))
                LoadProfile(3);
            if (Input.GetKeyDown(KeyCode.F5))
                SaveProfileService.Save();
            if (Input.GetKeyDown(KeyCode.F6))
                UnlockSecretStageDebug();
        }

        private static void UnlockSecretStageDebug()
        {
            SaveProfileService.UnlockSecretStage();
            Debug.Log("[SaveProfileDebug] Secret stage unlocked (F6).");
        }

        private void LoadProfile(int index)
        {
            SaveProfileService.LoadOrCreate(index);
            bootstrap?.ApplyToPlayer();
            Debug.Log($"[SaveProfileDebug] Loaded profile {index} from {SaveProfileService.GetProfilePath(index)}");
        }
    }
}
