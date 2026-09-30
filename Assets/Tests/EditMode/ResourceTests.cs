// Tests: 03-combat-design.md Section 0 — HP (no regen, floors), Stamina (spend/exhaust), Focus (shared, clamped).
using NUnit.Framework;
using UnityEngine;

namespace Ruminahui.Tests
{
    public class ResourceTests
    {
        GameObject go;

        [SetUp] public void SetUp() => go = new GameObject("TestTarget");
        [TearDown] public void TearDown() => Object.DestroyImmediate(go);

        [Test]
        public void Focus_AddClampsToMax_AndReportsGain()
        {
            var f = new FocusPool(100f);
            float gained = 0f;
            f.Gained += (_, amt) => gained += amt;
            f.Add(80f, "a");
            f.Add(50f, "b");
            Assert.AreEqual(100f, f.Current, 0.001f);
            Assert.AreEqual(100f, gained, 0.001f);
        }

        [Test]
        public void Focus_SpendFailsWhenInsufficient()
        {
            var f = new FocusPool(100f, 20f);
            Assert.IsFalse(f.TrySpend(30f));
            Assert.AreEqual(20f, f.Current, 0.001f);
            Assert.IsTrue(f.TrySpend(20f));
            Assert.AreEqual(0f, f.Current, 0.001f);
        }

        [Test]
        public void Health_DiesAtZero_AndRespectsMinimumFloor()
        {
            var h = go.AddComponent<Health>();
            h.max = 50f;
            h.ResetFull();
            h.ApplyDamage(20f);
            Assert.AreEqual(30f, h.Current, 0.001f);
            Assert.IsFalse(h.IsDead);

            h.minimumHp = 1f; // bosses that must be spared / AI allies
            h.ApplyDamage(999f);
            Assert.AreEqual(1f, h.Current, 0.001f);
            Assert.IsFalse(h.IsDead);

            h.minimumHp = 0f;
            h.ApplyDamage(999f);
            Assert.IsTrue(h.IsDead);
        }

        [Test]
        public void Health_HasNoPassiveRegen_OnlyExplicitHeal()
        {
            var h = go.AddComponent<Health>();
            h.max = 100f;
            h.ResetFull();
            h.ApplyDamage(40f);
            Assert.AreEqual(60f, h.Current, 0.001f); // nothing ticks it back up
            h.Heal(15f);
            Assert.AreEqual(75f, h.Current, 0.001f);
        }

        [Test]
        public void Stamina_SpendUntilExhausted_ThenRefuses()
        {
            var s = go.AddComponent<Stamina>();
            s.max = 30f;
            s.ResetFull();
            Assert.IsTrue(s.TrySpend(20f));
            Assert.IsTrue(s.TrySpend(20f));   // allowed to dip to 0 while any remains
            Assert.IsTrue(s.IsExhausted);
            Assert.IsFalse(s.TrySpend(1f));
        }

        [Test]
        public void Stamina_CostMultiplierAppliesToSpend()
        {
            var s = go.AddComponent<Stamina>();
            s.max = 100f;
            s.ResetFull();
            s.costMultiplier = 0.8f; // Puma Foundation tier
            s.TrySpend(50f);
            Assert.AreEqual(60f, s.Current, 0.001f);
        }
    }
}
