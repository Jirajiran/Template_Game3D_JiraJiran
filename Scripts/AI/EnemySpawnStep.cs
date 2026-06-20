using System;
using UnityEngine;

namespace FPSGame.AI
{
    [Serializable]
    public struct EnemySpawnStep
    {
        public GameObject enemyPrefab;
        [Min(1)] public int count;
        [Min(0f)] public float delayAfterPrevious;
    }
}
