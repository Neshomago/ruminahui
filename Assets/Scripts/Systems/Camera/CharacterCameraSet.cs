// Implements: 11-camera-performance.md, Part A Step 2 (VCam_Gameplay_ThirdPerson: 3rd Person Follow + Composer + collider),
// Step 3 (VCam_Combat_LockOn, priority-raised while locked), Step 6 (one VCam set per character, pre-placed, priority swap only),
// Step 7 (slightly wider framing for boss fights).
// NOTE: Unity 6 ships Cinemachine 3 — CinemachineVirtualCamera → CinemachineCamera, CinemachineCollider → CinemachineDeoccluder,
// Composer → CinemachineRotationComposer. Same concepts as the doc.
using Unity.Cinemachine;
using UnityEngine;

namespace Ruminahui
{
    public class CharacterCameraSet : MonoBehaviour
    {
        public const int ActivePriority = 20;
        public const int LockOnPriority = 25;
        public const int InactivePriority = 0;

        public float minPitch = -30f;
        public float maxPitch = 60f;
        public float normalDistance = 4.2f;   // PLACEHOLDER-BALANCE
        public float bossDistance = 5.8f;     // PLACEHOLDER-BALANCE: "slightly wider" (Step 7)

        public Transform CameraTarget { get; private set; }
        public CinemachineCamera Gameplay { get; private set; }
        public CinemachineCamera LockOn { get; private set; }

        float yaw;
        float pitch = 12f;
        bool isActiveCharacter;
        bool lockedOn;
        Transform lockTarget;
        CinemachineThirdPersonFollow gameplayFollow, lockFollow;

        void Awake()
        {
            CameraTarget = new GameObject("CameraTarget").transform;
            CameraTarget.SetParent(transform, false);
            CameraTarget.localPosition = new Vector3(0f, 1.55f, 0f);
            yaw = transform.eulerAngles.y;

            string id = GetComponent<PlayerCharacter>() != null ? GetComponent<PlayerCharacter>().id.ToString() : name;
            Gameplay = CreateCam($"VCam_Gameplay_{id}", out gameplayFollow, false);
            LockOn = CreateCam($"VCam_Combat_LockOn_{id}", out lockFollow, true);
            ApplyPriorities();
        }

        CinemachineCamera CreateCam(string camName, out CinemachineThirdPersonFollow follow, bool lockOnCam)
        {
            var go = new GameObject(camName);
            // Not parented to the character: Cinemachine drives the transform; parenting would double-apply motion.
            go.transform.position = transform.position - transform.forward * normalDistance + Vector3.up * 2f;
            var cam = go.AddComponent<CinemachineCamera>();
            cam.Follow = CameraTarget;
            cam.LookAt = lockOnCam ? null : CameraTarget;
            cam.Priority = InactivePriority;
            cam.Lens.FieldOfView = 55f;

            follow = go.AddComponent<CinemachineThirdPersonFollow>();
            follow.ShoulderOffset = new Vector3(0.55f, 0.15f, 0f);
            follow.VerticalArmLength = 0.3f;
            follow.CameraSide = 0.65f;
            follow.CameraDistance = normalDistance;
            follow.Damping = new Vector3(0.1f, 0.3f, 0.2f);

            var composer = go.AddComponent<CinemachineRotationComposer>();
            // Slightly off-centre toward the shoulder: classic over-the-shoulder framing, leaves HUD room (Step 2.4).
            composer.Composition.ScreenPosition = lockOnCam ? new Vector2(0f, 0.05f) : new Vector2(-0.08f, 0.06f);
            composer.Damping = new Vector2(0.4f, 0.4f);

            var deoccluder = go.AddComponent<CinemachineDeoccluder>();
            deoccluder.MinimumDistanceFromTarget = 0.6f;

            return cam;
        }

        void OnDestroy()
        {
            if (Gameplay != null) Destroy(Gameplay.gameObject);
            if (LockOn != null) Destroy(LockOn.gameObject);
        }

        public void SetActiveCharacter(bool active)
        {
            isActiveCharacter = active;
            ApplyPriorities();
        }

        public void SetLockTarget(Transform target)
        {
            lockTarget = target;
            lockedOn = target != null;
            LockOn.LookAt = target;
            ApplyPriorities();
        }

        public void SetBossFraming(bool wide)
        {
            float d = wide ? bossDistance : normalDistance;
            gameplayFollow.CameraDistance = d;
            lockFollow.CameraDistance = d;
        }

        void ApplyPriorities()
        {
            if (Gameplay == null || LockOn == null) return;
            Gameplay.Priority = isActiveCharacter ? ActivePriority : InactivePriority;
            LockOn.Priority = isActiveCharacter && lockedOn ? LockOnPriority : InactivePriority;
        }

        public void AddLook(Vector2 deltaDegrees)
        {
            yaw += deltaDegrees.x;
            pitch = Mathf.Clamp(pitch - deltaDegrees.y, minPitch, maxPitch);
        }

        public void SnapYawTo(float newYaw) => yaw = newYaw;

        /// <summary>Camera-relative flat forward/right for movement input.</summary>
        public Vector3 Forward => Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
        public Vector3 Right => Quaternion.Euler(0f, yaw, 0f) * Vector3.right;

        void LateUpdate()
        {
            if (lockedOn && lockTarget != null)
            {
                var to = lockTarget.position - transform.position;
                to.y = 0f;
                if (to.sqrMagnitude > 0.01f)
                {
                    float targetYaw = Quaternion.LookRotation(to).eulerAngles.y;
                    yaw = Mathf.MoveTowardsAngle(yaw, targetYaw, 360f * Time.deltaTime);
                }
            }
            else if (lockedOn && lockTarget == null)
            {
                SetLockTarget(null);
            }
            // World-space rotation so the follow target doesn't inherit the body's facing.
            CameraTarget.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }
    }
}
