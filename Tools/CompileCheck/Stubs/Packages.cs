// STUB — compile-check only. uGUI 2.0, Input System 1.11, Cinemachine 3.1, Addressables 2.x — only members used by Assets/Scripts.
#pragma warning disable
using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.EventSystems
{
    public class UIBehaviour : MonoBehaviour { }
    public class EventSystem : UIBehaviour { public static EventSystem current => null; }
    public abstract class BaseInputModule : UIBehaviour { }
}

namespace UnityEngine.UI
{
    using UnityEngine.EventSystems;

    public abstract class Graphic : UIBehaviour
    {
        public virtual Color color { get; set; }
        public bool raycastTarget { get; set; }
        public RectTransform rectTransform => null;
    }
    public abstract class MaskableGraphic : Graphic { }
    public class Image : MaskableGraphic { }
    public class Text : MaskableGraphic
    {
        public Font font { get; set; }
        public virtual string text { get; set; }
        public int fontSize { get; set; }
        public TextAnchor alignment { get; set; }
        public FontStyle fontStyle { get; set; }
        public bool supportRichText { get; set; }
        public HorizontalWrapMode horizontalOverflow { get; set; }
        public VerticalWrapMode verticalOverflow { get; set; }
    }

    public struct ColorBlock
    {
        public Color normalColor { get; set; }
        public Color highlightedColor { get; set; }
        public Color pressedColor { get; set; }
    }

    public class Selectable : UIBehaviour { public ColorBlock colors { get; set; } public bool interactable { get; set; } }
    public class Button : Selectable
    {
        public class ButtonClickedEvent : UnityEngine.Events.UnityEvent { }
        public ButtonClickedEvent onClick { get; set; }
    }

    public abstract class BaseMeshEffect : UIBehaviour { }
    public class Shadow : BaseMeshEffect { public Color effectColor { get; set; } }
    public class Outline : Shadow { }

    public class CanvasScaler : UIBehaviour
    {
        public enum ScaleMode { ConstantPixelSize, ScaleWithScreenSize, ConstantPhysicalSize }
        public ScaleMode uiScaleMode { get; set; }
        public Vector2 referenceResolution { get; set; }
        public float matchWidthOrHeight { get; set; }
    }

    public class GraphicRaycaster : BaseRaycaster { }
    public abstract class BaseRaycaster : UIBehaviour { }

    public abstract class LayoutGroup : UIBehaviour { public RectOffset padding { get; set; } }
    public abstract class HorizontalOrVerticalLayoutGroup : LayoutGroup
    {
        public float spacing { get; set; }
        public bool childForceExpandWidth { get; set; }
        public bool childForceExpandHeight { get; set; }
        public bool childControlWidth { get; set; }
        public bool childControlHeight { get; set; }
    }
    public class VerticalLayoutGroup : HorizontalOrVerticalLayoutGroup { }
    public class GridLayoutGroup : LayoutGroup
    {
        public Vector2 cellSize { get; set; }
        public Vector2 spacing { get; set; }
    }
}

namespace UnityEngine.InputSystem
{
    public enum InputActionType { Value, Button, PassThrough }

    public sealed class InputAction : IDisposable
    {
        public InputAction(string name = null, InputActionType type = default, string binding = null,
            string interactions = null, string processors = null, string expectedControlType = null) { }

        public struct BindingSyntax { }
        public struct CompositeSyntax
        {
            public CompositeSyntax With(string name, string binding, string groups = null, string processors = null) => this;
        }

        public BindingSyntax AddBinding(string path, string interactions = null, string processors = null, string groups = null) => default;
        public CompositeSyntax AddCompositeBinding(string composite, string interactions = null, string processors = null) => default;
        public void Enable() { }
        public void Disable() { }
        public void Dispose() { }
        public TValue ReadValue<TValue>() where TValue : struct => default;
        public bool WasPressedThisFrame() => false;
        public bool WasReleasedThisFrame() => false;
        public bool IsPressed() => false;
    }
}

namespace UnityEngine.InputSystem.UI
{
    public class InputSystemUIInputModule : UnityEngine.EventSystems.BaseInputModule { }
}

namespace Unity.Cinemachine
{
    using UnityEngine;

    public struct PrioritySettings
    {
        public bool Enabled;
        public int Value { get; set; }
        public static implicit operator int(PrioritySettings p) => p.Value;
        public static implicit operator PrioritySettings(int value) => new PrioritySettings { Enabled = true, Value = value };
    }

    public struct LensSettings { public float FieldOfView; }

    public abstract class CinemachineVirtualCameraBase : MonoBehaviour
    {
        public PrioritySettings Priority;
        public virtual Transform Follow { get; set; }
        public virtual Transform LookAt { get; set; }
    }

    public class CinemachineCamera : CinemachineVirtualCameraBase
    {
        public LensSettings Lens;
    }

    public abstract class CinemachineComponentBase : MonoBehaviour { }

    public class CinemachineThirdPersonFollow : CinemachineComponentBase
    {
        public Vector3 Damping;
        public Vector3 ShoulderOffset;
        public float VerticalArmLength;
        public float CameraSide;
        public float CameraDistance;
    }

    public struct ScreenComposerSettings { public Vector2 ScreenPosition; }

    public class CinemachineRotationComposer : CinemachineComponentBase
    {
        public ScreenComposerSettings Composition;
        public Vector2 Damping;
        public Vector3 TargetOffset;
    }

    public abstract class CinemachineExtension : MonoBehaviour { }
    public class CinemachineDeoccluder : CinemachineExtension
    {
        public LayerMask CollideAgainst;
        public float MinimumDistanceFromTarget;
    }

    public struct CinemachineBlendDefinition
    {
        public enum Styles { Cut, EaseInOut, EaseIn, EaseOut, HardIn, HardOut, Linear, Custom }
        public CinemachineBlendDefinition(Styles style, float time) { }
    }

    public class CinemachineBrain : MonoBehaviour
    {
        public CinemachineBlendDefinition DefaultBlend;
    }
}

namespace UnityEngine
{
    public struct LayerMask { public int value; }
}

namespace UnityEngine.ResourceManagement.AsyncOperations
{
    public enum AsyncOperationStatus { None, Succeeded, Failed }

    public struct AsyncOperationHandle : IEnumerator
    {
        public bool IsValid() => true;
        public object Current => null;
        public bool MoveNext() => false;
        public void Reset() { }
    }

    public struct AsyncOperationHandle<TObject> : IEnumerator
    {
        public AsyncOperationStatus Status => default;
        public TObject Result => default;
        public bool IsDone => true;
        public bool IsValid() => true;
        public object Current => null;
        public bool MoveNext() => false;
        public void Reset() { }
        public static implicit operator AsyncOperationHandle(AsyncOperationHandle<TObject> h) => default;
    }
}

namespace UnityEngine.ResourceManagement.ResourceLocations
{
    public interface IResourceLocation { string PrimaryKey { get; } }
}

namespace UnityEngine.ResourceManagement.ResourceProviders
{
    public struct SceneInstance
    {
        public UnityEngine.SceneManagement.Scene Scene => default;
    }
}

namespace UnityEngine.AddressableAssets
{
    using UnityEngine.ResourceManagement.AsyncOperations;
    using UnityEngine.ResourceManagement.ResourceLocations;
    using UnityEngine.ResourceManagement.ResourceProviders;
    using UnityEngine.SceneManagement;

    public static class Addressables
    {
        public static AsyncOperationHandle<IList<IResourceLocation>> LoadResourceLocationsAsync(object key, Type type = null) => default;
        public static AsyncOperationHandle<SceneInstance> LoadSceneAsync(object key, LoadSceneMode loadMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100) => default;
        public static AsyncOperationHandle<SceneInstance> UnloadSceneAsync(AsyncOperationHandle<SceneInstance> handle, bool autoReleaseHandle = true) => default;
        public static AsyncOperationHandle<SceneInstance> UnloadSceneAsync(SceneInstance scene, bool autoReleaseHandle = true) => default;
        public static void Release<TObject>(AsyncOperationHandle<TObject> handle) { }
        public static void Release(AsyncOperationHandle handle) { }
    }
}
