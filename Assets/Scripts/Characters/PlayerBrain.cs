// Implements: 03-combat-design.md, Sections 1-3 input columns → kit commands; camera-relative third-person movement
// (11-camera-performance.md Part A Step 2); lock-on / Condor's Eye on one input (Step 3.3).
using UnityEngine;

namespace Ruminahui
{
    public class PlayerBrain : MonoBehaviour
    {
        PlayerCharacter character;

        void Awake() => character = GetComponent<PlayerCharacter>();

        void OnDisable()
        {
            if (character != null && character.Sensor != null) character.Sensor.CancelHold();
        }

        void Update()
        {
            var input = GameInput.Instance;
            if (input == null || character == null) return;
            var kit = character.Kit;
            var motor = character.Motor;
            var cams = character.Cameras;

            if (character.ScriptLocked || !character.Health || character.Health.IsDead)
            {
                motor.SetMoveInput(Vector3.zero);
                return;
            }

            // Look
            if (cams != null) cams.AddLook(input.LookDelta);

            // Move (camera-relative)
            Vector2 mv = input.MoveValue;
            Vector3 fwd = cams != null ? cams.Forward : Vector3.forward;
            Vector3 right = cams != null ? cams.Right : Vector3.right;
            Vector3 worldMove = fwd * mv.y + right * mv.x;
            bool canMove = kit == null || kit.AllowsMovement;
            motor.SetMoveInput(canMove ? worldMove : Vector3.zero, kit != null ? kit.MoveSpeedMultiplier : 1f);

            var locked = character.Targeting != null ? character.Targeting.Locked : null;
            if (locked != null && canMove && worldMove.sqrMagnitude > 0.01f) motor.FaceTowards(locked.transform.position);

            // Interact works for everyone — including combat-disabled characters (M0.1's boy gathers wood, talks to the village).
            HandleInteract(input, kit);

            if (kit == null || character.CombatDisabled)
            {
                // Stealth missions: move/hide (+ jump) only — no attack option (04 M0.1 / M5.5).
                if (input.Pressed(input.Jump)) motor.Jump();
                return;
            }

            // Hold LB = call-in modifier: Y/X/B issue ally commands instead of heavy/light/dodge (approved plan).
            bool freeSwap = FreeSwapController.Instance != null && FreeSwapController.Instance.Active;
            bool commanding = input.CommandModifierHeld && !freeSwap;

            if (input.Pressed(input.Jump)) kit.CmdJump();
            if (!commanding && input.Pressed(input.Light)) kit.CmdLight();
            if (!commanding && input.Pressed(input.Heavy)) kit.CmdHeavyPressed();
            if (input.Released(input.Heavy)) kit.CmdHeavyReleased();
            if (input.Pressed(input.Block)) kit.CmdBlockPressed();
            if (input.Released(input.Block)) kit.CmdBlockReleased();
            if (!commanding && input.Pressed(input.Dodge))
                kit.CmdDodge(worldMove.sqrMagnitude > 0.01f ? worldMove : -transform.forward);
            if (input.Pressed(input.Special1)) kit.CmdSpecial(0);
            if (input.Pressed(input.Special2)) kit.CmdSpecial(1);
            if (input.Pressed(input.Special3)) kit.CmdSpecial(2);
            if (input.Pressed(input.Ultimate)) kit.CmdUltimate();
            if (input.Pressed(input.Place)) kit.CmdPlace();

            if (input.Pressed(input.Mark))
            {
                // Condor's Eye (Kuntur) marks; every kit locks on with the same press.
                kit.CmdMark();
                if (character.Targeting != null) character.Targeting.ToggleOrCycle(fwd);
            }

        }

        /// <summary>Interact / Grapple — interactables that override the kit (Spare) win; then the kit's context action (Grounding
        /// Throw, skipped when combat is disabled); then other interactables. Also drives hold-to-interact.</summary>
        void HandleInteract(GameInput input, CombatKit kit)
        {
            var sensor = character.Sensor;
            bool combat = kit != null && !character.CombatDisabled;
            if (input.Pressed(input.Interact))
            {
                bool done = false;
                if (sensor != null && sensor.Current != null && sensor.Current.OverridesKitContext) done = sensor.Press();
                if (!done && combat) done = kit.TryContextAction();
                if (!done && sensor != null) sensor.Press();
            }
            if (sensor != null)
            {
                if (input.Held(input.Interact)) sensor.Hold(Time.deltaTime);
                else if (sensor.IsHolding) sensor.CancelHold();
            }
        }
    }
}
