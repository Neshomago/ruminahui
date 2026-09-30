// Implements: 03-combat-design.md, Section 1 (Puma Kit — Rumiñahui): Basic Moveset (Stone Strikes x3, Warclub Crush charge,
// Stone Stance / Stone Parry, Root-Step, Grounding Throw), Specials (Earthbreaker, Unyielding, Stone Face),
// Ultimate (What Held the Line) and the 3-tier Upgrade Tree (Foundation / Weight / Stillness).
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ruminahui
{
    public class PumaKit : CombatKit, IHitInterceptor
    {
        public override KitId Kit => KitId.Puma;

        [Header("Stone Stance / Stone Parry")]
        public float blockChipFraction = 0.15f;       // PLACEHOLDER-BALANCE: "chip damage only"
        public float blockHoldDrainPerSecond = 8f;    // PLACEHOLDER-BALANCE: "STA drains while held"
        public float blockHitStaminaPerDamage = 0.8f; // PLACEHOLDER-BALANCE
        public float parryWindowBase = 0.18f;         // PLACEHOLDER-BALANCE: widened by Stillness tier
        public float parryStagger = 1.5f;             // PLACEHOLDER-BALANCE: "staggers most enemies"

        [Header("Root-Step")]
        public float dodgeDistance = 2.6f;            // PLACEHOLDER-BALANCE: deliberately shorter than Chaska's Skywalk
        public float dodgeDuration = 0.22f;
        public float dodgeIFrames = 0.18f;
        public float dodgeStamina = 18f;

        [Header("Warclub Crush")]
        public float heavyMaxCharge = 1f;             // PLACEHOLDER-BALANCE: seconds to full charge
        public float heavyChargeBonus = 1f;           // +100% damage at full charge

        [Header("Grounding Throw")]
        public float grappleRange = 2.6f;
        public float throwDamage = 22f;
        public float throwSplashDamage = 14f;
        public float throwKnockback = 14f;

        [Header("Specials")]
        public float earthbreakerDamage = 22f;
        public float earthbreakerRadiusBase = 4f;
        public float unyieldingDurationBase = 5f;
        public float stoneFaceDamage = 70f;
        public float ultimateDuration = 8f;
        public float ultimateIncomingMultiplier = 0.1f; // "near-invulnerability"
        public float ultimateDamageMultiplier = 2.5f;   // "heavily amplified damage"

        public int InterceptPriority => 90;

        bool blocking;
        float blockPressedAt = -10f;
        bool chargingHeavy;
        float chargeStart;
        float iFramesUntil;
        bool perfectDodgeAwarded;
        float ultimateUntil;

        AttackData[] chain3, chain4;
        AttackData heavyAtk;

        public bool IsBlocking => blocking;
        public bool IsChargingHeavy => chargingHeavy;
        public float ParryWindow => parryWindowBase + (Tier >= 3 ? 0.1f : 0f);
        float EarthbreakerRadius => earthbreakerRadiusBase + (Tier >= 2 ? 1.5f : 0f);
        float UnyieldingDuration => unyieldingDurationBase + (Tier >= 3 ? 2f : 0f);

        public override float MoveSpeedMultiplier => blocking ? 0.35f : chargingHeavy ? 0.3f : 1f;
        public override bool AllowsMovement => !IsBusy || chargingHeavy;

        protected override AttackData[] LightChain => Tier >= 1 ? chain4 : chain3; // Foundation: combo extension

        protected override void Awake()
        {
            // PLACEHOLDER-BALANCE: all numbers below — weighty, slower than Kuntur, third hit staggers.
            chain3 = new[]
            {
                new AttackData("Stone Strike I", 12f, 2.2f, 0.12f, 0.22f),
                new AttackData("Stone Strike II", 13f, 2.2f, 0.12f, 0.24f),
                new AttackData("Stone Strike III", 20f, 2.4f, 0.2f, 0.4f, HitFlags.Stagger, stagger: 0.9f, knockback: 4f),
            };
            chain4 = new[]
            {
                chain3[0], chain3[1],
                new AttackData("Stone Strike III", 16f, 2.3f, 0.14f, 0.26f),
                new AttackData("Stone Strike IV", 24f, 2.5f, 0.22f, 0.42f, HitFlags.Stagger, stagger: 1.0f, knockback: 5f),
            };
            heavyAtk = new AttackData("Warclub Crush", 28f, 2.6f, 0.35f, 0.55f, HitFlags.Heavy | HitFlags.GuardBreak,
                stagger: 1.2f, staminaCost: 20f, guardBreakPower: 1, knockback: 6f);
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
            Specials.Add(new AbilitySlot("earthbreaker", "Earthbreaker", 30f, 6f, Unlocks.Earthbreaker));
            Specials.Add(new AbilitySlot("unyielding", "Unyielding", 25f, 15f, Unlocks.Unyielding));
            Specials.Add(new AbilitySlot("stoneface", "Stone Face", 40f, 4f, Unlocks.StoneFace));
            Ultimate = new AbilitySlot("whatheldtheline", "What Held the Line", 100f, 60f, Unlocks.WhatHeldTheLine, true);
        }

        void Update()
        {
            // Foundation tier: stamina efficiency. Stillness tier: Stone Face cost reduced.
            stamina.costMultiplier = Tier >= 1 ? 0.8f : 1f;
            Specials[2].FocusCost = Tier >= 3 ? 30f : 40f;

            if (blocking)
            {
                stamina.regenBlocked = true;
                stamina.Drain(blockHoldDrainPerSecond * Time.deltaTime);
                if (stamina.IsExhausted) GuardBroken();
            }

            if (Time.time > ultimateUntil && ultimateUntil > 0f) EndUltimate();
        }

        // ───────── Heavy: Warclub Crush (chargeable, guard-break) ─────────
        public override void CmdHeavyPressed()
        {
            if (IsBusy || status.IsIncapacitated || blocking) return;
            chargingHeavy = true;
            chargeStart = Time.time;
            StartAction(ChargeHeavy(), "Warclub Crush (charging)");
        }

        public override void CmdHeavyReleased()
        {
            chargingHeavy = false;
        }

        IEnumerator ChargeHeavy()
        {
            while (chargingHeavy && Time.time - chargeStart < heavyMaxCharge + 0.5f) yield return null; // auto-release shortly after full
            chargingHeavy = false;
            float charge = Mathf.Clamp01((Time.time - chargeStart) / heavyMaxCharge);
            var atk = heavyAtk.Clone();
            atk.guardBreakPower = heavyAtk.guardBreakPower + (Tier >= 2 ? 1 : 0) + (charge >= 0.99f ? 1 : 0); // Weight tier: guard-break scaling
            atk.staggerDuration += charge * 0.6f;
            currentActionName = charge >= 0.99f ? "Warclub Crush (full charge)" : "Warclub Crush";
            yield return AttackRoutine(atk, 1f + charge * heavyChargeBonus);
        }

        protected override void OnActionCancelled() => chargingHeavy = false;

        // ───────── Block: Stone Stance / Stone Parry ─────────
        public override void CmdBlockPressed()
        {
            if (IsBusy || status.IsIncapacitated || stamina.IsExhausted) return;
            blocking = true;
            blockPressedAt = Time.time;
        }

        public override void CmdBlockReleased()
        {
            blocking = false;
            stamina.regenBlocked = false;
        }

        void GuardBroken()
        {
            blocking = false;
            stamina.regenBlocked = false;
            status.ApplyStagger(0.8f);
        }

        // ───────── Dodge: Root-Step ─────────
        public override void CmdDodge(Vector3 dir)
        {
            if (IsBusy || status.IsIncapacitated) return;
            if (!stamina.TrySpend(dodgeStamina)) return;
            blocking = false;
            StartAction(Dodge(dir), "Root-Step");
        }

        IEnumerator Dodge(Vector3 dir)
        {
            iFramesUntil = Time.time + dodgeIFrames;
            perfectDodgeAwarded = false;
            motor.Dash(dir, dodgeDistance, dodgeDuration);
            yield return Wait(dodgeDuration + 0.08f);
        }

        // ───────── Grapple: Grounding Throw ─────────
        CombatTarget FindGrappleTarget() =>
            CombatQuery.Nearest(self, transform.position, grappleRange, t => t.Status.IsOpen && !t.isDecoy && t.GetComponent<AtocBoss>() == null);

        public override bool TryContextAction()
        {
            if (IsBusy || status.IsIncapacitated) return false;
            var target = FindGrappleTarget();
            if (target == null) return false;
            StartAction(GroundingThrow(target), "Grounding Throw");
            return true;
        }

        public override bool GetContextPrompt(out string text, out PromptStyle style)
        {
            style = PromptStyle.Grapple;
            text = null;
            if (IsBusy) return false;
            if (FindGrappleTarget() == null) return false;
            text = "Grounding Throw";
            return true;
        }

        IEnumerator GroundingThrow(CombatTarget target)
        {
            motor.movementLocked = true;
            motor.FaceTowards(target.transform.position, true);
            yield return Wait(0.2f);
            if (target == null || !target.IsAlive) yield break;
            float dmgScale = Tier >= 2 ? 1.5f : 1f; // Weight tier: throw damage
            var info = new DamageInfo
            {
                Amount = throwDamage * dmgScale * damageMultiplier,
                Attacker = self,
                Point = target.AimPoint,
                Direction = transform.forward,
                Flags = HitFlags.Grab | HitFlags.Unblockable | HitFlags.Knockdown,
                StaggerDuration = 1.4f,
                Knockback = throwKnockback,
                AttackName = "Grounding Throw",
            };
            target.ReceiveHit(info);
            yield return Wait(0.3f);
            // "into terrain/other enemies": anyone the thrown body lands on takes splash damage.
            if (target != null)
            {
                foreach (var other in CombatQuery.Sphere(self, target.transform.position, 1.6f, false))
                {
                    if (other == target) continue;
                    other.ReceiveHit(new DamageInfo
                    {
                        Amount = throwSplashDamage * dmgScale * damageMultiplier, Attacker = self, Point = other.AimPoint,
                        Direction = (other.transform.position - target.transform.position).normalized,
                        Flags = HitFlags.Knockdown, StaggerDuration = 1f, Knockback = 6f, AttackName = "Grounding Throw (impact)",
                    });
                }
            }
            yield return Wait(0.35f);
        }

        // ───────── Specials ─────────
        protected override bool CanActivateSpecial(int index)
        {
            if (index == 2) return FindStoneFaceTarget() != null; // only on an enemy who is already staggered
            return true;
        }

        CombatTarget FindStoneFaceTarget()
        {
            var locked = character != null && character.Targeting != null ? character.Targeting.Locked : null;
            if (locked != null && locked.Status.IsOpen && Vector3.Distance(locked.transform.position, transform.position) <= 3f) return locked;
            return CombatQuery.Nearest(self, transform.position, 3f, t => t.Status.IsOpen && !t.isDecoy);
        }

        protected override void ActivateSpecial(int index)
        {
            switch (index)
            {
                case 0: StartAction(Earthbreaker(), "Earthbreaker"); break;
                case 1: Unyielding(); break;
                case 2: StartAction(StoneFace(FindStoneFaceTarget()), "Stone Face"); break;
            }
        }

        IEnumerator Earthbreaker()
        {
            motor.movementLocked = true;
            yield return Wait(0.35f);
            CombatPools.Spark(transform.position + Vector3.up * 0.2f);
            foreach (var t in CombatQuery.Sphere(self, transform.position, EarthbreakerRadius))
            {
                t.ReceiveHit(new DamageInfo
                {
                    Amount = earthbreakerDamage * damageMultiplier, Attacker = self, Point = t.AimPoint,
                    Direction = (t.transform.position - transform.position).normalized,
                    Flags = HitFlags.Knockdown | HitFlags.Heavy | HitFlags.GuardBreak, GuardBreakPower = 1,
                    StaggerDuration = 1.5f, Knockback = 7f, AttackName = "Earthbreaker",
                });
            }
            yield return Wait(0.5f);
        }

        void Unyielding()
        {
            // "incoming hits stagger instead of interrupt" — hyper-armour: hits still land, but never cancel the combo.
            status.ApplyHyperArmor(UnyieldingDuration);
            if (character != null && character.Visual != null) character.Visual.Flash(new Color(0.9f, 0.8f, 0.5f), 0.4f, false);
        }

        IEnumerator StoneFace(CombatTarget target)
        {
            if (target == null) yield break;
            motor.movementLocked = true;
            motor.FaceTowards(target.transform.position, true);
            yield return Wait(0.45f);
            if (target == null || !target.IsAlive) yield break;
            target.ReceiveHit(new DamageInfo
            {
                Amount = stoneFaceDamage * damageMultiplier, Attacker = self, Point = target.AimPoint, Direction = transform.forward,
                Flags = HitFlags.Unblockable | HitFlags.Heavy | HitFlags.GuardBreak, GuardBreakPower = 3,
                StaggerDuration = 1.5f, Knockback = 8f, AttackName = "Stone Face",
            });
            yield return Wait(0.6f);
        }

        protected override void ActivateUltimate()
        {
            ultimateUntil = Time.time + ultimateDuration;
            self.incomingDamageMultiplier = ultimateIncomingMultiplier;
            damageMultiplier = ultimateDamageMultiplier;
            status.ApplyHyperArmor(ultimateDuration);
            if (character != null && character.Visual != null) character.Visual.Flash(new Color(1f, 0.85f, 0.3f), ultimateDuration, false);
        }

        /// <summary>Used by the M3.5 shared-vision beat to trigger the ultimate without spending Focus.</summary>
        public void TriggerUltimateFree() => ActivateUltimate();

        void EndUltimate()
        {
            ultimateUntil = 0f;
            self.incomingDamageMultiplier = 1f;
            damageMultiplier = 1f;
        }

        public override void OnRespawn()
        {
            base.OnRespawn();
            EndUltimate();
            blocking = false;
            chargingHeavy = false;
            stamina.regenBlocked = false;
        }

        // ───────── Defence resolution ─────────
        public bool Intercept(ref DamageInfo info, out HitResult result)
        {
            result = default;

            // Root-Step i-frames — a hit landing inside them is a perfect dodge.
            if (Time.time < iFramesUntil)
            {
                if (!perfectDodgeAwarded)
                {
                    perfectDodgeAwarded = true;
                    Focus.Add(FocusRules.PerfectDodge, "Perfect dodge");
                    TryTellReadBonus(info.Attacker);
                }
                result = new HitResult(HitOutcome.Evaded);
                return true;
            }

            if (!blocking) return false;
            if (info.Has(HitFlags.Unblockable) || info.Has(HitFlags.Grab)) return false;
            if (info.Attacker != null && !self.IsInFront(info.Attacker.transform.position, 80f)) return false;

            // Stone Parry: block pressed just before the hit.
            if (Time.time - blockPressedAt <= ParryWindow && !info.Has(HitFlags.Charge))
            {
                if (info.Attacker != null) info.Attacker.Status.ApplyStagger(parryStagger);
                Focus.Add(FocusRules.PerfectParry, "Stone Parry");
                TryTellReadBonus(info.Attacker);
                AudioPool.Instance?.PlayCue(PlaceholderCue.Parry, self.AimPoint);
                if (character != null && character.Visual != null) character.Visual.Flash(Color.white, 0.15f, false);
                result = new HitResult(HitOutcome.Parried);
                return true;
            }

            // Cavalry charges crush a held guard (06: "punishes players who default to blocking").
            if (info.Has(HitFlags.Charge) || (info.Has(HitFlags.GuardBreak) && info.GuardBreakPower >= 2))
            {
                GuardBroken();
                return false;
            }

            // Stone Stance: chip damage only, stamina pays for the rest.
            float chip = info.Amount * blockChipFraction;
            bool broke = stamina.Drain(info.Amount * blockHitStaminaPerDamage);
            float dealt = self.Health.ApplyDamage(chip);
            AudioPool.Instance?.PlayCue(PlaceholderCue.Block, self.AimPoint);
            if (broke) GuardBroken();
            result = new HitResult(HitOutcome.Blocked, dealt);
            return true;
        }
    }
}
