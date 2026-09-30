// Implements: 03-combat-design.md (Bolas Snap, Sky-Cut volley), 06 Enemy Roster (Skirmisher sling, Arquebusier shot,
// Lieutenant sling) — one pooled projectile type (11-camera-performance.md Part B Step 2).
using UnityEngine;

namespace Ruminahui
{
    public class Projectile : MonoBehaviour, IPoolable
    {
        public float hitRadius = 0.35f;
        public float lifetime = 4f;

        CombatTarget owner;
        CombatTarget homingTarget;
        DamageInfo payload;
        Vector3 velocity;
        float speed;
        float age;
        bool live;
        System.Action<CombatTarget, HitResult> onHit;
        PlaceholderVisual visual;

        public const string PoolKey = "Combat/Projectile";

        void Awake() => visual = GetComponent<PlaceholderVisual>();

        public static Projectile Fire(CombatTarget owner, Vector3 from, Vector3 direction, float speed, DamageInfo payload,
            Color color, CombatTarget homing = null, System.Action<CombatTarget, HitResult> onHit = null, float scale = 0.25f)
        {
            if (PoolManager.Instance == null) return null;
            var p = PoolManager.Instance.Spawn<Projectile>(PoolKey, from, Quaternion.LookRotation(direction.sqrMagnitude > 0.001f ? direction : Vector3.forward));
            if (p == null) return null;
            p.owner = owner;
            p.speed = speed;
            p.velocity = direction.normalized * speed;
            p.payload = payload;
            p.payload.Attacker = owner;
            p.homingTarget = homing;
            p.onHit = onHit;
            p.live = true;
            p.age = 0f;
            p.transform.localScale = Vector3.one * (scale / 0.25f);
            if (p.visual != null) p.visual.SetBaseColor(color);
            return p;
        }

        public void OnSpawned() { age = 0f; }

        public void OnDespawned()
        {
            live = false;
            owner = null;
            homingTarget = null;
            onHit = null;
        }

        void Update()
        {
            if (!live) return;
            float dt = TimeDilation.DeltaFor(owner != null ? owner.gameObject : null);
            age += dt;
            if (age > lifetime || owner == null) { Despawn(); return; }

            if (homingTarget != null && homingTarget.IsTargetable)
            {
                var desired = (homingTarget.AimPoint - transform.position).normalized * speed;
                velocity = Vector3.RotateTowards(velocity, desired, 8f * dt, 0f);
            }

            var step = velocity * dt;
            // Environment blocks projectiles; characters are resolved by distance below.
            if (Physics.Raycast(transform.position, step.normalized, out var hit, step.magnitude + 0.05f, ~0, QueryTriggerInteraction.Ignore)
                && hit.collider.GetComponentInParent<CombatTarget>() == null)
            {
                Despawn();
                return;
            }
            transform.position += step;

            foreach (var t in CombatTarget.All)
            {
                if (!CombatQuery.IsHostileTo(owner, t)) continue;
                if ((t.AimPoint - transform.position).magnitude > hitRadius + t.radius + 0.4f) continue;
                var info = payload;
                info.Point = transform.position;
                info.Direction = velocity.normalized;
                var result = t.ReceiveHit(info);
                onHit?.Invoke(t, result);
                Despawn();
                return;
            }
        }

        void Despawn()
        {
            live = false;
            PoolManager.Instance.Despawn(gameObject);
        }
    }
}
