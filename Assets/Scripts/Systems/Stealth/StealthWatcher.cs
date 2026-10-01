// Implements: 02 M0.1 raiders / M5.5 Spanish scouts — sight cone + line of sight, simple waypoint patrol, scripted chase.
// Not a combat enemy (no CombatTarget): these sequences have no attack option by design.
using System.Collections.Generic;
using UnityEngine;

namespace Ruminahui
{
    [RequireComponent(typeof(CharacterMotor))]
    public class StealthWatcher : MonoBehaviour
    {
        public float viewRange = 14f;        // PLACEHOLDER-BALANCE
        public float viewHalfAngle = 55f;    // PLACEHOLDER-BALANCE
        public float eyeHeight = 1.6f;
        public float patrolSpeedScale = 0.45f;
        public float waitAtPoint = 1.5f;
        public List<Vector3> patrol = new List<Vector3>();

        public float Detection { get; set; }
        public bool CanSeePlayer { get; private set; }
        public bool Chasing { get; private set; }

        CharacterMotor motor;
        PlaceholderVisual visual;
        int patrolIndex;
        float waitUntil;
        Transform chaseTarget;
        float chaseSpeed = 1f;

        void Awake()
        {
            motor = GetComponent<CharacterMotor>();
            visual = GetComponent<PlaceholderVisual>();
        }

        void OnEnable() { if (StealthSystem.Instance != null) StealthSystem.Instance.Watchers.Add(this); }
        void OnDisable() { if (StealthSystem.Instance != null) StealthSystem.Instance.Watchers.Remove(this); }

        /// <summary>Scripted: walk/run straight at a target (M5.5 scouts closing in, M0.1 raiders passing).</summary>
        public void ChaseTo(Transform target, float speedScale = 1f)
        {
            chaseTarget = target;
            chaseSpeed = speedScale;
            Chasing = target != null;
        }

        public void Evaluate(PlayerCharacter player, bool playerHidden, float dt)
        {
            var eye = transform.position + Vector3.up * eyeHeight;
            var chest = player.transform.position + Vector3.up * 1.0f;
            bool visible = !playerHidden && StealthMath.InViewCone(eye, transform.forward, chest, viewRange, viewHalfAngle) && HasLineOfSight(eye, chest, player);
            CanSeePlayer = visible;
            Detection = StealthMath.Step(Detection, visible, Vector3.Distance(eye, chest), viewRange, dt);
            if (visual != null && visible && Detection > 0.5f) visual.Flash(new Color(1f, 0.6f, 0.2f), 0.1f, false);
        }

        static bool HasLineOfSight(Vector3 eye, Vector3 target, PlayerCharacter player)
        {
            var dir = target - eye;
            if (!Physics.Raycast(eye, dir.normalized, out var hit, dir.magnitude, ~0, QueryTriggerInteraction.Ignore)) return true;
            // Hitting the player's own capsule (or anything on them) counts as seeing them.
            return hit.collider != null && hit.collider.GetComponentInParent<PlayerCharacter>() == player;
        }

        void Update()
        {
            if (Chasing)
            {
                if (chaseTarget == null) { Chasing = false; return; }
                var to = chaseTarget.position - transform.position;
                to.y = 0f;
                motor.SetMoveInput(to.magnitude > 1.2f ? to.normalized : Vector3.zero, chaseSpeed);
                if (to.sqrMagnitude > 0.01f) motor.FaceDirection(to);
                return;
            }

            if (patrol.Count == 0 || Time.time < waitUntil) { motor.SetMoveInput(Vector3.zero); return; }
            var target = patrol[patrolIndex];
            var d = target - transform.position;
            d.y = 0f;
            if (d.magnitude < 0.6f)
            {
                patrolIndex = (patrolIndex + 1) % patrol.Count;
                waitUntil = Time.time + waitAtPoint;
                motor.SetMoveInput(Vector3.zero);
                return;
            }
            motor.SetMoveInput(d.normalized, patrolSpeedScale);
        }
    }
}
