# Design Direction: Fun First, Learning Follows

**Date**: 2026-08-13
**Status**: Active — guides all implementation decisions

---

## The Shift

The original vision was a text-based educational game that teaches coding. It has grown into something bigger — a full game with a Unity build, narrative arc, and multiple technology tracks. With that growth comes a critical reframing:

**Old priority**: Learning first, fun second.
**New priority**: Fun game where the player happens to learn.

The game should feel like a *game* — with progression, stakes, mastery expression, and satisfaction — not a gamified tutorial. The coding skills emerge naturally from engaging gameplay, not the other way around.

---

## Analysis: What Was Missing

We evaluated the prototype against a 5-Component game design framework (Clarity, Motivation, Response, Satisfaction, Fit). The diagnosis:

### Motivation — The Weakest Component

The player's only motivation was "the contract says to do this." No stakes, no persistent economy, no risk, no reason to care about *how well* they solved a problem.

**What TFWR does right**: The farm is always running. Currency accumulates. Upgrades are gated by currency. The player is motivated by *wanting to unlock* the next thing.

**Our gap**: Contracts were binary — fixed or not. No "I fixed it but could fix it *better*." No resource accumulation. No persistent world reflecting competence.

### Satisfaction — Only One Feedback Channel

Text output was the only feedback. The game design framework requires minimum 2 channels for significant actions. A brute-force 50-line solution and an elegant 3-line solution got the same "Contract Complete."

### Core Loop — Flat

The loop (board → briefing → code → pass/fail → debrief → board) had no escalating tension, no replayability, no persistent state changing how the next contract felt.

### Clarity & Response — Strong

Error messages that guide the player, system feedback on wrong actions, and the "feel the limitation first" philosophy are the game's strongest design instincts. These should be preserved and amplified.

---

## What We Built: Gamification Layer

### Star Ratings (1-3★)

Every contract now awards a performance rating:

**Python/Combined contracts** — rated by function call efficiency:

> ⚠ **These thresholds are known-defective — see `ECONOMY.md` Part 1 (audit, 2026-08-14).** Two confirmed problems: (1) randomised contracts (C8/C10/C11) grade partly on the dice roll, because thresholds are absolute while the optimal call count varies; (2) call count cannot distinguish an unrolled solution from a loop — they make identical calls — so the "replay with better tools for 3★" loop is structurally impossible on C1–C4, and C1's naive first attempt already scores 3★. Do not treat the table below as authoritative.

| Contract | 3★ | 2★ | Min Calls |
|----------|-----|-----|-----------|
| C1 Power Node | ≤13 | ≤16 | 12 |
| C2 Drone Route | ≤10 | ≤15 | 9 |
| C3 Drone Dispatch | ≤17 | ≤19 | 16 |
| C4 Transit Signals | ≤14 | ≤18 | 13 |
| C8 Grid Restoration | ≤15 | ≤25 | 9-13 |
| C10 Water Treatment | ≤32 | ≤42 | 26-30 |
| C11 Sector Sweep | ≤33 | ≤40 | 25-31 |

**Terminal contracts** — rated by bonus discovery:
- 2★ on completion (exploration is the skill)
- 3★ requires finding hidden bonus content

**The replayability loop**: Early contracts completed with basic tools earn 1-2★. After learning better tools (controlled while loops, for loops, functions), the player returns and earns 3★ with cleaner code. This naturally rewards mastery.

### Credits

Each contract pays credits scaled by star rating:
- Credits = BASE_CREDITS × star_count
- Replay with improved rating earns the difference
- Bonus objectives (hidden files, extra discoveries) worth +50cr each
- Credits accumulate across the session

| Contracts | Base Credits |
|-----------|-------------|
| C1-C2 | 100 |
| C3-C4 | 150 |
| C5-C7, C9 | 150 |
| C8 | 200 |
| C10 | 250 |
| C11 | 300 |

### Contractor Rank

Progression based on total stars (max 33 from 11 contracts):

| Stars | Rank |
|-------|------|
| 0-5 | Junior Contractor |
| 6-12 | Contractor |
| 13-20 | Senior Contractor |
| 21-28 | Systems Engineer |
| 29-33 | Chief Architect |

### Bonus Objectives

Each contract has 1 optional bonus — discoverable through exploration:

| Contract | Bonus |
|----------|-------|
| C5 | Find hidden .bash_history |
| C6 | Read /etc/firewall.conf |
| C7 | Read migration.log |
| C8 | Find hidden command reference |
| C9 | Find hidden intrusion trace |
| C10 | Find hidden command reference |
| C11 | Find hidden diagnostic manual |

---

## Implications for Unity

### Contract Board → Overmap

The contract board is the **overmap** — a stylised district map of Neo-Kyoto where contracts are selected by clicking a district. It should display:
- Contractor rank and title prominently
- Total credits and star count
- Per-contract star ratings (filled/empty stars)
- Credit amounts earned per contract
- Visual distinction between 1★/2★/3★ (bronze/silver/gold, or color intensity)
- **District state** — a district where the player has 3★ on everything should read differently on the map from one still failing

The overmap also gives the Act 2 branching structure a physical form: the tree in `GDD.md` §3 becomes a map lighting up with available work.

### Performance Display

**Ordering matters: the world reveal comes first, the scoreboard second.**

On completion, the jack-out sequence pulls the camera back to reveal the location *now working*, with the ambient swelling to its fixed state. Only then does the summary appear. Satisfaction before score — if the numbers land first, the player reads the scoreboard and never looks at the city they just repaired.

The summary itself shows:
- Star rating with visual fanfare (1★ = modest, 3★ = celebration)
- Call count vs. target (for scripting contracts)
- Credits earned
- Bonus objective status
- Rank progress bar

**Bonus objectives need their own feedback at the moment of discovery.** Currently a hidden file is found and nothing happens until this screen, minutes later — one delayed channel, which fails the two-channel minimum. Toast it the instant it is read.

### Visual Feedback Per Star Level

Stars should map to visual satisfaction:
- **1★**: System works but looks basic — lights on, minimal flair
- **2★**: System works well — smooth animations, good color
- **3★**: System works perfectly — extra particle effects, satisfying cascade, the "wow" moment

This means the world-as-feedback principle gets richer: the player doesn't just see their code work, they see *how well* it works.

### Replay Flow

The Unity UI should make replaying easy:
- Completed contracts show current star rating on the board
- A "REPLAY" option on completed contracts
- On replay, show the improvement: "★☆☆ → ★★★ (+200cr)"
- Consider a "replay all" or "optimize" mode for endgame

---

## New Contracts (C9-C11)

Three new contracts were added to the prototype:

### C9 — Process Lockdown (Terminal, Comms Tower)
- Teaches `ps` and `kill` (process management)
- 4 rogue processes to identify and terminate
- Hidden intrusion trace file plants the Act 2 investigation breadcrumb (stolen SSH key, origin District 9)
- Narrative seed: "Cross-reference with personnel database recommended. [Requires database access.]" — foreshadows SQL

### C10 — Water Treatment (Combined, Underground Plant)
- Forces code duplication: pump stations and intake valves need identical 4-step repair
- The twist: after fixing stations, valves are revealed needing the same procedure
- Player succeeds but code smells — this is the "feel the limitation" moment for functions
- **Unlocks `def`** (function definitions)

### C11 — Sector Sweep (Combined, Industrial Zone)
- First `def` contract — player writes `fix_line(line_id)` function
- 12 production lines with randomized JAMMED/OVERHEATED failures
- Function must diagnose, branch, and restart — combines everything learned
- The milestone: "using tools → building tools"

---

## What's Next

### Near-term (prototype)
- Functions with return values
- String methods / data processing
- SQL introduction (investigation arc — C9 already seeds this)
- Git forensics (tracing tampered configs)

### Design principles going forward
1. **Every new feature needs a Motivation answer**: Why does the player *want* to use this? Not "because we're teaching it" but "because the game makes them need it"
2. **Stars should feel fair but aspirational**: 1★ should always be achievable with current tools; 3★ should reward mastery or replaying with better tools
3. **Credits sink: the deck.** ~~Right now they accumulate.~~ **Resolved (2026-08-14)** — the contractor carries a physical deck, and credits buy tools for it. See `ONSITE_PIVOT.md` §3. Guard rail: story-critical technologies (SQL, Git) are *granted by the narrative*, never purchased. The store sells conveniences and alternatives — terminal themes, a faster scanner, a diagnostic that reveals one hidden file per contract, extra window slots. If SQL costs credits, players grind old contracts for money and the Motivation win becomes a chore.
4. **The world should reflect mastery**: A district where the player has 3★ on all contracts should look and feel different from one with 1★ completions
5. **Fun first, always**: If a mechanic teaches something but isn't fun, redesign it. If a mechanic is fun but doesn't teach, that's fine — games have non-educational moments too

---

## Reference

- **Game design skill**: `.claude/skills/game-design/` — 5-Component Filter, debugging protocol, playtest templates
- **GDD**: `docs/GDD.md` — full game design document
- **Prototype**: `main.py --dev` — run with all contracts unlocked
