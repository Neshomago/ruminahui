// Tests: 04 prompts (WARN HIM timed, STAND/RUN choice, STAY SILENT quiet) and the M0.1/M5.5 stealth rules.
using NUnit.Framework;
using UnityEngine;

namespace Ruminahui.Tests
{
    public class PromptAndStealthTests
    {
        [Test]
        public void Timed_PressInsideWindow_Succeeds()
        {
            var p = new PromptState();
            p.BeginTimed("WARN HIM", 2.5f);
            Assert.IsFalse(p.Tick(1.0f, false, false));
            Assert.IsTrue(p.Tick(1.0f, true, false));
            Assert.AreEqual(PromptOutcome.Pressed, p.Outcome);
        }

        [Test]
        public void Timed_NoPress_Misses_AfterWindow()
        {
            var p = new PromptState();
            p.BeginTimed("WARN HIM", 2.5f);
            Assert.IsFalse(p.Tick(2.4f, false, false));
            Assert.IsTrue(p.Tick(0.2f, false, false));
            Assert.AreEqual(PromptOutcome.Missed, p.Outcome);
            Assert.IsFalse(p.IsActive);
        }

        [Test]
        public void Timed_ProgressRunsZeroToOne()
        {
            var p = new PromptState();
            p.BeginTimed("WARN HIM", 2f);
            p.Tick(1f, false, false);
            Assert.AreEqual(0.5, p.Progress01, 0.001);
        }

        [Test]
        public void Choice_PicksAOrB_AndWaitsWithoutTimeout()
        {
            var p = new PromptState();
            p.BeginChoice("STAND", "RUN");
            Assert.IsFalse(p.Tick(30f, false, false)); // no timeout: waits
            Assert.IsTrue(p.Tick(0.1f, false, true));
            Assert.AreEqual(PromptOutcome.ChoseB, p.Outcome);

            p.BeginChoice("STAND", "RUN");
            Assert.IsTrue(p.Tick(0.1f, true, false));
            Assert.AreEqual(PromptOutcome.ChoseA, p.Outcome);
        }

        [Test]
        public void Quiet_NeverFails_ResolvesOnPress()
        {
            var p = new PromptState();
            p.BeginQuiet("STAY SILENT");
            Assert.IsFalse(p.Tick(600f, false, false));
            Assert.IsFalse(p.Tick(1f, false, true));   // the other button does nothing
            Assert.IsTrue(p.Tick(0.1f, true, false));
            Assert.AreEqual(PromptOutcome.Pressed, p.Outcome);
        }

        [Test]
        public void ViewCone_RangeAndAngle()
        {
            var eye = new Vector3(0f, 1.6f, 0f);
            Assert.IsTrue(StealthMath.InViewCone(eye, Vector3.forward, new Vector3(0f, 1f, 10f), 14f, 55f));
            Assert.IsFalse(StealthMath.InViewCone(eye, Vector3.forward, new Vector3(0f, 1f, 20f), 14f, 55f));   // too far
            Assert.IsFalse(StealthMath.InViewCone(eye, Vector3.forward, new Vector3(0f, 1f, -5f), 14f, 55f));   // behind
            Assert.IsFalse(StealthMath.InViewCone(eye, Vector3.forward, new Vector3(9f, 1f, 3f), 14f, 55f));    // ~72° to the side
            Assert.IsTrue(StealthMath.InViewCone(eye, Vector3.forward, new Vector3(5f, 1f, 8f), 14f, 55f));     // ~32°
        }

        [Test]
        public void Detection_FillsFasterUpClose_AndDecaysWhenUnseen()
        {
            float far = StealthMath.Step(0f, true, 13f, 14f, 1f);
            float near = StealthMath.Step(0f, true, 1f, 14f, 1f);
            Assert.IsTrue(near > far, $"near {near} should exceed far {far}");
            Assert.AreEqual(1.0, StealthMath.Step(0.95f, true, 1f, 14f, 1f), 0.0001); // clamps at 1
            Assert.AreEqual(0.25, StealthMath.Step(0.5f, false, 5f, 14f, 1f), 0.0001); // decay 0.25/s
            Assert.AreEqual(0.0, StealthMath.Step(0.1f, false, 5f, 14f, 1f), 0.0001);  // floors at 0
        }
    }
}
