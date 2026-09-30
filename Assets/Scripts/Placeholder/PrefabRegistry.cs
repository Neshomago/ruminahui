// Implements: AI_BUILD_PROMPT.md placeholder convention — "swapping in real models later is a matter of replacing a prefab reference".
// Lives at Resources/PrefabRegistry.asset (created by the editor tool). Empty/missing → factories build from code.
using System.Collections.Generic;
using UnityEngine;

namespace Ruminahui
{
    [CreateAssetMenu(menuName = "Rumiñahui/Prefab Registry", fileName = "PrefabRegistry")]
    public class PrefabRegistry : ScriptableObject
    {
        [System.Serializable]
        public class Entry
        {
            public string id;
            public GameObject prefab;
        }

        public bool usePrefabs = true;
        public List<Entry> entries = new List<Entry>();

        static PrefabRegistry cached;
        static bool looked;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { cached = null; looked = false; }

        public static PrefabRegistry Instance
        {
            get
            {
                if (!looked) { cached = Resources.Load<PrefabRegistry>("PrefabRegistry"); looked = true; }
                return cached;
            }
        }

        public static GameObject Get(string id)
        {
            var reg = Instance;
            if (reg == null || !reg.usePrefabs) return null;
            foreach (var e in reg.entries) if (e.id == id) return e.prefab;
            return null;
        }

        public void Set(string id, GameObject prefab)
        {
            foreach (var e in entries) if (e.id == id) { e.prefab = prefab; return; }
            entries.Add(new Entry { id = id, prefab = prefab });
        }
    }
}
