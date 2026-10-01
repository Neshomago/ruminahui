// Implements: 02 M2.2 "The Prince Notices" — "Mostly dialogue/cinematic; one guarded-escort combat sequence (protect a convoy)".
// First meeting with Atahualpa (lines not yet written in 04 → marked placeholders), then two ambushes on the convoy road.
using System.Collections;
using UnityEngine;

namespace Ruminahui
{
    public class ConvoyDirector : EscortMissionBase
    {
        public TalkNpc atahualpa;
        public int firstAmbushWaypoint = 2;
        public int secondAmbushWaypoint = 4;

        protected override void Start()
        {
            base.Start();
            EscortHud.Visible = false; // shown once the convoy leaves
            foreach (var e in escorts) e.moving = false;
            StartCoroutine(Run());
        }

        IEnumerator Run()
        {
            yield return null;
            ObjectiveTracker.Set("M2.2 — The military review. Speak with Prince Atahualpa.");
            while (!atahualpa.Talked) yield return null;
            while (DialogueRunner.Shared != null && DialogueRunner.Shared.IsPlaying) yield return null;

            ObjectiveTracker.Set("M2.2 — Escort the convoy to the garrison. It halts when enemies are near: clear the road.");
            EscortHud.Title = "Convoy";
            EscortHud.Visible = true;
            foreach (var e in escorts) e.moving = true;
            var lead = escorts[0];
            lead.WaypointReached += (_, wp) =>
            {
                if (wp + 1 == firstAmbushWaypoint) { checkpointWaypoint = wp + 1; ambushes[0].Begin(); ObjectiveTracker.Say("Rumiñahui", "Ambush — hold the carts!"); }
                if (wp + 1 == secondAmbushWaypoint && ambushes.Count > 1) { checkpointWaypoint = wp + 1; ambushes[1].Begin(); ObjectiveTracker.Say("Rumiñahui", "Again — from the ridge!"); }
            };
            while (!AllArrived()) yield return null;

            ObjectiveTracker.Set("M2.2 — The convoy is through.");
            ObjectiveTracker.Say("Atahualpa", "[placeholder — M2.2 dialogue not in 04] Well held, general's-man.");
            yield return new WaitForSeconds(3f);
            if (MissionManager.Instance != null && MissionManager.Instance.Current != null) MissionManager.Instance.CompleteCurrent();
            else ObjectiveTracker.Set("Convoy test complete.");
        }
    }
}
