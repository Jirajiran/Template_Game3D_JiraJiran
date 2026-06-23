using FPSGame.Core;
using UnityEngine;
using UnityEngine.UI;

namespace FPSGame.UI
{
    /// <summary>
    /// Victory overlay shown after clearing a campaign level.
    /// </summary>
    public class GameplayVictoryUI : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Text messageText;
        [SerializeField] private Button backToCampaignButton;

        private bool isOpen;
        private PlayerController playerController;

        private void Awake()
        {
            CloseImmediate();

            if (backToCampaignButton != null)
            {
                backToCampaignButton.onClick.RemoveListener(BackToCampaign);
                backToCampaignButton.onClick.AddListener(BackToCampaign);
            }
        }

        private void Start()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerController = player.GetComponent<PlayerController>();
        }

        public void Show(string message)
        {
            if (isOpen)
                return;

            isOpen = true;

            if (panelRoot != null)
                panelRoot.SetActive(true);

            if (messageText != null)
                messageText.text = message;

            SetPlayerCursor(false);
        }

        public void BackToCampaign()
        {
            CloseImmediate();
            GameplayPauseService.ForceResume();

            if (CampaignSessionState.HasActiveCampaignContext)
                MenuFlowState.RequestCampaignLevelList(CampaignSessionState.ActiveCampaign);
            else
                MenuFlowState.RequestPlaySubPanel();

            GameSceneFlow.LoadMainMenu();
        }

        private void CloseImmediate()
        {
            isOpen = false;

            if (panelRoot != null)
                panelRoot.SetActive(false);

            SetPlayerCursor(true);
        }

        private void SetPlayerCursor(bool locked)
        {
            if (playerController == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                    playerController = player.GetComponent<PlayerController>();
            }

            playerController?.SetCursorLocked(locked);
        }
    }
}
