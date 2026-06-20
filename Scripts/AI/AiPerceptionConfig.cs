using UnityEngine;

namespace FPSGame.AI
{
    [CreateAssetMenu(fileName = "AiPerceptionConfig", menuName = "FPSGame/AI Perception Config")]
    public class AiPerceptionConfig : ScriptableObject
    {
        [Header("Sight")]
        public float sightRange = 25f;
        [Range(10f, 360f)] public float fieldOfViewDegrees = 110f;
        public LayerMask lineOfSightMask = ~0;

        [Header("Hearing")]
        public float hearingRange = 18f;
        public float gunshotMemorySeconds = 1.5f;

        [Header("Memory")]
        public float loseTargetDelay = 2.5f;
    }
}
