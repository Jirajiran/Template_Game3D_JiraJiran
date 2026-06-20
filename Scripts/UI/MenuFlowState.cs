using UnityEngine;

namespace FPSGame.UI
{
    /// <summary>
    /// Remembers menu state when returning from gameplay (Phase 5 pause → menu).
    /// </summary>
    public static class MenuFlowState
    {
        public static bool OpenPlaySubPanelOnLoad { get; private set; }
        public static bool OpenCampaignLevelListOnLoad { get; private set; }
        public static int CampaignIndexOnLoad { get; private set; }

        public static void RequestPlaySubPanel()
        {
            OpenPlaySubPanelOnLoad = true;
            OpenCampaignLevelListOnLoad = false;
        }

        public static void RequestCampaignLevelList(int campaignIndex)
        {
            OpenCampaignLevelListOnLoad = true;
            CampaignIndexOnLoad = Mathf.Clamp(campaignIndex, 1, FPSGame.Save.CampaignKeys.CampaignCount);
            OpenPlaySubPanelOnLoad = false;
        }

        public static bool HasPendingCampaignRestore => OpenCampaignLevelListOnLoad;

        public static bool ConsumePlaySubPanelRequest()
        {
            if (!OpenPlaySubPanelOnLoad)
                return false;

            OpenPlaySubPanelOnLoad = false;
            return true;
        }

        public static bool TryConsumeCampaignLevelList(out int campaignIndex)
        {
            if (!OpenCampaignLevelListOnLoad)
            {
                campaignIndex = 0;
                return false;
            }

            OpenCampaignLevelListOnLoad = false;
            campaignIndex = CampaignIndexOnLoad;
            return true;
        }
    }
}
