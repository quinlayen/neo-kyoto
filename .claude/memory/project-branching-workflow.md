---
name: project-branching-workflow
description: "Two agents work this repo from different machines — branch convention, who owns what, and how to avoid stepping on each other"
metadata:
  type: project
---

## Two agents, one repo (from 2026-08-13)

Peter runs a second Claude Code agent from another machine. Work is split by area,
which is why merges have been clean so far:

| Area | Owner | Paths |
|---|---|---|
| Python prototype, design docs | the other agent | `contracts/`, `systems/`, `main.py`, `game_state.py`, `docs/` |
| Unity port | this machine | `unity/`, `.claude/` |

**Always `git fetch` before starting.** Master has moved under us twice mid-session,
once mid-push. Both times a fast-forward, because the file split holds — but check
rather than assume.

**Branch convention:** `design/*` for prototype and design work, `unity/*` for the
Unity port, both merging to `master`. Do not commit straight to master; the other
agent has, and a non-fast-forward push was the result.

**The real risk is design divergence, not merge conflicts.** The gamification layer
merged cleanly as text while being pedagogically wrong for C1–C3 — the rating
punished the exact solution the debrief teaches. Files not overlapping says nothing
about whether the designs agree. When the other agent adds a mechanic, test it
against the teaching arc before porting it. See [[project-gamification-status]].

**Verify claims in their commit messages.** One commit said it added the game-design
skill; it contained no such files (the skill was already committed here).

Related: [[project-design-only-workflow]] [[project-gamification-status]]
