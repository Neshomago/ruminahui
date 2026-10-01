// Implements: 02-mission-list.md (one scene per mission, content per its gameplay notes) + AI_BUILD_PROMPT.md placeholder rule.
// Levels are built from code at scene start so regenerating scenes never loses content; when real environments arrive, a
// mission's builder is replaced by authored scene content. Static environment gets runtime static batching
// (11-camera-performance.md Part B Step 5.2).
using System.Collections.Generic;
using UnityEngine;

namespace Ruminahui
{
    public static partial class LevelBuilder
    {
        static Transform env;       // static, batched
        static Transform dynamic;   // moving props, logic objects

        public static readonly Color GroundColor = new Color(0.36f, 0.4f, 0.3f);
        public static readonly Color RockColor = new Color(0.45f, 0.42f, 0.4f);
        public static readonly Color WoodColor = new Color(0.4f, 0.3f, 0.2f);

        public static void Build(string contentId, Transform root)
        {
            env = new GameObject("Environment").transform;
            env.SetParent(root, false);
            dynamic = new GameObject("Dynamic").transform;
            dynamic.SetParent(root, false);
            EnsureLight();

            switch (contentId)
            {
                case MissionDatabase.BootScene: BuildBoot(); break;
                case "Test_PumaDummy": case "M0.2": BuildDummyYard(contentId); break;
                case "M1.1": BuildDummyYard(contentId, withChaska: true); break;
                case "M4.2": BuildDummyYard(contentId, optional: true); break;
                case "Test_EarlyEncounter": case "M1.3": BuildEarlyEncounter(contentId); break;
                case "M2.1": BuildGarrisonGauntlet(); break;
                case "M2.3": BuildWillkaAmbush(); break;
                case "M3.1": BuildOpeningMoves(); break;
                case "Test_Kuntur": case "M3.2": BuildKunturGround(contentId); break;
                case "M3.3": BuildSiege(); break;
                case "Test_Amaru": BuildAmaruGround(); break;
                case "Test_AtocBoss": case "M3.4": BuildAtocArena(contentId); break;
                case "Test_AllEnemies": BuildAllEnemies(); break;
                case "Test_M3_5_ForcedSwap": case "M3.5": BuildM35(); break;
                case "M5.1": BuildSpanishAdvance(); break;
                case "M5.2": BuildHardestOrder(); break;
                case "Test_M5_3_FreeSwap": case "M5.3": BuildM53(); break;
                case "M5.4": BuildLongRetreat(); break;
                default: BuildPlaceholderMission(contentId); break;
            }

            StaticBatchingUtility.Combine(env.gameObject);
        }

        // ───────────────────────── helpers ─────────────────────────
        static void EnsureLight()
        {
            if (Object.FindFirstObjectByType<Light>() != null) return;
            var go = new GameObject("Directional Light");
            go.transform.SetParent(env, false);
            go.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            var l = go.AddComponent<Light>();
            l.type = LightType.Directional;
            l.intensity = 1.1f;
            l.shadows = LightShadows.Soft;
        }

        public static GameObject Block(string name, Vector3 center, Vector3 size, Color color, bool isStatic = true)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(isStatic ? env : dynamic, false);
            go.transform.position = center;
            go.transform.localScale = size;
            PlaceholderMaterials.Apply(go.GetComponent<Renderer>(), color);
            return go;
        }

        static GameObject Ground(Vector3 center, Vector2 size) =>
            Block("Ground", center + Vector3.down * 0.5f, new Vector3(size.x, 1f, size.y), GroundColor);

        static Transform Point(string name, Vector3 pos, float yRot = 0f)
        {
            var t = new GameObject(name).transform;
            t.SetParent(dynamic, false);
            t.SetPositionAndRotation(pos, Quaternion.Euler(0f, yRot, 0f));
            return t;
        }

        static PlayerCharacter Spawn(CharacterId id, Vector3 pos, float yRot, bool controlled, AllyMode mode = AllyMode.Combat) =>
            CharacterFactory.Create(id, pos, Quaternion.Euler(0f, yRot, 0f), controlled, mode);

        static Huaca PlaceHuaca(Vector3 pos, float yRot = 0f)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = "Huaca";
            go.transform.SetParent(dynamic, false);
            go.transform.SetPositionAndRotation(pos + Vector3.up * 0.75f, Quaternion.Euler(0f, yRot, 0f));
            go.transform.localScale = new Vector3(0.8f, 0.75f, 0.8f);
            PlaceholderMaterials.Apply(go.GetComponent<Renderer>(), RockColor);
            var cap = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Object.Destroy(cap.GetComponent<Collider>());
            cap.transform.SetParent(go.transform, false);
            cap.transform.localPosition = Vector3.up * 1.1f;
            cap.transform.localScale = Vector3.one * 0.6f;
            PlaceholderMaterials.Apply(cap.GetComponent<Renderer>(), new Color(1f, 0.8f, 0.3f));
            var h = go.AddComponent<Huaca>();
            CheckpointService.Instance?.SetCheckpoint(pos + go.transform.forward * 2f, go.transform.rotation);
            return h;
        }

        static EncounterDirector Encounter(string name, Vector3 pos, float yRot, float activationRadius, params Wave[] waves)
        {
            var t = Point("Encounter_" + name, pos, yRot);
            var e = t.gameObject.AddComponent<EncounterDirector>();
            e.encounterName = name;
            e.activationRadius = activationRadius;
            e.waves = new List<Wave>(waves);
            return e;
        }

        static SpawnEntry S(EnemyType t, float x, float z, float yRot = 180f) => new SpawnEntry(t, new Vector3(x, 0.1f, z), yRot);

        static Wave W(string name, params SpawnEntry[] spawns) => new Wave(name, spawns);

        static VantagePoint Vantage(Vector3 topCenter, Vector3 platformSize)
        {
            Block("VantagePlatform", topCenter + Vector3.down * platformSize.y * 0.5f, platformSize, RockColor);
            var t = Point("VantagePoint", topCenter + Vector3.up * 0.05f);
            var marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Object.Destroy(marker.GetComponent<Collider>());
            marker.transform.SetParent(t, false);
            marker.transform.localScale = new Vector3(0.8f, 0.02f, 0.8f);
            PlaceholderMaterials.Apply(marker.GetComponent<Renderer>(), new Color(0.5f, 0.8f, 1f));
            return t.gameObject.AddComponent<VantagePoint>();
        }

        /// <summary>Glowing marker + zone that completes the mission when reached.</summary>
        static ZoneTrigger ExitZone(Vector3 pos, string label, bool startActive = true)
        {
            var t = Point("MissionExit", pos);
            var pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Object.Destroy(pillar.GetComponent<Collider>());
            pillar.transform.SetParent(t, false);
            pillar.transform.localPosition = Vector3.up * 2f;
            pillar.transform.localScale = new Vector3(0.6f, 2f, 0.6f);
            PlaceholderMaterials.Apply(pillar.GetComponent<Renderer>(), new Color(1f, 0.9f, 0.4f));
            var z = t.gameObject.AddComponent<ZoneTrigger>();
            z.size = new Vector3(3f, 4f, 3f);
            z.Entered += () =>
            {
                ObjectiveTracker.Set(label + " — complete");
                MissionManager.Instance?.CompleteCurrent();
            };
            t.gameObject.SetActive(startActive);
            return z;
        }

        static void OnComplete(EncounterDirector e, GameObject reveal, string objective)
        {
            e.Completed += () =>
            {
                if (reveal != null) reveal.SetActive(true);
                ObjectiveTracker.Set(objective);
            };
        }

        static void Wreckage(Vector3 center, float radius, int count, float height = 1.6f)
        {
            for (int i = 0; i < count; i++)
            {
                float a = i * Mathf.PI * 2f / count;
                var p = center + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * radius;
                var go = Block("Wreckage", p + Vector3.up * height * 0.5f, new Vector3(3f, height, 1f), WoodColor);
                go.transform.rotation = Quaternion.Euler(0f, -a * Mathf.Rad2Deg + 90f, Random.Range(-8f, 8f));
            }
        }

        // ───────────────────────── boot / placeholder ─────────────────────────
        static void BuildBoot()
        {
            Ground(Vector3.zero, new Vector2(30f, 30f));
            Spawn(CharacterId.Ruminahui, Vector3.zero, 0f, true);
            ObjectiveTracker.Set("Main menu. Missions lists every mission and TEST scene.");
            MainMenuUI.Instance?.Open(true);
        }

        static void BuildPlaceholderMission(string id)
        {
            var m = MissionDatabase.Get(id);
            string title = m != null ? $"{m.Id} — {m.Title}" : id;
            Ground(new Vector3(0f, 0f, 10f), new Vector2(30f, 40f));
            var who = id == "M5.7" ? CharacterId.Chaska : CharacterId.Ruminahui;
            Spawn(who, Vector3.zero, 0f, true);
            ExitZone(new Vector3(0f, 0f, 20f), title);
            ObjectiveTracker.Set($"PLACEHOLDER {title} ({(m != null ? m.PlaysAs : "")}). Content not built yet — walk to the light to continue.");
            PlayMissionDialogue(id);
        }

        /// <summary>04-dialogue-script.md: missions with a written script play it (placeholder playback, see DialogueRunner).</summary>
        static void PlayMissionDialogue(string id)
        {
            if (DialogueDatabase.Has(id)) DialogueRunner.PlayMission(id, dynamic.gameObject);
        }
    }
}
