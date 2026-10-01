// Implements: 02 M5.2 "The Hardest Order" — "escort/evacuation objectives under time pressure, culminating in a scripted,
// deliberately uncomfortable 'light the fire' beat — the game should not make this feel good to do."
// Evacuees reach the north gate before Benalcázar's column arrives; then Rumiñahui alone lights the granary. No reward beat.
using System.Collections;
using UnityEngine;

namespace Ruminahui
{
    public class HardestOrderDirector : EscortMissionBase
    {
        public float timeLimit = 150f;           // PLACEHOLDER-BALANCE
        public ScriptedInteractable lightTheFire;
        public GameObject fires;                 // placeholder fire props, revealed after the beat

        float remaining;

        protected override void Start()
        {
            base.Start();
            remaining = timeLimit;
            lightTheFire.gameObject.SetActive(false);
            if (fires != null) fires.SetActive(false);
            StartCoroutine(Run());
        }

        protected override void OnRetry() => remaining = timeLimit;

        IEnumerator Run()
        {
            yield return null;
            ObjectiveTracker.Set("M5.2 — Get the families to the north gate before Benalcázar's column arrives.");
            EscortHud.Title = "Evacuation";
            ambushes[0].Begin();
            bool secondWave = false;

            while (!AllArrived())
            {
                if (!failing)
                {
                    remaining -= Time.deltaTime;
                    EscortHud.TimeRemaining = Mathf.Max(0f, remaining);
                    if (!secondWave && remaining < timeLimit * 0.5f && ambushes.Count > 1) { secondWave = true; ambushes[1].Begin(); }
                    if (remaining <= 0f) Fail("The column reached the plaza.");
                }
                yield return null;
            }
            EscortHud.TimeRemaining = -1f;
            foreach (var a in ambushes) a.Abort();

            // The hardest order. Plain prompt, no fanfare.
            ObjectiveTracker.Set("M5.2 — The city can't be left to them. Go to the granary.");
            lightTheFire.gameObject.SetActive(true);
            while (!lightTheFire.Used) yield return null;

            var input = GameInput.Instance;
            input?.PushCutscene();
            if (UIRoot.Instance != null) UIRoot.Instance.SetCombatUIVisible(false);
            EscortHud.Visible = false;
            if (fires != null) fires.SetActive(true);
            AudioPool.Instance?.PlayCue(PlaceholderCue.Wind, leader.transform.position, 0.5f);
            ObjectiveTracker.Say("Rumiñahui", "[placeholder — M5.2 not in 04: no line; he watches it catch]");
            // Slow, quiet, uncomfortable: a long hold, then a slow fade. No music sting, no reward text.
            yield return new WaitForSeconds(5f);
            if (ScreenFader.Instance != null) yield return ScreenFader.Instance.FadeOut(3f, new Color(0.15f, 0.05f, 0.02f));
            yield return new WaitForSeconds(1.5f);
            if (UIRoot.Instance != null) UIRoot.Instance.SetCombatUIVisible(true);
            input?.PopCutscene();
            if (MissionManager.Instance != null && MissionManager.Instance.Current != null) MissionManager.Instance.CompleteCurrent();
            else if (ScreenFader.Instance != null) yield return ScreenFader.Instance.FadeIn(0.5f);
        }
    }
}
