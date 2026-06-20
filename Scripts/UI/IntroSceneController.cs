using FPSGame.Core;
using UnityEngine;

namespace FPSGame.UI
{
    /// <summary>
    /// Intro scene: advance to profile selection on any key or click.
    /// </summary>
    public class IntroSceneController : MonoBehaviour
    {
        [SerializeField] private float minDisplaySeconds = 1f;

        private float shownAt;

        private void OnEnable() => shownAt = Time.unscaledTime;

        private void Update()
        {
            if (Time.unscaledTime - shownAt < minDisplaySeconds)
                return;

            if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
                GameSceneFlow.LoadProfileSelection();
        }

        public void SkipIntro() => GameSceneFlow.LoadProfileSelection();
    }
}
