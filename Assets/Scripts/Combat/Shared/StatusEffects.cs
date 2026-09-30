// Implements: 03-combat-design.md — stagger (Stone Strikes 3rd hit, Stone Parry), knockdown (Earthbreaker), wrap (Bolas Snap),
// root (Snare), poison (Venom Riposte), slow (Numbing Draught), mark (Condor's Eye), hyper-armour (Unyielding).
using System.Collections.Generic;
using UnityEngine;

namespace Ruminahui
{
    public class StatusEffects : MonoBehaviour
    {
        [Tooltip("Multiplies incoming stagger durations. Bosses use < 1.")]
        public float staggerResistance = 1f;

        float staggerUntil, knockdownUntil, rootUntil, wrapUntil, poisonUntil, markUntil, hyperArmorUntil;
        float poisonDps;
        float poisonTickTimer;
        readonly List<Vector2> slows = new List<Vector2>(); // x = multiplier, y = until
        Health health;

        public bool IsStaggered => Time.time < staggerUntil;
        public bool IsKnockedDown => Time.time < knockdownUntil;
        public bool IsRooted => Time.time < rootUntil;
        public bool IsWrapped => Time.time < wrapUntil;
        public bool IsPoisoned => Time.time < poisonUntil;
        public bool IsMarked => Time.time < markUntil;
        public bool HasHyperArmor => Time.time < hyperArmorUntil;
        /// <summary>Held forever by the boss "spare" state until cleared.</summary>
        public bool ForcedStagger;
        public float MarkBonus { get; private set; } = 1f;

        /// <summary>Can't act (attacks/moves). Used by AI and by Grounding Throw / Stone Face target checks.</summary>
        public bool IsIncapacitated => IsStaggered || IsKnockedDown || IsRooted || ForcedStagger;
        /// <summary>"Already staggered" for Stone Face / Grapple — knockdown and wraps count as openings too.</summary>
        public bool IsOpen => IsStaggered || IsKnockedDown || IsWrapped || ForcedStagger;
        /// <summary>Any active debuff — Amaru's Fang Strikes are "built around setups".</summary>
        public bool HasSetupDebuff => IsPoisoned || IsRooted || IsWrapped || SlowMultiplier < 0.99f;

        public float SlowMultiplier
        {
            get
            {
                float m = 1f;
                for (int i = slows.Count - 1; i >= 0; i--)
                {
                    if (Time.time >= slows[i].y) { slows.RemoveAt(i); continue; }
                    m *= slows[i].x;
                }
                return m;
            }
        }

        public event System.Action<float> Staggered;
        public event System.Action Interrupted;

        void Awake() => health = GetComponent<Health>();

        public void ClearAll()
        {
            staggerUntil = knockdownUntil = rootUntil = wrapUntil = poisonUntil = markUntil = hyperArmorUntil = 0f;
            poisonDps = 0f;
            slows.Clear();
            ForcedStagger = false;
            MarkBonus = 1f;
        }

        public void ApplyStagger(float duration)
        {
            duration *= staggerResistance;
            if (duration <= 0f) return;
            staggerUntil = Mathf.Max(staggerUntil, Time.time + duration);
            Staggered?.Invoke(duration);
        }

        public void ClearStagger() => staggerUntil = 0f;

        public void ApplyKnockdown(float duration)
        {
            knockdownUntil = Mathf.Max(knockdownUntil, Time.time + duration * staggerResistance);
            Staggered?.Invoke(duration);
        }

        public void ApplyRoot(float duration) => rootUntil = Mathf.Max(rootUntil, Time.time + duration);
        public void ApplyWrap(float duration) { wrapUntil = Mathf.Max(wrapUntil, Time.time + duration); ApplyRoot(duration); }
        public void ConsumeWrap() { wrapUntil = 0f; rootUntil = 0f; }
        public void ApplySlow(float multiplier, float duration) => slows.Add(new Vector2(Mathf.Clamp01(multiplier), Time.time + duration));
        public void ApplyHyperArmor(float duration) => hyperArmorUntil = Mathf.Max(hyperArmorUntil, Time.time + duration);
        public void ClearHyperArmor() => hyperArmorUntil = 0f;

        public void ApplyMark(float duration, float bonus)
        {
            markUntil = Mathf.Max(markUntil, Time.time + duration);
            MarkBonus = Mathf.Max(1f, bonus);
        }

        public void ClearMark() { markUntil = 0f; MarkBonus = 1f; }

        public void ApplyPoison(float dps, float duration)
        {
            poisonDps = Mathf.Max(poisonDps, dps);
            poisonUntil = Mathf.Max(poisonUntil, Time.time + duration);
        }

        /// <summary>Called by CombatTarget after an un-blocked hit. Hyper-armour (Unyielding) swallows the interrupt.</summary>
        public void NotifyHitTaken()
        {
            if (HasHyperArmor) return;
            Interrupted?.Invoke();
        }

        void Update()
        {
            if (!IsPoisoned || health == null || health.IsDead) return;
            poisonTickTimer += Time.deltaTime;
            if (poisonTickTimer >= 0.5f)
            {
                poisonTickTimer = 0f;
                health.ApplyDamage(poisonDps * 0.5f); // DoT bypasses block — it's already in the blood
            }
        }
    }
}
