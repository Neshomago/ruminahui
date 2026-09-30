// Implements: 11-camera-performance.md, Part A Step 3 (lock-on: nearest enemy in a forward cone, or nearest marked enemy,
// drives VCam_Combat_LockOn) and Step 3.3 (same input as Kuntur's Condor's Eye).
using UnityEngine;

namespace Ruminahui
{
    public class TargetingSystem : MonoBehaviour
    {
        public float lockRange = 20f;
        public float lockConeHalfAngle = 45f;
        public float breakRange = 28f;

        public CombatTarget Locked { get; private set; }
        CombatTarget self;
        PlayerCharacter character;

        void Awake()
        {
            self = GetComponent<CombatTarget>();
            character = GetComponent<PlayerCharacter>();
        }

        /// <summary>Press: lock the best target, or cycle to the next one, or release if nothing is in view.</summary>
        public void ToggleOrCycle(Vector3 viewForward)
        {
            // Prefer marked targets (Condor's Eye) when choosing.
            var next = CombatQuery.BestInCone(self, transform.position, viewForward, lockRange, lockConeHalfAngle, Locked);
            var markedPick = CombatQuery.Nearest(self, transform.position, lockRange, t => t.Status.IsMarked && t != Locked && !t.isDecoy);
            if (markedPick != null && (next == null || Vector3.Distance(markedPick.transform.position, transform.position) < lockRange * 0.6f)) next = markedPick;

            if (next == null)
            {
                if (Locked != null && Locked.IsTargetable) return; // only target in view — keep it
                ClearLock();
                return;
            }
            SetLock(next);
        }

        public void SetLock(CombatTarget t)
        {
            Locked = t;
            if (character != null && character.Cameras != null) character.Cameras.SetLockTarget(t != null ? t.transform : null);
        }

        public void ClearLock() => SetLock(null);

        void Update()
        {
            if (Locked == null) return;
            if (!Locked.IsTargetable || Vector3.Distance(Locked.transform.position, transform.position) > breakRange) ClearLock();
        }
    }
}
