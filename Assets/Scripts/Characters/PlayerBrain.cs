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

            if (kit == null) return;

            if (input.Pressed(input.Jump)) kit.CmdJump();
            if (input.Pressed(input.Light)) kit.CmdLight();
            if (input.Pressed(input.Heavy)) kit.CmdHeavyPressed();
            if (input.Released(input.Heavy)) kit.CmdHeavyReleased();
            if (input.Pressed(input.Block)) kit.CmdBlockPressed();
            if (input.Released(input.Block)) kit.CmdBlockReleased();
            if (input.Pressed(input.Dodge))
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

            // Interact / Grapple — interactables that override the kit (Spare) win; then kit context (Grounding Throw); then others.
            var sensor = character.Sensor;
            if (input.Pressed(input.Interact))
            {
                bool done = false;
                if (sensor != null && sensor.Current != null && sensor.Current.OverridesKitContext) done = sensor.Press();
                if (!done) done = kit.TryContextAction();
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
