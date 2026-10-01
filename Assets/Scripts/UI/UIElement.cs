// Implements: approved UI plan — base for every placeholder UI element (built in code, swappable for prefabs later).
using UnityEngine;

namespace Ruminahui
{
    /// <summary>Base for every placeholder UI element.</summary>
    public abstract class UIElement : MonoBehaviour
    {
        protected RectTransform Root { get; private set; }

        public void Build(RectTransform root)
        {
            Root = root;
            OnBuild();
        }

        protected abstract void OnBuild();

        protected static PlayerCharacter Controlled => PartyManager.Instance != null ? PartyManager.Instance.Controlled : null;
    }

    /// <summary>One Esc press closes one menu: whoever handles it marks the frame so stacked menus don't all react.</summary>
    public static class UIEscape
    {
        static int consumedFrame = -1;
        public static bool ConsumedThisFrame => consumedFrame == Time.frameCount;
        public static void Consume() => consumedFrame = Time.frameCount;

        /// <summary>True if Esc was pressed this frame and nobody has handled it yet (and marks it handled).</summary>
        public static bool TryConsume(GameInput input)
        {
            if (input == null || ConsumedThisFrame || !input.Pause.WasPressedThisFrame()) return false;
            Consume();
            return true;
        }
    }
}
