using UnityEngine;
using UnityEngine.EventSystems;

namespace FPSGame.UI
{
    public enum LoadoutDropZone
    {
        WeaponSlot,
        UnequipPool
    }

    /// <summary>
    /// Drop target for weapon drag in the loadout panel.
    /// </summary>
    public class LoadoutDropTarget : MonoBehaviour, IDropHandler
    {
        [SerializeField] private LoadoutPanelController panel;
        [SerializeField] private LoadoutDropZone zone;
        [SerializeField] private int slotIndex;

        public void Configure(LoadoutPanelController owner, LoadoutDropZone dropZone, int slot = -1)
        {
            panel = owner;
            zone = dropZone;
            slotIndex = slot;
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (panel == null || !LoadoutDragSession.IsActive)
                return;

            panel.HandleDrop(zone, slotIndex);
        }
    }
}
