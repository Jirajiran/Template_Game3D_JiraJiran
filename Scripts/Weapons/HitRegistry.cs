using System.Collections.Generic;
using FPSGame.Core;
using UnityEngine;

namespace FPSGame.Weapons
{
    /// <summary>
    /// Central hit rules: hitscan delay, unit-only penetration, melee anti-double-hit, damage apply.
    /// </summary>
    public static class HitRegistry
    {
        private static readonly Dictionary<int, HashSet<IDamageable>> MeleeHitsBySwing = new Dictionary<int, HashSet<IDamageable>>();

        /// <summary>
        /// Hitscan travel delay: base + extra per 100m beyond reference range.
        /// </summary>
        public static float CalculateHitscanDelay(HitscanWeaponData data, float distanceMeters)
        {
            if (data == null)
                return 0f;

            float delay = data.baseHitDelay;
            if (distanceMeters > data.referenceRangeMeters)
            {
                float extraBands = (distanceMeters - data.referenceRangeMeters) / 100f;
                delay += extraBands * data.extraDelayPer100m;
            }

            return Mathf.Max(0f, delay);
        }

        /// <summary>
        /// Resolves a hitscan ray: hostile units (penetration-limited) then first obstacle for bullet hole.
        /// </summary>
        public static void ResolveHitscanRay(
            Vector3 origin,
            Vector3 direction,
            HitscanWeaponData data,
            CharacterBase instigator,
            LayerMask hitMask,
            List<HitscanTargetHit> unitHits,
            out RaycastHit obstacleHit,
            out bool hasObstacle)
        {
            unitHits?.Clear();
            obstacleHit = default;
            hasObstacle = false;

            if (data == null)
                return;

            var hits = Physics.RaycastAll(origin, direction, data.maxRange, hitMask, QueryTriggerInteraction.Ignore);
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            int unitsRemaining = data.penetration + 1;

            for (int i = 0; i < hits.Length; i++)
            {
                ref RaycastHit hit = ref hits[i];
                var collider = hit.collider;

                if (IsOwnerCollider(collider, instigator))
                    continue;

                if (TryGetDamageable(collider, out IDamageable damageable))
                {
                    if (!IsValidHostileTarget(damageable, instigator))
                        continue;

                    unitHits?.Add(new HitscanTargetHit
                    {
                        Target = damageable,
                        Distance = hit.distance,
                        Point = hit.point
                    });

                    unitsRemaining--;
                    if (unitsRemaining <= 0)
                        return;

                    continue;
                }

                if (IsObstacle(collider, instigator))
                {
                    obstacleHit = hit;
                    hasObstacle = true;
                    return;
                }
            }
        }

        public static bool TryApplyDamage(IDamageable target, float amount, CharacterBase instigator)
        {
            if (target == null || !target.IsAlive || amount <= 0f)
                return false;

            if (instigator != null && target is CharacterBase unit && !instigator.IsHostileTo(unit))
                return false;

            target.TakeDamage(amount);
            return true;
        }

        public static bool TryApplyMeleeHit(int swingId, IDamageable target, float amount, CharacterBase instigator)
        {
            if (target == null || swingId <= 0)
                return false;

            if (!MeleeHitsBySwing.TryGetValue(swingId, out HashSet<IDamageable> hitSet))
            {
                hitSet = new HashSet<IDamageable>();
                MeleeHitsBySwing[swingId] = hitSet;
            }

            if (!hitSet.Add(target))
                return false;

            return TryApplyDamage(target, amount, instigator);
        }

        public static void ReleaseMeleeSwing(int swingId)
        {
            if (swingId <= 0)
                return;

            MeleeHitsBySwing.Remove(swingId);
        }

        public static bool TryGetDamageable(Collider col, out IDamageable damageable)
        {
            damageable = null;
            if (col == null)
                return false;

            var character = col.GetComponentInParent<CharacterBase>();
            if (character == null)
                return false;

            damageable = character;
            return true;
        }

        private static bool IsValidHostileTarget(IDamageable target, CharacterBase instigator)
        {
            if (target == null || !target.IsAlive || instigator == null)
                return false;

            if (target is CharacterBase unit)
                return instigator.IsHostileTo(unit);

            return instigator.Faction != target.Faction;
        }

        private static bool IsOwnerCollider(Collider col, CharacterBase instigator) =>
            instigator != null && col != null && col.transform.IsChildOf(instigator.transform);

        private static bool IsObstacle(Collider col, CharacterBase instigator)
        {
            if (col == null || IsOwnerCollider(col, instigator))
                return false;

            if (TryGetDamageable(col, out _))
                return false;

            return !col.isTrigger;
        }
    }

    public struct HitscanTargetHit
    {
        public IDamageable Target;
        public float Distance;
        public Vector3 Point;
    }
}
