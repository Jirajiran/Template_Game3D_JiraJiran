using FPSGame.Core;
using FPSGame.Weapons;
using UnityEngine;

namespace FPSGame.Save
{
    /// <summary>
    /// Applies active profile loadout to the player when entering gameplay.
    /// </summary>
    public class GameplayProfileBootstrap : MonoBehaviour
    {
        [SerializeField] private SaveStartPack startPack;
        [SerializeField] private GameContentRegistry contentRegistry;
        [SerializeField] private int profileIndexOnStart = 1;
        [SerializeField] private bool applyOnStart = true;

        private void Awake()
        {
            if (startPack != null)
                SaveProfileService.Configure(startPack);
        }

        private void Start()
        {
            if (!applyOnStart)
                return;

            if (!SaveProfileService.HasActiveProfile &&
                profileIndexOnStart >= SaveProfileService.MinProfileIndex)
            {
                SaveProfileService.LoadOrCreate(profileIndexOnStart);
            }

            ApplyToPlayer();
        }

        public void ApplyToPlayer()
        {
            if (!SaveProfileService.HasActiveProfile)
                return;

            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
                return;

            ApplyHero(player);
            ApplyWeapons(player);
            ApplyWallet(player);
        }

        private void ApplyHero(GameObject player)
        {
            if (contentRegistry == null)
                return;

            var character = player.GetComponent<CharacterBase>();
            if (character == null)
                return;

            string heroId = SaveProfileService.Data.selectedHeroId;
            if (string.IsNullOrEmpty(heroId) || !SaveProfileService.IsHeroUnlocked(heroId))
                return;

            var heroStats = contentRegistry.GetHero(heroId);
            if (heroStats == null)
                return;

            character.ApplyStats(heroStats);
        }

        private void ApplyWeapons(GameObject player)
        {
            if (contentRegistry == null)
                return;

            var weaponHandler = player.GetComponentInChildren<WeaponHandler>();
            if (weaponHandler == null)
                return;

            for (int i = 0; i < SaveProfileService.LoadoutSlotCount; i++)
            {
                string weaponId = SaveProfileService.GetLoadoutWeaponId(i);
                if (string.IsNullOrEmpty(weaponId) || !SaveProfileService.IsWeaponUnlocked(weaponId))
                {
                    weaponHandler.SetWeaponSlot(i, null);
                    continue;
                }

                weaponHandler.SetWeaponSlot(i, contentRegistry.GetWeapon(weaponId));
            }

            weaponHandler.SetActiveSlot(0);
        }

        private static void ApplyWallet(GameObject player)
        {
            var wallet = player.GetComponent<PlayerWallet>();
            if (wallet != null)
                SaveProfileService.ApplyWalletTo(wallet);
        }
    }
}
