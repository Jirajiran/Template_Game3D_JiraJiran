using System;
using System.Collections.Generic;
using FPSGame.Core;
using UnityEngine;

namespace FPSGame.Weapons
{
    /// <summary>
    /// Runtime weapon logic on character. Assign WeaponData assets in Inspector.
    /// </summary>
    public class WeaponHandler : MonoBehaviour
    {
        private struct PendingHitscanHit
        {
            public IDamageable target;
            public float damage;
            public float applyTime;
        }

        [SerializeField] private Transform muzzle;
        [SerializeField] private WeaponData[] weaponSlots = new WeaponData[3];
        [SerializeField] private LayerMask hitMask = ~0;

        private readonly WeaponSlotState[] slotStates = new WeaponSlotState[3];
        private readonly List<PendingHitscanHit> pendingHits = new List<PendingHitscanHit>();
        private readonly List<HitscanTargetHit> hitscanBuffer = new List<HitscanTargetHit>(8);

        private int activeSlot;
        private int meleeSwingSequence;
        private float nextFireTime;
        private CharacterBase owner;
        private ReloadPhase reloadPhase = ReloadPhase.None;
        private float reloadPhaseEndTime;
        private int reloadingSlot = -1;

        public WeaponData CurrentWeapon =>
            weaponSlots != null && activeSlot >= 0 && activeSlot < weaponSlots.Length
                ? weaponSlots[activeSlot]
                : null;

        public int ActiveSlot => activeSlot;
        public bool IsReloading => reloadPhase != ReloadPhase.None;
        public ReloadPhase CurrentReloadPhase => reloadPhase;

        public event Action<int, int, int> OnAmmoChanged;
        public event Action<ReloadPhase> OnReloadPhaseChanged;
        public event Action<int> OnActiveSlotChanged;

        private void Awake() => owner = GetComponentInParent<CharacterBase>();

        private void Start() => InitializeAllSlots();

        private void Update()
        {
            TickReload();
            TickPendingHits();
        }

        public void SetActiveSlot(int slot)
        {
            if (weaponSlots == null || slot < 0 || slot >= weaponSlots.Length) return;
            if (slot == activeSlot) return;

            CancelReload();
            activeSlot = slot;
            NotifyAmmoChanged(activeSlot);
            OnActiveSlotChanged?.Invoke(activeSlot);
        }

        public void CycleActiveSlot(int direction)
        {
            if (weaponSlots == null || weaponSlots.Length == 0 || direction == 0) return;

            int count = weaponSlots.Length;
            int next = activeSlot;
            for (int i = 0; i < count; i++)
            {
                next = (next + direction + count) % count;
                if (weaponSlots[next] != null)
                {
                    SetActiveSlot(next);
                    return;
                }
            }
        }

        public bool TryReload()
        {
            var weapon = CurrentWeapon as RangedWeaponData;
            if (weapon == null || IsReloading) return false;

            ref WeaponSlotState state = ref slotStates[activeSlot];
            EnsureSlotInitialized(activeSlot, weapon);

            if (state.reserveMagazines <= 0) return false;
            if (state.clipAmmo >= weapon.clipSize) return false;

            BeginReload(activeSlot, weapon);
            return true;
        }

        public void TryPrimaryAction()
        {
            if (IsReloading)
                CancelReload();

            if (Time.time < nextFireTime) return;

            var weapon = CurrentWeapon;
            if (weapon == null) return;

            switch (weapon.category)
            {
                case WeaponCategory.Hitscan:
                    if (TryFireRanged((HitscanWeaponData)weapon))
                        FireHitscan((HitscanWeaponData)weapon);
                    break;
                case WeaponCategory.Melee:
                    StartMelee((MeleeWeaponData)weapon);
                    nextFireTime = Time.time + weapon.attackCooldown;
                    break;
                case WeaponCategory.Projectile:
                    if (TryFireRanged((ProjectileWeaponData)weapon))
                        FireProjectile((ProjectileWeaponData)weapon);
                    break;
            }
        }

        public int GetClipAmmo(int slot)
        {
            if (!IsValidSlot(slot)) return 0;
            return slotStates[slot].clipAmmo;
        }

        public int GetReserveMagazines(int slot)
        {
            if (!IsValidSlot(slot)) return 0;
            return slotStates[slot].reserveMagazines;
        }

        public WeaponData GetWeaponInSlot(int slot)
        {
            if (!IsValidSlot(slot)) return null;
            return weaponSlots[slot];
        }

        public void SetWeaponSlot(int slot, WeaponData weapon)
        {
            if (!IsValidSlot(slot))
                return;

            weaponSlots[slot] = weapon;

            if (weapon is RangedWeaponData ranged)
                EnsureSlotInitialized(slot, ranged);

            if (slot == activeSlot)
                NotifyAmmoChanged(slot);
        }

        private bool TryFireRanged(RangedWeaponData data)
        {
            EnsureSlotInitialized(activeSlot, data);
            ref WeaponSlotState state = ref slotStates[activeSlot];

            if (state.clipAmmo <= 0)
            {
                TryReload();
                return false;
            }

            state.clipAmmo--;
            NotifyAmmoChanged(activeSlot);
            nextFireTime = Time.time + data.attackCooldown;
            return true;
        }

        private void FireHitscan(HitscanWeaponData data)
        {
            if (muzzle == null)
                return;

            Vector3 origin = muzzle.position;
            Vector3 direction = ApplySpread(muzzle.forward, data.spreadDegrees);

            HitRegistry.ResolveHitscanRay(
                origin,
                direction,
                data,
                owner,
                hitMask,
                hitscanBuffer,
                out RaycastHit obstacleHit,
                out bool hasObstacle);

            for (int i = 0; i < hitscanBuffer.Count; i++)
            {
                var unitHit = hitscanBuffer[i];
                float delay = HitRegistry.CalculateHitscanDelay(data, unitHit.Distance);
                ScheduleHitscanDamage(unitHit.Target, data.damage, delay);
            }

            if (hasObstacle)
                SpawnBulletHole(data.bulletHolePrefab, obstacleHit);

            NotifyFriendlyGunshot();
        }

        private void FireProjectile(ProjectileWeaponData data)
        {
            if (muzzle == null || data.projectilePrefab == null) return;

            Vector3 direction = ApplySpread(muzzle.forward, data.spreadDegrees);
            var projectile = Instantiate(data.projectilePrefab, muzzle.position, Quaternion.LookRotation(direction));
            if (projectile.TryGetComponent<Rigidbody>(out var rb))
                rb.velocity = direction * data.projectileSpeed;

            NotifyFriendlyGunshot();
        }

        private void StartMelee(MeleeWeaponData data)
        {
            if (muzzle == null || owner == null)
                return;

            meleeSwingSequence++;
            int swingId = meleeSwingSequence;

            var hitboxGo = new GameObject($"MeleeHitbox_{swingId}");
            int hitboxLayer = LayerMask.NameToLayer("Hitbox");
            hitboxGo.layer = hitboxLayer >= 0 ? hitboxLayer : gameObject.layer;

            hitboxGo.transform.SetPositionAndRotation(
                muzzle.TransformPoint(data.hitboxOffset),
                muzzle.rotation);

            var hitbox = hitboxGo.AddComponent<MeleeHitbox>();
            hitbox.Initialize(owner, data.damage, data.hitboxSize, swingId);
            Destroy(hitboxGo, Mathf.Max(0.01f, data.activeTime));
        }

        private void BeginReload(int slot, RangedWeaponData data)
        {
            reloadingSlot = slot;
            slotStates[slot].clipAmmo = 0;
            NotifyAmmoChanged(slot);

            reloadPhase = ReloadPhase.Eject;
            reloadPhaseEndTime = Time.time + data.reloadPhaseDuration;
            OnReloadPhaseChanged?.Invoke(reloadPhase);
        }

        private void TickReload()
        {
            if (reloadPhase == ReloadPhase.None || reloadingSlot < 0)
                return;

            if (Time.time < reloadPhaseEndTime)
                return;

            var weapon = weaponSlots[reloadingSlot] as RangedWeaponData;
            if (weapon == null)
            {
                CancelReload();
                return;
            }

            switch (reloadPhase)
            {
                case ReloadPhase.Eject:
                    reloadPhase = ReloadPhase.Insert;
                    reloadPhaseEndTime = Time.time + weapon.reloadPhaseDuration;
                    OnReloadPhaseChanged?.Invoke(reloadPhase);
                    break;
                case ReloadPhase.Insert:
                    reloadPhase = ReloadPhase.Chamber;
                    reloadPhaseEndTime = Time.time + weapon.reloadPhaseDuration;
                    OnReloadPhaseChanged?.Invoke(reloadPhase);
                    break;
                case ReloadPhase.Chamber:
                    CompleteReload(weapon);
                    break;
            }
        }

        private void CompleteReload(RangedWeaponData data)
        {
            int slot = reloadingSlot;
            ref WeaponSlotState state = ref slotStates[slot];
            state.clipAmmo = data.clipSize;
            state.reserveMagazines = Mathf.Max(0, state.reserveMagazines - 1);

            reloadPhase = ReloadPhase.None;
            reloadingSlot = -1;
            OnReloadPhaseChanged?.Invoke(reloadPhase);
            NotifyAmmoChanged(slot);
        }

        private void CancelReload()
        {
            if (reloadPhase == ReloadPhase.None)
                return;

            reloadPhase = ReloadPhase.None;
            reloadingSlot = -1;
            OnReloadPhaseChanged?.Invoke(ReloadPhase.None);
        }

        private void ScheduleHitscanDamage(IDamageable target, float damage, float delay)
        {
            pendingHits.Add(new PendingHitscanHit
            {
                target = target,
                damage = damage,
                applyTime = Time.time + delay
            });
        }

        private void TickPendingHits()
        {
            for (int i = pendingHits.Count - 1; i >= 0; i--)
            {
                if (Time.time < pendingHits[i].applyTime)
                    continue;

                var hit = pendingHits[i];
                pendingHits.RemoveAt(i);
                HitRegistry.TryApplyDamage(hit.target, hit.damage, owner);
            }
        }

        private void InitializeAllSlots()
        {
            if (weaponSlots == null) return;

            for (int i = 0; i < weaponSlots.Length && i < slotStates.Length; i++)
            {
                if (weaponSlots[i] is RangedWeaponData ranged)
                    EnsureSlotInitialized(i, ranged);
            }
        }

        private void EnsureSlotInitialized(int slot, RangedWeaponData data)
        {
            if (!IsValidSlot(slot) || slotStates[slot].initialized)
                return;

            slotStates[slot].InitFrom(data);
            NotifyAmmoChanged(slot);
        }

        private void NotifyAmmoChanged(int slot)
        {
            if (!IsValidSlot(slot)) return;
            OnAmmoChanged?.Invoke(slot, slotStates[slot].clipAmmo, slotStates[slot].reserveMagazines);
        }

        private bool IsValidSlot(int slot) =>
            weaponSlots != null && slot >= 0 && slot < weaponSlots.Length && slot < slotStates.Length;

        private static void SpawnBulletHole(GameObject prefab, RaycastHit hit)
        {
            if (prefab == null) return;
            Instantiate(prefab, hit.point, Quaternion.LookRotation(hit.normal));
        }

        private static Vector3 ApplySpread(Vector3 forward, float spreadDegrees)
        {
            if (spreadDegrees <= 0f) return forward;
            return Quaternion.Euler(
                UnityEngine.Random.Range(-spreadDegrees, spreadDegrees),
                UnityEngine.Random.Range(-spreadDegrees, spreadDegrees),
                0f) * forward;
        }

        private void NotifyFriendlyGunshot()
        {
            if (owner != null && owner.Faction == Faction.Friendly)
                PlayerNoiseEmitter.RegisterGunshot(owner.gameObject);
        }
    }
}
