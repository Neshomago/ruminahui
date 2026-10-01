// Implements: 04-dialogue-script.md playback (placeholder) — plays a mission's lines in order through ObjectiveTracker.Say, which
// the existing hint panel shows. No new UI: the subtitle system is deferred per the approved UI plan; it will subscribe to
// BeatPlayed. Gameplay/QTE notes ("WARN HIM", "STAY SILENT"…) are announced as "not built yet" so testers see where they go.
using System.Collections;
using UnityEngine;

namespace Ruminahui
{
    public class DialogueRunner : MonoBehaviour
    {
        /// <summary>Raised for every beat as it plays — the hook for the future subtitle UI and VO playback.</summary>
        public static event System.Action<DialogueBeat> BeatPlayed;

        public const float SecondsPerChar = 0.055f;   // PLACEHOLDER-BALANCE: ~reading speed for subtitles
        public const float MinLineSeconds = 2f;
        public const float MaxLineSeconds = 7f;
        public float pauseBeatSeconds = 1.2f;         // "(Beat.)" / "(No answer.)" style directions
        public float sceneGapSeconds = 1f;

        public bool IsPlaying { get; private set; }
        public DialogueScript Script { get; private set; }
        public DialogueBeat Current { get; private set; }
        Coroutine routine;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => BeatPlayed = null;

        /// <summary>How long a line stays up. Static so it can be unit-tested.</summary>
        public static float LineDuration(string text)
        {
            int n = string.IsNullOrEmpty(text) ? 0 : text.Length;
            return Mathf.Clamp(0.8f + n * SecondsPerChar, MinLineSeconds, MaxLineSeconds);
        }

        /// <summary>Plays the mission's script (if 04 has one) on a runner attached to <paramref name="host"/>.</summary>
        public static DialogueRunner PlayMission(string missionId, GameObject host)
        {
            var script = DialogueDatabase.Get(missionId);
            if (script == null || host == null) return null;
            var runner = host.GetComponent<DialogueRunner>();
            if (runner == null) runner = host.AddComponent<DialogueRunner>();
            runner.Play(script);
            return runner;
        }

        public void Play(DialogueScript script)
        {
            Stop();
            Script = script;
            routine = StartCoroutine(PlayAll(script));
        }

        public void Stop()
        {
            if (routine != null) StopCoroutine(routine);
            routine = null;
            IsPlaying = false;
            Current = null;
        }

        void OnDisable() => Stop();

        IEnumerator PlayAll(DialogueScript script)
        {
            IsPlaying = true;
            Debug.Log($"[Dialogue] {script.missionId} \"{script.docTitle}\" — {script.scenes.Length} scene(s)");
            foreach (var scene in script.scenes)
            {
                yield return PlayScene(scene);
                yield return new WaitForSeconds(sceneGapSeconds);
            }
            IsPlaying = false;
            Current = null;
            routine = null;
        }

        public IEnumerator PlayScene(DialogueScene scene)
        {
            if (!string.IsNullOrEmpty(scene.heading)) Debug.Log($"[Dialogue] — {scene.heading} —");
            if (scene.beats == null) yield break;
            foreach (var beat in scene.beats)
            {
                Current = beat;
                BeatPlayed?.Invoke(beat);
                switch (beat.Kind)
                {
                    case DialogueBeatKind.Line:
                        ObjectiveTracker.Say(beat.speaker, beat.text);
                        yield return new WaitForSeconds(LineDuration(beat.text));
                        break;

                    case DialogueBeatKind.TextCard:
                        ObjectiveTracker.Say("Text card", beat.text);
                        yield return new WaitForSeconds(LineDuration(beat.text) + 1f);
                        break;

                    case DialogueBeatKind.Action:
                        Debug.Log($"[Dialogue:action] {beat.text}");
                        if (beat.text.StartsWith("(")) yield return new WaitForSeconds(pauseBeatSeconds);
                        break;

                    case DialogueBeatKind.Gameplay:
                    case DialogueBeatKind.Qte:
                        Debug.Log($"[Dialogue:{beat.kind}] {beat.text}");
                        if (beat.HasPrompts)
                        {
                            ObjectiveTracker.Say("Prompt (not built yet)", string.Join("  /  ", beat.prompts));
                            yield return new WaitForSeconds(MinLineSeconds);
                        }
                        break;
                }
            }
        }
    }
}
