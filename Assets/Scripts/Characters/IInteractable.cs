// Implements: 08-atoc-boss-fight.md Section 2 (Spare prompt), 06 M5.3 obstacles (heave, line anchor, disarm), Huaca checkpoints.
using UnityEngine;

namespace Ruminahui
{
    public interface IInteractable
    {
        Vector3 Position { get; }
        float Range { get; }
        /// <summary>Prompt text for this character; return null to hide.</summary>
        string PromptFor(PlayerCharacter c);
        PromptStyle Style { get; }
        bool CanInteract(PlayerCharacter c);
        /// <summary>0 = instant. Otherwise the button must be held this long.</summary>
        float HoldDuration { get; }
        /// <summary>True when this should win over the kit's context action (e.g. Spare over Grounding Throw).</summary>
        bool OverridesKitContext { get; }
        void Interact(PlayerCharacter c);
    }

    /// <summary>Registry so the sensor doesn't need physics triggers (see LESSONS_LEARNED.md).</summary>
    public static class InteractableRegistry
    {
        public static readonly System.Collections.Generic.List<IInteractable> All = new System.Collections.Generic.List<IInteractable>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => All.Clear();
    }
}
