// Implements: 06-m5-3-flow-and-enemy-roster.md, M5.3 Obstacle 3 "The Narrow Dark" — old rigged deadfalls; Atoc's trap-sense reads and disarms them.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ruminahui
{
    public class RiggedDeadfall : SwapPuzzleInteractable
    {
        public float triggerRadius = 1.3f;
        public float senseRadius = 6f;
        public Transform resetPoint;
        public PlaceholderVisual marker;
        bool revealed;
        float rearmAt;

        void Reset() { requiredCharacter = CharacterId.Atoc; holdDuration = 1f; actionText = "Disarm the deadfall"; }

        public override string PromptFor(PlayerCharacter c)
        {
            if (!revealed) return null; // unseen until trap-sense reads it
            return base.PromptFor(c);
        }

        void Update()
        {
            if (completed) return;
            var pc = PartyManager.Instance != null ? PartyManager.Instance.Controlled : null;
            if (pc == null) return;
            float d = Vector3.Distance(pc.transform.position, transform.position);

            // Amaru trap-sense: Atoc sees them from a distance.
            bool sense = pc.id == CharacterId.Atoc && d <= senseRadius;
            if (sense != revealed)
            {
                revealed = sense;
                if (marker != null) marker.SetVisible(revealed);
            }

            // Anyone else stepping in springs it: a recoverable stumble, never damage (no fail state).
            if (pc.id != CharacterId.Atoc && d <= triggerRadius && Time.time >= rearmAt)
            {
                rearmAt = Time.time + 2f;
                ObjectiveTracker.Say(pc.displayName, "(a deadfall drops — stumbles back to the entrance)");
                if (marker != null) { marker.SetVisible(true); marker.Flash(Color.red, 0.6f, false); }
                if (resetPoint != null) pc.Motor.Teleport(resetPoint.position, resetPoint.rotation);
            }
        }

        protected override void OnSolved(PlayerCharacter c)
        {
            if (marker != null) { marker.SetVisible(true); marker.SetBaseColor(new Color(0.3f, 0.3f, 0.3f)); }
            ObjectiveTracker.Say("Atoc", "(disarms the old deadfall)");
        }
    }
}
