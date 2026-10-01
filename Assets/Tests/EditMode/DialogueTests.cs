// Tests: 04-dialogue-script.md → Resources/Dialogue JSON (via Tools/DialogueImport) → DialogueDatabase.
// Anchors are specific beats from the doc, so a doc edit that breaks the importer shows up here.
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Ruminahui.Tests
{
    public class DialogueTests
    {
        static readonly string[] ScriptedMissions = { "M0.1", "M0.2", "M5.5", "M5.6", "M5.7" };

        [Test]
        public void EveryScriptedMissionLoads_AndMatchesItsId()
        {
            foreach (var id in ScriptedMissions)
            {
                var s = DialogueDatabase.Get(id);
                Assert.IsTrue(s != null, "missing dialogue for " + id);
                Assert.AreEqual(id, s.missionId);
                Assert.IsTrue(s.AllBeats().Any(b => b.Kind == DialogueBeatKind.Line), id + " has no spoken lines");
                Assert.IsTrue(MissionDatabase.IsMission(id), id + " not in the mission list");
            }
        }

        [Test]
        public void UnwrittenMissionsHaveNoScript()
        {
            Assert.IsFalse(DialogueDatabase.Has("M3.4"));
            Assert.IsNull(DialogueDatabase.Get("M9.9"));
            Assert.IsNull(DialogueDatabase.Get(null));
        }

        [Test]
        public void BeatIdsAreUnique_AndKindsAreKnown()
        {
            var ids = new HashSet<string>();
            foreach (var id in ScriptedMissions)
                foreach (var b in DialogueDatabase.Get(id).AllBeats())
                {
                    Assert.IsTrue(ids.Add(b.id), "duplicate beat id " + b.id);
                    Assert.IsTrue(b.Kind != DialogueBeatKind.Unknown, "unknown kind " + b.kind + " at " + b.id);
                    Assert.IsTrue(!string.IsNullOrEmpty(b.text), "empty text at " + b.id);
                    if (b.Kind == DialogueBeatKind.Line) Assert.IsTrue(!string.IsNullOrEmpty(b.speaker), "line without speaker at " + b.id);
                }
        }

        [Test]
        public void SpeakersAreTheCastOf04()
        {
            var cast = new HashSet<string> { "Anta", "Pillahuaso", "Elder", "Willka", "Camp Boy", "Camp Boy 2", "Rumiñahui", "Interpreter", "Atoc", "Chaska" };
            foreach (var id in ScriptedMissions)
                foreach (var b in DialogueDatabase.Get(id).AllBeats().Where(b => b.Kind == DialogueBeatKind.Line))
                    Assert.IsTrue(cast.Contains(b.speaker), $"unexpected speaker '{b.speaker}' at {b.id}");
        }

        [Test]
        public void Prompts_WarnHim_StandRun_StaySilent()
        {
            Assert.IsTrue(HasPrompt("M0.1", "WARN HIM"));
            Assert.IsTrue(HasPrompt("M5.5", "STAND"));
            Assert.IsTrue(HasPrompt("M5.5", "RUN"));
            Assert.IsTrue(HasPrompt("M5.6", "STAY SILENT"));
        }

        [Test]
        public void M02_TheNameIsSpokenByPillahuaso()
        {
            var line = DialogueDatabase.Get("M0.2").AllBeats().FirstOrDefault(b => b.Kind == DialogueBeatKind.Line && b.text == "Rumiñahui.");
            Assert.IsTrue(line != null, "the naming line is missing");
            Assert.AreEqual("Pillahuaso", line.speaker);
        }

        [Test]
        public void M56_RumiñahuiSpeaksTwice_StartingWithTheMountainsLine()
        {
            var lines = DialogueDatabase.Get("M5.6").AllBeats().Where(b => b.Kind == DialogueBeatKind.Line && b.speaker == "Rumiñahui").ToList();
            Assert.AreEqual(2, lines.Count);
            Assert.IsTrue(lines[0].text.StartsWith("Tell him my mother named me for the mountains."));
        }

        [Test]
        public void M57_EndsOnTheTwoLineTextCard()
        {
            var s = DialogueDatabase.Get("M5.7");
            var last = s.scenes[s.scenes.Length - 1];
            Assert.IsTrue(last.heading.StartsWith("TEXT CARD"));
            Assert.AreEqual(2, last.beats.Length);
            Assert.IsTrue(last.beats.All(b => b.Kind == DialogueBeatKind.TextCard));
        }

        [Test]
        public void LineDuration_IsClampedForReadability()
        {
            Assert.AreEqual(DialogueRunner.MinLineSeconds, DialogueRunner.LineDuration("No—"), 0.001);
            Assert.AreEqual(DialogueRunner.MaxLineSeconds, DialogueRunner.LineDuration(new string('x', 500)), 0.001);
            float mid = DialogueRunner.LineDuration(new string('x', 60));
            Assert.IsTrue(mid > DialogueRunner.MinLineSeconds && mid < DialogueRunner.MaxLineSeconds);
        }

        static bool HasPrompt(string mission, string prompt) =>
            DialogueDatabase.Get(mission).AllBeats().Any(b => b.HasPrompts && b.prompts.Contains(prompt));
    }
}
