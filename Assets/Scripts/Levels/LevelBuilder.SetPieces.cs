// Implements: 08-atoc-boss-fight.md Section 1 (arena: roughly circular clearing bounded by battle wreckage that doubles as
// cover; visible background battle), 05-m3-5-flow-diagram.md (one continuous M3.5 level: breach, flank, rear line/cage, huaca),
// 06 M5.3 flow (Fallen Gate → Wide Break → Narrow Dark → regroup → Joint Lift → Hiding Chamber).
using System.Collections.Generic;
using UnityEngine;

namespace Ruminahui
{
    public static partial class LevelBuilder
    {
        // ───────────────────────── M3.4 / Test_AtocBoss ─────────────────────────
        static void BuildAtocArena(string id)
        {
            Ground(Vector3.zero, new Vector2(70f, 70f));
            Wreckage(Vector3.zero, 17f, 14, 2.2f);                // the boundary: broken palisade / fallen banners
            var covers = new List<Transform>();
            for (int i = 0; i < 4; i++)
            {
                float a = (i * 90f + 45f) * Mathf.Deg2Rad;
                var p = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * 9f;
                Block("CoverWreck", p + Vector3.up * 1f, new Vector3(3.5f, 2f, 1.2f), WoodColor).transform.rotation = Quaternion.Euler(0f, -a * Mathf.Rad2Deg, 0f);
                covers.Add(Point("CoverPoint", p - p.normalized * 1.3f, -a * Mathf.Rad2Deg + 180f)); // behind the wreck, arena side hidden
            }
            // Ambient chaos: distant, non-interactive "battle" silhouettes.
            for (int i = 0; i < 16; i++)
            {
                float a = i * Mathf.PI * 2f / 16f;
                var p = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * 30f;
                var sil = Block("DistantFighter", p + Vector3.up * 0.9f, new Vector3(0.7f, 1.8f, 0.7f), i % 2 == 0 ? new Color(0.3f, 0.2f, 0.2f) : new Color(0.2f, 0.25f, 0.3f), false);
                sil.GetComponent<Collider>().enabled = false;
                sil.AddComponent<AmbientBattleBob>();
            }

            var player = Spawn(CharacterId.Ruminahui, new Vector3(0f, 0f, -8f), 0f, true);
            if (player.Cameras != null) player.Cameras.SetBossFraming(true);
            var boss = EnemyFactory.Spawn(EnemyType.AtocBoss, new Vector3(0f, 0.1f, 8f), Quaternion.Euler(0f, 180f, 0f)) as AtocBoss;
            if (boss != null)
            {
                boss.coverPoints = covers;
                boss.Spared += () =>
                {
                    if (id == "M3.4") ExitZone(new Vector3(0f, 0f, -14f), "M3.4").gameObject.SetActive(true);
                    else ObjectiveTracker.Set("Spared. (Test scene — Esc to pick another.)");
                };
            }
            ObjectiveTracker.Set("M3.4 The Mercy — Phase 1: Stone Parry (Q) his direct strikes. Watch for the crouch-and-glance before he drops a Snare.");
        }

        // ───────────────────────── M3.5 / Test_M3_5_ForcedSwap ─────────────────────────
        static void BuildM35()
        {
            Ground(new Vector3(0f, 0f, -5f), new Vector2(110f, 90f));
            // The huaca (convergence point) behind the breach.
            var huacaPos = new Vector3(0f, 0f, 22f);
            Block("HuacaStone", huacaPos + new Vector3(0f, 1.5f, 3f), new Vector3(3f, 3f, 2f), RockColor);
            // Breach: a broken palisade line with a gap.
            Block("PalisadeL", new Vector3(-14f, 1.5f, 0f), new Vector3(24f, 3f, 1f), WoodColor);
            Block("PalisadeR", new Vector3(14f, 1.5f, 0f), new Vector3(24f, 3f, 1f), WoodColor);
            // Flank route (Chaska) and rear line (Atoc) separated by walls so each section reads as its own space.
            Block("FlankWall", new Vector3(-20f, 1.5f, -20f), new Vector3(1f, 3f, 30f), RockColor);
            Block("RearWall", new Vector3(20f, 1.5f, -20f), new Vector3(1f, 3f, 30f), RockColor);
            Vantage(new Vector3(-30f, 3f, -12f), new Vector3(4f, 3f, 4f));

            var breach = Point("Breach", new Vector3(0f, 0f, -2f), 180f);
            var flankStart = Point("FlankStart", new Vector3(-32f, 0f, -38f), 0f);
            var rearStart = Point("RearStart", new Vector3(32f, 0f, -40f), 0f);
            var huaca = Point("HuacaPoint", huacaPos, 180f);
            var chaskaGoal = Point("ChaskaGoal", new Vector3(-3f, 0f, -4f));
            var atocGoal = Point("AtocGoal", huacaPos + new Vector3(0f, 0f, -2f));

            // Atoc's cage at the rear.
            var cage = new GameObject("AtocCage");
            cage.transform.SetParent(dynamic, false);
            for (int i = 0; i < 4; i++)
            {
                var bar = Block("CageBar", rearStart.position + new Vector3(i < 2 ? -1.2f : 1.2f, 1.2f, i % 2 == 0 ? -1.2f : 1.2f), new Vector3(0.2f, 2.4f, 0.2f), WoodColor, false);
                bar.transform.SetParent(cage.transform, true);
            }

            var rumi = Spawn(CharacterId.Ruminahui, breach.position, 180f, true);
            var chaska = Spawn(CharacterId.Chaska, flankStart.position, 0f, false, AllyMode.Idle);
            var atoc = Spawn(CharacterId.Atoc, rearStart.position, 0f, false, AllyMode.Idle);

            var eA = Encounter("A_Breach", new Vector3(0f, 0f, -14f), 0f, 0f,
                W("Push 1", S(EnemyType.ShieldBearer, -2f, 0f), S(EnemyType.ShieldBearer, 2f, 0f), S(EnemyType.Skirmisher, 0f, -8f, 0f)),
                W("Push 2", S(EnemyType.ShieldBearerArmored, 0f, 0f), S(EnemyType.HighlandScout, -5f, 2f), S(EnemyType.Skirmisher, 5f, -8f, 0f)));
            eA.transform.rotation = Quaternion.identity;
            var eD = Encounter("D_Flank", new Vector3(-30f, 0f, -20f), 0f, 0f,
                W("Flank", S(EnemyType.Skirmisher, -3f, 4f), S(EnemyType.Skirmisher, 3f, 8f), S(EnemyType.ShieldBearer, 0f, 12f)));
            var eH = Encounter("H_RearLine", new Vector3(30f, 0f, -18f), 0f, 0f,
                W("Rear", S(EnemyType.HighlandScout, -3f, 0f), S(EnemyType.ShieldBearer, 2f, 6f), S(EnemyType.Skirmisher, 0f, 14f)));
            var eL = Encounter("L_FinalWave", new Vector3(0f, 0f, -12f), 0f, 0f,
                W("Final 1", S(EnemyType.ShieldBearerArmored, -3f, 0f), S(EnemyType.ShieldBearerArmored, 3f, 0f), S(EnemyType.Skirmisher, -8f, -6f), S(EnemyType.Skirmisher, 8f, -6f)),
                W("Final 2", S(EnemyType.AtocLieutenant, 0f, -2f), S(EnemyType.HighlandScout, -6f, 4f), S(EnemyType.ShieldBearer, 6f, 0f)));
            foreach (var e in new[] { eA, eD, eH, eL }) e.listenForRespawn = false; // the director owns restarts

            var dir = dynamic.gameObject.AddComponent<ForcedSwapDirector>();
            dir.ruminahui = rumi; dir.chaska = chaska; dir.atoc = atoc;
            dir.breachPoint = breach; dir.flankStart = flankStart; dir.rearStart = rearStart; dir.huacaPoint = huaca;
            dir.encounterA = eA; dir.encounterD = eD; dir.encounterH = eH; dir.encounterL = eL;
            dir.chaskaGoal = chaskaGoal; dir.atocGoal = atocGoal; dir.atocCage = cage;
            PlaceHuaca(huacaPos + new Vector3(3f, 0f, 1f), 180f).range = 2f;
        }

        // ───────────────────────── M5.3 / Test_M5_3_FreeSwap ─────────────────────────
        static void BuildM53()
        {
            const float w = 8f; // path width
            // Ground sections with the ravine gap between z=29 and z=41.
            Ground(new Vector3(0f, 0f, 12f), new Vector2(w * 2f, 34f));   // z -5 … 29
            Ground(new Vector3(0f, 0f, 83f), new Vector2(w * 3f, 84f));   // z 41 … 125
            Block("WallL", new Vector3(-w, 3f, 12f), new Vector3(1f, 6f, 34f), RockColor);
            Block("WallR", new Vector3(w, 3f, 12f), new Vector3(1f, 6f, 34f), RockColor);

            // B: the Fallen Gate — a collapsed stone door across the path.
            Block("GateFrameL", new Vector3(-4.5f, 2.5f, 15f), new Vector3(7f, 5f, 1.5f), RockColor);
            Block("GateFrameR", new Vector3(4.5f, 2.5f, 15f), new Vector3(7f, 5f, 1.5f), RockColor);
            var stone = Block("FallenStone", new Vector3(0f, 1.5f, 15f), new Vector3(2.2f, 3f, 1.4f), new Color(0.5f, 0.45f, 0.4f), false);
            var gate = Point("FallenGate", new Vector3(0f, 0f, 13.5f)).gameObject.AddComponent<FallenGateObstacle>();
            gate.requiredCharacter = CharacterId.Ruminahui; gate.holdDuration = 1.5f; gate.actionText = "Heave the stone"; gate.stone = stone.transform;

            // C: the Wide Break — Chaska Vantage Leaps across, then drops a line (bridge).
            Vantage(new Vector3(0f, 0.3f, 42f), new Vector3(3f, 0.3f, 3f));
            var bridge = Block("RopeBridge", new Vector3(0f, -0.1f, 35f), new Vector3(2f, 0.2f, 13f), WoodColor, false);
            bridge.SetActive(false);
            var anchor = Point("LineAnchor", new Vector3(2.5f, 0f, 43f)).gameObject.AddComponent<LineAnchorObstacle>();
            anchor.requiredCharacter = CharacterId.Chaska; anchor.actionText = "Drop a line for the others"; anchor.bridge = bridge;
            var nearSafe = Point("NearLedgeSafe", new Vector3(0f, 0.2f, 26f));
            var fall = Point("RavineSoftFail", new Vector3(0f, -7f, 35f)).gameObject.AddComponent<SoftFailVolume>();
            fall.size = new Vector3(60f, 6f, 14f); fall.safePoint = nearSafe;

            // Second-section boundaries so the only way on is through the puzzle.
            Block("EdgeL", new Vector3(-12f, 3f, 83f), new Vector3(1f, 6f, 84f), RockColor);
            Block("EdgeR", new Vector3(12f, 3f, 83f), new Vector3(1f, 6f, 84f), RockColor);
            Block("CreviceBarrierL", new Vector3(-7.5f, 3f, 55f), new Vector3(9f, 6f, 1f), RockColor);
            Block("CreviceBarrierR", new Vector3(7.5f, 3f, 55f), new Vector3(9f, 6f, 1f), RockColor);
            // Catch-all below the whole map (no fail state anywhere).
            var abyss = Point("AbyssSoftFail", new Vector3(0f, -30f, 60f)).gameObject.AddComponent<SoftFailVolume>();
            abyss.size = new Vector3(300f, 20f, 300f); abyss.safePoint = Point("StartSafe", new Vector3(0f, 0.2f, 0f));

            // D: the Narrow Dark — a crevice with old rigged deadfalls (visible only to Atoc's trap-sense).
            Block("CreviceL", new Vector3(-2.5f, 3f, 67f), new Vector3(1f, 6f, 24f), RockColor);
            Block("CreviceR", new Vector3(2.5f, 3f, 67f), new Vector3(1f, 6f, 24f), RockColor);
            var creviceEntrance = Point("CreviceEntrance", new Vector3(0f, 0.2f, 53f));
            var narrow = new List<RiggedDeadfall>();
            for (int i = 0; i < 4; i++) narrow.Add(Deadfall(new Vector3(i % 2 == 0 ? -0.7f : 0.7f, 0f, 58f + i * 5f), creviceEntrance));

            // E: regroup at the inner approach.
            var regroup = Point("RegroupZone", new Vector3(0f, 1f, 86f)).gameObject.AddComponent<ZoneTrigger>();
            regroup.size = new Vector3(14f, 4f, 8f); regroup.requireWholeParty = true; regroup.armed = false;

            // F: the Joint Lift.
            var treasure = Block("Treasure", new Vector3(0f, 0.6f, 100f), new Vector3(2.5f, 1.2f, 1.6f), new Color(0.85f, 0.7f, 0.2f), false);
            var brace = Station(CharacterId.Ruminahui, "Brace and lift", new Vector3(0f, 0f, 98.5f), 2f);
            Vantage(new Vector3(9f, 4f, 100f), new Vector3(4f, 4f, 4f));
            var spot = Station(CharacterId.Chaska, "Spot the safe path from the ledge", new Vector3(9f, 4f, 100f), 0f);
            var finalTraps = new List<RiggedDeadfall>
            {
                Deadfall(new Vector3(-1.5f, 0f, 104f), Point("LiftReset", new Vector3(0f, 0.2f, 95f))),
                Deadfall(new Vector3(1.5f, 0f, 106f), Point("LiftReset2", new Vector3(0f, 0.2f, 95f))),
            };
            var clear = Station(CharacterId.Atoc, "Clear the final approach", new Vector3(0f, 0f, 108f), 1f);

            // G: the Hiding Chamber.
            Block("ChamberL", new Vector3(-4f, 2f, 118f), new Vector3(1f, 4f, 10f), RockColor);
            Block("ChamberR", new Vector3(4f, 2f, 118f), new Vector3(1f, 4f, 10f), RockColor);
            Block("ChamberBack", new Vector3(0f, 2f, 123f), new Vector3(9f, 4f, 1f), RockColor);
            var hiding = Point("HidingSpot", new Vector3(0f, 0.6f, 120f));
            var chamber = Point("ChamberZone", new Vector3(0f, 1f, 118f)).gameObject.AddComponent<ZoneTrigger>();
            chamber.size = new Vector3(7f, 4f, 8f); chamber.armed = false;

            // Optional relics.
            Relic("relic_m53_1", "Willka's carved bead", new Vector3(-6f, 0f, 5f));
            Relic("relic_m53_2", "A condor feather charm", new Vector3(9f, 0f, 60f + 20f));
            Relic("relic_m53_3", "Three-Worlds tally cord", new Vector3(-9f, 0f, 110f));

            var rumi = Spawn(CharacterId.Ruminahui, new Vector3(0f, 0f, 0f), 0f, true, AllyMode.Follow);
            var chaska = Spawn(CharacterId.Chaska, new Vector3(2f, 0f, -2f), 0f, false, AllyMode.Follow);
            var atoc = Spawn(CharacterId.Atoc, new Vector3(-2f, 0f, -2f), 0f, false, AllyMode.Follow);

            var dir = dynamic.gameObject.AddComponent<LlanganatesDirector>();
            dir.ruminahui = rumi; dir.chaska = chaska; dir.atoc = atoc;
            dir.fallenGate = gate; dir.lineAnchor = anchor; dir.narrowDarkTraps = narrow; dir.regroupZone = regroup;
            dir.braceStation = brace; dir.spotStation = spot; dir.clearStation = clear; dir.finalApproachTraps = finalTraps;
            dir.treasure = treasure.transform; dir.treasureHidingSpot = hiding; dir.chamberZone = chamber;
        }

        static RiggedDeadfall Deadfall(Vector3 pos, Transform reset)
        {
            var t = Point("Deadfall", pos);
            var markerGo = new GameObject("Marker");
            markerGo.SetActive(false); // configure before Awake builds the primitive (LESSONS_LEARNED L-006)
            markerGo.transform.SetParent(t, false);
            var v = markerGo.AddComponent<PlaceholderVisual>();
            v.shape = PlaceholderShape.Cube;
            v.baseColor = new Color(0.8f, 0.2f, 0.1f);
            v.scale = new Vector3(1.2f, 0.15f, 1.2f);
            v.offset = new Vector3(0f, 0.08f, 0f);
            v.addFacingNose = false;
            markerGo.SetActive(true);
            v.SetVisible(false); // unseen until trap-sense reveals it
            var d = t.gameObject.AddComponent<RiggedDeadfall>();
            d.requiredCharacter = CharacterId.Atoc; d.holdDuration = 1f; d.actionText = "Disarm the deadfall";
            d.resetPoint = reset; d.marker = v;
            return d;
        }

        static JointLiftStation Station(CharacterId who, string role, Vector3 pos, float hold)
        {
            var t = Point("Station_" + who, pos);
            var pad = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Object.Destroy(pad.GetComponent<Collider>());
            pad.transform.SetParent(t, false);
            pad.transform.localScale = new Vector3(1.2f, 0.03f, 1.2f);
            PlaceholderMaterials.Apply(pad.GetComponent<Renderer>(), CharacterFactory.ColorFor(who));
            var s = t.gameObject.AddComponent<JointLiftStation>();
            s.requiredCharacter = who; s.roleText = role; s.actionText = role; s.holdDuration = hold; s.holdPoint = t; s.range = 2.2f;
            return s;
        }

        static void Relic(string id, string title, Vector3 pos)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "Relic_" + id;
            Object.Destroy(go.GetComponent<Collider>());
            go.transform.SetParent(dynamic, false);
            go.transform.position = pos + Vector3.up * 0.6f;
            go.transform.localScale = Vector3.one * 0.35f;
            PlaceholderMaterials.Apply(go.GetComponent<Renderer>(), new Color(0.9f, 0.6f, 1f));
            var c = go.AddComponent<Collectible>();
            c.id = id; c.title = title;
        }
    }
}
