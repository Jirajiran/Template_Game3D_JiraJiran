using UnityEngine;

namespace FPSGame.Weapons
{
    [CreateAssetMenu(fileName = "ProjectileWeapon", menuName = "FPSGame/Weapons/Projectile")]
    public class ProjectileWeaponData : RangedWeaponData
    {
        [Header("Projectile")]
        public GameObject projectilePrefab;
        public float projectileSpeed = 40f;
        public float explosionRadius;

        private void OnEnable() => category = WeaponCategory.Projectile;
    }
}
