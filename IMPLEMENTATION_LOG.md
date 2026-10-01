# Implementation Log

Maintained by the coding AI. Before changing a system, find its entry here, open the governing doc, and change the doc and code together.
Mistakes and their prevention rules live in `LESSONS_LEARNED.md`. Setup and test steps are in `UNITY_SETUP.md`.

- Every script's header says which doc/section it implements (`// Implements: …`).
- Every invented number is tagged `// PLACEHOLDER-BALANCE:` in code. Find them all with `grep -rn "PLACEHOLDER-BALANCE" Assets/Scripts`.
- Status: **compiles offline against Unity API stubs (0 errors, 0 warnings); 19/19 logic tests pass** (`Tools/CompileCheck/check.sh`). The riskiest package APIs were checked against real source (LESSONS L-010/L-012/L-018). Not yet opened in Unity.

---

## Conventions (read once)

**Placeholder visuals.** Every character, enemy, projectile, trap and decoy has exactly one `PlaceholderVisual` (Assets/Scripts/Placeholder). It builds a flat-colour primitive plus optional "marker" parts (shield, club, horse…) and adds an `LODGroup` by convention. **To swap in real art:** assign `PlaceholderVisual.modelPrefab` on the prefab in `Assets/Prefabs/Placeholder`. No logic changes needed. Colours are defined in `CharacterFactory` / `EnemyFactory.ColorFor`:

| Who | Colour | Shape / markers |
|---|---|---|
| Rumiñahui | stone brown | capsule + warclub |
| Chaska | sky blue | capsule + twin blades |
| Atoc | serpent green | capsule + coil ring |
| Training dummy | tan | cylinder |
| Skirmisher | yellow-orange | small capsule + sling |
| Shield-Bearer / armored | dark red / darker red | capsule + wood / steel shield |
| Highland Scout | olive | small capsule (hidden until ambush) |
| Atoc's Lieutenant | purple | big capsule + shield |
| Spanish Infantry | steel | capsule + helmet + shield |
| Spanish Cavalry | steel rider | capsule on a brown "horse" box |
| Arquebusier | dark grey | capsule + long gun |
| Spanish Officer | crimson | capsule + gold plume |

**Tell colours:** orange = normal wind-up · red = unblockable · cyan = shield lowering · yellow = arquebus flare · gold = officer rally · lime = Atoc's crouch-and-glance.

**Code-built content.** Scenes contain only a light and a `SceneSetup(contentId)`. `LevelBuilder` builds each level at Start, so regenerating scenes never loses content. When real environments arrive, replace that mission's builder with authored content.

**Single namespace** `Ruminahui` (editor: `Ruminahui.EditorTools`, tests: `Ruminahui.Tests`). Assemblies: `Ruminahui.Runtime`, `Ruminahui.Editor`, `Ruminahui.Tests.EditMode`.

**No physics layers or triggers.** Targeting, melee, AoE and zones use distance queries over `CombatTarget.All` / polled bounds. Physics is used for ground (CharacterController), projectiles vs walls, and cavalry lane checks. No Rigidbodies (doc 11, Step 8).

**Controls** (Input System, defined in code in `GameInput.cs`):

| Action | Keyboard / mouse | Gamepad |
|---|---|---|
| Move / look | WASD / mouse | L-stick / R-stick |
| Jump | Space | A |
| Light / heavy | LMB / RMB (Puma: hold to charge) | X / Y |
| Block (Puma) · counter (Amaru) | Q | LT |
| Dodge | Left Shift | B |
| Interact / grapple / spare | E | RT |
| Lock-on + Condor's Eye mark | Tab / MMB | R3 |
| Specials 1-3 · ultimate | R F G · V | d-pad ↑ → ← · L3 |
| Snare (Amaru) | T | d-pad ↓ |
| Swap (M5.3) | 1 2 3 · [ ] | LB / RB |
| Pause · debug · hint panel | Esc · F1 · H | Start · Select |

---

## Entries

### 2026-09-30 · Step 0 — Project foundations
- **Docs:** 11-camera-performance.md Part A Steps 1-3, 6-7; Part B Steps 1-5, 7-9. AI_BUILD_PROMPT step 0.
- **Files:** `Packages/manifest.json`, `*.asmdef`, `Systems/Core/*`, `Systems/Pooling/*`, `Systems/Streaming/SceneStreamer.cs`, `Systems/Camera/*`, `Systems/Input/GameInput.cs`, `Editor/RuminahuiSetupMenu.cs`, `.gitignore`.
- **What:** URP (Standard + Performance quality tiers, created by menu step 1). Persistent `[Systems]` root created before the first scene loads, so any scene is directly playable. `PoolManager` (string-keyed, used for enemies, projectiles, snares, decoys, VFX, damage numbers). Pooled `AudioSource`s. Addressables additive per-mission streaming with a Build Settings fallback. Per-character `CinemachineCamera` pairs (gameplay + lock-on) swapped by priority.
- **Assumptions:** Cinemachine 3 (Unity 6) names instead of the doc's CM2 names (mapping in LESSONS L-010). Cutscene shots are code-driven placeholder cameras with slow drift in place of Timeline assets and Noise profiles, until the cinematics phase. Next-mission preloading is not done (Unity blocks other loads while a scene waits for activation); only the current mission stays resident. Occlusion culling is deferred until real geometry exists (Step 4.2).

### 2026-09-30 · Step 1a — Shared resources (HP / Stamina / Focus)
- **Doc:** 03 Section 0.
- **Files:** `Combat/Shared/{Health,Stamina,FocusPool,FocusRules,StatusEffects,CombatTarget,CombatQuery,AttackData,AbilitySlot,CharacterMotor,CombatTypes,CombatKit}.cs`, `Systems/Checkpoints/{Huaca,CheckpointService}.cs`.
- **What:** HP has no regen and is restored only at `Huaca`. Stamina regenerates after a short delay. Focus is **one pool on `PartyManager`**, shared by all kits. Focus sources: clean hit 2.5, perfect parry 15, perfect dodge 9, counters 12, tell-read 7 (only post-M3.5, flag `Focus.TellReading`). All hits resolve through `IHitInterceptor`s (i-frames → counters/parry → block/shield → rebirth).
- **Assumptions:** "tell reading" = landing a hit, dodge, parry or counter while the enemy is in its Telegraph state. Stamina may dip to 0 on the last action; an empty bar refuses new actions.

### 2026-09-30 · Step 1b — Puma kit + training dummy
- **Doc:** 03 Section 1. AI_BUILD_PROMPT step 1.
- **Files:** `Combat/Puma/PumaKit.cs`, `Enemies/TrainingDummy.cs`, `Levels/LevelBuilder.Combat.cs` (`Test_PumaDummy`, M0.2, M1.1, M4.2).
- **What:** Stone Strikes x3 (3rd hit staggers). Warclub Crush (charge up to 1 s, guard-break power +1 at full charge). Stone Stance (15% chip, stamina drain) and Stone Parry (0.18 s window, staggers the attacker). Root-Step (2.6 m). Grounding Throw (Interact on an open enemy; splash damage to whoever it lands on). Earthbreaker, Unyielding (hyper-armour: hits land but don't interrupt), Stone Face (open targets only), What Held the Line (×0.1 damage taken, ×2.5 damage dealt, 8 s). Tiers: T1 stamina ×0.8 + 4-hit chain · T2 guard-break +1, throw ×1.5, Earthbreaker radius +1.5 · T3 Unyielding +2 s, parry window +0.1 s, Stone Face cost 30.
- **Assumptions:** "already staggered" for Stone Face / Grapple also counts knockdown, bolas wrap and the spare state. Cavalry charges and power-2 guard-breaks crush a held block.

### 2026-09-30 · Step 2 — Enemy roster AI
- **Doc:** 06 Enemy Roster (all 9 entries) + 03 Section 5.
- **Files:** `Enemies/*.cs` (`EnemyBrain` base, one class per entry, `ShieldGuard`, `FormationGroup`, `EnemyFactory`, `EnemyStats`, `BossBarTarget`).
- **What:** Skirmisher (kites at 7-13 m, sling after a 0.8 s tell). Shield-Bearer (frontal block; lowers shield after 2-3 blocked hits; guard-break threshold 1, armored 2). Highland Scout (hidden; audio-only snap cue; a nearby player Snare neutralises the ambush). Lieutenant (shield + sling + roar → 3-hit unblockable string). Spanish Infantry (threshold 3, 0.35 s wind-ups, line formation). Cavalry (long tell → straight charge that crushes blocks; vulnerable recovery; won't charge without a clear lane). Arquebusier (flare tell, unblockable shot, 5 s reload, shoots decoys). Officer (interruptible rally buffs nearby troops ×1.3 for 10 s).
- **Assumptions:** Low/Med/High/Very High → numbers in `EnemyStats.cs` (one place to tune). Enemies avoid each other by separation steering; no NavMesh yet (flat placeholder arenas).

### 2026-09-30 · Step 3 — Chaska controller + Kuntur kit
- **Doc:** 03 Section 2; 11 Part A Step 3.3.
- **Files:** `Combat/Kuntur/{KunturKit,VantagePoint}.cs`, `Characters/*`.
- **What:** Flurry x4. Bolas Snap (projectile wraps 2.5 s; next melee hit on a wrapped target ×1.6). Skywalk (4.5 m, chain 2 / T1 3). Falling Star (Light while airborne → air string → dive AoE finisher). Condor's Eye on the lock-on button (mark 12 s, ×1.35 damage, no cooldown). Vantage Leap (arc to the nearest blue pad ≤14 m, else straight up). Sky-Cut (hits every marked target; refuses with no marks). Star-Fall (execute ≤25% HP, T3 ≤35%). The Gap in the Line (world ×0.25 for 6 s, Chaska exempt, via per-object `TimeDilation`, not `Time.timeScale`).
- **Assumptions:** "launcher" is covered by jumping or Vantage Leap (no enemy-launch move is specified). Bosses are immune to Star-Fall. Vantage Leap costs no Focus when no enemy is within 25 m (traversal use; needed for M5.3).

### 2026-09-30 · Step 4a — Atoc controller + Amaru kit
- **Doc:** 03 Section 3.
- **Files:** `Combat/Amaru/AmaruKit.cs`, `Combat/Shared/{Snare,Decoy,Projectile}.cs`.
- **What:** Fang Strikes (×1.5 vs any debuffed target). Coil Grab (pulls a target onto your nearest Snare and springs it). Shed Skin (dash + decoy that absorbs one hit and drains the attacker's stamina). Venom Riposte (Block button, 0.25 s timed read → sidestep + poison; a whiff costs 0.45 s). Snare (capacity 2, T1 3; oldest removed). Numbing Draught (radius 6, ×0.5 speed). False Trail (untargetable 4 s; attacking breaks it). Last Fang (0.45 s stance: heavy/charge/unblockable → big counter, anything else → ×2 damage + stagger, whiff → exposed ×1.5). A Hundred and One (armed 10 s; critical damage → vanish 1 s → 90% HP + ×1.6 damage for 8 s, once per activation).

### 2026-09-30 · Step 4b — Atoc boss fight (M3.4)
- **Doc:** 08 (all sections); 11 Part A Step 7.
- **Files:** `Bosses/AtocBoss.cs`, `Levels/LevelBuilder.SetPieces.cs` (`BuildAtocArena`), `UI/BossBarUI.cs`.
- **What:** Phases by HP 65% / 30%. P1: strikes + crouch-and-glance → Snare + reposition. P2: two decoys (no idle bob, the "tell within a tell"), vanishes behind wreckage cover, flank strike; getting within 3 m of his cover forces him out and staggers him. P3: ×1.3 speed, ×0.7 wind-ups, roar + stance change → 3-hit unblockable string then a 1.4 s punish window. HP floors at 1. Staggered at ≤10% → spare state: invulnerable, no damage numbers, a distinct pale **Spare** prompt, a dedicated side-on shot, combat UI hidden, `Puma.StoneFace` unlocked. No kill option exists (by design, 08 Section 3).

### 2026-09-30 · Step 5a — M3.5 forced swap
- **Doc:** 05 (flow A→P + technical notes).
- **Files:** `Systems/CharacterSwap/ForcedSwapDirector.cs`, `Systems/Party/PartyManager.cs`, `UI/ScreenFader.cs`, `LevelBuilder.SetPieces.cs` (`BuildM35`).
- **What:** One scene, one flow. Control shifts happen under a warm flash, with a hard camera cut + controller swap, no load and no camera creation. D-F and G-I encounters use enemy damage ×0.85; stage L uses ×1.15. Death in D-F or G-I restarts only that section. K = dedicated two-shot convergence with combat UI hidden. N unlocks all three ultimates live and fires What Held the Line. P → M3.6.
- **Assumptions:** Willka's VO is a keyed placeholder (`VO_Willka_M1_2_Fireside_01`) because the M1.2 script isn't in 04 yet. Characters not in play (downed / caged / waiting) are untargetable.

### 2026-09-30 · Step 5b — M5.3 free swap
- **Doc:** 06 M5.3 (flow A→I + technical notes).
- **Files:** `Systems/FreeSwap/*`, `UI/SwapSelectorUI.cs`, `LevelBuilder.SetPieces.cs` (`BuildM53`).
- **What:** 1/2/3 or LB/RB. Fallen Gate (Rumiñahui holds E 1.5 s). Wide Break (Chaska Vantage Leaps, then drops a line → bridge). Narrow Dark (4 deadfalls visible only to Atoc within 6 m; anyone else is bounced back without damage; Atoc holds E to disarm). Regroup (whole party in zone). Joint Lift (three stations; an engaged character holds position and can't be swapped into; swap auto-locks while anyone is mid-hold; control auto-passes to the next needed character). Hiding chamber (no loot). Dialogue beat → M5.4. Everyone is invulnerable, and falls are recoverable stumbles. 3 optional relics.
- **Assumptions:** "all three simultaneously" is done as sequential engage-and-hold (you can only control one at a time), with the final lift fully locked. Deadfalls affect only the controlled character; companions ignore them.

### 2026-09-30 · Step 6 — Mission / scene manager
- **Doc:** 02 (all 26 missions); 11 Part B Step 3.
- **Files:** `Systems/MissionManager/*`, `Systems/Progression/*`, `Levels/*`, `Editor/RuminahuiSetupMenu.cs`.
- **What:** `MissionDatabase` (26 missions + 8 test scenes + `_Boot`). Mission completion → next mission. `Progression.ApplyForMission` rebuilds unlocks from `UnlockSchedule`. Scenes named by ID (`Assets/Scenes/M3.4.unity`) are generated by menu, added to Build Settings, and marked Addressable (address = ID). Built content: M0.2, M1.1, M1.3, M2.1, M2.3, M3.1-M3.5, M4.2, M5.1-M5.4. The rest are placeholders (ground + "walk to the light").
- **Assumptions:** default upgrade tiers by Act (Act I-II T1, III-IV T2, V T3) until an upgrade menu exists. Test scenes unlock everything at tier 0 (change tiers from F1).

### 2026-09-30 · AI allies (supports 03 Section 4, 05 stage L, 06 M5.3)
- **Files:** `Characters/AllyBrain.cs`.
- **What:** Combat mode (picks the leader's lock target or the nearest enemy; kit flavour: Chaska marks + Bolas, Atoc Snares + riposte reads; specials on a 9 s AI timer), Follow, Hold, Idle.
- **Assumptions:** AI allies **don't spend the shared Focus** (it's the player's momentum) and can't die (HP floors at 1). No call-in command menu yet (03 Section 4 "simple context commands", deferred).

### 2026-09-30 · UI (approved plan items 1-11)
- **Files:** `UI/*`.
- **What:** Built in code with uGUI and legacy `Text` (TextMeshPro needs an essentials import). Resource HUD, ability strip, enemy markers (HP / ◆ mark / [ ] lock), tell flash (toggle in F1), boss bar with phase ticks, context prompt (Spare styled differently), swap selector, fader/vision flash, pooled damage numbers, F1 debug panel, Esc pause + mission list. Added testing aid: the top-right hint panel (controls + objective + last dialogue line, H to hide).
- **Deferred:** upgrade menu, subtitle/dialogue system, main menu/settings.

### 2026-09-30 · Tests
- **Files:** `Assets/Tests/EditMode/*`: resources, unlock schedule and mission order, AbilitySlot free-use, Atoc phase thresholds.

### 2026-09-30 · Offline compile harness (Unity not installed)
- **Why:** Unity couldn't be installed; this verifies the code without it.
- **Files:** `Tools/CompileCheck/*` (outside `Assets/`, so Unity ignores it).
- **What:** .NET Standard 2.1 build against hand-written stubs for UnityEngine, uGUI, Input System, Cinemachine 3, Addressables, URP, UnityEditor and NUnit, plus a reflection test runner. Result: one real error found and fixed (LESSONS L-017). Cinemachine/URP/Addressables members checked against their public sources: one URP setup gap (L-012) and one Cinemachine `LookAt` gotcha (L-010) fixed.
- **Limit:** stubs reflect our understanding of the API, so a pass isn't proof for Unity APIs that haven't been verified.
