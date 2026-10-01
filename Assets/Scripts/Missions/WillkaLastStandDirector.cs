// Implements: 02 M2.3 "Willka's Last Stand" — "A hard, humbling combat encounter that the player is meant to struggle with,
// followed by a scripted, uninteractive loss (Willka's death) — deliberately takes control away from the player at the worst
// moment." The beat fires mid-way through the final wave (or when Rumiñahui is hurt), not after it. 04 has no M2.3 lines yet.
using System.Collections;
using UnityEngine;

namespace Ruminahui
{
    public class WillkaLastStandDirector : MonoBehaviour
    {
        public PlayerCharacter ruminahui;
        public Transform willka;
        public EncounterDirector ambush;
        public float hardScale = 1.3f;               // PLACEHOLDER-BALANCE: "meant to struggle with"
        public float finalWaveTriggerSeconds = 8f;   // PLACEHOLDER-BALANCE
        public float hurtTrigger = 0.5f;             // or when HP falls below half

        bool finalWave;

        void Start()
        {
            ambush.damageScale = hardScale;
            ambush.WaveStarted += w => { if (w == ambush.waves.Count - 1) finalWave = true; };
            StartCoroutine(Run());
        }

        IEnumerator Run()
        {
            yield return null;
            ObjectiveTracker.Set("M2.3 — The mountain pass. Listen: a snapped branch comes a beat before each ambush.");
            ambush.Begin();
            while (!finalWave) yield return null;
            float t = 0f;
            while (t < finalWaveTriggerSeconds && ruminahui.Health.Normalized > hurtTrigger) { t += Time.deltaTime; yield return null; }

            // ── The worst moment: control is taken away.
            var input = GameInput.Instance;
            input?.PushCutscene();
            ruminahui.ScriptLocked = true;
            ruminahui.Target.invulnerable = true;
            if (UIRoot.Instance != null) UIRoot.Instance.SetCombatUIVisible(false);
            // The fight all but freezes around the beat. (Disabling enemy components would NOT stop their coroutines — LESSONS L-028.)
            TimeDilation.StartSlow(0.05f, 30f, ruminahui.gameObject);

            var follower = willka.GetComponent<NpcFollower>();
            if (follower != null) follower.following = false;
            var attackerPos = ruminahui.transform.position + ruminahui.transform.forward * 3f;
            var cd = CameraDirector.Instance;
            var shot = cd.CreateShot("VCam_M2_3_Willka", ruminahui.transform.position + ruminahui.transform.right * 4.5f + Vector3.up * 1.4f,
                (ruminahui.transform.position + attackerPos) * 0.5f + Vector3.up * 1.1f, 0.08f);
            cd.HardCut();
            shot.Priority = CameraDirector.ShotPriority;

            ObjectiveTracker.Say("Willka", "[placeholder — M2.3 not in 04] Boy—!");
            yield return ScriptedMove.To(willka, ruminahui.transform.position + ruminahui.transform.forward * 1.2f, 0.35f); // steps in
            var wv = willka.GetComponent<PlaceholderVisual>();
            if (wv != null) wv.Flash(Color.red, 0.3f, false);
            AudioPool.Instance?.PlayCue(PlaceholderCue.Hit, willka.position, 0.7f);
            yield return new WaitForSeconds(1.2f);
            if (wv != null) wv.SetPoseScale(new Vector3(1.3f, 0.25f, 1.3f)); // falls
            if (ruminahui.Visual != null) ruminahui.Visual.SetPoseScale(new Vector3(1f, 0.6f, 1f)); // drops beside him
            yield return new WaitForSeconds(2.5f);
            ObjectiveTracker.Say("Willka", "[placeholder — last words not yet written in 04]");
            yield return new WaitForSeconds(3f);

            if (ScreenFader.Instance != null) yield return ScreenFader.Instance.FadeOut(2f, Color.black);
            ambush.Abort();
            PoolManager.Instance.DespawnAll();
            TimeDilation.StopSlow();
            shot.Priority = 0;
            Destroy(shot.gameObject);
            yield return new WaitForSeconds(1.5f);
            if (UIRoot.Instance != null) UIRoot.Instance.SetCombatUIVisible(true);
            ruminahui.Target.invulnerable = false;
            input?.PopCutscene();
            if (MissionManager.Instance != null && MissionManager.Instance.Current != null) MissionManager.Instance.CompleteCurrent();
            else if (ScreenFader.Instance != null) yield return ScreenFader.Instance.FadeIn(0.5f);
        }
    }
}
