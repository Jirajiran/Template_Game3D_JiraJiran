using UnityEngine;

namespace FPSGame.Core
{
    [CreateAssetMenu(fileName = "CharacterStats", menuName = "FPSGame/Character Stats")]
    public class CharacterStats : ScriptableObject
    {
        [Header("Identity")]
        public string characterId;

        [Header("Vitality")]
        public float maxHealth = 100f;

        [Header("Movement")]
        public float walkSpeed = 5f;
        public float sprintSpeed = 8f;
        public float jumpHeight = 1.5f;
        public int jumpLimit = 1;
        public float turnSpeed = 10f;
        public float gravity = -20f;

        [Header("Faction")]
        public Faction faction = Faction.Friendly;
    }
}
