// Implements: 03-combat-design.md, Section 4 — Rumiñahui, Chaska and Atoc share one controller shape; each wears its own kit,
// and is driven either by the player (PlayerBrain) or by AI (AllyBrain). 05/06: control can move between them without reloads.
using UnityEngine;

namespace Ruminahui
{
    public enum AllyMode { Combat, Follow, Hold, Idle }

    [RequireComponent(typeof(CharacterMotor), typeof(CombatTarget))]
    public class PlayerCharacter : MonoBehaviour
    {
        public CharacterId id;
        public string displayName = "Rumiñahui";
        public Color color = Color.gray;
        [Tooltip("What this character does while the player controls someone else.")]
        public AllyMode allyMode = AllyMode.Combat;
        [Tooltip("AI allies can't die (HP floors at 1) — ASSUMPTION, see IMPLEMENTATION_LOG.")]
        public bool alliesCannotDie = true;

        public bool IsPlayerControlled { get; private set; }
        public CombatKit Kit { get; private set; }
        public CharacterMotor Motor { get; private set; }
        public CombatTarget Target { get; private set; }
        public Health Health { get; private set; }
        public Stamina Stamina { get; private set; }
        public StatusEffects Status { get; private set; }
        public PlaceholderVisual Visual { get; private set; }
        public CharacterCameraSet Cameras { get; private set; }
        public TargetingSystem Targeting { get; private set; }
        public InteractionSensor Sensor { get; private set; }
        public PlayerBrain PlayerBrain { get; private set; }
        public AllyBrain AllyBrain { get; private set; }
        /// <summary>Target chosen by the AI ally brain (kits read it for aim when not player-controlled).</summary>
        public CombatTarget AllyTarget { get; set; }
        /// <summary>Scripted states (M3.5 "drops to one knee", M5.3 station holds) freeze the brain.</summary>
        public bool ScriptLocked { get; set; }

        public FocusPool Focus => PartyManager.Instance != null ? PartyManager.Instance.Focus : null;

        void Awake()
        {
            Kit = GetComponent<CombatKit>();
            Motor = GetComponent<CharacterMotor>();
            Target = GetComponent<CombatTarget>();
            Health = GetComponent<Health>();
            Stamina = GetComponent<Stamina>();
            Status = GetComponent<StatusEffects>();
            Visual = GetComponent<PlaceholderVisual>();
            Cameras = GetComponent<CharacterCameraSet>();
            Targeting = GetComponent<TargetingSystem>();
            Sensor = GetComponent<InteractionSensor>();
            PlayerBrain = GetComponent<PlayerBrain>();
            AllyBrain = GetComponent<AllyBrain>();
            Motor.useTimeDilation = true;
            // Don't call ApplyControlState here: sibling Awake order is undefined, the kit may not be initialised yet.
            IsPlayerControlled = false;
            if (PlayerBrain != null) PlayerBrain.enabled = false;
            if (AllyBrain != null) AllyBrain.enabled = true;
            Health.minimumHp = alliesCannotDie ? 1f : 0f;
        }

        void OnEnable() { if (PartyManager.Instance != null) PartyManager.Instance.Register(this); }
        void OnDisable() { if (PartyManager.Instance != null) PartyManager.Instance.Unregister(this); }

        /// <summary>Called only by PartyManager.SetControlled.</summary>
        public void ApplyControlState(bool controlled)
        {
            IsPlayerControlled = controlled;
            if (PlayerBrain != null) PlayerBrain.enabled = controlled;
            if (AllyBrain != null) AllyBrain.enabled = !controlled;
            if (Targeting != null && !controlled) Targeting.ClearLock();
            if (Kit != null)
            {
                Kit.CmdBlockReleased();   // held inputs don't survive a swap
                Kit.CmdHeavyReleased();
                if (Kit.IsBusy) Kit.CancelAction();
            }
            Motor.SetMoveInput(Vector3.zero);
            Health.minimumHp = !controlled && alliesCannotDie ? 1f : 0f;
            AllyTarget = null;
        }

        public void RespawnAt(Vector3 position, Quaternion rotation)
        {
            Motor.Teleport(position, rotation);
            Health.ResetFull();
            Stamina.ResetFull();
            Status.ClearAll();
            if (Kit != null) Kit.OnRespawn();
            Target.stealthed = false;
            if (Visual != null) { Visual.SetVisible(true); Visual.ResetPose(); Visual.SetBaseColor(color); }
            if (Cameras != null) Cameras.SnapYawTo(rotation.eulerAngles.y);
        }
    }
}
