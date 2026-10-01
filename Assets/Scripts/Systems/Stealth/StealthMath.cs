// Implements: 02 M0.1 / M5.5 stealth ("move from cover to cover", "tense stealth/evasion") — pure sight + detection rules.
using UnityEngine;

namespace Ruminahui
{
    public static class StealthMath
    {
        public const float FillPerSecondAtRange = 0.35f;  // PLACEHOLDER-BALANCE: ~3 s to be spotted at the edge of sight
        public const float FillPerSecondClose = 1.6f;     // PLACEHOLDER-BALANCE: ~0.6 s up close
        public const float DecayPerSecond = 0.25f;        // PLACEHOLDER-BALANCE

        /// <summary>Inside the watcher's view cone (distance + angle only; line-of-sight is checked separately).</summary>
        public static bool InViewCone(Vector3 eye, Vector3 forward, Vector3 target, float range, float halfAngle)
        {
            var to = target - eye;
            if (to.magnitude > range) return false;
            var flatTo = new Vector3(to.x, 0f, to.z);
            var flatFwd = new Vector3(forward.x, 0f, forward.z);
            if (flatTo.sqrMagnitude < 0.01f) return true;
            return Vector3.Angle(flatFwd, flatTo) <= halfAngle;
        }

        /// <summary>Detection meter step. Visible: fills faster the closer you are. Not visible: decays.</summary>
        public static float Step(float current, bool visible, float distance, float range, float dt)
        {
            if (!visible) return Mathf.Max(0f, current - DecayPerSecond * dt);
            float closeness = range <= 0f ? 1f : 1f - Mathf.Clamp01(distance / range);
            float rate = Mathf.Lerp(FillPerSecondAtRange, FillPerSecondClose, closeness);
            return Mathf.Min(1f, current + rate * dt);
        }

        /// <summary>White → amber → red (approved U2).</summary>
        public static Color MeterColor(float detection)
        {
            var amber = new Color(1f, 0.7f, 0.2f);
            return detection < 0.5f ? Color.Lerp(Color.white, amber, detection * 2f) : Color.Lerp(amber, new Color(1f, 0.2f, 0.15f), (detection - 0.5f) * 2f);
        }
    }
}
