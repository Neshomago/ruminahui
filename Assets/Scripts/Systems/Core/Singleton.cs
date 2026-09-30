// Implements: AI_BUILD_PROMPT.md, Build Order step 0 (project foundations) — shared singleton base for persistent systems.
using UnityEngine;

namespace Ruminahui
{
    /// <summary>Scene-agnostic singleton. Instances live on the [Systems] root created by GameBootstrap.</summary>
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        public static T Instance { get; private set; }

        protected virtual void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = (T)this;
        }

        protected virtual void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
