namespace FPSGame.Weapons
{
    /// <summary>Runtime ammo per weapon slot. Initialized from RangedWeaponData on start.</summary>
    public struct WeaponSlotState
    {
        public int clipAmmo;
        public int reserveMagazines;
        public bool initialized;

        public void InitFrom(RangedWeaponData data)
        {
            clipAmmo = data.clipSize;
            reserveMagazines = data.magazineCount;
            initialized = true;
        }
    }
}
