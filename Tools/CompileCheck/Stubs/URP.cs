// STUB — compile-check only. URP + Core (Volume) members used by Assets/Scripts (runtime + editor). Verified against the
// Graphics repo source 2026-10-01: Volume.{isGlobal,priority,weight,profile}, DepthOfField.{mode,focusDistance,focalLength,aperture}.
#pragma warning disable
namespace UnityEngine.Rendering
{
    public static class GraphicsSettings
    {
        public static RenderPipelineAsset defaultRenderPipeline { get; set; }
    }

    public class VolumeParameter<T>
    {
        public T value;
        public bool overrideState;
        public void Override(T x) { overrideState = true; value = x; }
    }
    public class FloatParameter : VolumeParameter<float> { public FloatParameter(float v, bool overrideState = false) { value = v; } }
    public class MinFloatParameter : VolumeParameter<float> { public MinFloatParameter(float v, float min, bool overrideState = false) { value = v; } }
    public class ClampedFloatParameter : VolumeParameter<float> { public ClampedFloatParameter(float v, float min, float max, bool overrideState = false) { value = v; } }

    public abstract class VolumeComponent : ScriptableObject { public bool active = true; }

    public sealed class VolumeProfile : ScriptableObject
    {
        public T Add<T>(bool overrides = false) where T : VolumeComponent => null;
    }

    public class Volume : MonoBehaviour
    {
        public bool isGlobal { get; set; }
        public float priority;
        public float weight;
        public float blendDistance;
        public VolumeProfile sharedProfile;
        public VolumeProfile profile { get; set; }
    }
}

namespace UnityEngine.Rendering.Universal
{
    public abstract class ScriptableRendererData : ScriptableObject { }
    public class PostProcessData : ScriptableObject { }
    public class UniversalRendererData : ScriptableRendererData { public PostProcessData postProcessData = null; }
    public class UniversalRenderPipelineAsset : UnityEngine.Rendering.RenderPipelineAsset
    {
        public static UniversalRenderPipelineAsset Create(ScriptableRendererData rendererData = null) => null;
        public float renderScale { get; set; }
        public bool supportsHDR { get; set; }
        public float shadowDistance { get; set; }
    }

    public enum DepthOfFieldMode { Off, Gaussian, Bokeh }
    public sealed class DepthOfFieldModeParameter : VolumeParameter<DepthOfFieldMode> { public DepthOfFieldModeParameter(DepthOfFieldMode v, bool overrideState = false) { value = v; } }
    public sealed class DepthOfField : VolumeComponent
    {
        public DepthOfFieldModeParameter mode = new DepthOfFieldModeParameter(DepthOfFieldMode.Off);
        public MinFloatParameter focusDistance = new MinFloatParameter(10f, 0.1f);
        public ClampedFloatParameter focalLength = new ClampedFloatParameter(50f, 1f, 300f);
        public ClampedFloatParameter aperture = new ClampedFloatParameter(5.6f, 1f, 32f);
    }

    public class UniversalAdditionalCameraData : MonoBehaviour { public bool renderPostProcessing { get; set; } }
    public static class CameraExtensions
    {
        public static UniversalAdditionalCameraData GetUniversalAdditionalCameraData(this Camera camera) => null;
    }
}
