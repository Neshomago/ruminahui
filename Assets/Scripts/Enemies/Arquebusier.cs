// Implements: 06 Enemy Roster, Firearm-User (Arquebusier, M5.2) — HP Low / Dmg Very High (single hit) / Speed Low; stationary,
// long reload; tell: visible match-lock smoke/flare a beat before firing; decoys bait a shot during reload downtime.
// "Standing still or holding a block is actively wrong" → the shot is unblockable; dodge i-frames or decoys beat it.
using System.Collections;
using UnityEngine;

namespace Ruminahui
{
    public class Arquebusier : EnemyBrain
    {
        public float range = 30f;
        public float aimTell = 1.0f;         // PLACEHOLDER-BALANCE: "a beat before firing"
        public float reload = 5f;            // PLACEHOLDER-BALANCE: "long reload"
        public float shotSpeed = 70f;
        public float backOffDistance = 4f;

        protected override void Awake()
        {
            base.Awake();
            aggroRange = range;
        }

        protected override IEnumerator Behaviour()
        {
            while (true)
            {
                if (!AcquireTarget()) { State = EnemyState.Idle; Stop(); yield return Wait(0.5f); continue; }
                if (DistanceToTarget > range) { Stop(); yield return Wait(0.3f); continue; }

                // Flare tell (smoke puff + flash + crack).
                if (PoolManager.Instance != null)
                    PoolManager.Instance.Spawn(CombatPools.HitSparkKey, self.AimPoint + transform.forward * 0.7f + Vector3.up * 0.3f, Quaternion.identity);
                AudioPool.Instance?.PlayCue(PlaceholderCue.Flare, self.AimPoint, 0.6f);
                yield return Telegraph(aimTell, new Color(1f, 1f, 0.3f), null);

                // Re-pick at the trigger pull: a decoy that appeared during the tell takes the shot.
                var decoy = CombatQuery.Nearest(self, transform.position, range, t => t.isDecoy);
                var shotTarget = decoy != null ? decoy : Target;
                if (shotTarget != null)
                    FireAt(shotTarget, EnemyStats.DmgVeryHigh, shotSpeed, new Color(0.2f, 0.2f, 0.2f),
                        HitFlags.Ranged | HitFlags.Unblockable | HitFlags.Knockdown, "Arquebus shot", 0.15f);

                // Long reload: slow back-off if pressed, otherwise stand.
                State = EnemyState.Recover;
                float t = 0f;
                while (t < reload)
                {
                    if (Target != null && DistanceToTarget < backOffDistance) MoveAwayFrom(Target.transform.position, 0.4f);
                    else Stop();
                    t += Dt;
                    yield return null;
                }
            }
        }
    }
}
