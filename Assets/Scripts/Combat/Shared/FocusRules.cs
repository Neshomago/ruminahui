// Implements: 03-combat-design.md, Section 0 — Focus fills from clean hits, perfect dodges/parries and (post-M3.5) tell reading.
namespace Ruminahui
{
    public static class FocusRules
    {
        public const float Max = 100f;                // PLACEHOLDER-BALANCE: round number, costs below are fractions of it
        public const float CleanHit = 2.5f;           // PLACEHOLDER-BALANCE: ~40 clean hits to fill from empty
        public const float PerfectParry = 15f;        // PLACEHOLDER-BALANCE: doc says parry "fills FOC fast"
        public const float PerfectDodge = 9f;         // PLACEHOLDER-BALANCE: slightly less than a parry (less risky)
        public const float TellRead = 7f;             // PLACEHOLDER-BALANCE: bonus for acting during an enemy tell (post-M3.5 only)
        public const float Counter = 12f;             // PLACEHOLDER-BALANCE: Venom Riposte / Last Fang success
    }
}
