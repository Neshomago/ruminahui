// Implements: 06 Enemy Roster — Shield-Bearer ("only exposed after a guard-break"; tell: "shield lowers briefly after 2-3
// blocked hits"), armored variant (M2.1), Spanish Infantry ("higher guard-break threshold") and the Lieutenant's shield half.
using UnityEngine;

namespace Ruminahui
{
    public class ShieldGuard : MonoBehaviour, IHitInterceptor
    {
        [Tooltip("Guard-break power needed to break the shield (Warclub Crush = 1, +1 full charge, +1 Weight tier).")]
        public int guardBreakThreshold = 1;
        public float frontalHalfAngle = 70f;
        public int minBlocksBeforeDrop = 2;
        public int maxBlocksBeforeDrop = 3;
        public float lowerDuration = 1.2f;       // PLACEHOLDER-BALANCE: "shield lowers briefly"
        public float breakStagger = 1.5f;        // PLACEHOLDER-BALANCE
        public float brokenOpenDuration = 2f;    // PLACEHOLDER-BALANCE
        [Tooltip("Officer rally adds to the threshold while buffed.")]
        public int bonusThreshold;
        public Transform shieldVisual;

        public int InterceptPriority => 80;
        public bool IsLowered => Time.time < loweredUntil;

        CombatTarget self;
        PlaceholderVisual visual;
        int blocksUntilDrop;
        int accumulatedPower;
        float loweredUntil;

        void Awake()
        {
            self = GetComponent<CombatTarget>();
            visual = GetComponent<PlaceholderVisual>();
            ResetGuard();
        }

        void OnEnable() => self.RegisterInterceptor(this);
        void OnDisable() => self.UnregisterInterceptor(this);

        public void ResetGuard()
        {
            blocksUntilDrop = Random.Range(minBlocksBeforeDrop, maxBlocksBeforeDrop + 1);
            accumulatedPower = 0;
            loweredUntil = 0f;
            bonusThreshold = 0;
        }

        public bool Intercept(ref DamageInfo info, out HitResult result)
        {
            result = default;
            if (IsLowered || self.Status.IsIncapacitated) return false;
            if (info.Has(HitFlags.Unblockable) || info.Has(HitFlags.Grab) || info.Has(HitFlags.Poison)) return false;
            if (info.Attacker != null && !self.IsInFront(info.Attacker.transform.position, frontalHalfAngle)) return false;

            if (info.Has(HitFlags.GuardBreak))
            {
                accumulatedPower += Mathf.Max(1, info.GuardBreakPower);
                if (accumulatedPower >= guardBreakThreshold + bonusThreshold)
                {
                    // Guard broken: the hit lands and opens them up.
                    accumulatedPower = 0;
                    loweredUntil = Time.time + brokenOpenDuration;
                    info.StaggerDuration = Mathf.Max(info.StaggerDuration, breakStagger);
                    if (visual != null) visual.Flash(Color.white, 0.2f, false);
                    return false;
                }
            }

            // Blocked.
            AudioPool.Instance?.PlayCue(PlaceholderCue.Block, self.AimPoint, 0.3f);
            blocksUntilDrop--;
            if (blocksUntilDrop <= 0)
            {
                // The tell: shield lowers briefly.
                loweredUntil = Time.time + lowerDuration;
                blocksUntilDrop = Random.Range(minBlocksBeforeDrop, maxBlocksBeforeDrop + 1);
                if (visual != null) visual.Flash(new Color(0.2f, 1f, 1f), lowerDuration);
            }
            result = new HitResult(HitOutcome.Blocked);
            return true;
        }

        void Update()
        {
            if (shieldVisual == null) return;
            // Placeholder "animation": rotate the shield down while lowered.
            var target = IsLowered ? Quaternion.Euler(70f, 0f, 0f) : Quaternion.identity;
            shieldVisual.localRotation = Quaternion.RotateTowards(shieldVisual.localRotation, target, 400f * Time.deltaTime);
        }
    }
}
