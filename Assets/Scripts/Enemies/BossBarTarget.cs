// Implements: approved UI plan item 5 (boss bar: name, HP, tick marks at the phase thresholds) — marks an enemy as boss-tier.
using System.Collections.Generic;
using UnityEngine;

namespace Ruminahui
{
    public class BossBarTarget : MonoBehaviour
    {
        public static BossBarTarget Current { get; private set; }

        public string displayName = "Boss";
        public float showRange = 25f;
        /// <summary>Normalised HP thresholds drawn as ticks (Atoc: 0.65 and 0.30 — 08 Section 2).</summary>
        public List<float> phaseTicks = new List<float>();

        public Health Health { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => Current = null;

        void Awake() => Health = GetComponent<Health>();

        void Update()
        {
            if (Health == null || Health.IsDead) { if (Current == this) Current = null; return; }
            var c = PartyManager.Instance != null ? PartyManager.Instance.Controlled : null;
            bool near = c != null && Vector3.Distance(c.transform.position, transform.position) <= showRange;
            if (near && Current == null) Current = this;
            else if (!near && Current == this) Current = null;
        }

        void OnDisable() { if (Current == this) Current = null; }
    }
}
