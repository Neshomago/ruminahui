// Implements: 03-combat-design.md movement verbs (Root-Step, Skywalk, Shed Skin dashes, jumps/launches, knockback) and
// 11-camera-performance.md Part B Step 8 (capsule CharacterController, no Rigidbodies on characters).
using UnityEngine;

namespace Ruminahui
{
    [RequireComponent(typeof(CharacterController))]
    public class CharacterMotor : MonoBehaviour
    {
        public float moveSpeed = 6f;
        public float rotationSpeed = 720f;
        public float gravity = -25f;
        public float jumpHeight = 1.3f;
        [Tooltip("Use TimeDilation (enemies) — player characters stay on real time unless slowed by others.")]
        public bool useTimeDilation = true;

        public CharacterController Controller { get; private set; }
        public bool IsGrounded => Controller != null && Controller.isGrounded;
        public float VerticalVelocity { get => verticalVelocity; set => verticalVelocity = value; }
        public bool IsDashing => dashTimer > 0f;
        public Vector3 Velocity { get; private set; }

        [HideInInspector] public bool movementLocked;
        [HideInInspector] public bool rotationLocked;
        [HideInInspector] public bool gravityEnabled = true;
        [HideInInspector] public float speedMultiplier = 1f;

        Vector3 desiredMove;
        float desiredSpeedScale = 1f;
        Vector3 desiredFacing;
        float verticalVelocity;
        Vector3 knockback;
        Vector3 dashVelocity;
        float dashTimer;
        StatusEffects status;

        void Awake()
        {
            Controller = GetComponent<CharacterController>();
            status = GetComponent<StatusEffects>();
        }

        public float Delta => useTimeDilation ? TimeDilation.DeltaFor(gameObject) : Time.deltaTime;

        /// <summary>Call every frame (zero to stop). Direction is world-space and flattened.</summary>
        public void SetMoveInput(Vector3 worldDirection, float speedScale = 1f)
        {
            worldDirection.y = 0f;
            desiredMove = worldDirection.sqrMagnitude > 1f ? worldDirection.normalized : worldDirection;
            desiredSpeedScale = speedScale;
            if (!movementLocked && desiredMove.sqrMagnitude > 0.001f) desiredFacing = desiredMove;
        }

        public void FaceDirection(Vector3 dir, bool instant = false)
        {
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f) return;
            desiredFacing = dir.normalized;
            if (instant) transform.rotation = Quaternion.LookRotation(desiredFacing);
        }

        public void FaceTowards(Vector3 point, bool instant = false) => FaceDirection(point - transform.position, instant);

        public void Jump()
        {
            if (!IsGrounded) return;
            verticalVelocity = Mathf.Sqrt(-2f * gravity * jumpHeight);
        }

        public void Launch(float upwardVelocity) => verticalVelocity = upwardVelocity;

        public void Dash(Vector3 direction, float distance, float duration)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.001f) direction = -transform.forward;
            direction.Normalize();
            dashTimer = Mathf.Max(0.01f, duration);
            dashVelocity = direction * (distance / dashTimer);
        }

        public void StopDash() { dashTimer = 0f; dashVelocity = Vector3.zero; }

        public void AddKnockback(Vector3 v) => knockback += v;

        /// <summary>Safe teleport — CharacterController overwrites transform.position unless disabled first.</summary>
        public void Teleport(Vector3 position, Quaternion? rotation = null)
        {
            bool was = Controller.enabled;
            Controller.enabled = false;
            transform.position = position;
            if (rotation.HasValue) transform.rotation = rotation.Value;
            Controller.enabled = was;
            verticalVelocity = 0f;
            knockback = Vector3.zero;
            StopDash();
        }

        void Update()
        {
            if (Controller == null || !Controller.enabled) return;
            float dt = Delta;
            if (dt <= 0f) return;

            bool incapacitated = status != null && status.IsIncapacitated;
            float slow = status != null ? status.SlowMultiplier : 1f;

            Vector3 horizontal = Vector3.zero;
            if (!movementLocked && !incapacitated && dashTimer <= 0f)
                horizontal = desiredMove * (moveSpeed * desiredSpeedScale * speedMultiplier * slow);

            if (dashTimer > 0f)
            {
                horizontal += dashVelocity;
                dashTimer -= dt;
                if (dashTimer <= 0f) dashVelocity = Vector3.zero;
            }

            horizontal += knockback;
            knockback = Vector3.MoveTowards(knockback, Vector3.zero, 30f * dt);

            if (gravityEnabled)
            {
                if (Controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
                verticalVelocity += gravity * dt;
            }
            else if (verticalVelocity < 0f) verticalVelocity = 0f;

            var motion = horizontal + Vector3.up * verticalVelocity;
            Controller.Move(motion * dt);
            Velocity = motion;

            if (!rotationLocked && !incapacitated && desiredFacing.sqrMagnitude > 0.001f)
            {
                var target = Quaternion.LookRotation(desiredFacing);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, target, rotationSpeed * dt);
            }
        }
    }
}
