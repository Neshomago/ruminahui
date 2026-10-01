// Implements: approved UI item U3 data — what the escort panel shows (title + optional countdown). Directors set it.
namespace Ruminahui
{
    public static class EscortHud
    {
        public static string Title = "";
        /// <summary>Seconds left, or a negative number for "no timer".</summary>
        public static float TimeRemaining = -1f;
        public static bool Visible;

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => Clear();

        public static void Clear()
        {
            Title = "";
            TimeRemaining = -1f;
            Visible = false;
        }

        /// <summary>"2:05" style.</summary>
        public static string FormatTime(float seconds)
        {
            if (seconds < 0f) return "";
            int s = (int)System.Math.Ceiling(seconds);
            return $"{s / 60}:{s % 60:00}";
        }
    }
}
