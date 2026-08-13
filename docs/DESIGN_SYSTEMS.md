# Design Document: Onboarding, Branching, Errors, Reference

## Context

With the gamification layer (stars/credits/rank) in place, we're now focusing on four design areas that will make the existing 11 contracts feel like a real game. This is a design-only document — no implementation yet. It captures decisions and specifications for future implementation.

---

## 1. ONBOARDING — Redesigning the First Five Minutes

### The Problem

C1's briefing is ~80 lines of text explaining what a program is before the player does anything. The game's own principle says "teach through mechanics, not text." TFWR drops you on a farm with `harvest()` available — no tutorial, instant feedback.

### The Design: Three Phases

**Phase 1 — Immediate Action (0:00–0:30):** Cold open. No briefing wall. The player sees a short emergency dispatch (3 lines) and an interactive prompt. They type `rebalance()` directly and see the system respond immediately. No file editing, no run command — just type and watch.

```
  ─── EMERGENCY DISPATCH ───
  Block 7 power node is failing.
  Type  rebalance()  to stabilize.

  BLOCK 7 POWER NODE
  Status:     [!!] FLICKERING
  Load:       0.97

contractor@block7> _
```

The player types `rebalance()`, sees the load drop, status update. Cause and effect in under 10 seconds. This is the hook.

**Phase 2 — Guided Escalation (0:30–2:00):** After 4 manual calls, a 2-line nudge: "The node needs about 12 rebalances. That is a lot of typing." After 8 calls: "4 more to go. There is a faster way — but first, finish the job." After 12: node stabilizes. Short debrief (5 lines, not 70).

**Phase 3 — Full Workflow (2:00–5:00):** Node resets. "This time, write a script." The player learns edit/save/run, but with critical differences: they already know what `rebalance()` does, they already know the system responds, and the workflow introduction is 10 lines, not 50. After script succeeds, the while loop unlock is introduced with exact code (2 lines), not a conceptual essay.

### Star Rating

Stars apply to Phase 3 (the script run), not Phase 1 (the REPL). Phase 1 is pure discovery. Existing `STAR_THRESHOLDS = (13, 16)` remain valid.

### Unity Translation

Phase 1: The power node is visually flickering. A small command input at the bottom of the screen (not the full editor). Type `rebalance()` and watch a conduit stabilize with a pulse of light. Phase 3: Full jack-in view opens — editor on the right, live power node on the left. Hit RUN and watch 12 energy pulses cascade through the system. The title screen IS the city: dark, flickering Neo-Kyoto, dispatch message as in-world transmission.

### Framework Connection

- **Clarity**: Player understands `rebalance()` through doing, not reading
- **Response**: Phase 1 REPL is the fastest possible feedback loop
- **Motivation**: The tedium of 12 manual calls creates intrinsic desire for scripts/loops
- **Satisfaction**: Two payoffs — gradual in Phase 1, cascading in Phase 3

---

## 2. LINEAR → BRANCHING

### The Problem

All 11 contracts are strictly sequential. A player stuck on C7 has zero options. A player excited about Python who is bored by terminal can't act on their interest.

### When Branching Begins

After C7. C1-C7 are linear (each concept builds on the previous). After C7, the player has two complete skill sets (Python scripting + terminal navigation) and is ready to combine them.

### The Branch Structure

```
Act 1 (Linear):
  C1 → C2 → C3 → C4 → C5 → C6 → C7
                                    │
Act 2 (Branching):          ┌───────┴───────┐
                            ▼               ▼
                     [Python Track]   [Terminal Track]
                       C8 Grid          C9 Process
                       Restoration      Lockdown
                       (for loops)      (ps/kill)
                            │
                            ▼
                       C10 Water      [Future: SQL]
                       Treatment      [Future: Git]
                       (unlocks def)
                            │
                            ▼
                       C11 Sector
                       Sweep
                       (first def)
```

### Prerequisite System

Replace the current "previous contract completed" check with explicit prerequisites per contract:

| Contract | Prerequisites |
|----------|--------------|
| C1-C7 | Each requires the previous (unchanged) |
| C8 | Requires C7 (Python track) |
| C9 | Requires C7 (Terminal track) |
| C10 | Requires C8 |
| C11 | Requires C10 |
| Future SQL | Requires C9 |
| Future Git | Requires C9 |
| Convergence | Requires contracts from multiple tracks |

After completing C7, the player sees both C8 and C9 as [AVAILABLE] simultaneously. C9 is independent — the player can tackle it before, after, or never relative to C8.

### Contract Board Display

```
  ─── ACT 1 ───
  [ 1]  Keep the Lights On — Block 7         ★★★ 300cr
  ...
  [ 7]  Server Migration — Server Farm       ★★★ 450cr

  ─── PYTHON TRACK ───
  [ 8]  Grid Restoration — Central Grid      [AVAILABLE]
  [10]  Water Treatment — Underground Plant  [LOCKED] Requires: C8
  [11]  Sector Sweep — Industrial Zone       [LOCKED] Requires: C10

  ─── TERMINAL TRACK ───
  [ 9]  Process Lockdown — Comms Tower       [AVAILABLE]
```

### Stuck Players

Branching provides natural relief: switch tracks, replay earlier contracts for better stars, or use the reference system. After 3+ failed runs, progressive hints could offer increasingly specific guidance.

### How SQL/Git Plug In

SQL contracts branch off the terminal track (C9 seeds the investigation). Git contracts also branch from C9. Convergence contracts require mastery across tracks. The prerequisite system handles all of this without code changes — just data.

---

## 3. ERROR EXPERIENCE

### The Problem

Current errors are functional but not teaching tools. "Syntax error on line 3: invalid syntax. Check your code for typos." doesn't help a beginner who wrote `if status = "STUCK"` instead of `==`.

### Design Principle

Errors ARE the primary teaching mechanism. Each error should answer three questions: what went wrong, why, and what to try next. Errors should feel like a mentor nudging, not a compiler rejecting.

### Specific Error Improvements

**Missing parentheses** — Player types `rebalance` (no `()`):
```
  Almost! rebalance is a command — add ()
  to call it:
      rebalance()
```
Detection: AST pre-scan for bare Name nodes matching known commands.

**Assignment vs. comparison** — `if status = "STUCK"`:
```
  Line 3: if status = "STUCK":

  A single = means "store this value."
  A double == means "is this equal to?"

  In an if statement, you want to compare:
      if status == "STUCK":
```
Detection: SyntaxError on a line containing `if` + single `=` (not `==`).

**Near-miss command names** — `reblance()`:
```
  Did you mean rebalance()?
```
Detection: Levenshtein distance ≤ 2 against active commands.

**Indentation errors** — Missing indent after `while True:`:
```
  Line 3 needs to be indented.

  Lines after while, if, for, or def must
  start with spaces (4 spaces or a tab):
      while True:
          rebalance()    ← indented
```
Detection: IndentationError subclass check with context from previous line.

**Python in terminal** — typing `rebalance()` at a terminal prompt:
```
  That is a Python command. You are in a
  terminal. Try terminal commands like ls, cd, cat.
```

**Terminal in Python** — typing `ls` in a Python script:
```
  ls is a terminal command. You are writing
  a Python script. Your available commands:
  rebalance, scan_grid, repair
```

### Progressive Hints

Track error history per session. Same-type errors escalate:
- **1st**: Standard message (as above)
- **2nd**: "This is the same issue as last time. Take a close look at line N."
- **3rd**: More specific hint with corrected code shown
- **4th+**: "Type 'ref' to see the reference for [topic]."

### Tone Guidelines

- Never lead with "Error." Use "Almost," "Not quite," "Careful."
- Avoid jargon until the player has learned it (no "expression" or "statement" in C1)
- Always end with a concrete action
- Show the player's actual code alongside the corrected version
- Warm amber in Unity, not harsh red — errors are diagnostics, not punishments

### Complete Error Matrix

| Category | Detection Method | Message Pattern |
|----------|-----------------|-----------------|
| Missing parens | AST: bare Name matching command | "Add () to call it" |
| Name typo | Levenshtein ≤ 2 on NameError | "Did you mean X()?" |
| Unknown name | NameError, no near match | "Not available. Commands: X, Y" |
| `=` vs `==` in if | SyntaxError + `if` + single `=` | "= stores, == compares" |
| Missing colon | SyntaxError after while/if/for/def | "Add : at the end" |
| Missing indent | IndentationError: expected block | "Indent after while/if/for/def" |
| Extra indent | IndentationError: unexpected | "Remove extra spaces" |
| Feature gate | AST node type check | "Not unlocked yet" |
| Call limit | _call_count > max | "Sandbox stopped" |
| Timeout | SIGALRM | "Ran too long, check loops" |
| Python in terminal | Known Python syntax at terminal | "Try terminal commands" |
| Terminal in Python | Terminal cmd as NameError | "Try Python commands" |

---

## 4. CODE/SKILL REFERENCE

### The Problem

Players who forget syntax must re-read long briefings or leave the game to Google. Neither option is fun. The GDD (Section 6) already describes a reference panel — this section specifies it concretely.

### Access

`ref` command available from all views (contract board, Python contracts, terminal, combined). Always one command away.

- `ref` — show table of contents
- `ref while` — show the while loop entry
- `ref grep` — show the grep entry
- `ref all` — show everything unlocked

### Table of Contents Display

```
─── REFERENCE ──────────────────────────

  PYTHON
    Function Calls
    print()
    Variables
    while True Loops
    if / elif / else
    Comparison Operators
    Controlled While Loops
    for Loops                       [NEW]
    Lists                           [NEW]
    range() and len()               [NEW]
    Functions (def)                 [NEW]

  TERMINAL
    pwd, ls, cd, cat
    grep
    chmod
    mkdir, touch, rm
    cp and mv
    ps and kill

  SQL                               [LOCKED]
  GIT                               [LOCKED]

  Type  ref <topic>  to read an entry.
```

### Entry Format (four-part template)

Every entry follows: **What** / **Syntax** / **Example** / **Watch Out**.
All examples use game-specific code (not textbook examples).

**Example — for Loops:**
```
─── FOR LOOPS ──────────────────────────
Repeats code once for each item in a list.

  SYNTAX:
    for item in my_list:
        <do something with item>

  EXAMPLE:
    broken = get_broken_sectors()
    for sector in broken:
        repair(sector)

  ALSO WORKS WITH range():
    for i in range(10):
        print(i)        # prints 0 through 9

  WATCH OUT:
    • The indented block runs once per item
    • Code AFTER the loop runs once, when
      the loop is done
```

**Example — grep:**
```
─── GREP ───────────────────────────────
Searches inside a file for lines that
contain a word or pattern.

  SYNTAX:
    grep <pattern> <file>

  EXAMPLE:
    grep BREACH /var/log/access.log
    grep ERROR /var/log/system.log

  WATCH OUT:
    • Case-sensitive: grep breach won't
      find BREACH
    • Searches contents, not file names.
      Use ls to find files.
```

**Example — Functions (def):**
```
─── FUNCTIONS (def) ────────────────────
Create your own reusable command.

  SYNTAX:
    def my_function(parameter):
        <steps>
    my_function(value)

  EXAMPLE:
    def repair_unit(unit_id):
        drain(unit_id)
        flush(unit_id)
        refill(unit_id)
        restart(unit_id)

    for s in get_broken_stations():
        repair_unit(s)

  WATCH OUT:
    • def goes at the TOP of your script
    • The colon : is required
    • Indent the function body
```

### Unlock System

- **Python entries**: gated by `game_state.is_unlocked(feature)` — e.g., `while` entry appears when `"loops"` is unlocked
- **Terminal entries**: gated by `game_state.is_contract_completed(contract_id)` — e.g., `grep` appears after C6
- **[NEW] markers**: entries unlocked since the player last viewed `ref` are marked
- **Locked sections**: SQL and Git show as `[LOCKED]` until their first contract is completed

### Extension for SQL/Git

Same template. SQL entries use game investigation examples:
```
─── SELECT ─────────────────────────────
Retrieves data from a database table.

  EXAMPLE:
    query("SELECT name FROM personnel
           WHERE access_level > 5")
```

Git entries use forensic examples:
```
─── GIT LOG ────────────────────────────
Shows the history of changes to files.

  EXAMPLE:
    git log config.yaml
    git log --oneline
```

---

## How the Four Areas Interact

These systems form a reinforcing loop:

1. **Onboarding** gets the player doing something in 10 seconds
2. When they make mistakes, **Errors** teach them
3. When they forget syntax, **Reference** reminds them
4. When they're stuck, **Branching** gives them options
5. Errors point to reference entries; reference prevents errors; branching prevents frustration spirals

### Recommended Build Order

1. **Error Experience** — highest impact, smallest scope. Improves every contract immediately.
2. **Reference System** — new module, small integration. Independent of other changes.
3. **Onboarding** — restructures C1 flow. High impact on first impression.
4. **Branching** — largest scope, most impactful when more contracts exist.
