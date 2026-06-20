using UnityEngine;

namespace FPSGame.AI
{
    /// <summary>
    /// Activates spawn points when the player enters this trigger volume.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class EnemySpawnTrigger : MonoBehaviour
    {
        [SerializeField] private EnemySpawnPoint[] spawnPoints;
        [SerializeField] private bool triggerOnce = true;

        private bool hasTriggered;

        private void Awake()
        {
            var col = GetComponent<Collider>();
            col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (triggerOnce && hasTriggered)
                return;

            if (!other.CompareTag("Player"))
                return;

            hasTriggered = true;
            ActivateSpawnPoints();
        }

        public void ActivateSpawnPoints()
        {
            if (spawnPoints == null)
                return;

            for (int i = 0; i < spawnPoints.Length; i++)
            {
                if (spawnPoints[i] != null)
                    spawnPoints[i].BeginSpawn();
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.4f, 0f, 0.35f);
            var box = GetComponent<BoxCollider>();
            if (box != null)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(box.center, box.size);
            }
        }
#endif
    }
}
