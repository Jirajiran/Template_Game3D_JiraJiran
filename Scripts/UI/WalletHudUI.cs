using FPSGame.Core;
using UnityEngine;
using UnityEngine.UI;

namespace FPSGame.UI
{
    public class WalletHudUI : MonoBehaviour
    {
        [SerializeField] private Text valueText;
        [SerializeField] private string prefix = "$ ";

        private PlayerWallet wallet;

        public void Bind(PlayerWallet target)
        {
            Unbind();

            wallet = target;
            if (wallet == null)
                return;

            wallet.OnBalanceChanged += HandleBalanceChanged;
            HandleBalanceChanged(wallet.Balance);
        }

        private void OnDestroy() => Unbind();

        private void HandleBalanceChanged(int balance)
        {
            if (valueText != null)
                valueText.text = prefix + balance;
        }

        private void Unbind()
        {
            if (wallet != null)
                wallet.OnBalanceChanged -= HandleBalanceChanged;

            wallet = null;
        }
    }
}
