# RUMIÑAHUI — Camera, Cinematics & Performance Optimization
### Companion document to the Combat Design Doc and the M3.5/M5.3 Flow Diagrams

## 0. PERSPECTIVE DECISION

**This is a third-person action game**, camera positioned over-the-shoulder/behind the character, in the tradition of the God of War comparison the whole project is built from. This was implicit in the Combat Design Doc but never stated outright — it's now been added there directly (see the note at the top of `03-combat-design.md`). Every step below assumes third-person.

Reasoning, briefly: aerial finishers, reading enemy tells at range, terrain-based ambushes (Amaru kit), and especially the M3.5 sequence (watching Chaska and Atoc as separate characters) all depend on the camera seeing the character's whole body and surroundings — none of that works in first-person.

---

## PART A — CAMERA & CINEMATICS SETUP (step by step, Unity + Cinemachine)

Recommended tooling: **Cinemachine** (camera logic) + **Timeline** (cutscene sequencing). Both are official Unity packages and integrate directly with each other — this is the standard, well-supported approach rather than hand-rolling camera code.

### Step 1 — Install the packages
1. Window → Package Manager → install **Cinemachine** and **Timeline** (Timeline ships with modern Unity by default; Cinemachine needs adding).
2. Add a **CinemachineBrain** component to your Main Camera. This is the component that lets Cinemachine virtual cameras actually control the real camera — nothing works without it.

### Step 2 — Core gameplay camera (the one running 90% of the time)
1. Create a **CinemachineVirtualCamera** named `VCam_Gameplay_ThirdPerson`.
2. Set **Follow** and **Look At** to the player character's root/head transform.
3. Body: use **3rd Person Follow** (Cinemachine's purpose-built third-person body) rather than a generic Transposer — it handles shoulder-offset and collision-aware framing more naturally for over-the-shoulder combat.
4. Aim: **Composer** with a modest screen-position offset (slightly off-center toward the shoulder, not dead-center) — this is the classic GoW-style framing and also leaves room on screen for HUD elements.
5. Add a **CinemachineCollider** extension so the camera doesn't clip through walls/enemies during combat — set a reasonable minimum distance to avoid the camera snapping uncomfortably close in tight spaces.

### Step 3 — Combat/lock-on camera
1. Create a second VCam, `VCam_Combat_LockOn`, same Body settings as Step 2 but with **Look At** driven dynamically by script — write a simple target-selection system (nearest enemy in a forward cone, or nearest marked enemy per the Kuntur kit's Condor's Eye mechanic) that swaps the Look At target when the player locks on.
2. Raise this VCam's **Priority** above the base gameplay VCam only while locked on; Cinemachine blends between them automatically based on priority — you don't need to write blend logic by hand.
3. Tie lock-on camera activation to the same input that triggers Condor's Eye marking (Combat Design Doc, Kuntur kit) so targeting feels like one unified system rather than two separate ones layered together.

### Step 4 — Cinematic/cutscene cameras
1. For story beats (Willka's fireside scene, the M3.5 convergence moment, the execution scene), build each as a **Timeline** asset with one or more **CinemachineTrack**s controlling dedicated VCams for that scene only (e.g., `VCam_Cutscene_Fireside_01`, `_02`, for a two-shot conversation).
2. Keep cutscene VCams simple **Composer**-based static or slow-drift shots rather than reusing the gameplay follow-cam settings — cinematic framing should feel deliberately different from moment-to-moment gameplay camera, which also gives you an easy technical hook for the vision-sequence camera language (Step 5).
3. Use Timeline's **Activation Track** to enable/disable NPCs and props precisely for the cutscene, and an **Audio Track** in the same Timeline for VO/music sync — keeps everything for one scene in a single asset, easy to find and edit later.

### Step 5 — Vision-sequence camera language (ties to the Myth Ambiguity direction doc)
1. Add a **Noise** profile (Cinemachine's built-in procedural handheld-shake module) to vision-sequence VCams only — a low-amplitude, low-frequency setting for subtle handheld drift, not an action-scene shake.
2. Use **Depth of Field** in your post-processing volume, blended in specifically for vision sequences (via a Timeline **Control Track** toggling the volume's weight), to get the "focus-breathing" soft-edge look specified in the Myth Ambiguity doc — this should be the *only* place in the game DoF is used this heavily, so it stays a recognizable signal.
3. For the "abrupt return to reality" cut specified in that doc: do this as a hard Timeline cut (no cross-fade) back to the gameplay VCam, paired with an equally abrupt audio cut — both systems (camera and audio) should snap together, not separately.

### Step 6 — The M3.5 multi-character sequence specifically
1. Because control passes between three characters without a load screen (per the M3.5 Flow Diagram's technical notes), pre-place a VCam per character (`VCam_Gameplay_Ruminahui`, `_Chaska`, `_Atoc`) all live in the scene simultaneously, and swap **priority** at the moment of each control shift rather than instantiating/destroying cameras — this avoids any hitch from object creation during a moment that specifically must not have a visible load.
2. Use the same Step 4 Timeline approach for the brief non-combat "convergence" beat (stage K in the flow diagram) — a short dedicated cutscene VCam sequence, then hand back to gameplay VCams for the final wave.

### Step 7 — Boss fight camera (Atoc, and template for later bosses)
1. A slightly wider **Composer** framing than the standard gameplay VCam, giving the player more peripheral visibility of arena hazards (per the Boss Fight doc's arena-design notes about wreckage/sightlines).
2. A dedicated, very brief cutscene VCam for the "spare" finishing-blow moment (Boss Fight doc, Section 2) — since that moment is meant to read as visually distinct from a normal finisher, give it its own short Timeline rather than relying on the gameplay camera to sell the moment.

---

## PART B — PERFORMANCE OPTIMIZATION (step by step)

Given you're building systems-first with placeholder primitive assets, the good news is your current build is about as cheap as it gets — the goal here is to set up the *foundations* now so performance doesn't quietly degrade once real art, VFX, and more enemies get added later. Retrofitting optimization after the fact is much more painful than building it in from the start.

### Step 1 — Pick the render pipeline deliberately
1. Use **URP (Universal Render Pipeline)**, not HDRP or Built-in, for this project. Given the earlier engine discussion (starting systems-first, wanting broad performance headroom), URP gives you the best performance-per-visual-fidelity ratio and scales down to lower-end hardware far more easily than HDRP, while still supporting Cinemachine, Timeline, and reasonably good lighting (URP's Forward+ renderer handles the kind of mixed indoor/outdoor lighting this game needs — village interiors, forest canopy, battlefield exteriors).
2. Set this up **before** any real content is built on top of it — a render pipeline switch later means re-authoring materials and shaders project-wide.

### Step 2 — Object pooling from day one
1. Build a generic object pool system now and route **everything spawned repeatedly** through it: enemies, projectiles (sling/bolas), VFX (hit sparks, dust), and any UI popups (damage numbers, prompts).
2. This matters even with primitive placeholders — the habit and the code path matter more than the current cost, and it's far easier to build this convention in now than to retrofit it once fifty enemy prefabs exist.

### Step 3 — Scene/mission streaming
1. Structure scenes per the Mission List (one scene per mission, as already recommended in the build prompt) and load them via **Addressables** with additive scene loading, rather than one giant persistent scene holding the whole game.
2. Only keep the current mission's scene (plus maybe the next one, pre-loading) resident in memory — this directly prevents the single biggest cause of resource bloat in large story-driven games: everything from every mission staying loaded at once.

### Step 4 — LOD groups and culling, set up structurally now
1. Even on placeholder primitives, add a **LODGroup** component as a matter of convention on every character/enemy prefab (even if LOD1/LOD2 are just lower-poly stand-ins or the same mesh for now) — this means when real models arrive, LOD variants slot into an existing system instead of requiring new infrastructure.
2. Enable **Occlusion Culling** (Window → Rendering → Occlusion Culling) once your first real environment geometry exists (Píllaro village, the training camp) — bake it per-scene as environments get built out, not just once at the end.

### Step 5 — Batching and instancing
1. For any repeated prop (trees, rocks, training posts, later: armor/weapon props across many enemy instances), enable **GPU Instancing** on the material and keep prop variants to a reasonable count — many *instances* of few *unique* meshes/materials is far cheaper than many unique props.
2. Mark static, non-moving environment pieces as **Static** in the inspector so Unity's static batching can merge draw calls automatically.

### Step 6 — Animation cost control
1. Use **Animator culling modes** set to "Cull Update Transforms" or similar off-screen settings so characters/enemies well outside camera view don't pay full animation cost.
2. For crowd scenes (battlefield missions, M3.1/M3.3 siege content), consider simplified animation rigs or even animation LOD (fewer bones, lower update rate) for background combatants who aren't the player's current focus — full-fidelity rigs only for nearby, camera-relevant characters.

### Step 7 — Audio
1. Route all one-shot combat/environment SFX through a pooled **AudioSource** system (same principle as Step 2) rather than instantiating new AudioSources per sound.
2. Compress ambient/music tracks appropriately (streaming, compressed formats) versus short combat SFX (decompress-on-load, uncompressed/lightly compressed) — Unity's audio import settings let you set this per-clip; get this convention right early since re-importing hundreds of audio assets later is tedious.

### Step 8 — Physics
1. Keep collider complexity low on characters/enemies (capsule/box colliders for gameplay, not mesh colliders) — this matters more, not less, once real art replaces primitives, since detailed visual meshes should almost never double as physics colliders.
2. Limit the number of active Rigidbodies during large battle scenes (M3.1, M3.3, M5.1) — background combatants can often be handled with simpler kinematic movement rather than full physics simulation.

### Step 9 — Quality tiers and post-processing budget
1. Set up **URP Quality/Volume profiles** for at least two tiers (e.g., Standard and Performance) early, even before you know your final target hardware — toggling heavier post-processing (bloom, SSAO, the vision-sequence Depth of Field from Part A) per tier is far easier to wire in now than after dozens of scenes already assume one fixed profile.
2. Keep the heaviest post-processing (the vision-sequence DoF, any bloom on fire/gold per the World Bible's palette rules) scoped to specific Volumes that trigger only during those specific scenes, not as a global always-on setting — this keeps your baseline frame cost low everywhere else.

### Step 10 — Profile continuously, not just at the end
1. Use the **Unity Profiler** and **Frame Debugger** from the very first playable build (Puma-kit-vs-training-dummy) onward, not just once the game is "done" — establishing a performance baseline early makes it obvious exactly which later addition (a new enemy type, a new VFX, a new environment) caused a regression, since you'll have a clean prior baseline to compare against.
2. Revisit this step every time a major system from the build order (Combat Design Doc's kit rollout, Mission List's environments) lands — treat it as a recurring checklist item, not a one-time task.

---

## SUMMARY CHECKLIST FOR THE CODING AI

If handing this doc to the same coding AI building the systems, the load-bearing asks are:
- Third-person Cinemachine setup (Parts A, Steps 1–3) before any other camera work
- URP as the render pipeline, decided now, not later (Part B, Step 1)
- Object pooling and Addressables-based scene streaming built as core conventions from the very first systems, not retrofitted (Part B, Steps 2–3)
- LOD groups and Static flags applied as a matter of habit on every prefab, even placeholder ones (Part B, Step 4)
