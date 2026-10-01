// Implements: generic one-shot "do the thing" interactable for scripted mission beats (M0.1 "go find more wood", etc.).
using UnityEngine;

namespace Ruminahui
{
    public class ScriptedInteractable : MonoBehaviour, IInteractable
    {
        public string prompt = "Interact";
        public float range = 2.2f;
        public float holdDuration;
        public bool oneShot = true;
        public bool Used { get; private set; }
        public event System.Action Interacted;

        public Vector3 Position => transform.position;
        public float Range => range;
        public PromptStyle Style => PromptStyle.Normal;
        public float HoldDuration => holdDuration;
        public bool OverridesKitContext => true;

        void OnEnable() => InteractableRegistry.All.Add(this);
        void OnDisable() => InteractableRegistry.All.Remove(this);

        public string PromptFor(PlayerCharacter c) => oneShot && Used ? null : prompt;
        public bool CanInteract(PlayerCharacter c) => !(oneShot && Used);

        public void Interact(PlayerCharacter c)
        {
            if (!CanInteract(c)) return;
            Used = true;
            Interacted?.Invoke();
            if (oneShot) gameObject.SetActive(false);
        }
    }
}
