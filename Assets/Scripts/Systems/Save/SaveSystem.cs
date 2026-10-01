// Implements: approved save format (2026-10-01) — JSON save + settings files in Application.persistentDataPath.
// Saves on mission complete and at huacas; settings save whenever they change.
using System.IO;
using UnityEngine;

namespace Ruminahui
{
    public class SaveSystem : Singleton<SaveSystem>
    {
        public const string SaveFileName = "ruminahui_save.json";
        public const string SettingsFileName = "ruminahui_settings.json";

        public GameSettings Settings { get; private set; } = new GameSettings();
        public SaveData LastSave { get; private set; }
        public bool HasSave => LastSave != null;

        string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);
        string SettingsPath => Path.Combine(Application.persistentDataPath, SettingsFileName);

        protected override void Awake()
        {
            base.Awake();
            LoadSettings();
            LastSave = ReadSave();
        }

        void Start() => ApplySettings();

        // ───────── Save ─────────
        public SaveData ReadSave()
        {
            try
            {
                if (!File.Exists(SavePath)) return null;
                var data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
                if (data == null || !data.IsValid()) { Debug.LogWarning("[Save] Save file invalid — ignoring."); return null; }
                return data;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[Save] Could not read save: " + e.Message);
                return null;
            }
        }

        public void Save(string currentMission)
        {
            if (!MissionDatabase.IsMission(currentMission)) return; // test scenes / boot never overwrite the campaign save
            var data = SaveData.Capture(currentMission, System.DateTime.UtcNow.ToString("o"));
            try
            {
                File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
                LastSave = data;
                Debug.Log($"[Save] Saved at {currentMission} → {SavePath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError("[Save] Could not write save: " + e.Message);
            }
        }

        /// <summary>Continue: restore progression, return the mission to load.</summary>
        public string LoadForContinue()
        {
            LastSave = ReadSave();
            if (LastSave == null) return null;
            LastSave.ApplyToProgression();
            return LastSave.currentMission;
        }

        public void NewGame()
        {
            Progression.ResetAll();
            Save(MissionDatabase.Missions[0].Id);
        }

        public void DeleteSave()
        {
            try { if (File.Exists(SavePath)) File.Delete(SavePath); } catch (System.Exception e) { Debug.LogWarning(e.Message); }
            LastSave = null;
        }

        // ───────── Settings ─────────
        void LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsPath)) Settings = (JsonUtility.FromJson<GameSettings>(File.ReadAllText(SettingsPath)) ?? new GameSettings()).Clamp();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[Save] Could not read settings: " + e.Message);
                Settings = new GameSettings();
            }
        }

        public void SaveSettings()
        {
            Settings.Clamp();
            try { File.WriteAllText(SettingsPath, JsonUtility.ToJson(Settings, true)); }
            catch (System.Exception e) { Debug.LogWarning("[Save] Could not write settings: " + e.Message); }
            ApplySettings();
        }

        public void ApplySettings()
        {
            var s = Settings;
            AudioListener.volume = s.masterVolume;
            TellFlashSettings.Enabled = s.tellFlashes;
            DamageNumbers.Enabled = s.damageNumbers;
            if (GameInput.Instance != null)
            {
                GameInput.Instance.mouseSensitivity = s.mouseSensitivity;
                GameInput.Instance.stickSensitivity = s.stickSensitivity;
            }
            ApplyQuality(s.qualityTier);
        }

        /// <summary>Quality level 0 holds the URP Performance asset; the highest level holds Standard (see RuminahuiSetupMenu).</summary>
        static void ApplyQuality(int tier)
        {
            int count = QualitySettings.names.Length;
            if (count == 0) return;
            int level = tier == GameSettings.QualityPerformance ? 0 : count - 1;
            if (QualitySettings.GetQualityLevel() != level) QualitySettings.SetQualityLevel(level, true);
        }
    }
}
