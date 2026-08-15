---
name: project-branching-workflow
description: "Single machine as of 2026-08-14 — the two-agent split is over; branch convention and the one lesson worth keeping"
metadata:
  type: project
---

## Single machine (from 2026-08-14)

**The two-agent setup is over.** Peter got rid of the other computer, so the second Claude Code
agent that owned `contracts/`, `systems/`, `main.py` and `docs/` is gone. There is no longer a
path-ownership split, and no other writer to race with.

What this changes:

- **`docs/` is now freely editable from here.** It used to belong to the other agent.
- The "always `git fetch` before starting" rule is no longer load-bearing, though it costs nothing.
- Non-fast-forward pushes from a second writer are no longer a risk.

**Branch convention still applies** — `design/*` for prototype and design work, `unity/*` for the
Unity port, merging to `master`. Worth keeping for reviewable units of work, not for contention.

### The one lesson worth carrying forward

**Files not overlapping says nothing about whether the designs agree.** The gamification layer
merged cleanly as text while being pedagogically wrong for C1–C3 — the star rating punished the
exact solution the debrief teaches. When a mechanic lands, test it against the teaching arc rather
than against the diff. See [[project-gamification-status]].

Related: [[project-design-only-workflow]] [[project-gamification-status]]
