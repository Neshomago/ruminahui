// Testing aid: current objective/stage text shown in the on-screen test hint panel (approved "controls on screen" item).
using UnityEngine;

namespace Ruminahui
{
    public static class ObjectiveTracker
    {
        public static string Current { get; private set; } = "";
        public static string LastLine { get; private set; } = "";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { Current = ""; LastLine = ""; }

        public static void Set(string objective)
        {
            Current = objective;
            Debug.Log($"[Objective] {objective}");
        }

        /// <summary>Placeholder for dialogue/VO until the subtitle system exists (deferred per the UI plan).</summary>
        public static void Say(string speaker, string line)
        {
            LastLine = $"{speaker}: {line}";
            Debug.Log($"[Dialogue] {LastLine}");
        }
    }
}
