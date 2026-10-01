// Implements: 02 M5.5 "Sigchos" + 04 M5.5 — the game's second powerless sequence, mirroring M0.1: movement/stealth only, no attack
// prompts, the same UI framing. Meant to fail no matter what: crossing the open ground OR being spotted brings the scouts in from
// two directions ("This time, unlike the Prologue, he sees them coming"). Then STAND / RUN — "framed honestly in the UI as
// unwinnable"; STAND gives the more dignified capture. 1.07 "He's taken."
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ruminahui
{
    public class SigchosDirector : MonoBehaviour
    {
        [Header("Wiring (set by LevelBuilder)")]
        public PlayerCharacter ruminahui;
        public ZoneTrigger openGround;
        public List<StealthWatcher> closingScouts = new List<StealthWatcher>();

        DialogueScript script;
        bool triggered;

        void Start()
        {
            script = DialogueDatabase.Get("M5.5");
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
            ruminahui.CombatDisabled = true; // identical framing to the Prologue: no attack option
            PartyManager.Instance.SetControlled(ruminahui, true);
            foreach (var s in closingScouts) s.gameObject.SetActive(false);

            yield return Play("M5.5.1.01", "M5.5.1.02");
            ObjectiveTracker.Set("M5.5 — Two days to the meeting point. Keep to cover along the trail.");
            var stealth = StealthSystem.Instance;
            stealth.SpottedEvent += Trigger;
            openGround.Entered += Trigger;
            openGround.armed = true;
            stealth.Begin();
            yield return Play("M5.5.1.03", "M5.5.1.03");
            while (!triggered) yield return null;
            stealth.SpottedEvent -= Trigger;

            // He sees them coming — from two directions.
            yield return Play("M5.5.1.04", "M5.5.1.04");
            foreach (var s in closingScouts)
            {
                s.gameObject.SetActive(true);
                s.ChaseTo(ruminahui.transform, 0.9f);
            }
            yield return new WaitForSeconds(2.5f);
            yield return Play("M5.5.1.05", "M5.5.1.05");

            // STAND or RUN — either way, capture.
            int choice = 0;
            yield return PromptSystem.Instance.Choice("STAND", "RUN", c => choice = c, "Neither choice changes what happens next. Only how.");
            var input = GameInput.Instance;
            if (choice == 1)
            {
                ObjectiveTracker.Set("M5.5 — Run.");
                foreach (var s in closingScouts) s.ChaseTo(ruminahui.transform, 1.3f);
                yield return new WaitForSeconds(3f); // the player keeps control for a few desperate seconds
                input?.PushCutscene();
                ruminahui.ScriptLocked = true;
                if (ruminahui.Visual != null) ruminahui.Visual.SetPoseScale(new Vector3(1.2f, 0.4f, 1.2f)); // brought down
            }
            else
            {
                input?.PushCutscene();
                ruminahui.ScriptLocked = true; // he stands still and lets them come — upright, unhurried
                yield return new WaitForSeconds(2f);
            }
            stealth.End();

            // He's taken. No dialogue as it happens.
            yield return Play("M5.5.1.06", "M5.5.1.07");
            if (ScreenFader.Instance != null) yield return ScreenFader.Instance.FadeOut(1.5f, Color.black);
            yield return new WaitForSeconds(1.5f);
            input?.PopCutscene();
            if (MissionManager.Instance != null && MissionManager.Instance.Current != null) MissionManager.Instance.CompleteCurrent();
            else if (ScreenFader.Instance != null) yield return ScreenFader.Instance.FadeIn(0.4f);
        }

        void Trigger() => triggered = true;
    }
}
