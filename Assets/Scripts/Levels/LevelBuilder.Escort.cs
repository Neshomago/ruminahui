// Implements: placeholder layouts for 02 M2.2 (military review → convoy road with two ambushes), M2.3 (mountain pass ambush with
// Willka beside you), M5.2 (Quito streets: evacuees to the north gate under a timer, then the granary).
using System.Collections.Generic;
using UnityEngine;

namespace Ruminahui
{
    public static partial class LevelBuilder
    {
        static EscortTarget EscortUnit(string name, Vector3 pos, float yRot, float hp, Color color, bool cart, params Vector3[] path)
        {
            var go = new GameObject(name);
            go.SetActive(false); // configure before Awake (LESSONS rule #2)
            go.transform.SetParent(dynamic, false);
            go.transform.SetPositionAndRotation(pos, Quaternion.Euler(0f, yRot, 0f));
            var cc = go.AddComponent<CharacterController>();
            cc.height = 1.6f; cc.radius = cart ? 0.8f : 0.5f; cc.center = new Vector3(0f, 0.8f, 0f);
            var health = go.AddComponent<Health>();
            health.max = hp;
            go.AddComponent<StatusEffects>();
            var target = go.AddComponent<CombatTarget>();
            target.faction = Faction.Player;
            target.radius = cart ? 0.9f : 0.5f;
            var motor = go.AddComponent<CharacterMotor>();
            motor.moveSpeed = 5f;
            var v = go.AddComponent<PlaceholderVisual>();
            v.baseColor = color;
            if (cart)
            {
                v.shape = PlaceholderShape.Cube;
                v.scale = new Vector3(1.6f, 1f, 2.4f);
                v.offset = new Vector3(0f, 0.7f, 0f);
                v.addFacingNose = false;
                v.AddMarker("Llama", PlaceholderShape.Capsule, new Color(0.85f, 0.8f, 0.7f), new Vector3(0f, 0.1f, 1.9f), new Vector3(0.5f, 0.6f, 0.5f));
            }
            else
            {
                // A family group: three small capsules.
                v.scale = new Vector3(0.7f, 0.75f, 0.7f);
                v.offset = new Vector3(0f, 0.75f, 0f);
                v.AddMarker("Child", PlaceholderShape.Capsule, color, new Vector3(0.7f, -0.25f, -0.3f), new Vector3(0.45f, 0.5f, 0.45f));
                v.AddMarker("Elder", PlaceholderShape.Capsule, color, new Vector3(-0.7f, -0.05f, -0.4f), new Vector3(0.6f, 0.7f, 0.6f));
            }
            var e = go.AddComponent<EscortTarget>();
            e.displayName = name;
            e.path = new List<Vector3>(path);
            go.SetActive(true);
            return e;
        }

        // ───────────────────────── M2.2 — The Prince Notices ─────────────────────────
        static void BuildConvoy()
        {
            Ground(new Vector3(0f, 0f, 55f), new Vector2(40f, 140f));
            // The review: banners, ranks of soldiers, the prince.
            for (int i = 0; i < 10; i++)
            {
                var soldier = Npc("Soldier", new Color(0.55f, 0.4f, 0.25f), new Vector3(-9f + (i % 5) * 4.5f, 0f, -6f - (i / 5) * 2.5f), 0f);
                soldier.GetComponent<PlaceholderVisual>().idleBob = false; // standing at attention
            }
            var prince = Npc("Atahualpa", new Color(0.9f, 0.75f, 0.25f), new Vector3(0f, 0f, 6f), 180f, 1.05f);
            prince.GetComponent<PlaceholderVisual>().AddMarker("Mascapaicha", PlaceholderShape.Cube, new Color(0.8f, 0.1f, 0.1f), new Vector3(0f, 0.75f, 0.1f), new Vector3(0.5f, 0.12f, 0.3f));
            var talk = prince.AddComponent<TalkNpc>();
            talk.npcName = "Atahualpa";
            talk.lines = new List<string>
            {
                "[placeholder — M2.2 dialogue not in 04] You're the one they call Stone-Face.",
                "[placeholder] The courtiers say you were born nowhere. Good. Neither was I, to them.",
                "[placeholder] A convoy leaves for the garrison. See it there.",
            };
            for (int i = 0; i < 3; i++) Npc("Courtier", new Color(0.7f, 0.55f, 0.6f), new Vector3(-3f + i * 3f, 0f, 9f), 180f);

            var r = Spawn(CharacterId.Ruminahui, new Vector3(0f, 0f, 0f), 0f, true);
            PlaceHuaca(new Vector3(-5f, 0f, 2f), 90f);

            // The road north.
            var path = new[] { new Vector3(0f, 0f, 18f), new Vector3(4f, 0f, 35f), new Vector3(-2f, 0f, 55f), new Vector3(3f, 0f, 78f), new Vector3(0f, 0f, 100f), new Vector3(0f, 0f, 118f) };
            var cart1 = EscortUnit("Supply cart", new Vector3(0f, 0f, 12f), 0f, 160f, WoodColor, true, path);
            var offset = new List<Vector3>();
            foreach (var p in path) offset.Add(p - new Vector3(0f, 0f, 4f));
            var cart2 = EscortUnit("Grain cart", new Vector3(0f, 0f, 8f), 0f, 160f, new Color(0.5f, 0.4f, 0.22f), true, offset.ToArray());
            Block("Garrison", new Vector3(0f, 2f, 124f), new Vector3(14f, 4f, 3f), RockColor);

            var a1 = Encounter("Road ambush 1", new Vector3(4f, 0f, 42f), 180f, 0f,
                W("Raiders", S(EnemyType.Skirmisher, -8f, -4f), S(EnemyType.Skirmisher, 8f, -2f), S(EnemyType.ShieldBearer, 0f, -6f)));
            var a2 = Encounter("Road ambush 2", new Vector3(0f, 0f, 84f), 180f, 0f,
                W("Ridge", S(EnemyType.HighlandScout, -5f, 2f), S(EnemyType.HighlandScout, 5f, 0f), S(EnemyType.ShieldBearerArmored, 0f, -4f), S(EnemyType.Skirmisher, 0f, 6f)));

            var dir = dynamic.gameObject.AddComponent<ConvoyDirector>();
            dir.leader = r; dir.atahualpa = talk;
            dir.escorts = new List<EscortTarget> { cart1, cart2 };
            dir.ambushes = new List<EncounterDirector> { a1, a2 };
        }

        // ───────────────────────── M2.3 — Willka's Last Stand ─────────────────────────
        static void BuildWillkaAmbush()
        {
            Ground(new Vector3(0f, 0f, 30f), new Vector2(12f, 80f));
            Block("CliffL", new Vector3(-7f, 4f, 30f), new Vector3(2f, 8f, 80f), RockColor);
            Block("CliffR", new Vector3(7f, 4f, 30f), new Vector3(2f, 8f, 80f), RockColor);
            var r = Spawn(CharacterId.Ruminahui, Vector3.zero, 0f, true);
            var willka = Npc("Willka", new Color(0.65f, 0.55f, 0.4f), new Vector3(1.6f, 0f, -1.2f), 0f);
            var follow = willka.AddComponent<NpcFollower>();
            follow.target = r.transform;
            var e = Encounter("Pass ambush", new Vector3(0f, 0f, 25f), 0f, 0f,
                W("Hidden", S(EnemyType.HighlandScout, -3f, 0f), S(EnemyType.HighlandScout, 3f, 6f), S(EnemyType.HighlandScout, -2f, 12f)),
                W("Pinned", S(EnemyType.ShieldBearer, 0f, 10f), S(EnemyType.Skirmisher, -3f, 16f), S(EnemyType.Skirmisher, 3f, 16f), S(EnemyType.HighlandScout, 0f, -8f)));
            e.listenForRespawn = true;
            var dir = dynamic.gameObject.AddComponent<WillkaLastStandDirector>();
            dir.ruminahui = r; dir.willka = willka.transform; dir.ambush = e;
        }

        // ───────────────────────── M5.2 — The Hardest Order ─────────────────────────
        static void BuildHardestOrder()
        {
            Ground(new Vector3(0f, 0f, 30f), new Vector2(60f, 90f));
            for (int i = 0; i < 10; i++) Block("House", new Vector3(i % 2 == 0 ? -13f : 13f, 2f, -6f + i * 7f), new Vector3(7f, 4f, 5f), WoodColor);
            Block("NorthGateL", new Vector3(-6f, 3f, 70f), new Vector3(6f, 6f, 2f), RockColor);
            Block("NorthGateR", new Vector3(6f, 3f, 70f), new Vector3(6f, 6f, 2f), RockColor);

            var r = Spawn(CharacterId.Ruminahui, new Vector3(0f, 0f, -4f), 0f, true);
            PlaceHuaca(new Vector3(-5f, 0f, -6f), 90f);

            var families = new List<EscortTarget>();
            var colors = new[] { new Color(0.75f, 0.6f, 0.45f), new Color(0.6f, 0.5f, 0.6f), new Color(0.5f, 0.6f, 0.5f) };
            for (int i = 0; i < 3; i++)
            {
                float x = -3f + i * 3f;
                families.Add(EscortUnit($"Family {i + 1}", new Vector3(x, 0f, 2f - i), 0f, 70f, colors[i], false,
                    new Vector3(x * 0.5f, 0f, 22f), new Vector3(x, 0f, 45f), new Vector3(x * 0.4f, 0f, 74f)));
            }

            var w1 = Encounter("Firing line", new Vector3(0f, 0f, 34f), 180f, 0f,
                W("Firing line", S(EnemyType.Arquebusier, -8f, -10f), S(EnemyType.Arquebusier, 8f, -10f), S(EnemyType.SpanishInfantry, 0f, 0f)));
            var w2 = Encounter("Second line", new Vector3(0f, 0f, 58f), 180f, 0f,
                W("Second line", S(EnemyType.Arquebusier, 0f, -8f), S(EnemyType.SpanishInfantry, -3f, 2f), S(EnemyType.SpanishInfantry, 3f, 2f)));
            w1.WaveStarted += _ => GroupInfantry();
            w2.WaveStarted += _ => GroupInfantry();

            // The granary, back in the city.
            Block("Granary", new Vector3(0f, 2.5f, 12f), new Vector3(6f, 5f, 5f), new Color(0.55f, 0.45f, 0.3f));
            var torch = Point("LightTheFire", new Vector3(0f, 0f, 8.8f)).gameObject.AddComponent<ScriptedInteractable>();
            torch.prompt = "Light the fire";
            torch.holdDuration = 2.5f;
            var fires = new GameObject("Fires").transform;
            fires.SetParent(dynamic, false);
            for (int i = 0; i < 12; i++)
            {
                var f = Npc("Fire", new Color(1f, 0.45f, 0.1f), new Vector3(i % 2 == 0 ? -13f : 13f, 3.5f, -6f + (i % 10) * 7f), 0f, 0.8f, fires);
                f.GetComponent<PlaceholderVisual>().addFacingNose = false;
            }
            Npc("GranaryFire", new Color(1f, 0.5f, 0.1f), new Vector3(0f, 5f, 12f), 0f, 1.6f, fires);

            var dir = dynamic.gameObject.AddComponent<HardestOrderDirector>();
            dir.leader = r; dir.escorts = families; dir.ambushes = new List<EncounterDirector> { w1, w2 };
            dir.lightTheFire = torch; dir.fires = fires.gameObject;
        }
    }
}
