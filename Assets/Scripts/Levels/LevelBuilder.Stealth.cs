// Implements: placeholder layouts for 02 M0.1 "Night Raid" (village → cover route → low stone wall), M5.5 "Sigchos" (highland trail
// → open ground), M5.6 "Stone Face" (holding quarters + execution ground), wired to their directors in Assets/Scripts/Missions.
using System.Collections.Generic;
using UnityEngine;

namespace Ruminahui
{
    public static partial class LevelBuilder
    {
        static readonly Color RaiderColor = new Color(0.45f, 0.2f, 0.12f);
        static readonly Color SpanishScoutColor = new Color(0.62f, 0.64f, 0.7f);
        static readonly Color CoverColor = new Color(0.12f, 0.18f, 0.1f);

        // ───────────────────────── helpers ─────────────────────────
        static GameObject Npc(string name, Color color, Vector3 pos, float yRot, float scale = 1f, Transform parent = null)
        {
            var go = new GameObject(name);
            go.SetActive(false); // configure before Awake (LESSONS rule #2)
            go.transform.SetParent(parent != null ? parent : dynamic, false);
            go.transform.SetPositionAndRotation(pos, Quaternion.Euler(0f, yRot, 0f));
            var v = go.AddComponent<PlaceholderVisual>();
            v.baseColor = color;
            v.scale = Vector3.one * 0.9f * scale;
            v.offset = new Vector3(0f, 0.9f * scale, 0f);
            v.idleBob = true;
            go.SetActive(true);
            return go;
        }

        static StealthWatcher Watcher(string name, Color color, Vector3 pos, float yRot, Transform parent, bool torch, params Vector3[] patrol)
        {
            var go = new GameObject(name);
            go.SetActive(false);
            go.transform.SetParent(parent != null ? parent : dynamic, false);
            go.transform.SetPositionAndRotation(pos, Quaternion.Euler(0f, yRot, 0f));
            var cc = go.AddComponent<CharacterController>();
            cc.height = 1.8f; cc.radius = 0.4f; cc.center = new Vector3(0f, 0.9f, 0f);
            var motor = go.AddComponent<CharacterMotor>();
            motor.moveSpeed = 3.6f;
            var v = go.AddComponent<PlaceholderVisual>();
            v.baseColor = color;
            v.offset = new Vector3(0f, 0.9f, 0f);
            v.idleBob = true;
            if (torch) v.AddMarker("Torch", PlaceholderShape.Sphere, new Color(1f, 0.6f, 0.15f), new Vector3(0.45f, 0.6f, 0.25f), Vector3.one * 0.22f);
            var w = go.AddComponent<StealthWatcher>();
            w.patrol = new List<Vector3>(patrol);
            go.SetActive(true);
            return w;
        }

        /// <summary>Dark cover patch the player can step into (tall grass / shadow under a terrace lip). No colliders — hiding is a zone.</summary>
        static ZoneTrigger Cover(Vector3 pos, Vector3 size)
        {
            var patch = Block("CoverPatch", pos + Vector3.up * 0.03f, new Vector3(size.x, 0.06f, size.z), CoverColor, false);
            patch.GetComponent<Collider>().enabled = false;
            for (int i = 0; i < 6; i++)
            {
                var reed = Block("Reed", pos + new Vector3(Random.Range(-size.x, size.x) * 0.4f, 0.6f, Random.Range(-size.z, size.z) * 0.4f),
                    new Vector3(0.12f, 1.2f, 0.12f), new Color(0.2f, 0.3f, 0.15f), false);
                reed.GetComponent<Collider>().enabled = false;
            }
            var spot = patch.AddComponent<HidingSpot>();
            spot.size = new Vector3(size.x, 3f, size.z);
            var zone = Point("CoverCheckpoint", pos + Vector3.up).gameObject.AddComponent<ZoneTrigger>();
            zone.size = new Vector3(size.x, 3f, size.z);
            return zone;
        }

        static TalkNpc Villager(string name, Color color, Vector3 pos, float yRot, params string[] lines)
        {
            var go = Npc(name, color, pos, yRot);
            var t = go.AddComponent<TalkNpc>();
            t.npcName = name;
            t.lines = new List<string>(lines);
            return t;
        }

        // ───────────────────────── M0.1 — Night Raid ─────────────────────────
        static void BuildNightRaid()
        {
            Ground(new Vector3(0f, 0f, 14f), new Vector2(60f, 70f));
            // Village around the fire; terraces up-slope (+z).
            for (int i = 0; i < 6; i++)
            {
                float a = i * Mathf.PI * 2f / 6f + 0.3f;
                Block("Hut", new Vector3(Mathf.Cos(a) * 9f, 1.5f, Mathf.Sin(a) * 6f), new Vector3(4f, 3f, 3.5f), WoodColor);
            }
            var fire = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Object.Destroy(fire.GetComponent<Collider>());
            fire.transform.SetParent(dynamic, false);
            fire.transform.position = new Vector3(0f, 0.3f, 0f);
            fire.transform.localScale = new Vector3(0.9f, 0.5f, 0.9f);
            PlaceholderMaterials.Apply(fire.GetComponent<Renderer>(), new Color(1f, 0.55f, 0.15f));
            for (int i = 0; i < 3; i++) Block("Terrace", new Vector3(0f, 0.4f + i * 0.4f, 34f + i * 4f), new Vector3(50f, 0.8f + i * 0.8f, 4f), RockColor);

            var boy = Spawn(CharacterId.Ruminahui, new Vector3(0f, 0f, -2.5f), 0f, true);
            var anta = Npc("Anta", new Color(0.55f, 0.3f, 0.2f), new Vector3(2.2f, 0f, -1f), -60f);
            anta.GetComponent<PlaceholderVisual>().AddMarker("Blade", PlaceholderShape.Cube, new Color(0.8f, 0.8f, 0.85f), new Vector3(0.5f, 0f, 0.3f), new Vector3(0.06f, 0.6f, 0.06f));
            var villagers = new[]
            {
                // PLACEHOLDER lines — 04 calls for "light interactions with 2–3 NPCs" but doesn't write them.
                Villager("Inti Chaska", new Color(0.75f, 0.55f, 0.35f), new Vector3(-2.5f, 0f, 0.5f), 110f,
                    "[placeholder — not in 04] Anta says you went for wood an hour ago.", "[placeholder] Go on. Before it's dark."),
                Villager("Elder", new Color(0.6f, 0.6f, 0.55f), new Vector3(-3.6f, 0f, 1.4f), 100f,
                    "[placeholder — not in 04] The terraces held well this year."),
                Villager("Villager", new Color(0.5f, 0.45f, 0.4f), new Vector3(6f, 0f, 4f), -120f,
                    "[placeholder — not in 04] Your mother's looking for you, boy."),
            };

            var pileGo = Block("WoodPile", new Vector3(10f, 0.4f, 12f), new Vector3(1.6f, 0.8f, 1f), WoodColor, false);
            var pile = pileGo.AddComponent<ScriptedInteractable>();
            pile.prompt = "Gather wood";

            // The raid: torches on the ridge, raiders in the village (inactive until the horn).
            var raid = new GameObject("RaidGroup").transform;
            raid.SetParent(dynamic, false);
            for (int i = 0; i < 8; i++) Npc("RidgeTorch", new Color(1f, 0.55f, 0.1f), new Vector3(-21f + i * 6f, 3.2f, 44f), 180f, 0.5f, raid);
            Watcher("Raider_Patrol1", RaiderColor, new Vector3(-10f, 0f, 14f), 90f, raid, true, new Vector3(-10f, 0f, 14f), new Vector3(10f, 0f, 14f));
            Watcher("Raider_Patrol2", RaiderColor, new Vector3(8f, 0f, 19f), -120f, raid, true, new Vector3(8f, 0f, 19f), new Vector3(-3f, 0f, 22f));
            Watcher("Raider_Guard", RaiderColor, new Vector3(7f, 0f, 25f), -90f, raid, true);
            Watcher("Raider_Patrol3", RaiderColor, new Vector3(-12f, 0f, 22f), 0f, raid, true, new Vector3(-12f, 0f, 22f), new Vector3(-12f, 0f, 10f));
            var raiderA = Npc("Raider_A", RaiderColor, new Vector3(-8f, 0f, 36f), 180f, 1f, raid).transform;
            var raiderB = Npc("Raider_B", RaiderColor, new Vector3(-4f, 0f, 36f), 180f, 1f, raid).transform;
            var raiderC = Npc("Raider_C", RaiderColor, new Vector3(2f, 0f, 36f), 180f, 1f, raid).transform;
            raid.gameObject.SetActive(false);

            // Cover route to the low stone wall.
            var checkpoints = new[]
            {
                Cover(new Vector3(4f, 0f, 9f), new Vector3(2.6f, 0f, 2.6f)),
                Cover(new Vector3(0f, 0f, 16.5f), new Vector3(2.6f, 0f, 2.6f)),
                Cover(new Vector3(-4.5f, 0f, 20.5f), new Vector3(2.6f, 0f, 2.6f)),
                Cover(new Vector3(-6f, 0f, 24.3f), new Vector3(3f, 0f, 2f)),
            };
            Block("LowStoneWall", new Vector3(-6f, 0.5f, 27f), new Vector3(6f, 1f, 0.6f), RockColor);
            var wallZone = Point("WallZone", new Vector3(-6f, 1f, 26f)).gameObject.AddComponent<ZoneTrigger>();
            wallZone.size = new Vector3(5f, 3f, 2f);
            wallZone.armed = false;

            var dir = dynamic.gameObject.AddComponent<NightRaidDirector>();
            dir.boy = boy; dir.anta = anta.transform; dir.raiderA = raiderA; dir.raiderB = raiderB; dir.raiderC = raiderC;
            dir.woodPile = pile; dir.villagers = villagers; dir.torches = raid.gameObject; dir.wallZone = wallZone; dir.coverCheckpoints = checkpoints;
            dir.wallHidePoint = Point("WallHide", new Vector3(-6f, 0f, 26.2f), 0f);
            dir.antaFightPoint = Point("AntaFight", new Vector3(-6f, 0f, 31f), 180f);
            dir.raidersExitPoint = Point("RaidersExit", new Vector3(-14f, 0f, 25.5f), 0f);
        }

        // ───────────────────────── M5.5 — Sigchos ─────────────────────────
        static void BuildSigchos()
        {
            Ground(new Vector3(0f, 0f, 35f), new Vector2(44f, 90f));
            for (int i = 0; i < 8; i++)
                Block("Boulder", new Vector3(i % 2 == 0 ? -7f : 7f, 1.2f, 6f + i * 5f), new Vector3(3f, 2.4f, 2.5f), RockColor);
            var r = Spawn(CharacterId.Ruminahui, Vector3.zero, 0f, true);
            r.displayName = "Rumiñahui";

            Cover(new Vector3(-3f, 0f, 11f), new Vector3(2.6f, 0f, 2.6f));
            Cover(new Vector3(3f, 0f, 22f), new Vector3(2.6f, 0f, 2.6f));
            Cover(new Vector3(-2f, 0f, 33f), new Vector3(2.6f, 0f, 2.6f));
            Watcher("Scout_Patrol1", SpanishScoutColor, new Vector3(-14f, 0f, 18f), 90f, null, false, new Vector3(-14f, 0f, 18f), new Vector3(-5f, 0f, 26f));
            Watcher("Scout_Patrol2", SpanishScoutColor, new Vector3(14f, 0f, 30f), -90f, null, false, new Vector3(14f, 0f, 30f), new Vector3(6f, 0f, 38f));

            var open = Point("OpenGround", new Vector3(0f, 1f, 52f)).gameObject.AddComponent<ZoneTrigger>();
            open.size = new Vector3(44f, 4f, 14f);
            open.armed = false;

            var closing = new List<StealthWatcher>();
            for (int i = 0; i < 3; i++) closing.Add(Watcher("Scout_Closing_L" + i, SpanishScoutColor, new Vector3(-20f, 0f, 48f + i * 4f), 90f, null, false));
            for (int i = 0; i < 3; i++) closing.Add(Watcher("Scout_Closing_R" + i, SpanishScoutColor, new Vector3(20f, 0f, 50f + i * 4f), -90f, null, false));

            var dir = dynamic.gameObject.AddComponent<SigchosDirector>();
            dir.ruminahui = r; dir.openGround = open; dir.closingScouts = closing;
        }

        // ───────────────────────── M5.6 — Stone Face ─────────────────────────
        static void BuildStoneFace()
        {
            // Holding quarters: sparse, functional.
            Ground(Vector3.zero, new Vector2(10f, 10f));
            Block("WallN", new Vector3(0f, 2f, 5f), new Vector3(10f, 4f, 0.4f), new Color(0.35f, 0.32f, 0.3f));
            Block("WallW", new Vector3(-5f, 2f, 0f), new Vector3(0.4f, 4f, 10f), new Color(0.35f, 0.32f, 0.3f));
            Block("WallE", new Vector3(5f, 2f, 0f), new Vector3(0.4f, 4f, 10f), new Color(0.35f, 0.32f, 0.3f));
            Block("Bench", new Vector3(0f, 0.25f, -0.3f), new Vector3(1.2f, 0.5f, 0.6f), WoodColor);
            var r = Spawn(CharacterId.Ruminahui, new Vector3(0f, 0f, 0f), 0f, true);
            var interpreter = Npc("Interpreter", new Color(0.55f, 0.5f, 0.42f), new Vector3(1.3f, 0f, 2.2f), 200f);
            var officer = Npc("Officer", new Color(0.7f, 0.15f, 0.2f), new Vector3(-1.4f, 0f, 2.8f), 160f);

            // Execution ground: far away in the same scene (no load between beats).
            Ground(new Vector3(0f, 0f, 80f), new Vector2(30f, 30f));
            var crowd = new GameObject("ExecutionSoldiers").transform;
            crowd.SetParent(dynamic, false);
            for (int i = 0; i < 4; i++) Npc("Soldier", new Color(0.62f, 0.64f, 0.7f), new Vector3(-3f + i * 2f, 0f, 84f), 180f, 1f, crowd);

            var dir = dynamic.gameObject.AddComponent<StoneFaceDirector>();
            dir.ruminahui = r; dir.interpreter = interpreter; dir.officer = officer; dir.executionCrowd = crowd.gameObject;
            dir.quartersSeat = Point("Seat", new Vector3(0f, 0f, 0f), 0f);
            dir.quartersCamPos = Point("QuartersCam", new Vector3(3.6f, 3.2f, -3.8f));
            dir.quartersLook = Point("QuartersLook", new Vector3(0f, 1f, 1.2f));
            dir.executionSpot = Point("ExecutionSpot", new Vector3(0f, 0f, 80f), 180f);
            dir.executionCamPos = Point("ExecutionCam", new Vector3(0f, 1.6f, 77.6f));
            dir.executionLook = Point("ExecutionLook", new Vector3(0f, 1.5f, 80f));
        }
    }
}
