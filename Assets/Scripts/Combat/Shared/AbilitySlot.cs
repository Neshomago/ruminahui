// Implements: 03-combat-design.md, "Special Abilities (Focus-gated, unlocked across Acts)" and Ultimates — cost, cooldown, unlock flag.
using UnityEngine;

namespace Ruminahui
{
    public class AbilitySlot
    {
        public readonly string Id;
        public readonly string DisplayName;
        public float FocusCost;
        public float Cooldown;
        public readonly string UnlockFlag;
        public readonly bool IsUltimate;
        float readyAt;

        public AbilitySlot(string id, string displayName, float focusCost, float cooldown, string unlockFlag, bool isUltimate = false)
        {
            Id = id;
            DisplayName = displayName;
            FocusCost = focusCost;
            Cooldown = cooldown;
            UnlockFlag = unlockFlag;
            IsUltimate = isUltimate;
        }

        public bool Unlocked => Progression.IsUnlocked(UnlockFlag);
        public float CooldownRemaining => Mathf.Max(0f, readyAt - Time.time);
        public float CooldownNormalized => Cooldown <= 0f ? 0f : CooldownRemaining / Cooldown;
        public bool OffCooldown => CooldownRemaining <= 0f;

        public bool CanUse(FocusPool focus, bool free) => Unlocked && OffCooldown && (free || focus.Has(FocusCost));

        /// <summary>Spends focus (unless free — AI allies don't drain the player's shared meter) and starts the cooldown.</summary>
        public bool TryConsume(FocusPool focus, bool free)
        {
            if (!CanUse(focus, free)) return false;
            if (!free && !focus.TrySpend(FocusCost)) return false;
            readyAt = Time.time + Cooldown;
            return true;
        }

        public void ResetCooldown() => readyAt = 0f;
    }
}
