// Implements: 03-combat-design.md, Section 0 (resources) + Sections 1-3 (block/parry/dodge/counter all resolve here through
// IHitInterceptor). Also keeps a static registry used for targeting/AoE instead of physics layers (cheap, no layer setup).
using System.Collections.Generic;
using UnityEngine;

namespace Ruminahui
{
    [RequireComponent(typeof(Health), typeof(StatusEffects))]
    public class CombatTarget : MonoBehaviour
    {
        public static readonly List<CombatTarget> All = new List<CombatTarget>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => All.Clear();

        public Faction faction = Faction.Enemy;
        public float aimHeight = 1.1f;
        public float radius = 0.45f;
        [Tooltip("Decoys use a high value so enemies prefer them (Shed Skin, Arquebusier bait).")]
        public int aggroPriority;
        public bool isDecoy;
        public bool suppressDamageNumbers;

        [Header("Runtime modifiers")]
        public float incomingDamageMultiplier = 1f;
        public float outgoingDamageMultiplier = 1f;
        public bool stealthed;       // False Trail, hidden scouts, Atoc's stealth reposition
        public bool invulnerable;    // debug god mode, no-fail missions, spare state

        public Health Health { get; private set; }
        public StatusEffects Status { get; private set; }
        public CharacterMotor Motor { get; private set; }
        public Vector3 AimPoint => transform.position + Vector3.up * aimHeight;
        public bool IsAlive => Health != null && !Health.IsDead;
        public bool IsTargetable => IsAlive && isActiveAndEnabled && !stealthed;

        /// <summary>Raised on the target after any hit resolves.</summary>
        public event System.Action<DamageInfo, HitResult> HitReceived;
        /// <summary>Raised on the attacker after one of its hits resolves.</summary>
        public event System.Action<CombatTarget, DamageInfo, HitResult> HitDealt;

        readonly List<IHitInterceptor> interceptors = new List<IHitInterceptor>();

        void Awake()
        {
            Health = GetComponent<Health>();
            Status = GetComponent<StatusEffects>();
            Motor = GetComponent<CharacterMotor>();
        }

        void OnEnable() { if (!All.Contains(this)) All.Add(this); }
        void OnDisable() => All.Remove(this);

        public void RegisterInterceptor(IHitInterceptor i)
        {
            if (interceptors.Contains(i)) return;
            interceptors.Add(i);
            interceptors.Sort((a, b) => b.InterceptPriority.CompareTo(a.InterceptPriority));
        }

        public void UnregisterInterceptor(IHitInterceptor i) => interceptors.Remove(i);

        public HitResult ReceiveHit(DamageInfo info)
        {
            if (!IsAlive) return new HitResult(HitOutcome.Ignored);
            if (invulnerable && !isDecoy)
            {
                var none = new HitResult(HitOutcome.Evaded);
                Finish(info, none);
                return none;
            }

            info.Amount *= incomingDamageMultiplier;
            for (int i = 0; i < interceptors.Count; i++)
            {
                if (interceptors[i].Intercept(ref info, out var intercepted))
                {
                    Finish(info, intercepted);
                    return intercepted;
                }
            }

            float dealt = Health.ApplyDamage(info.Amount);
            var result = new HitResult(Health.IsDead ? HitOutcome.Killed : HitOutcome.Hit, dealt);

            if (!Health.IsDead)
            {
                Status.NotifyHitTaken();
                if (info.Has(HitFlags.Knockdown)) Status.ApplyKnockdown(Mathf.Max(0.8f, info.StaggerDuration));
                else if (info.StaggerDuration > 0f || info.Has(HitFlags.Stagger)) Status.ApplyStagger(Mathf.Max(0.5f, info.StaggerDuration));
                if (info.Knockback > 0f && Motor != null)
                {
                    var dir = info.Direction; dir.y = 0f;
                    Motor.AddKnockback(dir.normalized * info.Knockback);
                }
            }

            Finish(info, result);
            return result;
        }

        void Finish(DamageInfo info, HitResult result)
        {
            if (result.DamageDealt > 0f && !suppressDamageNumbers && !info.Has(HitFlags.NoDamageNumber))
            {
                var color = faction == Faction.Player ? new Color(1f, 0.35f, 0.35f) : Color.white;
                if (info.Has(HitFlags.Execute)) color = new Color(1f, 0.85f, 0.2f);
                DamageNumbers.Show(AimPoint + Vector3.up * 0.5f, result.DamageDealt, color);
            }

            if (result.Outcome == HitOutcome.Hit || result.Outcome == HitOutcome.Killed)
                AudioPool.Instance?.PlayCue(PlaceholderCue.Hit, AimPoint, 0.35f);

            HitReceived?.Invoke(info, result);
            if (info.Attacker != null) info.Attacker.HitDealt?.Invoke(this, info, result);
        }

        /// <summary>Direction from this target toward a world point, flattened.</summary>
        public Vector3 FlatDirectionTo(Vector3 point)
        {
            var d = point - transform.position;
            d.y = 0f;
            return d.sqrMagnitude < 0.0001f ? transform.forward : d.normalized;
        }

        /// <summary>True when <paramref name="attackerPos"/> is inside this target's front arc.</summary>
        public bool IsInFront(Vector3 attackerPos, float halfAngleDeg)
        {
            var to = FlatDirectionTo(attackerPos);
            return Vector3.Angle(transform.forward, to) <= halfAngleDeg;
        }
    }
}
