// Implements: 03-combat-design.md Section 0 (HP restored only at huacas — the checkpoint/shrine system) and
// 05 Section 3 fail-state handling (a mission can install its own respawn handler, e.g. M3.5 section restarts).
using System.Collections;
using UnityEngine;

namespace Ruminahui
{
    public class CheckpointService : Singleton<CheckpointService>
    {
        public Vector3 RespawnPosition { get; private set; }
        public Quaternion RespawnRotation { get; private set; } = Quaternion.identity;
        public bool HasCheckpoint { get; private set; }

        /// <summary>Raised after a respawn so encounters can reset their current wave.</summary>
        public event System.Action Respawned;

        /// <summary>Mission-specific override (M3.5 section restart). Return an IEnumerator to run instead of the default.</summary>
        public System.Func<IEnumerator> CustomRespawn;

        bool respawning;

        void Start()
        {
            if (PartyManager.Instance != null) PartyManager.Instance.ControlledDied += OnControlledDied;
        }

        protected override void OnDestroy()
        {
            if (PartyManager.Instance != null) PartyManager.Instance.ControlledDied -= OnControlledDied;
            base.OnDestroy();
        }

        public void SetCheckpoint(Vector3 position, Quaternion rotation)
        {
            RespawnPosition = position;
            RespawnRotation = rotation;
            HasCheckpoint = true;
        }

        public void ClearForNewScene()
        {
            HasCheckpoint = false;
            CustomRespawn = null;
            respawning = false;
        }

        void OnControlledDied(PlayerCharacter c)
        {
            if (!respawning) StartCoroutine(RespawnRoutine());
        }

        public void RestartFromCheckpoint()
        {
            if (!respawning) StartCoroutine(RespawnRoutine());
        }

        IEnumerator RespawnRoutine()
        {
            respawning = true;
            yield return new WaitForSeconds(1.2f);
            var fader = ScreenFader.Instance;
            if (fader != null) yield return fader.FadeOut(0.4f, Color.black);

            if (CustomRespawn != null)
            {
                yield return CustomRespawn();
            }
            else
            {
                PoolManager.Instance.DespawnAll();
                var pm = PartyManager.Instance;
                var c = pm != null ? pm.Controlled : null;
                if (c != null)
                {
                    var pos = HasCheckpoint ? RespawnPosition : c.transform.position;
                    c.RespawnAt(pos, HasCheckpoint ? RespawnRotation : c.transform.rotation);
                    foreach (var m in pm.Members)
                        if (m != null && m != c) m.RespawnAt(pos + Random.insideUnitSphere.WithY(0f) * 2f, RespawnRotation);
                }
                Respawned?.Invoke();
            }

            if (fader != null) yield return fader.FadeIn(0.4f);
            respawning = false;
        }

        public void RaiseRespawned() => Respawned?.Invoke();
    }

    public static class VectorExtensions
    {
        public static Vector3 WithY(this Vector3 v, float y) => new Vector3(v.x, y, v.z);
    }
}
