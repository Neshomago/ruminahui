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
}
