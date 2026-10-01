# Lessons Learned — Rumiñahui Unity build

**Read this before every build step.** Each entry is a mistake (made, or caught before it shipped) plus the rule that prevents it.
Status: **caught** = found in review · **hit** = found by a compiler (Unity or the offline `Tools/CompileCheck`) · **verified** = checked against real package source · **verify** = still to check in Unity.

**Offline check:** run `Tools/CompileCheck/check.sh` after every change, even without Unity installed.

When Unity reports an error: find it here first. If it's new, add an entry (ID, symptom, cause, fix, rule).

---

## Rules checklist (the short version)

1. One MonoBehaviour / ScriptableObject per file, and the file name must match the class name.
2. Build GameObjects **inactive**, configure them, *then* activate. Never configure a component after `AddComponent` on an active object if its `Awake` reads that config.
3. Never rely on sibling `Awake` order. In `Awake`, only cache references; do cross-component work in `Start`/`OnEnable`, or lazily.
4. A coroutine can finish *inside* `StartCoroutine`. Don't treat "handle != null" as "still running". Use a flag + serial number.
5. Teleport a `CharacterController` only through `CharacterMotor.Teleport` (disable → move → enable), or position it while inactive.
6. Never `?.` / `??` on UnityEngine.Object (component/GameObject) references. The exception is singleton `Instance` fields, which are nulled in `OnDestroy`.
7. Never `using System;` next to `using UnityEngine;` (`Object` and `Random` become ambiguous). Write `System.Action` / `System.Func` in full.
8. Any list you iterate while hitting/killing things (`CombatTarget.All`) must be copied first. `CombatQuery` always returns a new list.
9. Cinemachine 3 names (Unity 6), not the doc's CM2 names. See L-010.
10. Anything created in `Start` of a streamed scene must end up in that scene: `SceneSetup` sets it active first.
11. macOS shell: BSD `sed` has no `\b`, and `sed -i` needs `''`. Use `perl -pi -e` for regex edits.
12. `Reset()` only runs in the editor. Runtime builders must set every field explicitly.
13. Event-based stage gates must be armed or polled: an event that fires before anyone subscribes is lost. Prefer polling a `fired` flag.
14. Check each mission's resource economy: a Focus-gated verb in a no-combat mission is a softlock.
15. Run `Tools/CompileCheck/check.sh` before every commit. New Unity APIs go into the stubs with their REAL signatures.
17. Generated data (dialogue JSON) is never hand-edited. Change the source doc, re-run the importer; `check.sh` fails on drift.
18. Don't put dots in Resources asset names (`M0_1.json`, not `M0.1.json`).
19. After writing a parser, print a human-readable dump of its output and read it against the source before trusting it.
16. C# forbids reusing a local name (`t`, `i`…) in a nested scope of the same method. Inside coroutines give loop variables descriptive names.

---

## Entries

### L-001 · caught · Multiple MonoBehaviours in one file
- **Symptom (would be):** "The associated script can not be loaded" / missing script on prefabs and scenes; components can't be added in the Inspector.
- **Cause:** `Obstacles.cs` held 7 MonoBehaviours; `CombatPools.cs`, `CameraDirector.cs` and `SpanishInfantry.cs` each held a second one (`TimedDespawn`, `ShotDrift`, `FormationGroup`).
- **Fix:** split into one file per class.
- **Rule:** #1. Plain C# classes and static classes can share a file; Unity components can't.

### L-002 · caught · Awake ordering between sibling components
- **Symptom (would be):** `NullReferenceException` in `PumaKit.CmdBlockReleased` on spawn.
- **Cause:** `PlayerCharacter.Awake` called `ApplyControlState`, which calls into the kit, and the kit's `Awake` may not have run yet.
- **Fix:** `PlayerCharacter.Awake` only caches references and sets flags. Control changes go through `PartyManager.SetControlled` after activation.
- **Rule:** #3.

### L-003 · caught · Nested coroutine yield + interrupt
- **Note:** stopping the outer coroutine (`StopCoroutine(behaviour)`) also stops IEnumerators it is `yield return`-ing. Nested helpers like `Telegraph()`, `Charge()` and `Wait()` rely on this. Don't start them with a separate `StartCoroutine` unless they must survive an interrupt.

### L-004 · caught · Coroutine that finishes synchronously leaves the kit "busy" forever
- **Symptom (would be):** after a dodge with no stamina, the character never attacks again.
- **Cause:** `currentAction = StartCoroutine(RunAction(...))`. If the routine `yield break`s immediately, the handle is assigned *after* it has finished, so `IsBusy` stays true.
- **Fix:** `busy` flag + `actionSerial` in `CombatKit.StartAction/RunAction/CancelAction`.
- **Rule:** #4.

### L-005 · caught · Operator precedence in damage math
- **Cause:** `EnemyStats.DmgHigh + 8f * DamageScale` (cavalry charge) only scaled the 8.
- **Fix:** `(EnemyStats.DmgHigh + 8f) * DamageScale`.
- **Rule:** always parenthesise `base * scale` expressions.

### L-006 · caught · Configuring a component after its Awake already ran
- **Symptom (would be):** M5.3 deadfall markers built as capsules instead of flat red tiles.
- **Cause:** `AddComponent<PlaceholderVisual>()` on an *active* GameObject runs `Awake` (which builds the primitive) before `shape`/`scale` are set.
- **Fix:** `SetActive(false)` → add + configure → `SetActive(true)`. All factories (`CharacterFactory`, `EnemyFactory`, `CombatPools`) follow this.
- **Rule:** #2.

### L-007 · caught · Companions followed the leader off a ledge
- **Cause:** follow AI steered straight at the leader across the M5.3 ravine, and the catch-up teleport skipped the puzzle.
- **Fix:** ledge raycast in `AllyBrain.MoveTo`; catch-up distance raised to 40 m; a soft-fail volume under the whole M5.3 map.

### L-008 · caught · Shell typo created a stray file
- **Cause:** `cat > LevelBuilder.Set pieces.cs` → the space split the filename, which created an empty `LevelBuilder.Set` file.
- **Rule:** never put spaces in generated file names; check `ls` after writing files.

### L-009 · caught · Objects created in Start of an additively-loaded scene land in the wrong scene
- **Symptom (would be):** switching missions shows an empty level, because the characters, cameras and props were created in the *old* scene and destroyed with it.
- **Cause:** `SceneStreamer` calls `SetActiveScene` after the load finishes, but the new scene's `Start` can run first.
- **Fix:** `SceneSetup.Start` makes its own scene active before building.
- **Rule:** #10.

### L-010 · verified (2026-09-30, against com.unity.cinemachine main source) · Cinemachine 3 API names (doc 11 uses Cinemachine 2 names)
| Doc (CM2) | Code (CM3, Unity 6) |
|---|---|
| `CinemachineVirtualCamera` | `CinemachineCamera` |
| 3rd Person Follow body | `CinemachineThirdPersonFollow` |
| Composer aim | `CinemachineRotationComposer` (`Composition.ScreenPosition`) |
| `CinemachineCollider` | `CinemachineDeoccluder` (`MinimumDistanceFromTarget`) |
| `m_Priority` (int) | `Priority` (`PrioritySettings`, assigned from int via implicit conversion) |
| namespace `Cinemachine` | namespace `Unity.Cinemachine`, asmdef `Unity.Cinemachine` |
- **Verified in source:** `PrioritySettings` struct with implicit `int` conversions both ways · `CinemachineCamera.Lens` (field, `LensSettings.FieldOfView` float) · abstract `Follow`/`LookAt` · `ThirdPersonFollow.{Damping, ShoulderOffset, VerticalArmLength, CameraSide, CameraDistance}` · `RotationComposer.{Composition.ScreenPosition, Damping (Vector2), TargetOffset}` · `Deoccluder.MinimumDistanceFromTarget` (class is `#if CINEMACHINE_PHYSICS`, so it needs the physics module, which the manifest has) · `CinemachineBlendDefinition(Styles, float)` struct with `Styles.Cut/EaseInOut` · `CinemachineBrain.DefaultBlend` field.
- **Gotcha found:** the `LookAt` setter always sets `Target.CustomLookAtTarget = true`, *even for null*. Fixed: `CharacterCameraSet` only assigns non-null targets.

### L-011 · verify · Package versions in `Packages/manifest.json`
- URP (`17.0.x`) is locked to the editor version. If Unity complains, let the Package Manager pick the version that ships with your Unity 6 patch.
- Input System: when Unity asks to enable the new input backend and restart, say **Yes** (or set Player ▸ Active Input Handling = *Input System Package* or *Both*).

### L-012 · verified + fixed · URP asset created from code
- **Cause (confirmed in URP source):** URP's own menu builds the renderer through an internal `CreateRendererData`, which sets `rendererData.postProcessData = PostProcessData.GetDefaultPostProcessData()` (internal, editor-only, loads `Packages/com.unity.render-pipelines.universal/Runtime/Data/PostProcessData.asset`). A bare `CreateInstance<UniversalRendererData>()` skips that.
- **Fix:** `RuminahuiSetupMenu` loads that asset path itself and assigns the public field `postProcessData`. `Create(rendererData)`, `renderScale`, `supportsHDR` and `shadowDistance` are all public with setters (verified).
- **Still verify in Unity:** no pink materials after step 1. Fallback: *Assets ▸ Create ▸ Rendering ▸ URP Asset (with Universal Renderer)*.

### L-013 · caught · BSD sed on macOS
- `sed -E 's/\bX/…/'` silently matched nothing (BSD sed has no `\b`).
- **Rule:** #11. Use `perl -pi -e`, then grep to confirm the edit actually happened.

### L-014 · caught · `Reset()` isn't called at runtime
- `FallenGateObstacle.Reset()` and similar only run when a component is added *in the editor*. `LevelBuilder` sets `requiredCharacter`, `holdDuration` and `actionText` explicitly.
- **Rule:** #12.

### L-015 · caught · Stage-gate event fired before the director subscribed
- **Symptom (would be):** M5.3 stuck at "[E] Regroup". Atoc walks past the (harmless-to-him) deadfalls, the companions catch up, and the regroup zone fires while the director is still waiting on stage D.
- **Fix:** `ZoneTrigger.armed` (false at build, armed when the stage begins); the director polls `fired` instead of subscribing late.
- **Rule:** #13.

### L-016 · caught · Focus-gated traversal in a mission with no Focus income
- **Symptom (would be):** M5.3 softlock at the Wide Break (Vantage Leap costs 15 Focus; the mission has no combat).
- **Fix:** `CombatKit.IsTraversalAbility`: Vantage Leap is free when no enemy is within 25 m.
- **Rule:** #14.

### L-017 · hit (offline compiler) · Duplicate local name in one method
- **Error:** `CS0136: A local named 't' cannot be declared in this scope` in `LlanganatesDirector.Run` (`foreach (var t …)` and later `float t`).
- **Fix:** renamed the loop variable to `trap`.
- **Rule:** #16. This was the only compile error in about 9,800 lines; the first-pass review missed it, the offline compiler caught it.

### L-018 · verified · Addressables editor API
- `AddressableAssetSettings.SetDirty(ModificationEvent, object, bool postEvent, bool settingsModified = false)`, `DefaultGroup` and `ModificationEvent.EntryMoved` all exist as used (checked against package source).

### L-019 · verify · Dots in Resources asset names
- **Risk:** `Resources.Load("Dialogue/M0.1")` for a file named `M0.1.json`. Unity drops the final extension to get the asset name, and a dotted name is a common source of failed loads.
- **Precaution:** files are named `M0_1.json`; `DialogueDatabase.ResourcePath` maps `"M0.1"` → `"Dialogue/M0_1"`.
- **Verify in Unity:** the `DialogueTests` EditMode tests load all five scripts through `Resources.Load`.
- **Rule:** #18.

### L-020 · caught · Dialogue importer mis-parsed two note formats
- `[QTE-style choice, … only the manner of it: "STAND" or "RUN."]`: the label regex `[^:]*:` treated the whole prose up to the colon as a label and dropped it.
- `"RUN."`: the period inside the quotes defeated the prompt regex; `STAND` was also counted twice.
- `[Gameplay ends here — …]` (no colon) came out as "Gameplayends here".
- **Fix:** strip the label only when it's directly followed by `:`, otherwise keep the bracket text verbatim; allow trailing punctuation inside quotes; de-duplicate prompts.
- **Found by:** printing a one-line-per-beat dump of every script and comparing it with the doc. **Rule:** #19.

### L-021 · caught · Used /tmp instead of the session scratchpad
- A test-sensitivity check moved a file to `/tmp`. It was restored, but temporary files belong in the session scratchpad.
- **Rule:** temporary files go in the scratchpad directory, never `/tmp`.
