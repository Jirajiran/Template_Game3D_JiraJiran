using System.Collections.Generic;
using FPSGame.Save;
using FPSGame.Weapons;
using UnityEngine;
using UnityEngine.UI;

namespace FPSGame.UI
{
    /// <summary>
    /// Loadout panel: pick hero, drag weapons into 3 slots, save to active profile JSON.
    /// </summary>
    public class LoadoutPanelController : MonoBehaviour
    {
        [SerializeField] private SaveStartPack startPack;
        [SerializeField] private GameContentRegistry contentRegistry;
        [SerializeField] private Transform heroListContent;
        [SerializeField] private Transform weaponListContent;
        [SerializeField] private LoadoutItemView loadoutItemPrefab;
        [SerializeField] private LoadoutItemView[] weaponSlotViews = new LoadoutItemView[3];
        [SerializeField] private LoadoutDropTarget[] weaponSlotDropTargets = new LoadoutDropTarget[3];
        [SerializeField] private LoadoutDropTarget unequipDropTarget;
        [SerializeField] private Text selectedHeroLabel;
        [SerializeField] private Text statusText;

        private readonly List<LoadoutItemView> spawnedHeroItems = new List<LoadoutItemView>();
        private readonly List<LoadoutItemView> spawnedWeaponItems = new List<LoadoutItemView>();

        private void Awake()
        {
            if (startPack != null)
                SaveProfileService.Configure(startPack);

            WireDropTargets();
        }

        private void OnEnable() => RefreshAll();

        private void WireDropTargets()
        {
            for (int i = 0; i < weaponSlotDropTargets.Length; i++)
            {
                if (weaponSlotDropTargets[i] != null)
                    weaponSlotDropTargets[i].Configure(this, LoadoutDropZone.WeaponSlot, i);
            }

            if (unequipDropTarget != null)
                unequipDropTarget.Configure(this, LoadoutDropZone.UnequipPool);
        }

        public void RefreshAll()
        {
            if (!SaveProfileService.HasActiveProfile)
            {
                SetStatus("No active profile — select a profile first.");
                ClearLists();
                RefreshSlots();
                RefreshSelectedHeroLabel();
                return;
            }

            if (contentRegistry == null)
            {
                SetStatus("Missing GameContentRegistry reference.");
                return;
            }

            RebuildHeroList();
            RebuildWeaponList();
            RefreshSlots();
            RefreshSelectedHeroLabel();
            SetStatus("Drag weapons into slots. Changes save automatically.");
        }

        public void HandleDrop(LoadoutDropZone zone, int slotIndex)
        {
            if (!SaveProfileService.HasActiveProfile || !LoadoutDragSession.IsActive)
                return;

            string weaponId = LoadoutDragSession.ItemId;
            if (string.IsNullOrEmpty(weaponId) || !SaveProfileService.IsWeaponUnlocked(weaponId))
                return;

            switch (zone)
            {
                case LoadoutDropZone.WeaponSlot:
                    if (slotIndex < 0 || slotIndex >= SaveProfileService.LoadoutSlotCount)
                        return;

                    if (LoadoutDragSession.Kind == LoadoutDragKind.EquippedWeapon &&
                        LoadoutDragSession.SourceSlot == slotIndex)
                        return;

                    AssignWeaponToSlot(slotIndex, weaponId, LoadoutDragSession.SourceSlot);
                    break;

                case LoadoutDropZone.UnequipPool:
                    if (LoadoutDragSession.Kind != LoadoutDragKind.EquippedWeapon)
                        return;

                    ClearSlot(LoadoutDragSession.SourceSlot);
                    break;
            }

            RefreshAll();
        }

        private void SelectHero(string heroId)
        {
            if (!SaveProfileService.IsHeroUnlocked(heroId))
                return;

            SaveProfileService.SetSelectedHero(heroId);
            RefreshAll();
        }

        private void AssignWeaponToSlot(int targetSlot, string weaponId, int sourceSlot)
        {
            string displacedId = SaveProfileService.GetLoadoutWeaponId(targetSlot);

            for (int i = 0; i < SaveProfileService.LoadoutSlotCount; i++)
            {
                if (i != targetSlot && SaveProfileService.GetLoadoutWeaponId(i) == weaponId)
                    SaveProfileService.SetLoadoutWeapon(i, string.Empty, saveImmediately: false);
            }

            SaveProfileService.SetLoadoutWeapon(targetSlot, weaponId, saveImmediately: false);

            if (sourceSlot >= 0 && sourceSlot < SaveProfileService.LoadoutSlotCount && sourceSlot != targetSlot)
            {
                string swapId = displacedId;
                if (!string.IsNullOrEmpty(swapId) && SaveProfileService.IsWeaponUnlocked(swapId))
                    SaveProfileService.SetLoadoutWeapon(sourceSlot, swapId, saveImmediately: false);
                else
                    SaveProfileService.SetLoadoutWeapon(sourceSlot, string.Empty, saveImmediately: false);
            }

            SaveProfileService.Save();
        }

        private void ClearSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= SaveProfileService.LoadoutSlotCount)
                return;

            SaveProfileService.SetLoadoutWeapon(slotIndex, string.Empty);
        }

        private void RebuildHeroList()
        {
            ClearSpawned(spawnedHeroItems, heroListContent);

            if (contentRegistry.Heroes == null)
                return;

            string selectedId = SaveProfileService.Data?.selectedHeroId ?? string.Empty;

            for (int i = 0; i < contentRegistry.Heroes.Length; i++)
            {
                var hero = contentRegistry.Heroes[i];
                if (hero == null || string.IsNullOrEmpty(hero.characterId))
                    continue;

                if (!SaveProfileService.IsHeroUnlocked(hero.characterId))
                    continue;

                var item = InstantiateItem(heroListContent);
                item.BindHero(hero.characterId, FormatHeroLabel(hero.characterId),
                    hero.characterId == selectedId, SelectHero);
                spawnedHeroItems.Add(item);
            }
        }

        private void RebuildWeaponList()
        {
            ClearSpawned(spawnedWeaponItems, weaponListContent);

            if (contentRegistry.Weapons == null)
                return;

            for (int i = 0; i < contentRegistry.Weapons.Length; i++)
            {
                var weapon = contentRegistry.Weapons[i];
                if (weapon == null || string.IsNullOrEmpty(weapon.weaponId))
                    continue;

                if (!SaveProfileService.IsWeaponUnlocked(weapon.weaponId))
                    continue;

                var item = InstantiateItem(weaponListContent);
                item.BindWeapon(weapon.weaponId, FormatWeaponLabel(weapon));
                EnsureDragHandle(item);
                spawnedWeaponItems.Add(item);
            }
        }

        private void RefreshSlots()
        {
            for (int i = 0; i < weaponSlotViews.Length && i < SaveProfileService.LoadoutSlotCount; i++)
            {
                var slotView = weaponSlotViews[i];
                if (slotView == null)
                    continue;

                string weaponId = SaveProfileService.GetLoadoutWeaponId(i);
                bool empty = string.IsNullOrEmpty(weaponId);
                string label = empty
                    ? $"Slot {i + 1} (empty)"
                    : FormatWeaponLabel(contentRegistry?.GetWeapon(weaponId), weaponId);

                slotView.BindEquippedWeapon(i, weaponId, label, empty);
                EnsureDragHandle(slotView);
            }
        }

        private void RefreshSelectedHeroLabel()
        {
            if (selectedHeroLabel == null)
                return;

            string heroId = SaveProfileService.Data?.selectedHeroId ?? string.Empty;
            selectedHeroLabel.text = string.IsNullOrEmpty(heroId)
                ? "Selected hero: —"
                : $"Selected hero: {FormatHeroLabel(heroId)}";
        }

        private LoadoutItemView InstantiateItem(Transform parent)
        {
            if (loadoutItemPrefab == null || parent == null)
                return null;

            return Instantiate(loadoutItemPrefab, parent);
        }

        private static void EnsureDragHandle(LoadoutItemView item)
        {
            if (item == null || !item.CanDrag)
                return;

            if (item.GetComponent<LoadoutDragHandle>() == null)
                item.gameObject.AddComponent<LoadoutDragHandle>();
        }

        private static void ClearSpawned(List<LoadoutItemView> list, Transform parent)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] != null)
                    Destroy(list[i].gameObject);
            }

            list.Clear();

            if (parent == null)
                return;

            for (int i = parent.childCount - 1; i >= 0; i--)
                Destroy(parent.GetChild(i).gameObject);
        }

        private void ClearLists()
        {
            ClearSpawned(spawnedHeroItems, heroListContent);
            ClearSpawned(spawnedWeaponItems, weaponListContent);
        }

        private void SetStatus(string message)
        {
            if (statusText != null)
                statusText.text = message;
        }

        private static string FormatHeroLabel(string heroId) =>
            string.IsNullOrEmpty(heroId) ? "—" : heroId.Replace('_', ' ');

        private static string FormatWeaponLabel(WeaponData weapon) =>
            weapon == null ? "—" : FormatWeaponLabel(weapon, weapon.weaponId);

        private static string FormatWeaponLabel(WeaponData weapon, string fallbackId)
        {
            if (weapon != null && !string.IsNullOrEmpty(weapon.displayName))
                return weapon.displayName;

            return string.IsNullOrEmpty(fallbackId) ? "—" : fallbackId.Replace('_', ' ');
        }
    }
}
