// Implements: 06 Enemy Roster — stats are given relatively (Low/Med/High/Very High); this is the one place they become numbers.
namespace Ruminahui
{
    public enum EnemyType
    {
        TrainingDummy, Skirmisher, ShieldBearer, ShieldBearerArmored, HighlandScout, AtocLieutenant,
        SpanishInfantry, SpanishCavalry, Arquebusier, SpanishOfficer, AtocBoss,
    }

    public static class EnemyStats
    {
        // PLACEHOLDER-BALANCE: relative tiers from 06 mapped to numbers. Tune here, not in each enemy.
        public const float HpLow = 40f, HpMed = 90f, HpHigh = 300f, HpVeryHigh = 900f;
        public const float DmgLow = 6f, DmgMed = 12f, DmgMedHigh = 16f, DmgHigh = 22f, DmgVeryHigh = 45f;
        public const float SpeedLow = 2.2f, SpeedLowMed = 2.9f, SpeedMed = 3.6f, SpeedHigh = 5f, SpeedVeryHigh = 9f;
    }
}
