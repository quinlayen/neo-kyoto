# ONCALL: Systems Contractor – Project Overview & Scope

> **Status note (2026-08-14): this document describes the original Python-prototype phase and is largely historical.**
>
> The prototype validated C1–C11 and its teaching mechanics. Development has since moved to Unity, and the game has pivoted to **on-site contracts** — the player travels to a district and works at the failing system rather than jacking in remotely.
>
> The "Out of Scope" list at the foot of this document is no longer accurate: Unity work, multiple contracts, graphics, and save/load are all now in scope.
>
> Current sources of truth: `GDD.md`, `ONSITE_PIVOT.md`, `ENVIRONMENT_BRIEF.md`, `DESIGN_DIRECTION.md`.
>
> **What remains valid and worth keeping:** the core fantasy, the language progression philosophy, and the early-game design principles below. Those have not changed.

## What This Project Is

ONCALL: Systems Contractor is a programming education game set in a cyberpunk world (Neo-Kyoto, 2189). The player takes the role of a freelance systems contractor who fixes and automates broken city infrastructure by writing real code.

The game teaches practical technical skills through gameplay:
- Python-style programming (custom restricted language)
- Linux-style terminal usage
- SQL for data inspection and manipulation
- Git concepts (versioning, commits, history)

It is **not** a hacking game. The fantasy is competent engineering and automation under pressure in a dense, neon-lit megacity.

The design is heavily inspired by *The Farmer Was Replaced*, especially its gradual introduction of programming concepts and the satisfaction of watching your code run and improve a live system.

## Core Fantasy

You are not a glamorous netrunner. You are the person who shows up when the drones stop delivering, the power grid starts oscillating, the logistics databases are a mess, or an entire system is stuck in a bad state. Your tools are a terminal, a code editor, SQL access, and version control. Competence is the only currency that matters.

## Current Scope (Prototype Phase)

We are building a **local Python prototype** (no Unity yet) to validate the core feel of the early game.

### What exists right now
- Terminal-based interface
- One contract: “Keep the Lights On” (Block 7 Power Node)
- Extremely restricted player language (only `rebalance()` at the start)
- Sequential function calls as the first programming experience
- Unlock of `while True` loops after the first contract is completed
- Basic teaching introduction that explains what a program is and how function calls work

### Design Principles for the Early Game
- Start extremely simple (closer to *The Farmer Was Replaced* than to typical coding tutorials)
- Introduce only one new concept at a time
- Let the player feel the limitation first, then give them the tool that removes it
- Prefer sequential statements → then loops → then variables/conditionals later
- Avoid nested conditionals and try/except in the early language

### Language Progression Philosophy
We are using a **custom restricted interpreter**, not full Python. This gives us complete control over what features exist and when they unlock. The language should feel like Python but only expose the concepts we want to teach at each stage.

## Longer-Term Vision (Not yet implemented)

- Multiple contracts with increasing complexity
- Persistent systems the player can leave running
- Introduction of variables, conditionals, functions, lists, and dictionaries
- Simple SQL queries against city/corporate data
- Terminal commands for investigation
- Light Git concepts (commit, history)
- A cyberpunk district that visibly improves as the player stabilizes systems
- Eventually move the whole experience into Unity with the same custom language

## Current Goal of the Prototype

Validate that the first 10–15 minutes feel good:
1. Player understands what a program is
2. Player successfully stabilizes the power node with repeated `rebalance()` calls
3. Player receives a clear unlock of loops
4. Player is encouraged to refactor the repetitive code into a `while True` loop
5. The teaching tone is clear and supportive without being condescending

## Technical Approach

- Pure Python, standard library only
- Restricted `exec()` environment for the first prototype (will later be replaced by a real custom interpreter)
- Simple simulated systems (currently just the power node)
- Player scripts live in real `.py` files so they can be edited in any editor

## Out of Scope for Now

- Unity / game engine work
- Full custom interpreter (lexer/parser/AST)
- Multiple systems or contracts
- Graphics or rich UI
- Saving/loading progress beyond the current session
- Multiplayer or online features

---

This document defines the project’s identity, current boundaries, and design direction. All implementation decisions in the prototype should stay aligned with the principles above.