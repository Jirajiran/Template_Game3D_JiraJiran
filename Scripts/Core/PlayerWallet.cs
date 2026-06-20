using System;
using UnityEngine;

namespace FPSGame.Core
{
    /// <summary>
    /// Runtime wallet on player. Save system will sync this in Phase 9.
    /// </summary>
    public class PlayerWallet : MonoBehaviour
    {
        [SerializeField] private int startingBalance = 100;

        private int balance;

        public int Balance => balance;

        public event Action<int> OnBalanceChanged;

        private void Awake()
        {
            balance = Mathf.Max(0, startingBalance);
            OnBalanceChanged?.Invoke(balance);
        }

        public void SetBalance(int amount)
        {
            balance = Mathf.Max(0, amount);
            OnBalanceChanged?.Invoke(balance);
        }

        public void Add(int amount)
        {
            if (amount == 0) return;
            SetBalance(balance + amount);
        }
    }
}
