using UnityEngine;

namespace FPSGame.AI
{
    [CreateAssetMenu(fileName = "EnemyWaveSequence", menuName = "FPSGame/AI/Enemy Wave Sequence")]
    public class EnemyWaveSequence : ScriptableObject
    {
        [Tooltip("Ordered spawn steps executed when a spawn point is triggered.")]
        public EnemySpawnStep[] steps;

        [Tooltip("If true, each spawn point runs this sequence only once.")]
        public bool runOncePerSpawnPoint = true;
    }
}
