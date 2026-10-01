# Unity Setup & Testing

## 0. Without Unity: offline check
```bash
Tools/CompileCheck/check.sh
```
Compiles everything against Unity API stubs, checks the dialogue JSON is in sync with `docs/04-dialogue-script.md`, and runs the logic tests (needs the .NET SDK + python3). See `Tools/CompileCheck/README.md`.

After editing the dialogue doc: `python3 Tools/DialogueImport/import_dialogue.py`.

## 1. Open the project (Unity 6 LTS)
1. Unity Hub → **Add → Add project from disk** → select this `ruminahui` folder (the one containing `Assets/` and `Packages/`).
2. Open it with **Unity 6 LTS (6000.0.x)**. The first import downloads the packages in `Packages/manifest.json` (URP, Cinemachine 3, Input System, Addressables, Timeline, Test Framework).
3. If Unity asks to **enable the new Input System backends and restart**, click **Yes**.
4. If the Package Manager reports a version mismatch for URP (it's tied to your exact editor patch), accept the version Unity suggests.

## 2. One-time setup (menu bar → **Rumiñahui**)
1. **1. Configure Project**: creates and assigns the URP assets (Standard + Performance tiers), the placeholder material, the prefab registry, and the Addressables settings.
2. **2. Build Scenes**: generates `_Boot`, the 26 mission scenes (`Assets/Scenes/M0.1.unity` … `M5.7.unity`) and 8 test scenes (`Assets/Scenes/Tests/`), adds them to Build Settings, and marks them Addressable.
3. *(Optional)* **3. Save Placeholder Prefabs**: saves every character/enemy to `Assets/Prefabs/Placeholder` and switches the factories to use them. This is where real art gets swapped in later.

## 3. What to test
Open any scene and press **Play**. `_Boot` opens the **main menu** (New Game / Continue / Missions / Settings / Quit). Esc in a mission opens the pause menu (Upgrades, Settings, mission list).

Saves live in `Application.persistentDataPath` (`ruminahui_save.json`, `ruminahui_settings.json`). Delete them to start fresh.

| Scene | What to try |
|---|---|
| `M0.1` | Talk to villagers, gather wood → raid → sneak cover to cover (eye meter bottom-centre) → WARN HIM prompt |
| `M5.5` | Same no-attack stealth; cross the open ground (or get seen) → scouts close in → STAND / RUN |
| `M5.6` | No control except three STAY SILENT prompts; wide still shots; ends on the wind |
| `M2.2` | Talk to Atahualpa → escort two carts (panel top-centre; carts halt near enemies) |
| `M2.3` | Hard ambush → mid-fight, control is taken away for Willka's death |
| `M5.2` | Evacuate 3 families before the timer ends → light the granary |
| `M1.1` | Spar with Chaska; tick the checklist (light ×3, heavy, dodge, parry) — you're meant to lose |
| `M1.2` | Talk to Willka → touch the stones in the told order (above, here, below) → vision sequences (depth-of-field) |
| `M2.4` | Messenger → Atahualpa → call-in drill (Z / X / C) |
| `M3.6` | Talk to Atoc and Chaska → the Three-Worlds upgrade menu opens |
| `M1.4`, `M4.1`, `M4.3`, `M5.7` | Walk-and-talk beats (M4.1 has one skirmish; M5.7 ends on the text card) |
| `M0.2` | Training yard; doc 04 dialogue plays as subtitles |
| `Tests/Test_PumaDummy` | Light chain, charged heavy (hold RMB), block (Q), **parry** the middle dummy (tap Q just before its orange flash ends), dodge, Earthbreaker (R) |
| `Tests/Test_EarlyEncounter` | Break shields with heavies; shields lower (cyan flash) after 2-3 blocked hits; slingers kite you |
| `Tests/Test_Kuntur` | Tab to mark, RMB bolas then light = finisher, R Vantage Leap onto blue pads, jump + LMB Falling Star, F Sky-Cut, V slow time |
| `Tests/Test_Amaru` | T snares ahead *before* the hidden scouts spring (listen for the snap), RMB Coil Grab into snares, Shift decoy vs the arquebusier, Q riposte |
| `Tests/Test_AtocBoss` | Three phases, decoys (the fake ones don't bob), cover, red roar = unblockable string, then **Spare** |
| `Tests/Test_AllEnemies` | One of each enemy on a ring; Chaska and Atoc fight as AI allies. Try the **call-ins**: Z Mark, X Trap, C Cover (gamepad: hold LB + Y/X/B); the strip above the abilities shows cooldowns and why one is unavailable |
| `Tests/Test_M3_5_ForcedSwap` | Rumiñahui → flash → Chaska → flash → Atoc → convergence → final wave with allies. Die as Chaska/Atoc: only that section restarts |
| `Tests/Test_M5_3_FreeSwap` | 1/2/3 to swap. Gate (Rumiñahui), ravine (Chaska), deadfalls (Atoc), regroup, joint lift, chamber |

**F1 debug panel:** unlock all, refill, god mode, spawn any enemy, upgrade tiers, tell-flash toggle, live Focus-gain feed.
**H:** hide the hint panel (top-right: controls, current objective, last dialogue line).

## 4. Unit tests
*Window ▸ General ▸ Test Runner ▸ EditMode ▸ Run All.*

## 5. When something breaks
Send the Console errors (first error first). Every error gets logged in `LESSONS_LEARNED.md` with its fix and a prevention rule.

## 6. Unity-only work — where each piece plugs in
These need the editor, so the code leaves a single, clear hook for each.

| Task | Doc | Hook in code |
|---|---|---|
| **Real models + animation** | AI_BUILD_PROMPT | Set `PlaceholderVisual.modelPrefab` on the prefabs from *Rumiñahui ▸ 3. Save Placeholder Prefabs*. Poses currently fake it with `SetPoseScale`; replace those calls with Animator triggers. |
| **Animator culling** | 11 B Step 6 | On each model's Animator set Culling Mode = *Cull Update Transforms* (no code needed). |
| **Timeline cutscenes** | 11 A Step 4 | Every placeholder shot goes through `CameraDirector.CreateShot` / `PlayShot` (spare, M2.3 Willka, M3.5 convergence, M5.6). Swap each for a `PlayableDirector` with a CinemachineTrack. |
| **Vision post FX tuning** | 11 A Step 5 | `VisionSequence` (DoF values at the top). Replace `ShotDrift` with a Cinemachine Noise profile on the vision camera. |
| **NavMesh** | (for real levels) | Enemy steering is centralised in `EnemyBrain.MoveTowards/MoveAwayFrom/Strafe`; escorts in `EscortTarget.Update`; allies in `AllyBrain.MoveTo`. Swap those three for NavMeshAgent paths. |
| **Real environments** | 07 / 10 | Each mission's layout is one method in `Levels/LevelBuilder*.cs`. Replace it with authored scene content and keep the director wiring. |
| **Occlusion culling + static flags** | 11 B Steps 4-5 | Bake per scene once real geometry exists. Code-built placeholder geometry is already static-batched at runtime. |
| **Audio** | 11 B Step 7 | Drop clips in `Assets/Audio/{Music,Ambience,VO,SFX}`; `AudioImportRules` sets compression. Replace `PlaceholderCue` tones with clips in `AudioPool`. VO hooks into `DialogueRunner.BeatPlayed`. |
| **Play-mode tests** | — | Good first ones: the parry window, shield guard-break, M3.5 section restart, and M5.3 swap lock. All of them can drive `CombatKit.Cmd*` directly. |
| **Balance** | — | `grep -rn "PLACEHOLDER-BALANCE" Assets/Scripts`, with enemy stat tiers in `Enemies/EnemyStats.cs`. |
| **Missing dialogue** | 04 | Lines marked `[placeholder]` (M1.1-M4.3, village NPCs). Write them in 04, run the importer, and switch the director to `PlayRange`. |
