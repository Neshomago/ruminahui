// Implements: 11-camera-performance.md, Part B Step 2 — everything spawned repeatedly (enemies, projectiles, VFX,
// UI pop-ups) goes through this pool. Pools are keyed by string so code-built placeholder objects and real prefabs share one path.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ruminahui
{
    public class PoolManager : Singleton<PoolManager>
    {
        readonly Dictionary<string, System.Func<GameObject>> factories = new Dictionary<string, System.Func<GameObject>>();
        readonly Dictionary<string, Stack<GameObject>> free = new Dictionary<string, Stack<GameObject>>();
        readonly List<PooledObject> spawned = new List<PooledObject>();
        Transform poolRoot;

        protected override void Awake()
        {
            base.Awake();
            poolRoot = new GameObject("[Pool]").transform;
            poolRoot.SetParent(transform, false);
        }

        public bool IsRegistered(string key) => factories.ContainsKey(key);

        /// <summary>Registers a factory. The factory should return an object (active or inactive) — the pool deactivates it.</summary>
        public void Register(string key, System.Func<GameObject> factory, int prewarm = 0)
        {
            factories[key] = factory;
            if (!free.ContainsKey(key)) free[key] = new Stack<GameObject>();
            for (int i = 0; i < prewarm; i++) free[key].Push(Create(key));
        }

        GameObject Create(string key)
        {
            var go = factories[key]();
            go.SetActive(false);
            var po = go.GetComponent<PooledObject>();
            if (po == null) po = go.AddComponent<PooledObject>();
            po.PoolKey = key;
            if (po.ReturnToPoolRoot) go.transform.SetParent(poolRoot, false);
            return go;
        }

        public GameObject Spawn(string key, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (!factories.ContainsKey(key))
            {
                Debug.LogError($"[PoolManager] No pool registered for key '{key}'.");
                return null;
            }

            var stack = free[key];
            GameObject go = null;
            while (stack.Count > 0 && go == null) go = stack.Pop(); // skip entries destroyed externally
            if (go == null) go = Create(key);

            if (parent != null) go.transform.SetParent(parent, false);
            // Position while inactive: a CharacterController would otherwise overwrite a teleport (see LESSONS_LEARNED.md).
            go.transform.SetPositionAndRotation(position, rotation);

            var po = go.GetComponent<PooledObject>();
            po.IsSpawned = true;
            spawned.Add(po);
            go.SetActive(true);

            foreach (var p in go.GetComponentsInChildren<IPoolable>(true)) p.OnSpawned();
            return go;
        }

        public T Spawn<T>(string key, Vector3 position, Quaternion rotation, Transform parent = null) where T : Component
        {
            var go = Spawn(key, position, rotation, parent);
            return go != null ? go.GetComponent<T>() : null;
        }

        /// <summary>Prefab overload: real art prefabs can use the same pool path later.</summary>
        public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            string key = "prefab:" + prefab.GetInstanceID();
            if (!IsRegistered(key)) Register(key, () => Instantiate(prefab));
            return Spawn(key, position, rotation);
        }

        public void Despawn(GameObject go, float delay = 0f)
        {
            if (go == null) return;
            if (delay > 0f) { StartCoroutine(DespawnLater(go, delay)); return; }

            var po = go.GetComponent<PooledObject>();
            if (po == null || !factories.ContainsKey(po.PoolKey))
            {
                Destroy(go); // not pool-owned
                return;
            }
            if (!po.IsSpawned) return; // double-despawn guard

            foreach (var p in go.GetComponentsInChildren<IPoolable>(true)) p.OnDespawned();
            po.IsSpawned = false;
            spawned.Remove(po);
            go.SetActive(false);
            if (po.ReturnToPoolRoot) go.transform.SetParent(poolRoot, false);
            free[po.PoolKey].Push(go);
        }

        IEnumerator DespawnLater(GameObject go, float delay)
        {
            yield return new WaitForSeconds(delay);
            Despawn(go);
        }

        /// <summary>Called on mission change so pooled enemies/projectiles never leak between scenes.</summary>
        public void DespawnAll(bool includeUI = false)
        {
            StopAllCoroutines();
            var copy = new List<PooledObject>(spawned);
            foreach (var po in copy)
            {
                if (po == null) continue;
                if (!includeUI && !po.ReturnToPoolRoot) continue;
                Despawn(po.gameObject);
            }
        }
    }
}
