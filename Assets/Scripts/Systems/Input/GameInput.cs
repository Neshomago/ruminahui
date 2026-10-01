// Implements: 03-combat-design.md, Sections 1-3 (movesets' input column) using the Unity Input System.
// Actions are defined in code (no .inputactions asset / generated class) so there is nothing to regenerate.
// Keyboard/mouse and gamepad bindings — see IMPLEMENTATION_LOG.md "Controls".
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ruminahui
{
    public class GameInput : Singleton<GameInput>
    {
        public InputAction Move, LookMouse, LookStick, Jump, Light, Heavy, Block, Dodge, Interact, Mark,
            Special1, Special2, Special3, Ultimate, Place, SwapNext, SwapPrev, Swap1, Swap2, Swap3,
            Pause, DebugToggle, HintToggle,
            CallMark, CallTrap, CallCover, CommandModifier;

        public float mouseSensitivity = 0.12f;
        public float stickSensitivity = 160f;

        /// <summary>Menus (pause, debug panel) and cutscenes block gameplay input.</summary>
        public bool GameplayBlocked => menuBlockers > 0 || cutsceneBlockers > 0;
        int menuBlockers;
        int cutsceneBlockers;

        protected override void Awake()
        {
            base.Awake();
            Build();
        }

        void Build()
        {
            Move = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");
            Move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            Move.AddBinding("<Gamepad>/leftStick");

            LookMouse = new InputAction("LookMouse", InputActionType.Value, "<Mouse>/delta", expectedControlType: "Vector2");
            LookStick = new InputAction("LookStick", InputActionType.Value, "<Gamepad>/rightStick", expectedControlType: "Vector2");

            Jump = Button("Jump", "<Keyboard>/space", "<Gamepad>/buttonSouth");
            Light = Button("Light", "<Mouse>/leftButton", "<Gamepad>/buttonWest");
            Heavy = Button("Heavy", "<Mouse>/rightButton", "<Gamepad>/buttonNorth");
            Block = Button("Block", "<Keyboard>/q", "<Gamepad>/leftTrigger");
            Dodge = Button("Dodge", "<Keyboard>/leftShift", "<Gamepad>/buttonEast");
            Interact = Button("Interact", "<Keyboard>/e", "<Gamepad>/rightTrigger");
            Mark = Button("Mark", "<Keyboard>/tab", "<Gamepad>/rightStickPress");
            Mark.AddBinding("<Mouse>/middleButton");
            Special1 = Button("Special1", "<Keyboard>/r", "<Gamepad>/dpad/up");
            Special2 = Button("Special2", "<Keyboard>/f", "<Gamepad>/dpad/right");
            Special3 = Button("Special3", "<Keyboard>/g", "<Gamepad>/dpad/left");
            Ultimate = Button("Ultimate", "<Keyboard>/v", "<Gamepad>/leftStickPress");
            Place = Button("Place", "<Keyboard>/t", "<Gamepad>/dpad/down");
            SwapNext = Button("SwapNext", "<Keyboard>/rightBracket", "<Gamepad>/rightShoulder");
            SwapPrev = Button("SwapPrev", "<Keyboard>/leftBracket", "<Gamepad>/leftShoulder");
            Swap1 = Button("Swap1", "<Keyboard>/1", null);
            Swap2 = Button("Swap2", "<Keyboard>/2", null);
            Swap3 = Button("Swap3", "<Keyboard>/3", null);
            Pause = Button("Pause", "<Keyboard>/escape", "<Gamepad>/start");
            DebugToggle = Button("DebugToggle", "<Keyboard>/f1", "<Gamepad>/select");
            HintToggle = Button("HintToggle", "<Keyboard>/h", null);

            // Ally call-ins (03 Section 4): Z / X / C, or hold LB + Y / X / B ("OneModifier" composite — LESSONS L-022).
            CommandModifier = Button("CommandModifier", null, "<Gamepad>/leftShoulder");
            CallMark = CallIn("CallMark", "<Keyboard>/z", "<Gamepad>/buttonNorth");
            CallTrap = CallIn("CallTrap", "<Keyboard>/x", "<Gamepad>/buttonWest");
            CallCover = CallIn("CallCover", "<Keyboard>/c", "<Gamepad>/buttonEast");
        }

        static InputAction CallIn(string name, string keyboard, string gamepadButton)
        {
            var a = new InputAction(name, InputActionType.Button);
            a.AddBinding(keyboard);
            a.AddCompositeBinding("OneModifier")
                .With("Modifier", "<Gamepad>/leftShoulder")
                .With("Binding", gamepadButton);
            return a;
        }

        /// <summary>While LB is held the face buttons issue call-ins instead of attacking/dodging.</summary>
        public bool CommandModifierHeld => CommandModifier != null && CommandModifier.IsPressed();

        static InputAction Button(string name, string keyboard, string gamepad)
        {
            var a = new InputAction(name, InputActionType.Button);
            if (!string.IsNullOrEmpty(keyboard)) a.AddBinding(keyboard);
            if (!string.IsNullOrEmpty(gamepad)) a.AddBinding(gamepad);
            return a;
        }

        InputAction[] All => new[] { Move, LookMouse, LookStick, Jump, Light, Heavy, Block, Dodge, Interact, Mark,
            Special1, Special2, Special3, Ultimate, Place, SwapNext, SwapPrev, Swap1, Swap2, Swap3, Pause, DebugToggle, HintToggle,
            CallMark, CallTrap, CallCover, CommandModifier };

        void OnEnable()
        {
            if (Move == null) Build();
            foreach (var a in All) a.Enable();
        }

        void OnDisable()
        {
            if (Move == null) return;
            foreach (var a in All) a.Disable();
        }

        protected override void OnDestroy()
        {
            if (Move != null) foreach (var a in All) a.Dispose();
            base.OnDestroy();
        }

        void Update()
        {
            // Cursor is locked only while playing; any menu frees it.
            bool locked = menuBlockers == 0;
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        public void PushMenu() => menuBlockers++;
        public void PopMenu() => menuBlockers = Mathf.Max(0, menuBlockers - 1);
        public void PushCutscene() => cutsceneBlockers++;
        public void PopCutscene() => cutsceneBlockers = Mathf.Max(0, cutsceneBlockers - 1);
        public void ClearCutsceneBlocks() => cutsceneBlockers = 0;

        public Vector2 MoveValue => GameplayBlocked ? Vector2.zero : Move.ReadValue<Vector2>();

        /// <summary>Look delta in degrees for this frame.</summary>
        public Vector2 LookDelta
        {
            get
            {
                if (GameplayBlocked) return Vector2.zero;
                return LookMouse.ReadValue<Vector2>() * mouseSensitivity
                       + LookStick.ReadValue<Vector2>() * (stickSensitivity * Time.unscaledDeltaTime);
            }
        }

        public bool Pressed(InputAction a) => !GameplayBlocked && a.WasPressedThisFrame();
        public bool Released(InputAction a) => a.WasReleasedThisFrame();
        public bool Held(InputAction a) => !GameplayBlocked && a.IsPressed();
    }
}
