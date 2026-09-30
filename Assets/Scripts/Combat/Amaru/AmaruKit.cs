// Implements: 03-combat-design.md, Section 3 (Amaru Kit — Atoc): Fang Strikes x3 (setup-based damage), Coil Grab (pull into a
// trap), Shed Skin (dodge + decoy), Venom Riposte (timed counter, poison), Snare (placeable trap), Specials (Numbing Draught,
// False Trail, Last Fang), Ultimate (A Hundred and One) and the Upgrade Tree (Foundation / Venom / Rebirth).
using System.Collections;
using UnityEngine;

namespace Ruminahui
{
    public class AmaruKit : CombatKit, IHitInterceptor
    {
        public override KitId Kit => KitId.Amaru;

        [Header("Fang Strikes")]
        public float setupBonus = 1.5f;             // PLACEHOLDER-BALANCE: "built around setups, not standalone damage"

        [Header("Coil Grab")]
        public float coilRange = 3.5f;
        public float coilSnareSearchRadius = 5f;
        public float coilStamina = 12f;

        [Header("Shed Skin")]
        public float dodgeDistance = 3.2f;
        public float dodgeDuration = 0.22f;
        public float dodgeIFrames = 0.2f;
        public float dodgeStamina = 16f;
        public float decoyDurationBase = 1.6f;

        [Header("Venom Riposte")]
        public float riposteWindowBase = 0.25f;
        public float riposteWhiffRecovery = 0.45f;
        public float poisonDpsBase = 4f;
        public float poisonDurationBase = 6f;

        [Header("Snare")]
        public int snareCapacityBase = 2;

        [Header("Specials")]
        public float numbingRadius = 6f;
        public float numbingSlow = 0.5f;
        public float numbingDurationBase = 5f;
        public float falseTrailDuration = 4f;
        public float lastFangWindow = 0.45f;
        public float lastFangDamageBase = 60f;
        public float lastFangWhiffRecovery = 1.0f;
        public float rebirthArmDuration = 10f;
        public float rebirthCriticalFraction = 0.15f;
        public float rebirthRestoreFraction = 0.9f;
        public float rebirthBuffMultiplier = 1.6f;
        public float rebirthBuffDuration = 8f;

        public int InterceptPriority => 95;

        AttackData[] chain;
        AttackData coilAtk;
        float iFramesUntil;
        bool perfectDodgeAwarded;
        float riposteUntil = -1f;
        float lastFangUntil = -1f;
        float falseTrailUntil;
        float rebirthArmedUntil;
        float rebirthBuffUntil;
        bool rebirthUsed;

        int SnareCapacity => snareCapacityBase + (Tier >= 1 ? 1 : 0);
        float DecoyDuration => decoyDurationBase + (Tier >= 1 ? 0.8f : 0f);
        float RiposteWindow => riposteWindowBase + (Tier >= 2 ? 0.1f : 0f);
        float PoisonDps => poisonDpsBase * (Tier >= 2 ? 1.5f : 1f);
        float PoisonDuration => poisonDurationBase + (Tier >= 2 ? 2f : 0f);
        float NumbingDuration => numbingDurationBase + (Tier >= 2 ? 2f : 0f);
        float LastFangDamage => lastFangDamageBase * (Tier >= 3 ? 1.4f : 1f);

        public bool IsRiposteActive => Time.time < riposteUntil;
        public bool IsStealthed => Time.time < falseTrailUntil;
        public bool IsRebirthArmed => Time.time < rebirthArmedUntil && !rebirthUsed;

        protected override AttackData[] LightChain => chain;

        protected override void Awake()
        {
            // PLACEHOLDER-BALANCE: fastest, lowest raw damage of the three kits.
            chain = new[]
            {
                new AttackData("Fang Strike I", 6f, 1.9f, 0.06f, 0.13f, lunge: 0.7f),
                new AttackData("Fang Strike II", 6f, 1.9f, 0.06f, 0.13f, lunge: 0.7f),
                new AttackData("Fang Strike III", 9f, 2.0f, 0.08f, 0.22f, HitFlags.Stagger, stagger: 0.5f, lunge: 0.8f),
            };
            coilAtk = new AttackData("Coil Grab", 5f, 3.5f, 0.2f, 0.35f, HitFlags.Grab | HitFlags.Unblockable, stagger: 0.6f, halfAngle: 40f, lunge: 0f);
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
            Specials.Add(new AbilitySlot("numbing", "Numbing Draught", 30f, 12f, Unlocks.NumbingDraught));
            Specials.Add(new AbilitySlot("falsetrail", "False Trail", 25f, 14f, Unlocks.FalseTrail));
            Specials.Add(new AbilitySlot("lastfang", "Last Fang", 20f, 3f, Unlocks.LastFang));
            Ultimate = new AbilitySlot("hundredandone", "A Hundred and One", 100f, 90f, Unlocks.HundredAndOne, true);
        }

        void Update()
        {
            Ultimate.Cooldown = Tier >= 3 ? 60f : 90f; // Rebirth tier: cooldown reduced
            if (falseTrailUntil > 0f && Time.time >= falseTrailUntil) EndFalseTrail();
            if (rebirthBuffUntil > 0f && Time.time >= rebirthBuffUntil) { rebirthBuffUntil = 0f; damageMultiplier = 1f; }
        }

        protected override void ModifyLightHit(CombatTarget target, ref DamageInfo info)
        {
            if (target.Status.HasSetupDebuff) info.Amount *= setupBonus;
        }

        protected override void OnHitDealt(CombatTarget target, DamageInfo info, HitResult result)
        {
            base.OnHitDealt(target, info, result);
            if (IsStealthed && info.AttackName != "Snare") EndFalseTrail(); // attacking breaks False Trail
        }

        // ───────── Heavy: Coil Grab ─────────
        public override void CmdHeavyPressed()
        {
            if (IsBusy || status.IsIncapacitated) return;
            if (!stamina.TrySpend(coilStamina)) return;
            StartAction(CoilGrab(), "Coil Grab");
        }

        IEnumerator CoilGrab()
        {
            motor.movementLocked = true;
            FaceAim();
            yield return Wait(coilAtk.windup);
            var targets = CombatQuery.Cone(self, transform.position, transform.forward, coilRange, coilAtk.halfAngle, false);
            CombatTarget victim = null;
            float best = float.MaxValue;
            foreach (var t in targets)
            {
                float d = Vector3.Distance(t.transform.position, transform.position);
                if (d < best) { best = d; victim = t; }
            }
            if (victim != null)
            {
                victim.ReceiveHit(BuildDamage(coilAtk, victim, 1f));
                // Pull off-balance, or straight into one of our placed traps.
                var snare = Snare.NearestArmed(victim.transform.position, coilSnareSearchRadius, self.faction);
                Vector3 dest = snare != null ? snare.transform.position : transform.position + transform.forward * 1.2f;
                if (victim.Motor != null) victim.Motor.Teleport(new Vector3(dest.x, victim.transform.position.y, dest.z));
                if (snare != null) snare.Trigger(victim);
            }
            yield return Wait(coilAtk.recovery);
        }

        // ───────── Dodge: Shed Skin ─────────
        public override void CmdDodge(Vector3 dir)
        {
            if (IsBusy || status.IsIncapacitated) return;
            if (!stamina.TrySpend(dodgeStamina)) return;
            StartAction(ShedSkin(dir), "Shed Skin");
        }

        IEnumerator ShedSkin(Vector3 dir)
        {
            Decoy.Spawn(self, transform.position, transform.rotation, DecoyDuration, character != null ? character.color : Color.green);
            iFramesUntil = Time.time + dodgeIFrames;
            perfectDodgeAwarded = false;
            motor.Dash(dir, dodgeDistance, dodgeDuration);
            yield return Wait(dodgeDuration + 0.05f);
        }

        // ───────── Counter: Venom Riposte (timed, not a block) ─────────
        public override void CmdBlockPressed()
        {
            if (IsBusy || status.IsIncapacitated) return;
            StartAction(RiposteStance(), "Venom Riposte");
        }

        IEnumerator RiposteStance()
        {
            motor.movementLocked = true;
            riposteUntil = Time.time + RiposteWindow;
            yield return Wait(RiposteWindow);
            if (riposteUntil > 0f)
            {
                // Whiffed read: brief vulnerable recovery.
                riposteUntil = -1f;
                currentActionName = "Venom Riposte (whiff)";
                yield return Wait(riposteWhiffRecovery);
            }
        }

        // ───────── Placeable: Snare ─────────
        public override void CmdPlace()
        {
            if (IsBusy || status.IsIncapacitated) return;
            if (Snare.CountOwnedBy(self) >= SnareCapacity)
            {
                var oldest = Snare.OldestOwnedBy(self);
                if (oldest != null) oldest.Remove();
            }
            Snare.Place(self, transform.position + transform.forward * 1.0f);
            StartAction(Wait(0.25f), "Snare");
        }

        public int SnaresPlaced => Snare.CountOwnedBy(self);
        public int SnareCap => SnareCapacity;

        // ───────── Specials ─────────
        protected override void ActivateSpecial(int index)
        {
            switch (index)
            {
                case 0: StartAction(NumbingDraught(), "Numbing Draught"); break;
                case 1: FalseTrail(); break;
                case 2: StartAction(LastFangStance(), "Last Fang"); break;
            }
        }

        IEnumerator NumbingDraught()
        {
            motor.movementLocked = true;
            yield return Wait(0.3f);
            foreach (var t in CombatQuery.Sphere(self, transform.position, numbingRadius, false))
                t.Status.ApplySlow(numbingSlow, NumbingDuration);
            yield return Wait(0.3f);
        }

        void FalseTrail()
        {
            falseTrailUntil = Time.time + falseTrailDuration;
            self.stealthed = true;
            if (character != null && character.Visual != null) character.Visual.SetBaseColor(Color.Lerp(character.color, Color.black, 0.65f));
        }

        void EndFalseTrail()
        {
            falseTrailUntil = 0f;
            self.stealthed = false;
            if (character != null && character.Visual != null) character.Visual.SetBaseColor(character.color);
        }

        IEnumerator LastFangStance()
        {
            motor.movementLocked = true;
            lastFangUntil = Time.time + lastFangWindow;
            yield return Wait(lastFangWindow);
            if (lastFangUntil > 0f)
            {
                // Whiff: "leaves Atoc fully exposed".
                lastFangUntil = -1f;
                currentActionName = "Last Fang (whiff — exposed)";
                self.incomingDamageMultiplier = 1.5f;
                yield return Wait(lastFangWhiffRecovery);
                self.incomingDamageMultiplier = 1f;
            }
        }

        protected override void ActivateUltimate()
        {
            rebirthArmedUntil = Time.time + rebirthArmDuration;
            rebirthUsed = false;
            if (character != null && character.Visual != null) character.Visual.Flash(new Color(0.3f, 1f, 0.5f), 0.5f, false);
        }

        IEnumerator Rebirth()
        {
            // "dies" and resurfaces: vanish briefly, then return near full HP with a damage buff.
            self.invulnerable = true;
            self.stealthed = true;
            if (character != null && character.Visual != null) character.Visual.SetVisible(false);
            motor.movementLocked = true;
            yield return Wait(1.0f);
            self.Health.SetCurrent(self.Health.max * rebirthRestoreFraction);
            if (character != null && character.Visual != null) { character.Visual.SetVisible(true); character.Visual.Flash(new Color(0.3f, 1f, 0.5f), 0.6f, false); }
            self.invulnerable = false;
            self.stealthed = false;
            damageMultiplier = rebirthBuffMultiplier;
            rebirthBuffUntil = Time.time + rebirthBuffDuration;
        }

        protected override void OnActionCancelled()
        {
            riposteUntil = -1f;
            lastFangUntil = -1f;
            if (self.incomingDamageMultiplier > 1f) self.incomingDamageMultiplier = 1f;
        }

        public override void OnRespawn()
        {
            base.OnRespawn();
            EndFalseTrail();
            rebirthArmedUntil = 0f;
            rebirthBuffUntil = 0f;
            self.invulnerable = false;
            if (character != null && character.Visual != null) character.Visual.SetVisible(true);
        }

        // ───────── Defence resolution ─────────
        public bool Intercept(ref DamageInfo info, out HitResult result)
        {
            result = default;

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

            // Last Fang: perfect read of a HEAVY attack → massive damage; anything else while exposed → punished.
            if (Time.time < lastFangUntil)
            {
                lastFangUntil = -1f;
                if (info.Has(HitFlags.Heavy) || info.Has(HitFlags.Charge) || info.Has(HitFlags.Unblockable))
                {
                    if (info.Attacker != null)
                    {
                        info.Attacker.ReceiveHit(new DamageInfo
                        {
                            Amount = LastFangDamage * damageMultiplier, Attacker = self, Point = info.Attacker.AimPoint,
                            Direction = (info.Attacker.transform.position - transform.position).normalized,
                            Flags = HitFlags.Unblockable | HitFlags.Stagger, StaggerDuration = 1.5f, AttackName = "Last Fang",
                        });
                    }
                    Focus.Add(FocusRules.Counter, "Last Fang");
                    result = new HitResult(HitOutcome.Countered);
                    return true;
                }
                info.Amount *= 2f;
                info.StaggerDuration = Mathf.Max(info.StaggerDuration, 1.2f);
                return false;
            }

            // Venom Riposte: sidestep + poison on a successful read.
            if (Time.time < riposteUntil && !info.Has(HitFlags.Grab))
            {
                riposteUntil = -1f;
                if (info.Attacker != null)
                {
                    info.Attacker.Status.ApplyPoison(PoisonDps, PoisonDuration);
                    var side = Vector3.Cross(Vector3.up, info.Direction).normalized;
                    motor.Dash(side, 1.5f, 0.12f);
                    TryTellReadBonus(info.Attacker);
                }
                Focus.Add(FocusRules.Counter, "Venom Riposte");
                AudioPool.Instance?.PlayCue(PlaceholderCue.Parry, self.AimPoint);
                result = new HitResult(HitOutcome.Countered);
                return true;
            }

            // A Hundred and One: lethal/critical damage → "die" and resurface.
            if (IsRebirthArmed && self.Health.Current - info.Amount <= self.Health.max * rebirthCriticalFraction)
            {
                rebirthUsed = true;
                rebirthArmedUntil = 0f;
                CancelAction();
                StartAction(Rebirth(), "A Hundred and One", false);
                result = new HitResult(HitOutcome.Absorbed);
                return true;
            }

            return false;
        }
    }
}
