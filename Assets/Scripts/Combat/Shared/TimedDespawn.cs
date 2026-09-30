// Implements: 11-camera-performance.md, Part B Step 2 — returns pooled one-shots (VFX, smoke puffs) to the pool after a delay.
using UnityEngine;

namespace Ruminahui
{
    /// <summary>Returns a pooled one-shot (VFX, smoke puffs) to the pool after a delay.</summary>
    public class TimedDespawn : MonoBehaviour, IPoolable
    {
        public float lifetime = 0.5f;
        float t;

        public void OnSpawned() => t = 0f;
        public void OnDespawned() { }

        void Update()
        {
            t += Time.deltaTime;
            if (t >= lifetime && PoolManager.Instance != null) PoolManager.Instance.Despawn(gameObject);
        }
    }
}
