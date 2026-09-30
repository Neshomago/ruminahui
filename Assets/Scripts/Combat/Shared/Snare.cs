// Implements: 03-combat-design.md, Section 3 (Amaru "Snare" placeable terrain trap) — also used by Atoc as a boss
// (08-atoc-boss-fight.md Phase 1) and as the pre-emptive Highland Scout counter (06 Enemy Roster).
using System.Collections.Generic;
using UnityEngine;

namespace Ruminahui
{
    public class Snare : MonoBehaviour, IPoolable
    {
        public const string PoolKey = "Combat/Snare";
        public static readonly List<Snare> Active = new List<Snare>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => Active.Clear();

        public float triggerRadius = 1.2f;   // PLACEHOLDER-BALANCE
        public float armTime = 0.5f;         // PLACEHOLDER-BALANCE
        public float rootDuration = 2.5f;    // PLACEHOLDER-BALANCE
        public float damage = 10f;           // PLACEHOLDER-BALANCE
        public float lifetime = 45f;         // PLACEHOLDER-BALANCE

        public CombatTarget Owner { get; private set; }
        public Faction OwnerFaction { get; private set; }
        public bool IsArmed => live && age >= armTime;
        bool live;
        float age;
        PlaceholderVisual visual;

        void Awake() => visual = GetComponent<PlaceholderVisual>();

        public static Snare Place(CombatTarget owner, Vector3 position)
        {
            if (PoolManager.Instance == null) return null;
            var s = PoolManager.Instance.Spawn<Snare>(PoolKey, position, Quaternion.identity);
            if (s == null) return null;
            s.Owner = owner;
            s.OwnerFaction = owner != null ? owner.faction : Faction.Neutral;
            s.live = true;
            s.age = 0f;
            if (s.visual != null)
                s.visual.SetBaseColor(s.OwnerFaction == Faction.Player ? new Color(0.35f, 0.25f, 0.1f) : new Color(0.5f, 0.1f, 0.1f));
            return s;
        }

        public static int CountOwnedBy(CombatTarget owner)
        {
            int n = 0;
            foreach (var s in Active) if (s.Owner == owner) n++;
            return n;
        }

        public static Snare OldestOwnedBy(CombatTarget owner)
        {
            foreach (var s in Active) if (s.Owner == owner) return s; // Active is in placement order
            return null;
        }

        public static Snare NearestArmed(Vector3 pos, float radius, Faction ownerFaction)
        {
            Snare best = null;
            float bestD = radius;
            foreach (var s in Active)
            {
                if (!s.IsArmed || s.OwnerFaction != ownerFaction) continue;
                float d = Vector3.Distance(pos, s.transform.position);
                if (d <= bestD) { bestD = d; best = s; }
            }
            return best;
        }

        public void OnSpawned()
        {
            if (!Active.Contains(this)) Active.Add(this);
        }

        public void OnDespawned()
        {
            Active.Remove(this);
            live = false;
            Owner = null;
        }

        void Update()
        {
            if (!live) return;
            age += Time.deltaTime;
            if (age > lifetime) { Remove(); return; }
            if (!IsArmed) return;

            foreach (var t in CombatTarget.All)
            {
                if (t.isDecoy || !t.IsAlive) continue;
                if (!FactionUtil.AreHostile(OwnerFaction, t.faction)) continue;
                if (Vector3.Distance(t.transform.position, transform.position) > triggerRadius + t.radius) continue;
                Trigger(t);
                return;
            }
        }

        /// <summary>Springs the trap on a target (also used by Coil Grab pulling an enemy into it).</summary>
        public void Trigger(CombatTarget victim)
        {
            if (!live) return;
            victim.Status.ApplyRoot(rootDuration);
            var info = new DamageInfo
            {
                Amount = damage,
                Attacker = Owner,
                Point = transform.position,
                Direction = Vector3.up,
                Flags = HitFlags.Unblockable | HitFlags.Stagger,
                StaggerDuration = rootDuration,
                AttackName = "Snare",
            };
            victim.ReceiveHit(info);
            AudioPool.Instance?.PlayCue(PlaceholderCue.Block, transform.position);
            Remove();
        }

        public void Remove()
        {
            live = false;
            PoolManager.Instance.Despawn(gameObject);
        }
    }
}
