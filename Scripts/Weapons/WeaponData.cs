using UnityEngine;

namespace FPSGame.Weapons
{
    public abstract class WeaponData : ScriptableObject
    {
        [Header("Identity")]
        public string weaponId;
        public string displayName;
        public WeaponCategory category;

        [Header("Combat")]
        public float damage = 10f;
        public float attackCooldown = 0.1f;

        [Header("Animation")]
        public RuntimeAnimatorController animatorOverride;
    }
}
