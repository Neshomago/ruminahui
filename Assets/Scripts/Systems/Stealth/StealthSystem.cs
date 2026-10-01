// Implements: 02 M0.1 "forced-stealth sequence… move from cover to cover toward the designated safe point" and M5.5 "tense
// stealth/evasion attempt". One shared detection meter (max over watchers); hiding spots make the player unseen.
using System.Collections.Generic;
using UnityEngine;

namespace Ruminahui
{
    public class StealthSystem : Singleton<StealthSystem>
    {
        public bool Active { get; private set; }
        public float Detection { get; private set; }
        public bool PlayerHidden { get; private set; }
        public bool Spotted { get; private set; }

        /// <summary>Raised once when the meter fills. Directors decide what "caught" means (M0.1 retry, M5.5 the inevitable).</summary>
        public event System.Action SpottedEvent;

        public readonly List<StealthWatcher> Watchers = new List<StealthWatcher>();
        public readonly List<HidingSpot> Spots = new List<HidingSpot>();

        public void Begin()
        {
            Active = true;
            ResetMeter();
        }

        public void End()
        {
            Active = false;
            ResetMeter();
        }

        public void ResetMeter()
        {
            Detection = 0f;
            Spotted = false;
            foreach (var w in Watchers) if (w != null) w.Detection = 0f;
        }

        void Update()
        {
            if (!Active) return;
            var c = PartyManager.Instance != null ? PartyManager.Instance.Controlled : null;
            if (c == null) return;

            PlayerHidden = false;
            foreach (var s in Spots) if (s != null && s.Contains(c.transform.position)) { PlayerHidden = true; break; }

            float max = 0f;
            foreach (var w in Watchers)
            {
                if (w == null || !w.isActiveAndEnabled) continue;
                w.Evaluate(c, PlayerHidden, Time.deltaTime);
                if (w.Detection > max) max = w.Detection;
            }
            Detection = max;

            if (!Spotted && Detection >= 1f)
            {
                Spotted = true;
                AudioPool.Instance?.PlayCue(PlaceholderCue.Spotted, c.transform.position, 0.6f);
                SpottedEvent?.Invoke();
            }
        }

        public void ClearScene()
        {
            End();
            Watchers.Clear();
            Spots.Clear();
            SpottedEvent = null;
        }
    }
}
