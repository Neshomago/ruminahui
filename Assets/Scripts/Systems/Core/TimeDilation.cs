// Implements: 03-combat-design.md, Section 2 (Kuntur Ultimate "The Gap in the Line" — time slows for everyone but Chaska).
// Instead of Time.timeScale (which would also slow Chaska), every gameplay object asks for its own delta time.
using UnityEngine;

namespace Ruminahui
{
    public static class TimeDilation
    {
        public static float SlowScale { get; private set; } = 1f;
        static GameObject exempt;
        static float slowUntil;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            SlowScale = 1f;
            exempt = null;
            slowUntil = 0f;
        }

        public static bool IsSlowActive => Time.time < slowUntil;

        /// <summary>Slows the world for <paramref name="duration"/> seconds, except for <paramref name="exemptRoot"/> and anything it owns.</summary>
        public static void StartSlow(float scale, float duration, GameObject exemptRoot)
        {
            SlowScale = Mathf.Clamp(scale, 0.05f, 1f);
            exempt = exemptRoot;
            slowUntil = Time.time + duration;
        }

        public static void StopSlow()
        {
            slowUntil = 0f;
            SlowScale = 1f;
            exempt = null;
        }

        public static float ScaleFor(GameObject go)
        {
            if (!IsSlowActive) return 1f;
            if (go != null && exempt != null && (go == exempt || go.transform.IsChildOf(exempt.transform))) return 1f;
            return SlowScale;
        }

        public static float DeltaFor(GameObject go) => Time.deltaTime * ScaleFor(go);
    }
}
