using FPSGame.Core;
using FPSGame.Weapons;
using UnityEngine;

namespace FPSGame.UI
{
    /// <summary>
    /// Binds gameplay HUD widgets to the local player.
    /// </summary>
    public class GameplayHudManager : MonoBehaviour
    {
        [SerializeField] private DualLayerHealthBarUI healthBar;
        [SerializeField] private WeaponSlotHudUI weaponSlotHud;
        [SerializeField] private WalletHudUI walletHud;
        [SerializeField] private bool bindPlayerOnStart = true;

        private void Start()
        {
            if (bindPlayerOnStart)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                Bind(player);
            }
        }

        public void Bind(GameObject player)
        {
            if (player == null)
                return;

            var character = player.GetComponent<CharacterBase>();
            var weaponHandler = player.GetComponentInChildren<WeaponHandler>();
            var wallet = player.GetComponent<PlayerWallet>();

            if (healthBar != null)
                healthBar.Bind(character);

            if (weaponSlotHud != null)
                weaponSlotHud.Bind(weaponHandler);

            if (walletHud != null)
                walletHud.Bind(wallet);
        }
    }
}
