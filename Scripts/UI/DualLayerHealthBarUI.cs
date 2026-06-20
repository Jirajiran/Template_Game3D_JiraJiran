using FPSGame.Core;
using UnityEngine;
using UnityEngine.UI;

namespace FPSGame.UI
{
    /// <summary>
    /// Primary HP (instant) + delay bar (visual catch-up from CharacterBase).
    /// </summary>
    public class DualLayerHealthBarUI : MonoBehaviour
    {
        [SerializeField] private Image primaryFill;
        [SerializeField] private Image delayFill;

        private CharacterBase character;

        public void Bind(CharacterBase target)
        {
            Unbind();

            character = target;
            if (character == null)
                return;

            character.OnHealthChanged += HandleHealthChanged;
            RefreshImmediate();
        }

        private void OnDestroy() => Unbind();

        private void Update()
        {
            if (character == null || delayFill == null)
                return;

            delayFill.fillAmount = GetFraction(character.DisplayDelayHealth);
        }

        private void HandleHealthChanged(float previous, float current)
        {
            RefreshImmediate();
        }

        private void RefreshImmediate()
        {
            if (character == null)
                return;

            float fraction = GetFraction(character.CurrentHealth);
            if (primaryFill != null)
                primaryFill.fillAmount = fraction;

            if (delayFill != null && Mathf.Approximately(character.DisplayDelayHealth, character.CurrentHealth))
                delayFill.fillAmount = fraction;
        }

        private float GetFraction(float health)
        {
            float max = character.MaxHealth;
            return max > 0f ? Mathf.Clamp01(health / max) : 0f;
        }

        private void Unbind()
        {
            if (character != null)
                character.OnHealthChanged -= HandleHealthChanged;

            character = null;
        }
    }
}
