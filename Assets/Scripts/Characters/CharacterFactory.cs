// Implements: AI_BUILD_PROMPT.md placeholder convention — builds Rumiñahui / Chaska / Atoc from components + PlaceholderVisual.
// If a prefab is registered in Resources/PrefabRegistry (Rumiñahui ▸ 3. Save Placeholder Prefabs) that prefab is used instead,
// which is where real models get swapped in later.
using UnityEngine;

namespace Ruminahui
{
    public static class CharacterFactory
    {
        // Colours are the placeholder identity of each character (documented in IMPLEMENTATION_LOG.md).
        public static readonly Color RuminahuiColor = new Color(0.55f, 0.45f, 0.35f); // stone
        public static readonly Color ChaskaColor = new Color(0.3f, 0.6f, 0.95f);      // sky
        public static readonly Color AtocColor = new Color(0.15f, 0.55f, 0.3f);       // serpent green

        public static Color ColorFor(CharacterId id) =>
            id == CharacterId.Chaska ? ChaskaColor : id == CharacterId.Atoc ? AtocColor : RuminahuiColor;

        public static string NameFor(CharacterId id) =>
            id == CharacterId.Chaska ? "Chaska" : id == CharacterId.Atoc ? "Atoc" : "Rumiñahui";

        public static PlayerCharacter Create(CharacterId id, Vector3 position, Quaternion rotation, bool controlled, AllyMode allyMode = AllyMode.Combat)
        {
            GameObject go;
            var prefab = PrefabRegistry.Get("Character/" + id);
            if (prefab != null)
            {
                go = Object.Instantiate(prefab, position, rotation);
                go.name = NameFor(id);
            }
            else
            {
                go = Build(id, forPrefab: false);
                go.transform.SetPositionAndRotation(position, rotation);
            }

            var pc = go.GetComponent<PlayerCharacter>();
            pc.allyMode = allyMode;
            go.SetActive(true);
            if (controlled && PartyManager.Instance != null) PartyManager.Instance.SetControlled(pc, true);
            return pc;
        }

        /// <summary>Builds the character hierarchy inactive (so Awake sees configured fields). Also used by the prefab tool.</summary>
        public static GameObject Build(CharacterId id, bool forPrefab)
        {
            var go = new GameObject(NameFor(id));
            go.SetActive(false);

            var cc = go.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.4f;
            cc.center = new Vector3(0f, 0.9f, 0f);
            cc.stepOffset = 0.4f;
            cc.slopeLimit = 50f;

            var health = go.AddComponent<Health>();
            // PLACEHOLDER-BALANCE: Rumiñahui is the durable one ("immovable object").
            health.max = id == CharacterId.Ruminahui ? 200f : id == CharacterId.Chaska ? 150f : 160f;
            go.AddComponent<Stamina>();
            go.AddComponent<StatusEffects>();
            var target = go.AddComponent<CombatTarget>();
            target.faction = Faction.Player;

            var motor = go.AddComponent<CharacterMotor>();
            // PLACEHOLDER-BALANCE: Puma is deliberately the slowest.
            motor.moveSpeed = id == CharacterId.Ruminahui ? 5.5f : id == CharacterId.Chaska ? 7f : 6.5f;
            motor.jumpHeight = id == CharacterId.Chaska ? 1.8f : 1.2f;

            var visual = go.AddComponent<PlaceholderVisual>();
            visual.shape = PlaceholderShape.Capsule;
            visual.baseColor = ColorFor(id);
            visual.scale = id == CharacterId.Ruminahui ? new Vector3(1.05f, 0.95f, 1.05f) : new Vector3(0.9f, 0.9f, 0.9f);
            visual.offset = new Vector3(0f, 0.9f, 0f);
            visual.idleBob = true;
            // Kit silhouettes: club (Puma), twin blades (Kuntur), coil (Amaru).
            if (id == CharacterId.Ruminahui)
                visual.AddMarker("Warclub", PlaceholderShape.Cube, new Color(0.3f, 0.25f, 0.2f), new Vector3(0.55f, 0f, 0.2f), new Vector3(0.18f, 0.9f, 0.18f));
            else if (id == CharacterId.Chaska)
            {
                visual.AddMarker("BladeL", PlaceholderShape.Cube, new Color(0.85f, 0.9f, 1f), new Vector3(-0.5f, 0f, 0.25f), new Vector3(0.06f, 0.5f, 0.06f));
                visual.AddMarker("BladeR", PlaceholderShape.Cube, new Color(0.85f, 0.9f, 1f), new Vector3(0.5f, 0f, 0.25f), new Vector3(0.06f, 0.5f, 0.06f));
            }
            else
                visual.AddMarker("Coil", PlaceholderShape.Cylinder, new Color(0.1f, 0.3f, 0.15f), new Vector3(0f, -0.1f, 0f), new Vector3(1.1f, 0.05f, 1.1f));

            switch (id)
            {
                case CharacterId.Ruminahui: go.AddComponent<PumaKit>(); break;
                case CharacterId.Chaska: go.AddComponent<KunturKit>(); break;
                default: go.AddComponent<AmaruKit>(); break;
            }

            go.AddComponent<TargetingSystem>();
            go.AddComponent<InteractionSensor>();
            go.AddComponent<PlayerBrain>();
            go.AddComponent<AllyBrain>();
            go.AddComponent<CharacterCameraSet>();

            var pc = go.AddComponent<PlayerCharacter>();
            pc.id = id;
            pc.displayName = NameFor(id);
            pc.color = ColorFor(id);
            return go;
        }
    }
}
