using FPSGame.Core;
using UnityEngine;

namespace FPSGame.Weapons
{
    /// <summary>
    /// Short-lived melee trigger (~0.01s). One hit per IDamageable per swing via HitRegistry.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class MeleeHitbox : MonoBehaviour
    {
        private CharacterBase owner;
        private float damage;
        private int swingId;

        public void Initialize(CharacterBase ownerCharacter, float damageAmount, Vector3 size, int swingSequenceId)
        {
            owner = ownerCharacter;
            damage = damageAmount;
            swingId = swingSequenceId;

            var box = GetComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = size;
            box.center = Vector3.zero;

            if (GetComponent<Rigidbody>() == null)
            {
                var rb = gameObject.AddComponent<Rigidbody>();
                rb.isKinematic = true;
                rb.useGravity = false;
            }
        }

        private void OnTriggerEnter(Collider other) => TryHit(other);

        private void OnTriggerStay(Collider other) => TryHit(other);

        private void TryHit(Collider other)
        {
            if (owner == null || swingId <= 0)
                return;

            if (!HitRegistry.TryGetDamageable(other, out IDamageable target))
                return;

            if (target is CharacterBase unit && unit == owner)
                return;

            HitRegistry.TryApplyMeleeHit(swingId, target, damage, owner);
        }

        private void OnDestroy() => HitRegistry.ReleaseMeleeSwing(swingId);
    }
}
