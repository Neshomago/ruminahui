// Implements: 03-combat-design.md upgrade trees (3 tiers per kit) with the approved economy (2026-10-01):
// 1 point per completed combat mission + 1 per relic; each tier costs 1; tiers are bought in order.
// 02 M3.6: "full unlock of the Three-Worlds ability menu going forward" → Kuntur/Amaru tabs open from M3.6.
namespace Ruminahui
{
    public enum UpgradeStatus { Bought, Available, NeedsPreviousTier, NotEnoughPoints, KitLocked }

    public static class UpgradeEconomy
    {
        public const int CostPerTier = 1;   // PLACEHOLDER-BALANCE (approved default)

        public static int EarnedPoints()
        {
            int points = 0;
            foreach (var id in Progression.CompletedMissions)
            {
                var m = MissionDatabase.Get(id);
                if (m != null && m.AwardsUpgradePoint) points++;
            }
            return points + Progression.CollectedCount;
        }

        public static int SpentPoints() => Progression.TotalTiersBought * CostPerTier;

        public static int AvailablePoints() => System.Math.Max(0, EarnedPoints() - SpentPoints());

        /// <summary>Puma from the start; Kuntur and Amaru once the Three-Worlds menu unlocks (M3.6) — or in test scenes.</summary>
        public static bool KitMenuUnlocked(KitId kit) => kit == KitId.Puma || Progression.IsUnlocked(Unlocks.ThreeWorldsMenu);

        /// <summary>Status of tier (1-3) for a kit.</summary>
        public static UpgradeStatus StatusOf(KitId kit, int tier)
        {
            int owned = Progression.GetTier(kit);
            if (tier <= owned) return UpgradeStatus.Bought;
            if (!KitMenuUnlocked(kit)) return UpgradeStatus.KitLocked;
            if (tier > owned + 1) return UpgradeStatus.NeedsPreviousTier;
            if (AvailablePoints() < CostPerTier) return UpgradeStatus.NotEnoughPoints;
            return UpgradeStatus.Available;
        }

        public static bool TryBuy(KitId kit, int tier)
        {
            if (StatusOf(kit, tier) != UpgradeStatus.Available) return false;
            Progression.SetTier(kit, tier);
            return true;
        }
    }
}
