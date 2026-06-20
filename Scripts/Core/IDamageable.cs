namespace FPSGame.Core
{
    /// <summary>
    /// Anything that can receive weapon damage (Player, AI units, destructibles later).
    /// </summary>
    public interface IDamageable
    {
        bool IsAlive { get; }
        Faction Faction { get; }
        void TakeDamage(float amount);
    }
}
