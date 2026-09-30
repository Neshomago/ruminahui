// Implements: AI_BUILD_PROMPT.md step 1 ("placeholder training-dummy target → playable Puma combat loop") and 02 M0.2/M1.1/M4.2.
// Never dies (HP floors at 1 and refills after a pause). Practice mode swings on a telegraph so parries/dodges can be drilled.
using System.Collections;
using UnityEngine;

namespace Ruminahui
{
    public class TrainingDummy : EnemyBrain
    {
        public bool practiceSwings = true;
        public float swingInterval = 3f;      // PLACEHOLDER-BALANCE
        public float resetDelay = 3f;
        float lastHitAt;

        protected override void Awake()
        {
            base.Awake();
            health.minimumHp = 1f;
            flinchOnHit = false;
        }

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

        void OnHit(DamageInfo info, HitResult r) => lastHitAt = Time.time;

        protected override void Update()
        {
            base.Update();
            if (health.Current < health.max && Time.time - lastHitAt > resetDelay) health.ResetFull();
        }

        protected override IEnumerator Behaviour()
        {
            while (true)
            {
                State = EnemyState.Idle;
                yield return Wait(swingInterval);
                if (!practiceSwings || !AcquireTarget() || DistanceToTarget > 3f) continue;
                yield return Telegraph(0.6f, TellColor);
                MeleeStrike(5f, 2.6f, 70f, HitFlags.None, 0f, 0f, "Dummy swing");
                yield return Wait(0.4f);
            }
        }
    }
}
