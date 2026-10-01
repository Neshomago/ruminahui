// Implements: 02 M5.6 "Stone Face" + 04 M5.6 — restrained, mostly non-interactive; torture is NEVER depicted (04 production notes):
//   Scene 1 holding quarters, the Interpreter's questions, Rumiñahui's one line → hard cut to black on that line.
//   Scene 2 return from black, wide and still; "STAY SILENT" three times, spaced by stillness, no fail state; the Interpreter's
//   private offer and Rumiñahui's answer.
//   Scene 3 execution ground: camera holds on his face; wind (the Prologue's opening cue) → cut before anything happens.
using System.Collections;
using UnityEngine;

namespace Ruminahui
{
    public class StoneFaceDirector : MonoBehaviour
    {
        [Header("Wiring (set by LevelBuilder)")]
        public PlayerCharacter ruminahui;
        public GameObject interpreter, officer;
        public Transform quartersSeat, quartersCamPos, quartersLook;
        public Transform executionSpot, executionCamPos, executionLook;
        public GameObject executionCrowd;

        public float stillnessBetweenPrompts = 6f;   // PLACEHOLDER-BALANCE: "spaced by stretches of stillness"

        DialogueScript script;

        void Start()
        {
            script = DialogueDatabase.Get("M5.6");
            if (DialogueRunner.Shared != null) DialogueRunner.Shared.AnnouncePrompts = false;
            StartCoroutine(Run());
        }

        IEnumerator Play(string from, string to)
        {
            if (script != null && DialogueRunner.Shared != null) yield return DialogueRunner.Shared.PlayRange(script, from, to);
        }

        IEnumerator Run()
        {
            yield return null;
            var input = GameInput.Instance;
            input?.PushCutscene();
            ruminahui.CombatDisabled = true;
            ruminahui.ScriptLocked = true;
            PartyManager.Instance.SetControlled(ruminahui, true);
            ruminahui.Motor.Teleport(quartersSeat.position, quartersSeat.rotation);
            if (ruminahui.Visual != null) ruminahui.Visual.SetPoseScale(new Vector3(1f, 0.7f, 1f)); // seated, bound
            if (UIRoot.Instance != null) UIRoot.Instance.SetCombatUIVisible(false);
            if (executionCrowd != null) executionCrowd.SetActive(false);

            // Wide, still shot for the whole interior (04: "the camera stays wide and still").
            var cd = CameraDirector.Instance;
            var wide = cd.CreateShot("VCam_M5_6_Quarters_Wide", quartersCamPos.position, quartersLook.position);
            cd.HardCut();
            wide.Priority = CameraDirector.ShotPriority;

            // ── Scene 1
            ObjectiveTracker.Set("M5.6 — Stone Face");
            yield return Play("M5.6.1.01", "M5.6.1.05");
            yield return Play("M5.6.1.06", "M5.6.1.06");
            if (ScreenFader.Instance != null) yield return ScreenFader.Instance.FadeOut(0.05f, Color.black); // hard cut on the line
            yield return new WaitForSeconds(2.5f);

            // ── Scene 2: alone, worse for it. Nothing is shown.
            if (interpreter != null) interpreter.SetActive(false);
            if (officer != null) officer.SetActive(false);
            if (ruminahui.Visual != null) ruminahui.Visual.SetPoseScale(new Vector3(1f, 0.62f, 1f));
            yield return Play("M5.6.2.01", "M5.6.2.02");
            if (ScreenFader.Instance != null) yield return ScreenFader.Instance.FadeIn(1.5f);

            for (int i = 0; i < 3; i++)
            {
                yield return new WaitForSeconds(stillnessBetweenPrompts);
                yield return PromptSystem.Instance.Quiet("STAY SILENT");
            }

            yield return Play("M5.6.2.03", "M5.6.2.03");
            if (interpreter != null) interpreter.SetActive(true);
            yield return Play("M5.6.2.04", "M5.6.2.06");
            if (interpreter != null) interpreter.SetActive(false);
            yield return new WaitForSeconds(2f);

            // ── Scene 3: the execution ground. No crowd for spectacle, no rescue, no final speech.
            if (ScreenFader.Instance != null) yield return ScreenFader.Instance.FadeOut(1f, Color.black);
            wide.Priority = 0;
            Destroy(wide.gameObject);
            ruminahui.Motor.Teleport(executionSpot.position, executionSpot.rotation);
            if (ruminahui.Visual != null) ruminahui.Visual.ResetPose(); // standing
            if (executionCrowd != null) executionCrowd.SetActive(true);
            if (interpreter != null) { interpreter.SetActive(true); interpreter.transform.position = executionSpot.position + new Vector3(9f, 0f, -6f); }
            var face = cd.CreateShot("VCam_M5_6_Execution_Face", executionCamPos.position, executionLook.position, 0.05f);
            cd.HardCut();
            face.Priority = CameraDirector.ShotPriority;
            if (ScreenFader.Instance != null) yield return ScreenFader.Instance.FadeIn(1.5f);
            yield return Play("M5.6.3.01", "M5.6.3.02");
            yield return new WaitForSeconds(3f); // the camera holds on his face

            // The cut comes on the wind, before anything happens on screen.
            AudioPool.Instance?.PlayCue(PlaceholderCue.Wind, executionSpot.position, 0.6f);
            yield return Play("M5.6.3.03", "M5.6.3.03");
            if (ScreenFader.Instance != null) yield return ScreenFader.Instance.FadeOut(0.05f, Color.black);
            yield return new WaitForSeconds(3.5f);

            face.Priority = 0;
            if (UIRoot.Instance != null) UIRoot.Instance.SetCombatUIVisible(true);
            input?.PopCutscene();
            if (MissionManager.Instance != null && MissionManager.Instance.Current != null) MissionManager.Instance.CompleteCurrent();
            else if (ScreenFader.Instance != null) yield return ScreenFader.Instance.FadeIn(0.4f);
        }
    }
}
