// Implements: 03-combat-design.md, Section 2 (Vantage Leap "launches to a height point") and 06 M5.3 Obstacle 2 (Wide Break).
using System.Collections.Generic;
using UnityEngine;

namespace Ruminahui
{
    public class VantagePoint : MonoBehaviour
    {
        public static readonly List<VantagePoint> All = new List<VantagePoint>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => All.Clear();

        void OnEnable() => All.Add(this);
        void OnDisable() => All.Remove(this);

        /// <summary>Best point within range, favouring ones in front of the jumper.</summary>
        public static VantagePoint Find(Vector3 from, Vector3 forward, float maxRange)
        {
            VantagePoint best = null;
            float bestScore = float.MaxValue;
            forward.y = 0f;
            foreach (var v in All)
            {
                var to = v.transform.position - from;
                float d = to.magnitude;
                if (d > maxRange || d < 1.5f) continue;
                to.y = 0f;
                float angle = to.sqrMagnitude > 0.01f ? Vector3.Angle(forward, to) : 0f;
                if (angle > 75f) continue;
                float score = d + angle * 0.1f;
                if (score < bestScore) { bestScore = score; best = v; }
            }
            return best;
        }
    }
}
