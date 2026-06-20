using UnityEngine;

namespace FPSGame.Weapons
{
    [CreateAssetMenu(fileName = "MeleeWeapon", menuName = "FPSGame/Weapons/Melee")]
    public class MeleeWeaponData : WeaponData
    {
        [Header("Melee")]
        public float activeTime = 0.01f;
        public Vector3 hitboxSize = new Vector3(1f, 1f, 1.5f);
        public Vector3 hitboxOffset = new Vector3(0f, 0f, 1f);

        private void OnEnable() => category = WeaponCategory.Melee;
    }
}
