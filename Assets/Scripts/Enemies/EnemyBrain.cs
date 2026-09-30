// Implements: 06 Enemy Roster — shared enemy AI skeleton: target acquisition (decoy-aware), tells (visual flash + optional audio),
// stagger/flinch handling, death → pool despawn. Each roster entry is one subclass.
using System.Collections;
using UnityEngine;

namespace Ruminahui
{
    public enum EnemyState { Idle, Hidden, Chase, Reposition, Telegraph, Attack, Recover, Staggered, Special, Dead }

    [RequireComponent(typeof(CharacterMotor), typeof(CombatTarget))]
    public abstract class EnemyBrain : MonoBehaviour, IPoolable
    {
        public static readonly System.Collections.Generic.List<EnemyBrain> Active = new System.Collections.Generic.List<EnemyBrain>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => Active.Clear();

        public EnemyType type;
        public string displayName = "Enemy";
        public float aggroRange = 20f;
        [Tooltip("Light hits interrupt this enemy's windups (fodder yes, shields/bosses no).")]
        public bool flinchOnHit = true;
        [Tooltip("Set by EncounterDirector (M3.5 section tuning).")]
        public float encounterDamageScale = 1f;
        [Tooltip("Set by the Spanish Officer's rally.")]
        public float buffDamageScale = 1f;
        public float despawnDelay = 1.5f;

        public EnemyState State { get; protected set; }
        public CombatTarget Target { get; protected set; }
        public bool IsTelegraphing => State == EnemyState.Telegraph;
        public bool IsDead => State == EnemyState.Dead;

        public event System.Action<EnemyBrain> Died;

        protected CharacterMotor motor;
        protected CombatTarget self;
        protected StatusEffects status;
        protected Health health;
        protected PlaceholderVisual visual;
        protected bool superArmor;

        Coroutine behaviour;
        Coroutine reaction;
        bool started;
        float buffUntil;

        public static readonly UnityEngine.Color TellColor = new UnityEngine.Color(1f, 0.55f, 0.1f);
        public static readonly UnityEngine.Color UnblockableColor = new UnityEngine.Color(1f, 0.05f, 0.05f);

        protected float Dt => TimeDilation.DeltaFor(gameObject);
        protected float DamageScale => encounterDamageScale * buffDamageScale * DifficultyScaler.EnemyDamage;

        protected virtual void Awake()
        {
            motor = GetComponent<CharacterMotor>();
            self = GetComponent<CombatTarget>();
            status = GetComponent<StatusEffects>();
            health = GetComponent<Health>();
            visual = GetComponent<PlaceholderVisual>();
        }

        protected virtual void OnEnable()
        {
            if (!Active.Contains(this)) Active.Add(this);
            health.Died += HandleDeath;
            status.Staggered += HandleStagger;
            status.Interrupted += HandleInterrupt;
        }

        protected virtual void OnDisable()
        {
            Active.Remove(this);
            health.Died -= HandleDeath;
            status.Staggered -= HandleStagger;
            status.Interrupted -= HandleInterrupt;
        }

        void Start()
        {
            // Scene-placed (non-pooled) enemies start themselves; pooled ones get OnSpawned from the pool.
            if (!started && GetComponent<PooledObject>() == null) OnSpawned();
        }

        public virtual void OnSpawned()
        {
            started = true;
            StopAllCoroutines();
            behaviour = reaction = null;
            health.ResetFull();
            status.ClearAll();
            self.stealthed = false;
            self.incomingDamageMultiplier = 1f;
            buffDamageScale = 1f;
            superArmor = false;
            Target = null;
            State = EnemyState.Idle;
            if (visual != null) { visual.SetVisible(true); visual.ResetPose(); }
            OnReset();
            RestartBehaviour();
        }

        public virtual void OnDespawned()
        {
            StopAllCoroutines();
            behaviour = reaction = null;
            started = false;
            encounterDamageScale = 1f;
        }

        /// <summary>Subclass reset hook (called on every spawn).</summary>
        protected virtual void OnReset() { }

        protected abstract IEnumerator Behaviour();

        protected void RestartBehaviour()
        {
            if (behaviour != null) StopCoroutine(behaviour);
            behaviour = StartCoroutine(Behaviour());
        }

        protected void StopBehaviour()
        {
            if (behaviour != null) StopCoroutine(behaviour);
            behaviour = null;
            motor.SetMoveInput(Vector3.zero);
            motor.movementLocked = false;
            OnBehaviourStopped();
        }

        protected virtual void OnBehaviourStopped() { }

        protected virtual void Update()
        {
            if (buffUntil > 0f && Time.time >= buffUntil) { buffUntil = 0f; buffDamageScale = 1f; }
        }

        // ───────── Reactions ─────────
        void HandleStagger(float duration)
        {
            if (IsDead) return;
            StopBehaviour();
            if (reaction != null) StopCoroutine(reaction);
            reaction = StartCoroutine(StaggerRoutine());
        }

        IEnumerator StaggerRoutine()
        {
            State = EnemyState.Staggered;
            if (visual != null) visual.SetPoseScale(new Vector3(1f, 0.85f, 1f));
            while (status.IsIncapacitated) yield return null;
            if (visual != null) visual.ResetPose();
            reaction = null;
            OnRecoveredFromStagger();
            if (!IsDead) RestartBehaviour();
        }

        protected virtual void OnRecoveredFromStagger() { }

        void HandleInterrupt()
        {
            if (IsDead || superArmor || !flinchOnHit || status.IsIncapacitated) return;
            if (State != EnemyState.Telegraph && State != EnemyState.Attack) return;
            StopBehaviour();
            if (reaction != null) StopCoroutine(reaction);
            reaction = StartCoroutine(FlinchRoutine());
        }

        IEnumerator FlinchRoutine()
        {
            State = EnemyState.Recover;
            yield return Wait(0.3f);
            reaction = null;
            if (!IsDead) RestartBehaviour();
        }

        void HandleDeath()
        {
            State = EnemyState.Dead;
            StopAllCoroutines();
            behaviour = reaction = null;
            motor.SetMoveInput(Vector3.zero);
            if (visual != null) { visual.SetBaseColor(Color.Lerp(visual.baseColor, Color.black, 0.6f)); visual.SetPoseScale(new Vector3(1.2f, 0.3f, 1.2f)); }
            OnDeath();
            Died?.Invoke(this);
            if (GetComponent<PooledObject>() != null) PoolManager.Instance.Despawn(gameObject, despawnDelay);
            else StartCoroutine(DisableLater());
        }

        protected virtual void OnDeath() { }

        IEnumerator DisableLater()
        {
            yield return new WaitForSeconds(despawnDelay);
            gameObject.SetActive(false);
        }

        public void ApplyBuff(float damageScale, float duration)
        {
            buffDamageScale = damageScale;
            buffUntil = Time.time + duration;
            if (visual != null) visual.Flash(new Color(1f, 0.85f, 0.2f), 0.5f, false);
        }

        // ───────── Helpers for subclasses ─────────
        protected IEnumerator Wait(float seconds)
        {
            float t = 0f;
            while (t < seconds) { t += Dt; yield return null; }
        }

        protected bool AcquireTarget()
        {
            if (Target != null && Target.IsTargetable && !Target.isDecoy &&
                Vector3.Distance(Target.transform.position, transform.position) <= aggroRange * 1.5f)
            {
                // Still re-check for decoys: Shed Skin baits enemies mid-fight.
                var decoy = CombatQuery.Nearest(self, transform.position, aggroRange, t => t.isDecoy);
                if (decoy != null) Target = decoy;
                return true;
            }
            Target = CombatQuery.Nearest(self, transform.position, aggroRange, null, preferAggro: true);
            return Target != null;
        }

        protected float DistanceToTarget => Target != null ? Vector3.Distance(transform.position, Target.transform.position) : float.MaxValue;

        protected void MoveTowards(Vector3 point, float speedScale = 1f)
        {
            var dir = point - transform.position;
            dir.y = 0f;
            dir = dir.normalized + Separation();
            motor.SetMoveInput(dir.normalized, speedScale);
        }

        protected void MoveAwayFrom(Vector3 point, float speedScale = 1f)
        {
            var dir = transform.position - point;
            dir.y = 0f;
            motor.SetMoveInput((dir.normalized + Separation()).normalized, speedScale);
            motor.FaceTowards(point);
        }

        protected void Strafe(Vector3 around, float sign, float speedScale = 0.5f)
        {
            var to = around - transform.position;
            to.y = 0f;
            var side = Vector3.Cross(Vector3.up, to.normalized) * sign;
            motor.SetMoveInput((side + Separation()).normalized, speedScale);
            motor.FaceTowards(around);
        }

        protected void Stop() => motor.SetMoveInput(Vector3.zero);

        Vector3 Separation()
        {
            Vector3 push = Vector3.zero;
            foreach (var e in Active)
            {
                if (e == this || e.IsDead) continue;
                var d = transform.position - e.transform.position;
                d.y = 0f;
                float m = d.magnitude;
                if (m < 1.4f && m > 0.01f) push += d / m * (1.4f - m);
            }
            return push;
        }

        /// <summary>The visible (and optionally audible) tell before an attack.</summary>
        protected IEnumerator Telegraph(float duration, Color color, PlaceholderCue? cue = PlaceholderCue.Tell)
        {
            State = EnemyState.Telegraph;
            Stop();
            if (Target != null) motor.FaceTowards(Target.transform.position, true);
            if (visual != null) visual.Flash(color, duration);
            if (cue.HasValue && AudioPool.Instance != null) AudioPool.Instance.PlayCue(cue.Value, self.AimPoint, 0.35f);
            float t = 0f;
            while (t < duration)
            {
                if (Target != null) motor.FaceTowards(Target.transform.position);
                t += Dt;
                yield return null;
            }
        }

        /// <summary>Melee strike in a forward cone. Returns number of targets hit.</summary>
        protected int MeleeStrike(float damage, float range, float halfAngle, HitFlags flags = HitFlags.None, float stagger = 0f, float knockback = 0f, string attackName = "Strike", int guardBreak = 0)
        {
            State = EnemyState.Attack;
            int n = 0;
            foreach (var t in CombatQuery.Cone(self, transform.position, transform.forward, range, halfAngle))
            {
                t.ReceiveHit(new DamageInfo
                {
                    Amount = damage * DamageScale,
                    Attacker = self,
                    Point = t.AimPoint,
                    Direction = (t.transform.position - transform.position).normalized,
                    Flags = flags,
                    StaggerDuration = stagger,
                    Knockback = knockback,
                    AttackName = attackName,
                    GuardBreakPower = guardBreak,
                });
                n++;
            }
            return n;
        }

        protected void FireAt(CombatTarget target, float damage, float speed, Color color, HitFlags flags = HitFlags.Ranged, string attackName = "Shot", float scale = 0.25f)
        {
            State = EnemyState.Attack;
            var from = self.AimPoint + transform.forward * 0.6f + Vector3.up * 0.3f;
            var aim = target != null ? target.AimPoint : from + transform.forward * 10f;
            var payload = new DamageInfo { Amount = damage * DamageScale, Flags = flags, AttackName = attackName };
            Projectile.Fire(self, from, aim - from, speed, payload, color, null, null, scale);
        }

        /// <summary>Short forward lunge used by melee strikes.</summary>
        protected void Lunge(float distance) => motor.Dash(transform.forward, distance, 0.1f);
    }
}
