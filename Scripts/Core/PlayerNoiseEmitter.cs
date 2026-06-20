using UnityEngine;

namespace FPSGame.Core
{
    /// <summary>
    /// Tracks sprint and gunshot noise for AI hearing. Place on Player.
    /// </summary>
    public class PlayerNoiseEmitter : MonoBehaviour
    {
        private static readonly System.Collections.Generic.Dictionary<int, float> GunshotTimes =
            new System.Collections.Generic.Dictionary<int, float>();

        private static readonly System.Collections.Generic.Dictionary<int, bool> SprintingStates =
            new System.Collections.Generic.Dictionary<int, bool>();

        public static bool IsSprinting(GameObject source)
        {
            if (source == null)
                return false;

            return SprintingStates.TryGetValue(source.GetInstanceID(), out bool sprinting) && sprinting;
        }

        public static float GetLastGunshotTime(GameObject source)
        {
            if (source == null)
                return float.NegativeInfinity;

            return GunshotTimes.TryGetValue(source.GetInstanceID(), out float time) ? time : float.NegativeInfinity;
        }

        public static void SetSprinting(GameObject source, bool sprinting)
        {
            if (source == null)
                return;

            SprintingStates[source.GetInstanceID()] = sprinting;
        }

        public static void RegisterGunshot(GameObject source)
        {
            if (source == null)
                return;

            GunshotTimes[source.GetInstanceID()] = Time.time;
        }
    }
}
