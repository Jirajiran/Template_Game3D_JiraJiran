using UnityEngine;

namespace FPSGame.Core
{
  /// <summary>
  /// Prototype-only input for Phase 1 testing. Remove or disable before shipping.
  /// </summary>
  public class CharacterDamageTester : MonoBehaviour
  {
    [SerializeField] private CharacterBase character;
    [SerializeField] private float testDamageAmount = 10f;
    [SerializeField] private float testHealAmount = 10f;

    private void Awake()
    {
      if (character == null)
        character = GetComponent<CharacterBase>();
    }

    private void Update()
    {
      if (GameplayPauseService.IsPaused)
        return;

      if (character == null) return;

      if (Input.GetKeyDown(KeyCode.K))
        character.TakeDamage(testDamageAmount);

      if (Input.GetKeyDown(KeyCode.H))
        character.Heal(testHealAmount);

      if (Input.GetKeyDown(KeyCode.Y) && !character.IsAlive)
        character.Revive();
    }
  }
}
