// Implements: AI_BUILD_PROMPT.md "HOW TO WORK" — every character/enemy is a flat-coloured primitive driven by ONE component,
// so real models replace it by assigning `modelPrefab` (no logic changes). Also 11-camera-performance.md Part B Step 4:
// an LODGroup is added by convention even on placeholders.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ruminahui
{
    public enum PlaceholderShape { Capsule, Cube, Sphere, Cylinder }

    public class PlaceholderVisual : MonoBehaviour
    {
        [Tooltip("Real art goes here later. When set, it replaces the primitive.")]
        public GameObject modelPrefab;
        public PlaceholderShape shape = PlaceholderShape.Capsule;
        public Color baseColor = Color.white;
        public Vector3 scale = Vector3.one;
        public Vector3 offset = new Vector3(0f, 1f, 0f);
        [Tooltip("Idle 'breathing' bob. Atoc's decoys deliberately lack it (08-atoc-boss-fight.md Phase 2 tell-within-a-tell).")]
        public bool idleBob;
        public bool addFacingNose = true;

        [SerializeField] Transform modelRoot;
        // Serialized so marker colours survive being saved into a prefab (the dictionary below is runtime-only).
        [SerializeField] List<Renderer> markerRenderers = new List<Renderer>();
        [SerializeField] List<Color> markerColors = new List<Color>();
        readonly List<Renderer> renderers = new List<Renderer>();
        readonly Dictionary<Renderer, Color> rendererColors = new Dictionary<Renderer, Color>();
        Coroutine flashRoutine;
        Vector3 baseLocalPos;
        Vector3 baseLocalScale;
        float bobSeed;
        bool visible = true;

        public Transform ModelRoot { get { Build(); return modelRoot; } }
        public Color CurrentColor { get; private set; }

        void Awake()
        {
            Build();
            RefreshRenderers();
            bobSeed = Random.value * 10f;
        }

        public void Build()
        {
            if (modelRoot != null) return;

            modelRoot = new GameObject("Model").transform;
            modelRoot.SetParent(transform, false);
            modelRoot.localPosition = offset;

            GameObject body;
            if (modelPrefab != null)
            {
                body = Instantiate(modelPrefab, modelRoot, false);
            }
            else
            {
                body = GameObject.CreatePrimitive(ToPrimitive(shape));
                body.name = "Body";
                // Visual only — gameplay uses the root CharacterController / CombatTarget distance checks.
                var col = body.GetComponent<Collider>();
                if (col != null) DestroyImmediate(col);
                body.transform.SetParent(modelRoot, false);
                body.transform.localScale = scale;
            }

            if (addFacingNose && modelPrefab == null)
            {
                float headY = shape == PlaceholderShape.Capsule ? 0.55f * scale.y : 0.3f * scale.y;
                AddMarker("Nose", PlaceholderShape.Cube, Color.Lerp(baseColor, Color.black, 0.5f),
                    new Vector3(0f, headY, 0.5f * scale.z), new Vector3(0.2f, 0.12f, 0.25f));
            }

            baseLocalPos = modelRoot.localPosition;
            baseLocalScale = modelRoot.localScale;
            RefreshRenderers();
            ApplyAllColors(baseColor);

            var lod = GetComponent<LODGroup>();
            if (lod == null) lod = gameObject.AddComponent<LODGroup>();
            lod.SetLODs(new[] { new LOD(0.01f, renderers.ToArray()) });
            lod.RecalculateBounds();
        }

        static PrimitiveType ToPrimitive(PlaceholderShape s)
        {
            switch (s)
            {
                case PlaceholderShape.Cube: return PrimitiveType.Cube;
                case PlaceholderShape.Sphere: return PrimitiveType.Sphere;
                case PlaceholderShape.Cylinder: return PrimitiveType.Cylinder;
                default: return PrimitiveType.Capsule;
            }
        }

        /// <summary>Adds an extra primitive part (shield, horse body, weapon…) that keeps its own colour.</summary>
        public Transform AddMarker(string partName, PlaceholderShape partShape, Color color, Vector3 localPos, Vector3 localScale)
        {
            Build();
            var part = GameObject.CreatePrimitive(ToPrimitive(partShape));
            part.name = partName;
            var col = part.GetComponent<Collider>();
            if (col != null) DestroyImmediate(col);
            part.transform.SetParent(modelRoot, false);
            part.transform.localPosition = localPos;
            part.transform.localScale = localScale;
            var r = part.GetComponent<Renderer>();
            PlaceholderMaterials.Apply(r, color);
            rendererColors[r] = color;
            markerRenderers.Add(r);
            markerColors.Add(color);
            RefreshRenderers();
            return part.transform;
        }

        void RefreshRenderers()
        {
            renderers.Clear();
            if (modelRoot == null) return;
            for (int i = 0; i < markerRenderers.Count && i < markerColors.Count; i++)
                if (markerRenderers[i] != null) rendererColors[markerRenderers[i]] = markerColors[i];
            modelRoot.GetComponentsInChildren(true, renderers);
            foreach (var r in renderers)
            {
                if (!rendererColors.ContainsKey(r)) rendererColors[r] = baseColor;
                PlaceholderMaterials.Apply(r, rendererColors[r]);
            }
        }

        void ApplyAllColors(Color bodyColor)
        {
            CurrentColor = bodyColor;
            foreach (var r in renderers)
            {
                if (r == null) continue;
                bool isBody = r.gameObject.name == "Body" || modelPrefab != null;
                PlaceholderMaterials.SetColor(r, isBody ? bodyColor : rendererColors[r]);
            }
        }

        public void SetBaseColor(Color c)
        {
            baseColor = c;
            if (flashRoutine == null) ApplyAllColors(c);
        }

        /// <summary>Tell / feedback flash. Tell flashes respect TellFlashSettings; feedback flashes pass force=true.</summary>
        public void Flash(Color color, float duration, bool isTell = true)
        {
            if (isTell && !TellFlashSettings.Enabled) return;
            if (!isActiveAndEnabled) return;
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(FlashRoutine(color, duration));
        }

        IEnumerator FlashRoutine(Color color, float duration)
        {
            float t = 0f;
            while (t < duration)
            {
                float pulse = Mathf.PingPong(t * 8f, 1f);
                ApplyAllColors(Color.Lerp(baseColor, color, 0.5f + 0.5f * pulse));
                t += Time.deltaTime;
                yield return null;
            }
            flashRoutine = null;
            ApplyAllColors(baseColor);
        }

        public void SetVisible(bool v)
        {
            visible = v;
            foreach (var r in renderers) if (r != null) r.enabled = v;
        }

        public bool IsVisible => visible;

        /// <summary>Squash/stretch for placeholder "poses" (crouch tell, downed, stance change).</summary>
        public void SetPoseScale(Vector3 multiplier)
        {
            Build();
            modelRoot.localScale = Vector3.Scale(baseLocalScale, multiplier);
        }

        public void ResetPose()
        {
            Build();
            modelRoot.localScale = baseLocalScale;
            modelRoot.localPosition = baseLocalPos;
        }

        void OnDisable()
        {
            if (flashRoutine != null) { StopCoroutine(flashRoutine); flashRoutine = null; }
            if (modelRoot != null) ApplyAllColors(baseColor);
        }

        void Update()
        {
            if (!idleBob || modelRoot == null) return;
            float bob = Mathf.Sin((Time.time + bobSeed) * 2.2f) * 0.04f;
            modelRoot.localPosition = baseLocalPos + new Vector3(0f, bob, 0f);
        }
    }
}
