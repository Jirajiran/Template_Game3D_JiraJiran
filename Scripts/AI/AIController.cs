using FPSGame.Core;
using FPSGame.Weapons;
using UnityEngine;
using UnityEngine.AI;

namespace FPSGame.AI
{
    /// <summary>
    /// AI unit controller: Patrol → Chase → Attack using NavMesh and WeaponHandler.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(CharacterBase))]
    public class AIController : MonoBehaviour
    {
        [Header("Combat")]
        [SerializeField] private float attackRange = 18f;
        [SerializeField] private float meleeAttackRange = 2.5f;
        [SerializeField] private int rangedWeaponSlot = 0;
        [SerializeField] private int meleeWeaponSlot = 1;

        [Header("Patrol")]
        [SerializeField] private Transform[] patrolPoints;
        [SerializeField] private float patrolWaitSeconds = 1.5f;
        [SerializeField] private float patrolPointReachDistance = 0.75f;

        private NavMeshAgent agent;
        private CharacterBase character;
        private WeaponHandler weaponHandler;
        private AiPerception perception;

        private AiState state = AiState.Patrol;
        private int patrolIndex;
        private float patrolWaitTimer;
        private float faceTargetSpeed = 720f;

        public AiState State => state;

        public void ConfigurePatrol(Transform[] points)
        {
            patrolPoints = points;
            patrolIndex = 0;
            patrolWaitTimer = 0f;

            if (agent != null && agent.isOnNavMesh && patrolPoints != null && patrolPoints.Length > 0)
                agent.SetDestination(patrolPoints[0].position);
        }

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            character = GetComponent<CharacterBase>();
            perception = GetComponent<AiPerception>();
            weaponHandler = GetComponentInChildren<WeaponHandler>();

            if (character != null && character.Stats != null)
                agent.speed = character.Stats.walkSpeed;
        }

        private void Start()
        {
            if (patrolPoints != null && patrolPoints.Length > 0 && agent.isOnNavMesh)
                agent.SetDestination(patrolPoints[patrolIndex].position);
        }

        private void OnEnable()
        {
            if (character != null)
                character.OnDied += HandleDied;
        }

        private void OnDisable()
        {
            if (character != null)
                character.OnDied -= HandleDied;
        }

        private void Update()
        {
            if (character == null || !character.IsAlive || GameplayPauseService.IsPaused)
                return;

            perception?.Tick();
            UpdateStateMachine();
        }

        private void UpdateStateMachine()
        {
            switch (state)
            {
                case AiState.Patrol:
                    TickPatrol();
                    break;
                case AiState.Chase:
                    TickChase();
                    break;
                case AiState.Attack:
                    TickAttack();
                    break;
            }
        }

        private void TickPatrol()
        {
            agent.isStopped = false;

            if (HasPerceivedTarget())
            {
                TransitionTo(AiState.Chase);
                return;
            }

            if (patrolPoints == null || patrolPoints.Length == 0)
            {
                agent.isStopped = true;
                return;
            }

            Transform point = patrolPoints[patrolIndex];
            if (point == null)
            {
                patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
                return;
            }

            if (!agent.pathPending && agent.remainingDistance <= patrolPointReachDistance)
            {
                patrolWaitTimer += Time.deltaTime;
                agent.isStopped = true;
                if (patrolWaitTimer >= patrolWaitSeconds)
                {
                    patrolWaitTimer = 0f;
                    patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
                    agent.isStopped = false;
                    agent.SetDestination(patrolPoints[patrolIndex].position);
                }

                return;
            }

            if (!agent.hasPath && !agent.pathPending)
                agent.SetDestination(point.position);
        }

        private void TickChase()
        {
            agent.isStopped = false;

            if (!HasPerceivedTarget())
            {
                TransitionTo(AiState.Patrol);
                return;
            }

            var target = perception.CurrentTarget;
            float distance = Vector3.Distance(transform.position, target.transform.position);

            if (distance <= GetPreferredAttackRange())
            {
                TransitionTo(AiState.Attack);
                return;
            }

            agent.SetDestination(target.transform.position);
        }

        private void TickAttack()
        {
            if (!HasPerceivedTarget())
            {
                TransitionTo(AiState.Patrol);
                return;
            }

            var target = perception.CurrentTarget;
            float distance = Vector3.Distance(transform.position, target.transform.position);

            if (distance > attackRange * 1.15f)
            {
                TransitionTo(AiState.Chase);
                return;
            }

            agent.isStopped = true;
            FaceTarget(target.transform.position);

            SelectWeaponForDistance(distance);
            weaponHandler?.TryPrimaryAction();
        }

        private void TransitionTo(AiState next)
        {
            state = next;
            patrolWaitTimer = 0f;
        }

        private bool HasPerceivedTarget() =>
            perception != null && perception.HasTarget;

        private float GetPreferredAttackRange()
        {
            if (HasMeleeWeapon())
                return meleeAttackRange;

            return attackRange;
        }

        private void SelectWeaponForDistance(float distance)
        {
            if (weaponHandler == null)
                return;

            if (distance <= meleeAttackRange && HasMeleeWeapon())
                weaponHandler.SetActiveSlot(meleeWeaponSlot);
            else if (HasRangedWeapon())
                weaponHandler.SetActiveSlot(rangedWeaponSlot);
        }

        private bool HasRangedWeapon() =>
            weaponHandler != null && weaponHandler.GetWeaponInSlot(rangedWeaponSlot) != null;

        private bool HasMeleeWeapon() =>
            weaponHandler != null && weaponHandler.GetWeaponInSlot(meleeWeaponSlot) != null;

        private void FaceTarget(Vector3 worldPosition)
        {
            Vector3 toTarget = worldPosition - transform.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude < 0.001f)
                return;

            Quaternion desired = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                desired,
                faceTargetSpeed * Time.deltaTime);
        }

        private void HandleDied()
        {
            if (agent != null && agent.isOnNavMesh)
                agent.isStopped = true;

            enabled = false;
        }
    }
}
