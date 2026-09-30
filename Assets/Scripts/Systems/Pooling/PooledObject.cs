// Implements: 11-camera-performance.md, Part B Step 2 (object pooling from day one).
using UnityEngine;

namespace Ruminahui
{
    /// <summary>Added automatically by PoolManager. Marks an object as pool-owned.</summary>
    public class PooledObject : MonoBehaviour
    {
        public string PoolKey;
        public bool IsSpawned;
        /// <summary>UI pop-ups keep their canvas parent; world objects return under the pool root.</summary>
        public bool ReturnToPoolRoot = true;

        public void Despawn(float delay = 0f)
        {
            if (PoolManager.Instance != null) PoolManager.Instance.Despawn(gameObject, delay);
            else gameObject.SetActive(false);
        }
    }
}
