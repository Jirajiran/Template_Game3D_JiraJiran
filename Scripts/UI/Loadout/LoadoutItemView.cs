using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FPSGame.UI
{
    public enum LoadoutItemKind
    {
        Hero,
        Weapon,
        EquippedWeapon
    }

    /// <summary>
    /// One hero or weapon row in the loadout grids / slots.
    /// </summary>
    public class LoadoutItemView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Text label;
        [SerializeField] private Image background;
        [SerializeField] private Color normalColor = new Color(0.2f, 0.2f, 0.2f, 0.95f);
        [SerializeField] private Color selectedColor = new Color(0.15f, 0.45f, 0.25f, 0.95f);
        [SerializeField] private Color emptyColor = new Color(0.12f, 0.12f, 0.12f, 0.75f);

        public LoadoutItemKind ItemKind { get; private set; }
        public string HeroId { get; private set; }
        public string WeaponId { get; private set; }
        public int EquippedSlotIndex { get; private set; } = -1;
        public bool CanDrag { get; private set; }
        public bool IsEquippedWeapon => ItemKind == LoadoutItemKind.EquippedWeapon;

        private Action<string> onHeroClicked;

        public void BindHero(string heroId, string displayText, bool selected, Action<string> clickHandler)
        {
            ItemKind = LoadoutItemKind.Hero;
            HeroId = heroId;
            WeaponId = string.Empty;
            EquippedSlotIndex = -1;
            CanDrag = false;
            onHeroClicked = clickHandler;

            SetLabel(displayText);
            SetBackground(selected ? selectedColor : normalColor);
        }

        public void BindWeapon(string weaponId, string displayText)
        {
            ItemKind = LoadoutItemKind.Weapon;
            HeroId = string.Empty;
            WeaponId = weaponId;
            EquippedSlotIndex = -1;
            CanDrag = true;
            onHeroClicked = null;

            SetLabel(displayText);
            SetBackground(normalColor);
        }

        public void BindEquippedWeapon(int slotIndex, string weaponId, string displayText, bool empty)
        {
            ItemKind = LoadoutItemKind.EquippedWeapon;
            HeroId = string.Empty;
            WeaponId = weaponId;
            EquippedSlotIndex = slotIndex;
            CanDrag = !empty;
            onHeroClicked = null;

            SetLabel(displayText);
            SetBackground(empty ? emptyColor : normalColor);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (ItemKind == LoadoutItemKind.Hero && !string.IsNullOrEmpty(HeroId))
                onHeroClicked?.Invoke(HeroId);
        }

        private void SetLabel(string text)
        {
            if (label != null)
                label.text = text;
        }

        private void SetBackground(Color color)
        {
            if (background != null)
                background.color = color;
        }
    }
}
