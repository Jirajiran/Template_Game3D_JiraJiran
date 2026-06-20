using UnityEngine;

namespace FPSGame.Weapons
{
    [CreateAssetMenu(fileName = "HitscanWeapon", menuName = "FPSGame/Weapons/Hitscan")]
    public class HitscanWeaponData : RangedWeaponData
    {
        [Header("Hitscan")]
        public float baseHitDelay = 0.01f;
        public float extraDelayPer100m = 0.01f;
        public float referenceRangeMeters = 100f;
        public float maxRange = 300f;
        public int penetration = 0;

        [Header("VFX")]
        public GameObject bulletHolePrefab;

        private void OnEnable() => category = WeaponCategory.Hitscan;
    }
}
