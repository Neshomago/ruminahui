// Implements: 11-camera-performance.md, Part B Step 2 — registers the pooled combat objects (projectiles, snares, decoys, hit VFX).
using UnityEngine;

namespace Ruminahui
{
    public static class CombatPools
    {
        public const string HitSparkKey = "VFX/HitSpark";

        public static void RegisterPools()
        {
            var pm = PoolManager.Instance;
            if (pm == null) return;

            pm.Register(Projectile.PoolKey, () =>
            {
                var go = NewInactive("Projectile");
                var v = go.AddComponent<PlaceholderVisual>();
                v.shape = PlaceholderShape.Sphere;
                v.scale = Vector3.one * 0.25f;
                v.offset = Vector3.zero;
                v.addFacingNose = false;
                go.AddComponent<Projectile>();
                return go;
            }, 12);

            pm.Register(Snare.PoolKey, () =>
            {
                var go = NewInactive("Snare");
                var v = go.AddComponent<PlaceholderVisual>();
                v.shape = PlaceholderShape.Cylinder;
                v.scale = new Vector3(1.4f, 0.05f, 1.4f);
                v.offset = new Vector3(0f, 0.05f, 0f);
                v.addFacingNose = false;
                go.AddComponent<Snare>();
                return go;
            }, 4);

            pm.Register(Decoy.PoolKey, () =>
            {
                var go = NewInactive("Decoy");
                var v = go.AddComponent<PlaceholderVisual>();
                v.shape = PlaceholderShape.Capsule;
                v.idleBob = false; // the Phase 2 "tell within a tell"
                go.AddComponent<Health>().max = 1f;
                go.AddComponent<StatusEffects>();
                go.AddComponent<CombatTarget>();
                go.AddComponent<Decoy>();
                return go;
            }, 3);

            pm.Register(HitSparkKey, () =>
            {
                var go = NewInactive("HitSpark");
                var v = go.AddComponent<PlaceholderVisual>();
                v.shape = PlaceholderShape.Sphere;
                v.scale = Vector3.one * 0.3f;
                v.offset = Vector3.zero;
                v.addFacingNose = false;
                v.baseColor = new Color(1f, 0.9f, 0.5f);
                go.AddComponent<TimedDespawn>().lifetime = 0.15f;
                return go;
            }, 6);
        }

        static GameObject NewInactive(string name)
        {
            var go = new GameObject(name);
            go.SetActive(false); // configure before Awake runs
            return go;
        }

        public static void Spark(Vector3 position)
        {
            if (PoolManager.Instance != null) PoolManager.Instance.Spawn(HitSparkKey, position, Quaternion.identity);
        }
    }

}
