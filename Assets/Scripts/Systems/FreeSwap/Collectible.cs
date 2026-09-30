// Implements: 06-m5-3-flow-and-enemy-roster.md, M5.3 Section 3 — 2-3 optional lore/collectible items.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ruminahui
{
    public class Collectible : MonoBehaviour, IInteractable
    {
        public string id = "relic";
        public string title = "Willka-era relic";
        bool taken;

        public Vector3 Position => transform.position;
        public float Range => 2f;
        public PromptStyle Style => PromptStyle.Normal;
        public float HoldDuration => 0f;
        public bool OverridesKitContext => false;

        void OnEnable() => InteractableRegistry.All.Add(this);
        void OnDisable() => InteractableRegistry.All.Remove(this);

        public string PromptFor(PlayerCharacter c) => taken ? null : $"Take: {title}";
        public bool CanInteract(PlayerCharacter c) => !taken;

        public void Interact(PlayerCharacter c)
        {
            taken = true;
            Progression.Collect(id);
            AudioPool.Instance?.PlayCue(PlaceholderCue.Pickup, transform.position);
            ObjectiveTracker.Say("Relic", $"{title} — found ({Progression.CollectedCount})");
            gameObject.SetActive(false);
        }
    }
}
