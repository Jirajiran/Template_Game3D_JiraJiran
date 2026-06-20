using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FPSGame.UI
{
    /// <summary>
    /// Stack-based menu navigation with one global Back button.
    /// </summary>
    public class MenuNavigator : MonoBehaviour
    {
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject playPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject creditsPanel;
        [SerializeField] private GameObject quitConfirmPanel;
        [SerializeField] private GameObject loadoutPanel;
        [SerializeField] private GameObject campaignPanel;
        [SerializeField] private Button globalBackButton;
        [SerializeField] private CampaignPanelController campaignPanelController;

        private readonly Stack<GameObject> panelHistory = new Stack<GameObject>();
        private GameObject currentPanel;

        private void Awake()
        {
            if (globalBackButton != null)
            {
                globalBackButton.onClick.RemoveAllListeners();
                globalBackButton.onClick.AddListener(GoBack);
            }
        }

        private void OnEnable()
        {
            if (MenuFlowState.HasPendingCampaignRestore)
            {
                ResetToMainMenuThenOpen(playPanel);
                OpenPanel(campaignPanel);
            }
            else if (MenuFlowState.ConsumePlaySubPanelRequest())
                ResetToMainMenuThenOpen(playPanel);
            else
                ResetToMainMenu();
        }

        public void ResetToMainMenu()
        {
            panelHistory.Clear();
            HideAllPanels();

            currentPanel = mainPanel;
            SetActiveSafe(currentPanel, true);
            panelHistory.Push(currentPanel);
            UpdateBackButtonVisibility();
        }

        public void ResetToMainMenuThenOpen(GameObject targetPanel)
        {
            ResetToMainMenu();
            if (targetPanel != null && targetPanel != mainPanel)
                OpenPanel(targetPanel);
        }

        public void OpenPanel(GameObject targetPanel)
        {
            if (targetPanel == null || targetPanel == currentPanel)
                return;

            SetActiveSafe(currentPanel, false);
            currentPanel = targetPanel;
            SetActiveSafe(currentPanel, true);
            panelHistory.Push(currentPanel);
            UpdateBackButtonVisibility();
        }

        public void GoBack()
        {
            if (campaignPanelController == null && campaignPanel != null)
                campaignPanelController = campaignPanel.GetComponent<CampaignPanelController>();

            if (campaignPanelController != null && campaignPanelController.TryHandleSubBack())
                return;

            if (panelHistory.Count <= 1)
                return;

            GameObject popped = panelHistory.Pop();
            SetActiveSafe(popped, false);

            currentPanel = panelHistory.Peek();
            SetActiveSafe(currentPanel, true);
            UpdateBackButtonVisibility();
        }

        public void OpenPlayPanel() => OpenPanel(playPanel);
        public void OpenSettingsPanel() => OpenPanel(settingsPanel);
        public void OpenCreditsPanel() => OpenPanel(creditsPanel);
        public void OpenQuitConfirmPanel() => OpenPanel(quitConfirmPanel);
        public void OpenLoadoutPanel() => OpenPanel(loadoutPanel);
        public void OpenCampaignPanel() => OpenPanel(campaignPanel);

        private void HideAllPanels()
        {
            SetActiveSafe(mainPanel, false);
            SetActiveSafe(playPanel, false);
            SetActiveSafe(settingsPanel, false);
            SetActiveSafe(creditsPanel, false);
            SetActiveSafe(quitConfirmPanel, false);
            SetActiveSafe(loadoutPanel, false);
            SetActiveSafe(campaignPanel, false);
        }

        private void UpdateBackButtonVisibility()
        {
            if (globalBackButton != null)
                globalBackButton.gameObject.SetActive(currentPanel != mainPanel);
        }

        private static void SetActiveSafe(GameObject go, bool value)
        {
            if (go != null && go.activeSelf != value)
                go.SetActive(value);
        }
    }
}
