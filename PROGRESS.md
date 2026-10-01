# Rumiñahui — Progress & Handoff

**The single place to see where the project stands and how to continue.** It's written for any person or AI coding tool, on any machine. Update the *Status* section and the checklists at the end of every work session.

| | |
|---|---|
| **Last updated** | 2026-10-01 |
| **Branch** | `feature/systems-build` (all work so far; not merged to `main`) |
| **Code status** | Complete code pass: all systems + content for all 26 missions |
| **Verified** | Offline only: `Tools/CompileCheck/check.sh` → 0 compile errors, 55/55 EditMode logic tests, dialogue data in sync with doc 04 |
| **Not yet verified** | Never opened in a real Unity editor. Input feel, UI layout, URP visuals/post-FX, Addressables loading and balance are untested |
| **Next step** | Install Unity 6 LTS → first compile → fix errors (see [Resume](#2-how-to-resume)) |

---

## 1. What's built

### Systems
| Area | Status | Main files |
|---|---|---|
| Foundations: URP setup tool, Cinemachine 3 cameras, pooling, Addressables streaming, Input System | ✅ code | `Assets/Scripts/Systems/{Core,Camera,Pooling,Streaming,Input}`, `Assets/Scripts/Editor/RuminahuiSetupMenu.cs` |
| Resources: HP (huaca-only regen) / Stamina / shared Focus | ✅ code + tests | `Assets/Scripts/Combat/Shared` |
| Puma / Kuntur / Amaru kits: full movesets, specials, ultimates, 3-tier upgrades | ✅ code | `Assets/Scripts/Combat/{Puma,Kuntur,Amaru}` |
| Enemy roster (9 types) + Atoc boss (3 phases + mandatory Spare) | ✅ code + tests | `Assets/Scripts/Enemies`, `Assets/Scripts/Bosses/AtocBoss.cs` |
| M3.5 forced swap / M5.3 free swap | ✅ code | `Assets/Scripts/Systems/{CharacterSwap,FreeSwap}` |
| AI allies + call-ins (Mark / Trap / Cover) | ✅ code + tests | `Assets/Scripts/Characters/AllyBrain.cs`, `Assets/Scripts/Systems/Party` |
| Dialogue from doc 04 as data + subtitles | ✅ code + tests | `Tools/DialogueImport`, `Assets/Resources/Dialogue`, `Assets/Scripts/Systems/Dialogue` |
| Prompts / QTE, stealth, escort, vision sequences | ✅ code + tests | `Assets/Scripts/Systems/{Prompts,Stealth,Escort}`, `Assets/Scripts/Systems/Camera/VisionSequence.cs` |
| Save/load, upgrade economy + menu, main menu, settings (quality tiers) | ✅ code + tests | `Assets/Scripts/Systems/{Save,Progression}`, `Assets/Scripts/UI` |
| Placeholder visuals + prefab swap path for real art | ✅ code | `Assets/Scripts/Placeholder` |
| Offline compile + test harness (no Unity needed) | ✅ working | `Tools/CompileCheck` |

### Missions (all 26 have content; layouts are code-built placeholders)
| Act | Missions | Notes |
|---|---|---|
| Prologue | M0.1 stealth + WARN HIM · M0.2 training yard | M0.1/M0.2 use doc-04 dialogue |
| I | M1.1 duel you lose · M1.2 Three Worlds vision puzzle · M1.3 encounter · M1.4 walk-and-talk | |
| II | M2.1 gauntlet · M2.2 review + convoy escort · M2.3 ambush + Willka's death · M2.4 promotion + call-in drill | |
| III | M3.1 battle with Chaska · M3.2 Chaska level · M3.3 siege + Lieutenant · M3.4 Atoc boss · M3.5 three-character climax · M3.6 camp + Three-Worlds menu | |
| IV | M4.1 reports + skirmish · M4.2 optional gauntlet · M4.3 north to Quito | |
| V | M5.1 Spanish advance · M5.2 timed evacuation + "light the fire" · M5.3 free-swap puzzle · M5.4 guerrilla ambushes · M5.5 Sigchos · M5.6 Stone Face · M5.7 epilogue | M5.5-M5.7 use doc-04 dialogue |

Plus 8 test scenes (`Test_PumaDummy`, `Test_EarlyEncounter`, `Test_Kuntur`, `Test_Amaru`, `Test_AtocBoss`, `Test_AllEnemies`, `Test_M3_5_ForcedSwap`, `Test_M5_3_FreeSwap`) and `_Boot` (main menu).

Full per-feature detail, with the doc section behind each one: [IMPLEMENTATION_LOG.md](IMPLEMENTATION_LOG.md).

---

## 2. How to resume

### On any machine
```bash
git clone <repo-url> ruminahui
cd ruminahui
git checkout feature/systems-build
```

### Without Unity (sanity check, ~30 s)
Needs the .NET SDK 6+ and python3:
```bash
Tools/CompileCheck/check.sh
```
Expected output: `0 Error(s)` twice, `Dialogue JSON up to date.`, `55 passed, 0 failed`.

### With Unity
1. Install **Unity 6 LTS (6000.0.x)** via Unity Hub.
2. Follow [UNITY_SETUP.md](UNITY_SETUP.md) §1-2: add this folder in the Hub, open it, accept the Input System restart, then run the **Rumiñahui ▸ 1. Configure Project** and **Rumiñahui ▸ 2. Build Scenes** menu items.
3. Fix the Console errors, first error first. Log each in [LESSONS_LEARNED.md](LESSONS_LEARNED.md). The likely ones are already listed there as **verify** entries (L-010, L-011, L-012, L-019, L-022).
4. Work through the checklists below.

### Resume prompt (paste into your AI coding tool, opened at the repo root)
> Read `PROGRESS.md`, then `README.md`, `LESSONS_LEARNED.md` (especially the rules checklist), `IMPLEMENTATION_LOG.md` and `UNITY_SETUP.md`. The design docs in `/docs` are the source of truth. Follow the working rules in PROGRESS.md §5. We're on branch `feature/systems-build`. Continue from the first unchecked item in PROGRESS.md §3. Before any UI change, write a plan and wait for my approval. After each step: run `Tools/CompileCheck/check.sh`, log mistakes in LESSONS_LEARNED.md, update IMPLEMENTATION_LOG.md and PROGRESS.md, and commit.

---

## 3. Next steps (tick as you go)

### Phase 1 — First Unity compile
- [ ] Unity 6 LTS installed; project opens; packages resolve (accept the URP version Unity suggests, see L-011)
- [ ] Rumiñahui ▸ 1. Configure Project ran: URP active, no pink materials (L-012)
- [ ] Rumiñahui ▸ 2. Build Scenes ran: 35 scenes in Build Settings and Addressables
- [ ] Console has **0 compile errors** (log every fix in LESSONS_LEARNED.md)
- [ ] Test Runner ▸ EditMode: all tests pass inside Unity (dialogue loads via `Resources.Load`, L-019)
- [ ] *(optional)* Rumiñahui ▸ 3. Save Placeholder Prefabs

### Phase 2 — Playtest (scene list + what to try: [UNITY_SETUP.md](UNITY_SETUP.md) §3)
- [ ] `_Boot`: main menu, New Game / Continue / Missions / Settings
- [ ] `Test_PumaDummy`: combo, charged heavy, block / **parry** timing, dodge, Earthbreaker
- [ ] `Test_EarlyEncounter`: shield guard-break, skirmisher kiting
- [ ] `Test_Kuntur` / `Test_Amaru`: marks, bolas, Vantage Leap, snares, decoys, riposte
- [ ] `Test_AtocBoss`: three phases, decoys, unblockable string, **Spare**
- [ ] `Test_AllEnemies`: every enemy + call-ins (Z / X / C, or LB + Y/X/B; L-022)
- [ ] `Test_M3_5_ForcedSwap`: no visible load on swaps; section restart on death
- [ ] `Test_M5_3_FreeSwap`: all obstacles, swap lock during the joint lift
- [ ] Missions M0.1 → M5.7 in order (stealth, prompts, escorts, visions, epilogue text card)
- [ ] Save → quit → Continue resumes at the right mission; upgrades persist
- [ ] Settings: quality tier switch, volume, sensitivity, subtitles toggle

### Phase 3 — Unity-only work ([UNITY_SETUP.md](UNITY_SETUP.md) §6 has the code hook for each)
- [ ] Real models + Animator (via `PlaceholderVisual.modelPrefab`), Animator culling
- [ ] Timeline cutscenes in place of the placeholder shot cameras
- [ ] NavMesh for real levels (steering is centralised in 3 methods)
- [ ] Real environments in place of `LevelBuilder` methods; occlusion bake
- [ ] Real audio in `Assets/Audio/...` (import rules automatic); VO via `DialogueRunner.BeatPlayed`
- [ ] Play-mode tests (parry window, guard-break, M3.5 restart, M5.3 swap lock)
- [ ] Balance pass: `grep -rn "PLACEHOLDER-BALANCE" Assets/Scripts`

### Phase 4 — Content gaps (needs writing or design, not just code)
- [ ] Dialogue for M1.1-M4.3 and the village NPCs (currently `[placeholder]`). Write it in `docs/04-dialogue-script.md`, then run the importer
- [ ] M5.2 "deal with collaborators" (no spec yet)
- [ ] Names for the M2.4 drill captains (currently stand-ins)
- [ ] Upgrade economy tuning (1 point per combat mission + 1 per relic, 1 per tier)
- [ ] Merge `feature/systems-build` → `main` once Phase 1-2 pass

---

## 4. Decisions already made (don't re-ask)
| Topic | Decision |
|---|---|
| Engine stack | Unity 6 LTS · URP · Cinemachine 3 · Input System (actions defined in code) · Addressables · uGUI with legacy `Text` (no TextMeshPro import) |
| Content | Levels are code-built at scene start (`LevelBuilder`); scenes hold only a `SceneSetup(contentId)` |
| Placeholders | One `PlaceholderVisual` per object; real art goes in via `modelPrefab` |
| AI allies | Don't spend the player's Focus, can't die (HP floors at 1); call-ins route **by kit** |
| Call-ins | Mark (Kuntur ally) · Trap (Amaru ally) · Cover = the ally pulls you out of the next hit; Z/X/C or LB + Y/X/B; unlocked at M2.4; cooldowns, no Focus |
| Upgrade economy | 1 point per combat mission + 1 per relic; 1 point per tier, bought in order; Kuntur/Amaru tabs from M3.6 |
| Saves | JSON in `Application.persistentDataPath`; auto-save at huacas + on mission complete; Continue = start of the saved mission |
| M1.1 duel | Chaska can't drop below 50% HP; scripted loss at 25% player HP |
| M5.2 | Completes quietly (no "+1 point" message) |
| Vision language | DoF Volume only in visions; hard camera + audio cut back to reality |
| UI | Every UI element so far came from a user-approved plan (see IMPLEMENTATION_LOG) |

---

## 5. Working rules (for whoever continues: human or AI)
1. **Design docs in `/docs` are the source of truth.** Change the doc and the code together, and cite the doc section in a header comment on each script.
2. **Any new or changed UI needs a written plan approved by the project owner first.**
3. **Log every mistake** (compile error, logic bug, wrong API) in `LESSONS_LEARNED.md` with cause, fix and a prevention rule. Read its rules checklist before each work step.
4. **Run `Tools/CompileCheck/check.sh` before every commit.** When code starts using a new Unity API, add it to the stubs with its *real* signature (check the package source), not one invented to make it compile.
5. **Invented numbers** get a `// PLACEHOLDER-BALANCE:` comment.
6. **Never hand-edit `Assets/Resources/Dialogue/*.json`.** Edit doc 04 and run `python3 Tools/DialogueImport/import_dialogue.py`.
7. **Update `IMPLEMENTATION_LOG.md` and this file** at the end of each step. Commit in small batches on a feature branch.

---

## 6. File map
| File | What it is |
|---|---|
| [README.md](README.md) | Project overview + docs index |
| [AI_BUILD_PROMPT.md](AI_BUILD_PROMPT.md) | The original build brief |
| **PROGRESS.md** | This file: status, how to resume, checklists |
| [IMPLEMENTATION_LOG.md](IMPLEMENTATION_LOG.md) | What was built, which doc governs it, every assumption |
| [LESSONS_LEARNED.md](LESSONS_LEARNED.md) | Mistakes + prevention rules (read before working) |
| [UNITY_SETUP.md](UNITY_SETUP.md) | Open in Unity, menu steps, what to test per scene, Unity-only hooks |
| `Tools/CompileCheck/` | Offline compile + test harness ([README](Tools/CompileCheck/README.md)) |
| `Tools/DialogueImport/` | Doc 04 → dialogue JSON importer |
| `Assets/Scripts/` | All game code (one namespace `Ruminahui`) |
| `Assets/Tests/EditMode/` | Unit tests (also run offline by the harness) |

## 7. Session history
| Date | What happened |
|---|---|
| 2026-09-30 | Build-order steps 0-6 written; offline harness built; Cinemachine/URP/Addressables APIs verified against source; doc-04 dialogue as data; ally call-ins |
| 2026-10-01 | Batches A-D: saves, economy, menus, subtitles, prompts, stealth, escort, visions; all 26 missions given content; this file created |
