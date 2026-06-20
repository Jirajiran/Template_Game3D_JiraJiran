using FPSGame.Core;
using UnityEngine;
using UnityEngine.UI;

namespace FPSGame.UI
{
    /// <summary>
    /// Pause menu: Resume, Restart scene, Back to Main Menu. Toggle via Escape or pause button.
    /// </summary>
    public class GameplayPauseUI : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button pauseToggleButton;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button exitToMenuButton;
        [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

        private bool isOpen;
        private PlayerController playerController;

        private void Awake()
        {
            CloseImmediate();
            Bind(resumeButton, Resume);
            Bind(restartButton, Restart);
            Bind(exitToMenuButton, ExitToMenu);
            Bind(pauseToggleButton, TogglePauseMenu);
        }

        private void Start()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerController = player.GetComponent<PlayerController>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(pauseKey))
                TogglePauseMenu();
        }

        public void TogglePauseMenu()
        {
            if (isOpen)
                Resume();
            else
                Open();
        }

        public void Open()
        {
            if (isOpen)
                return;

            isOpen = true;
            if (panelRoot != null)
                panelRoot.SetActive(true);

            GameplayPauseService.PushPause();
            SetPlayerCursor(false);
        }

        public void Resume()
        {
            if (!isOpen)
                return;

            CloseImmediate();
        }

        public void Restart()
        {
            CloseImmediate();
            GameSceneFlow.RestartCurrentScene();
        }

        public void ExitToMenu()
        {
            CloseImmediate();

            if (CampaignSessionState.HasActiveCampaignContext)
                MenuFlowState.RequestCampaignLevelList(CampaignSessionState.ActiveCampaign);
            else
                MenuFlowState.RequestPlaySubPanel();

            GameSceneFlow.LoadMainMenu();
        }

        private void CloseImmediate()
        {
            bool wasOpen = isOpen;
            isOpen = false;

            if (panelRoot != null)
                panelRoot.SetActive(false);

            if (wasOpen)
                GameplayPauseService.PopPause();

            SetPlayerCursor(true);
        }

        private void OnDestroy()
        {
            if (isOpen && GameplayPauseService.IsPaused)
                GameplayPauseService.PopPause();

            UnityEngine.Time.timeScale = 1f;
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

        private static void Bind(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null)
                return;

            button.onClick.RemoveListener(action);
            button.onClick.AddListener(action);
        }
    }
}
