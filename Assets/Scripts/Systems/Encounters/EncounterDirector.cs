// Implements: 06 Enemy Roster (encounter composition) and 02 Mission List gameplay notes (waves, gauntlets, sub-encounters);
// enemies spawn from the pool (11-camera-performance.md Part B Step 2).
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ruminahui
{
    [System.Serializable]
    public class SpawnEntry
    {
        public EnemyType type;
        public Vector3 localPosition;
        public float yRotation;

        public SpawnEntry(EnemyType type, Vector3 localPosition, float yRotation = 180f)
        {
            this.type = type;
            this.localPosition = localPosition;
            this.yRotation = yRotation;
        }
    }

    [System.Serializable]
    public class Wave
    {
        public string name = "Wave";
        public float delayBefore = 1f;
        public List<SpawnEntry> spawns = new List<SpawnEntry>();

        public Wave(string name, params SpawnEntry[] entries)
        {
            this.name = name;
            spawns.AddRange(entries);
        }
    }

    public class EncounterDirector : MonoBehaviour
    {
        public string encounterName = "Encounter";
        public List<Wave> waves = new List<Wave>();
        [Tooltip("Enemy damage multiplier for this encounter (M3.5 control-shift sections are slightly easier).")]
        public float damageScale = 1f;
        [Tooltip("0 = start when Begin() is called. Otherwise auto-starts when the controlled character comes this close.")]
        public float activationRadius;
        public bool listenForRespawn = true;

        public int CurrentWave { get; private set; } = -1;
        public bool IsRunning { get; private set; }
        public bool IsComplete { get; private set; }
        public int AliveCount => alive.Count;

        public event System.Action<int> WaveStarted;
        public event System.Action Completed;

        readonly List<EnemyBrain> alive = new List<EnemyBrain>();
        Coroutine routine;

        void Start()
        {
            if (listenForRespawn && CheckpointService.Instance != null) CheckpointService.Instance.Respawned += OnRespawned;
        }

        void OnDestroy()
        {
            if (CheckpointService.Instance != null) CheckpointService.Instance.Respawned -= OnRespawned;
        }

        void Update()
        {
            if (IsRunning || IsComplete || activationRadius <= 0f) return;
            var c = PartyManager.Instance != null ? PartyManager.Instance.Controlled : null;
            if (c != null && Vector3.Distance(c.transform.position, transform.position) <= activationRadius) Begin();
        }

        public void Begin()
        {
            if (IsRunning || IsComplete) return;
            IsRunning = true;
            routine = StartCoroutine(Run(0));
        }

        IEnumerator Run(int startWave)
        {
            for (int w = startWave; w < waves.Count; w++)
            {
                CurrentWave = w;
                yield return new WaitForSeconds(waves[w].delayBefore);
                SpawnWave(waves[w]);
                WaveStarted?.Invoke(w);
                Debug.Log($"[Encounter] {encounterName}: {waves[w].name} ({alive.Count} enemies)");
                while (alive.Count > 0) yield return null;
            }
            IsRunning = false;
            IsComplete = true;
            routine = null;
            Debug.Log($"[Encounter] {encounterName}: complete");
            Completed?.Invoke();
        }

        void SpawnWave(Wave wave)
        {
            foreach (var s in wave.spawns)
            {
                var pos = transform.TransformPoint(s.localPosition);
                var rot = transform.rotation * Quaternion.Euler(0f, s.yRotation, 0f);
                var e = EnemyFactory.Spawn(s.type, pos, rot);
                if (e == null) continue;
                e.encounterDamageScale = damageScale;
                alive.Add(e);
                e.Died += OnEnemyDied;
            }
        }

        void OnEnemyDied(EnemyBrain e)
        {
            e.Died -= OnEnemyDied;
            alive.Remove(e);
        }

        /// <summary>Restart the current wave (checkpoint respawn / M3.5 section restart).</summary>
        public void ResetCurrentWave()
        {
            if (!IsRunning) return;
            if (routine != null) StopCoroutine(routine);
            DespawnAlive();
            routine = StartCoroutine(Run(Mathf.Max(0, CurrentWave)));
        }

        /// <summary>Stop and remove everything (used when a scripted beat moves on).</summary>
        public void Abort()
        {
            if (routine != null) StopCoroutine(routine);
            routine = null;
            DespawnAlive();
            IsRunning = false;
        }

        public void ResetEncounter()
        {
            Abort();
            IsComplete = false;
            CurrentWave = -1;
        }

        void DespawnAlive()
        {
            foreach (var e in alive)
            {
                if (e == null) continue;
                e.Died -= OnEnemyDied;
                if (PoolManager.Instance != null) PoolManager.Instance.Despawn(e.gameObject);
            }
            alive.Clear();
        }

        void OnRespawned() => ResetCurrentWave();
    }
}
