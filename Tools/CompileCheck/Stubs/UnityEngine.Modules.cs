// STUB — compile-check only. Physics, rendering, audio, camera, scenes.
#pragma warning disable
using System.Collections.Generic;

namespace UnityEngine
{
    public enum QueryTriggerInteraction { UseGlobal, Ignore, Collide }

    public struct RaycastHit
    {
        public Collider collider => null;
        public Vector3 point => default;
        public float distance => 0f;
    }

    public static class Physics
    {
        public static bool Raycast(Vector3 origin, Vector3 direction, float maxDistance, int layerMask, QueryTriggerInteraction q) => false;
        public static bool Raycast(Vector3 origin, Vector3 direction, out RaycastHit hitInfo, float maxDistance, int layerMask, QueryTriggerInteraction q) { hitInfo = default; return false; }
        public static bool Raycast(Vector3 origin, Vector3 direction, float maxDistance) => false;
        public static bool SphereCast(Vector3 origin, float radius, Vector3 direction, out RaycastHit hitInfo, float maxDistance, int layerMask, QueryTriggerInteraction q) { hitInfo = default; return false; }
        public static void SyncTransforms() { }
    }

    public class Collider : Component { public bool enabled { get; set; } }
    public class BoxCollider : Collider { }

    public enum CollisionFlags { None = 0, Sides = 1, Above = 2, Below = 4 }

    public class CharacterController : Collider
    {
        public bool isGrounded => false;
        public float height { get; set; }
        public float radius { get; set; }
        public Vector3 center { get; set; }
        public float stepOffset { get; set; }
        public float slopeLimit { get; set; }
        public CollisionFlags Move(Vector3 motion) => CollisionFlags.None;
    }

    public class Shader : Object
    {
        public static Shader Find(string name) => null;
        public static int PropertyToID(string name) => 0;
    }

    public class Material : Object
    {
        public Material(Shader shader) { }
        public Material(Material source) { }
        public bool enableInstancing { get; set; }
    }

    public sealed class MaterialPropertyBlock
    {
        public void SetColor(int nameID, Color value) { }
    }

    public class Renderer : Component
    {
        public bool enabled { get; set; }
        public Material sharedMaterial { get; set; }
        public void GetPropertyBlock(MaterialPropertyBlock properties) { }
        public void SetPropertyBlock(MaterialPropertyBlock properties) { }
    }

    public struct LOD
    {
        public LOD(float screenRelativeTransitionHeight, Renderer[] renderers) { }
    }

    public class LODGroup : Component
    {
        public void SetLODs(LOD[] lods) { }
        public void RecalculateBounds() { }
    }

    public enum LightType { Spot, Directional, Point }
    public enum LightShadows { None, Hard, Soft }
    public class Light : Behaviour
    {
        public LightType type { get; set; }
        public float intensity { get; set; }
        public LightShadows shadows { get; set; }
    }

    public class Camera : Behaviour
    {
        public static Camera main => null;
        public float nearClipPlane { get; set; }
        public float farClipPlane { get; set; }
        public Vector3 WorldToScreenPoint(Vector3 position) => position;
    }

    public sealed class AudioListener : Behaviour { }
    public enum AudioRolloffMode { Logarithmic, Linear, Custom }

    public sealed class AudioClip : Object
    {
        public static AudioClip Create(string name, int lengthSamples, int channels, int frequency, bool stream) => null;
        public bool SetData(float[] data, int offsetSamples) => true;
    }

    public sealed class AudioSource : Behaviour
    {
        public bool playOnAwake { get; set; }
        public float spatialBlend { get; set; }
        public AudioRolloffMode rolloffMode { get; set; }
        public float maxDistance { get; set; }
        public float pitch { get; set; }
        public float volume { get; set; }
        public AudioClip clip { get; set; }
        public void Play() { }
    }

    // UI-adjacent types that live in UnityEngine (UIModule / TextRendering)
    public enum RenderMode { ScreenSpaceOverlay, ScreenSpaceCamera, WorldSpace }
    public sealed class Canvas : Behaviour
    {
        public RenderMode renderMode { get; set; }
        public int sortingOrder { get; set; }
    }
    public sealed class CanvasGroup : Behaviour
    {
        public float alpha { get; set; }
        public bool interactable { get; set; }
        public bool blocksRaycasts { get; set; }
    }
    public class RectOffset { public RectOffset(int left, int right, int top, int bottom) { } }
    public sealed class Font : Object { }
    public enum TextAnchor { UpperLeft, UpperCenter, UpperRight, MiddleLeft, MiddleCenter, MiddleRight, LowerLeft, LowerCenter, LowerRight }
    public enum FontStyle { Normal, Bold, Italic, BoldAndItalic }
    public enum HorizontalWrapMode { Wrap, Overflow }
    public enum VerticalWrapMode { Truncate, Overflow }

    public static class RectTransformUtility
    {
        public static bool ScreenPointToLocalPointInRectangle(RectTransform rect, Vector2 screenPoint, Camera cam, out Vector2 localPoint) { localPoint = default; return true; }
    }
}

namespace UnityEngine.SceneManagement
{
    public enum LoadSceneMode { Single, Additive }

    public struct Scene
    {
        public string name => null;
        public bool isLoaded => true;
        public bool IsValid() => true;
        public static bool operator ==(Scene a, Scene b) => true;
        public static bool operator !=(Scene a, Scene b) => false;
        public override bool Equals(object o) => false;
        public override int GetHashCode() => 0;
    }

    public static class SceneManager
    {
        public static Scene GetActiveScene() => default;
        public static bool SetActiveScene(Scene scene) => true;
        public static Scene GetSceneByName(string name) => default;
        public static AsyncOperation LoadSceneAsync(string sceneName, LoadSceneMode mode) => null;
        public static AsyncOperation UnloadSceneAsync(Scene scene) => null;
    }
}

namespace UnityEngine.Events
{
    public delegate void UnityAction();
    public class UnityEvent { public void AddListener(UnityAction call) { } }
}
