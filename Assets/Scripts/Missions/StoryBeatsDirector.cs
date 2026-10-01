// Implements: 02's walk-and-talk / cinematic-forward missions (M1.2, M1.4, M2.4, M3.6, M4.1, M4.3, M5.7) as a data-driven list of
// beats: set an objective → wait for a zone / a talk / an action → play lines (04 ranges where written, marked placeholders where
// not) → next. Completes the mission at the end.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ruminahui
{
    public class StoryBeat
    {
        public string objective;
        public ZoneTrigger zone;                        // wait until entered (armed when the beat starts)
        public TalkNpc talkTo;                          // wait until talked to (and their lines finish)
        public List<(string speaker, string text)> lines = new List<(string, string)>();
        public string docMission, docFrom, docTo;       // play a 04 range instead of / before placeholder lines
        public System.Func<IEnumerator> action;         // custom gameplay (encounter, puzzle, vision…)
        public float holdAfter = 0.5f;
    }

    public class StoryBeatsDirector : MonoBehaviour
    {
        public string missionLabel = "";
        public readonly List<StoryBeat> beats = new List<StoryBeat>();
        public bool completeMissionAtEnd = true;
        public System.Func<IEnumerator> onFinished;

        void Start()
        {
            if (DialogueRunner.Shared != null) DialogueRunner.Shared.AnnouncePrompts = false;
            foreach (var b in beats) if (b.zone != null) b.zone.armed = false;
            StartCoroutine(Run());
        }

        IEnumerator Run()
        {
            yield return null;
            var runner = DialogueRunner.Shared;
            foreach (var b in beats)
            {
                if (!string.IsNullOrEmpty(b.objective)) ObjectiveTracker.Set($"{missionLabel} — {b.objective}");
                if (b.zone != null)
                {
                    b.zone.armed = true;
                    while (!b.zone.fired) yield return null;
                }
                if (b.talkTo != null)
                {
                    while (!b.talkTo.Talked) yield return null;
                    while (runner != null && runner.IsPlaying) yield return null;
                }
                if (!string.IsNullOrEmpty(b.docMission) && runner != null)
                {
                    var script = DialogueDatabase.Get(b.docMission);
                    if (script != null) yield return runner.PlayRange(script, b.docFrom, b.docTo);
                }
                if (b.lines.Count > 0 && runner != null)
                {
                    var list = new List<DialogueBeat>();
                    for (int i = 0; i < b.lines.Count; i++)
                        list.Add(new DialogueBeat { id = $"beat.{i}", kind = "line", speaker = b.lines[i].speaker, direction = "", text = b.lines[i].text, prompts = new string[0] });
                    yield return runner.PlayScene(new DialogueScene { heading = "", beats = list.ToArray() });
                }
                if (b.action != null) yield return b.action();
                yield return new WaitForSeconds(b.holdAfter);
            }

            if (onFinished != null) yield return onFinished();
            if (completeMissionAtEnd && MissionManager.Instance != null && MissionManager.Instance.Current != null) MissionManager.Instance.CompleteCurrent();
            else ObjectiveTracker.Set($"{missionLabel} — complete (test).");
        }
    }
}
