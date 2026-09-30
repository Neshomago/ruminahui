// Tests: 02-mission-list.md (26 missions, order) and 03-combat-design.md unlock timing per Act/mission.
using NUnit.Framework;

namespace Ruminahui.Tests
{
    public class ProgressionTests
    {
        [TearDown] public void TearDown() => Progression.SetUnlockAll(false);

        [Test]
        public void MissionList_Has26UniqueMissionsInOrder()
        {
            Assert.AreEqual(26, MissionDatabase.Missions.Count);
            var ids = new System.Collections.Generic.HashSet<string>();
            foreach (var m in MissionDatabase.Missions) Assert.IsTrue(ids.Add(m.Id), "duplicate " + m.Id);
            Assert.AreEqual("M0.1", MissionDatabase.Missions[0].Id);
            Assert.AreEqual("M5.7", MissionDatabase.Missions[25].Id);
            Assert.AreEqual("M3.6", MissionDatabase.Next("M3.5").Id);
            Assert.IsNull(MissionDatabase.Next("M5.7"));
        }

        [Test]
        public void Earthbreaker_ActI_Unyielding_AfterWillkasDeath()
        {
            Progression.ApplyForMission("M1.3");
            Assert.IsTrue(Progression.IsUnlocked(Unlocks.Earthbreaker));
            Assert.IsFalse(Progression.IsUnlocked(Unlocks.Unyielding));
            Progression.ApplyForMission("M2.3");
            Assert.IsFalse(Progression.IsUnlocked(Unlocks.Unyielding));
            Progression.ApplyForMission("M2.4");
            Assert.IsTrue(Progression.IsUnlocked(Unlocks.Unyielding));
        }

        [Test]
        public void StoneFace_OnlyAfterSparingAtoc_UltimatesAndTellReadingAfterM35()
        {
            Progression.ApplyForMission("M3.4");
            Assert.IsFalse(Progression.IsUnlocked(Unlocks.StoneFace));
            Progression.ApplyForMission("M3.5");
            Assert.IsTrue(Progression.IsUnlocked(Unlocks.StoneFace));
            Assert.IsFalse(Progression.IsUnlocked(Unlocks.WhatHeldTheLine));
            Assert.IsFalse(Progression.IsUnlocked(Unlocks.TellReading));
            Progression.ApplyForMission("M3.6");
            Assert.IsTrue(Progression.IsUnlocked(Unlocks.WhatHeldTheLine));
            Assert.IsTrue(Progression.IsUnlocked(Unlocks.GapInTheLine));
            Assert.IsTrue(Progression.IsUnlocked(Unlocks.HundredAndOne));
            Assert.IsTrue(Progression.IsUnlocked(Unlocks.TellReading));
            Assert.IsTrue(Progression.IsUnlocked(Unlocks.StarFall));
            Assert.IsFalse(Progression.IsUnlocked(Unlocks.NumbingDraught)); // post-M3.6
        }

        [Test]
        public void ActV_Abilities()
        {
            Progression.ApplyForMission("M5.3");
            Assert.IsTrue(Progression.IsUnlocked(Unlocks.LastFang));
            Assert.IsFalse(Progression.IsUnlocked(Unlocks.FalseTrail));
            Progression.ApplyForMission("M5.4");
            Assert.IsTrue(Progression.IsUnlocked(Unlocks.FalseTrail));
        }

        [Test]
        public void UnlockAll_OverridesEverything()
        {
            Progression.ApplyForMission("M0.1");
            Assert.IsFalse(Progression.IsUnlocked(Unlocks.HundredAndOne));
            Progression.SetUnlockAll(true);
            Assert.IsTrue(Progression.IsUnlocked(Unlocks.HundredAndOne));
        }

        [Test]
        public void AbilitySlot_FreeUseDoesNotSpendSharedFocus()
        {
            Progression.SetUnlockAll(true);
            var focus = new FocusPool(100f, 10f);
            var slot = new AbilitySlot("x", "X", 30f, 0f, null);
            Assert.IsFalse(slot.CanUse(focus, false));
            Assert.IsTrue(slot.TryConsume(focus, true)); // AI ally use
            Assert.AreEqual(10f, focus.Current, 0.001f);
        }
    }
}
