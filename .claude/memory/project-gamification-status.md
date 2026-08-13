---
name: project-gamification-status
description: "Gamification ported to Unity on branch unity/gamification — awaiting review; includes a rating rule change from the prototype"
metadata:
  type: project
---

## Gamification — ported, pushed, NOT merged (2026-08-13)

Branch **`unity/gamification`** carries the Unity port of the star/credit/rank layer
from `docs/DESIGN_DIRECTION.md`. It is pushed but deliberately unmerged: it changes
a design decision the prototype agent made, so it needs Peter's sign-off.

**Ported:** star ratings, credits, contractor rank with progress-to-next, per-contract
ratings on the board, a performance summary on the debrief, C5's hidden
`.bash_history` bonus (2★ complete / 3★ with bonus). All persisted with the save.
`Scoring.cs` mirrors `game_state.py`; thresholds and credit values come from the doc.

**Two deliberate departures from the prototype's rule** — both measured, not assumed:

1. **Count calls only up to the goal, not the whole run.** `while True` cannot stop
   itself and always burns the sandbox call cap. Scoring the whole run gave the loop
   21 calls against 12 for the same command typed out by hand — one star for the
   solution the debrief explicitly asks for, three for the naive one. C3 could not
   exceed one star at all.
2. **The third star requires the loop to have done the work** (at least half the
   calls to goal made inside a loop body). Fixing (1) alone made everything three
   stars, because call count cannot distinguish a loop from repetition — for C1 both
   make exactly twelve calls. The loop saves *code*, not calls.

   Half, not all, because C4 legitimately calls `submit_report()` after its loop ends.

Result matches the doc's stated replayability loop: Block 7 scores ◆◆◇ +200cr on the
first pass with repeated lines, then ◆◆◆ +100cr on the replay with a loop — which
also gives the existing C1 loop follow-up debrief a reward.

**Rating marks are ◆/◇, not ★/☆** — Cascadia Mono has neither star glyph. See the
font note in [[project-unity-demo-status]]; this trap has now bitten twice.

**Not yet done:** nobody has *played* the scoring — it has only been driven
programmatically. Worth checking whether ◆◆◇ on a first solve reads as encouraging
or as a rebuke. Credits also still have no sink, which the design doc acknowledges;
"what are credits for" is the biggest open design question.

Related: [[project-unity-demo-status]] [[project-branching-workflow]]
