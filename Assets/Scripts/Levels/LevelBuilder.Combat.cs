// Implements: 02-mission-list.md gameplay notes for the combat missions (M0.2, M1.1, M1.3, M2.1, M2.3, M3.1, M3.2, M3.3, M4.2,
// M5.1, M5.2, M5.4) and the per-system test scenes — placeholder arenas, encounters per 06 Enemy Roster first appearances.
using UnityEngine;

namespace Ruminahui
{
    public static partial class LevelBuilder
    {
        // M0.2 / M1.1 / M4.2 / Test_PumaDummy — training dummies (M4.2: optional, skippable rage gauntlet).
        static void BuildDummyYard(string id, bool withChaska = false, bool optional = false)
        {
            Ground(new Vector3(0f, 0f, 8f), new Vector2(40f, 40f));
            Spawn(CharacterId.Ruminahui, Vector3.zero, 0f, true);
            if (withChaska) Spawn(CharacterId.Chaska, new Vector3(4f, 0f, -2f), 0f, false, AllyMode.Idle);
            PlaceHuaca(new Vector3(-6f, 0f, -2f), 90f);

            var d1 = EnemyFactory.Spawn(EnemyType.TrainingDummy, new Vector3(-3f, 0.1f, 6f), Quaternion.Euler(0f, 180f, 0f)) as TrainingDummy;
            var d2 = EnemyFactory.Spawn(EnemyType.TrainingDummy, new Vector3(0f, 0.1f, 7f), Quaternion.Euler(0f, 180f, 0f)) as TrainingDummy;
            var d3 = EnemyFactory.Spawn(EnemyType.TrainingDummy, new Vector3(3f, 0.1f, 6f), Quaternion.Euler(0f, 180f, 0f)) as TrainingDummy;
            if (d1 != null) d1.practiceSwings = false;
            if (d3 != null) d3.practiceSwings = false;
            // d2 swings on a telegraph — practise Stone Parry (Q just before the hit) and Root-Step.

            if (id != "Test_PumaDummy") ExitZone(new Vector3(0f, 0f, 22f), id);
            PlayMissionDialogue(id); // M0.2 has a script in 04
            ObjectiveTracker.Set(optional
                ? "M4.2 — optional: vent on the dummies, or walk to the light to skip."
                : "Training: middle dummy swings (orange flash = tell). Parry with Q right before the hit; heavy = hold RMB.");
        }

        // M1.3 / Test_EarlyEncounter — Shield-Bearers + Skirmishers (06: first appearance M1.3).
        static void BuildEarlyEncounter(string id)
        {
            Ground(new Vector3(0f, 0f, 15f), new Vector2(50f, 60f));
            Block("Rock", new Vector3(-8f, 1f, 14f), new Vector3(3f, 2f, 3f), RockColor);
            Block("Rock", new Vector3(9f, 1.5f, 20f), new Vector3(4f, 3f, 3f), RockColor);
            Spawn(CharacterId.Ruminahui, Vector3.zero, 0f, true);
            PlaceHuaca(new Vector3(-4f, 0f, -3f), 90f);

            var exit = ExitZone(new Vector3(0f, 0f, 38f), id, false);
            var e = Encounter("Border skirmish", new Vector3(0f, 0f, 18f), 0f, 0f,
                W("Shields", S(EnemyType.ShieldBearer, -2f, 0f), S(EnemyType.ShieldBearer, 2f, 0f)),
                W("Slingers", S(EnemyType.Skirmisher, -6f, 8f), S(EnemyType.Skirmisher, 6f, 8f), S(EnemyType.ShieldBearer, 0f, 2f)),
                W("Mixed line", S(EnemyType.ShieldBearer, -3f, 0f), S(EnemyType.ShieldBearer, 3f, 0f),
                    S(EnemyType.Skirmisher, -8f, 10f), S(EnemyType.Skirmisher, 0f, 12f), S(EnemyType.Skirmisher, 8f, 10f)));
            e.Begin();
            OnComplete(e, id == "M1.3" ? exit.gameObject : null, id == "M1.3" ? "Skirmish won. Walk to the light." : "Encounter cleared. Spawn more from F1.");
            ObjectiveTracker.Set("Break shields with Warclub Crush (RMB, hold to charge). Prioritise slingers.");
        }

        // M2.1 — escalating gauntlet across 3 sub-encounters; armored Shield-Bearer variant appears.
        static void BuildGarrisonGauntlet()
        {
            Ground(new Vector3(0f, 0f, 45f), new Vector2(30f, 110f));
            Spawn(CharacterId.Ruminahui, Vector3.zero, 0f, true);
            PlaceHuaca(new Vector3(-4f, 0f, -2f), 90f);
            PlaceHuaca(new Vector3(-6f, 0f, 58f), 90f);
            Encounter("Garrison 1", new Vector3(0f, 0f, 18f), 0f, 12f,
                W("Scouts", S(EnemyType.Skirmisher, -4f, 4f), S(EnemyType.Skirmisher, 4f, 4f), S(EnemyType.ShieldBearer, 0f, 0f)));
            Encounter("Garrison 2", new Vector3(0f, 0f, 45f), 0f, 12f,
                W("Armored", S(EnemyType.ShieldBearerArmored, -2f, 0f), S(EnemyType.ShieldBearer, 2f, 0f), S(EnemyType.Skirmisher, 0f, 8f)));
            var last = Encounter("Garrison 3", new Vector3(0f, 0f, 75f), 0f, 12f,
                W("Wall", S(EnemyType.ShieldBearerArmored, -3f, 0f), S(EnemyType.ShieldBearerArmored, 3f, 0f)),
                W("Pressure", S(EnemyType.Skirmisher, -6f, 6f), S(EnemyType.Skirmisher, 6f, 6f), S(EnemyType.ShieldBearer, 0f, 0f)));
            var exit = ExitZone(new Vector3(0f, 0f, 95f), "M2.1", false);
            OnComplete(last, exit.gameObject, "Gauntlet done. Walk to the light.");
            ObjectiveTracker.Set("M2.1 Garrison Duty — three sub-encounters. Armored shields need a FULLY charged heavy (or two).");
        }


        // M3.1 — large battle; Chaska fights as an AI ally with her own kit.
        static void BuildOpeningMoves()
        {
            Ground(new Vector3(0f, 0f, 20f), new Vector2(70f, 70f));
            Spawn(CharacterId.Ruminahui, Vector3.zero, 0f, true);
            Spawn(CharacterId.Chaska, new Vector3(3f, 0f, -1f), 0f, false, AllyMode.Combat);
            PlaceHuaca(new Vector3(-5f, 0f, -3f), 90f);
            var e = Encounter("Northern approach", new Vector3(0f, 0f, 22f), 0f, 0f,
                W("Front", S(EnemyType.ShieldBearer, -4f, 0f), S(EnemyType.ShieldBearer, 0f, 0f), S(EnemyType.ShieldBearer, 4f, 0f), S(EnemyType.Skirmisher, 0f, 10f)),
                W("Flank", S(EnemyType.Skirmisher, -12f, 4f), S(EnemyType.Skirmisher, 12f, 4f), S(EnemyType.ShieldBearerArmored, 0f, 2f), S(EnemyType.HighlandScout, 8f, -6f)));
            e.Begin();
            var exit = ExitZone(new Vector3(0f, 0f, 45f), "M3.1", false);
            OnComplete(e, exit.gameObject, "Approach taken. Walk to the light.");
            ObjectiveTracker.Set("M3.1 — Chaska fights beside you (AI ally, Kuntur kit).");
        }

        // M3.2 / Test_Kuntur — Chaska: verticality (vantage points), marks, ranged pressure.
        static void BuildKunturGround(string id)
        {
            Ground(new Vector3(0f, 0f, 20f), new Vector2(60f, 60f));
            Spawn(CharacterId.Chaska, Vector3.zero, 0f, true);
            PlaceHuaca(new Vector3(-4f, 0f, -3f), 90f);
            Vantage(new Vector3(-8f, 4f, 12f), new Vector3(4f, 4f, 4f));
            Vantage(new Vector3(8f, 5f, 20f), new Vector3(4f, 5f, 4f));
            Vantage(new Vector3(0f, 6f, 30f), new Vector3(5f, 6f, 5f));
            var e = Encounter("Scouting position", new Vector3(0f, 0f, 20f), 0f, 0f,
                W("Pickets", S(EnemyType.Skirmisher, -6f, 0f), S(EnemyType.Skirmisher, 6f, 4f), S(EnemyType.Skirmisher, 0f, 10f)),
                W("Patrol", S(EnemyType.ShieldBearer, 0f, 0f), S(EnemyType.Skirmisher, -8f, 8f), S(EnemyType.Skirmisher, 8f, 8f), S(EnemyType.Skirmisher, 0f, 14f)));
            e.Begin();
            if (id == "M3.2")
            {
                var exit = ExitZone(new Vector3(0f, 0f, 45f), "M3.2", false);
                OnComplete(e, exit.gameObject, "Atoc's positions scouted. Walk to the light.");
            }
            else OnComplete(e, null, "Cleared. Spawn more from F1.");
            ObjectiveTracker.Set("Chaska: Tab marks (Condor's Eye), RMB Bolas then light = finisher, R Vantage Leap to blue pads, jump+LMB Falling Star, F Sky-Cut hits every mark.");
        }

        // M3.3 — multi-wave siege + Atoc's Lieutenant mini-boss.
        static void BuildSiege()
        {
            Ground(new Vector3(0f, 0f, 20f), new Vector2(60f, 60f));
            Wreckage(new Vector3(0f, 0f, 22f), 16f, 10);
            Spawn(CharacterId.Ruminahui, Vector3.zero, 0f, true);
            PlaceHuaca(new Vector3(-4f, 0f, -3f), 90f);
            var e = Encounter("Mullihambato", new Vector3(0f, 0f, 20f), 0f, 0f,
                W("Wave 1", S(EnemyType.ShieldBearer, -3f, 0f), S(EnemyType.ShieldBearer, 3f, 0f), S(EnemyType.Skirmisher, 0f, 8f)),
                W("Wave 2", S(EnemyType.ShieldBearerArmored, 0f, 0f), S(EnemyType.HighlandScout, -8f, 4f), S(EnemyType.Skirmisher, 6f, 8f), S(EnemyType.Skirmisher, -6f, 8f)),
                W("Lieutenant", S(EnemyType.AtocLieutenant, 0f, 4f)));
            e.Begin();
            var exit = ExitZone(new Vector3(0f, 0f, 44f), "M3.3", false);
            OnComplete(e, exit.gameObject, "The Lieutenant falls. Walk to the light.");
            ObjectiveTracker.Set("M3.3 — the Lieutenant roars (red flash) before an UNBLOCKABLE 3-hit string: dodge it.");
        }

        // Test_Amaru — Atoc: snares, decoys, counters vs. ambushers and an arquebusier.
        static void BuildAmaruGround()
        {
            Ground(new Vector3(0f, 0f, 20f), new Vector2(60f, 60f));
            Spawn(CharacterId.Atoc, Vector3.zero, 0f, true);
            PlaceHuaca(new Vector3(-4f, 0f, -3f), 90f);
            EnemyFactory.Spawn(EnemyType.TrainingDummy, new Vector3(-4f, 0.1f, 5f), Quaternion.Euler(0f, 180f, 0f));
            // Hidden scouts ahead: place Snares (T) near their hiding spots first to neutralise the ambush.
            EnemyFactory.Spawn(EnemyType.HighlandScout, new Vector3(-6f, 0.1f, 16f), Quaternion.Euler(0f, 180f, 0f));
            EnemyFactory.Spawn(EnemyType.HighlandScout, new Vector3(6f, 0.1f, 18f), Quaternion.Euler(0f, 180f, 0f));
            EnemyFactory.Spawn(EnemyType.ShieldBearer, new Vector3(0f, 0.1f, 22f), Quaternion.Euler(0f, 180f, 0f));
            EnemyFactory.Spawn(EnemyType.Arquebusier, new Vector3(0f, 0.1f, 36f), Quaternion.Euler(0f, 180f, 0f));
            ObjectiveTracker.Set("Atoc: T Snare · RMB Coil Grab (pulls into snares) · Shift Shed Skin (decoy baits the arquebus) · Q Venom Riposte (timed). Scouts hide ahead.");
        }

        // Test_AllEnemies — every roster entry, spread out.
        static void BuildAllEnemies()
        {
            Ground(Vector3.zero, new Vector2(140f, 140f));
            Spawn(CharacterId.Ruminahui, Vector3.zero, 0f, true);
            Spawn(CharacterId.Chaska, new Vector3(3f, 0f, -2f), 0f, false);
            Spawn(CharacterId.Atoc, new Vector3(-3f, 0f, -2f), 0f, false);
            PlaceHuaca(new Vector3(0f, 0f, -5f));
            var types = new[] { EnemyType.Skirmisher, EnemyType.ShieldBearer, EnemyType.ShieldBearerArmored, EnemyType.HighlandScout,
                EnemyType.AtocLieutenant, EnemyType.SpanishInfantry, EnemyType.SpanishCavalry, EnemyType.Arquebusier, EnemyType.SpanishOfficer };
            for (int i = 0; i < types.Length; i++)
            {
                float a = i * Mathf.PI * 2f / types.Length;
                var p = new Vector3(Mathf.Sin(a), 0f, Mathf.Cos(a)) * 45f + Vector3.up * 0.1f;
                var marker = Block("Pedestal_" + types[i], p + Vector3.down * 0.45f, new Vector3(4f, 0.1f, 4f), EnemyFactory.ColorFor(types[i]));
                marker.GetComponent<Collider>().enabled = false;
                EnemyFactory.Spawn(types[i], p, Quaternion.LookRotation(-p.normalized));
            }
            ObjectiveTracker.Set("One of each enemy on a ring (45 m out, coloured pads). Allies follow and fight.");
        }

        // M5.1 — Spanish infantry (formation) + cavalry; a canyon removes charge lanes.
        static void BuildSpanishAdvance()
        {
            Ground(new Vector3(0f, 0f, 30f), new Vector2(80f, 90f));
            Block("CanyonL", new Vector3(-3f, 3f, 60f), new Vector3(2f, 6f, 20f), RockColor);
            Block("CanyonR", new Vector3(3f, 3f, 60f), new Vector3(2f, 6f, 20f), RockColor);
            Spawn(CharacterId.Ruminahui, Vector3.zero, 0f, true);
            PlaceHuaca(new Vector3(-4f, 0f, -3f), 90f);
            var e = Encounter("Approaches to Quito", new Vector3(0f, 0f, 25f), 0f, 0f,
                W("Infantry", S(EnemyType.SpanishInfantry, -3f, 0f), S(EnemyType.SpanishInfantry, 0f, 0f), S(EnemyType.SpanishInfantry, 3f, 0f)),
                W("Cavalry", S(EnemyType.SpanishCavalry, -15f, 10f), S(EnemyType.SpanishCavalry, 15f, 10f)));
            e.WaveStarted += w => GroupInfantry();
            e.Begin();
            var exit = ExitZone(new Vector3(0f, 0f, 68f), "M5.1", false);
            OnComplete(e, exit.gameObject, "Held. Walk to the light (through the canyon).");
            ObjectiveTracker.Set("M5.1 — shorter tells, steel shields. Cavalry charges crush blocks: DODGE, or fight in the canyon.");
        }

        static void GroupInfantry()
        {
            var group = Point("Formation", Vector3.zero).gameObject.AddComponent<FormationGroup>();
            foreach (var b in EnemyBrain.Active)
                if (b is SpanishInfantry si && si.formation == null) group.Add(si);
        }


        // M5.4 — guerrilla ambushes with limited healing; officer mini-boss; Chaska/Atoc as AI allies.
        static void BuildLongRetreat()
        {
            Ground(new Vector3(0f, 0f, 40f), new Vector2(50f, 100f));
            Spawn(CharacterId.Ruminahui, Vector3.zero, 0f, true);
            Spawn(CharacterId.Chaska, new Vector3(3f, 0f, -1f), 0f, false);
            Spawn(CharacterId.Atoc, new Vector3(-3f, 0f, -1f), 0f, false);
            // Limited healing: one huaca only, at the start (attrition).
            PlaceHuaca(new Vector3(-5f, 0f, -3f), 90f);
            Encounter("Ambush 1", new Vector3(0f, 0f, 20f), 0f, 10f,
                W("Column", S(EnemyType.SpanishInfantry, -2f, 0f), S(EnemyType.SpanishInfantry, 2f, 0f), S(EnemyType.Arquebusier, 0f, 12f)));
            var last = Encounter("Ambush 2", new Vector3(0f, 0f, 55f), 0f, 10f,
                W("Officer's detachment", S(EnemyType.SpanishOfficer, 0f, 4f), S(EnemyType.SpanishInfantry, -3f, 0f), S(EnemyType.SpanishInfantry, 3f, 0f), S(EnemyType.SpanishCavalry, 12f, 12f)));
            last.WaveStarted += w => GroupInfantry();
            var exit = ExitZone(new Vector3(0f, 0f, 85f), "M5.4", false);
            OnComplete(last, exit.gameObject, "Slipped away again. Walk to the light.");
            ObjectiveTracker.Set("M5.4 — interrupt the Officer's gold 'rally' flash by hitting him, or his troops get stronger.");
        }
    }
}
