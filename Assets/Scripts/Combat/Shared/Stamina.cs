// Implements: 03-combat-design.md, Section 0 — STA pays for dodges, blocks/parries and heavies; regenerates passively but drains fast under pressure.
using UnityEngine;

namespace Ruminahui
{
    public class Stamina : MonoBehaviour
    {
        public float max = 100f;
        public float regenPerSecond = 30f;   // PLACEHOLDER-BALANCE: full bar in ~3.3s of not spending
        public float regenDelay = 0.7f;      // PLACEHOLDER-BALANCE: short pause after spending so pressure matters
        [Tooltip("Upgrade hook: Puma Foundation tier lowers this.")]
        public float costMultiplier = 1f;
        [Tooltip("Set by block-hold etc. so regen pauses while the drain is active.")]
        public bool regenBlocked;

        public float Current { get; private set; }
        public float Normalized => max <= 0f ? 0f : Current / max;
        public bool IsExhausted => Current <= 0.01f;

        float lastSpendTime = -100f;
        bool initialized;

        void Awake() => EnsureInit();

        void EnsureInit()
        {
            if (initialized) return;
            initialized = true;
            Current = max;
        }

        public void ResetFull()
        {
            initialized = true;
            Current = max;
        }

        /// <summary>Spends stamina. Allowed while any stamina remains (the bar can dip to 0) — exhausted means refused.</summary>
        public bool TrySpend(float amount)
        {
            EnsureInit();
            if (IsExhausted) return false;
            Current = Mathf.Max(0f, Current - amount * costMultiplier);
            lastSpendTime = Time.time;
            return true;
        }

        /// <summary>Forced drain (e.g. absorbing a blocked hit). Returns true if the bar ran out.</summary>
        public bool Drain(float amount)
        {
            EnsureInit();
            Current = Mathf.Max(0f, Current - amount * costMultiplier);
            lastSpendTime = Time.time;
            return IsExhausted;
        }

        public void Tick(float dt)
        {
            EnsureInit();
            if (regenBlocked) return;
            if (Time.time - lastSpendTime < regenDelay) return;
            Current = Mathf.Min(max, Current + regenPerSecond * dt);
        }

        void Update() => Tick(Time.deltaTime);
    }
}
