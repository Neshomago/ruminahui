# Rumiñahui — Action Game Project

Narrative and design documentation for an action-adventure game about Rumiñahui, built as reference material for AI-assisted Unity development (starting with code/systems, generic placeholder assets, real art to follow later).

## Repo structure (suggested)

```
/docs                     ← all design documents (this package)
/Assets                   ← Unity project assets (created once the Unity project is initialized)
  /Scripts
  /Prefabs/Placeholder
  /Scenes
/AI_BUILD_PROMPT.md        ← paste this into your coding AI to start/continue implementation
/IMPLEMENTATION_LOG.md      ← generated and maintained BY the coding AI as it builds — do not write this by hand
```

## Docs index (in build-priority order)

| # | File | Use for code? | What it's for |
|---|---|---|---|
| 01 | `story-bible.md` | Context only | Character names, arcs, tone — for naming/comments, not systems |
| 02 | `mission-list.md` | **Yes — primary** | Scene/level manifest, mission sequencing, checkpoint structure |
| 03 | `combat-design.md` | **Yes — primary** | Core combat spec: resources, movesets, abilities, upgrade trees |
| 04 | `dialogue-script.md` | Context / data later | Becomes text data once scenes are wired up, not engine logic |
| 05 | `m3-5-flow-diagram.md` | **Yes — primary** | Spec for the forced-character-swap mid-combat system |
| 06 | `m5-3-flow-and-enemy-roster.md` | **Yes — primary** | Spec for the free-character-swap puzzle system, plus full enemy AI spec |
| 07 | `world-bible.md` | Not for code | Art/environment direction — hold until art phase |
| 08 | `atoc-boss-fight.md` | **Yes — secondary** | Boss fight implementation template, once base combat works |
| 09 | `myth-ambiguity-direction.md` | Not for code | Camera/sound/color direction for vision sequences — art/cinematics phase |
| 10 | `asset-list.md` | Not for code | Production checklist for art/audio — not a coding input |
| 11 | `camera-performance.md` | **Yes — primary** | Third-person camera/Cinemachine setup, cutscene/vision-camera language, and performance/optimization foundations (URP, pooling, streaming, LOD) |

## How to use this repo

1. Initialize this as a git repo (`git init`) and commit `/docs` first, as the project's source of truth.
2. Hand `AI_BUILD_PROMPT.md` to your coding AI (Claude Code or similar) to begin implementation.
3. As the AI builds, it should maintain `IMPLEMENTATION_LOG.md` at the repo root — a running record of what was built, which doc/section drove each decision, and any assumptions made where the docs didn't specify something. **This log is what makes future edits safe**: before changing any system, check the log entry for it to see which doc governs that feature and update the doc and the code together.
4. Real art/audio assets get swapped in later using `10-asset-list.md` as the production checklist and `07-world-bible.md` / `09-myth-ambiguity-direction.md` as direction — none of that blocks starting the code now.
