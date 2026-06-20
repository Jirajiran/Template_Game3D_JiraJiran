using FPSGame.Weapons;
using UnityEngine;
using UnityEngine.UI;

namespace FPSGame.UI
{
    /// <summary>
    /// Shows weapon slots 1–3, highlights active slot, ammo for ranged weapons.
    /// </summary>
    public class WeaponSlotHudUI : MonoBehaviour
    {
        [System.Serializable]
        public struct SlotView
        {
            public Image background;
            public Text label;
        }

        [SerializeField] private SlotView[] slots = new SlotView[3];
        [SerializeField] private Color activeColor = new Color(1f, 0.85f, 0.2f, 0.9f);
        [SerializeField] private Color inactiveColor = new Color(0.15f, 0.15f, 0.15f, 0.75f);
        [SerializeField] private Color emptyColor = new Color(0.1f, 0.1f, 0.1f, 0.4f);

        private WeaponHandler weaponHandler;

        public void Bind(WeaponHandler handler)
        {
            Unbind();

            weaponHandler = handler;
            if (weaponHandler == null)
                return;

            weaponHandler.OnAmmoChanged += HandleAmmoChanged;
            weaponHandler.OnActiveSlotChanged += HandleActiveSlotChanged;
            RefreshAll();
        }

        private void OnDestroy() => Unbind();

        private void HandleAmmoChanged(int slot, int clip, int reserve) => RefreshSlot(slot);

        private void HandleActiveSlotChanged(int slot) => RefreshAll();

        private void RefreshAll()
        {
            if (weaponHandler == null)
                return;

            for (int i = 0; i < slots.Length; i++)
                RefreshSlot(i);
        }

        private void RefreshSlot(int index)
        {
            if (index < 0 || index >= slots.Length)
                return;

            var view = slots[index];
            if (view.label == null)
                return;

            var weapon = weaponHandler.GetWeaponInSlot(index);
            bool isActive = weaponHandler.ActiveSlot == index;

            if (weapon == null)
            {
                view.label.text = $"{index + 1}: —";
                if (view.background != null)
                    view.background.color = emptyColor;
                return;
            }

            string ammoText = string.Empty;
            if (weapon is RangedWeaponData)
            {
                int clip = weaponHandler.GetClipAmmo(index);
                int reserve = weaponHandler.GetReserveMagazines(index);
                ammoText = $" [{clip}|{reserve}]";
            }

            string name = string.IsNullOrEmpty(weapon.displayName) ? weapon.weaponId : weapon.displayName;
            view.label.text = $"{index + 1}: {name}{ammoText}";

            if (view.background != null)
                view.background.color = isActive ? activeColor : inactiveColor;
        }

        private void Unbind()
        {
            if (weaponHandler == null)
                return;

            weaponHandler.OnAmmoChanged -= HandleAmmoChanged;
            weaponHandler.OnActiveSlotChanged -= HandleActiveSlotChanged;
            weaponHandler = null;
        }
    }
}
