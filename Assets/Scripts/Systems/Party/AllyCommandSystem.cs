// Implements: 03-combat-design.md Section 4 ally call-ins (approved plan 2026-09-30) — Mark (Kuntur ally), Trap (Amaru ally),
// Cover (any ally). Input: Z / X / C, or hold LB + Y / X / B. No Focus cost; per-call-in cooldowns. Unlocked from M2.4
// (02: "light command-tutorial … ally-command mechanics"). Disabled while free swap is active (M5.3).
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ruminahui
{
    public class AllyCommandSystem : Singleton<AllyCommandSystem>
    {
        public const float TrapDelay = 0.4f;            // PLACEHOLDER-BALANCE: the throw
        public const float AimRange = 20f;
        public const float AimHalfAngle = 35f;
        public const float MarkSplashRadius = 6f;
        public const int MarkSplashCount = 2;           // "plus up to 2 enemies near it"
        public const float TrapNoTargetDistance = 3f;   // with no target, the snare lands ahead of you (pre-placing for ambushes)

        public readonly AllyCommandCooldowns Cooldowns = new AllyCommandCooldowns();
        /// <summary>Raised after a call-in succeeds (M2.4 command tutorial listens).</summary>
        public event System.Action<AllyCommand, PlayerCharacter> Issued;
        static readonly AllyCommand[] All = { AllyCommand.Mark, AllyCommand.Trap, AllyCommand.Cover };

        readonly List<AllyInfo> infos = new List<AllyInfo>();
        readonly List<PlayerCharacter> members = new List<PlayerCharacter>();

        public bool Unlocked => Progression.IsUnlocked(Unlocks.AllyCommands);
        bool FreeSwapActive => FreeSwapController.Instance != null && FreeSwapController.Instance.Active;

        /// <summary>True when there's at least one AI ally in the scene (the call-in strip hides otherwise).</summary>
        public bool HasAllies
        {
            get
            {
                var pm = PartyManager.Instance;
                if (pm == null || pm.Controlled == null) return false;
                foreach (var m in pm.Members) if (m != null && m != pm.Controlled) return true;
                return false;
            }
        }

        void Update()
        {
            var input = GameInput.Instance;
            if (input == null || FreeSwapActive) return; // M5.3: LB is the swap button, and there's no combat
            var leader = PartyManager.Instance != null ? PartyManager.Instance.Controlled : null;
            if (leader != null && leader.CombatDisabled) return;
            if (input.Pressed(input.CallMark)) TryIssue(AllyCommand.Mark);
            if (input.Pressed(input.CallTrap)) TryIssue(AllyCommand.Trap);
            if (input.Pressed(input.CallCover)) TryIssue(AllyCommand.Cover);
        }

        // ───────── Availability ─────────
        void Snapshot()
        {
            infos.Clear();
            members.Clear();
            var pm = PartyManager.Instance;
            if (pm == null || pm.Controlled == null) return;
            var leader = pm.Controlled;
            foreach (var m in pm.Members)
            {
                if (m == null || m.Kit == null) continue;
                members.Add(m);
                infos.Add(new AllyInfo
                {
                    id = m.id,
                    name = m.displayName,
                    kit = m.Kit.Kit,
                    controlled = m == leader,
                    mode = m.allyMode,
                    scriptLocked = m.ScriptLocked,
                    alive = !m.Health.IsDead,
                    distance = Vector3.Distance(m.transform.position, leader.transform.position),
                });
            }
        }

        public AllyCommandCheck Check(AllyCommand c, out PlayerCharacter executor)
        {
            executor = null;
            var pm = PartyManager.Instance;
            if (pm == null || pm.Controlled == null) return AllyCommandCheck.No("No one to command");
            Snapshot();
            var check = AllyCommandRules.Evaluate(c, infos, Unlocked, FreeSwapActive, Cooldowns.IsReady(c, Time.time));
            if (check.ExecutorIndex >= 0) executor = members[check.ExecutorIndex];
            return check;
        }

        public float CooldownNormalized(AllyCommand c) => Cooldowns.Normalized(c, Time.time);

        public void ResetCooldowns() => Cooldowns.ResetAll();

        // ───────── Issue ─────────
        public bool TryIssue(AllyCommand c)
        {
            var check = Check(c, out var ally);
            var leader = PartyManager.Instance != null ? PartyManager.Instance.Controlled : null;
            if (!check.Available || ally == null || leader == null)
            {
                if (HasAllies || !Unlocked) ObjectiveTracker.Say("Call-in", $"{AllyCommandRules.Label(c)}: {check.Reason}");
                return false;
            }

            bool done;
            switch (c)
            {
                case AllyCommand.Mark: done = DoMark(ally, leader); break;
                case AllyCommand.Trap: done = DoTrap(ally, leader); break;
                default: done = DoCover(ally, leader); break;
            }
            if (done)
            {
                Cooldowns.Start(c, Time.time);
                if (ally.Visual != null) ally.Visual.Flash(Color.white, 0.25f, false);
                Issued?.Invoke(c, ally);
            }
            return done;
        }

        CombatTarget AimTarget(PlayerCharacter leader)
        {
            if (leader.Targeting != null && leader.Targeting.Locked != null && !leader.Targeting.Locked.isDecoy) return leader.Targeting.Locked;
            var fwd = leader.Cameras != null ? leader.Cameras.Forward : leader.transform.forward;
            return CombatQuery.BestInCone(leader.Target, leader.transform.position, fwd, AimRange, AimHalfAngle);
        }

        bool DoMark(PlayerCharacter ally, PlayerCharacter leader)
        {
            var kk = ally.Kit as KunturKit;
            var target = AimTarget(leader);
            if (kk == null || target == null)
            {
                ObjectiveTracker.Say("Call-in", "Mark: no target in view");
                return false; // no cooldown spent on a miss
            }
            kk.MarkTarget(target);
            // Up to N more enemies near the target.
            var near = CombatQuery.Sphere(leader.Target, target.transform.position, MarkSplashRadius, false);
            near.Sort((a, b) => Vector3.Distance(a.transform.position, target.transform.position)
                .CompareTo(Vector3.Distance(b.transform.position, target.transform.position)));
            int extra = 0;
            foreach (var t in near)
            {
                if (t == target || extra >= MarkSplashCount) continue;
                kk.MarkTarget(t);
                extra++;
            }
            ally.Motor.FaceTowards(target.transform.position);
            ObjectiveTracker.Say(ally.displayName, extra > 0 ? $"Marked — {1 + extra} of them." : "Marked.");
            return true;
        }

        bool DoTrap(PlayerCharacter ally, PlayerCharacter leader)
        {
            var target = AimTarget(leader);
            Vector3 spot = target != null
                ? target.transform.position
                : leader.transform.position + (leader.Cameras != null ? leader.Cameras.Forward : leader.transform.forward) * TrapNoTargetDistance;
            ally.Motor.FaceTowards(spot);
            StartCoroutine(PlaceTrapAfter(ally, target, spot));
            ObjectiveTracker.Say(ally.displayName, target != null ? "Snare — at its feet." : "Snare down ahead.");
            return true;
        }

        IEnumerator PlaceTrapAfter(PlayerCharacter ally, CombatTarget target, Vector3 fallbackSpot)
        {
            yield return new WaitForSeconds(TrapDelay);
            if (ally == null) yield break;
            // Track a moving target through the throw; it lands where they are now.
            var spot = target != null && target.IsAlive ? target.transform.position : fallbackSpot;
            Snare.Place(ally.Target, spot);
        }

        bool DoCover(PlayerCharacter ally, PlayerCharacter leader)
        {
            var cover = leader.GetComponent<CoverAssist>();
            if (cover == null) cover = leader.gameObject.AddComponent<CoverAssist>();
            cover.Arm(ally.displayName);
            ObjectiveTracker.Say(ally.displayName, "I've got your back.");
            return true;
        }
    }
}
