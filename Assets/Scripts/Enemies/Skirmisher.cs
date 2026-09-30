// Implements: 06 Enemy Roster, Skirmisher — HP Low / Dmg Low / Speed High; harasses from range with slings, retreats when
// approached; tell: visible wind-up before a sling throw.
using System.Collections;
using UnityEngine;

namespace Ruminahui
{
    public class Skirmisher : EnemyBrain
    {
        public float preferredMin = 7f;      // PLACEHOLDER-BALANCE
        public float preferredMax = 13f;
        public float retreatDistance = 5f;
        public float windup = 0.8f;
        public float recovery = 1.2f;
        public float slingSpeed = 18f;

        protected override IEnumerator Behaviour()
        {
            float strafeSign = Random.value < 0.5f ? -1f : 1f;
            while (true)
            {
                if (!AcquireTarget()) { State = EnemyState.Idle; Stop(); yield return Wait(0.5f); continue; }
                float d = DistanceToTarget;

                if (d < retreatDistance)
                {
                    State = EnemyState.Reposition;
                    float t = 0f;
                    while (t < 1.2f && DistanceToTarget < preferredMin && Target != null)
                    {
                        MoveAwayFrom(Target.transform.position);
                        t += Dt;
                        yield return null;
                    }
                    continue;
                }

                if (d > preferredMax)
                {
                    State = EnemyState.Chase;
                    MoveTowards(Target.transform.position);
                    yield return null;
                    continue;
                }

                // In the band: wind up and sling.
                yield return Telegraph(windup, TellColor);
                if (Target != null) FireAt(Target, EnemyStats.DmgLow, slingSpeed, new Color(0.6f, 0.5f, 0.4f), HitFlags.Ranged, "Sling");
                State = EnemyState.Recover;
                float r = 0f;
                while (r < recovery && Target != null)
                {
                    Strafe(Target.transform.position, strafeSign);
                    r += Dt;
                    yield return null;
                }
                if (Random.value < 0.3f) strafeSign = -strafeSign;
            }
        }
    }
}
