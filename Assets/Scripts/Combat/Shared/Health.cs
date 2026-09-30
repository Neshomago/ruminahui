// Implements: 03-combat-design.md, Section 0 — HP has NO passive regen; it is restored only at huacas (see Huaca.cs).
using UnityEngine;

namespace Ruminahui
{
    public class Health : MonoBehaviour
    {
        public float max = 100f;
        [Tooltip("HP can't drop below this. Bosses that must be spared and AI allies use 1.")]
        public float minimumHp;

        public float Current { get; private set; }
        public bool IsDead { get; private set; }
        public float Normalized => max <= 0f ? 0f : Current / max;

        public event System.Action<float, float> Changed;   // current, max
        public event System.Action Died;
        public event System.Action Revived;

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
            IsDead = false;
            Current = max;
            Changed?.Invoke(Current, max);
        }

        public void SetMax(float newMax, bool refill)
        {
            EnsureInit();
            max = Mathf.Max(1f, newMax);
            if (refill) Current = max;
            Current = Mathf.Min(Current, max);
            Changed?.Invoke(Current, max);
        }

        /// <summary>Raw damage (interceptors already ran in CombatTarget). Returns the HP actually removed.</summary>
        public float ApplyDamage(float amount)
        {
            EnsureInit();
            if (IsDead || amount <= 0f) return 0f;
            float before = Current;
            Current = Mathf.Max(minimumHp, Current - amount);
            Changed?.Invoke(Current, max);
            if (Current <= 0f)
            {
                IsDead = true;
                Died?.Invoke();
            }
            return before - Current;
        }

        public void Heal(float amount)
        {
            EnsureInit();
            if (IsDead || amount <= 0f) return;
            Current = Mathf.Min(max, Current + amount);
            Changed?.Invoke(Current, max);
        }

        public void SetCurrent(float value)
        {
            EnsureInit();
            Current = Mathf.Clamp(value, 0f, max);
            if (IsDead && Current > 0f) { IsDead = false; Revived?.Invoke(); }
            Changed?.Invoke(Current, max);
        }

        public void Kill()
        {
            EnsureInit();
            if (IsDead) return;
            Current = 0f;
            IsDead = true;
            Changed?.Invoke(Current, max);
            Died?.Invoke();
        }
    }
}
