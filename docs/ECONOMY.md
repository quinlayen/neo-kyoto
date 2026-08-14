# Economy: Star Ratings & The Deck Store

**Date**: 2026-08-14
**Status**: Current thinking
**Backlog items**: B3 (star threshold audit), B2 (deck store taxonomy)

---

# Part 1 · Star Rating Audit (B3)

`DESIGN_DIRECTION.md` sets seven contracts' 3★/2★ cutoffs with no stated derivation — a Numbers Policy violation. Auditing them against the code turned up two defects that matter more than the numbers.

## Verified correct

**`print()` does not count toward the call total.** `interpreter.py:89` places `print` directly into `restricted_globals`, bypassing `_wrap_command`. Only domain commands are counted. This is right — C1's briefing actively encourages `print()` for visibility, and penalising it would punish the debugging habit the game teaches.

## Defect 1 · Randomised contracts grade partly on luck

`base.py:39-44` compares the call count against **fixed absolute thresholds**. But three contracts randomise their workload:

```python
broken = random.sample(self.ALL_SECTORS, random.randint(8, 12))   # power_grid.py:13
```

The optimal solution's call count therefore varies run to run, while the threshold does not:

| Contract | Optimal calls | 3★ at | Slack (lucky roll) | Slack (unlucky roll) |
|---|---|---|---|---|
| C8 Grid Restoration | 9–13 | 15 | **6** | **2** |
| C10 Water Treatment | 26–30 | 32 | **6** | **2** |
| C11 Sector Sweep | 25–31 | 33 | **8** | **2** |

Identical play earns different grades. A slightly sloppy solution passes on a light roll and fails on a heavy one, and the player has no way to attribute the difference.

**Fix:** compute thresholds from *that run's* actual minimum rather than a constant. One change in `get_star_rating`, and it makes the randomisation fair instead of noisy.

## Defect 2 · Call count cannot detect the thing it's meant to reward

This is the serious one.

`DESIGN_DIRECTION.md` states the intent plainly:

> *"Early contracts completed with basic tools earn 1-2★. After learning better tools… the player returns and earns 3★ with cleaner code."*

That loop cannot happen, because **a loop and its unrolled equivalent make the identical number of calls.**

```python
# naive — 12 statements, 12 calls          # taught — 2 statements, 12 calls
rebalance()                                 while True:
rebalance()                                     rebalance()
rebalance()   ... ×12
```

Both score 12. On C1, 3★ is set at 13 — so **the naive first-attempt solution already earns 3★**, before the player has ever seen a loop. There is nothing to return for.

The same holds across the deterministic contracts:

| Contract | Optimal calls | 3★ at | Naive solution scores |
|---|---|---|---|
| C1 Power Node | 12 | 13 | **3★ on first attempt** |
| C2 Drone Route | 9 | 10 | **3★ on first attempt** |
| C3 Drone Dispatch | 16 | 17 | 3★ (unrolled if/else) |
| C4 Transit Signals | 13 | 14 | 3★ (unrolled) |

Call count measures *runtime efficiency*. The design wants to reward *source elegance*. These are different quantities, and on contracts whose whole purpose is teaching a control structure, they are uncorrelated.

### Why C8 doesn't have this problem

C8 randomises which sectors fail, so the player cannot hardcode and must iterate a list they receive at runtime. The GDD says this explicitly: *"the randomization makes hardcoding impossible — the player must use a for loop."*

**C8 solves by construction what C1–C4 try to solve by scoring.** That's the correct pattern.

## Proposed direction

Three changes, all requiring playtest validation before commitment.

**1. Relative thresholds.** Derive from the run's actual minimum:

```
3★  calls ≤ min + max(2, ceil(min × 0.15))
2★  calls ≤ min + max(5, ceil(min × 0.50))
1★  completion
```

Worked through:

| Contract | min | 3★ | 2★ |
|---|---|---|---|
| C1 | 12 | 14 | 18 |
| C2 | 9 | 11 | 14 |
| C3 | 16 | 19 | 24 |
| C4 | 13 | 15 | 20 |
| C8 | 9–13 | 11–15 | 14–20 |
| C10 | 26–30 | 30–35 | 39–45 |
| C11 | 25–31 | 29–36 | 38–47 |

This fixes Defect 1 outright — slack now scales with workload, so the roll no longer decides the grade.

**2. Score source structure, not just calls, on construct-teaching contracts.** A second metric — statements written — is what actually distinguishes 12 unrolled calls from a two-line loop. Both metrics are cheap to collect; the interpreter already walks the AST for feature gating.

**3. Prefer construction over scoring.** Where a contract exists to teach a control structure, make brute force *impossible* the way C8 does — randomise the workload — rather than trying to grade it afterwards. This is the better fix and should be preferred wherever a contract can absorb it.

C1 is the exception: it is pre-loop by design, so brute force *is* the intended solution. C1's 3★ arguably should not be call-based at all — or C1 should simply have no star rating, with the ratings beginning at C2.

## Test plan

The Numbers Policy requires a pass/fail metric, so:

**Target first-completion distribution:** ~60% 1★, ~30% 2★, ~10% 3★.
**Target replay distribution** (returning with the unlocked tool): ~70%+ achieve 3★.

- If first-completion 3★ exceeds 25%, thresholds are too loose — reduce the 0.15 coefficient in 0.03 steps.
- If replay 3★ falls below 50%, they're too tight — raise it.
- **If both are high on the same contract, the metric is wrong for that contract** — that's Defect 2 surfacing, and the answer is construction or a structure metric, not tuning.

Sample of at least 10 players per contract before moving any number.

---

# Part 2 · The Deck Store (B2)

`ONSITE_PIVOT.md` §3 established the deck as the credits sink. This is the taxonomy.

## The governing rule

> **Story-critical technologies are granted by the narrative. The store sells convenience, expression, and edge.**

If SQL costs credits, a player short of funds grinds old contracts for money, and the Motivation fix becomes the chore it was meant to replace. Nothing behind a price tag may ever be required to progress.

## Three tiers

### Granted — never purchasable

Arrives when the story demands it. Announced by Voss, unlocked by a contract.

`grep` · `chmod` · file operations · `for` loops · `def` · **SQL** · **Git** · the `query()` interface

These are the curriculum. They are free, always, and arriving at them is the reward.

### Purchasable — convenience and edge

Real value, never required. Each must answer: *what decision does this create?*

| Item | ~Cost | The decision it creates |
|---|---|---|
| **Diagnostic probe** | 400 | Reveals one bonus objective's *region* per contract. Spend for completionism, or save |
| **Extended scrollback** | 200 | Deeper terminal history. Matters on log-heavy contracts |
| **Second script slot** | 600 | Two files at once, before the functions milestone grants it |
| **Fast handshake** | 150 | Abbreviated plug-in on first visits too. Pure convenience |
| **Call profiler** | 500 | Shows per-line call attribution after a run. Directly serves 3★ hunting |
| **Persistent notes app** | 250 | A deck notepad that survives travel. Genuinely useful in the investigation arc |
| **Auto-arrange** | 150 | Window layout presets |

The **call profiler** is the strongest item here: it's bought with credits earned from stars, and it helps earn more stars. That's a virtuous loop rather than a grind.

### Cosmetic — expression only

Terminal themes · deck casings and wear states · boot sequences · prompt styles · font choices

Cheap, plentiful, zero mechanical effect. This is where surplus credits go late-game, and it's the sink that never distorts balance.

## Visibility

Locked items are **shown, named, priced and described** — HackHub's pattern. A named tool you can't yet afford creates wanting; a hidden one creates nothing.

This is the same split as the reference app (`DECK_APPS.md` A4): **tools visible when locked, language features hidden when locked.**

## Balance sanity check

Total credits available across the eleven prototype contracts, at full marks:

```
C1–C2   100 × 3 × 2  =   600
C3–C4   150 × 3 × 2  =   900
C5–C7   150 × 3 × 3  = 1,350
C8      200 × 3      =   600
C9      150 × 3      =   450
C10     250 × 3      =   750
C11     300 × 3      =   900
bonuses  50 × 7      =   350
                       ──────
                        5,900
```

Purchasable tier above totals 2,250 — roughly 38% of a perfect run. That leaves comfortable room for cosmetics and means a player at 2★ average (~3,900) can still afford everything mechanical that they want.

**Starting value.** Test: does any player report feeling they *must* replay to afford something? If yes, prices are too high or the item is too necessary — and the answer is to move it to Granted, not to cut the price.

## Where the store lives

A deck app, launched from the rail, browsable at any time including mid-contract. Purchases apply immediately. No confirmation friction on cosmetics; one confirmation on items over 400.

---

## Open

- **Does replaying pay the difference only, or full credits?** `DESIGN_DIRECTION.md` says the difference, which is correct and should stay — it kills grinding dead.
- **Should bonus objectives pay more than 50cr?** At 7 bonuses for 350 total they're currently worth less than a single contract. If they're meant to drive exploration, they're underpriced.
- **Is there anything to spend on after everything is bought?** Late-game surplus needs somewhere to go, or credits stop meaning anything in Act 3 — exactly when the stakes are highest.
