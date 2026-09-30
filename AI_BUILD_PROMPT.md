# Build Prompt — Rumiñahui Unity Implementation

Paste this whole prompt into your coding AI session (e.g. Claude Code), opened at the root of this repository.

---

## CONTEXT

This repository contains the full design documentation for an action-adventure game about Rumiñahui, in `/docs`. Read `README.md` first — it tells you which docs are code-relevant and in what order.

**Your task:** build this out as a Unity (C#) project, starting from systems and gameplay code, using simple placeholder assets (primitive shapes, capsules, basic materials/colors) standing in for final art and audio, which will be swapped in later. Do not wait on art, audio, or animation to exist — everything should work and be testable with placeholders first.

## PRIMARY SOURCE DOCUMENTS (read these fully before writing code)

Read, in this order:
1. `docs/03-combat-design.md` — the core combat/ability spec (resources, movesets, abilities, upgrade trees for the three kits: Puma, Kuntur, Amaru)
2. `docs/05-m3-5-flow-diagram.md` — spec for the forced mid-combat character-swap system
3. `docs/06-m5-3-flow-and-enemy-roster.md` — spec for the free character-swap puzzle system, and the full enemy roster/AI behavior spec
4. `docs/02-mission-list.md` — the scene/level manifest and overall game structure
5. `docs/08-atoc-boss-fight.md` — a concrete boss-fight implementation template
6. `docs/11-camera-performance.md` — third-person Cinemachine camera setup, cutscene/vision-camera language, and performance foundations (render pipeline, pooling, scene streaming, LOD) — **read this before writing any camera code, and apply its Step 1 (URP) and Steps 2–4 (pooling/streaming/LOD conventions) from the very first systems you build, not as a later pass**

Use `docs/01-story-bible.md` and `docs/04-dialogue-script.md` only for naming conventions, character context, and (later) as text data sources — do not treat them as systems specs. Do not implement anything from `docs/07-world-bible.md`, `docs/09-myth-ambiguity-direction.md`, or `docs/10-asset-list.md` — those are art/production documents for a later phase.

## HOW TO WORK

- **Work autonomously in a loop.** Build one system at a time, test/verify it compiles and behaves as specified, then move to the next, without stopping to ask for approval at each step.
- **Only stop and ask me directly if:** something would be destructive/irreversible (e.g., deleting existing work), a document is genuinely self-contradictory (not just silent on a detail), or you need a decision only I can make (e.g., a real design change, not an implementation detail).
- **If a document is silent on a specific number or detail** (exact damage values, exact cooldown seconds, exact HP pools), choose a sensible placeholder value yourself, mark it clearly in code as `// PLACEHOLDER-BALANCE:` with a one-line reason, and log it (see below). Do not pause to ask about balance numbers — those are meant to be tuned later.
- **Use placeholder visuals systematically, not ad hoc:** every character/enemy should be a simple primitive (capsule/cube) in a distinct flat color, driven by a single `PlaceholderVisual` component or equivalent, so that swapping in real models later is a matter of replacing a prefab reference, not rewriting logic. Document this convention once, clearly, in the log.

## PROJECT STRUCTURE

Set up the Unity project with a structure like:
```
/Assets
  /Scripts
    /Characters        (Rumiñahui, Chaska, Atoc controllers)
    /Combat
      /Puma
      /Kuntur
      /Amaru
      /Shared           (resource system: HP/Stamina/Focus, shared interfaces)
    /Enemies            (one class/behavior per Enemy Roster entry)
    /Bosses             (Atoc boss fight, built from the template doc)
    /Systems
      /CharacterSwap     (M3.5-style forced swap manager)
      /FreeSwap          (M5.3-style free swap manager)
      /MissionManager    (scene sequencing per Mission List)
    /Placeholder         (PlaceholderVisual and related helper scripts)
  /Prefabs/Placeholder
  /Scenes                (one per mission, named to match Mission List IDs, e.g. M1.1, M1.2...)
```

## DOCUMENTATION DISCIPLINE (this is the important part)

Every time you implement a feature:
1. Add a short header comment in the relevant script(s) citing which doc and section it implements, e.g. `// Implements: 03-combat-design.md, Section 1 (Puma Kit — Basic Moveset)`
2. Append an entry to `IMPLEMENTATION_LOG.md` at the repo root (create it if it doesn't exist yet) with: date, feature built, doc/section reference, files touched, and any placeholder/assumption made where the docs didn't fully specify something.
3. Keep entries short — a few lines each. The goal is that I (or anyone else) can open the log later, find the doc that governs any given system, and edit the doc and code together with confidence, rather than having to reverse-engineer intent from code alone.

## SUGGESTED BUILD ORDER

This is a **third-person** action game — see `docs/03-combat-design.md`'s opening note and `docs/11-camera-performance.md` in full before building character controllers or cameras.

Follow the "Suggested Prep Order" at the end of `docs/10-asset-list.md` for scope, adapted to code-first:
0. Project foundations first: set the render pipeline to URP, and set up the base third-person Cinemachine camera (Part A, Steps 1–2 of `11-camera-performance.md`) plus the object-pooling and Addressables scene-streaming conventions (Part B, Steps 2–3) before writing gameplay systems — these are foundational, not a later pass.
1. Shared resource system (HP/Stamina/Focus) + Puma kit + a placeholder training-dummy target → playable Puma combat loop
2. Shield-Bearer and Skirmisher enemy AI (from the Enemy Roster doc) → a real, testable early-game encounter
3. Chaska controller + Kuntur kit
4. Atoc controller + Amaru kit, then the Atoc boss fight (`08-atoc-boss-fight.md`)
5. The M3.5 forced-swap system and the M5.3 free-swap system
6. Mission/scene manager wiring scenes together per the Mission List, using placeholder/empty scenes for content not yet built

Confirm you've read `README.md` and this prompt in full, then begin with step 1 of the build order and keep going, looping through the list above, logging as you go, until you hit a real blocker or finish the listed scope.
