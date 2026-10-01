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
        /// <summary>Placeholder missions announce prompts as "not built yet"; built missions turn this off.</summary>
        public bool AnnouncePrompts = true;
        public DialogueScript Script { get; private set; }
        public DialogueBeat Current { get; private set; }
        Coroutine routine;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { BeatPlayed = null; shared = null; }

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

        /// <summary>Plays beats from <paramref name="firstId"/> to <paramref name="lastId"/> inclusive (mission directors interleave
        /// gameplay between script sections). Missing ids log a warning and play nothing.</summary>
        public IEnumerator PlayRange(DialogueScript script, string firstId, string lastId)
        {
            var beats = new System.Collections.Generic.List<DialogueBeat>();
            bool inRange = false;
            foreach (var b in script.AllBeats())
            {
                if (b.id == firstId) inRange = true;
                if (inRange) beats.Add(b);
                if (b.id == lastId) break;
            }
            if (beats.Count == 0 || beats[beats.Count - 1].id != lastId)
            {
                Debug.LogWarning($"[Dialogue] Range {firstId}..{lastId} not found in {script.missionId} — re-run the importer?");
                yield break;
            }
            IsPlaying = true;
            yield return PlayScene(new DialogueScene { heading = "", beats = beats.ToArray() });
            IsPlaying = false;
        }

        /// <summary>Runner that lives on the [Systems] root so any director can play script sections.</summary>
        public static DialogueRunner Shared
        {
            get
            {
                if (shared == null && GameBootstrap.SystemsRoot != null)
                {
                    shared = GameBootstrap.SystemsRoot.GetComponent<DialogueRunner>();
                    if (shared == null) shared = GameBootstrap.SystemsRoot.AddComponent<DialogueRunner>();
                }
                return shared;
            }
        }
        static DialogueRunner shared;

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
                        // Built missions run their prompts through PromptSystem themselves; only announce in placeholders.
                        if (beat.HasPrompts && AnnouncePrompts)
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
