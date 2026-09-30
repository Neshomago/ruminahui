// Implements: 06 Enemy Roster, Spanish Infantry (M5.1) — HP Med / Dmg Med / Speed Low-Med; tighter formation fighting than
// Andean Shield-Bearers; steel armour = higher guard-break threshold; tell: SHORTER, sharper wind-ups.
using UnityEngine;

namespace Ruminahui
{
    public class SpanishInfantry : ShieldBearer
    {
        public FormationGroup formation;

        protected override void Awake()
        {
            base.Awake();
            windup = 0.35f;     // PLACEHOLDER-BALANCE: shorter than Andean 0.6
            recovery = 0.8f;
        }

        protected override Vector3 ApproachPoint()
        {
            if (formation == null) return Target.transform.position;
            return formation.SlotFor(this, Target.transform.position);
        }
    }

}
