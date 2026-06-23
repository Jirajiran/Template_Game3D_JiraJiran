using System.Collections;
using FPSGame.Core;
using FPSGame.UI;
using UnityEngine;
using UnityEngine.AI;

namespace FPSGame.AI
{
    /// <summary>
    /// Spawns enemies from an EnemyWaveSequence at this transform. Patrol points = direct children named PatrolPoint_*.
    /// </summary>
    public class EnemySpawnPoint : MonoBehaviour
    {
        [SerializeField] private EnemyWaveSequence sequence;
        [SerializeField] private Transform[] patrolPoints;
        [SerializeField] private bool useChildPatrolPoints = true;

        private bool hasRun;
        private bool isSpawning;

        public bool HasRun => hasRun;
        public bool IsSpawning => isSpawning;

        public event System.Action<CharacterBase> EnemySpawned;

        public void BeginSpawn()
        {
            if (sequence == null || sequence.steps == null || sequence.steps.Length == 0)
            {
                Debug.LogWarning($"{name}: EnemyWaveSequence not assigned.", this);
                return;
            }

            if (sequence.runOncePerSpawnPoint && hasRun)
                return;

            if (isSpawning)
                return;

            StartCoroutine(SpawnRoutine());
        }

        private IEnumerator SpawnRoutine()
        {
            isSpawning = true;
            var points = ResolvePatrolPoints();

            for (int i = 0; i < sequence.steps.Length; i++)
            {
                EnemySpawnStep step = sequence.steps[i];
                if (i > 0 && step.delayAfterPrevious > 0f)
                    yield return new WaitForSeconds(step.delayAfterPrevious);

                if (step.enemyPrefab == null)
                    continue;

                int count = Mathf.Max(1, step.count);
                for (int c = 0; c < count; c++)
                {
                    SpawnEnemy(step.enemyPrefab, points);
                    if (c < count - 1)
                        yield return new WaitForSeconds(0.25f);
                }
            }

            hasRun = true;
            isSpawning = false;
        }

        private void SpawnEnemy(GameObject prefab, Transform[] points)
        {
            Vector3 spawnPos = transform.position;
            Quaternion spawnRot = transform.rotation;

            GameObject instance = Instantiate(prefab, spawnPos, spawnRot);
            instance.name = prefab.name;

            if (instance.TryGetComponent(out NavMeshAgent agent))
            {
                if (NavMesh.SamplePosition(spawnPos, out NavMeshHit hit, 3f, NavMesh.AllAreas))
                    agent.Warp(hit.position);
            }

            if (instance.TryGetComponent(out AIController controller) && points != null && points.Length > 0)
                controller.ConfigurePatrol(points);

            if (instance.TryGetComponent(out CharacterBase character))
                EnemySpawned?.Invoke(character);
        }

        private Transform[] ResolvePatrolPoints()
        {
            if (patrolPoints != null && patrolPoints.Length > 0 && !useChildPatrolPoints)
                return patrolPoints;

            if (!useChildPatrolPoints)
                return null;

            var children = new System.Collections.Generic.List<Transform>();
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child.name.StartsWith("PatrolPoint"))
                    children.Add(child);
            }

            return children.Count > 0 ? children.ToArray() : patrolPoints;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.6f);
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 2f);
        }
#endif
    }
}
