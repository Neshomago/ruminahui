// Implements: 03-combat-design.md Section 0 — "Regenerates only at huacas (checkpoint/shrine system)".
using UnityEngine;

namespace Ruminahui
{
    public class Huaca : MonoBehaviour, IInteractable
    {
        public float range = 2.5f;

        public Vector3 Position => transform.position;
        public float Range => range;
        public PromptStyle Style => PromptStyle.Normal;
        public float HoldDuration => 0f;
        public bool OverridesKitContext => false;

        void OnEnable() => InteractableRegistry.All.Add(this);
        void OnDisable() => InteractableRegistry.All.Remove(this);

        public string PromptFor(PlayerCharacter c) => "Rest at the huaca";
        public bool CanInteract(PlayerCharacter c) => true;

        public void Interact(PlayerCharacter c)
        {
            var pm = PartyManager.Instance;
            if (pm != null)
                foreach (var m in pm.Members)
                    if (m != null) { m.Health.ResetFull(); m.Stamina.ResetFull(); }
            CheckpointService.Instance?.SetCheckpoint(transform.position + transform.forward * 2f, transform.rotation);
            AudioPool.Instance?.PlayCue(PlaceholderCue.Pickup, transform.position);
            ObjectiveTracker.Say("Huaca", "HP restored. Checkpoint set. Progress saved.");
            var mm = MissionManager.Instance;
            if (SaveSystem.Instance != null && mm != null && mm.Current != null) SaveSystem.Instance.Save(mm.Current.Id);
            UpgradeMenuUI.Instance?.Open(true); // approved: the upgrade menu opens from pause and at huacas
        }
    }
}
