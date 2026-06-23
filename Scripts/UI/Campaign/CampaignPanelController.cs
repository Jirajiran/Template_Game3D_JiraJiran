using System.Collections.Generic;
using FPSGame.Core;
using FPSGame.Save;
using UnityEngine;
using UnityEngine.UI;

namespace FPSGame.UI
{
    /// <summary>
    /// Campaign list (1–4) and per-campaign level list with linear unlock + secret stage.
    /// </summary>
    public class CampaignPanelController : MonoBehaviour
    {
        [SerializeField] private GameObject campaignListView;
        [SerializeField] private GameObject levelListView;
        [SerializeField] private Text campaignListTitle;
        [SerializeField] private Text levelListTitle;
        [SerializeField] private Text statusText;
        [SerializeField] private Button[] campaignButtons = new Button[4];
        [SerializeField] private Button levelListBackButton;
        [SerializeField] private Transform levelListContent;
        [SerializeField] private Button levelButtonPrefab;

        private readonly List<Button> spawnedLevelButtons = new List<Button>();
        private int currentCampaign;

        private void Awake()
        {
            BindCampaignButtons();
            if (levelListBackButton != null)
            {
                levelListBackButton.onClick.RemoveAllListeners();
                levelListBackButton.onClick.AddListener(ShowCampaignList);
            }
        }

        private void OnEnable()
        {
            if (MenuFlowState.TryConsumeCampaignLevelList(out int campaign))
                ShowLevelList(campaign);
            else
                ShowCampaignList();
        }

        public bool TryHandleSubBack()
        {
            if (levelListView == null || !levelListView.activeSelf)
                return false;

            ShowCampaignList();
            return true;
        }

        public void ShowCampaignList()
        {
            CampaignSessionState.Clear();
            currentCampaign = 0;
            SetViewVisible(campaignListView, true);
            SetViewVisible(levelListView, false);
            SetStatus("Select a campaign.");
        }

        public void ShowLevelList(int campaign)
        {
            currentCampaign = Mathf.Clamp(campaign, 1, CampaignKeys.CampaignCount);
            SetViewVisible(campaignListView, false);
            SetViewVisible(levelListView, true);

            if (levelListTitle != null)
                levelListTitle.text = $"Campaign {currentCampaign} — Levels";

            RebuildLevelButtons();
            SetStatus("Unlocked levels are selectable. Locked levels require linear progress.");
        }

        private void BindCampaignButtons()
        {
            for (int i = 0; i < campaignButtons.Length; i++)
            {
                int campaign = i + 1;
                Button button = campaignButtons[i];
                if (button == null)
                    continue;

                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => ShowLevelList(campaign));

                var label = button.GetComponentInChildren<Text>();
                if (label != null)
                    label.text = $"Campaign {campaign}";
            }
        }

        private void RebuildLevelButtons()
        {
            ClearLevelButtons();

            if (levelListContent == null || levelButtonPrefab == null)
                return;

            for (int level = 1; level <= CampaignKeys.LevelsPerCampaign; level++)
                CreateLevelButton(level, isSecret: false);

            if (SaveProfileService.IsSecretStageUnlocked)
                CreateLevelButton(0, isSecret: true);
        }

        private void CreateLevelButton(int level, bool isSecret)
        {
            string levelKey = isSecret
                ? CampaignKeys.SecretStage(currentCampaign)
                : CampaignKeys.Level(currentCampaign, level);

            bool unlocked = isSecret
                ? SaveProfileService.IsSecretStageAvailable(currentCampaign)
                : SaveProfileService.IsLevelUnlocked(levelKey);

            string label = isSecret
                ? (unlocked ? "Secret Stage" : "Secret Stage (Locked)")
                : unlocked ? $"Level {level}" : $"Level {level} (Locked)";

            var button = Instantiate(levelButtonPrefab, levelListContent);
            button.gameObject.SetActive(true);
            button.interactable = unlocked;

            var text = button.GetComponentInChildren<Text>();
            if (text != null)
                text.text = label;

            var image = button.GetComponent<Image>();
            if (image != null)
                image.color = unlocked
                    ? new Color(0.2f, 0.28f, 0.2f, 0.95f)
                    : new Color(0.15f, 0.15f, 0.15f, 0.75f);

            if (unlocked)
            {
                if (isSecret)
                    button.onClick.AddListener(() => LaunchSecretStage(currentCampaign));
                else
                {
                    int capturedLevel = level;
                    button.onClick.AddListener(() => LaunchLevel(currentCampaign, capturedLevel));
                }
            }

            spawnedLevelButtons.Add(button);
        }

        private void LaunchLevel(int campaign, int level)
        {
            string key = CampaignKeys.Level(campaign, level);
            if (!SaveProfileService.IsLevelUnlocked(key))
                return;

            CampaignSessionState.BeginCampaignLevel(campaign, level);
            MenuFlowState.RequestCampaignLevelList(campaign);
            GameSceneFlow.LoadCampaignLevel(campaign, level);
        }

        private void LaunchSecretStage(int campaign)
        {
            if (!SaveProfileService.IsSecretStageAvailable(campaign))
                return;

            CampaignSessionState.BeginSecretStage(campaign);
            MenuFlowState.RequestCampaignLevelList(campaign);
            GameSceneFlow.LoadSecretStage(campaign);
        }

        private void ClearLevelButtons()
        {
            for (int i = 0; i < spawnedLevelButtons.Count; i++)
            {
                if (spawnedLevelButtons[i] != null)
                    Destroy(spawnedLevelButtons[i].gameObject);
            }

            spawnedLevelButtons.Clear();

            if (levelListContent == null)
                return;

            for (int i = levelListContent.childCount - 1; i >= 0; i--)
                Destroy(levelListContent.GetChild(i).gameObject);
        }

        private void SetViewVisible(GameObject view, bool visible)
        {
            if (view != null && view.activeSelf != visible)
                view.SetActive(visible);
        }

        private void SetStatus(string message)
        {
            if (statusText != null)
                statusText.text = message;
        }
    }
}
