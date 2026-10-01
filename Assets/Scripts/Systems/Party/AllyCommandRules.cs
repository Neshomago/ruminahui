// Implements: 03-combat-design.md Section 4 — "AI allies who can be issued simple context commands (call-in a mark, call-in a
// trap, call-in a dodge-assist) rather than full squad-command complexity". Approved plan 2026-09-30.
// Pure rules (no MonoBehaviour) so availability/routing/cooldowns are unit-testable; AllyCommandSystem applies them.
using System.Collections.Generic;

namespace Ruminahui
{
    public enum AllyCommand { Mark, Trap, Cover }

    /// <summary>Snapshot of one party member as the rules see it.</summary>
    public struct AllyInfo
    {
        public CharacterId id;
        public string name;
        public KitId kit;
        public bool controlled;
        public AllyMode mode;
        public bool scriptLocked;
        public bool alive;
        public float distance;   // to the controlled character
    }

    public struct AllyCommandCheck
    {
        public bool Available;
        public int ExecutorIndex;   // index into the candidates list, -1 if none
        public string Reason;       // shown greyed in the call-in strip when unavailable

        public static AllyCommandCheck No(string reason) => new AllyCommandCheck { Available = false, ExecutorIndex = -1, Reason = reason };
    }

    public static class AllyCommandRules
    {
        public const float MaxRange = 30f;   // PLACEHOLDER-BALANCE

        // PLACEHOLDER-BALANCE: cooldowns per call-in (no Focus cost — allies never spend the shared meter).
        public static float CooldownFor(AllyCommand c) => c == AllyCommand.Mark ? 8f : c == AllyCommand.Trap ? 12f : 15f;

        public static string Label(AllyCommand c) => c == AllyCommand.Mark ? "Mark" : c == AllyCommand.Trap ? "Trap" : "Cover";

        /// <summary>Which kit a call-in needs. Cover works with anyone.</summary>
        public static KitId? RequiredKit(AllyCommand c) => c == AllyCommand.Mark ? KitId.Kuntur : c == AllyCommand.Trap ? KitId.Amaru : (KitId?)null;

        /// <summary>Display name of who *would* answer, for "X not here" messages.</summary>
        public static string ExpectedAlly(AllyCommand c) => c == AllyCommand.Mark ? "Chaska" : c == AllyCommand.Trap ? "Atoc" : "An ally";

        /// <summary>
        /// Picks the ally that answers <paramref name="command"/>. Routing is by KIT, not by name, so in M3.5 the call-ins go to whoever
        /// else is present. Cover picks the nearest eligible ally.
        /// </summary>
        public static AllyCommandCheck Evaluate(AllyCommand command, IList<AllyInfo> party, bool unlocked, bool freeSwapActive, bool offCooldown)
        {
            if (!unlocked) return AllyCommandCheck.No("Locked until M2.4");
            if (freeSwapActive) return AllyCommandCheck.No("Not in this mission");

            var kit = RequiredKit(command);
            int best = -1;
            float bestDistance = float.MaxValue;
            string blockedReason = null;

            for (int i = 0; i < party.Count; i++)
            {
                var a = party[i];
                if (a.controlled) continue;
                if (kit.HasValue && a.kit != kit.Value) continue;
                // Present but not able to answer → remember why (first reason wins), keep looking for someone who can.
                if (!a.alive) { blockedReason = blockedReason ?? $"{a.name} is down"; continue; }
                if (a.scriptLocked || a.mode != AllyMode.Combat) { blockedReason = blockedReason ?? $"{a.name} held back"; continue; }
                if (a.distance > MaxRange) { blockedReason = blockedReason ?? $"{a.name} too far"; continue; }
                if (a.distance < bestDistance) { bestDistance = a.distance; best = i; }
            }

            if (best < 0) return AllyCommandCheck.No(blockedReason ?? $"{ExpectedAlly(command)} not here");
            if (!offCooldown) return new AllyCommandCheck { Available = false, ExecutorIndex = best, Reason = "Cooling down" };
            return new AllyCommandCheck { Available = true, ExecutorIndex = best, Reason = null };
        }
    }

    /// <summary>Per-call-in cooldowns with injected time (testable).</summary>
    public class AllyCommandCooldowns
    {
        readonly Dictionary<AllyCommand, float> readyAt = new Dictionary<AllyCommand, float>();

        public bool IsReady(AllyCommand c, float now) => !readyAt.TryGetValue(c, out var t) || now >= t;

        public float Remaining(AllyCommand c, float now) => readyAt.TryGetValue(c, out var t) ? System.Math.Max(0f, t - now) : 0f;

        public float Normalized(AllyCommand c, float now) => Remaining(c, now) / AllyCommandRules.CooldownFor(c);

        public void Start(AllyCommand c, float now) => readyAt[c] = now + AllyCommandRules.CooldownFor(c);

        public void ResetAll() => readyAt.Clear();
    }
}
