// STUB — compile-check only. Mirrors the Unity 6 UnityEngine API surface used by Assets/Scripts. Bodies are dummies.
#pragma warning disable
using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.Rendering
{
    public abstract class RenderPipelineAsset : ScriptableObject { }
}

namespace UnityEngine
{
    public class Object
    {
        public string name { get; set; }
        public int GetInstanceID() => 0;
        public static implicit operator bool(Object o) => !ReferenceEquals(o, null);
        public static bool operator ==(Object a, Object b) => ReferenceEquals(a, b);
        public static bool operator !=(Object a, Object b) => !ReferenceEquals(a, b);
        public override bool Equals(object o) => ReferenceEquals(this, o);
        public override int GetHashCode() => 0;
        public static T Instantiate<T>(T original) where T : Object => original;
        public static T Instantiate<T>(T original, Transform parent, bool worldPositionStays) where T : Object => original;
        public static T Instantiate<T>(T original, Transform parent) where T : Object => original;
        public static T Instantiate<T>(T original, Vector3 position, Quaternion rotation) where T : Object => original;
        public static void Destroy(Object obj, float t = 0f) { }
        public static void DestroyImmediate(Object obj, bool allowDestroyingAssets = false) { }
        public static void DontDestroyOnLoad(Object target) { }
        public static T FindFirstObjectByType<T>() where T : Object => null;
    }

    public class Component : Object
    {
        internal GameObject __stubOwner;
        public GameObject gameObject => __stubOwner;
        public Transform transform => __stubOwner != null ? __stubOwner.transform : null;
        public string tag { get; set; }
        public T GetComponent<T>() => __stubOwner != null ? __stubOwner.GetComponent<T>() : default;
        public T[] GetComponents<T>() => null;
        public T GetComponentInParent<T>() => default;
        public T GetComponentInChildren<T>() => default;
        public T[] GetComponentsInChildren<T>(bool includeInactive) => null;
        public T[] GetComponentsInChildren<T>() => null;
        public void GetComponentsInChildren<T>(bool includeInactive, List<T> result) { }
    }

    public class Behaviour : Component
    {
        public bool enabled { get; set; }
        public bool isActiveAndEnabled => true;
    }

    public class MonoBehaviour : Behaviour
    {
        public Coroutine StartCoroutine(IEnumerator routine) => null;
        public void StopCoroutine(Coroutine routine) { }
        public void StopCoroutine(IEnumerator routine) { }
        public void StopAllCoroutines() { }
    }

    public class ScriptableObject : Object
    {
        public static T CreateInstance<T>() where T : ScriptableObject => null;
    }

    public sealed class GameObject : Object
    {
        // Minimal working component container so EditMode-style logic tests can run offline (no Awake/OnEnable, like EditMode).
        readonly List<Component> components = new List<Component>();
        Transform tr;
        public GameObject() { }
        public GameObject(string name) { this.name = name; }
        public GameObject(string name, params Type[] components) { this.name = name; }
        public Transform transform => tr ?? (tr = new Transform());
        public bool activeSelf => true;
        public bool activeInHierarchy => true;
        public int layer { get; set; }
        public string tag { get; set; }
        public SceneManagement.Scene scene => default;
        public void SetActive(bool value) { }
        public T AddComponent<T>() where T : Component
        {
            var c = (T)Activator.CreateInstance(typeof(T), true);
            c.__stubOwner = this;
            components.Add(c);
            return c;
        }
        public T GetComponent<T>()
        {
            foreach (var c in components) if (c is T t) return t;
            return default;
        }
        public T GetComponentInParent<T>() => default;
        public T[] GetComponentsInChildren<T>(bool includeInactive) => null;
        public static GameObject CreatePrimitive(PrimitiveType type) => null;
    }

    public enum PrimitiveType { Sphere, Capsule, Cylinder, Cube, Plane, Quad }

    public class Transform : Component, IEnumerable
    {
        public Vector3 position { get; set; }
        public Quaternion rotation { get; set; }
        public Vector3 localPosition { get; set; }
        public Quaternion localRotation { get; set; }
        public Vector3 localScale { get; set; }
        public Vector3 eulerAngles { get; set; }
        public Vector3 forward { get; set; }
        public Vector3 right { get; set; }
        public Vector3 up { get; set; }
        public Transform parent { get; set; }
        public int childCount => 0;
        public void SetParent(Transform p) { }
        public void SetParent(Transform p, bool worldPositionStays) { }
        public void SetPositionAndRotation(Vector3 position, Quaternion rotation) { }
        public bool IsChildOf(Transform parent) => false;
        public Transform GetChild(int index) => null;
        public Vector3 TransformPoint(Vector3 position) => position;
        public IEnumerator GetEnumerator() => null;
    }

    public sealed class RectTransform : Transform
    {
        public Vector2 anchorMin { get; set; }
        public Vector2 anchorMax { get; set; }
        public Vector2 pivot { get; set; }
        public Vector2 anchoredPosition { get; set; }
        public Vector2 sizeDelta { get; set; }
        public Vector2 offsetMin { get; set; }
        public Vector2 offsetMax { get; set; }
    }

    public class YieldInstruction { }
    public sealed class Coroutine : YieldInstruction { }
    public sealed class WaitForSeconds : YieldInstruction { public WaitForSeconds(float seconds) { } }
    public class AsyncOperation : YieldInstruction { public bool isDone => true; public float progress => 1f; }

    // ── Attributes ──
    [AttributeUsage(AttributeTargets.Field)] public class HeaderAttribute : Attribute { public HeaderAttribute(string header) { } }
    [AttributeUsage(AttributeTargets.Field)] public class TooltipAttribute : Attribute { public TooltipAttribute(string tooltip) { } }
    [AttributeUsage(AttributeTargets.Field)] public sealed class SerializeField : Attribute { }
    [AttributeUsage(AttributeTargets.Field)] public sealed class HideInInspector : Attribute { }
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class RequireComponent : Attribute
    {
        public RequireComponent(Type t) { }
        public RequireComponent(Type t, Type t2) { }
        public RequireComponent(Type t, Type t2, Type t3) { }
    }
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CreateAssetMenuAttribute : Attribute { public string menuName { get; set; } public string fileName { get; set; } public int order { get; set; } }
    public enum RuntimeInitializeLoadType { AfterSceneLoad, BeforeSceneLoad, AfterAssembliesLoaded, BeforeSplashScreen, SubsystemRegistration }
    [AttributeUsage(AttributeTargets.Method)]
    public class RuntimeInitializeOnLoadMethodAttribute : Attribute
    {
        public RuntimeInitializeOnLoadMethodAttribute() { }
        public RuntimeInitializeOnLoadMethodAttribute(RuntimeInitializeLoadType loadType) { }
    }

    // ── Static services ──
    public static class Time
    {
        public static float time => 0f;
        public static float deltaTime => 0f;
        public static float unscaledDeltaTime => 0f;
        public static float timeScale { get; set; }
        public static int frameCount => 0;
    }

    public static class Debug
    {
        public static void Log(object message) { }
        public static void LogWarning(object message) { }
        public static void LogError(object message) { }
    }

    public static class Random
    {
        public static float value => 0f;
        public static float Range(float min, float max) => min;
        public static int Range(int min, int max) => min;
        public static Vector3 insideUnitSphere => default;
    }

    public static class Application
    {
        public static bool CanStreamedLevelBeLoaded(string levelName) => true;
        public static string persistentDataPath => System.IO.Path.GetTempPath();
        public static void Quit() { }
        public static bool isPlaying => true;
    }

    public static class QualitySettings
    {
        public static string[] names => new string[0];
        public static int GetQualityLevel() => 0;
        public static void SetQualityLevel(int index, bool applyExpensiveChanges = true) { }
        public static UnityEngine.Rendering.RenderPipelineAsset renderPipeline { get; set; }
    }

    public enum CursorLockMode { None, Locked, Confined }
    public static class Cursor
    {
        public static CursorLockMode lockState { get; set; }
        public static bool visible { get; set; }
    }

    public class TextAsset : Object
    {
        public TextAsset() { }
        public TextAsset(string text) { this.text = text; }
        public string text { get; private set; }
    }

    public static class ColorUtility
    {
        public static string ToHtmlStringRGB(Color color) => "FFFFFF";
    }

    public static class JsonUtility
    {
#if NET5_0_OR_GREATER
        // Runner only: System.Text.Json with public fields ≈ JsonUtility's field serialization.
        static readonly System.Text.Json.JsonSerializerOptions Opts = new System.Text.Json.JsonSerializerOptions { IncludeFields = true };
        public static T FromJson<T>(string json) => System.Text.Json.JsonSerializer.Deserialize<T>(json, Opts);
        public static string ToJson(object obj, bool prettyPrint = false) => System.Text.Json.JsonSerializer.Serialize(obj, obj.GetType(), Opts);
#else
        public static T FromJson<T>(string json) => default;
        public static string ToJson(object obj, bool prettyPrint = false) => "";
#endif
    }

    public static class Resources
    {
#if NET5_0_OR_GREATER
        // Runner only: TextAssets resolve to real files under Assets/Resources (found by walking up from the binary).
        public static T Load<T>(string path) where T : Object
        {
            if (typeof(T) != typeof(TextAsset)) return null;
            var dir = new System.IO.DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !System.IO.Directory.Exists(System.IO.Path.Combine(dir.FullName, "Assets", "Resources"))) dir = dir.Parent;
            if (dir == null) return null;
            foreach (var ext in new[] { ".json", ".txt", ".bytes" })
            {
                var f = System.IO.Path.Combine(dir.FullName, "Assets", "Resources", path + ext);
                if (System.IO.File.Exists(f)) return (T)(Object)new TextAsset(System.IO.File.ReadAllText(f));
            }
            return null;
        }
#else
        public static T Load<T>(string path) where T : Object => null;
#endif
        public static T GetBuiltinResource<T>(string path) where T : Object => null;
        public static AsyncOperation UnloadUnusedAssets() => null;
    }

    public static class StaticBatchingUtility
    {
        public static void Combine(GameObject staticBatchRoot) { }
    }
}
