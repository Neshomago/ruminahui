// Implements: 06-m5-3-flow-and-enemy-roster.md, M5.3 "Into the Llanganates" flow A→I:
//   A free switching on · B Fallen Gate (Puma) · C Wide Break (Kuntur crosses, drops a line) · D Narrow Dark (Amaru trap-sense)
//   E regroup · F Joint Lift (all three simultaneously; swap auto-locks while a character is mid-action) · G Hiding Chamber
//   (no loot, deliberately anti-climactic) · H dialogue beat · I → M5.4.
// Section 3: no combat, no timers, no health loss, no fail state; idle companions follow/keep watch/comment.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ruminahui
{
    public class LlanganatesDirector : MonoBehaviour
    {
        [Header("Wiring (set by LevelBuilder)")]
        public PlayerCharacter ruminahui, chaska, atoc;
        public FallenGateObstacle fallenGate;
        public LineAnchorObstacle lineAnchor;
        public List<RiggedDeadfall> narrowDarkTraps = new List<RiggedDeadfall>();
        public ZoneTrigger regroupZone;
        public JointLiftStation braceStation, spotStation, clearStation;
        public List<RiggedDeadfall> finalApproachTraps = new List<RiggedDeadfall>();
        public Transform treasure, treasureHidingSpot;
        public ZoneTrigger chamberZone;

        public string StageLabel { get; private set; } = "";
        bool regrouped;
        readonly object holdLockToken = new object();
        readonly object liftLockToken = new object();

        void Start() => StartCoroutine(Run());

        void OnDestroy()
        {
            if (FreeSwapController.Instance != null) FreeSwapController.Instance.SetActive(false);
        }

        void Update()
        {
            // Stage F rule: "auto-lock the player to whichever character is mid-action".
            var swap = FreeSwapController.Instance;
            var c = PartyManager.Instance != null ? PartyManager.Instance.Controlled : null;
            if (swap == null || c == null) return;
            if (c.Sensor != null && c.Sensor.IsHolding) swap.Lock(holdLockToken);
            else swap.Unlock(holdLockToken);
        }

        IEnumerator Run()
        {
            yield return null;
            var pm = PartyManager.Instance;
            regroupZone.armed = false;
            chamberZone.armed = false;
            // No fail state: nothing can hurt anyone in this mission.
            foreach (var m in new[] { ruminahui, chaska, atoc }) { m.Target.invulnerable = true; m.allyMode = AllyMode.Follow; }
            pm.SetControlled(ruminahui, true);
            FreeSwapController.Instance.SetActive(true);

            Stage("[A] Free switching enabled (1/2/3 or LB/RB). Head up the path.");
            Stage("[B] The Fallen Gate — needs Rumiñahui's strength");
            yield return WaitFor(fallenGate);

            Stage("[C] The Wide Break — Chaska: Vantage Leap (R) across, then drop a line");
            yield return WaitFor(lineAnchor);

            Stage("[D] The Narrow Dark — Atoc: trap-sense reveals the deadfalls; disarm them");
            foreach (var t in narrowDarkTraps) yield return WaitFor(t);

            Stage("[E] Regroup at the inner approach (bring everyone)");
            regroupZone.armed = true;
            while (!regroupZone.fired) yield return null;
            regrouped = true;

            Stage("[F] The Joint Lift — Rumiñahui braces, Chaska spots from the ledge, Atoc clears the last traps");
            braceStation.availability = () => regrouped;
            spotStation.availability = () => regrouped;
            clearStation.availability = () => AllComplete(finalApproachTraps);
            braceStation.Completed += OnStationEngaged;
            spotStation.Completed += OnStationEngaged;
            clearStation.Completed += OnStationEngaged;
            while (!(braceStation.completed && spotStation.completed && clearStation.completed)) yield return null;

            // The lift itself: synchronized — nobody swaps mid-action.
            FreeSwapController.Instance.Lock(liftLockToken);
            if (GameInput.Instance != null) GameInput.Instance.PushCutscene();
            Stage("[F] Lifting…");
            AudioPool.Instance?.PlayCue(PlaceholderCue.Heave, treasure.position, 0.7f);
            var start = treasure.position;
            float t = 0f;
            while (t < 3f)
            {
                t += Time.deltaTime;
                treasure.position = Vector3.Lerp(start, treasureHidingSpot.position, Mathf.SmoothStep(0f, 1f, t / 3f));
                yield return null;
            }
            if (GameInput.Instance != null) GameInput.Instance.PopCutscene();

            // G: the hiding chamber — quiet, non-interactive, no reward pickup on purpose.
            foreach (var m in new[] { ruminahui, chaska, atoc }) { m.ScriptLocked = false; m.allyMode = AllyMode.Follow; if (m.AllyBrain != null) m.AllyBrain.ClearHold(); }
            FreeSwapController.Instance.Unlock(liftLockToken);
            Stage("[G] The Hiding Chamber. (No loot. Walk inside.)");
            chamberZone.armed = true;
            while (!chamberZone.fired) yield return null;

            // H: the three of them agree never to speak of it.
            Stage("[H] …");
            FreeSwapController.Instance.SetActive(false);
            if (GameInput.Instance != null) GameInput.Instance.PushCutscene();
            ObjectiveTracker.Say("Rumiñahui", "[M5.3 closing line — placeholder: we never speak of this place]");
            yield return new WaitForSeconds(2.5f);
            ObjectiveTracker.Say("Chaska", "[placeholder]");
            yield return new WaitForSeconds(2f);
            ObjectiveTracker.Say("Atoc", "[placeholder]");
            yield return new WaitForSeconds(2f);
            if (GameInput.Instance != null) GameInput.Instance.PopCutscene();

            // I: → M5.4
            if (MissionManager.Instance != null && MissionManager.Instance.Current != null) MissionManager.Instance.CompleteCurrent();
            else Stage("M5.3 test complete (in the real mission this loads M5.4).");
        }

        void OnStationEngaged(SwapPuzzleInteractable station)
        {
            // The engaged character holds their role (and can't be swapped back into mid-hold).
            var who = station.requiredCharacter;
            var c = PartyManager.Instance.Get(who);
            if (c != null) c.ScriptLocked = true;
            // Hand control to someone still needed, so the player isn't stuck on a frozen character.
            foreach (var s in new JointLiftStation[] { braceStation, spotStation, clearStation })
            {
                if (s.completed) continue;
                var next = PartyManager.Instance.Get(s.requiredCharacter);
                if (next != null && !next.ScriptLocked)
                {
                    FreeSwapController.Instance.Unlock(holdLockToken);
                    FreeSwapController.Instance.TrySwapTo(next.id);
                    break;
                }
            }
        }

        static bool AllComplete(List<RiggedDeadfall> traps)
        {
            foreach (var t in traps) if (t != null && !t.completed) return false;
            return true;
        }

        IEnumerator WaitFor(SwapPuzzleInteractable obstacle)
        {
            while (obstacle != null && !obstacle.completed) yield return null;
        }

        void Stage(string label)
        {
            StageLabel = label;
            ObjectiveTracker.Set("M5.3 " + label);
        }
    }
}
