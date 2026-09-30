// Implements: 06 Enemy Roster, Shield-Bearer — HP Med / Dmg Med / Speed Low; advances slowly behind a full shield, only
// exposed after a guard-break; tell: shield lowers after 2-3 blocked hits. Armored variant (M2.1) uses a higher threshold.
// Also reused for Spanish Infantry (see SpanishInfantry.cs).
using System.Collections;
using UnityEngine;

namespace Ruminahui
{
    public class ShieldBearer : EnemyBrain
    {
        public float attackRange = 2.3f;
        public float windup = 0.6f;          // PLACEHOLDER-BALANCE
        public float recovery = 1.0f;
        public float damage = EnemyStats.DmgMed;
        public float advanceSpeedScale = 1f;

        protected ShieldGuard guard;

        protected override void Awake()
        {
            base.Awake();
            guard = GetComponent<ShieldGuard>();
            flinchOnHit = false; // the shield is the point
        }

        protected override void OnReset()
        {
            if (guard != null) guard.ResetGuard();
        }

        protected virtual Vector3 ApproachPoint() => Target.transform.position;

        protected override IEnumerator Behaviour()
        {
            while (true)
            {
                if (!AcquireTarget()) { State = EnemyState.Idle; Stop(); yield return Wait(0.5f); continue; }

                if (DistanceToTarget > attackRange)
                {
                    State = EnemyState.Chase;
                    MoveTowards(ApproachPoint(), advanceSpeedScale);
                    motor.FaceTowards(Target.transform.position);
                    yield return null;
                    continue;
                }

                if (guard != null && guard.IsLowered) { Stop(); yield return null; continue; } // exposed — no attacks

                yield return Telegraph(windup, TellColor);
                Lunge(0.5f);
                MeleeStrike(damage, attackRange + 0.3f, 50f, HitFlags.None, 0f, 2f, "Spear thrust");
                State = EnemyState.Recover;
                yield return Wait(recovery);
            }
        }
    }
}
