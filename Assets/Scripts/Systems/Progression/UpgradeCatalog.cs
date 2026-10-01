// Implements: 03-combat-design.md upgrade trees — names and what each tier does (text mirrors the doc; numbers mirror the kits).
namespace Ruminahui
{
    public static class UpgradeCatalog
    {
        public static string KitTitle(KitId k) =>
            k == KitId.Puma ? "Puma — Rumiñahui" : k == KitId.Kuntur ? "Kuntur — Chaska" : "Amaru — Atoc";

        public static string TierName(KitId k, int tier)
        {
            switch (k)
            {
                case KitId.Puma: return tier == 1 ? "Foundation" : tier == 2 ? "Weight" : "Stillness";
                case KitId.Kuntur: return tier == 1 ? "Foundation" : tier == 2 ? "Height" : "Precision";
                default: return tier == 1 ? "Foundation" : tier == 2 ? "Venom" : "Rebirth";
            }
        }

        public static string TierEffect(KitId k, int tier)
        {
            switch (k)
            {
                case KitId.Puma:
                    return tier == 1 ? "Stamina efficiency (costs ×0.8) · Stone Strikes extend to a 4-hit chain"
                         : tier == 2 ? "Guard-break +1 power · Grounding Throw ×1.5 damage · Earthbreaker radius +1.5 m"
                         : "Unyielding +2 s · Stone Parry window +0.1 s · Stone Face costs 30 Focus";
                case KitId.Kuntur:
                    return tier == 1 ? "Skywalk +1 m and a 3rd chained dash · Condor's Eye range +6 m"
                         : tier == 2 ? "Vantage Leap cooldown 4 s · Falling Star gains an extra aerial hit"
                         : "Star-Fall executes at ≤35% HP · The Gap in the Line lasts +2 s";
                default:
                    return tier == 1 ? "Snare capacity +1 · Shed Skin decoy lasts +0.8 s"
                         : tier == 2 ? "Venom ×1.5 potency, +2 s · Venom Riposte window +0.1 s · Numbing Draught +2 s"
                         : "Last Fang ×1.4 damage · A Hundred and One cooldown 60 s";
            }
        }
    }
}
