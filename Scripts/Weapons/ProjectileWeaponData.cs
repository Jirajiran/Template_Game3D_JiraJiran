using UnityEngine;

namespace FPSGame.Weapons
{
    [CreateAssetMenu(fileName = "ProjectileWeapon", menuName = "FPSGame/Weapons/Projectile")]
    public class ProjectileWeaponData : RangedWeaponData
    {
        [Header("Projectile")]
        public GameObject projectilePrefab;
        public float projectileSpeed = 100f;
        [Tooltip("Gravity applied by RaycastProjectile (downward m/s²).")]
        public float projectileGravity = 9.81f;
        [Tooltip("Destroy projectile if it travels this long without hitting.")]
        public float projectileMaxLifetime = 5f;
        [Tooltip("Optional AOE on impact (not wired yet).")]
        public float explosionRadius;

        private void OnEnable() => category = WeaponCategory.Projectile;
    }
}
