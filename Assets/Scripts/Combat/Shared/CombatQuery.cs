// Implements: 03-combat-design.md (melee cones, AoE radii, ranged marks) — distance/angle queries over CombatTarget.All.
// Always returns a NEW list: hits can kill/despawn targets, which mutates CombatTarget.All mid-iteration (see LESSONS_LEARNED.md).
using System.Collections.Generic;
using UnityEngine;

namespace Ruminahui
{
    public static class CombatQuery
    {
        public static bool IsHostileTo(CombatTarget self, CombatTarget other, bool includeDecoys = true)
        {
            if (other == null || other == self || !other.IsTargetable) return false;
            if (!includeDecoys && other.isDecoy) return false;
            return FactionUtil.AreHostile(self.faction, other.faction);
        }

        public static List<CombatTarget> Cone(CombatTarget self, Vector3 origin, Vector3 forward, float range, float halfAngleDeg, bool includeDecoys = true)
        {
            var result = new List<CombatTarget>();
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.0001f) forward = self.transform.forward;
            forward.Normalize();
            foreach (var t in CombatTarget.All)
            {
                if (!IsHostileTo(self, t, includeDecoys)) continue;
                var to = t.transform.position - origin;
                to.y = 0f;
                float dist = to.magnitude - t.radius;
                if (dist > range) continue;
                if (to.sqrMagnitude > 0.01f && Vector3.Angle(forward, to) > halfAngleDeg) continue;
                if (Mathf.Abs(t.transform.position.y - origin.y) > 2.5f) continue; // no hitting things on another floor
                result.Add(t);
            }
            return result;
        }

        public static List<CombatTarget> Sphere(CombatTarget self, Vector3 center, float radius, bool includeDecoys = true)
        {
            var result = new List<CombatTarget>();
            foreach (var t in CombatTarget.All)
            {
                if (!IsHostileTo(self, t, includeDecoys)) continue;
                if ((t.transform.position - center).magnitude - t.radius <= radius) result.Add(t);
            }
            return result;
        }

        /// <summary>Nearest hostile. With preferAggro, higher aggroPriority wins over distance (decoys bait enemies).</summary>
        public static CombatTarget Nearest(CombatTarget self, Vector3 origin, float maxRange, System.Predicate<CombatTarget> filter = null, bool preferAggro = false)
        {
            CombatTarget best = null;
            float bestScore = float.MaxValue;
            foreach (var t in CombatTarget.All)
            {
                if (!IsHostileTo(self, t)) continue;
                if (filter != null && !filter(t)) continue;
                float d = Vector3.Distance(origin, t.transform.position);
                if (d > maxRange) continue;
                float score = d - (preferAggro ? t.aggroPriority * 100f : 0f);
                if (score < bestScore) { bestScore = score; best = t; }
            }
            return best;
        }

        /// <summary>Best target inside a forward cone (lock-on / marks), weighted toward the cone centre.</summary>
        public static CombatTarget BestInCone(CombatTarget self, Vector3 origin, Vector3 forward, float range, float halfAngleDeg, CombatTarget exclude = null)
        {
            CombatTarget best = null;
            float bestScore = float.MaxValue;
            forward.y = 0f;
            forward.Normalize();
            foreach (var t in CombatTarget.All)
            {
                if (t == exclude || !IsHostileTo(self, t, false)) continue;
                var to = t.transform.position - origin;
                to.y = 0f;
                float d = to.magnitude;
                if (d > range) continue;
                float angle = Vector3.Angle(forward, to);
                if (angle > halfAngleDeg) continue;
                float score = d + angle * 0.15f;
                if (score < bestScore) { bestScore = score; best = t; }
            }
            return best;
        }

        public static List<CombatTarget> AllHostile(CombatTarget self, System.Predicate<CombatTarget> filter = null)
        {
            var result = new List<CombatTarget>();
            foreach (var t in CombatTarget.All)
                if (IsHostileTo(self, t, false) && (filter == null || filter(t))) result.Add(t);
            return result;
        }
    }
}
