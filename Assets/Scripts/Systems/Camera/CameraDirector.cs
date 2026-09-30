// Implements: 11-camera-performance.md, Part A Step 1 (CinemachineBrain on the Main Camera), Step 4 (cutscene shot cameras —
// code-driven placeholder until Timeline assets are authored), Step 5.3 (hard cut back to gameplay), Step 6 (priority swaps
// for M3.5 control shifts, no camera instantiation at swap time), Step 7.2 (dedicated spare-moment shot).
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

namespace Ruminahui
{
    public class CameraDirector : Singleton<CameraDirector>
    {
        public const int ShotPriority = 100;

        public Camera MainCamera { get; private set; }
        public CinemachineBrain Brain { get; private set; }
        CinemachineBlendDefinition defaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, 0.35f);
        PlayerCharacter activeCharacter;

        void Start() => EnsureCamera();

        public void EnsureCamera()
        {
            if (MainCamera != null && Brain != null) return;
            MainCamera = Camera.main;
            if (MainCamera == null)
            {
                var go = new GameObject("Main Camera");
                go.tag = "MainCamera";
                MainCamera = go.AddComponent<Camera>();
                go.AddComponent<AudioListener>();
                DontDestroyOnLoad(go);
            }
            MainCamera.nearClipPlane = 0.1f;
            MainCamera.farClipPlane = 600f;
            Brain = MainCamera.GetComponent<CinemachineBrain>();
            if (Brain == null) Brain = MainCamera.gameObject.AddComponent<CinemachineBrain>();
            Brain.DefaultBlend = defaultBlend;
        }

        /// <summary>Makes <paramref name="c"/> the live gameplay camera. cut=true hides the change (M3.5 swaps happen under a flash).</summary>
        public void FocusCharacter(PlayerCharacter c, bool cut)
        {
            EnsureCamera();
            if (activeCharacter != null && activeCharacter != c && activeCharacter.Cameras != null)
                activeCharacter.Cameras.SetActiveCharacter(false);
            activeCharacter = c;
            if (c != null && c.Cameras != null) c.Cameras.SetActiveCharacter(true);
            if (cut) StartCoroutine(CutThisFrame());
        }

        IEnumerator CutThisFrame()
        {
            Brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.Cut, 0f);
            yield return null;
            yield return null; // brain evaluates in LateUpdate; restore after it has consumed the cut
            Brain.DefaultBlend = defaultBlend;
        }

        public void HardCut() { EnsureCamera(); StartCoroutine(CutThisFrame()); }

        /// <summary>Creates a static/slow-drift shot camera (Step 4.2 — deliberately different from the gameplay follow-cam).</summary>
        public CinemachineCamera CreateShot(string shotName, Vector3 position, Vector3 lookAtPoint, float drift = 0f)
        {
            var go = new GameObject(shotName);
            go.transform.SetPositionAndRotation(position, Quaternion.LookRotation(lookAtPoint - position));
            var cam = go.AddComponent<CinemachineCamera>();
            cam.Priority = 0;
            cam.Lens.FieldOfView = 40f;
            if (drift > 0f) go.AddComponent<ShotDrift>().speed = drift;
            return cam;
        }

        /// <summary>Plays a shot for a duration then hands back to gameplay with a hard cut (Step 5.3).</summary>
        public IEnumerator PlayShot(CinemachineCamera shot, float duration, bool cutIn = true)
        {
            EnsureCamera();
            if (cutIn) HardCut();
            shot.Priority = ShotPriority;
            yield return new WaitForSeconds(duration);
            HardCut();
            shot.Priority = 0;
        }
    }

}
