// Implements: AI_BUILD_PROMPT.md, Build Order step 0 (project foundations);
//             11-camera-performance.md, Part B Steps 2-3 (pooling + streaming live in one persistent root).
// Every scene (mission or test) can be played directly: the persistent systems are created before the first scene loads.
using UnityEngine;

namespace Ruminahui
{
    public static class GameBootstrap
    {
        public static GameObject SystemsRoot { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
            if (SystemsRoot != null) return;

            SystemsRoot = new GameObject("[Systems]");
            Object.DontDestroyOnLoad(SystemsRoot);

            // Order matters: later systems look up earlier ones in their Awake.
            SystemsRoot.AddComponent<PoolManager>();
            SystemsRoot.AddComponent<AudioPool>();
            SystemsRoot.AddComponent<GameInput>();
            SystemsRoot.AddComponent<SceneStreamer>();
            SystemsRoot.AddComponent<PartyManager>();
            SystemsRoot.AddComponent<CameraDirector>();
            SystemsRoot.AddComponent<CheckpointService>();
            SystemsRoot.AddComponent<MissionManager>();
            SystemsRoot.AddComponent<FreeSwapController>();
            SystemsRoot.AddComponent<UIRoot>();

            EnemyFactory.RegisterPools();
            CombatPools.RegisterPools();
        }
    }
}
