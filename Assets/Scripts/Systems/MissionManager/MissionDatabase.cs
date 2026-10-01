// Implements: 02-mission-list.md — the full 26-mission manifest (IDs, titles, Acts, playable characters).
// Scene names match mission IDs exactly (e.g. "M1.3"), per AI_BUILD_PROMPT.md PROJECT STRUCTURE.
using System.Collections.Generic;

namespace Ruminahui
{
    public class MissionDefinition
    {
        public string Id;
        public string Title;
        public int Act;              // 0 = Prologue
        public string PlaysAs;
        public bool HasBuiltContent; // false → placeholder scene with a "complete mission" zone
        /// <summary>Completing it earns 1 upgrade point (approved economy: combat missions only).</summary>
        public bool AwardsUpgradePoint => CombatMissions.Contains(Id);

        // Missions whose 02 gameplay notes include combat (M4.2's optional gauntlet counts).
        static readonly System.Collections.Generic.HashSet<string> CombatMissions = new System.Collections.Generic.HashSet<string>
        {
            "M1.1", "M1.3", "M2.1", "M2.2", "M2.3", "M3.1", "M3.2", "M3.3", "M3.4", "M3.5", "M4.1", "M4.2", "M5.1", "M5.2", "M5.4",
        };

        public MissionDefinition(string id, string title, int act, string playsAs, bool built = false)
        {
            Id = id; Title = title; Act = act; PlaysAs = playsAs; HasBuiltContent = built;
        }

        public string SceneKey => Id;
    }

    public static class MissionDatabase
    {
        public static readonly List<MissionDefinition> Missions = new List<MissionDefinition>
        {
            new MissionDefinition("M0.1", "Night Raid", 0, "Pillahuaso (child)"),
            new MissionDefinition("M0.2", "The Name They Gave Him", 0, "Pillahuaso (child)", true),
            new MissionDefinition("M1.1", "Two Prodigies", 1, "Rumiñahui (teen)", true),
            new MissionDefinition("M1.2", "The Three Worlds", 1, "Rumiñahui (teen)"),
            new MissionDefinition("M1.3", "What Discipline Buys", 1, "Rumiñahui (18)", true),
            new MissionDefinition("M1.4", "Leaving the Forest", 1, "Rumiñahui"),
            new MissionDefinition("M2.1", "Garrison Duty", 2, "Rumiñahui", true),
            new MissionDefinition("M2.2", "The Prince Notices", 2, "Rumiñahui"),
            new MissionDefinition("M2.3", "Willka's Last Stand", 2, "Rumiñahui", true),
            new MissionDefinition("M2.4", "The Empire Splits", 2, "Rumiñahui"),
            new MissionDefinition("M3.1", "Opening Moves", 3, "Rumiñahui (+Chaska AI)", true),
            new MissionDefinition("M3.2", "The Fox's Ground", 3, "Chaska", true),
            new MissionDefinition("M3.3", "Siege of Mullihambato, Part I", 3, "Rumiñahui", true),
            new MissionDefinition("M3.4", "The Mercy", 3, "Rumiñahui", true),
            new MissionDefinition("M3.5", "What Held the Line", 3, "Rumiñahui → Chaska → Atoc", true),
            new MissionDefinition("M3.6", "After the Fire", 3, "Rumiñahui"),
            new MissionDefinition("M4.1", "Strange Ships", 4, "Rumiñahui"),
            new MissionDefinition("M4.2", "The News from Cajamarca", 4, "Rumiñahui", true),
            new MissionDefinition("M4.3", "The Execution / North to Quito", 4, "Rumiñahui"),
            new MissionDefinition("M5.1", "Benalcázar's Advance", 5, "Rumiñahui", true),
            new MissionDefinition("M5.2", "The Hardest Order", 5, "Rumiñahui", true),
            new MissionDefinition("M5.3", "Into the Llanganates", 5, "Rumiñahui / Chaska / Atoc (free swap)", true),
            new MissionDefinition("M5.4", "The Long Retreat", 5, "Rumiñahui (+Chaska/Atoc AI)", true),
            new MissionDefinition("M5.5", "Sigchos", 5, "Rumiñahui"),
            new MissionDefinition("M5.6", "Stone Face", 5, "Rumiñahui"),
            new MissionDefinition("M5.7", "What the Mountains Keep", 5, "Chaska"),
        };

        /// <summary>Test scenes (not part of the campaign) — see Levels/LevelBuilder.cs.</summary>
        public static readonly string[] TestScenes =
        {
            "Test_PumaDummy", "Test_EarlyEncounter", "Test_Kuntur", "Test_Amaru", "Test_AtocBoss",
            "Test_AllEnemies", "Test_M3_5_ForcedSwap", "Test_M5_3_FreeSwap",
        };

        public const string BootScene = "_Boot";

        public static int IndexOf(string id)
        {
            for (int i = 0; i < Missions.Count; i++) if (Missions[i].Id == id) return i;
            return -1;
        }

        public static MissionDefinition Get(string id)
        {
            int i = IndexOf(id);
            return i >= 0 ? Missions[i] : null;
        }

        public static MissionDefinition Next(string id)
        {
            int i = IndexOf(id);
            return i >= 0 && i + 1 < Missions.Count ? Missions[i + 1] : null;
        }

        public static bool IsMission(string id) => IndexOf(id) >= 0;
    }
}
