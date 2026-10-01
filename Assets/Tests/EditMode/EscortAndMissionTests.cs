// Tests: escort countdown formatting (U3), M5.2's quiet completion (02: "should not make this feel good"), built-mission flags.
using NUnit.Framework;

namespace Ruminahui.Tests
{
    public class EscortAndMissionTests
    {
        [Test]
        public void Countdown_FormatsMinutesSeconds()
        {
            Assert.AreEqual("2:30", EscortHud.FormatTime(150f));
            Assert.AreEqual("0:05", EscortHud.FormatTime(4.2f));
            Assert.AreEqual("", EscortHud.FormatTime(-1f));
        }

        [Test]
        public void HardestOrder_CompletesQuietly_ButStillAwardsAPoint()
        {
            var m = MissionDatabase.Get("M5.2");
            Assert.IsTrue(m.QuietCompletion);
            Assert.IsTrue(m.AwardsUpgradePoint);
            Assert.IsFalse(MissionDatabase.Get("M5.1").QuietCompletion);
        }

        [Test]
        public void BatchBAndC_MissionsAreMarkedBuilt()
        {
            foreach (var id in new[] { "M0.1", "M2.2", "M5.5", "M5.6" })
                Assert.IsTrue(MissionDatabase.Get(id).HasBuiltContent, id);
        }

        [Test]
        public void EveryMissionHasBuiltContent_NoPlaceholdersLeft()
        {
            foreach (var m in MissionDatabase.Missions)
                Assert.IsTrue(m.HasBuiltContent, m.Id + " is still a placeholder");
        }

        [Test]
        public void ThreeWorldsMenu_UnlocksAtM36()
        {
            Progression.ApplyForMission("M3.5");
            Assert.IsFalse(Progression.IsUnlocked(Unlocks.ThreeWorldsMenu));
            Progression.ApplyForMission("M3.6");
            Assert.IsTrue(Progression.IsUnlocked(Unlocks.ThreeWorldsMenu));
        }
    }
}
