// Implements: 03-combat-design.md Section 4 "call-in a dodge-assist" (approved as "Cover"): for a short window the first enemy hit
// that would land on the player is turned into an evade — the ally pulls them clear. Counts as a perfect dodge for Focus.
using UnityEngine;

namespace Ruminahui
{
    [RequireComponent(typeof(CombatTarget))]
    public class CoverAssist : MonoBehaviour, IHitInterceptor
    {
        public const float WindowSeconds = 3f;      // PLACEHOLDER-BALANCE
        public const float PullDistance = 2.2f;     // PLACEHOLDER-BALANCE

        /// <summary>Runs before kit i-frames/blocks (100/90/80): the ally acts before you do.</summary>
        public int InterceptPriority => 110;

        CombatTarget self;
        CharacterMotor motor;
        float armedUntil = -1f;
        string allyName = "Ally";

        public bool IsArmed => Time.time < armedUntil;
        public float Remaining => Mathf.Max(0f, armedUntil - Time.time);

        void Awake()
        {
            self = GetComponent<CombatTarget>();
            motor = GetComponent<CharacterMotor>();
        }

        void OnEnable() => self.RegisterInterceptor(this);
        void OnDisable() { self.UnregisterInterceptor(this); armedUntil = -1f; }

        public void Arm(string ally)
        {
            allyName = ally;
            armedUntil = Time.time + WindowSeconds;
        }

        public void Disarm() => armedUntil = -1f;

        /// <summary>Pure decision (unit-tested): should this hit be covered?</summary>
        public static bool ShouldCover(bool armed, DamageInfo info, Faction defender)
        {
            if (!armed || info.Attacker == null) return false;
            if (info.Attacker.isDecoy) return false;
            return FactionUtil.AreHostile(info.Attacker.faction, defender) && info.Amount > 0f;
        }

        public bool Intercept(ref DamageInfo info, out HitResult result)
        {
            result = default;
            if (!ShouldCover(IsArmed, info, self.faction)) return false;

            armedUntil = -1f; // one hit per call
            if (motor != null)
            {
                var away = Vector3.Cross(Vector3.up, info.Direction);
                if (away.sqrMagnitude < 0.01f) away = -transform.forward;
                motor.Dash(away.normalized, PullDistance, 0.15f);
            }
            if (PartyManager.Instance != null) PartyManager.Instance.Focus.Add(FocusRules.PerfectDodge, $"Cover ({allyName})");
            ObjectiveTracker.Say(allyName, "Got you!");
            AudioPool.Instance?.PlayCue(PlaceholderCue.Parry, self.AimPoint, 0.4f);
            result = new HitResult(HitOutcome.Evaded);
            return true;
        }
    }
}
