// Implements: 11-camera-performance.md, Part B Step 3 — one scene per mission, loaded ADDITIVELY through Addressables; only the
// current mission stays resident. Falls back to Build Settings loading if a scene isn't marked Addressable yet.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Ruminahui
{
    public class SceneStreamer : Singleton<SceneStreamer>
    {
        public bool IsLoading { get; private set; }
        public string CurrentKey { get; private set; }

        AsyncOperationHandle<SceneInstance> currentHandle;
        bool currentIsAddressable;

        /// <summary>Loads <paramref name="key"/> additively, makes it active, then unloads the previous mission scene.</summary>
        public IEnumerator LoadExclusive(string key)
        {
            if (IsLoading) yield break;
            IsLoading = true;

            Scene previous = SceneManager.GetActiveScene();
            var previousHandle = currentHandle;
            bool previousWasAddressable = currentIsAddressable;

            bool addressable = false;
            var locHandle = Addressables.LoadResourceLocationsAsync(key, typeof(SceneInstance));
            yield return locHandle;
            if (locHandle.Status == AsyncOperationStatus.Succeeded && locHandle.Result != null && locHandle.Result.Count > 0) addressable = true;
            Addressables.Release(locHandle);

            Scene loaded = default;
            if (addressable)
            {
                var h = Addressables.LoadSceneAsync(key, LoadSceneMode.Additive);
                yield return h;
                if (h.Status != AsyncOperationStatus.Succeeded)
                {
                    Debug.LogError($"[SceneStreamer] Addressables failed to load '{key}'.");
                    IsLoading = false;
                    yield break;
                }
                currentHandle = h;
                currentIsAddressable = true;
                loaded = h.Result.Scene;
            }
            else
            {
                if (!Application.CanStreamedLevelBeLoaded(key))
                {
                    Debug.LogError($"[SceneStreamer] Scene '{key}' is neither Addressable nor in Build Settings. Run Rumiñahui ▸ 2. Build Scenes.");
                    IsLoading = false;
                    yield break;
                }
                var op = SceneManager.LoadSceneAsync(key, LoadSceneMode.Additive);
                yield return op;
                loaded = SceneManager.GetSceneByName(key);
                currentHandle = default;
                currentIsAddressable = false;
                Debug.LogWarning($"[SceneStreamer] '{key}' loaded via Build Settings fallback (not Addressable).");
            }

            if (loaded.IsValid()) SceneManager.SetActiveScene(loaded);
            CurrentKey = key;

            // Unload the previous mission (never the DontDestroyOnLoad systems).
            if (previousWasAddressable && previousHandle.IsValid())
            {
                var u = Addressables.UnloadSceneAsync(previousHandle);
                yield return u;
            }
            else if (previous.IsValid() && previous.isLoaded && previous != loaded)
            {
                var u = SceneManager.UnloadSceneAsync(previous);
                if (u != null) yield return u;
            }

            yield return Resources.UnloadUnusedAssets();
            IsLoading = false;
        }
    }
}
