using System;

namespace FPSGame.Core
{
    /// <summary>
    /// Freezes gameplay time (Time.timeScale). UI input still runs in Update.
    /// </summary>
    public static class GameplayPauseService
    {
        private static int pauseDepth;

        public static bool IsPaused => pauseDepth > 0;

        public static event Action<bool> OnPauseChanged;

        public static void PushPause()
        {
            pauseDepth++;
            Apply();
        }

        public static void PopPause()
        {
            pauseDepth = Math.Max(0, pauseDepth - 1);
            Apply();
        }

        public static void ForceResume()
        {
            pauseDepth = 0;
            Apply();
        }

        private static void Apply()
        {
            UnityEngine.Time.timeScale = IsPaused ? 0f : 1f;
            OnPauseChanged?.Invoke(IsPaused);
        }
    }
}
