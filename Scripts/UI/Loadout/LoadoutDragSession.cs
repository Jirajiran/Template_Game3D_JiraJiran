namespace FPSGame.UI
{
    public enum LoadoutDragKind
    {
        None,
        Weapon,
        EquippedWeapon
    }

    /// <summary>
    /// Static drag payload for loadout drag-and-drop.
    /// </summary>
    public static class LoadoutDragSession
    {
        public static LoadoutDragKind Kind { get; private set; } = LoadoutDragKind.None;
        public static string ItemId { get; private set; }
        public static int SourceSlot { get; private set; } = -1;

        public static bool IsActive => Kind != LoadoutDragKind.None;

        public static void BeginWeapon(string weaponId)
        {
            Kind = LoadoutDragKind.Weapon;
            ItemId = weaponId ?? string.Empty;
            SourceSlot = -1;
        }

        public static void BeginEquippedWeapon(string weaponId, int slotIndex)
        {
            Kind = LoadoutDragKind.EquippedWeapon;
            ItemId = weaponId ?? string.Empty;
            SourceSlot = slotIndex;
        }

        public static void Clear()
        {
            Kind = LoadoutDragKind.None;
            ItemId = string.Empty;
            SourceSlot = -1;
        }
    }
}
