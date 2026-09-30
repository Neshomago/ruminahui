// Implements: 06-m5-3-flow-and-enemy-roster.md, M5.3 Section 3 — "No fail state": a failed input is a recoverable stumble, never a reload.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ruminahui
{
    public class SoftFailVolume : MonoBehaviour
    {
        public Vector3 size = new Vector3(10f, 2f, 10f);
        public Transform safePoint;

        void Update()
        {
            var pm = PartyManager.Instance;
            if (pm == null) return;
            var b = new Bounds(transform.position, size);
            foreach (var m in pm.Members)
            {
                if (m == null || !b.Contains(m.transform.position)) continue;
                if (m.IsPlayerControlled)
                {
                    ObjectiveTracker.Say(m.displayName, "(stumbles — climbs back to the ledge)");
                    if (safePoint != null) m.Motor.Teleport(safePoint.position, safePoint.rotation);
                }
                else if (pm.Controlled != null)
                {
                    m.Motor.Teleport(pm.Controlled.transform.position - pm.Controlled.transform.forward * 1.5f);
                }
            }
        }
    }
}
