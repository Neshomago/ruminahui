# RUMIÑAHUI — Combat Design Doc
### The Three Worlds Kits: Puma / Kuntur / Amaru

**Perspective: third-person.** All movesets, camera-dependent mechanics (aerial finishers, mark/tell reading, terrain-based ambush) and the M3.5/M5.3 multi-character sequences assume an over-the-shoulder third-person camera, in line with the God of War-style combat this design is built from. See `11-camera-performance.md` for the full camera spec.

Companion document to the Story Bible and Mission List. Covers mechanical specs for all three combat styles, how they're taught to the player narratively, and how the "power sharing" climax (M3.5) actually functions as a system rather than just a cutscene.

---

## 0. CORE RESOURCES (shared across all three kits)

- **Health (HP):** standard damage pool. Regenerates only at huacas (checkpoint/shrine system) — no passive regen mid-fight, keeping tension GoW-style.
- **Stamina (STA):** spent on dodges, blocks/parries, and heavy attacks. Regenerates passively but drains fast under pressure — governs pacing of the Puma kit especially.
- **Focus (FOC):** a slower-building meter, filled by landing clean hits, perfect dodges/parries, and (post-M3.5) by successfully reading enemy tells. Spent on special abilities and the ultimate. Shared meter across all three kits so that switching characters mid-mission (M3.5, M5.3) doesn't reset player momentum.

---

## 1. PUMA KIT — Rumiñahui (Kay Pacha: the world here, weight and endurance)

**Fantasy:** immovable object. You are not the fastest thing on the field, but you are the thing that is still standing when everything else has stopped.

### Basic Moveset
| Input | Move | Notes |
|---|---|---|
| Light attack (chain x3) | Stone Strikes | Standard combo, ends in a stagger-inducing third hit |
| Heavy attack | Warclub Crush | Slow, high damage, breaks guards; can be charged |
| Block (hold) | Stone Stance | Full block, chip damage only; STA drains while held |
| Block (timed) | Stone Parry | Perfect-timed block staggers most enemies and fills FOC fast |
| Dodge | Root-Step | Short, weighty sidestep — deliberately less mobile than Chaska's dodge, reinforcing the "doesn't run" theme |
| Grapple (on staggered enemy) | Grounding Throw | Context throw into terrain/other enemies |

### Special Abilities (Focus-gated, unlocked across Acts I–III)
- **Earthbreaker** (Act I) — ground-slam AoE, knocks down nearby enemies. Core crowd-control tool.
- **Unyielding** (Act II, post-Willka's death) — a defensive cooldown: for a few seconds, incoming hits stagger instead of interrupt, letting the player push through a combo they'd otherwise be knocked out of. Mechanically encodes "he doesn't freeze anymore."
- **Stone Face** (Act III, unlocked at M3.4 after sparing Atoc) — a single, unblockable heavy strike that can only be used on an enemy who is *already staggered* — mechanically, mercy/restraint is rewarded rather than pure aggression, echoing the mercy choice narratively.

### Ultimate: **What Held the Line** (unlocked M3.5)
A short window of near-invulnerability paired with heavily amplified damage, framed narratively as the trance-state from the Long Battle. Deliberately the most expensive FOC ability in the game — meant to be used once or twice per major fight, not spammed.

### Upgrade Tree (3 tiers, simple — depth via enemy variety, not a sprawling skill web)
1. **Foundation:** stamina efficiency, combo extensions
2. **Weight:** guard-break scaling, throw damage, Earthbreaker radius
3. **Stillness:** Unyielding duration, Stone Parry window widened, Stone Face cost reduced

---

## 2. KUNTUR KIT — Chaska (Hanan Pacha: the world above, sight and speed)

**Fantasy:** always one step ahead. You win by not being where the hit lands.

### Basic Moveset
| Input | Move | Notes |
|---|---|---|
| Light attack (chain x4) | Twin-Blade Flurry | Faster, lower per-hit damage than Puma, rewards sustained combos |
| Heavy attack | Bolas Snap | Ranged, wraps a single target — can be followed by a melee finisher for bonus damage |
| Dodge | Skywalk | Longer dash than Rumiñahui's, can be chained into a second dodge at STA cost |
| Aerial (after a launcher or from height) | Falling Star | Airborne combo string, ends in a ground finisher |
| Mark (ranged, no cooldown) | Condor's Eye | Highlights a weak point on marked enemy for bonus damage — the game's primary "tell-reading" mechanic |

### Special Abilities
- **Vantage Leap** (Act I, tutorialized in M1.1) — traversal/combat hybrid: launches to a height point, usable both for platforming and to reposition mid-fight for a diving attack.
- **Sky-Cut** (Act III, M3.2) — a ranged volley that can hit multiple marked targets at once. Rewards using Condor's Eye proactively rather than reactively.
- **Star-Fall** (Act III, post-M3.5) — a high-damage aerial finisher usable only on enemies below a certain HP threshold — an execute move, mirroring how her arc shifts from reckless speed to precision.

### Ultimate: **The Gap in the Line** (unlocked M3.5)
Time briefly slows for everyone but Chaska, letting the player chain a full combo across multiple enemies. Visually tied to the "condor that sees the gap" line from Willka's story.

### Upgrade Tree
1. **Foundation:** dash distance/chain count, mark radius
2. **Height:** Vantage Leap cooldown, aerial combo extensions
3. **Precision:** Star-Fall threshold raised (executes tougher enemies), slow-time duration on ultimate

---

## 3. AMARU KIT — Atoc (Uku Pacha: the world below, cunning and rebirth)

**Fantasy:** the fight you don't see happening until it's already over. Reactive, trap-based, punishes players who button-mash.

### Basic Moveset
| Input | Move | Notes |
|---|---|---|
| Light attack (chain x3) | Fang Strikes | Fastest but lowest raw damage of the three kits; built around setups, not standalone damage |
| Heavy attack | Coil Grab | Short-range grapple that pulls an enemy off-balance or into a placed trap |
| Dodge | Shed Skin | A dodge that leaves a brief decoy afterimage — enemies mid-attack can be baited into hitting the decoy |
| Counter (timed, not a block) | Venom Riposte | Instead of blocking, sidesteps and applies a poison-style debuff (herbal/toxin-based, not literal magic) on a successful read |
| Placeable | Snare | A terrain trap (thorns, deadfall, tripline) placed pre-fight or mid-fight in a safe moment |

### Special Abilities
- **Numbing Draught** (Act III, introduced as enemy general's ability, unlocked for player after his redemption lands post-M3.6) — an AoE debuff that slows a group of enemies, opening them to Rumiñahui/Chaska follow-ups in co-op sections.
- **False Trail** (Act V, M5.4) — a full stealth-repositioning tool; briefly makes Atoc undetectable, core to the "guerrilla war" mission design.
- **Last Fang** (Act V) — a high-risk counter: if timed perfectly against a heavy attack, deals massive damage, but whiffing it leaves Atoc fully exposed. Encodes his character as still someone who takes bigger risks than the other two.

### Ultimate: **A Hundred and One** (unlocked M3.5)
On activation, if Atoc is reduced to critical HP within the next several seconds, he instead "dies" and immediately resurfaces at nearly full health with a damage buff for a short window — a mechanical dramatization of the serpent-rebirth line, and the game's one true "cheat death" tool, deliberately rare and dramatic rather than a repeatable safety net.

### Upgrade Tree
1. **Foundation:** trap capacity, decoy duration
2. **Venom:** debuff potency/duration, Venom Riposte window widened
3. **Rebirth:** Last Fang damage scaling, A Hundred and One cooldown reduced

---

## 4. HOW THE THREE KITS INTERACT (co-op / ally-assist design)

For most of the game, the player controls **Rumiñahui**, with Chaska and/or Atoc as AI allies who can be issued simple context commands (call-in a mark, call-in a trap, call-in a dodge-assist) rather than full squad-command complexity — keeping the core loop about Rumiñahui while still making the other two feel present and useful.

**Exceptions where the player directly controls Chaska or Atoc:**
- M3.2 ("The Fox's Ground") — full Chaska level
- M3.5 ("What Held the Line") — perspective shifts between all three mid-mission, unlocking each kit live as the story demands it
- M5.3 ("Into the Llanganates") — free switching between all three for the traversal puzzle
- M5.7 (epilogue) — Chaska only

**Design intent:** the player should never feel like they're managing three separate build trees at once. Rumiñahui is the throughline; Chaska and Atoc's kits exist to be occasionally *worn*, not permanently juggled — mechanically reinforcing the "three worlds in one skin" theme rather than turning this into a full party-based RPG.

---

## 5. ENEMY DESIGN SYNERGY (brief)

- **Andean-era enemies** (Acts I–III): shielded formations that reward Puma's guard-break and Earthbreaker; skirmishers that reward Condor's marking and aerial finishers; ambush-type enemies that reward Amaru's counters and traps. Encounter design in Act III should mix all three enemy archetypes together to make the M3.5 kit-switch feel earned rather than arbitrary.
- **Spanish-era enemies** (Act V): armored infantry with different guard-break thresholds than Andean shields (reinforces that Act V "feels different" mechanically, not just narratively); cavalry as a new verticality threat that specifically rewards Condor traversal to avoid; and firearm-users as the first enemy type in the game that punishes standing still — a deliberate late-game rug-pull on the Puma-heavy player who's been playing patiently all game.

---

## NEXT STEP
Full dialogue script for the Prologue (M0.1–M0.2) and the ending (M5.5–M5.7), incoming next.
