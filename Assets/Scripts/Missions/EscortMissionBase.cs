// Shared escort-mission plumbing (02 M2.2 convoy, M5.2 evacuation): fail = an escort lost, the player down, or (M5.2) the timer.
// A fail is a quick retry from the last escort checkpoint — escorts reset, enemies cleared, the current ambush restarts.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ruminahui
{
    public abstract class EscortMissionBase : MonoBehaviour
    {
        public PlayerCharacter leader;
        public List<EscortTarget> escorts = new List<EscortTarget>();
        public List<EncounterDirector> ambushes = new List<EncounterDirector>();

        protected int checkpointWaypoint;
        protected bool failing;

        protected virtual void Start()
        {
            foreach (var e in escorts) e.LostEvent += _ => Fail($"{e.displayName} was lost.");
            foreach (var a in ambushes) a.listenForRespawn = false;
            if (CheckpointService.Instance != null) CheckpointService.Instance.CustomRespawn = () => FailRoutine("You fell.");
            EscortHud.Visible = true;
        }

        protected virtual void OnDestroy()
        {
            EscortHud.Clear();
        }

        protected bool AllArrived()
        {
            foreach (var e in escorts) if (e != null && !e.Arrived) return false;
            return escorts.Count > 0;
        }

        protected void Fail(string reason)
        {
            if (!failing) StartCoroutine(FailRoutine(reason));
        }

        protected IEnumerator FailRoutine(string reason)
        {
            failing = true;
            var input = GameInput.Instance;
            input?.PushCutscene();
            ObjectiveTracker.Say("Mission", reason + " Retrying from the last checkpoint.");
            if (ScreenFader.Instance != null) yield return ScreenFader.Instance.FadeOut(0.5f, Color.black);

            PoolManager.Instance.DespawnAll();
            foreach (var a in ambushes) if (a.IsRunning) { a.ResetEncounter(); a.Begin(); }
            foreach (var e in escorts) e.ResetTo(checkpointWaypoint);
            var anchor = escorts.Count > 0 ? escorts[0].transform : null;
            if (leader != null)
                leader.RespawnAt(anchor != null ? anchor.position - anchor.forward * 3f : leader.transform.position, anchor != null ? anchor.rotation : leader.transform.rotation);
            OnRetry();

            if (ScreenFader.Instance != null) yield return ScreenFader.Instance.FadeIn(0.5f);
            input?.PopCutscene();
            failing = false;
        }

        protected virtual void OnRetry() { }
    }
}
