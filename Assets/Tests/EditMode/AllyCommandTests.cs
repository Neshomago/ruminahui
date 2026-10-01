// Tests: 03-combat-design.md Section 4 ally call-ins (approved plan): availability, routing by kit, reasons, cooldowns, Cover rule.
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Ruminahui.Tests
{
    public class AllyCommandTests
    {
        static AllyInfo A(CharacterId id, KitId kit, bool controlled = false, AllyMode mode = AllyMode.Combat,
            bool locked = false, bool alive = true, float distance = 5f) =>
            new AllyInfo { id = id, name = id.ToString(), kit = kit, controlled = controlled, mode = mode, scriptLocked = locked, alive = alive, distance = distance };

        static List<AllyInfo> FullParty() => new List<AllyInfo>
        {
            A(CharacterId.Ruminahui, KitId.Puma, controlled: true, distance: 0f),
            A(CharacterId.Chaska, KitId.Kuntur, distance: 6f),
            A(CharacterId.Atoc, KitId.Amaru, distance: 3f),
        };

        [Test]
        public void Mark_GoesToKunturAlly_Trap_GoesToAmaruAlly()
        {
            var party = FullParty();
            var mark = AllyCommandRules.Evaluate(AllyCommand.Mark, party, true, false, true);
            var trap = AllyCommandRules.Evaluate(AllyCommand.Trap, party, true, false, true);
            Assert.IsTrue(mark.Available);
            Assert.AreEqual(CharacterId.Chaska, party[mark.ExecutorIndex].id);
            Assert.IsTrue(trap.Available);
            Assert.AreEqual(CharacterId.Atoc, party[trap.ExecutorIndex].id);
        }

        [Test]
        public void Cover_PicksNearestEligibleAlly()
        {
            var party = FullParty();
            var cover = AllyCommandRules.Evaluate(AllyCommand.Cover, party, true, false, true);
            Assert.IsTrue(cover.Available);
            Assert.AreEqual(CharacterId.Atoc, party[cover.ExecutorIndex].id); // 3 m vs 6 m
        }

        [Test]
        public void RoutingIsByKit_SoM35SectionsWork()
        {
            // M3.5 D–F: the player controls Chaska. Mark has no other Kuntur user; Trap still goes to Atoc.
            var party = new List<AllyInfo>
            {
                A(CharacterId.Ruminahui, KitId.Puma, distance: 4f),
                A(CharacterId.Chaska, KitId.Kuntur, controlled: true, distance: 0f),
                A(CharacterId.Atoc, KitId.Amaru, distance: 8f),
            };
            var mark = AllyCommandRules.Evaluate(AllyCommand.Mark, party, true, false, true);
            Assert.IsFalse(mark.Available);
            Assert.AreEqual("Chaska not here", mark.Reason);
            Assert.AreEqual(CharacterId.Atoc, party[AllyCommandRules.Evaluate(AllyCommand.Trap, party, true, false, true).ExecutorIndex].id);
            Assert.AreEqual(CharacterId.Ruminahui, party[AllyCommandRules.Evaluate(AllyCommand.Cover, party, true, false, true).ExecutorIndex].id);
        }

        [Test]
        public void Reasons_Locked_FreeSwap_Absent_HeldBack_Down_TooFar_Cooldown()
        {
            var party = FullParty();
            Assert.AreEqual("Locked until M2.4", AllyCommandRules.Evaluate(AllyCommand.Mark, party, false, false, true).Reason);
            Assert.AreEqual("Not in this mission", AllyCommandRules.Evaluate(AllyCommand.Mark, party, true, true, true).Reason);

            var solo = new List<AllyInfo> { A(CharacterId.Ruminahui, KitId.Puma, controlled: true) };
            Assert.AreEqual("Atoc not here", AllyCommandRules.Evaluate(AllyCommand.Trap, solo, true, false, true).Reason);

            var caged = new List<AllyInfo> { party[0], A(CharacterId.Atoc, KitId.Amaru, mode: AllyMode.Idle, locked: true) };
            Assert.AreEqual("Atoc held back", AllyCommandRules.Evaluate(AllyCommand.Trap, caged, true, false, true).Reason);

            var down = new List<AllyInfo> { party[0], A(CharacterId.Chaska, KitId.Kuntur, alive: false) };
            Assert.AreEqual("Chaska is down", AllyCommandRules.Evaluate(AllyCommand.Mark, down, true, false, true).Reason);

            var far = new List<AllyInfo> { party[0], A(CharacterId.Chaska, KitId.Kuntur, distance: 45f) };
            Assert.AreEqual("Chaska too far", AllyCommandRules.Evaluate(AllyCommand.Mark, far, true, false, true).Reason);

            var cd = AllyCommandRules.Evaluate(AllyCommand.Mark, party, true, false, false);
            Assert.IsFalse(cd.Available);
            Assert.AreEqual("Cooling down", cd.Reason);
            Assert.AreEqual(CharacterId.Chaska, party[cd.ExecutorIndex].id); // the strip still shows who
        }

        [Test]
        public void FollowModeAllies_DoNotAnswer()
        {
            var party = new List<AllyInfo> { A(CharacterId.Ruminahui, KitId.Puma, controlled: true), A(CharacterId.Chaska, KitId.Kuntur, mode: AllyMode.Follow) };
            Assert.IsFalse(AllyCommandRules.Evaluate(AllyCommand.Mark, party, true, false, true).Available);
        }

        [Test]
        public void Cooldowns_StartAndExpire_Independently()
        {
            var cd = new AllyCommandCooldowns();
            Assert.IsTrue(cd.IsReady(AllyCommand.Mark, 0f));
            cd.Start(AllyCommand.Mark, 10f);
            Assert.IsFalse(cd.IsReady(AllyCommand.Mark, 10f + AllyCommandRules.CooldownFor(AllyCommand.Mark) - 0.01f));
            Assert.IsTrue(cd.IsReady(AllyCommand.Mark, 10f + AllyCommandRules.CooldownFor(AllyCommand.Mark)));
            Assert.IsTrue(cd.IsReady(AllyCommand.Trap, 10f));
            Assert.AreEqual(1.0, cd.Normalized(AllyCommand.Mark, 10f), 0.001);
            cd.ResetAll();
            Assert.IsTrue(cd.IsReady(AllyCommand.Mark, 10f));
        }

        [Test]
        public void CallIns_UnlockAtM24()
        {
            Progression.ApplyForMission("M2.3");
            Assert.IsFalse(Progression.IsUnlocked(Unlocks.AllyCommands));
            Progression.ApplyForMission("M2.4");
            Assert.IsTrue(Progression.IsUnlocked(Unlocks.AllyCommands));
        }

        [Test]
        public void Cover_OnlyCoversArmedHostileHits()
        {
            var enemyGo = new GameObject("Enemy");
            var enemy = enemyGo.AddComponent<CombatTarget>();
            enemy.faction = Faction.Enemy;
            var hit = new DamageInfo { Amount = 10f, Attacker = enemy };

            Assert.IsTrue(CoverAssist.ShouldCover(true, hit, Faction.Player));
            Assert.IsFalse(CoverAssist.ShouldCover(false, hit, Faction.Player));          // not armed
            Assert.IsFalse(CoverAssist.ShouldCover(true, new DamageInfo { Amount = 10f }, Faction.Player)); // no attacker (poison, falls)
            enemy.isDecoy = true;
            Assert.IsFalse(CoverAssist.ShouldCover(true, hit, Faction.Player));           // decoys don't hit for real
            enemy.isDecoy = false;
            enemy.faction = Faction.Player;
            Assert.IsFalse(CoverAssist.ShouldCover(true, hit, Faction.Player));           // friendly
            Object.DestroyImmediate(enemyGo);
        }
    }
}
