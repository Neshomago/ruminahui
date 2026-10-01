// Implements: 03-combat-design.md — ability unlocks across Acts and the 3-tier upgrade trees (Foundation / Weight|Height|Venom /
// Stillness|Precision|Rebirth). Tiers are BOUGHT with upgrade points (UpgradeEconomy, approved 2026-10-01); persisted by SaveSystem.
using System.Collections.Generic;
using UnityEngine;

namespace Ruminahui
{
    public static class Progression
    {
        static readonly HashSet<string> flags = new HashSet<string>();
        static readonly int[] tiers = new int[3];
        static readonly HashSet<string> collected = new HashSet<string>();
        static readonly HashSet<string> completed = new HashSet<string>();

        /// <summary>Debug / test scenes: everything available.</summary>
        public static bool UnlockAll;

        public static event System.Action Changed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            ResetAll();
            Changed = null;
        }

        /// <summary>New game: wipe everything.</summary>
        public static void ResetAll()
        {
            flags.Clear();
            collected.Clear();
            completed.Clear();
            for (int i = 0; i < tiers.Length; i++) tiers[i] = 0;
            UnlockAll = false;
            Changed?.Invoke();
        }

        public static bool IsUnlocked(string flag) => string.IsNullOrEmpty(flag) || UnlockAll || flags.Contains(flag);

        public static void Unlock(string flag)
        {
            if (flags.Add(flag)) Changed?.Invoke();
        }

        public static void SetUnlockAll(bool value)
        {
            UnlockAll = value;
            Changed?.Invoke();
        }

        /// <summary>Upgrade tier 0-3 for a kit (0 = base kit, 3 = all three tiers bought).</summary>
        public static int GetTier(KitId kit) => tiers[(int)kit];

        public static void SetTier(KitId kit, int tier)
        {
            tiers[(int)kit] = Mathf.Clamp(tier, 0, 3);
            Changed?.Invoke();
        }

        public static int TotalTiersBought => tiers[0] + tiers[1] + tiers[2];

        /// <summary>Rebuilds ability unlocks for the given mission from UnlockSchedule. Bought tiers are untouched.</summary>
        public static void ApplyForMission(string missionId)
        {
            int index = MissionDatabase.IndexOf(missionId);
            if (index < 0) return;
            flags.Clear();
            foreach (var (flag, fromId) in UnlockSchedule.Entries)
                if (MissionDatabase.IndexOf(fromId) <= index) flags.Add(flag);
            Changed?.Invoke();
        }

        // ───────── Relics ─────────
        public static bool Collect(string id)
        {
            bool added = collected.Add(id);
            if (added) Changed?.Invoke();
            return added;
        }

        public static int CollectedCount => collected.Count;
        public static bool HasCollected(string id) => collected.Contains(id);
        public static IEnumerable<string> CollectedIds => collected;

        // ───────── Mission completion ─────────
        public static void MarkCompleted(string missionId)
        {
            if (MissionDatabase.IsMission(missionId) && completed.Add(missionId)) Changed?.Invoke();
        }

        public static bool IsCompleted(string missionId) => completed.Contains(missionId);
        public static IEnumerable<string> CompletedMissions => completed;

        /// <summary>Chapter select: jumping to a mission counts everything before it as played (so points/unlocks catch up).</summary>
        public static void CatchUpTo(string missionId)
        {
            int index = MissionDatabase.IndexOf(missionId);
            for (int i = 0; i < index; i++) completed.Add(MissionDatabase.Missions[i].Id);
            Changed?.Invoke();
        }

        /// <summary>Restores state from a save (SaveSystem).</summary>
        public static void Restore(IEnumerable<string> completedIds, IEnumerable<string> relicIds, int[] savedTiers)
        {
            completed.Clear();
            collected.Clear();
            if (completedIds != null) foreach (var c in completedIds) completed.Add(c);
            if (relicIds != null) foreach (var r in relicIds) collected.Add(r);
            for (int i = 0; i < tiers.Length; i++) tiers[i] = savedTiers != null && i < savedTiers.Length ? Mathf.Clamp(savedTiers[i], 0, 3) : 0;
            Changed?.Invoke();
        }
    }
}
