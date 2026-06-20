using FPSGame.Core;
using UnityEngine;
using UnityEngine.UI;

namespace FPSGame.UI
{
    /// <summary>
    /// Wires main menu buttons to MenuNavigator and scene actions.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private MenuNavigator navigator;
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button creditsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Button loadoutButton;
        [SerializeField] private Button campaignButton;
        [SerializeField] private Button prototypePlayButton;
        [SerializeField] private Button confirmQuitButton;
        [SerializeField] private Button cancelQuitButton;

        private void Awake()
        {
            if (navigator == null)
                navigator = GetComponent<MenuNavigator>();

            Bind(playButton, navigator.OpenPlayPanel);
            Bind(settingsButton, navigator.OpenSettingsPanel);
            Bind(creditsButton, navigator.OpenCreditsPanel);
            Bind(quitButton, navigator.OpenQuitConfirmPanel);
            Bind(loadoutButton, navigator.OpenLoadoutPanel);
            Bind(campaignButton, navigator.OpenCampaignPanel);
            Bind(prototypePlayButton, GameSceneFlow.LoadPrototypeGameplay);
            Bind(confirmQuitButton, QuitApplication);
            Bind(cancelQuitButton, navigator.GoBack);
        }

        private static void Bind(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null || action == null)
                return;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        private static void QuitApplication()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
