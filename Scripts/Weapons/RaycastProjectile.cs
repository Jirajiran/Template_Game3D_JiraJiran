using FPSGame.Core;
using UnityEngine;

namespace FPSGame.Weapons
{
    /// <summary>
    /// Frame-by-frame projectile using segment Raycasts to prevent tunneling through walls/units.
    /// Attach to a projectile prefab; WeaponHandler spawns and calls Launch.
    /// </summary>
    public class RaycastProjectile : MonoBehaviour
    {
        [SerializeField] private float speed = 100f;
        [SerializeField] private float gravity = 9.81f;
        [SerializeField] private LayerMask hitLayers = ~0;
        [SerializeField] private float maxLifetime = 5f;
        [SerializeField] private GameObject impactEffectPrefab;

        private Vector3 velocity;
        private CharacterBase instigator;
        private float damage;
        private float spawnTime;
        private bool launched;

        /// <summary>
        /// Called by WeaponHandler after Instantiate. Weapon data overrides prefab defaults when &gt; 0.
        /// </summary>
        public void Launch(
            Vector3 direction,
            float launchSpeed,
            float damageAmount,
            CharacterBase fireInstigator,
            LayerMask mask,
            float gravityOverride = -1f,
            float lifetimeOverride = -1f)
        {
            float useSpeed = launchSpeed > 0f ? launchSpeed : speed;
            velocity = direction.normalized * useSpeed;
            damage = damageAmount;
            instigator = fireInstigator;
            hitLayers = mask;

            if (gravityOverride >= 0f)
                gravity = gravityOverride;

            if (lifetimeOverride > 0f)
                maxLifetime = lifetimeOverride;

            spawnTime = Time.time;
            launched = true;

            if (velocity.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(velocity.normalized);
        }

        private void Update()
        {
            if (!launched)
                return;

            if (Time.time - spawnTime >= maxLifetime)
            {
                Destroy(gameObject);
                return;
            }

            velocity += Vector3.down * gravity * Time.deltaTime;

            float distanceThisFrame = velocity.magnitude * Time.deltaTime;
            if (distanceThisFrame < 0.0001f)
                return;

            Vector3 moveDir = velocity.normalized;
            if (TryResolveSegmentHit(transform.position, moveDir, distanceThisFrame, out RaycastHit hit))
            {
                transform.position = hit.point;
                SpawnImpact(hit);
                HandleDamage(hit.collider);
                Destroy(gameObject);
                return;
            }

            transform.position += velocity * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(velocity.normalized);
        }

        private bool TryResolveSegmentHit(Vector3 origin, Vector3 direction, float maxDistance, out RaycastHit resolvedHit)
        {
            resolvedHit = default;
            float remaining = maxDistance;
            Vector3 rayOrigin = origin;
            const float skin = 0.01f;

            while (remaining > 0.0001f)
            {
                if (!Physics.Raycast(
                        rayOrigin,
                        direction,
                        out RaycastHit hit,
                        remaining,
                        hitLayers,
                        QueryTriggerInteraction.Ignore))
                {
                    return false;
                }

                if (IsInstigatorCollider(hit.collider))
                {
                    float advance = hit.distance + skin;
                    remaining -= advance;
                    rayOrigin = hit.point + direction * skin;
                    continue;
                }

                resolvedHit = hit;
                return true;
            }

            return false;
        }

        private bool IsInstigatorCollider(Collider col) =>
            instigator != null && col != null && col.transform.IsChildOf(instigator.transform);

        private void SpawnImpact(RaycastHit hit)
        {
            if (impactEffectPrefab == null)
                return;

            Instantiate(impactEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
        }

        private void HandleDamage(Collider col)
        {
            if (!HitRegistry.TryGetDamageable(col, out IDamageable damageable))
                return;

            HitRegistry.TryApplyDamage(damageable, damage, instigator);
        }
    }
}
