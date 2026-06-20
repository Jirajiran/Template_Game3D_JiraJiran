using UnityEngine;

namespace FPSGame.Weapons
{
    public abstract class RangedWeaponData : WeaponData
    {
        [Header("Ammo")]
        public int clipSize = 30;
        public int magazineCount = 3;

        [Header("Spread")]
        public float spreadDegrees = 1f;

        [Header("Reload")]
        public float reloadPhaseDuration = 0.2f;
    }
}
