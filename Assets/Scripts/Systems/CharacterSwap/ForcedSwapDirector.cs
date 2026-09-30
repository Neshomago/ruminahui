// Implements: 05-m3-5-flow-diagram.md in full — M3.5 "What Held the Line" as ONE continuous level:
//   A–C  Rumiñahui holds the breach → line fails → scripted stagger + vision flash (Willka VO, audio only)
//   D–F  CONTROL SHIFT 1: Chaska fights across the flank to the breach (tuned slightly easier)
//   G–I  CONTROL SHIFT 2: Atoc breaks free, fights through the rear line to the huaca (tuned slightly easier)
//   J–K  Convergence: all three together, dedicated cinematic shot, no combat UI
//   L    Final wave: Rumiñahui again, Chaska + Atoc as full AI allies (the real difficulty spike)
//   N    Shared vision → "What Held the Line" triggers;  O  victory beat;  P  → M3.6
// Technical notes honoured: no loading between shifts (flash masks a hard camera cut + controller swap; per-character VCams are
// pre-placed and swapped by priority), and a death in D–F / G–I restarts only that character's section.
using System.Collections;
using UnityEngine;

namespace Ruminahui
{
    public enum M35Stage { A_HoldBreach, C_LineFails, D_ChaskaFlank, G_SecondCollapse, H_AtocRearLine, K_Convergence, L_FinalWave, N_SharedVision, O_Victory, P_End }

    public class ForcedSwapDirector : MonoBehaviour
    {
        [Header("Wiring (set by LevelBuilder)")]
        public PlayerCharacter ruminahui, chaska, atoc;
        public Transform breachPoint, flankStart, rearStart, huacaPoint;
        public EncounterDirector encounterA, encounterD, encounterH, encounterL;
        public Transform chaskaGoal, atocGoal;
        public GameObject atocCage;
        public float goalRadius = 3f;

        [Header("Tuning")]
        public float sectionEaseScale = 0.85f;   // PLACEHOLDER-BALANCE: "slightly easier" (Section 3)
        public float finalWaveScale = 1.15f;     // PLACEHOLDER-BALANCE: "the mission's real difficulty spike"
        public float visionFlashDuration = 1.6f;

        // Reuses Willka's M1.2 fireside VO (Section 3 "Audio"); the M1.2 script isn't written yet, so this is a keyed placeholder.
        public string willkaVoKey = "VO_Willka_M1_2_Fireside_01";

        public M35Stage Stage { get; private set; }
        public bool IsMission => MissionManager.Instance != null && MissionManager.Instance.Current != null;

        void Start()
        {
            if (CheckpointService.Instance != null) CheckpointService.Instance.CustomRespawn = RestartSection;
            StartCoroutine(Run());
        }

        void OnDestroy()
        {
            if (CheckpointService.Instance != null && CheckpointService.Instance.CustomRespawn == (System.Func<IEnumerator>)RestartSection)
                CheckpointService.Instance.CustomRespawn = null;
        }

        IEnumerator Run()
        {
            yield return null; // let characters register
            yield return RunFrom(M35Stage.A_HoldBreach);
        }

        /// <summary>The whole mission as one flow. Section restarts re-enter it at D or H.</summary>
        IEnumerator RunFrom(M35Stage start)
        {
            var pm = PartyManager.Instance;

            if (start <= M35Stage.A_HoldBreach)
            {
                // ── A: Rumiñahui alone at the breach. Chaska pinned at the flank, Atoc caged at the rear.
                Stage = M35Stage.A_HoldBreach;
                SetAlly(chaska, AllyMode.Idle, true, true);
                SetAlly(atoc, AllyMode.Idle, true, true);
                pm.SetControlled(ruminahui, true);
                ObjectiveTracker.Set("M3.5 [A] Hold the breach — as Rumiñahui");
                encounterA.damageScale = 1f;
                encounterA.Begin();
                while (!encounterA.IsComplete) yield return null;

                // ── B/C: the line begins to fail. Scripted stagger + vision flash (masks the control shift).
                Stage = M35Stage.C_LineFails;
                ObjectiveTracker.Set("M3.5 [C] The line fails…");
                SetAlly(ruminahui, AllyMode.Idle, true, true);
                if (ruminahui.Visual != null) ruminahui.Visual.SetPoseScale(new Vector3(1f, 0.6f, 1f)); // drops to one knee
                yield return new WaitForSeconds(0.8f);
                ObjectiveTracker.Say("Willka (VO, audio only)", $"[{willkaVoKey}]");
                // ── CONTROL SHIFT 1 (hidden under the flash; no load).
                yield return VisionFlash(() => TakeControl(chaska));
            }

            if (start <= M35Stage.D_ChaskaFlank)
            {
                if (start == M35Stage.D_ChaskaFlank) TakeControl(chaska);
                // ── D–F: Chaska fights across the flank toward the breach.
                yield return ChaskaSection();

                // ── G: she reaches him; a second collapse exposes Atoc's holding point.
                Stage = M35Stage.G_SecondCollapse;
                ObjectiveTracker.Set("M3.5 [G] A second collapse — Atoc's cage is exposed");
                SetAlly(chaska, AllyMode.Idle, true, true);
                yield return new WaitForSeconds(0.6f);
                // ── CONTROL SHIFT 2: chained hands break free in the chaos.
                yield return VisionFlash(() =>
                {
                    if (atocCage != null) atocCage.SetActive(false);
                    TakeControl(atoc);
                    ObjectiveTracker.Say("Atoc", "(the chains give — he could run. He doesn't.)");
                });
            }

            if (start <= M35Stage.H_AtocRearLine)
            {
                if (start == M35Stage.H_AtocRearLine) TakeControl(atoc);
                // ── H–I: Atoc fights through the exposed rear line to the huaca.
                yield return AtocSection();
            }

            // ── J–K: convergence — the one non-combat beat, deliberately before the hardest wave.
            Stage = M35Stage.K_Convergence;
            yield return Convergence();

            // ── L: final wave — Rumiñahui leads, Chaska + Atoc as full AI allies.
            Stage = M35Stage.L_FinalWave;
            CheckpointService.Instance?.SetCheckpoint(huacaPoint.position + huacaPoint.forward * 2f, huacaPoint.rotation);
            SetAlly(chaska, AllyMode.Combat, false, false);
            SetAlly(atoc, AllyMode.Combat, false, false);
            pm.SetControlled(ruminahui, true);
            ObjectiveTracker.Set("M3.5 [L] Final wave — all three kits, together");
            encounterL.damageScale = finalWaveScale;
            encounterL.Begin();
            while (!encounterL.IsComplete) yield return null;

            // ── N: shared vision; the ultimate triggers.
            Stage = M35Stage.N_SharedVision;
            yield return SharedVision();

            // ── O: victory beat. Silence. Exhaustion.
            Stage = M35Stage.O_Victory;
            ObjectiveTracker.Set("M3.5 [O] The position holds.");
            if (GameInput.Instance != null) GameInput.Instance.PushCutscene();
            yield return new WaitForSeconds(3f);
            if (GameInput.Instance != null) GameInput.Instance.PopCutscene();

            // ── P: transitions straight into M3.6.
            Stage = M35Stage.P_End;
            if (IsMission) MissionManager.Instance.CompleteCurrent();
            else ObjectiveTracker.Set("M3.5 test complete (in the real mission this loads M3.6).");
        }

        void TakeControl(PlayerCharacter c)
        {
            SetAlly(c, AllyMode.Combat, false, false);
            PartyManager.Instance.SetControlled(c, true);
        }

        IEnumerator ChaskaSection()
        {
            Stage = M35Stage.D_ChaskaFlank;
            CheckpointService.Instance?.SetCheckpoint(flankStart.position, flankStart.rotation);
            ObjectiveTracker.Set("M3.5 [D–F] As Chaska: fight across the flank to Rumiñahui");
            encounterD.damageScale = sectionEaseScale;
            encounterD.ResetEncounter();
            encounterD.Begin();
            while (Vector3.Distance(chaska.transform.position, chaskaGoal.position) > goalRadius || encounterD.AliveCount > 0) yield return null;
            encounterD.Abort();
        }

        IEnumerator AtocSection()
        {
            Stage = M35Stage.H_AtocRearLine;
            CheckpointService.Instance?.SetCheckpoint(rearStart.position, rearStart.rotation);
            ObjectiveTracker.Set("M3.5 [H–I] As Atoc: fight through the rear line to the huaca");
            encounterH.damageScale = sectionEaseScale;
            encounterH.ResetEncounter();
            encounterH.Begin();
            while (Vector3.Distance(atoc.transform.position, atocGoal.position) > goalRadius || encounterH.AliveCount > 0) yield return null;
            encounterH.Abort();
        }

        IEnumerator Convergence()
        {
            var input = GameInput.Instance;
            if (input != null) input.PushCutscene();
            var fader = ScreenFader.Instance;
            if (fader != null) yield return fader.FadeOut(0.3f, Color.black);

            // Gather all three at the huaca.
            var c = huacaPoint.position;
            ruminahui.RespawnAt(c + huacaPoint.right * -1.5f, huacaPoint.rotation);
            chaska.RespawnAt(c + huacaPoint.right * 1.5f, huacaPoint.rotation);
            atoc.RespawnAt(c + huacaPoint.forward * -1.5f, huacaPoint.rotation);
            ruminahui.ScriptLocked = chaska.ScriptLocked = atoc.ScriptLocked = true;
            if (UIRoot.Instance != null) UIRoot.Instance.SetCombatUIVisible(false);

            // Dedicated cinematic pass (11 Part A Step 6.2): two placeholder shots in place of the Timeline asset.
            var cd = CameraDirector.Instance;
            var shot1 = cd.CreateShot("VCam_Cutscene_Convergence_01", c + huacaPoint.forward * 6f + Vector3.up * 1.6f, c + Vector3.up * 1.2f, 0.15f);
            var shot2 = cd.CreateShot("VCam_Cutscene_Convergence_02", c + huacaPoint.right * 4f + Vector3.up * 1.2f, c + Vector3.up * 1.3f, 0.1f);
            if (fader != null) StartCoroutine(fader.FadeIn(0.3f));
            ObjectiveTracker.Set("M3.5 [K] Convergence — all three, together for the first time");
            ObjectiveTracker.Say("Chaska", "[M3.5 convergence line 1 — placeholder]");
            yield return cd.PlayShot(shot1, 2.5f);
            ObjectiveTracker.Say("Atoc", "[M3.5 convergence line 2 — placeholder]");
            yield return cd.PlayShot(shot2, 2.5f);
            ObjectiveTracker.Say("Rumiñahui", "[M3.5 convergence line 3 — placeholder]");
            Destroy(shot1.gameObject);
            Destroy(shot2.gameObject);

            ruminahui.ScriptLocked = chaska.ScriptLocked = atoc.ScriptLocked = false;
            if (UIRoot.Instance != null) UIRoot.Instance.SetCombatUIVisible(true);
            if (input != null) input.PopCutscene();
        }

        IEnumerator SharedVision()
        {
            ObjectiveTracker.Set("M3.5 [N] Shared vision — puma, condor, serpent");
            // The three ultimates unlock live here ("unlocking each kit live as the story demands it").
            Progression.Unlock(Unlocks.WhatHeldTheLine);
            Progression.Unlock(Unlocks.GapInTheLine);
            Progression.Unlock(Unlocks.HundredAndOne);
            var fader = ScreenFader.Instance;
            if (fader != null)
            {
                yield return fader.VisionFlash(0.6f, CharacterFactory.RuminahuiColor);
                yield return fader.VisionFlash(0.6f, CharacterFactory.ChaskaColor);
                yield return fader.VisionFlash(0.6f, CharacterFactory.AtocColor);
            }
            var puma = ruminahui.Kit as PumaKit;
            if (puma != null) puma.TriggerUltimateFree(); // "'What Held the Line' triggers"
            yield return new WaitForSeconds(1f);
        }

        /// <summary>Flash that hides a hard cut + controller swap (Section 3: "hard engineering requirement").</summary>
        IEnumerator VisionFlash(System.Action atPeak)
        {
            var input = GameInput.Instance;
            if (input != null) input.PushCutscene();
            AudioPool.Instance?.PlayCue(PlaceholderCue.Vision, Vector3.zero, 0.5f);
            var fader = ScreenFader.Instance;
            if (fader != null) yield return fader.FadeOut(visionFlashDuration * 0.4f, new Color(1f, 0.92f, 0.7f));
            atPeak?.Invoke();
            yield return null;
            if (fader != null) yield return fader.FadeIn(visionFlashDuration * 0.6f);
            if (input != null) input.PopCutscene();
        }

        /// <param name="hidden">Out-of-play characters (downed, caged, waiting) aren't targeted by enemies.</param>
        void SetAlly(PlayerCharacter c, AllyMode mode, bool locked, bool hidden)
        {
            c.allyMode = mode;
            c.ScriptLocked = locked;
            c.Target.stealthed = hidden;
            if (!hidden && c.Visual != null) c.Visual.ResetPose();
        }

        /// <summary>Fail state: a death in D–F or G–I restarts that character's section only (Section 3).</summary>
        IEnumerator RestartSection()
        {
            PoolManager.Instance.DespawnAll();
            switch (Stage)
            {
                case M35Stage.D_ChaskaFlank:
                    chaska.RespawnAt(flankStart.position, flankStart.rotation);
                    StopAllCoroutines();
                    StartCoroutine(RunFrom(M35Stage.D_ChaskaFlank));
                    break;
                case M35Stage.H_AtocRearLine:
                    atoc.RespawnAt(rearStart.position, rearStart.rotation);
                    StopAllCoroutines();
                    StartCoroutine(RunFrom(M35Stage.H_AtocRearLine));
                    break;
                case M35Stage.L_FinalWave:
                    ruminahui.RespawnAt(huacaPoint.position + huacaPoint.forward * 2f, huacaPoint.rotation);
                    chaska.RespawnAt(huacaPoint.position + huacaPoint.right * 2f, huacaPoint.rotation);
                    atoc.RespawnAt(huacaPoint.position - huacaPoint.right * 2f, huacaPoint.rotation);
                    encounterL.ResetEncounter();
                    encounterL.Begin(); // RunFrom is still waiting on encounterL
                    break;
                default:
                    ruminahui.RespawnAt(breachPoint.position, breachPoint.rotation);
                    encounterA.ResetEncounter();
                    encounterA.Begin();
                    break;
            }
            yield return null;
        }
    }
}
