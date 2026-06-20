using FPSGame.Core;
using FPSGame.Save;
using UnityEngine;
using UnityEngine.UI;

namespace FPSGame.UI
{
    /// <summary>
    /// Profile selection: load profile 1–3 JSON then open main menu.
    /// </summary>
    public class ProfileSelectionController : MonoBehaviour
    {
        [SerializeField] private SaveStartPack startPack;
        [SerializeField] private Button profile1Button;
        [SerializeField] private Button profile2Button;
        [SerializeField] private Button profile3Button;
        [SerializeField] private Text profile1Label;
        [SerializeField] private Text profile2Label;
        [SerializeField] private Text profile3Label;

        private void Awake()
        {
            if (startPack != null)
                SaveProfileService.Configure(startPack);

            RefreshLabels();
            Bind(profile1Button, 1);
            Bind(profile2Button, 2);
            Bind(profile3Button, 3);
        }

        private void RefreshLabels()
        {
            SetLabel(profile1Label, 1);
            SetLabel(profile2Label, 2);
            SetLabel(profile3Label, 3);
        }

        private static void SetLabel(Text label, int profileIndex)
        {
            if (label == null)
                return;

            bool exists = SaveProfileService.ProfileFileExists(profileIndex);
            label.text = exists ? $"Profile {profileIndex} (Continue)" : $"Profile {profileIndex} (New)";
        }

        private void Bind(Button button, int profileIndex)
        {
            if (button == null)
                return;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => SelectProfile(profileIndex));
        }

        private void SelectProfile(int profileIndex)
        {
            SaveProfileService.LoadOrCreate(profileIndex);
            GameSceneFlow.LoadMainMenu();
        }
    }
}
