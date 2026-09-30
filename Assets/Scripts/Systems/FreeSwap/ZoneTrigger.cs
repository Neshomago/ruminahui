// Implements: polled trigger zone (no physics triggers/rigidbodies needed) — used for mission goals and M5.3 regroup.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ruminahui
{
    public class ZoneTrigger : MonoBehaviour
    {
        public Vector3 size = new Vector3(4f, 3f, 4f);
        public bool requireWholeParty;
        [Tooltip("Only fires while armed — directors arm a zone when its stage begins, so it can't fire unheard.")]
        public bool armed = true;
        public bool fired;
        public event System.Action Entered;

        public bool Contains(Vector3 p) => new Bounds(transform.position, size).Contains(p);

        void Update()
        {
            if (fired || !armed) return;
            var pm = PartyManager.Instance;
            if (pm == null || pm.Controlled == null) return;
            bool ok;
            if (requireWholeParty)
            {
                ok = pm.Members.Count > 0;
                foreach (var m in pm.Members) if (m != null && !Contains(m.transform.position)) { ok = false; break; }
            }
            else ok = Contains(pm.Controlled.transform.position);
            if (!ok) return;
            fired = true;
            Entered?.Invoke();
        }
    }
}
