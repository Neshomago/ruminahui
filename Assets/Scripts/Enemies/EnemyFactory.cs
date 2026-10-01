// Implements: 06 Enemy Roster (one build recipe per entry) + AI_BUILD_PROMPT.md placeholder convention (distinct flat colour per
// enemy, one PlaceholderVisual) + 11-camera-performance.md Part B Step 2 (all enemies pooled) / Step 8 (capsule colliders only).
using UnityEngine;

namespace Ruminahui
{
    public static class EnemyFactory
    {
        public static string PoolKey(EnemyType t) => "Enemy/" + t;

        public static void RegisterPools()
        {
            var pm = PoolManager.Instance;
            if (pm == null) return;
            foreach (EnemyType t in System.Enum.GetValues(typeof(EnemyType)))
            {
                var type = t;
                pm.Register(PoolKey(type), () =>
                {
                    var prefab = PrefabRegistry.Get(PoolKey(type));
                    return prefab != null ? Object.Instantiate(prefab) : Build(type);
                });
            }
        }

        public static EnemyBrain Spawn(EnemyType type, Vector3 position, Quaternion rotation)
        {
            if (PoolManager.Instance == null) return null;
            var go = PoolManager.Instance.Spawn(PoolKey(type), position, rotation);
            return go != null ? go.GetComponent<EnemyBrain>() : null;
        }

        public static Color ColorFor(EnemyType t)
        {
            switch (t)
            {
                case EnemyType.TrainingDummy: return new Color(0.8f, 0.7f, 0.5f);
                case EnemyType.Skirmisher: return new Color(0.95f, 0.7f, 0.2f);
                case EnemyType.ShieldBearer: return new Color(0.6f, 0.15f, 0.15f);
                case EnemyType.ShieldBearerArmored: return new Color(0.4f, 0.08f, 0.08f);
                case EnemyType.HighlandScout: return new Color(0.4f, 0.45f, 0.2f);
                case EnemyType.AtocLieutenant: return new Color(0.5f, 0.2f, 0.6f);
                case EnemyType.SpanishInfantry: return new Color(0.7f, 0.72f, 0.78f);
                case EnemyType.SpanishCavalry: return new Color(0.75f, 0.75f, 0.8f);
                case EnemyType.Arquebusier: return new Color(0.3f, 0.3f, 0.35f);
                case EnemyType.SpanishOfficer: return new Color(0.8f, 0.1f, 0.2f);
                case EnemyType.AtocBoss: return CharacterFactory.AtocColor;
                case EnemyType.ChaskaSparring: return CharacterFactory.ChaskaColor;
                default: return Color.magenta;
            }
        }

        /// <summary>Builds an enemy inactive (Awake runs on first activation with these values). Also used by the prefab tool.</summary>
        public static GameObject Build(EnemyType type)
        {
            var go = new GameObject(type.ToString());
            go.SetActive(false);

            var cc = go.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.4f;
            cc.center = new Vector3(0f, 0.9f, 0f);

            var health = go.AddComponent<Health>();
            go.AddComponent<StatusEffects>();
            var target = go.AddComponent<CombatTarget>();
            target.faction = Faction.Enemy;
            var motor = go.AddComponent<CharacterMotor>();
            var visual = go.AddComponent<PlaceholderVisual>();
            visual.baseColor = ColorFor(type);
            visual.offset = new Vector3(0f, 0.9f, 0f);
            visual.idleBob = true;

            EnemyBrain brain;
            switch (type)
            {
                case EnemyType.TrainingDummy:
                    health.max = 200f;
                    motor.moveSpeed = 0f;
                    visual.shape = PlaceholderShape.Cylinder;
                    visual.scale = new Vector3(0.8f, 0.9f, 0.8f);
                    visual.idleBob = false;
                    brain = go.AddComponent<TrainingDummy>();
                    break;

                case EnemyType.Skirmisher:
                    health.max = EnemyStats.HpLow;
                    motor.moveSpeed = EnemyStats.SpeedHigh;
                    visual.scale = new Vector3(0.8f, 0.85f, 0.8f);
                    visual.AddMarker("Sling", PlaceholderShape.Sphere, new Color(0.5f, 0.4f, 0.3f), new Vector3(0.45f, 0.3f, 0f), Vector3.one * 0.2f);
                    brain = go.AddComponent<Skirmisher>();
                    break;

                case EnemyType.ShieldBearer:
                case EnemyType.ShieldBearerArmored:
                {
                    bool armored = type == EnemyType.ShieldBearerArmored;
                    health.max = EnemyStats.HpMed * (armored ? 1.3f : 1f);
                    motor.moveSpeed = EnemyStats.SpeedLow;
                    visual.scale = new Vector3(1f, 0.95f, 1f);
                    var shield = visual.AddMarker("Shield", PlaceholderShape.Cube, armored ? new Color(0.5f, 0.5f, 0.55f) : new Color(0.55f, 0.4f, 0.25f),
                        new Vector3(0f, 0.1f, 0.55f), new Vector3(1.0f, 1.3f, 0.12f));
                    var guard = go.AddComponent<ShieldGuard>();
                    guard.guardBreakThreshold = armored ? 2 : 1;   // M2.1 armored variant
                    guard.shieldVisual = shield;
                    brain = go.AddComponent<ShieldBearer>();
                    break;
                }

                case EnemyType.HighlandScout:
                    health.max = EnemyStats.HpLow;
                    motor.moveSpeed = EnemyStats.SpeedMed;
                    visual.scale = new Vector3(0.85f, 0.85f, 0.85f);
                    brain = go.AddComponent<HighlandScout>();
                    break;

                case EnemyType.AtocLieutenant:
                {
                    health.max = EnemyStats.HpHigh;
                    motor.moveSpeed = EnemyStats.SpeedMed;
                    visual.scale = new Vector3(1.15f, 1.05f, 1.15f);
                    var shield = visual.AddMarker("Shield", PlaceholderShape.Cube, new Color(0.35f, 0.15f, 0.4f), new Vector3(-0.35f, 0.1f, 0.5f), new Vector3(0.7f, 1.1f, 0.12f));
                    var guard = go.AddComponent<ShieldGuard>();
                    guard.guardBreakThreshold = 2;
                    guard.shieldVisual = shield;
                    go.AddComponent<BossBarTarget>().displayName = "Atoc's Lieutenant";
                    brain = go.AddComponent<AtocLieutenant>();
                    break;
                }

                case EnemyType.SpanishInfantry:
                {
                    health.max = EnemyStats.HpMed;
                    motor.moveSpeed = EnemyStats.SpeedLowMed;
                    visual.shape = PlaceholderShape.Capsule;
                    visual.AddMarker("Helmet", PlaceholderShape.Sphere, new Color(0.85f, 0.85f, 0.9f), new Vector3(0f, 0.75f, 0f), new Vector3(0.6f, 0.3f, 0.6f));
                    var shield = visual.AddMarker("Shield", PlaceholderShape.Cube, new Color(0.6f, 0.6f, 0.65f), new Vector3(0f, 0.1f, 0.55f), new Vector3(0.9f, 1.2f, 0.1f));
                    var guard = go.AddComponent<ShieldGuard>();
                    guard.guardBreakThreshold = 3;       // steel: more guard-break than Andean shields
                    guard.minBlocksBeforeDrop = 3;
                    guard.maxBlocksBeforeDrop = 4;
                    guard.shieldVisual = shield;
                    brain = go.AddComponent<SpanishInfantry>();
                    break;
                }

                case EnemyType.SpanishCavalry:
                    health.max = EnemyStats.HpMed;
                    motor.moveSpeed = EnemyStats.SpeedVeryHigh * 0.5f; // repositioning speed; the charge itself is faster
                    cc.radius = 0.7f;
                    visual.scale = new Vector3(0.8f, 0.7f, 0.8f);
                    visual.offset = new Vector3(0f, 1.6f, 0f);
                    visual.AddMarker("Horse", PlaceholderShape.Cube, new Color(0.4f, 0.25f, 0.15f), new Vector3(0f, -0.9f, 0f), new Vector3(0.9f, 0.9f, 2.2f));
                    target.aimHeight = 1.4f;
                    target.radius = 0.8f;
                    brain = go.AddComponent<SpanishCavalry>();
                    break;

                case EnemyType.Arquebusier:
                    health.max = EnemyStats.HpLow;
                    motor.moveSpeed = EnemyStats.SpeedLow;
                    visual.AddMarker("Arquebus", PlaceholderShape.Cube, new Color(0.2f, 0.15f, 0.1f), new Vector3(0.3f, 0.25f, 0.6f), new Vector3(0.1f, 0.1f, 1.2f));
                    brain = go.AddComponent<Arquebusier>();
                    break;

                case EnemyType.SpanishOfficer:
                    health.max = EnemyStats.HpHigh;
                    motor.moveSpeed = EnemyStats.SpeedMed;
                    visual.scale = new Vector3(1.05f, 1.05f, 1.05f);
                    visual.AddMarker("Plume", PlaceholderShape.Sphere, new Color(1f, 0.85f, 0.2f), new Vector3(0f, 0.9f, 0f), Vector3.one * 0.3f);
                    go.AddComponent<BossBarTarget>().displayName = "Spanish Officer";
                    brain = go.AddComponent<SpanishOfficer>();
                    break;

                case EnemyType.AtocBoss:
                    health.max = EnemyStats.HpVeryHigh;
                    motor.moveSpeed = EnemyStats.SpeedHigh;
                    visual.scale = new Vector3(0.95f, 1f, 0.95f);
                    visual.AddMarker("Coil", PlaceholderShape.Cylinder, new Color(0.1f, 0.3f, 0.15f), new Vector3(0f, -0.1f, 0f), new Vector3(1.1f, 0.05f, 1.1f));
                    go.AddComponent<BossBarTarget>().displayName = "Atoc";
                    brain = go.AddComponent<AtocBoss>();
                    break;

                case EnemyType.ChaskaSparring:
                    health.max = 300f;
                    motor.moveSpeed = 6.5f;
                    visual.scale = new Vector3(0.9f, 0.9f, 0.9f);
                    visual.AddMarker("BladeL", PlaceholderShape.Cube, new Color(0.6f, 0.45f, 0.3f), new Vector3(-0.5f, 0f, 0.25f), new Vector3(0.06f, 0.5f, 0.06f));
                    visual.AddMarker("BladeR", PlaceholderShape.Cube, new Color(0.6f, 0.45f, 0.3f), new Vector3(0.5f, 0f, 0.25f), new Vector3(0.06f, 0.5f, 0.06f));
                    go.AddComponent<BossBarTarget>().displayName = "Chaska (sparring)";
                    brain = go.AddComponent<SparringChaska>();
                    break;

                default:
                    brain = go.AddComponent<TrainingDummy>();
                    break;
            }

            brain.type = type;
            brain.displayName = type.ToString();
            return go;
        }
    }
}
