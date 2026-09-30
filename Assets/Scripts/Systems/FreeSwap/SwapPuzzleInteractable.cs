// Implements: 06 M5.3 flow — base for obstacles that need a specific character ("Requires Rumiñahui / Chaska / Atoc").
// Wrong character gets a hint prompt instead of a failure (the mission has no fail state).
using UnityEngine;

namespace Ruminahui
{
    public abstract class SwapPuzzleInteractable : MonoBehaviour, IInteractable
    {
        public CharacterId requiredCharacter;
        public float range = 2.5f;
        public float holdDuration;
        public string actionText = "Interact";
        public bool completed;

        public Vector3 Position => transform.position;
        public float Range => range;
        public PromptStyle Style => PromptStyle.Normal;
        public float HoldDuration => holdDuration;
        public bool OverridesKitContext => true;

        public event System.Action<SwapPuzzleInteractable> Completed;

        protected virtual void OnEnable() => InteractableRegistry.All.Add(this);
        protected virtual void OnDisable() => InteractableRegistry.All.Remove(this);

        public virtual string PromptFor(PlayerCharacter c)
        {
            if (completed || !IsAvailable) return null;
            if (c.id != requiredCharacter) return $"Needs {CharacterFactory.NameFor(requiredCharacter)}  (swap: 1/2/3 or LB/RB)";
            return holdDuration > 0f ? $"Hold: {actionText}" : actionText;
        }

        public bool CanInteract(PlayerCharacter c) => !completed && IsAvailable && c.id == requiredCharacter;

        /// <summary>Gate for sequencing (e.g. station only usable once the regroup happened).</summary>
        public virtual bool IsAvailable => true;

        public void Interact(PlayerCharacter c)
        {
            if (!CanInteract(c)) return;
            completed = true;
            OnSolved(c);
            Completed?.Invoke(this);
        }

        protected abstract void OnSolved(PlayerCharacter c);
    }
}
