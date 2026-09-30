// Implements: 03-combat-design.md, Sections 1-4 — shared plumbing for the Puma/Kuntur/Amaru kits: action state machine,
// combo chains with input buffering, melee resolution, Focus gains (clean hits + post-M3.5 tell reading), ability slots,
// and the command surface used by BOTH the player brain and AI allies (Section 4 "AI allies using their own kit").
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ruminahui
{
    public enum PromptStyle { Normal, Grapple, Spare, Locked }

    public abstract class CombatKit : MonoBehaviour
    {
        public abstract KitId Kit { get; }

        [Header("Shared kit tuning")]
        public float comboWindow = 0.45f;        // PLACEHOLDER-BALANCE: time after an attack to continue the chain
        public float inputBufferTime = 0.25f;    // PLACEHOLDER-BALANCE
        public float hitStunDuration = 0.3f;     // PLACEHOLDER-BALANCE

        protected PlayerCharacter character;
        protected CharacterMotor motor;
        protected CombatTarget self;
        protected Stamina stamina;
        protected StatusEffects status;

        protected Coroutine currentAction;
        bool busy;
        int actionSerial;
        protected string currentActionName = "";
        protected bool currentInterruptible = true;
        protected int comboIndex;
        protected float comboExpiresAt;
        float bufferedLightUntil = -1f;
        readonly FocusPool fallbackFocus = new FocusPool();

        /// <summary>Specials in slot order (R/F/G). Index 3 conceptually = ultimate, kept separate.</summary>
        public readonly List<AbilitySlot> Specials = new List<AbilitySlot>();
        public AbilitySlot Ultimate { get; protected set; }

        /// <summary>Outgoing damage multiplier from buffs (ultimates, rebirth).</summary>
        public float damageMultiplier = 1f;

        public bool IsBusy => busy;
        public string CurrentActionName => currentActionName;
        public FocusPool Focus => PartyManager.Instance != null ? PartyManager.Instance.Focus : fallbackFocus;
        public bool IsAIControlled => character != null && !character.IsPlayerControlled;
        public virtual float MoveSpeedMultiplier => 1f;
        public virtual bool AllowsMovement => !IsBusy;
        /// <summary>Upgrade tier for this kit (0-3).</summary>
        protected int Tier => Progression.GetTier(Kit);

        protected virtual void Awake()
        {
            character = GetComponent<PlayerCharacter>();
            motor = GetComponent<CharacterMotor>();
            self = GetComponent<CombatTarget>();
            stamina = GetComponent<Stamina>();
            status = GetComponent<StatusEffects>();
            BuildAbilities();
        }

        protected virtual void OnEnable()
        {
            self.HitDealt += OnHitDealt;
            status.Interrupted += OnInterrupted;
        }

        protected virtual void OnDisable()
        {
            self.HitDealt -= OnHitDealt;
            status.Interrupted -= OnInterrupted;
            CancelAction();
        }

        protected abstract void BuildAbilities();

        // ───────────────────────── Command surface (player brain + AI allies) ─────────────────────────
        public void CmdLight()
        {
            if (IsBusy) { bufferedLightUntil = Time.time + inputBufferTime; return; }
            OnLight();
        }
        public virtual void CmdHeavyPressed() { }
        public virtual void CmdHeavyReleased() { }
        public virtual void CmdBlockPressed() { }
        public virtual void CmdBlockReleased() { }
        public virtual void CmdDodge(Vector3 worldDirection) { }
        public virtual void CmdMark() { }
        public virtual void CmdPlace() { }
        public virtual void CmdJump() { if (!IsBusy) motor.Jump(); }

        /// <summary>Contextual action (grapple etc.). Return true if consumed so interactables don't also fire.</summary>
        public virtual bool TryContextAction() => false;
        public virtual bool GetContextPrompt(out string text, out PromptStyle style) { text = null; style = PromptStyle.Normal; return false; }

        public void CmdSpecial(int index) => UseSpecial(index, false);
        public void CmdUltimate() => UseUltimate(false);

        /// <summary>free = AI ally use: doesn't drain the player's shared Focus (ASSUMPTION, see IMPLEMENTATION_LOG).</summary>
        public bool UseSpecial(int index, bool free)
        {
            if (index < 0 || index >= Specials.Count) return false;
            if (IsBusy || status.IsIncapacitated) return false;
            var slot = Specials[index];
            // Traversal use outside combat is free (M5.3 has no combat to earn Focus — ASSUMPTION, see IMPLEMENTATION_LOG).
            if (!free && IsTraversalAbility(index) && CombatQuery.Nearest(self, transform.position, 25f, t => !t.isDecoy) == null) free = true;
            if (!slot.CanUse(Focus, free)) return false;
            if (!CanActivateSpecial(index)) return false;
            slot.TryConsume(Focus, free);
            ActivateSpecial(index);
            return true;
        }

        public bool UseUltimate(bool free)
        {
            if (Ultimate == null || IsBusy || status.IsIncapacitated) return false;
            if (!Ultimate.CanUse(Focus, free)) return false;
            Ultimate.TryConsume(Focus, free);
            ActivateUltimate();
            return true;
        }

        protected virtual bool CanActivateSpecial(int index) => true;
        protected virtual bool IsTraversalAbility(int index) => false;
        protected abstract void ActivateSpecial(int index);
        protected abstract void ActivateUltimate();

        // ───────────────────────── Light chain ─────────────────────────
        protected abstract AttackData[] LightChain { get; }

        protected virtual void OnLight()
        {
            if (status.IsIncapacitated) return;
            var chain = LightChain;
            if (Time.time > comboExpiresAt) comboIndex = 0;
            if (comboIndex >= chain.Length) comboIndex = 0;
            var atk = chain[comboIndex];
            comboIndex++;
            StartAction(AttackRoutine(atk, 1f, ModifyLightHit), atk.name);
        }

        /// <summary>Kit hook to tweak a light hit per target (Bolas follow-up bonus, Fang setup bonus).</summary>
        protected virtual void ModifyLightHit(CombatTarget target, ref DamageInfo info) { }

        protected delegate void HitModifier(CombatTarget target, ref DamageInfo info);

        // ───────────────────────── Action runner ─────────────────────────
        protected bool StartAction(IEnumerator routine, string actionName, bool interruptible = true)
        {
            if (IsBusy) return false;
            currentActionName = actionName;
            currentInterruptible = interruptible;
            busy = true;
            int serial = ++actionSerial;
            // A routine can finish synchronously inside StartCoroutine (e.g. yield break on no stamina);
            // only keep the handle if it is still the running action (see LESSONS_LEARNED.md L-004).
            var co = StartCoroutine(RunAction(routine, serial));
            if (busy && serial == actionSerial) currentAction = co;
            return true;
        }

        IEnumerator RunAction(IEnumerator routine, int serial)
        {
            yield return routine;
            if (serial != actionSerial) yield break; // cancelled/replaced meanwhile
            busy = false;
            currentAction = null;
            currentActionName = "";
            motor.movementLocked = false;
            motor.rotationLocked = false;
            OnActionFinished();
            if (Time.time <= bufferedLightUntil)
            {
                bufferedLightUntil = -1f;
                OnLight();
            }
        }

        protected virtual void OnActionFinished() { }

        public void CancelAction()
        {
            if (currentAction != null) StopCoroutine(currentAction);
            currentAction = null;
            busy = false;
            actionSerial++;
            currentActionName = "";
            if (motor != null)
            {
                motor.movementLocked = false;
                motor.rotationLocked = false;
                motor.gravityEnabled = true;
            }
            OnActionCancelled();
        }

        protected virtual void OnActionCancelled() { }

        protected virtual void OnInterrupted()
        {
            if (!currentInterruptible && IsBusy) return;
            CancelAction();
            comboIndex = 0;
            StartAction(HitStun(), "Hit stun");
        }

        IEnumerator HitStun()
        {
            motor.movementLocked = true;
            yield return Wait(hitStunDuration);
        }

        protected IEnumerator Wait(float seconds)
        {
            float t = 0f;
            while (t < seconds)
            {
                t += motor.Delta;
                yield return null;
            }
        }

        protected IEnumerator AttackRoutine(AttackData atk, float damageScale, HitModifier modifier = null, System.Action<List<CombatTarget>> onHits = null)
        {
            if (atk.staminaCost > 0f && !stamina.TrySpend(atk.staminaCost)) yield break;

            motor.movementLocked = true;
            FaceAim();
            yield return Wait(atk.windup);

            if (atk.lunge > 0f) motor.Dash(transform.forward, atk.lunge, 0.08f);
            var hits = MeleeHit(atk, damageScale, modifier);
            onHits?.Invoke(hits);
            yield return Wait(atk.active + atk.recovery);
            comboExpiresAt = Time.time + comboWindow;
        }

        protected List<CombatTarget> MeleeHit(AttackData atk, float damageScale, HitModifier modifier = null)
        {
            var targets = CombatQuery.Cone(self, transform.position, transform.forward, atk.range, atk.halfAngle);
            foreach (var t in targets)
            {
                var info = BuildDamage(atk, t, damageScale);
                modifier?.Invoke(t, ref info);
                t.ReceiveHit(info);
                CombatPools.Spark(t.AimPoint);
            }
            return targets;
        }

        protected DamageInfo BuildDamage(AttackData atk, CombatTarget target, float scale)
        {
            float amount = atk.damage * scale * damageMultiplier * self.outgoingDamageMultiplier;
            if (target.Status.IsMarked) amount *= target.Status.MarkBonus;
            return new DamageInfo
            {
                Amount = amount,
                Attacker = self,
                Point = target.AimPoint,
                Direction = (target.transform.position - transform.position).normalized,
                Flags = atk.flags,
                GuardBreakPower = atk.guardBreakPower,
                StaggerDuration = atk.staggerDuration,
                Knockback = atk.knockback,
                AttackName = atk.name,
            };
        }

        protected void FaceAim()
        {
            var t = CurrentTarget();
            if (t != null) motor.FaceTowards(t.transform.position, true);
        }

        /// <summary>Lock-on target, else the nearest hostile in front (soft aim assist).</summary>
        protected CombatTarget CurrentTarget(float range = 6f)
        {
            if (character != null && character.Targeting != null && character.Targeting.Locked != null) return character.Targeting.Locked;
            if (character != null && character.AllyTarget != null) return character.AllyTarget;
            return CombatQuery.BestInCone(self, transform.position, transform.forward, range, 70f);
        }

        // ───────────────────────── Focus ─────────────────────────
        protected virtual void OnHitDealt(CombatTarget target, DamageInfo info, HitResult result)
        {
            if (!result.Landed || target.isDecoy) return;
            Focus.Add(FocusRules.CleanHit, "Clean hit");
            TryTellReadBonus(target);
        }

        /// <summary>03 Section 0: post-M3.5, successfully acting during an enemy's tell also fills Focus.</summary>
        protected void TryTellReadBonus(CombatTarget enemy)
        {
            if (enemy == null || !Progression.IsUnlocked(Unlocks.TellReading)) return;
            var brain = enemy.GetComponent<EnemyBrain>();
            if (brain != null && brain.IsTelegraphing) Focus.Add(FocusRules.TellRead, "Tell read");
        }

        protected void ResetKitState()
        {
            comboIndex = 0;
            damageMultiplier = 1f;
        }

        public virtual void OnRespawn()
        {
            CancelAction();
            ResetKitState();
        }
    }
}
