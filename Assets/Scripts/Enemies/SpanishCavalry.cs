// Implements: 06 Enemy Roster, Spanish Cavalry (M5.1) — HP Med / Dmg High (charge) / Speed Very High; charges in straight
// lines, vulnerable during recovery after a missed charge; long telegraphed wind-up; punishes blocking; narrow terrain
// removes their charge lanes.
using System.Collections;
using UnityEngine;

namespace Ruminahui
{
    public class SpanishCavalry : EnemyBrain
    {
        public float circleDistance = 13f;       // PLACEHOLDER-BALANCE
        public float chargeWindup = 1.2f;        // "long, telegraphed"
        public float chargeSpeed = 16f;
        public float chargeMaxDuration = 1.6f;
        public float chargeHitRadius = 1.4f;
        public float recoveryVulnerable = 2.2f;
        public float minClearLane = 6f;

        protected override void Awake()
        {
            base.Awake();
            flinchOnHit = false;
        }

        protected override IEnumerator Behaviour()
        {
            float sign = Random.value < 0.5f ? 1f : -1f;
            while (true)
            {
                if (!AcquireTarget()) { State = EnemyState.Idle; Stop(); yield return Wait(0.5f); continue; }

                // Reposition to charging distance.
                State = EnemyState.Reposition;
                float t = 0f;
                while (t < 2.5f && Target != null)
                {
                    float d = DistanceToTarget;
                    if (d < circleDistance - 2f) MoveAwayFrom(Target.transform.position, 0.6f);
                    else if (d > circleDistance + 3f) MoveTowards(Target.transform.position, 0.6f);
                    else Strafe(Target.transform.position, sign, 0.5f);
                    t += Dt;
                    yield return null;
                }
                if (Target == null) continue;

                // Narrow terrain: no lane, no charge.
                var lane = Target.transform.position - transform.position;
                lane.y = 0f;
                if (Physics.SphereCast(self.AimPoint, 0.8f, lane.normalized, out var hit, Mathf.Min(lane.magnitude, minClearLane), ~0, QueryTriggerInteraction.Ignore)
                    && hit.collider.GetComponentInParent<CombatTarget>() == null)
                {
                    sign = -sign;
                    continue;
                }

                yield return Telegraph(chargeWindup, TellColor);
                yield return Charge();
            }
        }

        IEnumerator Charge()
        {
            State = EnemyState.Attack;
            superArmor = true;
            var dir = transform.forward;
            float t = 0f;
            bool hitSomething = false;
            var alreadyHit = new System.Collections.Generic.List<CombatTarget>();
            while (t < chargeMaxDuration)
            {
                float dt = Dt;
                t += dt;
                motor.Controller.Move(dir * chargeSpeed * dt);
                // Wall ahead → the charge ends in a crash.
                if (Physics.Raycast(self.AimPoint, dir, out var wall, 1.5f, ~0, QueryTriggerInteraction.Ignore)
                    && wall.collider.GetComponentInParent<CombatTarget>() == null)
                {
                    status.ApplyStagger(1.5f);
                    break;
                }
                foreach (var c in CombatQuery.Sphere(self, transform.position, chargeHitRadius))
                {
                    if (alreadyHit.Contains(c)) continue;
                    alreadyHit.Add(c);
                    c.ReceiveHit(new DamageInfo
                    {
                        Amount = (EnemyStats.DmgHigh + 8f) * DamageScale, Attacker = self, Point = c.AimPoint, Direction = dir,
                        Flags = HitFlags.Charge | HitFlags.Knockdown | HitFlags.Heavy, StaggerDuration = 1.2f, Knockback = 10f,
                        AttackName = "Cavalry charge",
                    });
                    hitSomething = true;
                }
                yield return null;
            }
            superArmor = false;

            // Recovery: vulnerable, more so after a miss.
            State = EnemyState.Recover;
            self.incomingDamageMultiplier = hitSomething ? 1.2f : 1.5f;
            yield return Wait(recoveryVulnerable);
            self.incomingDamageMultiplier = 1f;
        }

        protected override void OnBehaviourStopped()
        {
            superArmor = false;
            self.incomingDamageMultiplier = 1f;
        }
    }
}
