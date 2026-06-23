using FPSGame.UI;
using UnityEngine;

namespace FPSGame.Save
{
    /// <summary>
    /// Prototype debug: F1–F3 load profile, F5 save, F6 unlock secret stage, F7 force win. Remove before shipping.
    /// </summary>
    public class SaveProfileDebug : MonoBehaviour
    {
        [SerializeField] private GameplayProfileBootstrap bootstrap;
        [SerializeField] private CampaignLevelWinController winController;

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
            if (Input.GetKeyDown(KeyCode.F7))
                ForceWinDebug();
        }

        private void ForceWinDebug()
        {
            if (winController == null)
                winController = FindObjectOfType<CampaignLevelWinController>();

            if (winController == null)
            {
                Debug.LogWarning("[SaveProfileDebug] CampaignLevelWinController not found — run Setup Campaign Win.");
                return;
            }

            winController.ForceWinForDebug();
            Debug.Log("[SaveProfileDebug] Forced level win (F7).");
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
