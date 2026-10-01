// Implements: 02 M0.1 "Night Raid" + 04-dialogue-script.md M0.1, beat by beat:
//   1.01–1.04 fireside (Anta / Pillahuaso) → 1.05 free roam (2–3 NPCs; Anta's "go find more wood") → 1.06–1.07 dog, torches, horn,
//   Elder → 2.01 forced stealth, cover to cover, no attack option → 2.02–2.05 Anta shoves him behind the low wall, fights the path
//   → 2.06 QTE "WARN HIM" (generous window; never winnable — success only changes the timing) → 2.07–2.10 Anta falls, the boy
//   freezes, the raiders pass → 2.11 hard cut, mission ends.
// Spotted during stealth = a quiet retry from the last cover reached (no damage) — ASSUMPTION, 04 has no fail state written.
using System.Collections;
using UnityEngine;

namespace Ruminahui
{
    public class NightRaidDirector : MonoBehaviour
    {
        [Header("Wiring (set by LevelBuilder)")]
        public PlayerCharacter boy;
        public Transform anta, raiderA, raiderB, raiderC;
        public ScriptedInteractable woodPile;
        public TalkNpc[] villagers;
        public GameObject torches;
        public ZoneTrigger wallZone;
        public ZoneTrigger[] coverCheckpoints;
        public Transform wallHidePoint, antaFightPoint, raidersExitPoint;

        public float qteWindow = 2.5f;           // "deliberately generous timing window" — PLACEHOLDER-BALANCE

        DialogueScript script;
        Vector3 retryPos;
        Quaternion retryRot;
        bool caught;

        void Start()
        {
            script = DialogueDatabase.Get("M0.1");
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
            boy.SetChildForm();
            PartyManager.Instance.SetControlled(boy, true);
            AudioPool.Instance?.PlayCue(PlaceholderCue.Wind, boy.transform.position, 0.35f); // the opening wind (reused at M5.6's end)

            // ── Fireside, then free roam.
            ObjectiveTracker.Set("M0.1 — The village at evening.");
            yield return Play("M0.1.1.01", "M0.1.1.04");
            ObjectiveTracker.Set("M0.1 — Find more wood (the pile by the terraces). Talk to the village if you like.");
            yield return Play("M0.1.1.05", "M0.1.1.05");
            while (!woodPile.Used) yield return null;

            // ── The raid arrives.
            AudioPool.Instance?.PlayCue(PlaceholderCue.DogBark, boy.transform.position, 0.6f);
            yield return Play("M0.1.1.06", "M0.1.1.06");
            if (torches != null) torches.SetActive(true);
            AudioPool.Instance?.PlayCue(PlaceholderCue.Horn, boy.transform.position + Vector3.forward * 20f, 0.8f);
            yield return Play("M0.1.1.07", "M0.1.1.07");

            // ── Forced stealth to the low wall.
            yield return Play("M0.1.2.01", "M0.1.2.01");
            ObjectiveTracker.Set("M0.1 — Get to the low stone wall. Stay in the dark cover patches; don't let the raiders see you.");
            retryPos = boy.transform.position;
            retryRot = boy.transform.rotation;
            foreach (var cp in coverCheckpoints)
            {
                var point = cp;
                point.Entered += () => { retryPos = point.transform.position; retryRot = boy.transform.rotation; };
            }
            var stealth = StealthSystem.Instance;
            stealth.SpottedEvent += OnSpotted;
            stealth.Begin();
            wallZone.armed = true;
            while (!wallZone.fired)
            {
                if (caught) yield return Caught();
                yield return null;
            }
            stealth.SpottedEvent -= OnSpotted;
            stealth.End();

            // ── Anta: "Stay down. Stay here."
            var input = GameInput.Instance;
            input?.PushCutscene();
            boy.ScriptLocked = true;
            boy.Motor.Teleport(wallHidePoint.position, wallHidePoint.rotation);
            if (boy.Visual != null) boy.Visual.SetPoseScale(new Vector3(0.72f, 0.45f, 0.72f)); // crouched behind the wall
            yield return MoveTo(anta, wallHidePoint.position + wallHidePoint.forward * 1.5f, 0.6f);
            yield return Play("M0.1.2.02", "M0.1.2.03");
            yield return MoveTo(anta, antaFightPoint.position, 0.8f);

            // He holds the path alone against two.
            yield return Play("M0.1.2.04", "M0.1.2.04");
            StartCoroutine(Skirmish());
            yield return Play("M0.1.2.05", "M0.1.2.05");

            // ── QTE: WARN HIM. Outcome can't be changed; success only delays it slightly.
            StartCoroutine(MoveTo(raiderC, antaFightPoint.position - antaFightPoint.right * 1.2f, 2.2f));
            bool warned = false;
            yield return PromptSystem.Instance.Timed("WARN HIM", qteWindow, ok => warned = ok);
            if (warned) ObjectiveTracker.Say("Pillahuaso", "(shouts — Anta turns, half a beat too late)");
            yield return new WaitForSeconds(warned ? 1.4f : 0.5f);

            // ── Anta falls. The boy can't move. The raiders pass.
            yield return Play("M0.1.2.07", "M0.1.2.08");
            anta.rotation = Quaternion.Euler(0f, anta.eulerAngles.y, 0f);
            var antaVisual = anta.GetComponent<PlaceholderVisual>();
            if (antaVisual != null) antaVisual.SetPoseScale(new Vector3(1.3f, 0.25f, 1.3f));
            yield return Play("M0.1.2.09", "M0.1.2.09");
            StartCoroutine(MoveTo(raiderA, raidersExitPoint.position, 2.5f));
            StartCoroutine(MoveTo(raiderB, raidersExitPoint.position + Vector3.right, 2.7f));
            StartCoroutine(MoveTo(raiderC, raidersExitPoint.position - Vector3.right, 2.9f));
            yield return Play("M0.1.2.10", "M0.1.2.10");

            // ── Hard cut. No further interaction.
            yield return Play("M0.1.2.11", "M0.1.2.11");
            if (ScreenFader.Instance != null) yield return ScreenFader.Instance.FadeOut(0.05f, Color.black);
            yield return new WaitForSeconds(2f);
            input?.PopCutscene();
            if (MissionManager.Instance != null && MissionManager.Instance.Current != null) MissionManager.Instance.CompleteCurrent();
            else if (ScreenFader.Instance != null) yield return ScreenFader.Instance.FadeIn(0.4f);
        }

        void OnSpotted() => caught = true;

        IEnumerator Caught()
        {
            caught = false;
            var input = GameInput.Instance;
            input?.PushCutscene();
            ObjectiveTracker.Say("Pillahuaso", "(a raider turns — he scrambles back into the dark)");
            if (ScreenFader.Instance != null) yield return ScreenFader.Instance.FadeOut(0.3f, Color.black);
            boy.Motor.Teleport(retryPos, retryRot);
            StealthSystem.Instance.ResetMeter();
            if (ScreenFader.Instance != null) yield return ScreenFader.Instance.FadeIn(0.3f);
            input?.PopCutscene();
        }

        /// <summary>Placeholder fight: Anta and two raiders trade flashes on the path.</summary>
        IEnumerator Skirmish()
        {
            var av = anta.GetComponent<PlaceholderVisual>();
            var ra = raiderA.GetComponent<PlaceholderVisual>();
            var rb = raiderB.GetComponent<PlaceholderVisual>();
            ScriptedMove.Place(raiderA, antaFightPoint.position + antaFightPoint.forward * 1.6f);
            ScriptedMove.Place(raiderB, antaFightPoint.position + antaFightPoint.forward * 1.4f + antaFightPoint.right * 1.4f);
            for (int i = 0; i < 6; i++)
            {
                if (av != null) av.Flash(Color.white, 0.15f, false);
                AudioPool.Instance?.PlayCue(PlaceholderCue.Block, anta.position, 0.35f);
                yield return new WaitForSeconds(0.45f);
                var r = i % 2 == 0 ? ra : rb;
                if (r != null) r.Flash(new Color(1f, 0.5f, 0.3f), 0.15f, false);
                yield return new WaitForSeconds(0.35f);
            }
        }

        static IEnumerator MoveTo(Transform t, Vector3 target, float seconds) => ScriptedMove.To(t, target, seconds);
    }
}
