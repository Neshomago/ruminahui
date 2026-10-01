// Implements: approved save format (2026-10-01) — JSON in Application.persistentDataPath; auto-save at huacas and on mission complete.
// Continue resumes at the START of the saved mission (checkpoints inside a mission aren't persisted — ASSUMPTION, see log).
namespace Ruminahui
{
    [System.Serializable]
    public class SaveData
    {
        public const int CurrentVersion = 1;

        public int version = CurrentVersion;
        public string currentMission = "M0.1";
        public string[] completedMissions = new string[0];
        public string[] relics = new string[0];
        public int[] tiers = new int[3];
        public string savedAtUtc = "";

        public static SaveData Capture(string currentMission, string timestampUtc)
        {
            var d = new SaveData { currentMission = currentMission, savedAtUtc = timestampUtc };
            d.completedMissions = new System.Collections.Generic.List<string>(Progression.CompletedMissions).ToArray();
            d.relics = new System.Collections.Generic.List<string>(Progression.CollectedIds).ToArray();
            d.tiers = new[] { Progression.GetTier(KitId.Puma), Progression.GetTier(KitId.Kuntur), Progression.GetTier(KitId.Amaru) };
            return d;
        }

        public void ApplyToProgression() => Progression.Restore(completedMissions, relics, tiers);

        /// <summary>Guards against hand-edited or older files.</summary>
        public bool IsValid() => version >= 1 && MissionDatabase.IsMission(currentMission);
    }

    [System.Serializable]
    public class GameSettings
    {
        public const int QualityStandard = 0;
        public const int QualityPerformance = 1;

        public int qualityTier = QualityStandard;     // 11-camera-performance.md Part B Step 9
        public float masterVolume = 0.8f;
        public float musicVolume = 0.7f;              // no music yet — stored for when it lands
        public float mouseSensitivity = 0.12f;
        public float stickSensitivity = 160f;
        public bool subtitles = true;
        public bool tellFlashes = true;
        public bool damageNumbers = true;

        public GameSettings Clamp()
        {
            masterVolume = UnityEngine.Mathf.Clamp01(masterVolume);
            musicVolume = UnityEngine.Mathf.Clamp01(musicVolume);
            mouseSensitivity = UnityEngine.Mathf.Clamp(mouseSensitivity, 0.02f, 0.5f);
            stickSensitivity = UnityEngine.Mathf.Clamp(stickSensitivity, 40f, 400f);
            qualityTier = qualityTier == QualityPerformance ? QualityPerformance : QualityStandard;
            return this;
        }
    }
}
