// Implements: 03-combat-design.md, Section 2 (Kuntur Kit — Chaska): Twin-Blade Flurry x4, Bolas Snap (ranged wrap + melee
// finisher bonus), Skywalk (long, chainable dash), Falling Star (aerial string + ground finisher), Condor's Eye (mark, no cooldown),
// Specials (Vantage Leap, Sky-Cut, Star-Fall), Ultimate (The Gap in the Line) and the Upgrade Tree (Foundation / Height / Precision).
// Also 11-camera-performance.md Part A Step 3.3: Condor's Eye shares the lock-on input.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ruminahui
{
    public class KunturKit : CombatKit, IHitInterceptor
    {
        public override KitId Kit => KitId.Kuntur;

        [Header("Skywalk")]
        public float dodgeDistanceBase = 4.5f;   // PLACEHOLDER-BALANCE: longer than Root-Step (2.6)
        public float dodgeDuration = 0.25f;
        public float dodgeIFrames = 0.22f;
        public float dodgeStamina = 16f;
        public float chainStaminaExtra = 10f;    // "chained into a second dodge at STA cost"
        public float chainWindow = 0.6f;

        [Header("Bolas Snap")]
        public float bolasSpeed = 22f;
        public float bolasRange = 16f;
        public float bolasWrapDuration = 2.5f;
        public float bolasStamina = 12f;
        public float wrappedFinisherBonus = 1.6f;

        [Header("Condor's Eye")]
        public float markRangeBase = 18f;
        public float markDuration = 12f;
        public float markBonus = 1.35f;          // "bonus damage" on the highlighted weak point

        [Header("Specials")]
        public float vantageRange = 14f;
        public float skyCutDamage = 16f;
        public float skyCutRange = 25f;
        public float starFallThresholdBase = 0.25f;
        public float gapSlowScale = 0.25f;
        public float gapDurationBase = 6f;

        public int InterceptPriority => 90;

        AttackData[] chain;
        AttackData[] airString;
        AttackData airFinisher;
        float iFramesUntil;
        bool perfectDodgeAwarded;
        int dodgeChainCount;
        float lastDodgeAt = -10f;
        readonly List<CombatTarget> marked = new List<CombatTarget>();

        float MarkRange => markRangeBase + (Tier >= 1 ? 6f : 0f);
        int MaxDodgeChain => Tier >= 1 ? 3 : 2;
        float DodgeDistance => dodgeDistanceBase + (Tier >= 1 ? 1f : 0f);
        float StarFallThreshold => starFallThresholdBase + (Tier >= 3 ? 0.1f : 0f);
        float GapDuration => gapDurationBase + (Tier >= 3 ? 2f : 0f);

        public IReadOnlyList<CombatTarget> Marked => marked;

        protected override AttackData[] LightChain => chain;

        protected override void Awake()
        {
            // PLACEHOLDER-BALANCE: faster, lower per-hit damage than Puma; rewards sustained combos.
            chain = new[]
            {
                new AttackData("Twin-Blade I", 7f, 2.0f, 0.07f, 0.14f, lunge: 0.8f),
                new AttackData("Twin-Blade II", 7f, 2.0f, 0.07f, 0.14f, lunge: 0.8f),
                new AttackData("Twin-Blade III", 8f, 2.0f, 0.08f, 0.16f, lunge: 0.8f),
                new AttackData("Twin-Blade IV", 11f, 2.2f, 0.1f, 0.3f, HitFlags.Stagger, stagger: 0.6f, lunge: 1f),
            };
            airString = new[]
            {
                new AttackData("Falling Star I", 8f, 2.4f, 0.06f, 0.12f, halfAngle: 80f, lunge: 0f),
                new AttackData("Falling Star II", 8f, 2.4f, 0.06f, 0.12f, halfAngle: 80f, lunge: 0f),
                new AttackData("Falling Star III", 9f, 2.4f, 0.06f, 0.12f, halfAngle: 80f, lunge: 0f),
            };
            airFinisher = new AttackData("Falling Star (finisher)", 18f, 2.6f, 0f, 0.35f, HitFlags.Stagger | HitFlags.Heavy,
                stagger: 1f, halfAngle: 180f, knockback: 5f, lunge: 0f);
            base.Awake();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            self.RegisterInterceptor(this);
        }

        protected override void OnDisable()
        {
            self.UnregisterInterceptor(this);
            base.OnDisable();
        }

        protected override void BuildAbilities()
        {
            Specials.Clear();
            // PLACEHOLDER-BALANCE: costs/cooldowns
            Specials.Add(new AbilitySlot("vantageleap", "Vantage Leap", 15f, 6f, Unlocks.VantageLeap));
            Specials.Add(new AbilitySlot("skycut", "Sky-Cut", 30f, 8f, Unlocks.SkyCut));
            Specials.Add(new AbilitySlot("starfall", "Star-Fall", 35f, 5f, Unlocks.StarFall));
            Ultimate = new AbilitySlot("gapintheline", "The Gap in the Line", 100f, 60f, Unlocks.GapInTheLine, true);
        }

        void Update()
        {
            Specials[0].Cooldown = Tier >= 2 ? 4f : 6f; // Height tier: Vantage Leap cooldown
            marked.RemoveAll(t => t == null || !t.IsAlive || !t.Status.IsMarked);
        }

        // ───────── Light: Twin-Blade Flurry, or Falling Star if airborne ─────────
        protected override void OnLight()
        {
            if (!motor.IsGrounded && !status.IsIncapacitated)
            {
                StartAction(FallingStar(), "Falling Star");
                return;
            }
            base.OnLight();
        }

        protected override void ModifyLightHit(CombatTarget target, ref DamageInfo info)
        {
            // Bolas Snap follow-up: melee finisher on a wrapped target deals bonus damage and frees it.
            if (target.Status.IsWrapped)
            {
                info.Amount *= wrappedFinisherBonus;
                info.AttackName += " (Bolas finisher)";
                target.Status.ConsumeWrap();
            }
        }

        IEnumerator FallingStar()
        {
            motor.movementLocked = true;
            motor.gravityEnabled = false;
            motor.VerticalVelocity = 0f;
            FaceAim();
            int hits = Tier >= 2 ? airString.Length : airString.Length - 1; // Height tier: aerial combo extension
            for (int i = 0; i < hits; i++)
            {
                yield return Wait(airString[i].windup);
                MeleeHit(airString[i], 1f, ModifyLightHit);
                yield return Wait(airString[i].recovery);
            }
            // Dive to the ground finisher.
            motor.gravityEnabled = true;
            motor.VerticalVelocity = -22f;
            float guard = 0f;
            while (!motor.IsGrounded && guard < 2f) { guard += motor.Delta; yield return null; }
            foreach (var t in CombatQuery.Sphere(self, transform.position, airFinisher.range))
            {
                t.ReceiveHit(BuildDamage(airFinisher, t, 1f));
                CombatPools.Spark(t.AimPoint);
            }
            yield return Wait(airFinisher.recovery);
        }

        protected override void OnActionCancelled() => motor.gravityEnabled = true;

        // ───────── Heavy: Bolas Snap ─────────
        public override void CmdHeavyPressed()
        {
            if (IsBusy || status.IsIncapacitated) return;
            if (!stamina.TrySpend(bolasStamina)) return;
            StartAction(BolasSnap(), "Bolas Snap");
        }

        IEnumerator BolasSnap()
        {
            motor.movementLocked = true;
            var target = CurrentTarget(bolasRange);
            if (target != null) motor.FaceTowards(target.transform.position, true);
            yield return Wait(0.12f);
            var dir = target != null ? (target.AimPoint - self.AimPoint) : transform.forward;
            var payload = new DamageInfo { Amount = 6f * damageMultiplier, Flags = HitFlags.Ranged, AttackName = "Bolas Snap" };
            Projectile.Fire(self, self.AimPoint + transform.forward * 0.5f, dir, bolasSpeed, payload, new Color(0.6f, 0.8f, 1f), target,
                (t, r) => { if (r.Landed || r.Outcome == HitOutcome.Blocked) t.Status.ApplyWrap(bolasWrapDuration); });
            yield return Wait(0.35f);
        }

        // ───────── Dodge: Skywalk (chainable) ─────────
        public override void CmdDodge(Vector3 dir)
        {
            bool chaining = Time.time - lastDodgeAt <= chainWindow && dodgeChainCount < MaxDodgeChain;
            if (IsBusy && !(chaining && CurrentActionName == "Skywalk")) return;
            if (status.IsIncapacitated) return;
            if (!chaining) dodgeChainCount = 0;
            float cost = dodgeStamina + (dodgeChainCount > 0 ? chainStaminaExtra : 0f);
            if (!stamina.TrySpend(cost)) return;
            if (IsBusy) CancelAction();
            dodgeChainCount++;
            lastDodgeAt = Time.time;
            StartAction(Skywalk(dir), "Skywalk");
        }

        IEnumerator Skywalk(Vector3 dir)
        {
            iFramesUntil = Time.time + dodgeIFrames;
            perfectDodgeAwarded = false;
            motor.Dash(dir, DodgeDistance, dodgeDuration);
            yield return Wait(dodgeDuration + 0.05f);
        }

        // ───────── Mark: Condor's Eye (no cooldown, shares lock-on input) ─────────
        public override void CmdMark()
        {
            var cam = character != null ? character.Cameras : null;
            var forward = cam != null ? cam.Forward : transform.forward;
            var t = CombatQuery.BestInCone(self, transform.position, forward, MarkRange, 35f, LastMarked);
            if (t == null) t = CombatQuery.BestInCone(self, transform.position, forward, MarkRange, 35f);
            if (t == null) return;
            MarkTarget(t);
        }

        CombatTarget LastMarked => marked.Count > 0 ? marked[marked.Count - 1] : null;

        public void MarkTarget(CombatTarget t)
        {
            t.Status.ApplyMark(markDuration, markBonus);
            if (!marked.Contains(t)) marked.Add(t);
            var v = t.GetComponent<PlaceholderVisual>();
            if (v != null) v.Flash(new Color(0.4f, 0.9f, 1f), 0.25f, false);
        }

        // ───────── Specials ─────────
        protected override bool CanActivateSpecial(int index)
        {
            if (index == 1 && CombatQuery.AllHostile(self, t => t.Status.IsMarked && Vector3.Distance(t.transform.position, transform.position) <= skyCutRange).Count == 0)
            {
                ObjectiveTracker.Say("Chaska", "(Sky-Cut needs marked targets — use Condor's Eye first.)");
                return false;
            }
            if (index == 2 && FindStarFallTarget() == null) return false;
            return true;
        }

        protected override bool IsTraversalAbility(int index) => index == 0; // Vantage Leap: "traversal/combat hybrid"

        protected override void ActivateSpecial(int index)
        {
            switch (index)
            {
                case 0: StartAction(VantageLeap(), "Vantage Leap"); break;
                case 1: StartAction(SkyCut(), "Sky-Cut"); break;
                case 2: StartAction(StarFall(FindStarFallTarget()), "Star-Fall"); break;
            }
        }

        IEnumerator VantageLeap()
        {
            var cam = character != null ? character.Cameras : null;
            var forward = cam != null ? cam.Forward : transform.forward;
            var point = VantagePoint.Find(transform.position, forward, vantageRange);
            Vector3 start = transform.position;
            Vector3 end = point != null ? point.transform.position : start + forward.normalized * 3f + Vector3.up * 6f;
            float duration = 0.7f;
            float arc = Mathf.Max(2.5f, end.y - start.y + 2f);
            motor.gravityEnabled = false;
            motor.movementLocked = true;
            motor.FaceDirection(end - start, true);
            float t = 0f;
            while (t < duration)
            {
                t += motor.Delta;
                float k = Mathf.Clamp01(t / duration);
                var pos = Vector3.Lerp(start, end, k) + Vector3.up * (Mathf.Sin(k * Mathf.PI) * arc);
                motor.Teleport(pos);
                yield return null;
            }
            motor.gravityEnabled = true;
            // Airborne after a leap with no platform: next Light is the diving Falling Star.
        }

        IEnumerator SkyCut()
        {
            motor.movementLocked = true;
            var targets = CombatQuery.AllHostile(self, t => t.Status.IsMarked && Vector3.Distance(t.transform.position, transform.position) <= skyCutRange);
            yield return Wait(0.15f);
            foreach (var t in targets)
            {
                var payload = new DamageInfo { Amount = skyCutDamage * damageMultiplier, Flags = HitFlags.Ranged | HitFlags.Stagger, StaggerDuration = 0.5f, AttackName = "Sky-Cut" };
                if (t.Status.IsMarked) payload.Amount *= t.Status.MarkBonus;
                Projectile.Fire(self, self.AimPoint + Vector3.up * 0.5f, t.AimPoint - self.AimPoint, 30f, payload, new Color(0.7f, 0.95f, 1f), t);
                yield return Wait(0.05f);
            }
            yield return Wait(0.3f);
        }

        CombatTarget FindStarFallTarget()
        {
            var locked = character != null && character.Targeting != null ? character.Targeting.Locked : null;
            if (locked != null && IsExecutable(locked)) return locked;
            return CombatQuery.Nearest(self, transform.position, 12f, IsExecutable);
        }

        bool IsExecutable(CombatTarget t) =>
            !t.isDecoy && t.Health.Normalized <= StarFallThreshold && t.GetComponent<AtocBoss>() == null
            && Vector3.Distance(t.transform.position, transform.position) <= 12f;

        IEnumerator StarFall(CombatTarget target)
        {
            if (target == null) yield break;
            motor.movementLocked = true;
            motor.gravityEnabled = false;
            Vector3 start = transform.position;
            Vector3 end = target.transform.position - target.FlatDirectionTo(start) * -0.8f;
            float t = 0f;
            while (t < 0.45f)
            {
                t += motor.Delta;
                float k = Mathf.Clamp01(t / 0.45f);
                motor.Teleport(Vector3.Lerp(start, end, k) + Vector3.up * (Mathf.Sin(k * Mathf.PI) * 4f));
                yield return null;
            }
            motor.gravityEnabled = true;
            if (target != null && target.IsAlive)
            {
                target.ReceiveHit(new DamageInfo
                {
                    Amount = target.Health.Current + 999f, Attacker = self, Point = target.AimPoint, Direction = Vector3.down,
                    Flags = HitFlags.Execute | HitFlags.Unblockable, AttackName = "Star-Fall",
                });
            }
            yield return Wait(0.4f);
        }

        protected override void ActivateUltimate()
        {
            // "Time briefly slows for everyone but Chaska" — per-object delta time (TimeDilation), not Time.timeScale.
            TimeDilation.StartSlow(gapSlowScale, GapDuration, gameObject);
            if (character != null && character.Visual != null) character.Visual.Flash(new Color(0.6f, 0.9f, 1f), GapDuration, false);
        }

        public override void OnRespawn()
        {
            base.OnRespawn();
            motor.gravityEnabled = true;
            marked.Clear();
        }

        public bool Intercept(ref DamageInfo info, out HitResult result)
        {
            result = default;
            if (Time.time >= iFramesUntil) return false;
            if (!perfectDodgeAwarded)
            {
                perfectDodgeAwarded = true;
                Focus.Add(FocusRules.PerfectDodge, "Perfect dodge");
                TryTellReadBonus(info.Attacker);
            }
            result = new HitResult(HitOutcome.Evaded);
            return true;
        }
    }
}
