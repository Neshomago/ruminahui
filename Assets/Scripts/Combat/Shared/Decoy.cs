// Implements: 03-combat-design.md, Section 3 (Shed Skin decoy afterimage — enemies mid-attack can be baited into hitting it);
// 06 Enemy Roster (Arquebusier baited by decoys); 08-atoc-boss-fight.md Phase 2 (Atoc's decoys lack his idle bob).
using UnityEngine;

namespace Ruminahui
{
    [RequireComponent(typeof(CombatTarget))]
    public class Decoy : MonoBehaviour, IPoolable, IHitInterceptor
    {
        public const string PoolKey = "Combat/Decoy";
        public float staminaPenaltyForAttacker = 8f; // PLACEHOLDER-BALANCE: "button-mashing at decoys wastes Stamina" (08 Phase 2)

        CombatTarget self;
        PlaceholderVisual visual;
        float expireAt;
        bool live;

        public int InterceptPriority => 1000;

        void Awake()
        {
            self = GetComponent<CombatTarget>();
            visual = GetComponent<PlaceholderVisual>();
        }

        void OnEnable() => self.RegisterInterceptor(this);
        void OnDisable() => self.UnregisterInterceptor(this);

        public static Decoy Spawn(CombatTarget owner, Vector3 position, Quaternion rotation, float duration, Color color)
        {
            if (PoolManager.Instance == null) return null;
            var d = PoolManager.Instance.Spawn<Decoy>(PoolKey, position, rotation);
            if (d == null) return null;
            d.self.faction = owner.faction;
            d.expireAt = Time.time + duration;
            d.live = true;
            if (d.visual != null) d.visual.SetBaseColor(color);
            return d;
        }

        public void OnSpawned()
        {
            self.Health.ResetFull();
            self.isDecoy = true;
            self.aggroPriority = 10;
        }

        public void OnDespawned() => live = false;

        public bool Intercept(ref DamageInfo info, out HitResult result)
        {
            result = new HitResult(HitOutcome.Absorbed);
            if (info.Attacker != null)
            {
                var sta = info.Attacker.GetComponent<Stamina>();
                if (sta != null) sta.Drain(staminaPenaltyForAttacker);
            }
            Pop();
            return true;
        }

        void Update()
        {
            if (live && Time.time >= expireAt) Pop();
        }

        void Pop()
        {
            if (!live) return;
            live = false;
            PoolManager.Instance.Despawn(gameObject);
        }
    }
}
