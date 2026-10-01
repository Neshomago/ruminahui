// Implements: 11-camera-performance.md Part A Step 5 (vision-sequence camera language):
//   5.1 low-amplitude handheld drift on vision cameras only (ShotDrift stands in for a Cinemachine Noise profile),
//   5.2 Depth of Field blended in for visions ONLY (a dedicated global Volume whose weight we drive) — the one place it's used heavily,
//   5.3 the "abrupt return to reality": a hard camera cut back to gameplay, with the audio cut at the same instant.
// Used by M1.2 (first vision, "establishes the visual language") and M3.5 stage N (shared vision).
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Ruminahui
{
    public class VisionSequence : Singleton<VisionSequence>
    {
        public float blendInSeconds = 1.2f;          // PLACEHOLDER-BALANCE: "focus-breathing" fade into the vision
        public float focusDistance = 2.5f;
        public float focalLength = 140f;
        public float aperture = 1.8f;

        public bool InVision { get; private set; }
        Volume volume;
        CinemachineCamera shot;

        protected override void Awake()
        {
            base.Awake();
            var go = new GameObject("VisionVolume");
            go.transform.SetParent(transform, false);
            volume = go.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 50f;
            volume.weight = 0f;
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            var dof = profile.Add<DepthOfField>(true);
            if (dof != null)
            {
                dof.mode.Override(DepthOfFieldMode.Bokeh);
                dof.focusDistance.Override(focusDistance);
                dof.focalLength.Override(focalLength);
                dof.aperture.Override(aperture);
            }
            volume.profile = profile;
        }

        /// <summary>Enter a vision on a dedicated drifting shot. Blocks gameplay input and hides combat UI.</summary>
        public IEnumerator Enter(Vector3 cameraPosition, Vector3 lookAt)
        {
            if (InVision) yield break;
            InVision = true;
            GameInput.Instance?.PushCutscene();
            if (UIRoot.Instance != null) UIRoot.Instance.SetCombatUIVisible(false);
            var cd = CameraDirector.Instance;
            cd.EnablePostProcessing();
            shot = cd.CreateShot("VCam_Vision", cameraPosition, lookAt, 0.12f); // the subtle handheld drift (Step 5.1)
            shot.Priority = CameraDirector.ShotPriority + 10;
            AudioPool.Instance?.PlayCue(PlaceholderCue.Vision, cameraPosition, 0.45f);
            float t = 0f;
            while (t < blendInSeconds)
            {
                t += Time.deltaTime;
                volume.weight = Mathf.SmoothStep(0f, 1f, t / blendInSeconds);
                yield return null;
            }
            volume.weight = 1f;
        }

        /// <summary>Move the vision camera between images without leaving the vision.</summary>
        public void Reframe(Vector3 cameraPosition, Vector3 lookAt)
        {
            if (shot == null) return;
            shot.transform.SetPositionAndRotation(cameraPosition, Quaternion.LookRotation(lookAt - cameraPosition));
            var drift = shot.GetComponent<ShotDrift>();
            if (drift != null) drift.Rebase();
        }

        /// <summary>The abrupt return: hard camera cut + DoF off + audio cut, all in the same frame (Step 5.3).</summary>
        public void ExitHard()
        {
            if (!InVision) return;
            InVision = false;
            volume.weight = 0f;
            CameraDirector.Instance.HardCut();
            AudioPool.Instance?.StopAll();
            if (shot != null) { shot.Priority = 0; Destroy(shot.gameObject); shot = null; }
            if (UIRoot.Instance != null) UIRoot.Instance.SetCombatUIVisible(true);
            GameInput.Instance?.PopCutscene();
        }
    }
}
