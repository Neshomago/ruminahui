// Implements: 03-combat-design.md, Section 0 (Core Resources) and Sections 1-3 (kit identities) — shared enums/structs.
using UnityEngine;

namespace Ruminahui
{
    public enum Faction { Player, Enemy, Neutral }
    public enum CharacterId { Ruminahui, Chaska, Atoc }
    public enum KitId { Puma, Kuntur, Amaru }

    public static class FactionUtil
    {
        public static bool AreHostile(Faction a, Faction b) => a != b && a != Faction.Neutral && b != Faction.Neutral;
    }

    [System.Flags]
    public enum HitFlags
    {
        None = 0,
        Heavy = 1 << 0,
        GuardBreak = 1 << 1,
        Unblockable = 1 << 2,
        Stagger = 1 << 3,
        Knockdown = 1 << 4,
        Ranged = 1 << 5,
        Grab = 1 << 6,
        Execute = 1 << 7,
        NoDamageNumber = 1 << 8,
        Charge = 1 << 9,
        Poison = 1 << 10,
    }

    public struct DamageInfo
    {
        public float Amount;
        public CombatTarget Attacker;
        public Vector3 Point;
        public Vector3 Direction;      // direction the hit travels (attacker -> target)
        public HitFlags Flags;
        public int GuardBreakPower;
        public float StaggerDuration;
        public float Knockback;
        public string AttackName;

        public bool Has(HitFlags f) => (Flags & f) != 0;
    }

    public enum HitOutcome { Ignored, Hit, Blocked, Parried, Evaded, Absorbed, Countered, Killed }

    public struct HitResult
    {
        public HitOutcome Outcome;
        public float DamageDealt;

        public HitResult(HitOutcome outcome, float dealt = 0f)
        {
            Outcome = outcome;
            DamageDealt = dealt;
        }

        public bool Landed => Outcome == HitOutcome.Hit || Outcome == HitOutcome.Killed;
    }

    /// <summary>Anything that can reshape or consume an incoming hit (block, parry, dodge i-frames, shields, decoys, rebirth).</summary>
    public interface IHitInterceptor
    {
        /// <summary>Higher runs first. Convention: i-frames 100, counters/parry 90, block/shield 80, damage modifiers 50, rebirth 10.</summary>
        int InterceptPriority { get; }

        /// <summary>Return true to consume the hit with <paramref name="result"/>. Return false to let it continue (info may be modified).</summary>
        bool Intercept(ref DamageInfo info, out HitResult result);
    }
}
