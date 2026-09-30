// Implements: 06 Enemy Roster, Spanish Officer (mini-boss, M5.4) — HP High / Dmg High / Speed Med; buffs nearby infantry while
// alive; tell: raised-sword "rally" animation, INTERRUPTIBLE if punished quickly; rewards target prioritisation.
using System.Collections;
using UnityEngine;

namespace Ruminahui
{
    public class SpanishOfficer : EnemyBrain
    {
        public float rallyInterval = 12f;     // PLACEHOLDER-BALANCE
        public float rallyTell = 1.5f;
        public float rallyRadius = 10f;
        public float rallyBuff = 1.3f;
        public float rallyDuration = 10f;
        public float meleeRange = 2.3f;

        float nextRally;
        bool rallying;
        bool rallyInterrupted;

        protected override void OnEnable()
        {
            base.OnEnable();
            self.HitReceived += OnHit;
        }

        protected override void OnDisable()
        {
            self.HitReceived -= OnHit;
            base.OnDisable();
        }

        protected override void OnReset()
        {
            nextRally = Time.time + 4f;
            rallying = false;
        }

        void OnHit(DamageInfo info, HitResult r)
        {
            if (rallying && r.Landed) rallyInterrupted = true;
        }

        protected override IEnumerator Behaviour()
        {
            while (true)
            {
                if (!AcquireTarget()) { State = EnemyState.Idle; Stop(); yield return Wait(0.5f); continue; }

                if (Time.time >= nextRally && AlliesNearby() > 0)
                {
                    nextRally = Time.time + rallyInterval;
                    yield return Rally();
                    continue;
                }

                if (DistanceToTarget > meleeRange)
                {
                    State = EnemyState.Chase;
                    MoveTowards(Target.transform.position);
                    yield return null;
                    continue;
                }

                for (int i = 0; i < 2; i++)
                {
                    yield return Telegraph(0.4f, TellColor);
                    Lunge(0.5f);
                    MeleeStrike(EnemyStats.DmgHigh, meleeRange + 0.3f, 60f, HitFlags.None, 0f, 2f, $"Officer sword {i + 1}/2");
                    yield return Wait(0.3f);
                }
                State = EnemyState.Recover;
                yield return Wait(1f);
            }
        }

        IEnumerator Rally()
        {
            rallying = true;
            rallyInterrupted = false;
            AudioPool.Instance?.PlayCue(PlaceholderCue.Rally, self.AimPoint, 0.7f);
            if (visual != null) visual.SetPoseScale(new Vector3(1f, 1.15f, 1f)); // sword raised
            State = EnemyState.Telegraph;
            if (visual != null) visual.Flash(new Color(1f, 0.85f, 0.2f), rallyTell);
            float t = 0f;
            while (t < rallyTell && !rallyInterrupted) { Stop(); t += Dt; yield return null; }
            if (visual != null) visual.ResetPose();
            rallying = false;

            if (rallyInterrupted)
            {
                ObjectiveTracker.Say("Officer", "(rally interrupted)");
                status.ApplyStagger(1f);
                yield break;
            }

            foreach (var e in EnemyBrain.Active)
            {
                if (e == this || e.IsDead) continue;
                if (Vector3.Distance(e.transform.position, transform.position) > rallyRadius) continue;
                e.ApplyBuff(rallyBuff, rallyDuration);
                var g = e.GetComponent<ShieldGuard>();
                if (g != null) g.bonusThreshold = 1;
            }
            ObjectiveTracker.Say("Officer", "(rallies the troops — nearby enemies buffed)");
        }

        protected override void OnBehaviourStopped() => rallying = false;

        int AlliesNearby()
        {
            int n = 0;
            foreach (var e in EnemyBrain.Active)
                if (e != this && !e.IsDead && Vector3.Distance(e.transform.position, transform.position) <= rallyRadius) n++;
            return n;
        }
    }
}
