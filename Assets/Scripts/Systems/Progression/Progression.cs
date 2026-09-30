// Implements: 03-combat-design.md — ability unlocks across Acts and the 3-tier upgrade trees (Foundation / Weight|Height|Venom /
// Stillness|Precision|Rebirth). In-memory for now; save/load is a later phase.
using System.Collections.Generic;
using UnityEngine;

namespace Ruminahui
{
    public static class Progression
    {
        static readonly HashSet<string> flags = new HashSet<string>();
        static readonly int[] tiers = new int[3];
        static readonly HashSet<string> collected = new HashSet<string>();

        /// <summary>Debug / test scenes: everything available.</summary>
        public static bool UnlockAll;

        public static event System.Action Changed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            flags.Clear();
            collected.Clear();
            for (int i = 0; i < tiers.Length; i++) tiers[i] = 0;
            UnlockAll = false;
            Changed = null;
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

        /// <summary>Rebuilds unlocks for the given mission from UnlockSchedule.</summary>
        public static void ApplyForMission(string missionId)
        {
            int index = MissionDatabase.IndexOf(missionId);
            if (index < 0) return;
            flags.Clear();
            foreach (var (flag, fromId) in UnlockSchedule.Entries)
                if (MissionDatabase.IndexOf(fromId) <= index) flags.Add(flag);

            // PLACEHOLDER-BALANCE: docs define the trees but no currency; default tiers by Act until an upgrade menu exists.
            int act = MissionDatabase.Get(missionId).Act;
            int tier = act >= 5 ? 3 : act >= 3 ? 2 : act >= 1 ? 1 : 0;
            for (int i = 0; i < tiers.Length; i++) tiers[i] = tier;
            Changed?.Invoke();
        }

        public static bool Collect(string id) => collected.Add(id);
        public static int CollectedCount => collected.Count;
    }
}
