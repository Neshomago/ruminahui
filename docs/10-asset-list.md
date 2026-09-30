# RUMIÑAHUI — Master Asset List
### Everything needed to move from documentation to production, organized by category

Cross-referenced against the Story Bible, Mission List, Combat Design Doc, World Bible, Boss Fight doc, and Myth Ambiguity doc. Use this as the checklist when scoping art, audio, and animation work — each item notes which mission(s) it's needed for first.

---

## 1. PLAYABLE CHARACTER MODELS & RIGS

| Asset | Variants needed | First needed |
|---|---|---|
| Rumiñahui — child model | 1 (Prologue only) | M0.1 |
| Rumiñahui — teen model | 1 | M1.1 |
| Rumiñahui — young adult/soldier model | 1–2 (soldier → general armor tiers) | M2.1 |
| Rumiñahui — general/Act III model | 1 (war-worn) | M3.1 |
| Rumiñahui — Act V model | 1 (gaunt, weathered, per World Bible Sec. 7) | M5.1 |
| Chaska — hunter/scout model | 1 | M1.1 |
| Chaska — captain model | 1 | M3.1 |
| Atoc — antagonist-faction model | 1 (Huáscar regional dress) | M3.3 |
| Atoc — allied model | 1 (adopts shared visual elements post-M3.6) | M3.6 |

**Rigging requirements:** full combat rig for all three (light/heavy attack chains, dodge, block/parry, grapple) per Combat Design Doc Sections 1–3; child-model Rumiñahui only needs a basic movement/hide rig (no combat animations required, per M0.1's design).

---

## 2. NON-PLAYABLE CHARACTER MODELS

| Asset | Notes | First needed |
|---|---|---|
| Willka model | Aging warrior-priest; 60s | M0.2 |
| Anta model | Village warrior, Prologue only | M0.1 |
| Village elder / generic Píllaro villagers (3–5 variants) | Background/NPC dialogue | M0.1 |
| Camp children (2–3 variants) | For the naming scene | M0.2 |
| Atahualpa model | Court dress + battlefield dress | M2.2 |
| Spanish Officer / Interpreter models | Kept deliberately generic per Dialogue Script notes | M5.6 |

---

## 3. ENEMY MODELS (see Enemy Roster Spec Sheet for full behavioral detail)

| Asset | Notes | First needed |
|---|---|---|
| Skirmisher | Andean-era, light armor | M1.3 |
| Shield-Bearer (basic + armored variant) | Andean-era | M1.3 / M2.1 |
| Ambush-Type / Highland Scout | Andean-era | M2.3 |
| Atoc's Lieutenant | Mini-boss, unique model | M3.3 |
| Atoc (boss version) | Unique — see Boss Fight doc | M3.4 |
| Spanish Infantry | Armored, distinct silhouette from Andean shields | M5.1 |
| Spanish Cavalry (rider + horse) | Needs mount rig/animations | M5.1 |
| Arquebusier | Includes firearm prop + muzzle-flash VFX | M5.2 |
| Spanish Officer (mini-boss) | Unique model, rally animation | M5.4 |

---

## 4. ENVIRONMENTS (see World Bible for full palette/mood direction)

| Environment | Key missions | Notes |
|---|---|---|
| Píllaro village (intact) | M0.1, M0.2 | Warm-lit interior/exterior set |
| Píllaro village (raided/aftermath) | M0.1 end, M5.7 reference | Damaged variant of the above |
| Forest training camp | M1.1–M1.4 | Willka's camp, fireside huaca |
| Frontier garrison | M2.1 | Functional military architecture |
| Imperial review ground | M2.2 | First large-scale vista |
| Mountain pass (ambush site) | M2.3 | Willka's death location |
| Army camp (multiple redress) | M2.4, M3.1, M4.1–M4.3 | Reusable modular camp set |
| Mullihambato battlefield | M3.1, M3.3, M3.4 | Large-scale siege environment |
| The huaca (battlefield version) | M3.5 | Visually rhymes with the fireside huaca — see World Bible Sec. 3 |
| Camp (post-battle, quiet) | M3.6 | Redress of army camp set |
| Quito, intact | M5.1 | Most visually rich location in the game |
| Quito, burning | M5.2 | Palette-corrupted redress of the above |
| Llanganates foothills/approach | M5.3 | Puzzle-traversal environment |
| Llanganates hiding chamber | M5.3 | Small, deliberately unglamorous interior |
| Highland guerrilla-war locations (multiple small sites) | M5.4 | Can reuse modular highland terrain kit |
| Sigchos capture site | M5.5 | Small, tense, mirrors Prologue village scale |
| Spanish holding quarters | M5.6 | Sparse interior |
| Execution ground | M5.6 | Plain, unglorified exterior |
| Llanganates ridge (epilogue) | M5.7 | Same terrain, warmer late-day lighting |

---

## 5. PROPS & SET DRESSING

- Village life props: cook-fires, thatch/stone structure kits, terraced-field set pieces, tools, textiles
- Training props: wooden practice weapons, training posts/dummies, sling and bolas props
- Military props: Inca-era weapons (warclubs, spears, slings, bolas), shields, banners, armor sets per rank tier
- Imperial props: royal regalia, court furnishings, trapezoidal-doorway architectural kit (see World Bible Sec. 3), goldwork set-dressing (reserved specifically for imperial/Atahualpa scenes per palette rules)
- Spanish-era props: European armor/weapon kit, firearms, cavalry tack/saddlery, siege equipment
- The treasure itself: a modest, non-spectacular prop design per World Bible/Mission List direction — explicitly not meant to look like a glittering hoard on screen
- Traps/deadfalls (Amaru kit props, both player-placed and Llanganates environmental versions)
- Huaca sacred-site props: stone circle elements, the twisted tree (recurring across two locations — see World Bible Sec. 2 and 3)

---

## 6. VFX

- Combat VFX per kit: Puma impact/dust (ground-based, weighty), Kuntur wind/motion trails (aerial, sharp), Amaru poison/decoy shimmer (subtle, non-magical per Myth Ambiguity rules)
- Fire/smoke VFX: village raid (Prologue), Quito burning (Act V) — two distinct fire "moods" per World Bible's harsh/unflattering direction for the Act V fire specifically
- Weather/atmosphere: mist (Píllaro), fog/low cloud (Llanganates), dust (battlefields)
- Vision-sequence VFX: soft-focus/overexposure treatment, silhouette/shadow-shape suggestion effects (NOT particle-based magic effects — see Myth Ambiguity doc Section 4)
- Firearm muzzle-flash/smoke (Arquebusier)
- Blood/impact effects calibrated to the "serious but restrained" tone — implied and brief rather than graphic, especially for M5.6

---

## 7. AUDIO

**Music:**
- Distinct thematic material for: Píllaro/forest (Acts I and Prologue), imperial court (Acts II–IV), the Long Battle (Act III climax), Act V's descent (fire/guerrilla war/ending)
- A specific recurring motif tied to the Three Worlds story, introduced in M1.2 and reprised (transformed) in M3.5 and the M5.7 epilogue

**SFX:**
- Combat SFX per kit (weapon-specific impacts for Puma/Kuntur/Amaru movesets)
- Environmental ambience per location (see Environments list — wind, fire, water, crowd where relevant)
- The specific wind-through-highlands cue used to bookend the Prologue and the execution scene (Dialogue Script, M5.6) — needs to be the same audio asset both times

**Voice-over:**
- Full VO for Rumiñahui (all ages — recommend at least 2 separate voice actors: child, and one continuous adult voice from teen through Act V, aged via direction rather than a third VO switch, per Boss Fight/Dialogue notes on vocal consistency)
- Full VO for Chaska, Atoc, Willka, Atahualpa
- Willka's specific fireside lines (M1.2) need to be flagged for reuse/reprocessing in M3.5 per the Myth Ambiguity doc — do not re-record new lines for that callback
- Minor VO: Anta, village elder, camp children, Interpreter, Spanish Officer (can be a smaller pool of voice actors covering multiple minor roles)

---

## 8. UI / HUD

- Combat HUD elements: Health/Stamina/Focus meters (shared system, Combat Design Doc Sec. 0)
- Kit-switch indicator (radial or shoulder-button cycle, per M5.3 Flow Diagram technical notes)
- Context-sensitive prompts: QTE prompts (Prologue, Sigchos), the distinct "spare" prompt (M3.4, visually different from normal finishers), the "stay silent" prompt (M5.6)
- Ability/upgrade tree UI for all three kits
- Mission/objective UI, huaca/checkpoint UI

---

## 9. CINEMATIC-SPECIFIC NEEDS

- Facial animation/mocap (or equivalent) sufficient for Rumiñahui's minimal-expression performance style (Dialogue Script production notes) — this is a harder animation problem than an expressive character, worth flagging early since subtlety is difficult to sell without good facial capture
- Camera rigs/tooling for the vision-sequence language (handheld drift, focus-breathing) as a reusable cinematic camera preset, not a one-off per scene

---

## SUGGESTED PREP ORDER
If prioritizing for a first vertical-slice build, this order gets you a playable, presentable chunk fastest:
1. Rumiñahui (teen model) + Puma kit animations + training-camp environment → enough for M1.1–M1.2
2. Add Shield-Bearer/Skirmisher enemy models → enough for M1.3
3. Add Chaska model + Kuntur kit → enough for a vertical slice through early Act III
4. Everything else can follow in story order from there
