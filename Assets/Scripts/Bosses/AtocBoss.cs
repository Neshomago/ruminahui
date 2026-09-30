// Implements: 08-atoc-boss-fight.md in full —
//   Phase 1 "Reading Him" (100-65%): direct strikes + trap-and-reposition; tell = brief crouch-and-glance before a Snare drop.
//   Phase 2 "The Fox's Ground" (65-30%): wreckage cover, Shed Skin decoys (decoys lack his idle bob), brief stealth repositioning.
//   Phase 3 "Cornered" (30-0%): faster, riskier; full-commitment UNBLOCKABLE string telegraphed by a roar/stance change.
//   Final Beat: once staggered at low HP the finisher is replaced by a mandatory, distinct SPARE prompt. No kill option exists.
// Also 06 Enemy Roster (Atoc boss: HP Very High / Dmg High / Speed High) and 11-camera-performance.md Part A Step 7 (wide framing).
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ruminahui
{
    public enum AtocPhase { ReadingHim = 1, FoxsGround = 2, Cornered = 3, Spare = 4 }

    public class AtocBoss : EnemyBrain, IInteractable
    {
        public const float Phase2Threshold = 0.65f;
        public const float Phase3Threshold = 0.30f;
        public const float SpareThreshold = 0.10f;   // PLACEHOLDER-BALANCE: "staggered at low HP"

        [Header("Tuning — PLACEHOLDER-BALANCE")]
        public float strikeDamage = EnemyStats.DmgHigh - 4f;
        public float strikeWindup = 0.45f;
        public float snareInterval = 6f;
        public float stringDamage = EnemyStats.DmgHigh - 2f;
        public float stringInterval = 7f;
        public float phase3SpeedMultiplier = 1.3f;
        public float phase3WindupMultiplier = 0.7f;
        public float coverForceOutRadius = 3f;

        public List<Transform> coverPoints = new List<Transform>();

        public AtocPhase Phase { get; private set; } = AtocPhase.ReadingHim;
        public bool SpareAvailable => Phase == AtocPhase.Spare && !spared;
        public event System.Action Spared;
        public event System.Action<AtocPhase> PhaseChanged;

        float nextSnare, nextString;
        bool spared;
        float baseSpeed;
        Transform currentCover;

        public static AtocPhase PhaseForHealth(float normalized)
        {
            if (normalized > Phase2Threshold) return AtocPhase.ReadingHim;
            if (normalized > Phase3Threshold) return AtocPhase.FoxsGround;
            return AtocPhase.Cornered;
        }

        protected override void Awake()
        {
            base.Awake();
            flinchOnHit = false;
            status.staggerResistance = 0.6f;
            health.minimumHp = 1f; // he cannot be killed — only spared
            baseSpeed = motor.moveSpeed;
            var bar = GetComponent<BossBarTarget>();
            if (bar != null) { bar.phaseTicks.Clear(); bar.phaseTicks.Add(Phase2Threshold); bar.phaseTicks.Add(Phase3Threshold); }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            InteractableRegistry.All.Add(this);
            health.Changed += OnHealthChanged;
        }

        protected override void OnDisable()
        {
            InteractableRegistry.All.Remove(this);
            health.Changed -= OnHealthChanged;
            base.OnDisable();
        }

        protected override void OnReset()
        {
            Phase = AtocPhase.ReadingHim;
            spared = false;
            motor.moveSpeed = baseSpeed;
            self.invulnerable = false;
            self.suppressDamageNumbers = false;
            status.ForcedStagger = false;
            nextSnare = Time.time + snareInterval;
            nextString = Time.time + stringInterval;
        }

        void OnHealthChanged(float current, float max)
        {
            if (Phase == AtocPhase.Spare) return;
            var p = PhaseForHealth(current / max);
            if (p != Phase)
            {
                Phase = p;
                Debug.Log($"[AtocBoss] Phase → {Phase}");
                ObjectiveTracker.Set($"Atoc — Phase {(int)Phase}: {PhaseLabel(Phase)}");
                if (Phase == AtocPhase.Cornered) motor.moveSpeed = baseSpeed * phase3SpeedMultiplier;
                PhaseChanged?.Invoke(Phase);
            }
        }

        public static string PhaseLabel(AtocPhase p) =>
            p == AtocPhase.ReadingHim ? "Reading Him" : p == AtocPhase.FoxsGround ? "The Fox's Ground" : p == AtocPhase.Cornered ? "Cornered" : "The Mercy";

        protected override void OnRecoveredFromStagger()
        {
            if (Phase == AtocPhase.Spare) return;
        }

        protected override void Update()
        {
            base.Update();
            // Final beat: staggered at low HP → the fight ends, the spare prompt appears.
            if (Phase != AtocPhase.Spare && health.Normalized <= SpareThreshold && status.IsStaggered) EnterSpareState();
        }

        void EnterSpareState()
        {
            Phase = AtocPhase.Spare;
            StopBehaviour();
            status.ForcedStagger = true;
            self.invulnerable = true;
            self.suppressDamageNumbers = true;
            State = EnemyState.Staggered;
            if (visual != null) visual.SetPoseScale(new Vector3(1f, 0.6f, 1f)); // down on one knee
            ObjectiveTracker.Set("Atoc is beaten. [E / RT] Spare him.");
            PhaseChanged?.Invoke(Phase);
        }

        float WindupScale => Phase == AtocPhase.Cornered ? phase3WindupMultiplier : 1f;

        protected override IEnumerator Behaviour()
        {
            if (Phase == AtocPhase.Spare) yield break;
            while (true)
            {
                if (!AcquireTarget()) { Stop(); yield return Wait(0.3f); continue; }
                switch (Phase)
                {
                    case AtocPhase.ReadingHim: yield return PhaseOneStep(); break;
                    case AtocPhase.FoxsGround: yield return PhaseTwoStep(); break;
                    case AtocPhase.Cornered: yield return PhaseThreeStep(); break;
                    default: yield break;
                }
            }
        }

        // ───────── Phase 1: direct strikes + snare-and-reposition ─────────
        IEnumerator PhaseOneStep()
        {
            if (Time.time >= nextSnare)
            {
                nextSnare = Time.time + snareInterval;
                yield return SnareDrop();
                yield break;
            }
            yield return ApproachAndStrike(2);
        }

        IEnumerator SnareDrop()
        {
            // The tell every Amaru ally reuses from M3.6 on: brief crouch-and-glance.
            State = EnemyState.Telegraph;
            Stop();
            if (visual != null) { visual.SetPoseScale(new Vector3(1f, 0.72f, 1f)); visual.Flash(new Color(0.75f, 1f, 0.3f), 0.5f); }
            if (Target != null) motor.FaceDirection(Vector3.Cross(Vector3.up, Target.transform.position - transform.position), true); // glance aside
            yield return Wait(0.5f);
            if (visual != null) visual.ResetPose();
            Snare.Place(self, transform.position);
            // Reposition away so the player has to come through the trap.
            State = EnemyState.Reposition;
            if (Target != null) motor.Dash(transform.position - Target.transform.position, 3.5f, 0.25f);
            yield return Wait(0.5f);
        }

        IEnumerator ApproachAndStrike(int hits)
        {
            float chase = 0f;
            while (Target != null && DistanceToTarget > 2.2f && chase < 3f)
            {
                State = EnemyState.Chase;
                MoveTowards(Target.transform.position);
                chase += Dt;
                yield return null;
            }
            if (Target == null || DistanceToTarget > 2.6f) yield break;
            for (int i = 0; i < hits; i++)
            {
                yield return Telegraph(strikeWindup * WindupScale, TellColor);
                Lunge(0.7f);
                MeleeStrike(strikeDamage, 2.5f, 60f, HitFlags.None, 0f, 2f, $"Atoc strike {i + 1}/{hits}");
                yield return Wait(0.25f * WindupScale);
            }
            State = EnemyState.Recover;
            yield return Wait(0.7f * WindupScale);
        }

        // ───────── Phase 2: cover, decoys, stealth repositioning ─────────
        IEnumerator PhaseTwoStep()
        {
            var cover = PickCover();
            if (cover != null)
            {
                // Vanish and slip behind wreckage, leaving decoys in the open.
                State = EnemyState.Special;
                var decoyColor = visual != null ? visual.baseColor : CharacterFactory.AtocColor;
                Decoy.Spawn(self, transform.position + transform.right * 1.5f, transform.rotation, 5f, decoyColor);
                Decoy.Spawn(self, transform.position - transform.right * 1.5f, transform.rotation, 5f, decoyColor);
                self.stealthed = true;
                if (visual != null) visual.SetVisible(false);
                motor.Teleport(cover.position, cover.rotation);
                currentCover = cover;

                // Wait behind cover. Crowding him (Root-Step in close) forces him back into the open.
                float t = 0f;
                bool forcedOut = false;
                while (t < 3f)
                {
                    var c = PartyManager.Instance != null ? PartyManager.Instance.Controlled : null;
                    if (c != null && Vector3.Distance(c.transform.position, cover.position) <= coverForceOutRadius) { forcedOut = true; break; }
                    t += Dt;
                    yield return null;
                }
                self.stealthed = false;
                if (visual != null) visual.SetVisible(true);
                currentCover = null;

                if (forcedOut)
                {
                    ObjectiveTracker.Say("Atoc", "(forced out of cover)");
                    status.ApplyStagger(0.8f);
                    yield break;
                }
                // Otherwise: flank strike from the side.
                if (Target != null)
                {
                    var flank = Target.transform.position + Target.transform.right * (Random.value < 0.5f ? 2f : -2f);
                    motor.Teleport(new Vector3(flank.x, transform.position.y, flank.z));
                    yield return ApproachAndStrike(1);
                }
                yield break;
            }
            yield return ApproachAndStrike(2);
        }

        Transform PickCover()
        {
            Transform best = null;
            float bestScore = float.MaxValue;
            foreach (var c in coverPoints)
            {
                if (c == null || c == currentCover) continue;
                float dPlayer = Target != null ? Vector3.Distance(c.position, Target.transform.position) : 10f;
                if (dPlayer < coverForceOutRadius + 2f) continue;
                float score = Vector3.Distance(c.position, transform.position) + Random.value * 4f;
                if (score < bestScore) { bestScore = score; best = c; }
            }
            return best;
        }

        // ───────── Phase 3: cornered — faster, the unblockable string ─────────
        IEnumerator PhaseThreeStep()
        {
            if (Time.time >= nextString && DistanceToTarget < 7f)
            {
                nextString = Time.time + stringInterval;
                yield return UnblockableString();
                yield break;
            }
            yield return ApproachAndStrike(3);
        }

        IEnumerator UnblockableString()
        {
            superArmor = true;
            AudioPool.Instance?.PlayCue(PlaceholderCue.Roar, self.AimPoint, 0.8f);
            if (visual != null) visual.SetPoseScale(new Vector3(1.1f, 1.1f, 1.1f)); // stance change
            yield return Telegraph(0.9f, UnblockableColor, null);
            if (visual != null) visual.ResetPose();
            for (int i = 0; i < 3; i++)
            {
                if (Target != null) motor.FaceTowards(Target.transform.position, true);
                Lunge(1.3f);
                MeleeStrike(stringDamage, 2.7f, 70f, HitFlags.Unblockable | HitFlags.Heavy, 0.3f, 3f, $"Cornered string {i + 1}/3");
                yield return Wait(0.3f);
            }
            superArmor = false;
            State = EnemyState.Recover;
            yield return Wait(1.4f); // the punish window — "more punishable"
        }

        protected override void OnBehaviourStopped()
        {
            superArmor = false;
            if (currentCover != null)
            {
                currentCover = null;
                self.stealthed = false;
                if (visual != null) visual.SetVisible(true);
            }
        }

        // ───────── Final beat: the mandatory spare (IInteractable) ─────────
        public Vector3 Position => transform.position;
        public float Range => 3f;
        public PromptStyle Style => PromptStyle.Spare;
        public float HoldDuration => 0f;
        public bool OverridesKitContext => true;
        public string PromptFor(PlayerCharacter c) => SpareAvailable ? "Spare" : null;
        public bool CanInteract(PlayerCharacter c) => SpareAvailable;

        public void Interact(PlayerCharacter c)
        {
            if (!SpareAvailable) return;
            spared = true;
            StartCoroutine(SpareSequence(c));
        }

        IEnumerator SpareSequence(PlayerCharacter c)
        {
            // Visually distinct: dedicated shot, no damage number, no kill-cam, combat UI hidden.
            var input = GameInput.Instance;
            if (input != null) input.PushCutscene();
            if (UIRoot.Instance != null) UIRoot.Instance.SetCombatUIVisible(false);
            var dir = c != null ? (transform.position - c.transform.position).normalized : transform.forward;
            var mid = c != null ? (transform.position + c.transform.position) * 0.5f : transform.position;
            var side = Vector3.Cross(Vector3.up, dir);
            var shot = CameraDirector.Instance.CreateShot("VCam_Cutscene_Spare", mid + side * 3.5f + Vector3.up * 1.3f, mid + Vector3.up * 1.1f, 0.1f);
            if (c != null && c.Visual != null) c.Visual.SetPoseScale(new Vector3(1f, 0.97f, 1f)); // weapon lowered (placeholder)
            ObjectiveTracker.Say("Rumiñahui", "(lowers the warclub)");
            yield return CameraDirector.Instance.PlayShot(shot, 3.5f);
            Destroy(shot.gameObject);
            if (c != null && c.Visual != null) c.Visual.ResetPose();
            if (UIRoot.Instance != null) UIRoot.Instance.SetCombatUIVisible(true);
            if (input != null) input.PopCutscene();
            Progression.Unlock(Unlocks.StoneFace); // "unlocked at M3.4 after sparing Atoc"
            ObjectiveTracker.Set("Atoc spared and captured.");
            Spared?.Invoke();
        }
    }
}
