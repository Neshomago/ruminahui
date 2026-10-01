// Tests: approved economy (1 point per combat mission + 1 per relic, tiers in order, Kuntur/Amaru from M3.6) and the save format.
using NUnit.Framework;
using UnityEngine;

namespace Ruminahui.Tests
{
    public class EconomyAndSaveTests
    {
        [SetUp] public void SetUp() => Progression.ResetAll();
        [TearDown] public void TearDown() => Progression.ResetAll();

        [Test]
        public void Points_OnePerCombatMission_PlusRelics()
        {
            Progression.MarkCompleted("M0.1");   // stealth prologue: no point
            Progression.MarkCompleted("M1.3");   // combat: +1
            Progression.MarkCompleted("M3.4");   // combat: +1
            Progression.Collect("relic_a");      // +1
            Assert.AreEqual(3, UpgradeEconomy.EarnedPoints());
            Assert.AreEqual(3, UpgradeEconomy.AvailablePoints());
        }

        [Test]
        public void Tiers_BoughtInOrder_AndCostAPoint()
        {
            Progression.MarkCompleted("M1.3");
            Progression.MarkCompleted("M2.1");
            Assert.AreEqual(UpgradeStatus.NeedsPreviousTier, UpgradeEconomy.StatusOf(KitId.Puma, 2));
            Assert.IsTrue(UpgradeEconomy.TryBuy(KitId.Puma, 1));
            Assert.AreEqual(UpgradeStatus.Bought, UpgradeEconomy.StatusOf(KitId.Puma, 1));
            Assert.IsTrue(UpgradeEconomy.TryBuy(KitId.Puma, 2));
            Assert.AreEqual(0, UpgradeEconomy.AvailablePoints());
            Assert.AreEqual(UpgradeStatus.NotEnoughPoints, UpgradeEconomy.StatusOf(KitId.Puma, 3));
            Assert.IsFalse(UpgradeEconomy.TryBuy(KitId.Puma, 3));
            Assert.AreEqual(2, Progression.GetTier(KitId.Puma));
        }

        [Test]
        public void KunturAndAmaruTabs_OpenAtM36()
        {
            Progression.MarkCompleted("M1.3");
            Progression.ApplyForMission("M3.5");
            Assert.AreEqual(UpgradeStatus.KitLocked, UpgradeEconomy.StatusOf(KitId.Kuntur, 1));
            Assert.AreEqual(UpgradeStatus.Available, UpgradeEconomy.StatusOf(KitId.Puma, 1));
            Progression.ApplyForMission("M3.6");
            Assert.AreEqual(UpgradeStatus.Available, UpgradeEconomy.StatusOf(KitId.Amaru, 1));
        }

        [Test]
        public void ChapterSelect_CatchUp_CountsEarlierMissionsOnly()
        {
            Progression.CatchUpTo("M2.1");
            Assert.IsTrue(Progression.IsCompleted("M1.4"));
            Assert.IsFalse(Progression.IsCompleted("M2.1"));
            Assert.AreEqual(2, UpgradeEconomy.EarnedPoints()); // M1.1 + M1.3
        }

        [Test]
        public void Save_RoundTrips_ThroughJson()
        {
            Progression.MarkCompleted("M1.1");
            Progression.MarkCompleted("M1.3");
            Progression.Collect("relic_m53_1");
            Progression.SetTier(KitId.Puma, 1);
            var json = JsonUtility.ToJson(SaveData.Capture("M1.4", "2026-10-01T00:00:00Z"), true);

            Progression.ResetAll();
            var loaded = JsonUtility.FromJson<SaveData>(json);
            Assert.IsTrue(loaded.IsValid());
            Assert.AreEqual("M1.4", loaded.currentMission);
            loaded.ApplyToProgression();
            Assert.IsTrue(Progression.IsCompleted("M1.3"));
            Assert.IsTrue(Progression.HasCollected("relic_m53_1"));
            Assert.AreEqual(1, Progression.GetTier(KitId.Puma));
            Assert.AreEqual(2, UpgradeEconomy.AvailablePoints()); // 3 earned − 1 spent
        }

        [Test]
        public void Save_RejectsUnknownMission()
        {
            Assert.IsFalse(new SaveData { currentMission = "Test_AtocBoss" }.IsValid());
            Assert.IsFalse(new SaveData { currentMission = "M9.9" }.IsValid());
        }

        [Test]
        public void Settings_AreClamped()
        {
            var s = new GameSettings { masterVolume = 3f, mouseSensitivity = -1f, qualityTier = 7 }.Clamp();
            Assert.AreEqual(1.0, s.masterVolume, 0.0001);
            Assert.AreEqual(0.02, s.mouseSensitivity, 0.0001);
            Assert.AreEqual(GameSettings.QualityStandard, s.qualityTier);
        }
    }
}
