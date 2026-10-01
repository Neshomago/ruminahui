// Implements: 02 M1.1 "Two Prodigies" — "First real combat tutorial framed as a training duel he loses… humility built into the
// mechanics lesson." Approved: Chaska's HP can't drop below half, and the bout ends in a scripted loss when your HP gets low.
// Faster, sharper rival: quick 3-hit flurries, Skywalk-style backsteps, the occasional bolas.
using System.Collections;
using UnityEngine;

namespace Ruminahui
{
    public class SparringChaska : EnemyBrain
    {
        public float floorFraction = 0.5f;     // approved: she can't be beaten
        public float flurryDamage = 5f;        // PLACEHOLDER-BALANCE: sparring, not lethal
        public float tell = 0.32f;             // quick, but readable (teaches parry/dodge timing)

        protected override void Awake()
        {
            base.Awake();
            flinchOnHit = false;
            health.minimumHp = health.max * floorFraction;
        }

        protected override IEnumerator Behaviour()
        {
            while (true)
            {
                if (!AcquireTarget()) { Stop(); yield return Wait(0.3f); continue; }
                float d = DistanceToTarget;
                if (d > 6f && Random.value < 0.35f)
                {
                    yield return Telegraph(0.45f, TellColor);
                    if (Target != null) FireAt(Target, flurryDamage * 0.6f, 20f, new Color(0.6f, 0.8f, 1f), HitFlags.Ranged, "Sparring bolas");
                    yield return Wait(0.6f);
                    continue;
                }
                if (d > 2f)
                {
                    State = EnemyState.Chase;
                    MoveTowards(Target.transform.position, 1f);
                    yield return null;
                    continue;
                }
                yield return Telegraph(tell, TellColor);
                for (int i = 0; i < 3; i++)
                {
                    if (Target != null) motor.FaceTowards(Target.transform.position, true);
                    Lunge(0.6f);
                    MeleeStrike(flurryDamage, 2.3f, 60f, i == 2 ? HitFlags.Stagger : HitFlags.None, i == 2 ? 0.4f : 0f, 1.5f, $"Chaska flurry {i + 1}/3");
                    yield return Wait(0.18f);
                }
                State = EnemyState.Recover;
                yield return Wait(0.5f);
                if (Random.value < 0.5f && Target != null) motor.Dash(transform.position - Target.transform.position, 4f, 0.25f); // backstep
                yield return Wait(0.6f);
            }
        }

        /// <summary>The director ramps her up to force the loss once the lesson is learned.</summary>
        public void Press(float scale) => buffDamageScale = scale;
    }
}
