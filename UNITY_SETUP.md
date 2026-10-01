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
Open any scene and press **Play**. `_Boot` opens the mission/test list (Esc also opens it at any time).

| Scene | What to try |
|---|---|
| `M0.1`, `M0.2`, `M5.5`, `M5.6`, `M5.7` | Their dialogue from doc 04 plays line by line in the hint panel (top-right); stage directions go to the Console |
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
