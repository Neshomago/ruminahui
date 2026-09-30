// Implements: the Interact input for non-combat verbs (huacas, M5.3 obstacles, the M3.4 spare prompt), incl. hold-to-interact.
using UnityEngine;

namespace Ruminahui
{
    public class InteractionSensor : MonoBehaviour
    {
        PlayerCharacter character;
        IInteractable holding;
        float holdProgress;

        public IInteractable Current { get; private set; }
        public float HoldProgress01 => holding != null && holding.HoldDuration > 0f ? holdProgress / holding.HoldDuration : 0f;
        public bool IsHolding => holding != null;

        void Awake() => character = GetComponent<PlayerCharacter>();

        void Update()
        {
            Current = FindBest();
            if (holding != null && (Current != holding || !holding.CanInteract(character))) CancelHold();
        }

        IInteractable FindBest()
        {
            IInteractable best = null;
            float bestD = float.MaxValue;
            foreach (var i in InteractableRegistry.All)
            {
                if (i == null) continue;
                float d = Vector3.Distance(transform.position, i.Position);
                if (d > i.Range || d >= bestD) continue;
                if (string.IsNullOrEmpty(i.PromptFor(character))) continue;
                best = i;
                bestD = d;
            }
            return best;
        }

        /// <summary>Called on press. Returns true if something was interacted with (or a hold started).</summary>
        public bool Press()
        {
            if (Current == null || !Current.CanInteract(character)) return false;
            if (Current.HoldDuration <= 0f)
            {
                Current.Interact(character);
                return true;
            }
            holding = Current;
            holdProgress = 0f;
            return true;
        }

        /// <summary>Called every frame the button is held.</summary>
        public void Hold(float dt)
        {
            if (holding == null) return;
            holdProgress += dt;
            if (holdProgress >= holding.HoldDuration)
            {
                var h = holding;
                CancelHold();
                h.Interact(character);
            }
        }

        public void CancelHold()
        {
            holding = null;
            holdProgress = 0f;
        }
    }
}
