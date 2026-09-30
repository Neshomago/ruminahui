// Implements: 06 Enemy Roster, Atoc's Lieutenant (mini-boss, M3.3) — HP High / Dmg Med-High / Speed Med; mix of Shield-Bearer
// defence and Skirmisher ranged pressure; tell: telegraphs a 3-hit UNBLOCKABLE string with a distinct wind-up roar.
using System.Collections;
using UnityEngine;

namespace Ruminahui
{
    public class AtocLieutenant : EnemyBrain
    {
        public float meleeRange = 2.4f;
        public float slingRange = 7f;
        public int actionsBetweenStrings = 3;    // PLACEHOLDER-BALANCE
        int actionCount;

        protected override void Awake()
        {
            base.Awake();
            flinchOnHit = false;
        }

        protected override void OnReset() => actionCount = 0;

        protected override IEnumerator Behaviour()
        {
            while (true)
            {
                if (!AcquireTarget()) { State = EnemyState.Idle; Stop(); yield return Wait(0.5f); continue; }
                float d = DistanceToTarget;

                if (actionCount >= actionsBetweenStrings && d < 6f)
                {
                    actionCount = 0;
                    yield return UnblockableString();
                    continue;
                }

                if (d > slingRange)
                {
                    actionCount++;
                    yield return Telegraph(0.7f, TellColor);
                    if (Target != null) FireAt(Target, EnemyStats.DmgMed, 20f, new Color(0.5f, 0.3f, 0.6f), HitFlags.Ranged, "Lieutenant sling");
                    State = EnemyState.Recover;
                    yield return Wait(0.8f);
                    continue;
                }

                if (d > meleeRange)
                {
                    State = EnemyState.Chase;
                    MoveTowards(Target.transform.position);
                    yield return null;
                    continue;
                }

                actionCount++;
                yield return Telegraph(0.5f, TellColor);
                Lunge(0.5f);
                MeleeStrike(EnemyStats.DmgMedHigh, meleeRange + 0.3f, 60f, HitFlags.None, 0f, 2f, "Lieutenant strike");
                State = EnemyState.Recover;
                yield return Wait(0.9f);
            }
        }

        IEnumerator UnblockableString()
        {
            superArmor = true;
            AudioPool.Instance?.PlayCue(PlaceholderCue.Roar, self.AimPoint, 0.7f);
            yield return Telegraph(1.0f, UnblockableColor, null);
            for (int i = 0; i < 3; i++)
            {
                if (Target != null) motor.FaceTowards(Target.transform.position, true);
                Lunge(1.2f);
                MeleeStrike(EnemyStats.DmgMedHigh - 2f, 2.6f, 70f, HitFlags.Unblockable, 0.3f, 3f, $"Unblockable string {i + 1}/3");
                yield return Wait(0.35f);
            }
            superArmor = false;
            State = EnemyState.Recover;
            yield return Wait(1.2f); // punish window
        }

        protected override void OnBehaviourStopped() => superArmor = false;
    }
}
