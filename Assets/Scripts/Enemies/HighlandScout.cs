// Implements: 06 Enemy Roster, Ambush-Type (Highland Scout) — HP Low / Dmg High (burst) / Speed Med; hides until triggered,
// opens with a high-damage sneak attack, then fights normally. Tell: a very brief AUDIO-ONLY cue (snapped branch/bird startle).
// Countered by Amaru Snares placed pre-emptively near the hiding spot.
using System.Collections;
using UnityEngine;

namespace Ruminahui
{
    public class HighlandScout : EnemyBrain
    {
        public float triggerRadius = 7f;         // PLACEHOLDER-BALANCE
        public float ambushCueLead = 0.6f;       // "very brief pre-ambush audio cue"
        public float snareNeutralizeRadius = 3.5f;
        public float ambushDamage = EnemyStats.DmgHigh + 6f;
        public float normalDamage = EnemyStats.DmgMed;

        protected override void OnReset()
        {
            State = EnemyState.Hidden;
            self.stealthed = true;
            if (visual != null) visual.SetVisible(false);
        }

        protected override IEnumerator Behaviour()
        {
            if (State == EnemyState.Hidden)
            {
                // Wait unseen.
                while (true)
                {
                    var c = CombatQuery.Nearest(self, transform.position, triggerRadius, t => !t.isDecoy);
                    // stealthed targets aren't returned by Nearest, so query players directly while hidden
                    if (c == null) c = NearestPlayerIgnoringStealth();
                    if (c != null) { Target = c; break; }
                    yield return Wait(0.15f);
                }

                // A player Snare near the hiding spot neutralises the ambush.
                var snare = Snare.NearestArmed(transform.position, snareNeutralizeRadius, Faction.Player);
                Reveal();
                if (snare != null)
                {
                    ObjectiveTracker.Say("Scout", "(caught in a snare before the ambush)");
                    snare.Trigger(self);
                    yield break; // stagger reaction restarts Behaviour in normal mode
                }

                // The audio-only tell.
                AudioPool.Instance?.PlayCue(PlaceholderCue.AmbushSnap, transform.position, 0.8f);
                State = EnemyState.Telegraph;
                if (visual != null) visual.SetVisible(false);
                yield return Wait(ambushCueLead);
                if (visual != null) visual.SetVisible(true);

                if (Target != null)
                {
                    var behind = Target.transform.position - Target.transform.forward * 1.2f;
                    motor.Teleport(new Vector3(behind.x, transform.position.y, behind.z));
                    motor.FaceTowards(Target.transform.position, true);
                    MeleeStrike(ambushDamage, 2.4f, 90f, HitFlags.Stagger, 0.6f, 3f, "Ambush");
                }
                yield return Wait(0.8f);
            }

            // Normal fighting afterwards.
            while (true)
            {
                if (!AcquireTarget()) { State = EnemyState.Idle; Stop(); yield return Wait(0.4f); continue; }
                if (DistanceToTarget > 2f)
                {
                    State = EnemyState.Chase;
                    MoveTowards(Target.transform.position);
                    yield return null;
                    continue;
                }
                yield return Telegraph(0.45f, TellColor);
                Lunge(0.4f);
                MeleeStrike(normalDamage, 2.3f, 60f, HitFlags.None, 0f, 1f, "Knife");
                State = EnemyState.Recover;
                yield return Wait(0.8f);
            }
        }

        CombatTarget NearestPlayerIgnoringStealth()
        {
            CombatTarget best = null;
            float bestD = triggerRadius;
            foreach (var t in CombatTarget.All)
            {
                if (t.faction != Faction.Player || t.isDecoy || !t.IsAlive) continue;
                float d = Vector3.Distance(t.transform.position, transform.position);
                if (d < bestD) { bestD = d; best = t; }
            }
            return best;
        }

        void Reveal()
        {
            self.stealthed = false;
            if (visual != null) visual.SetVisible(true);
            State = EnemyState.Chase;
        }

        protected override void OnRecoveredFromStagger()
        {
            if (State == EnemyState.Hidden) Reveal();
        }
    }
}
