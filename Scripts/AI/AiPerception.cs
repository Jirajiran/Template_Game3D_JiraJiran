using FPSGame.Core;
using FPSGame.Weapons;
using UnityEngine;

namespace FPSGame.AI
{
    /// <summary>
    /// Detects hostile targets via distance, FOV, line-of-sight, and hearing (sprint / gunshots).
    /// </summary>
    public class AiPerception : MonoBehaviour
    {
        [SerializeField] private AiPerceptionConfig config;
        [SerializeField] private Transform eyePoint;
        [SerializeField] private string targetTag = "Player";

        private CharacterBase owner;
        private CharacterBase currentTarget;
        private float lostTargetTimer;
        private Vector3 lastKnownPosition;

        public CharacterBase CurrentTarget => currentTarget;
        public Vector3 LastKnownPosition => lastKnownPosition;
        public bool HasTarget => currentTarget != null && currentTarget.IsAlive;

        private void Awake()
        {
            owner = GetComponentInParent<CharacterBase>();
            if (eyePoint == null)
                eyePoint = transform;
        }

        public void Tick()
        {
            if (config == null || owner == null || !owner.IsAlive)
            {
                ClearTarget();
                return;
            }

            if (TryAcquireTarget(out CharacterBase acquired))
            {
                currentTarget = acquired;
                lastKnownPosition = acquired.transform.position;
                lostTargetTimer = 0f;
                return;
            }

            if (currentTarget != null)
            {
                lostTargetTimer += Time.deltaTime;
                if (lostTargetTimer >= config.loseTargetDelay)
                    ClearTarget();
            }
        }

        public bool CanSee(CharacterBase target)
        {
            if (config == null || target == null || !target.IsAlive || owner == null)
                return false;

            if (!owner.IsHostileTo(target))
                return false;

            Vector3 origin = eyePoint.position;
            Vector3 toTarget = target.transform.position - origin;
            float distance = toTarget.magnitude;
            if (distance > config.sightRange)
                return false;

            Vector3 flatForward = transform.forward;
            flatForward.y = 0f;
            Vector3 flatToTarget = toTarget;
            flatToTarget.y = 0f;
            if (flatForward.sqrMagnitude < 0.001f || flatToTarget.sqrMagnitude < 0.001f)
                return false;

            flatForward.Normalize();
            flatToTarget.Normalize();
            float halfFov = config.fieldOfViewDegrees * 0.5f * Mathf.Deg2Rad;
            float angle = Vector3.Angle(flatForward, flatToTarget);
            if (angle > halfFov)
                return false;

            if (Physics.Raycast(origin, toTarget.normalized, out RaycastHit hit, distance, config.lineOfSightMask, QueryTriggerInteraction.Ignore))
            {
                var hitCharacter = hit.collider.GetComponentInParent<CharacterBase>();
                return hitCharacter == target;
            }

            return false;
        }

        public bool CanHear(CharacterBase target)
        {
            if (config == null || target == null || !target.IsAlive || owner == null)
                return false;

            if (!owner.IsHostileTo(target))
                return false;

            float distance = Vector3.Distance(transform.position, target.transform.position);
            if (distance > config.hearingRange)
                return false;

            if (PlayerNoiseEmitter.IsSprinting(target.gameObject))
                return true;

            if (Time.time - PlayerNoiseEmitter.GetLastGunshotTime(target.gameObject) <= config.gunshotMemorySeconds)
                return true;

            return false;
        }

        private bool TryAcquireTarget(out CharacterBase target)
        {
            target = null;

            if (currentTarget != null && currentTarget.IsAlive &&
                (CanSee(currentTarget) || CanHear(currentTarget)))
            {
                target = currentTarget;
                return true;
            }

            var playerGo = GameObject.FindGameObjectWithTag(targetTag);
            if (playerGo == null)
                return false;

            var candidate = playerGo.GetComponent<CharacterBase>();
            if (candidate == null || !candidate.IsAlive)
                return false;

            if (CanSee(candidate) || CanHear(candidate))
            {
                target = candidate;
                return true;
            }

            return false;
        }

        private void ClearTarget()
        {
            currentTarget = null;
            lostTargetTimer = 0f;
        }
    }
}
