// Implements: 04-dialogue-script.md data access — loads the importer's JSON from Resources/Dialogue by mission id.
using System.Collections.Generic;
using UnityEngine;

namespace Ruminahui
{
    public static class DialogueDatabase
    {
        public const string ResourceFolder = "Dialogue/";
        static readonly Dictionary<string, DialogueScript> cache = new Dictionary<string, DialogueScript>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => cache.Clear();

        /// <summary>"M0.1" → "Dialogue/M0_1" (dots avoided in Resources asset names — LESSONS_LEARNED L-019).</summary>
        public static string ResourcePath(string missionId) => ResourceFolder + missionId.Replace('.', '_');

        public static bool Has(string missionId) => Get(missionId) != null;

        /// <summary>Script for a mission, or null if 04 has none (most missions are unwritten yet).</summary>
        public static DialogueScript Get(string missionId)
        {
            if (string.IsNullOrEmpty(missionId)) return null;
            if (cache.TryGetValue(missionId, out var cached)) return cached;
            DialogueScript script = null;
            var asset = Resources.Load<TextAsset>(ResourcePath(missionId));
            if (asset != null)
            {
                script = JsonUtility.FromJson<DialogueScript>(asset.text);
                if (script != null && script.missionId != missionId)
                {
                    Debug.LogWarning($"[Dialogue] {ResourcePath(missionId)} contains '{script.missionId}', expected '{missionId}'.");
                    script = null;
                }
            }
            cache[missionId] = script; // cache misses too
            return script;
        }
    }
}
