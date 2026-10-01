// Implements: 03-combat-design.md — which Act/mission unlocks each special, ultimate and the post-M3.5 tell-reading Focus source.
namespace Ruminahui
{
    public static class Unlocks
    {
        // Puma
        public const string Earthbreaker = "Puma.Earthbreaker";     // Act I
        public const string Unyielding = "Puma.Unyielding";         // Act II, post-Willka's death (M2.3)
        public const string StoneFace = "Puma.StoneFace";           // M3.4, after sparing Atoc
        public const string WhatHeldTheLine = "Puma.Ultimate";      // M3.5
        // Kuntur
        public const string VantageLeap = "Kuntur.VantageLeap";     // Act I, tutorialised in M1.1
        public const string SkyCut = "Kuntur.SkyCut";               // M3.2
        public const string StarFall = "Kuntur.StarFall";           // post-M3.5
        public const string GapInTheLine = "Kuntur.Ultimate";       // M3.5
        // Amaru
        public const string NumbingDraught = "Amaru.NumbingDraught"; // post-M3.6
        public const string FalseTrail = "Amaru.FalseTrail";         // M5.4
        public const string LastFang = "Amaru.LastFang";             // Act V
        public const string HundredAndOne = "Amaru.Ultimate";        // M3.5
        // Systems
        public const string TellReading = "Focus.TellReading";       // post-M3.5 Focus source (03 Section 0)
        public const string AllyCommands = "Ally.Commands";          // 03 Section 4 call-ins; 02 M2.4 "light command-tutorial"
        public const string ThreeWorldsMenu = "Menu.ThreeWorlds";    // 02 M3.6 "full unlock of the Three-Worlds ability menu"
    }

    /// <summary>Mission id from which each flag is available (inclusive).</summary>
    public static class UnlockSchedule
    {
        public static readonly (string flag, string fromMission)[] Entries =
        {
            (Unlocks.VantageLeap, "M1.1"),
            (Unlocks.Earthbreaker, "M1.3"),     // ASSUMPTION: first crowd-control mission in Act I
            (Unlocks.Unyielding, "M2.4"),       // first mission after Willka's death
            (Unlocks.AllyCommands, "M2.4"),     // 02: M2.4 introduces ally-command mechanics
            (Unlocks.SkyCut, "M3.2"),
            (Unlocks.StoneFace, "M3.5"),        // unlocked by completing M3.4
            (Unlocks.WhatHeldTheLine, "M3.6"),  // granted live at M3.5 stage N, permanently from M3.6
            (Unlocks.GapInTheLine, "M3.6"),
            (Unlocks.HundredAndOne, "M3.6"),
            (Unlocks.StarFall, "M3.6"),
            (Unlocks.TellReading, "M3.6"),
            (Unlocks.ThreeWorldsMenu, "M3.6"),
            (Unlocks.NumbingDraught, "M4.1"),   // first mission after M3.6
            (Unlocks.LastFang, "M5.1"),         // ASSUMPTION: "Act V" → from its first mission
            (Unlocks.FalseTrail, "M5.4"),
        };
    }
}
