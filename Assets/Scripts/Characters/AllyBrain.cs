// Implements: 03-combat-design.md, Section 4 (Chaska/Atoc as AI allies using their own kits) — simple ally AI, no command menu yet;
// 05 stage L ("Chaska and Atoc now fight as full AI allies, each using their own kit at full strength");
// 06 M5.3 non-required characters idle/keep watch/comment rather than vanishing.
using UnityEngine;

namespace Ruminahui
{
    public class AllyBrain : MonoBehaviour
    {
        public float followDistance = 3.5f;       // PLACEHOLDER-BALANCE
        public float engageRadius = 12f;          // around the leader
        public float attackRange = 1.9f;
        public float attackInterval = 0.35f;
        public float specialInterval = 9f;        // AI uses specials on its own timer, Focus-free (ASSUMPTION)
        public float barkInterval = 25f;

        PlayerCharacter character;
        float nextAttack, nextSpecial, nextBark, nextSnare, nextBolas;
        Vector3 holdPosition;
        bool hasHold;
        static readonly string[] Barks =
        {
            "(keeps watch on the ridge)", "(checks the path ahead)", "(listens to the wind)", "(adjusts the pack)",
        };

        void Awake() => character = GetComponent<PlayerCharacter>();

        void OnEnable()
        {
            nextSpecial = Time.time + specialInterval * Random.Range(0.4f, 1f);
            nextBark = Time.time + barkInterval * Random.Range(0.5f, 1.5f);
        }

        public void HoldAt(Vector3 position)
        {
            holdPosition = position;
            hasHold = true;
        }

        public void ClearHold() => hasHold = false;

        void Update()
        {
            var motor = character.Motor;
            var kit = character.Kit;
            if (character.ScriptLocked || character.Health.IsDead || character.Status.IsIncapacitated)
            {
                motor.SetMoveInput(Vector3.zero);
                return;
            }

            var leader = PartyManager.Instance != null ? PartyManager.Instance.Controlled : null;

            switch (character.allyMode)
            {
                case AllyMode.Idle:
                    motor.SetMoveInput(Vector3.zero);
                    return;
                case AllyMode.Hold:
                    MoveTo(hasHold ? holdPosition : transform.position, 0.5f);
                    return;
                case AllyMode.Follow:
                    FollowLeader(leader);
                    Bark();
                    return;
            }

            // Combat
            var target = PickTarget(leader);
            character.AllyTarget = target;
            if (target == null)
            {
                FollowLeader(leader);
                return;
            }
            if (kit == null || kit.IsBusy) { motor.SetMoveInput(Vector3.zero); return; }

            float dist = Vector3.Distance(transform.position, target.transform.position);
            var brain = target.GetComponent<EnemyBrain>();
            bool threatened = brain != null && brain.IsTelegraphing && brain.Target == character.Target && dist < 3.5f;

            // Defensive reads on tells.
            if (threatened && Random.value < 0.6f)
            {
                if (kit is AmaruKit) kit.CmdBlockPressed(); // Venom Riposte read
                else kit.CmdDodge(-(target.transform.position - transform.position));
                return;
            }

            // Kit flavour.
            if (kit is KunturKit kk)
            {
                if (!target.Status.IsMarked) kk.MarkTarget(target);
                if (dist > 6f && Time.time >= nextBolas) { nextBolas = Time.time + 4f; motor.FaceTowards(target.transform.position, true); kit.CmdHeavyPressed(); return; }
            }
            if (kit is AmaruKit ak && Time.time >= nextSnare && ak.SnaresPlaced < ak.SnareCap && dist < 6f)
            {
                nextSnare = Time.time + 8f;
                kit.CmdPlace();
                return;
            }
            if (Time.time >= nextSpecial)
            {
                nextSpecial = Time.time + specialInterval;
                for (int i = 0; i < kit.Specials.Count; i++)
                    if (kit.UseSpecial(Random.Range(0, kit.Specials.Count), true)) return;
            }

            if (dist > attackRange)
            {
                MoveTo(target.transform.position, attackRange * 0.8f);
                return;
            }
            motor.SetMoveInput(Vector3.zero);
            motor.FaceTowards(target.transform.position);
            if (Time.time >= nextAttack)
            {
                nextAttack = Time.time + attackInterval;
                kit.CmdLight();
            }
        }

        CombatTarget PickTarget(PlayerCharacter leader)
        {
            Vector3 center = leader != null ? leader.transform.position : transform.position;
            var leaderLock = leader != null && leader.Targeting != null ? leader.Targeting.Locked : null;
            if (leaderLock != null && leaderLock.IsTargetable && !leaderLock.isDecoy) return leaderLock;
            return CombatQuery.Nearest(character.Target, center, engageRadius, t => !t.isDecoy);
        }

        void FollowLeader(PlayerCharacter leader)
        {
            if (leader == null || leader == character) { character.Motor.SetMoveInput(Vector3.zero); return; }
            // Offset per character so allies don't stack on the same spot.
            float side = character.id == CharacterId.Chaska ? 1f : -1f;
            var slot = leader.transform.position - leader.transform.forward * 1.5f + leader.transform.right * (side * 1.8f);
            if (Vector3.Distance(transform.position, leader.transform.position) > 40f)
            {
                character.Motor.Teleport(slot); // catch-up so companions never get lost (M5.3: "shared journey")
                return;
            }
            MoveTo(slot, followDistance * 0.4f);
        }

        void MoveTo(Vector3 point, float stopDistance)
        {
            var to = point - transform.position;
            to.y = 0f;
            if (to.magnitude <= stopDistance) { character.Motor.SetMoveInput(Vector3.zero); return; }
            // Don't follow the leader off a ledge (M5.3 Wide Break: companions wait for the line).
            var probe = transform.position + to.normalized * 0.9f + Vector3.up * 0.5f;
            if (character.Motor.IsGrounded && !Physics.Raycast(probe, Vector3.down, 2.5f, ~0, QueryTriggerInteraction.Ignore))
            {
                character.Motor.SetMoveInput(Vector3.zero);
                return;
            }
            float speed = to.magnitude > 6f ? 1f : 0.6f;
            character.Motor.SetMoveInput(to.normalized, speed);
        }

        void Bark()
        {
            if (Time.time < nextBark) return;
            nextBark = Time.time + barkInterval * Random.Range(0.8f, 1.4f);
            ObjectiveTracker.Say(character.displayName, Barks[Random.Range(0, Barks.Length)]);
        }
    }
}
