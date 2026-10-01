// Implements: 02 M2.2 "one guarded-escort combat sequence (protect a convoy)" and M5.2 "escort/evacuation objectives under time
// pressure". An escort is a Player-faction CombatTarget, so enemies attack it like they attack you. It follows a path and HALTS
// while enemies are close — you have to clear the way.
using System.Collections.Generic;
using UnityEngine;

namespace Ruminahui
{
    [RequireComponent(typeof(CombatTarget), typeof(CharacterMotor))]
    public class EscortTarget : MonoBehaviour
    {
        public static readonly List<EscortTarget> All = new List<EscortTarget>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => All.Clear();

        public string displayName = "Convoy";
        public List<Vector3> path = new List<Vector3>();
        public float speedScale = 0.45f;       // PLACEHOLDER-BALANCE: slower than walking pace
        public float haltRadius = 7f;          // PLACEHOLDER-BALANCE
        public float arriveRadius = 1.2f;
        public bool moving = true;

        public int Waypoint { get; private set; }
        public bool Arrived { get; private set; }
        public bool Lost { get; private set; }
        public bool Halted { get; private set; }
        public Health Health { get; private set; }

        public event System.Action<EscortTarget> ArrivedEvent;
        public event System.Action<EscortTarget> LostEvent;
        public event System.Action<EscortTarget, int> WaypointReached;

        CombatTarget target;
        CharacterMotor motor;
        Vector3 startPosition;

        void Awake()
        {
            startPosition = transform.position;
            target = GetComponent<CombatTarget>();
            motor = GetComponent<CharacterMotor>();
            Health = GetComponent<Health>();
        }

        void OnEnable()
        {
            if (!All.Contains(this)) All.Add(this);
            Health.Died += OnDied;
        }

        void OnDisable()
        {
            All.Remove(this);
            Health.Died -= OnDied;
        }

        void OnDied()
        {
            Lost = true;
            motor.SetMoveInput(Vector3.zero);
            LostEvent?.Invoke(this);
        }

        /// <summary>Retry: back to a waypoint, full health.</summary>
        public void ResetTo(int waypoint)
        {
            Waypoint = Mathf.Clamp(waypoint, 0, Mathf.Max(0, path.Count - 1));
            Arrived = false;
            Lost = false;
            Health.ResetFull();
            target.Status.ClearAll();
            // Back to the last waypoint it had reached (or where it started).
            motor.Teleport(Waypoint == 0 || path.Count == 0 ? startPosition : path[Waypoint - 1]);
        }

        void Update()
        {
            if (Lost || Arrived || !moving || path.Count == 0) { motor.SetMoveInput(Vector3.zero); return; }
            Halted = CombatQuery.Nearest(target, transform.position, haltRadius, t => !t.isDecoy) != null;
            if (Halted) { motor.SetMoveInput(Vector3.zero); return; }

            var goal = path[Waypoint];
            var to = goal - transform.position;
            to.y = 0f;
            if (to.magnitude <= arriveRadius)
            {
                WaypointReached?.Invoke(this, Waypoint);
                Waypoint++;
                if (Waypoint >= path.Count)
                {
                    Arrived = true;
                    motor.SetMoveInput(Vector3.zero);
                    ArrivedEvent?.Invoke(this);
                }
                return;
            }
            motor.SetMoveInput(to.normalized, speedScale);
        }
    }
}
