using FPSGame.Core;
using FPSGame.Weapons;
using UnityEngine;

namespace FPSGame.Save
{
    /// <summary>
    /// Maps weaponId / characterId strings to ScriptableObject assets for loadout apply.
    /// </summary>
    [CreateAssetMenu(fileName = "GameContentRegistry", menuName = "FPSGame/Save/Content Registry")]
    public class GameContentRegistry : ScriptableObject
    {
        [SerializeField] private CharacterStats[] heroes;
        [SerializeField] private WeaponData[] weapons;

        public CharacterStats[] Heroes => heroes;
        public WeaponData[] Weapons => weapons;

        public CharacterStats GetHero(string heroId)
        {
            if (string.IsNullOrEmpty(heroId) || heroes == null)
                return null;

            for (int i = 0; i < heroes.Length; i++)
            {
                if (heroes[i] != null && heroes[i].characterId == heroId)
                    return heroes[i];
            }

            return null;
        }

        public WeaponData GetWeapon(string weaponId)
        {
            if (string.IsNullOrEmpty(weaponId) || weapons == null)
                return null;

            for (int i = 0; i < weapons.Length; i++)
            {
                if (weapons[i] != null && weapons[i].weaponId == weaponId)
                    return weapons[i];
            }

            return null;
        }
    }
}
