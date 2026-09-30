# RUMIÑAHUI — Boss Fight Deep Dive: Atoc (M3.4 "The Mercy")
### Companion document to the Combat Design Doc and Enemy Roster

The game's first true boss fight, and mechanically the template every later boss should be judged against. This fight has to do three jobs at once: teach the player to read Amaru-kit tells (since they'll rely on an AI ally using this exact kit from M3.6 onward), stand as a satisfying skill test, and end in a mandatory non-lethal resolution that has to feel earned rather than scripted-past.

---

## 1. ARENA DESIGN

- **Location:** the breach point at Mullihambato, mid-siege — the arena should carry visible damage/debris from the battle already raging around it (see Mission List M3.4), reinforcing that this is one duel inside a much larger fight, not an isolated boss room.
- **Layout:** a roughly circular cleared space bounded by battle wreckage (broken palisade, fallen banners) rather than an obviously artificial "boss arena" — wreckage doubles as sightline-breaking cover Atoc can exploit (see Phase 2).
- **Ambient chaos:** background combat should be visible/audible throughout (distant fighting, not directly interactive) — keeps tension high without the fight itself needing extra mechanical noise.

---

## 2. PHASE STRUCTURE

### Phase 1 — "Reading Him" (100%–65% HP)
- Atoc fights primarily with direct strikes and a moderate trap-and-reposition rhythm — the fight's tutorial phase for Amaru tells.
- **Key tell to teach:** a brief crouch-and-glance before he drops a Snare — the same tell used by every Amaru-kit ally moment from M3.6 onward, so players who learn it here get payoff for the rest of the game.
- **Player goal:** land Stone Parries against his direct strikes to build Focus early, since Phase 3 is Focus-expensive.

### Phase 2 — "The Fox's Ground" (65%–30% HP)
- Atoc starts using the arena's wreckage for Shed Skin decoys and brief stealth repositioning — the fight's hardest phase, deliberately named to echo M3.2 (Chaska's earlier scouting mission establishing his tactical brilliance), rewarding players who paid attention there.
- **Key tell to teach:** decoys don't fully replicate his idle animation — a subtle "tell within a tell," giving skilled players a real edge without requiring a strict pattern memorization.
- **Player goal:** avoid button-mashing at decoys (wastes Stamina); use Puma's Root-Step to reposition and force him back into the open instead of chasing.

### Phase 3 — "Cornered" (30%–0% HP, until stagger)
- Atoc drops the decoy play and fights with increasing directness and risk — narratively, he's out of tricks and out of ground to give, mechanically mirrored by faster, more aggressive (and more punishable) attacks.
- **Key tell to teach:** a full-commitment unblockable string, telegraphed by a distinct roar/stance change — same "wind-up roar" language used later for Lieutenant-tier enemies, so this fight also doubles as advance teaching for Act III's mini-bosses.
- **Player goal:** survive the aggression, land the stagger.

### Final Beat — The Mandatory Spare
- Once staggered at low HP, the normal "finishing blow" input is replaced by the **spare prompt** flagged in the Mission List (M3.4) — this should be visually and mechanically distinct from every other finisher in the game (different animation, no damage number, no kill-cam) so it reads immediately as a different kind of moment, not just a reskinned execute.
- No player input can change this outcome — Rumiñahui spares him regardless — but the game should never take the *fight itself* out of the player's hands. Only the ending of it.

---

## 3. WHY THIS FIGHT MATTERS STRUCTURALLY

- It's the only time in the game the player fights a *full* Amaru kit as an opponent rather than alongside it as an ally — everything learned here should feel like it "unlocks understanding" of Atoc as a character for the rest of the campaign, not just of the enemy type.
- The mandatory non-lethal ending is the story's first hard proof that Rumiñahui's arc is bending away from pure hardness — worth flagging to whoever handles boss-fight UI/UX that a "no fail state on the ending, but full skill test on the fight" design is intentional and shouldn't be "fixed" later into a binary spare/kill choice. There is no choice. There's only whether the player earns the moment through the fight.

---

## 4. TEMPLATE FOR LATER BOSSES (if this pattern gets reused)

If future bosses follow this three-phase structure:
1. **Teach** a new tell in Phase 1 at low risk
2. **Complicate** it in Phase 2 using the specific arena/context
3. **Escalate** in Phase 3 into higher-risk, higher-reward aggression
4. **Land a story beat** in the final moment that only this specific fight could deliver — never a generic victory pose

Recommend this same four-beat structure for any future named-enemy encounter (e.g., a Spanish Officer boss variant in Act V, if one gets added beyond the mini-boss tier already in the Enemy Roster).
