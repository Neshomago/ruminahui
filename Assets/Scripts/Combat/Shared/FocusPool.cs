// Implements: 03-combat-design.md, Section 0 — Focus is ONE meter shared by all three kits so swaps (M3.5, M5.3) keep momentum.
// Owned by PartyManager; plain C# so it is trivially unit-testable.
namespace Ruminahui
{
    public class FocusPool
    {
        public float Max { get; private set; }
        public float Current { get; private set; }
        public float Normalized => Max <= 0f ? 0f : Current / Max;

        public event System.Action<float, float> Changed;      // current, max
        public event System.Action<string, float> Gained;      // reason, amount (debug panel feed)

        public FocusPool(float max = FocusRules.Max, float start = 0f)
        {
            Max = max;
            Current = UnityEngine.Mathf.Clamp(start, 0f, max);
        }

        public void Add(float amount, string reason)
        {
            if (amount <= 0f) return;
            float before = Current;
            Current = UnityEngine.Mathf.Min(Max, Current + amount);
            if (Current > before)
            {
                Gained?.Invoke(reason, Current - before);
                Changed?.Invoke(Current, Max);
            }
        }

        public bool Has(float amount) => Current + 0.001f >= amount;

        public bool TrySpend(float amount)
        {
            if (!Has(amount)) return false;
            Current = UnityEngine.Mathf.Max(0f, Current - amount);
            Changed?.Invoke(Current, Max);
            return true;
        }

        public void Fill()
        {
            Current = Max;
            Changed?.Invoke(Current, Max);
        }

        public void Clear()
        {
            Current = 0f;
            Changed?.Invoke(Current, Max);
        }
    }
}
