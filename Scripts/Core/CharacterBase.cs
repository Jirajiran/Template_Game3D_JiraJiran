using System;
using UnityEngine;

namespace FPSGame.Core
{
  /// <summary>
  /// Shared base for Player and AI units. Control differs via controller components.
  /// </summary>
  public class CharacterBase : MonoBehaviour, IDamageable
  {
    [SerializeField] private CharacterStats stats;
    [SerializeField] private float delayBarCatchUpPerSecond = 40f;

    private float currentHealth;
    private float displayDelayHealth;
    private bool isGrounded;
    private bool isDead;

    public CharacterStats Stats => stats;
    public Faction Faction => stats != null ? stats.faction : Faction.Friendly;
    public float MaxHealth => stats != null ? stats.maxHealth : 0f;
    public float CurrentHealth => currentHealth;
    public float DisplayDelayHealth => displayDelayHealth;
    public float HealthFraction => MaxHealth > 0f ? currentHealth / MaxHealth : 0f;
    public bool IsAlive => !isDead;
    public bool IsGrounded => isGrounded;

    public event Action<float, float> OnHealthChanged;
    public event Action<float> OnDelayHealthChanged;
    public event Action<float> OnHealed;
    public event Action OnDied;
    public event Action OnRevived;

    private void Awake()
    {
      if (stats == null)
      {
        Debug.LogError($"{name}: CharacterStats not assigned.", this);
        return;
      }

      ResetHealthState();
    }

    private void Update()
    {
      if (stats == null || !IsAlive) return;
      TickDelayHealthBar();
    }

    public void SetGrounded(bool grounded) => isGrounded = grounded;

    public virtual void TakeDamage(float amount)
    {
      if (!IsAlive || stats == null || amount <= 0f) return;

      float previous = currentHealth;
      currentHealth = Mathf.Max(0f, currentHealth - amount);
      OnHealthChanged?.Invoke(previous, currentHealth);

      if (currentHealth <= 0f)
        Die();
    }

    public virtual void Heal(float amount)
    {
      if (!IsAlive || stats == null || amount <= 0f) return;
      if (currentHealth >= stats.maxHealth) return;

      float previous = currentHealth;
      currentHealth = Mathf.Min(stats.maxHealth, currentHealth + amount);
      float healed = currentHealth - previous;
      if (healed <= 0f) return;

      OnHealthChanged?.Invoke(previous, currentHealth);
      OnHealed?.Invoke(healed);
    }

    public virtual void Revive(bool restoreFullHealth = true)
    {
      if (stats == null) return;

      isDead = false;
      if (restoreFullHealth)
      {
        float previous = currentHealth;
        currentHealth = stats.maxHealth;
        displayDelayHealth = stats.maxHealth;
        OnHealthChanged?.Invoke(previous, currentHealth);
      }

      OnRevived?.Invoke();
    }

    public void ResetHealthState()
    {
      if (stats == null) return;

      isDead = false;
      currentHealth = stats.maxHealth;
      displayDelayHealth = stats.maxHealth;
    }

    /// <summary>Runtime swap (loadout / profile apply).</summary>
    public void ApplyStats(CharacterStats newStats)
    {
      if (newStats == null)
        return;

      stats = newStats;
      ResetHealthState();
    }

    protected virtual void Die()
    {
      if (isDead) return;

      isDead = true;
      currentHealth = 0f;
      OnDied?.Invoke();
    }

    private void TickDelayHealthBar()
    {
      if (Mathf.Approximately(displayDelayHealth, currentHealth)) return;

      float previous = displayDelayHealth;
      float speed = delayBarCatchUpPerSecond > 0f
        ? delayBarCatchUpPerSecond
        : stats.maxHealth;
      displayDelayHealth = Mathf.MoveTowards(displayDelayHealth, currentHealth, speed * Time.deltaTime);
      OnDelayHealthChanged?.Invoke(previous - displayDelayHealth);
    }

    public bool IsHostileTo(CharacterBase other)
    {
      if (other == null || other == this) return false;
      return Faction != other.Faction;
    }
  }
}
