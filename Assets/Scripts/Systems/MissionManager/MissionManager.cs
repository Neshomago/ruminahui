// Implements: 02-mission-list.md — scene sequencing through the 26 missions (M0.1 → M5.7), per-mission unlock state
// (03 Acts), and completion → next mission. Streaming via SceneStreamer (11 Part B Step 3).
using System.Collections;
using UnityEngine;

namespace Ruminahui
{
    public class MissionManager : Singleton<MissionManager>
    {
        public MissionDefinition Current { get; private set; }
        public string CurrentContentId { get; private set; } = "";
        public bool IsTestScene => CurrentContentId.StartsWith("Test_");
        public event System.Action<string> SceneStarted;

        bool transitioning;

        /// <summary>Called by SceneSetup when a scene begins (whether streamed in or opened directly in the editor).</summary>
        public void OnSceneStarted(SceneSetup setup)
        {
            CurrentContentId = setup.contentId;
            Current = MissionDatabase.Get(setup.contentId);
            CheckpointService.Instance?.ClearForNewScene();
            ObjectiveTracker.Set("");
            TimeDilation.StopSlow();
            DifficultyScaler.EnemyDamage = 1f;
            if (GameInput.Instance != null) GameInput.Instance.ClearCutsceneBlocks();
            PartyManager.Instance?.ClearMembers();
            AllyCommandSystem.Instance?.ResetCooldowns();
            StealthSystem.Instance?.ClearScene();
            if (DialogueRunner.Shared != null) { DialogueRunner.Shared.Stop(); DialogueRunner.Shared.AnnouncePrompts = true; }

            if (Current != null)
            {
                Progression.SetUnlockAll(false);
                // Opened directly in the editor (nothing played yet): count earlier missions as played so points/unlocks make sense.
                if (!Progression.CompletedMissions.GetEnumerator().MoveNext()) Progression.CatchUpTo(Current.Id);
                Progression.ApplyForMission(Current.Id);
                Debug.Log($"[Mission] {Current.Id} — {Current.Title}");
            }
            else if (IsTestScene)
            {
                Progression.SetUnlockAll(true); // test scenes expose every ability; tiers adjustable in the debug panel
            }
            SceneStarted?.Invoke(setup.contentId);
        }

        public void StartMission(string id)
        {
            if (!transitioning) StartCoroutine(Load(id));
        }

        public void CompleteCurrent()
        {
            if (Current == null)
            {
                ObjectiveTracker.Set("Test complete. (Missions advance automatically; test scenes don't.)");
                return;
            }
            var next = MissionDatabase.Next(Current.Id);
            Debug.Log($"[Mission] {Current.Id} complete → {(next != null ? next.Id : "end of campaign")}");
            Progression.MarkCompleted(Current.Id);
            if (Current.AwardsUpgradePoint) ObjectiveTracker.Say("Upgrades", $"+1 upgrade point ({UpgradeEconomy.AvailablePoints()} available — Esc ▸ Upgrades)");
            if (SaveSystem.Instance != null) SaveSystem.Instance.Save(next != null ? next.Id : Current.Id);
            StartMission(next != null ? next.Id : MissionDatabase.BootScene);
        }

        /// <summary>Mission list (chapter select): earlier missions count as played so the economy catches up.</summary>
        public void StartFromChapterSelect(string id)
        {
            if (MissionDatabase.IsMission(id)) Progression.CatchUpTo(id);
            StartMission(id);
        }

        public void ContinueFromSave()
        {
            var id = SaveSystem.Instance != null ? SaveSystem.Instance.LoadForContinue() : null;
            if (id != null) StartMission(id);
        }

        public void NewGame()
        {
            if (SaveSystem.Instance != null) SaveSystem.Instance.NewGame();
            else Progression.ResetAll();
            StartMission(MissionDatabase.Missions[0].Id);
        }

        public void RestartCurrent()
        {
            if (!string.IsNullOrEmpty(CurrentContentId)) StartMission(CurrentContentId);
        }

        IEnumerator Load(string id)
        {
            transitioning = true;
            var fader = ScreenFader.Instance;
            if (fader != null) yield return fader.FadeOut(0.35f, Color.black);
            PoolManager.Instance.DespawnAll();
            TimeDilation.StopSlow();
            yield return SceneStreamer.Instance.LoadExclusive(id);
            if (fader != null) yield return fader.FadeIn(0.35f);
            transitioning = false;
        }
    }
}
