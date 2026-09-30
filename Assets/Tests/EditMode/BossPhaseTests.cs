// Tests: 08-atoc-boss-fight.md Section 2 — phase thresholds 100-65 / 65-30 / 30-0.
using NUnit.Framework;

namespace Ruminahui.Tests
{
    public class BossPhaseTests
    {
        [TestCase(1.00f, AtocPhase.ReadingHim)]
        [TestCase(0.66f, AtocPhase.ReadingHim)]
        [TestCase(0.65f, AtocPhase.FoxsGround)]
        [TestCase(0.31f, AtocPhase.FoxsGround)]
        [TestCase(0.30f, AtocPhase.Cornered)]
        [TestCase(0.01f, AtocPhase.Cornered)]
        public void PhaseForHealth(float normalized, AtocPhase expected)
        {
            Assert.AreEqual(expected, AtocBoss.PhaseForHealth(normalized));
        }

        [Test]
        public void SpareThreshold_IsInsideCornered()
        {
            Assert.Less(AtocBoss.SpareThreshold, AtocBoss.Phase3Threshold);
        }
    }
}
