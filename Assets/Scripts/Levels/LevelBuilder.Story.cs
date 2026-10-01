// Implements: placeholder content for 02's remaining missions via StoryBeatsDirector —
//   M1.1 training duel you lose (tutorial checklist), M1.2 Three Worlds vision puzzle-lite (01: above → here → below),
//   M1.4 / M4.3 walk-and-talk, M2.4 promotion + command tutorial, M3.6 camp + Three-Worlds menu unlock, M4.1 reports + one skirmish,
//   M5.7 epilogue (04 dialogue + closing text card). Lines not written in 04 are marked "[placeholder]".
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ruminahui
{
    public static partial class LevelBuilder
    {
        static readonly Color WillkaColor = new Color(0.65f, 0.55f, 0.4f);
        static readonly Color MessengerColor = new Color(0.8f, 0.7f, 0.3f);

        static StoryBeatsDirector Story(string label)
        {
            var d = dynamic.gameObject.AddComponent<StoryBeatsDirector>();
            d.missionLabel = label;
            return d;
        }

        static ZoneTrigger Zone(Vector3 pos, Vector3 size, bool wholeParty = false)
        {
            var z = Point("StoryZone", pos + Vector3.up).gameObject.AddComponent<ZoneTrigger>();
            z.size = size;
            z.requireWholeParty = wholeParty;
            z.armed = false;
            return z;
        }

        static StoryBeat Beat(string objective, params (string, string)[] lines)
        {
            var b = new StoryBeat { objective = objective };
            b.lines.AddRange(lines);
            return b;
        }

        static TalkNpc Talker(string name, Color color, Vector3 pos, float yRot, params string[] lines) => Villager(name, color, pos, yRot, lines);

        static PlayerCharacter TalkableMember(CharacterId id, Vector3 pos, float yRot, params string[] lines)
        {
            var c = Spawn(id, pos, yRot, false, AllyMode.Idle);
            var t = c.gameObject.AddComponent<TalkNpc>();
            t.npcName = c.displayName;
            t.lines = new List<string>(lines);
            return c;
        }

        // ───────────────────────── M1.1 — Two Prodigies (training duel he loses) ─────────────────────────
        static void BuildTwoProdigies()
        {
            Ground(new Vector3(0f, 0f, 6f), new Vector2(40f, 40f));
            Wreckage(new Vector3(0f, 0f, 6f), 11f, 12, 0.8f); // the training ring's fence
            var r = Spawn(CharacterId.Ruminahui, Vector3.zero, 0f, true);
            Npc("Willka", WillkaColor, new Vector3(-9f, 0f, 2f), 70f);
            var d = Story("M1.1 Two Prodigies");
            d.beats.Add(Beat("Chaska waits in the ring.",
                ("Willka", "[placeholder — M1.1 not in 04] Light, heavy, step, guard. Show me you were listening."),
                ("Chaska", "[placeholder] Try to keep up.")));
            d.beats.Add(new StoryBeat { objective = "Spar with Chaska.", action = () => DuelBout(r) });
            d.beats.Add(Beat("The bout is over.",
                ("Chaska", "[placeholder] Again tomorrow?"),
                ("Willka", "[placeholder] Losing well is a skill too, boy.")));
        }

        static IEnumerator DuelBout(PlayerCharacter r)
        {
            var chaska = EnemyFactory.Spawn(EnemyType.ChaskaSparring, new Vector3(0f, 0.1f, 9f), Quaternion.Euler(0f, 180f, 0f)) as SparringChaska;
            if (chaska == null) yield break;
            int lights = 0;
            bool heavy = false, dodge = false, parry = false;
            System.Action<CombatTarget, DamageInfo, HitResult> onHit = (t, info, res) =>
            {
                if (t.GetComponent<SparringChaska>() == null || string.IsNullOrEmpty(info.AttackName)) return;
                if (info.AttackName.StartsWith("Stone Strike")) lights++;
                if (info.AttackName.StartsWith("Warclub")) heavy = true;
            };
            System.Action<string, float> onFocus = (reason, amt) =>
            {
                if (reason == "Perfect dodge") dodge = true;
                if (reason == "Stone Parry") parry = true;
            };
            r.Target.HitDealt += onHit;
            var focus = PartyManager.Instance.Focus;
            focus.Gained += onFocus;
            r.Health.minimumHp = r.Health.max * 0.2f; // a sparring bout — nobody dies

            float t = 0f;
            bool pressing = false;
            while (r.Health.Normalized > 0.25f)
            {
                bool done = lights >= 3 && heavy && dodge && parry;
                string Mark(bool b) => b ? "✓" : " ";
                ObjectiveTracker.Set($"M1.1 — Light ×3 [{Mark(lights >= 3)}]  Heavy [{Mark(heavy)}]  Dodge her flurry [{Mark(dodge)}]  Parry with Q [{Mark(parry)}]" +
                                     (pressing ? "  — she's pressing now" : ""));
                if (!pressing && (done || t > 60f)) { pressing = true; chaska.Press(2.5f); } // the lesson is learned (or time's up): she ends it
                t += Time.deltaTime;
                yield return null;
            }

            // Scripted loss.
            r.Target.HitDealt -= onHit;
            focus.Gained -= onFocus;
            var input = GameInput.Instance;
            input?.PushCutscene();
            r.ScriptLocked = true;
            if (r.Visual != null) r.Visual.SetPoseScale(new Vector3(1f, 0.6f, 1f)); // down on one knee
            ObjectiveTracker.Say("Chaska", "[placeholder] Yield?");
            yield return new WaitForSeconds(2f);
            PoolManager.Instance.Despawn(chaska.gameObject);
            if (r.Visual != null) r.Visual.ResetPose();
            r.ScriptLocked = false;
            r.Health.minimumHp = 0f;
            input?.PopCutscene();
        }

        // ───────────────────────── M1.2 — The Three Worlds (first vision; puzzle-lite) ─────────────────────────
        static void BuildThreeWorlds()
        {
            Ground(Vector3.zero, new Vector2(40f, 40f));
            var r = Spawn(CharacterId.Ruminahui, new Vector3(0f, 0f, -4f), 0f, true);
            r.CombatDisabled = true; // "no combat"
            PlaceHuaca(new Vector3(0f, 0f, 2f), 180f);
            var willka = Talker("Willka", WillkaColor, new Vector3(1.8f, 0f, 0.5f), -120f,
                "[placeholder — M1.2 telling not in 04] Above us, the condor. It sees what's coming before anyone else does.",
                "[placeholder] Here, the puma. It doesn't run. It stays, and it holds.",
                "[placeholder] Below, the serpent. It goes into the dark and comes back changed.",
                "[placeholder] Touch the stones as I told them: above, here, below.");

            // Order told in 01-story-bible.md: Hanan Pacha (Kuntur) → Kay Pacha (Puma) → Uku Pacha (Amaru).
            var order = new[] { ("Condor stone", CharacterFactory.ChaskaColor, new Vector3(-8f, 0f, 10f)),
                                ("Puma stone", CharacterFactory.RuminahuiColor, new Vector3(0f, 0f, 13f)),
                                ("Serpent stone", CharacterFactory.AtocColor, new Vector3(8f, 0f, 10f)) };
            // Placed in the world in a DIFFERENT order than told, so it's a (light) puzzle.
            var placement = new[] { 1, 2, 0 };
            var stones = new ScriptedInteractable[3];
            var images = new GameObject[3];
            for (int i = 0; i < 3; i++)
            {
                var (name, color, _) = order[i];
                var pos = order[placement[i]].Item3;
                var stone = Block(name, pos + Vector3.up * 0.9f, new Vector3(1.2f, 1.8f, 1.2f), Color.Lerp(RockColor, color, 0.4f), false);
                var si = stone.AddComponent<ScriptedInteractable>();
                si.prompt = "Touch the " + name.ToLower();
                si.oneShot = false;
                si.range = 2.4f;
                stones[i] = si;
                images[i] = VisionImage(i, pos + Vector3.up * 3.5f + Vector3.forward * 4f);
            }

            var d = Story("M1.2 The Three Worlds");
            d.beats.Add(new StoryBeat { objective = "Sit with Willka by the fire (talk to him).", talkTo = willka });
            d.beats.Add(new StoryBeat { objective = "Touch the three stones in the order Willka told them.", action = () => StonePuzzle(stones, images) });
            d.beats.Add(Beat("…", ("Willka", "It's a teaching, boy, not a truth."))); // 01-story-bible.md, Willka
        }

        /// <summary>Placeholder vision imagery: condor (wings), puma (long body), serpent (coil). Hidden outside visions.</summary>
        static GameObject VisionImage(int kind, Vector3 pos)
        {
            var root = new GameObject("VisionImage_" + kind);
            root.transform.SetParent(dynamic, false);
            root.transform.position = pos;
            if (kind == 0)
            {
                Block("Wings", pos, new Vector3(6f, 0.2f, 1.2f), CharacterFactory.ChaskaColor, false).transform.SetParent(root.transform, true);
                Block("Body", pos, new Vector3(0.8f, 0.8f, 2f), CharacterFactory.ChaskaColor, false).transform.SetParent(root.transform, true);
            }
            else if (kind == 1)
            {
                Block("Body", pos, new Vector3(1.2f, 1.2f, 4f), CharacterFactory.RuminahuiColor, false).transform.SetParent(root.transform, true);
                Block("Head", pos + new Vector3(0f, 0.6f, 2.3f), new Vector3(1f, 1f, 1f), CharacterFactory.RuminahuiColor, false).transform.SetParent(root.transform, true);
            }
            else
            {
                for (int i = 0; i < 6; i++)
                {
                    float a = i * 0.9f;
                    Block("Coil", pos + new Vector3(Mathf.Cos(a) * 2f, i * 0.3f, Mathf.Sin(a) * 2f), new Vector3(0.8f, 0.6f, 0.8f), CharacterFactory.AtocColor, false)
                        .transform.SetParent(root.transform, true);
                }
            }
            foreach (var c in root.GetComponentsInChildren<Collider>()) c.enabled = false;
            root.SetActive(false);
            return root;
        }

        static IEnumerator StonePuzzle(ScriptedInteractable[] stonesInToldOrder, GameObject[] images)
        {
            int next = 0;
            int pressed = -1;
            for (int i = 0; i < stonesInToldOrder.Length; i++)
            {
                int idx = i;
                stonesInToldOrder[i].Interacted += () => pressed = idx;
            }
            var vision = VisionSequence.Instance;
            while (next < stonesInToldOrder.Length)
            {
                if (pressed < 0) { yield return null; continue; }
                int got = pressed;
                pressed = -1;
                if (got != next)
                {
                    ObjectiveTracker.Say("Willka", "[placeholder] Not that one yet. Listen again: above, here, below.");
                    next = 0;
                    continue;
                }
                // A short vision vignette for each correct stone.
                var img = images[got];
                img.SetActive(true);
                if (vision != null)
                {
                    yield return vision.Enter(img.transform.position + new Vector3(0f, 1f, -7f), img.transform.position);
                    yield return new WaitForSeconds(2.2f);
                    vision.ExitHard();
                }
                img.SetActive(false);
                next++;
            }
            // All three at once — the first full vision.
            foreach (var img in images) img.SetActive(true);
            if (vision != null)
            {
                var c = (images[0].transform.position + images[1].transform.position + images[2].transform.position) / 3f;
                yield return vision.Enter(c + new Vector3(0f, 2f, -14f), c);
                yield return new WaitForSeconds(3f);
                vision.ExitHard();
            }
            foreach (var img in images) img.SetActive(false);
        }

        // ───────────────────────── M1.4 — Leaving the Forest ─────────────────────────
        static void BuildLeavingTheForest()
        {
            Ground(new Vector3(0f, 0f, 45f), new Vector2(20f, 110f));
            var r = Spawn(CharacterId.Ruminahui, Vector3.zero, 0f, true);
            var chaska = Talker("Chaska", CharacterFactory.ChaskaColor, new Vector3(2.5f, 0f, 3f), -100f,
                "[placeholder — M1.4 not in 04] So you're going.", "[placeholder] Don't get famous without me.");
            var willka = Npc("Willka", WillkaColor, new Vector3(-6f, 0f, 30f), 0f);
            var d = Story("M1.4 Leaving the Forest");
            d.beats.Add(new StoryBeat { objective = "Say goodbye to Chaska.", talkTo = chaska });
            d.beats.Add(new StoryBeat
            {
                objective = "Take the road south.",
                zone = Zone(new Vector3(0f, 0f, 28f), new Vector3(20f, 4f, 4f)),
                lines = { ("Willka", "[placeholder] You didn't think I'd let you walk into an army alone?") },
                action = () => { var f = willka.AddComponent<NpcFollower>(); f.target = r.transform; return null; },
            });
            d.beats.Add(new StoryBeat
            {
                objective = "Keep walking. The world is bigger than Píllaro.",
                zone = Zone(new Vector3(0f, 0f, 92f), new Vector3(20f, 4f, 4f)),
                lines = { ("Willka", "[placeholder] Don't look back yet. Look at it all first.") },
            });
        }

        // ───────────────────────── M2.4 — The Empire Splits (promotion + command tutorial) ─────────────────────────
        static void BuildEmpireSplits()
        {
            Ground(new Vector3(0f, 0f, 8f), new Vector2(40f, 40f));
            var r = Spawn(CharacterId.Ruminahui, Vector3.zero, 0f, true);
            var messenger = Talker("Messenger", MessengerColor, new Vector3(3f, 0f, 4f), -140f,
                "[placeholder — M2.4 not in 04] Huayna Cápac is dead. The empire is splitting between Huáscar and Atahualpa.");
            var prince = Talker("Atahualpa", new Color(0.9f, 0.75f, 0.25f), new Vector3(-3f, 0f, 8f), 160f,
                "[placeholder] I need a general whose only loyalty is to me. Take the command.");
            var d = Story("M2.4 The Empire Splits");
            d.beats.Add(new StoryBeat { objective = "A messenger has news.", talkTo = messenger });
            d.beats.Add(new StoryBeat { objective = "Atahualpa sends for you.", talkTo = prince });
            d.beats.Add(new StoryBeat { objective = "Drill with your captains.", action = () => CommandTutorial(r) });
            d.beats.Add(Beat("Your command is ready.", ("Atahualpa", "[placeholder] Good. You'll need them in the north.")));
        }

        static IEnumerator CommandTutorial(PlayerCharacter r)
        {
            // Drill stand-ins wearing the Kuntur and Amaru kits (Chaska and Atoc arrive in Act III; call-ins route by kit).
            var scout = Spawn(CharacterId.Chaska, r.transform.position + new Vector3(3f, 0f, -1f), 0f, false, AllyMode.Combat);
            scout.displayName = "Scout captain";
            scout.color = new Color(0.45f, 0.55f, 0.65f);
            if (scout.Visual != null) scout.Visual.SetBaseColor(scout.color);
            var trapper = Spawn(CharacterId.Atoc, r.transform.position + new Vector3(-3f, 0f, -1f), 0f, false, AllyMode.Combat);
            trapper.displayName = "Trapper captain";
            trapper.color = new Color(0.4f, 0.55f, 0.45f);
            if (trapper.Visual != null) trapper.Visual.SetBaseColor(trapper.color);
            var dummy = EnemyFactory.Spawn(EnemyType.TrainingDummy, r.transform.position + new Vector3(0f, 0.1f, 7f), Quaternion.Euler(0f, 180f, 0f)) as TrainingDummy;

            bool mark = false, trap = false, cover = false;
            var sys = AllyCommandSystem.Instance;
            System.Action<AllyCommand, PlayerCharacter> onIssued = (c, ally) =>
            {
                if (c == AllyCommand.Mark) mark = true;
                if (c == AllyCommand.Trap) trap = true;
                if (c == AllyCommand.Cover) cover = true;
            };
            sys.Issued += onIssued;
            while (!(mark && trap && cover))
            {
                string M(bool b) => b ? "✓" : " ";
                ObjectiveTracker.Set($"M2.4 — Call-ins: Z Mark the dummy [{M(mark)}] · X Trap [{M(trap)}] · C Cover, then stand near the swinging dummy [{M(cover)}]  (gamepad: hold LB + Y/X/B)");
                yield return null;
            }
            sys.Issued -= onIssued;
            if (dummy != null) PoolManager.Instance.Despawn(dummy.gameObject);
        }

        // ───────────────────────── M3.6 — After the Fire ─────────────────────────
        static void BuildAfterTheFire()
        {
            Ground(new Vector3(0f, 0f, 6f), new Vector2(40f, 40f));
            for (int i = 0; i < 5; i++) Block("Tent", new Vector3(-12f + i * 6f, 1f, 14f), new Vector3(3.5f, 2f, 3f), new Color(0.6f, 0.55f, 0.45f));
            var r = Spawn(CharacterId.Ruminahui, Vector3.zero, 0f, true);
            r.CombatDisabled = true; // "No combat. Dialogue/exploration level."
            var atoc = TalkableMember(CharacterId.Atoc, new Vector3(5f, 0f, 6f), -120f,
                "[placeholder — M3.6 not in 04] You could have put me in a cell.", "[placeholder] …A place, then. I'll take it.");
            var chaska = TalkableMember(CharacterId.Chaska, new Vector3(-5f, 0f, 5f), 120f,
                "[placeholder] What happened at the huaca — you saw it too.", "[placeholder] Don't tell me it was nothing.");
            PlaceHuaca(new Vector3(0f, 0f, 9f), 180f);
            var d = Story("M3.6 After the Fire");
            d.beats.Add(new StoryBeat { objective = "Atoc is still a prisoner. Offer him a place (talk to Atoc).", talkTo = atoc.GetComponent<TalkNpc>() });
            d.beats.Add(new StoryBeat { objective = "Find Chaska.", talkTo = chaska.GetComponent<TalkNpc>() });
            d.beats.Add(new StoryBeat
            {
                objective = "The Three Worlds menu is fully open — Kuntur and Amaru upgrades are now available.",
                action = OpenThreeWorldsMenu,
            });
        }

        static IEnumerator OpenThreeWorldsMenu()
        {
            Progression.Unlock(Unlocks.ThreeWorldsMenu); // 02 M3.6: "full unlock of the Three-Worlds ability menu going forward"
            var menu = UpgradeMenuUI.Instance;
            if (menu == null) yield break;
            menu.Open(true);
            while (menu.IsOpen) yield return null;
        }

        // ───────────────────────── M4.1 — Strange Ships ─────────────────────────
        static void BuildStrangeShips()
        {
            Ground(new Vector3(0f, 0f, 15f), new Vector2(50f, 60f));
            var r = Spawn(CharacterId.Ruminahui, Vector3.zero, 0f, true);
            PlaceHuaca(new Vector3(-4f, 0f, -3f), 90f);
            var messenger = Talker("Messenger", MessengerColor, new Vector3(3f, 0f, 3f), -140f,
                "[placeholder — M4.1 not in 04] Atahualpa is crowned in Cusco.",
                "[placeholder] And… there are reports from the coast. Ships. Strangers, armed, moving inland.");
            var skirmish = Encounter("Local raiders", new Vector3(0f, 0f, 22f), 0f, 0f,
                W("Raiders", S(EnemyType.Skirmisher, -5f, 4f), S(EnemyType.Skirmisher, 5f, 4f), S(EnemyType.ShieldBearer, 0f, 0f)));
            var d = Story("M4.1 Strange Ships");
            d.beats.Add(new StoryBeat { objective = "A messenger from the south.", talkTo = messenger });
            d.beats.Add(new StoryBeat { objective = "Raiders on the north road — an unrelated local threat.", action = () => RunEncounter(skirmish) });
            d.beats.Add(Beat("The road is clear.", ("Rumiñahui", "[placeholder] Ships…")));
        }

        static IEnumerator RunEncounter(EncounterDirector e)
        {
            e.Begin();
            while (!e.IsComplete) yield return null;
        }

        // ───────────────────────── M4.3 — The Execution / North to Quito ─────────────────────────
        static void BuildNorthToQuito()
        {
            Ground(new Vector3(0f, 0f, 40f), new Vector2(24f, 100f));
            var r = Spawn(CharacterId.Ruminahui, Vector3.zero, 0f, true);
            r.CombatDisabled = true; // "Walking/dialogue level"
            Spawn(CharacterId.Chaska, new Vector3(2f, 0f, -2f), 0f, false, AllyMode.Follow);
            Spawn(CharacterId.Atoc, new Vector3(-2f, 0f, -2f), 0f, false, AllyMode.Follow);
            var messenger = Talker("Messenger", MessengerColor, new Vector3(0f, 0f, 6f), 180f,
                "[placeholder — M4.3 not in 04] It's confirmed. Atahualpa is dead. Executed at Cajamarca.");
            var d = Story("M4.3 North to Quito");
            d.beats.Add(new StoryBeat { objective = "A rider on the road.", talkTo = messenger });
            d.beats.Add(new StoryBeat
            {
                objective = "Turn north, toward Quito.",
                zone = Zone(new Vector3(0f, 0f, 45f), new Vector3(24f, 4f, 4f), true),
                lines =
                {
                    ("Chaska", "[placeholder] No more emperor. No more army. Just us."),
                    ("Atoc", "[placeholder] Then we stop being soldiers."),
                    ("Rumiñahui", "[placeholder] We're the last of what's left. North."),
                },
            });
            d.beats.Add(new StoryBeat { objective = "Keep walking.", zone = Zone(new Vector3(0f, 0f, 85f), new Vector3(24f, 4f, 4f)) });
        }

        // ───────────────────────── M5.7 — What the Mountains Keep (epilogue) ─────────────────────────
        static void BuildWhatTheMountainsKeep()
        {
            Ground(new Vector3(0f, 0f, 30f), new Vector2(30f, 80f));
            for (int i = 0; i < 5; i++) Block("Ridge", new Vector3(0f, 0.25f + i * 0.5f, 10f + i * 5f), new Vector3(14f, 0.5f + i, 5f), RockColor);
            for (int i = 0; i < 10; i++) Block("Tree", new Vector3(-12f + (i % 5) * 6f, 3f, 58f + (i / 5) * 4f), new Vector3(1f, 6f, 1f), new Color(0.2f, 0.35f, 0.2f));
            var chaska = Spawn(CharacterId.Chaska, Vector3.zero, 0f, true);
            chaska.CombatDisabled = true; // "no combat"
            var atoc = Spawn(CharacterId.Atoc, new Vector3(0f, 5f, 32f), 180f, false, AllyMode.Idle);
            var d = Story("M5.7 What the Mountains Keep");
            d.beats.Add(new StoryBeat
            {
                objective = "Climb the ridge. Atoc is waiting at the top.",
                zone = Zone(new Vector3(0f, 4.5f, 30f), new Vector3(14f, 6f, 6f)),
                docMission = "M5.7", docFrom = "M5.7.1.01", docTo = "M5.7.1.08",
            });
            d.beats.Add(new StoryBeat
            {
                objective = "Walk the ridge with Atoc, toward the trees.",
                action = () => { atoc.allyMode = AllyMode.Follow; return null; },
            });
            d.beats.Add(new StoryBeat
            {
                zone = Zone(new Vector3(0f, 0f, 56f), new Vector3(30f, 6f, 4f)),
                docMission = "M5.7", docFrom = "M5.7.1.09", docTo = "M5.7.1.09",
                action = FadeToBlackForTextCard,
            });
            d.beats.Add(new StoryBeat { docMission = "M5.7", docFrom = "M5.7.2.01", docTo = "M5.7.2.02", holdAfter = 3f }); // text card over black
        }

        static IEnumerator FadeToBlackForTextCard()
        {
            GameInput.Instance?.PushCutscene();
            if (UIRoot.Instance != null) UIRoot.Instance.SetCombatUIVisible(false);
            if (ScreenFader.Instance != null) yield return ScreenFader.Instance.FadeOut(3f, Color.black);
        }
    }
}
