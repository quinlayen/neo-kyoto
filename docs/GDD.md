# ONCALL: Systems Contractor — Game Design Document

**Status**: Draft
**Last Updated**: 2026-08-13

---

## 1. Game Identity

**Title**: ONCALL: Systems Contractor
**Setting**: Neo-Kyoto, 2189

**Genre**: Programming puzzle / automation game

**Elevator Pitch**: You are a freelance systems contractor in a crumbling cyberpunk megacity. When infrastructure breaks — power grids, drone networks, transit systems — you get the call. Your tools are a code editor and a terminal. Write real code, fix real systems, and watch the city respond. What starts as repair jobs becomes an investigation — someone is sabotaging Neo-Kyoto, and the only way to find them is to go deeper.

**Core Fantasy**: Competent engineering under pressure. You are not a hacker or a hero. You are the person who shows up when everything is broken and makes it work again. The satisfaction comes from understanding a system, writing code that fixes it, and watching the world respond in real time.

**Target Audience**: Aspiring programmers, CS students, and curious non-programmers who want to learn to code through gameplay rather than tutorials. Secondary audience: experienced developers who enjoy automation puzzles (Zachtronics fans, TFWR players).

**Platform**: PC (Steam) primary, Web (WebGL) secondary

**Inspirations**:
- *The Farmer Was Replaced* — gradual concept introduction, mechanics create the need to learn
- *Shenzhen I/O* — the satisfaction of engineering under constraints
- *Hacknet* — terminal-as-gameplay, cyberpunk atmosphere
- *Exapunks* — narrative wrapper around programming puzzles

---

## 2. Core Loop

### The Contract Cycle

```
Contract Board → Accept Contract → Briefing → Work (Code / Terminal) → System Responds → Completion → Unlock → Board
```

1. **Contract Board**: The player's home screen. Shows available, locked, and completed contracts. Early game is linear (each contract unlocks after the previous). Later, the board opens into a branching tree where the player chooses what to tackle (see Section 3: Progression).

2. **Briefing**: Short scene-setting text. Establishes what's broken and why. Lists available commands. States the goal. No tutorials — teaching happens in-game through discoverable files, error messages, and system behavior.

3. **Work Phase**: The player writes code (Python contracts) or types terminal commands (terminal contracts) or both (combined contracts). Scripts are written in an in-game editor and executed against the live system.

4. **System Response**: When the player runs their code, the world reacts visually within 1-2 seconds. Broken systems stabilize, drones reroute, power flows. This tight feedback loop is the core pleasure of the game.

5. **Completion**: When all objectives are met, the system locks in. A debrief explains what the player just learned and previews the next concept — specifically by showing why their current tools are insufficient for the next challenge.

6. **Unlock**: New language features, commands, or contract types become available. The player returns to the board with expanded capabilities.

### Design Philosophy: Feel the Limitation First

Every new concept is introduced the same way:

1. The player succeeds with their current tools
2. The next contract makes their current approach painful or impossible
3. The debrief names the limitation
4. The new tool is unlocked
5. The next contract is designed so the new tool solves the pain

The game never says "now learn X." It makes the player want X, then gives it to them.

---

## 3. Progression & Contracts

### Game Scope

Neo-Kyoto is designed to be a substantial game — significantly larger than the initial 8 prototype contracts. The final contract count is TBD, but the game should feel like a full experience with a satisfying narrative arc, not a short tutorial. Contracts will span multiple city districts, each with distinct visual identity and systems.

### Progression Structure: Linear → Branching

**Early game (Act 1)**: Strictly linear. Each contract unlocks after the previous one. The player is learning fundamentals — there's one right path and each concept builds on the last.

**Mid game (Act 2)**: The contract board opens into a **branching tree**. After the player has core competency (loops, conditionals, terminal basics), multiple contracts become available simultaneously. The player chooses what to tackle based on interest, difficulty, or narrative curiosity. If a contract is too difficult, they can set it aside and try a different one — building skills sideways before returning.

**Late game (Act 3)**: Multiple branches converge on high-level contracts that require mastery across technologies. Some contracts may have prerequisites from different branches (e.g., "requires any 2 of these 4 contracts completed").

```
Act 1 (Linear):     C1 → C2 → C3 → C4 → C5 → C6 → C7 → C8
                                                              │
Act 2 (Branching):                                    ┌───────┼───────┐
                                                      ▼       ▼       ▼
                                                   [Python] [Terminal] [Investigation]
                                                   advanced  advanced   contracts
                                                      │       │       │
                                                      ├───┬───┤       │
                                                      ▼   ▼   ▼       │
Act 3 (Convergence):                             [Combined contracts]◄─┘
                                                   requiring mastery
                                                   across all skills
```

### Skill Progression

| Phase | Contracts | Technology | Concepts Taught |
|-------|-----------|------------|-----------------|
| Python Phase 1 | C1-C4 | Python scripting | Sequential calls → while True → variables/if-else → controlled while/counters |
| Linux Terminal | C5-C7 | Terminal commands | pwd/ls/cd/cat → grep/chmod/hidden files → cp/mv/rm |
| Combined | C8+ | Python + Terminal | for loops/lists, combining investigation with automation |
| Functions | Mid-game | Python | `def`, parameters, return values — player starts writing their own tools |
| Investigation | Mid-game | SQL | SELECT, WHERE, JOIN — searching city databases for clues |
| Forensics | Mid-game | Git | git log, git show, git diff — tracing changes to find who tampered with systems |
| Mastery | Late-game | SQL + Python | Writing Python scripts that query databases, process results, and act on findings |
| Field work | Late-game | All skills | On-location contracts requiring physical presence and combined mastery |

### The Functions Milestone

Functions (`def`) represent a critical shift in the game's design. Before functions, the player calls pre-built commands we provide (`rebalance()`, `scan_drones()`, `repair()`). After functions, the player begins **writing their own commands**.

This is the transition from "use our tools" to "build your own tools":

- **Before functions**: The game provides domain-specific commands. The player's job is to orchestrate them with loops, conditionals, and variables.
- **After functions**: The game provides lower-level building blocks and raw data. The player writes their own functions to process, analyze, and act on that data.
- **Late game**: Contracts may provide almost nothing — just access to a system's raw state. The player writes the entire solution from scratch, including helper functions they reuse across contracts.

This mirrors real engineering growth: from following procedures, to writing procedures, to designing systems.

### Story-Driven Skill Introduction

New technologies are introduced when the narrative demands them, not on a fixed schedule:

- **SQL**: Introduced when the player discovers that someone is behind the failures (Act 2). The player needs to search through city databases — personnel records, access logs, transaction histories — to find hidden clues. SQL is the natural tool for asking questions of structured data. "Who accessed this system at 2:30am?" is a query, not a grep.
- **Git**: Introduced when the player discovers that critical system configs have been tampered with and someone tried to cover their tracks. The player needs version history to find what was changed and when. Git commands become forensic tools:
  - `git log` — scan commit history for suspicious changes. "Who touched this file at 3am?"
  - `git show <commit>` — inspect exactly what a specific commit changed
  - `git diff <commit1> <commit2>` — compare versions to find what was altered
  - Git also reinforces earlier skills: the player navigates to repos with terminal commands (`cd`, `ls`), reads files with `cat`, and uses `grep` to search through git output. Old skills stay relevant in new contexts.
- **SQL inside Python** (late-game): Advanced contracts introduce a simplified database interface that players call from within their Python scripts. No ORMs or SQLAlchemy — just a clean game-provided function like `query("SELECT name FROM personnel WHERE access_level > 5")` that returns results the player can process with Python loops, conditionals, and their own functions. This is the ultimate skill combination: terminal to investigate, SQL to query, Python to automate, all in one script.
- **Other skills**: The door is open. Any skill that serves the story can be introduced. The question is always: "Does the player need this to solve a problem they care about?"

### Contract Table (Prototype Phase)

These 8 contracts are implemented and validated in the prototype:

| # | Title | Location | Type | System | Teaches | Unlocks |
|---|-------|----------|------|--------|---------|---------|
| C1 | Keep the Lights On | Block 7 | Python | Power Node | Function calls, sequential programs, print() | `while True` loops |
| C2 | Drone Route Cleanup | Sector 12 | Python | Drone Router | While True loops, variables | `if/else` conditionals |
| C3 | Drone Dispatch | Sector 14 | Python | Drone Dispatch | If/else in loops, branching logic | Controlled while loops |
| C4 | Signal Interference | Transit Hub | Python | Transit Signals | Controlled while, counters, function arguments | Terminal access |
| C5 | System Recovery | Data Center | Terminal | Virtual Filesystem | pwd, ls, cd, cat (navigation) | grep, chmod, mkdir, touch, rm |
| C6 | Log Analysis | Network Ops | Terminal | Virtual Filesystem | grep, ls -la, chmod, hidden files | cp, mv |
| C7 | Server Migration | Server Farm | Terminal | Virtual Filesystem | cp, mv, file reorganization | Combined contracts |
| C8 | Grid Restoration | Central Grid | Combined | Power Grid | for loops, lists, terminal + scripting | range(), len() |

Many more contracts will be designed beyond these. The prototype validates the teaching mechanics and progression feel; the full game expands this foundation with more systems, more narrative, and branching paths.

### Feature Gates

The interpreter enforces feature gates at the AST level. Players cannot use language features they haven't unlocked:

- **Start**: Function calls, print(), variables
- **After C1**: `while True:` loops
- **After C2**: `if`, `elif`, `else`, comparison operators
- **After C8**: `for` loops, `range()`, `len()`
- **Mid-game**: `def` (functions), parameters, return values
- **Investigation arc**: SQL query interface unlocks; Git commands unlock
- **Late-game**: `query()` function available inside Python scripts for SQL-in-Python contracts
- **As needed**: Additional builtins, data structures, string methods

Attempting to use a locked feature produces a clear error: *"You haven't unlocked [feature] yet."*

### Multi-Technology Interleaving

Technologies are interleaved rather than front-loaded. The player learns Python basics, switches to terminal commands (a completely different feel), then returns to Python with richer motivation. For loops are more meaningful when you're iterating over file listings you discovered through the terminal. Functions are more meaningful when you're reusing logic across both contexts. SQL arrives when the player has a reason to ask questions of data, not as an arbitrary curriculum item.

### Demo Scope: C1-C5

The first Unity build covers:
- **C1-C4**: Complete Python Phase 1 — sequential calls through controlled while loops
- **C5**: First terminal contract — introduces a new mode of interaction
- This provides a natural arc: learn to code → hit the limits of scripting alone → jack into the terminal directly

---

## 4. Game Systems

### 4.1 Restricted Python Interpreter

A custom interpreter that executes player code in a sandboxed environment. It parses Python via AST and enforces:

- **Feature gates**: Language constructs are blocked until unlocked (while loops, conditionals, for loops, etc.)
- **Call limits**: Each contract defines a MAX_CALLS ceiling (12-40 depending on contract complexity) to prevent infinite loops and encourage efficient solutions
- **Timeout**: 5-second execution limit per script run
- **Restricted builtins**: Only `print()` initially; `range()` and `len()` unlock with for loops
- **Clear errors**: Syntax errors, gate violations, timeouts, and call limits all produce human-readable messages

The interpreter is not full Python — it's a teaching language that feels like Python. This gives complete control over what the player can do and when.

**Evolution over the game**: Early contracts provide high-level commands (`rebalance()`, `repair()`). After functions unlock, contracts increasingly provide lower-level primitives and raw data, expecting the player to write their own functions. Late-game contracts may provide only system access — the player builds the entire solution. The interpreter grows with the player.

### 4.2 Terminal Emulator

A simulated Linux terminal with a virtual in-memory filesystem. Supports:

**Commands**: pwd, ls (with -a, -l, -la flags), cd, cat, head, tail, grep, mkdir, touch, rm, cp, mv, chmod, echo, ps, kill

**Filesystem features**:
- Directory tree with realistic home directory structure
- File permissions (rwx for user/group/other)
- Hidden files (dot-prefixed, visible only with `ls -a`)
- Permission-locked files (require `chmod` before reading)
- Path resolution: absolute, relative, `~`, `.`, `..`

The filesystem is contract-specific — each terminal contract builds its own directory tree with planted files, hidden clues, and permission puzzles.

### 4.3 Combined Mode

Combined contracts give the player both a terminal and a script editor. The player investigates via terminal (reading docs, finding hidden files, understanding the system) and then writes a script to automate the fix. This mirrors real-world engineering: investigate first, then automate.

### 4.4 Per-Contract Systems

**Power Node (C1)**: A junction with load and stability readings. Each `rebalance()` call reduces load by 0.05. After 12+ calls, the node reaches STABLE. Visual: flickering conduits → steady glow.

**Drone Router (C2)**: Fleet of 8 drones, all MISROUTED. `scan_drones()` shows the fleet table. `reroute_next()` fixes one drone at a time. Visual: tangled drone paths → clean routes.

**Drone Dispatch (C3)**: 7 drones with two failure modes — MISROUTED and GROUNDED — requiring different fixes (`reroute()` vs `repair()`). `check_next()` reveals the current drone's state. Forces if/else branching. Visual: mixed-state drone fleet → all operational.

**Transit Signals (C4)**: 6 signals with STUCK or SCRAMBLED states. `check_signal(n)` inspects, `reset_signal(n)` fixes STUCK, `calibrate_signal(n)` fixes SCRAMBLED. `submit_report()` must be called after all signals are fixed — teaches that code after a loop executes once the loop ends. Visual: flickering/frozen signals → synchronized flow.

**Virtual Filesystem (C5-C7)**: Realistic directory trees with planted files. C5 is navigation (find a crash report). C6 is investigation (grep through 200-line logs, find hidden backups, fix permissions). C7 is operations (migrate files, clean up legacy directories).

**Power Grid (C8)**: 15 sectors with randomized failures (8-12 broken each run). `get_broken_sectors()` returns a list. The randomization makes hardcoding impossible — the player must use a for loop. Visual: city grid with sector-by-sector restoration.

---

## 5. Visual Design

### Core Principle: The World Is the Feedback

The most important visual design principle: **the player's code runs in the world, not in a console**. When the player hits RUN, they should watch the city respond — drones reroute, power flows through conduits, signals synchronize, sectors light up. The world is the primary feedback channel. Console output exists for debugging; the city exists for satisfaction.

This is the TFWR principle: in The Farmer Was Replaced, you don't read "harvested 5 wheat" in a log — you watch the drone fly across the field and harvest each tile. The code is abstract; the result is visual and immediate. Neo-Kyoto must deliver the same experience: you write `repair(sector)` and you *watch* that sector come online, lights flooding back, machines humming to life.

Every system in the game must have a live visual representation that the player can see responding to their code in real time.

### Art Direction

**Starting style**: Low-poly 3D with flat/cel shading. Clean geometry, strong silhouettes, neon accent lighting. The city should feel dense but readable.

**Upgrade path**: The art pipeline is designed for modular asset replacement. Materials, models, and effects can be upgraded to higher-fidelity assets as the project matures without changing gameplay or camera systems. The long-term goal is a richly detailed, immersive Neo-Kyoto that draws the player into the world.

### Camera

**Primary view (God View)**: Top-down with slight isometric angle. The player sees a district of the city — buildings, infrastructure, moving elements. This is the idle/overview state. The camera can pan freely and zoom in/out.

**Jack-In view**: When the player accepts a contract, a coding interface appears — but the world stays visible and active. The code editor overlays or docks to one side, while the live system remains the dominant visual element. The player writes code on one side and watches the world respond on the other. The world is never hidden behind a full-screen editor.

**Code execution camera**: When the player hits RUN, the camera can optionally track the action — following a drone as it reroutes, panning across the grid as sectors power up, zooming into a signal junction as it calibrates. This is not mandatory (the player can keep the camera static) but the option to "watch your code work" should feel cinematic.

### Live System Visualization

Each contract's system exists as a visible, animated scene in the game world. The player's code controls what happens in that scene.

**Power Node (C1)** — A central junction with branching conduits feeding into surrounding buildings:
- FLICKERING: Conduits spark and pulse unevenly. Buildings flicker — some bright, some dark. Orange/red energy arcs visibly along the conduits. The junction itself stutters.
- RUNNING CODE: Each `rebalance()` call sends a visible energy pulse through the conduits. The player watches the pulse travel outward, and the conduit it touches stabilizes briefly. With each call, more of the system calms down.
- STABLE: All conduits glow steady cyan/blue. Buildings illuminate evenly. The junction hums quietly. The transformation from chaos to order is visible and satisfying.

**Drone Network (C2-C3)** — A district with drone pads and delivery routes shown as glowing path lines:
- MISROUTED: Drones follow tangled, crossing paths. They pause at intersections, circle back, take detours. Path lines glow orange and overlap.
- RUNNING CODE: Each `reroute_next()` call picks up a drone and visibly redirects it — the old orange path fades, a new clean cyan path draws itself, and the drone follows it. The player watches each drone snap into place, one by one, as their loop runs.
- GROUNDED (C3): Drones sit dark on pads, no path line at all. `repair()` makes a drone power up (lights activate, rotors spin) and lift off onto its new path.
- OPERATIONAL: All drones moving smoothly on clean, non-crossing paths. Cyan/green trails. The district feels alive and efficient.

**Transit Signals (C4)** — An intersection or transit hub with signal poles and vehicle traffic:
- STUCK: Signal frozen on one color. Vehicles (trains, trams, or ground traffic) queue up at the intersection, backing up. Visible gridlock.
- SCRAMBLED: Signal cycles rapidly through random colors. Vehicles start-stop-start in confusion.
- RUNNING CODE: `check_signal(n)` highlights a signal — camera can focus on it. `calibrate_signal(n)` or `reset_signal(n)` triggers an animation: the signal snaps to correct timing, queued traffic releases and flows through. Each fix visibly clears a bottleneck.
- FIXED: All signals in synchronized rhythm. Traffic flows smoothly in all directions. The intersection feels like a well-oiled machine.

**Filesystem / Terminal (C5-C7)** — The player is inside a data center or server room:
- The terminal view is the primary interface, but the environment is a 3D server room, not just a text console. Racks of servers, blinking lights, cable runs.
- When the player `cat`s a file, relevant server indicators respond. When they `chmod` a file, a lock icon visually disengages on a rack.
- When the player finds the target file (C5), the connected system visually comes online — a monitor flickers to life, a server rack lights up green.
- The environment provides atmosphere and context. The player isn't typing into a void — they're physically jacked into a machine, and the room around them reflects what they're doing.

**Power Grid (C8)** — The city viewed from above, divided into a grid of sectors:
- OFFLINE: Sectors are dark. No lights, no movement.
- DEGRADED: Sectors flicker dimly.
- RUNNING CODE: The for loop is the star here. As the loop iterates through `get_broken_sectors()`, the player watches sectors light up one by one across the city. Each `repair(sector)` call triggers a power-up animation — a wave of light flooding outward from the sector's center, buildings illuminating, street lights coming on. The loop creates a cascade of restoration across the grid.
- ALL ONLINE: The entire city glows. The view of a fully powered Neo-Kyoto — every sector lit, every system humming — is the payoff for mastering the for loop.

### Visual Language

The city communicates system state through color and motion:

| State | Color Palette | Motion | Examples |
|-------|--------------|--------|----------|
| Broken / Unstable | Warm — orange, red, amber | Flickering, stuttering, irregular pulses | Power conduits sparking, drones circling aimlessly |
| Working / Stable | Cool — cyan, blue, soft green | Smooth, continuous, rhythmic flow | Steady power glow, drones on clean paths |
| Transitioning | Brief white flash or sweep | Quick ripple from source outward | When repair() runs, a pulse travels through the system |
| Code executing | Bright highlight on active element | Follows code flow — sequential, looping, or branching | Active drone highlighted during reroute, active sector during repair |

**Design principles**:
- The world is the feedback — console output confirms, the city celebrates
- Color is the fastest signal — the player should know a system's state at a glance
- Motion tells the story — smooth = good, erratic = bad
- Each function call should have a visible effect — the player sees their code *doing something*
- Code execution should feel physical — energy pulses, drones moving, lights activating
- Few readable systems per district — avoid visual clutter
- Clicking a system focuses the camera and opens relevant context

### The Immersion Goal

The player should feel like they are *in* Neo-Kyoto, not just playing a coding tutorial with a theme. The code editor is a tool they use; the city is where they live. When they fix a system, the neighborhood around it should feel different — quieter, brighter, more alive. When they walk into a new district (via the contract board), the broken state should be visually apparent before they even read the briefing.

Over time, as the art upgrades from low-poly to higher fidelity, this immersion deepens. But even in low-poly: a dark city sector that floods with light when the player's for loop runs is a powerful moment. The visual payoff of watching your code transform the world is the core of Neo-Kyoto's appeal.

---

## 6. UI/UX

### Contract Board

The player's hub. A terminal-styled interface showing available contracts:
- Contract name, location, status (AVAILABLE / LOCKED / COMPLETE)
- Selecting a contract shows its briefing
- Completed contracts show a star and can be replayed
- The board exists within the game world (a screen in the contractor's workspace, not a separate menu)

### Code Editor (Jack-In View)

The coding interface shares the screen with the live world. The world is always visible — the editor is a tool docked to one side, not a full-screen replacement.

```
┌────────────────────────────────────┬─────────────────────┐
│                                    │                     │
│                                    │    CODE EDITOR      │
│         LIVE WORLD VIEW            │    (player writes   │
│         (system responding         │     here)           │
│          to player's code)         │                     │
│                                    ├─────────────────────┤
│                                    │    OUTPUT / STATUS   │
│                                    │    (print output,   │
│                                    │     errors, system  │
│                                    │     readouts)       │
├────────────────────────────────────┼─────────────────────┤
│                                    │  [RUN]  [BRIEF]     │
└────────────────────────────────────┴─────────────────────┘
```

- The live world view takes the majority of screen space
- Syntax highlighting for the restricted Python subset
- Line numbers
- Clear error display with line references
- RUN button with visual feedback (button pulses, world reacts)
- The editor is in-game — no external file editing required in Unity
- When the player hits RUN, their eyes should naturally move to the world view to watch the result

### Terminal Interface

For terminal and combined contracts:

```
┌────────────────────────────────────────────────────┐
│  contractor@neo-kyoto:/home/contractor$            │
│  $ ls                                              │
│  Desktop  Documents  Downloads  notes.txt          │
│  $ cat notes.txt                                   │
│  GRID RESTORATION — URGENT                         │
│  ...                                               │
│  $                                                 │
│                                                    │
│                                                    │
│                                                    │
├────────────────────────────────────────────────────┤
│  contractor@neo-kyoto:/home/contractor$ _          │
└────────────────────────────────────────────────────┘
```

- Monospace font, dark background, green or cyan text
- Scrollback buffer
- Command history (up/down arrow)
- Tab completion (stretch goal)

### Combined Interface

For contracts like C8 that use both terminal and scripting:

```
┌──────────────────┬──────────────────┬──────────────┐
│                  │                  │              │
│   TERMINAL       │   CODE EDITOR    │  SYSTEM      │
│                  │                  │  STATUS      │
│                  │                  │              │
│                  │                  │              │
│                  │                  │              │
├──────────────────┴──────────────────┼──────────────┤
│  contractor@neo-kyoto:~$ _         │  [RUN]       │
└────────────────────────────────────┴──────────────┘
```

The player can switch focus between terminal and editor. Terminal output and editor are visible simultaneously.

### Briefings & Debriefs

- Briefings appear as in-world transmissions (styled terminal messages, not UI popups)
- Short: scene + commands + goal. 15-25 lines max
- Debriefs appear on completion — reflect on what was learned, preview next challenge
- Teaching happens through in-game discoverable files (repair_protocol.txt, cheat sheets), not briefing text

### In-Game Reference System

The player should never need to leave the game to remember how something works. An always-accessible reference panel provides documentation for every skill, command, and concept the player has unlocked.

**Access**: A persistent tab or hotkey (e.g., `F1` or a `[REF]` button in the editor toolbar) opens the reference panel. It slides in from the side or overlays without disrupting the workspace. The player can read it while their code is visible — no context switch.

**Structure**: The reference is organized by technology, with sections that unlock as the player progresses:

```
┌─ REFERENCE ──────────────────────────┐
│                                      │
│  ▼ Python                            │
│    ► Function Calls                  │
│    ► while True                      │
│    ► Variables                       │
│    ► if / elif / else                │
│    ► Controlled While Loops          │
│    ► for Loops & Lists               │
│    ► Functions (def)          [NEW]  │
│    ► range() & len()                 │
│                                      │
│  ▼ Terminal                          │
│    ► Navigation (pwd, ls, cd)        │
│    ► Reading Files (cat, head, tail) │
│    ► Searching (grep)                │
│    ► Permissions (chmod)             │
│    ► File Ops (cp, mv, rm, mkdir)    │
│                                      │
│  ▸ SQL                        [🔒]  │
│  ▸ Git                        [🔒]  │
│                                      │
└──────────────────────────────────────┘
```

**Each entry contains**:

1. **What it does** — one or two sentences, plain language
2. **Syntax** — the exact pattern, clearly formatted
3. **Example** — a short, concrete code snippet the player can reference while writing. Uses game-relevant examples, not abstract ones
4. **Common mistakes** — one or two pitfalls (e.g., "Don't forget the colon after `while True:`")

**Example entry — for loops**:
```
─── FOR LOOPS ───────────────────────

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

**Design rules**:

- **Only unlocked skills appear** — locked sections show as collapsed with a lock icon. No spoilers for what's coming. The player discovers new entries after completing the contract that unlocks them.
- **[NEW] badge** — recently unlocked entries are marked so the player knows to check them.
- **Game examples, not textbook examples** — every code snippet uses game commands and scenarios the player has seen. The for loop example uses `get_broken_sectors()` and `repair()`, not `fruits = ["apple", "banana"]`.
- **Searchable** (stretch goal) — a search bar at the top filters entries by keyword. The player types "loop" and sees both `while True` and `for` entries.
- **Terminal commands included** — terminal skills get the same treatment: syntax, flags, example usage. `ls -la` gets its own entry explaining what `-l` and `-a` do.
- **Cross-references** — entries can link to related concepts. The `for` loop entry links to `range()` and `lists`. The `grep` entry links to `chmod` (since finding hidden files often leads to needing permission changes).
- **Accessible from all views** — the reference works from the code editor, terminal, combined interface, and even the contract board. The player can check syntax anytime.

### Status Displays

Each contract has a persistent status panel showing:
- Overall status indicator: [!!] broken, [OK] working
- Per-objective checklist for multi-step contracts
- Numeric progress (e.g., "10/15 sectors online")
- Status updates in real time as the player's code runs

---

## 7. Narrative

### Setting

**Neo-Kyoto, 2189**. A sprawling megacity that runs on automated systems — power grids, drone logistics, transit networks, data infrastructure. For decades, these systems ran themselves. Now they're failing, one by one, and nobody knows why.

The city's corporate owners don't investigate. They outsource. When a system goes down, they post a contract. Freelance contractors like you pick it up, fix it, and get paid. You don't ask questions. You just make things work.

But as you take on more contracts, you start noticing patterns. The failures aren't random. Someone — or something — is systematically destabilizing Neo-Kyoto's infrastructure. The deeper you dig, the more you realize the contracts aren't just repair jobs. They're breadcrumbs.

### Player Character

You are a freelance systems contractor. No backstory, no name, no face. You are defined by competence. The city knows you by your contractor ID and your track record.

### Story Arc Structure

The narrative unfolds across three acts, delivered entirely through in-game content — no cutscenes, no dialogue trees.

**Act 1: The Repair Jobs** (C1-C5)
- Contracts feel routine. Systems are broken, you fix them. Briefings are matter-of-fact.
- Subtle clues in log files: timestamps that don't add up, error codes that reference systems the player hasn't seen yet, mentions of an "Architect Protocol" in buried logs.
- The player isn't looking for a story — they're learning to code. The clues are there for those who read carefully.

**Act 2: The Pattern** (mid-game, branching)
- Failures become more complex and interconnected. The breach investigation reveals unauthorized access. Server migrations uncover encrypted files. A grid cascade was triggered, not accidental.
- Briefings shift in tone — the dispatcher starts asking questions, not just issuing orders.
- The player begins to realize they're not just a contractor. They're an investigator.
- **SQL is introduced here**: The player discovers that someone is behind the failures and needs to search city databases — personnel records, access logs, contractor histories, system change records — to find out who and why. SQL is the natural tool: "Show me everyone who accessed the grid control system between 2am and 3am on the night of the cascade failure." This is investigation, not coursework.
- **Git is introduced here**: The player discovers tampered system configs — someone changed critical settings and tried to hide it. Git version history becomes a forensic tool. `git log` to find suspicious commits, `git show` to see exactly what was changed, `git diff` to compare the current state with what it was before. This reinforces earlier skills too: the player navigates to repos via the terminal, reads files with `cat`, and uses `grep` to search through commit messages and diffs.
- The contract board opens into branching paths. The player can pursue the investigation (narrative-driven contracts), take on advanced repair jobs (skill-building contracts), or explore new districts. Multiple contracts are available simultaneously.

**Act 3: The Architect** (late-game, convergence)
- The player discovers that Neo-Kyoto's original systems architect embedded a failsafe — a controlled degradation protocol designed to force human intervention when the city's AI governance began making dangerous autonomous decisions.
- The "failures" are intentional. The architect is testing whether humans can still understand and control the systems they built.
- Late-game contracts require mastery across technologies. The player writes their own functions, queries databases, navigates complex systems, and combines everything they've learned.
- **SQL inside Python** arrives here: the player writes scripts that query databases, loop through results, and act on findings. A single script might grep through logs, query a database for matching personnel, diff a git repo for related changes, and output a report. All the skills converge.
- **Field work** begins: some contracts require physical presence. The player travels to corporate headquarters, remote data centers, or infrastructure sites. They navigate the physical space, find a terminal, jack in, and work the problem on-premises. These locations tell their own stories through environmental details.
- The final contracts are open-ended: the game provides access to raw systems and the player builds the solution from scratch, using functions they've written, tools they've built, and knowledge they've gathered.
- The narrative converges on a choice about the city's future — resolved through the player's technical ability, not a dialogue option.

### Narrative Delivery

All story is delivered through channels the player is already using:

- **Log files**: System logs contain timestamps, error codes, and entries that hint at larger events
- **Hidden files**: Dot-files and permission-locked documents contain sensitive information
- **Database records**: SQL queries against city databases reveal connections — who accessed what system, when contracts were filed, which personnel were reassigned before failures
- **Briefing tone shifts**: The dispatcher's language changes from neutral to concerned to urgent
- **Terminal history**: `.bash_history` files show what previous contractors found (or didn't)
- **The city itself**: The visual state of districts tells a story. Early districts look run-down but recoverable. Later districts show deliberate sabotage — cut cables, overridden failsafes, systems that were broken on purpose. The player sees this before reading any briefing.

No exposition dumps. No NPCs explaining the plot. The player pieces the story together the same way they fix systems — by reading, investigating, querying, and connecting dots.

### Field Work: Physical Locations

As the investigation deepens, some contracts require the player to go somewhere. Not every system can be accessed remotely — some require physical presence. The player travels to a location, enters the building, and jacks in on-premises.

This adds a spatial dimension to the game and serves both narrative and mechanical purposes:

- **Corporate headquarters**: Access executive-level databases that aren't on the public network. The player physically enters the building, finds a terminal, and jacks in. The environment tells a story — empty offices, signs of hasty departure, systems left running.
- **Remote data centers**: Isolated facilities outside the city where backup systems and archives are stored. The player navigates the physical space (security doors, server rooms) before accessing the systems inside.
- **Infrastructure sites**: Substations, relay towers, tunnel junctions. The player is on-site, seeing the physical hardware their code controls. Jacking in here means watching the machinery respond right in front of them — not from a god-view, but from ground level.

Field work contracts break the pattern of the contract board. Instead of picking a job from a list, the player goes to a place. The location itself becomes part of the puzzle — finding the right terminal, getting past physical access controls (badge readers that need terminal commands, locked doors that need the right file permissions), and piecing together what happened here by reading what was left behind.

This is a later-game feature. Early contracts are all remote (jack in from the contractor terminal). Field work arrives when the investigation demands it — when remote access isn't enough and the player needs to be physically present to find what they're looking for.

---

## 8. Audio

### Ambient Soundscape

The city hums. Each district has a base ambient layer — distant traffic, ventilation systems, the electric buzz of neon. The ambient track reflects system state:

- **Systems broken**: Discordant hum, intermittent crackles, silence where there should be sound
- **Systems fixed**: Full, warm ambient drone. Machines running smoothly. The city breathing

### System Audio

Each system type has distinct audio feedback:

| System | Broken State | Fixed State | Repair Event |
|--------|-------------|-------------|--------------|
| Power Node | Arcing, electrical snap | Low steady hum | Power surge whoosh |
| Drones | Stuttering motor whine | Smooth propeller buzz | Reroute confirmation beep |
| Transit Signals | Alarm chirp, static | Rhythmic click sequence | Calibration tone sweep |
| Power Grid | Silence (dead sectors) | Building electrical hum | Sector-by-sector power-up cascade |

### Code Execution

- **Run button**: Satisfying mechanical "engage" sound
- **Successful execution**: Subtle confirmation tone, paired with visual system response
- **Error**: Soft error buzz — not punishing, just informative
- **Contract complete**: A distinct, rewarding "system online" chord. Rare enough to feel earned

### Music

Minimal and atmospheric. The player is concentrating on code — music should not compete for attention.

- Ambient electronic / synthwave undertones
- Reactive to game state: more presence when systems come online, more sparse when the player is working
- No lyrics, no strong melodies
- Volume lower than ambient by default

---

## 9. Technical Architecture

### Engine & Pipeline

- **Unity 2022 LTS** (or latest LTS at development start)
- **Universal Render Pipeline (URP)**: Required for WebGL compatibility. Supports the visual style (post-processing, bloom for neon, light cookies for atmospheric lighting) while maintaining web performance
- **Target frame rate**: 60fps on mid-range hardware, 30fps WebGL minimum

### Code Editor System

The in-game code editor is a critical component. Options:

**Recommended approach**: Custom TextMeshPro-based editor with:
- Monospace text rendering with line numbers
- Basic syntax highlighting (keywords, strings, numbers, comments)
- Cursor and text selection
- Copy/paste support
- Scrolling for longer scripts
- No autocomplete (the player should type and learn the commands)

This is simpler than integrating a full editor component and gives complete control over the restricted language's highlighting rules.

### Interpreter Integration

**Recommended approach**: Port the Python interpreter to C# rather than embedding Python.

The current `interpreter.py` uses Python's AST module to parse and execute player code. For Unity:
- Rewrite the restricted interpreter in C# with a simple recursive-descent parser
- The language is small enough (function calls, while, if/else, for, variables, print) that a custom parser is manageable
- Feature gates translate directly: the parser checks which constructs are allowed before execution
- Call limits and timeouts port cleanly to C# coroutines or frame-budgeted execution

**Why not embedded Python**: IronPython/CPython add complexity, bundle size (critical for WebGL), and expose full Python (defeating the purpose of the restricted language).

### Virtual Filesystem

Port `virtual_fs.py` to C# as an in-memory tree structure:
- `VirtualNode` class with children, permissions, content, metadata
- Path resolution logic (already well-defined in the prototype)
- Per-contract filesystem builder methods
- No actual disk I/O — everything lives in memory

### Terminal Emulator

Port `terminal.py` to C# with a TextMeshPro-based console:
- Command parsing and dispatch
- Output rendering with scrollback
- Prompt rendering with current directory
- Input handling with history (up/down arrow)

### Save System

- JSON-based save file tracking:
  - Completed contracts and branch progression
  - Unlocked features and technologies
  - Player scripts (last saved version per contract)
  - Player-defined functions (persistent across contracts once written)
  - Narrative flags (clues found, story beats triggered)
- Auto-save on contract completion
- Single save slot for simplicity (multiple saves are a stretch goal)
- WebGL: use `PlayerPrefs` or IndexedDB via JavaScript interop

### WebGL Considerations

- **Bundle size**: Keep under 30MB compressed. No embedded Python runtime. Minimal asset footprint with low-poly art.
- **Input**: Full keyboard support required (code editor). Mobile browser is not a target.
- **Performance**: URP with reduced post-processing. Simplified particle effects. Instanced rendering for repeated geometry (city buildings, grid sectors).
- **Persistence**: IndexedDB for save data. No filesystem access.
- **Audio**: WebGL audio context requires user interaction to start — handle gracefully on first click.

---

## 10. Demo Milestone (C1-C5)

### Scope

The first playable Unity build covers contracts C1 through C5: the complete Python Phase 1 plus the first terminal contract.

### Acceptance Criteria

**C1 — Keep the Lights On**:
- [ ] Player can write and run code in the in-game editor
- [ ] `rebalance()` command works, status display updates live
- [ ] Power node visual state transitions from FLICKERING to STABLE
- [ ] Completion unlocks `while True` with debrief message
- [ ] World visuals respond: conduits go from orange/flickering to blue/steady

**C2 — Drone Route Cleanup**:
- [ ] `scan_drones()` and `reroute_next()` commands work
- [ ] While True loops execute correctly in the interpreter
- [ ] Drone visual state transitions (tangled paths → clean routes)
- [ ] Completion unlocks `if/else`

**C3 — Drone Dispatch**:
- [ ] `check_next()`, `reroute()`, `repair()` commands work
- [ ] If/else branching works in the interpreter
- [ ] Wrong-fix errors display clearly
- [ ] Drone fleet visual with mixed failure states

**C4 — Signal Interference**:
- [ ] `check_signal(n)`, `reset_signal(n)`, `calibrate_signal(n)`, `submit_report()` commands work
- [ ] Function arguments work in the interpreter
- [ ] Controlled while loops (with condition variables) execute correctly
- [ ] Transit signal visual states (stuck/scrambled/fixed)
- [ ] Completion message signals transition to terminal

**C5 — System Recovery**:
- [ ] Terminal interface renders and accepts commands
- [ ] pwd, ls, cd, cat all work against the virtual filesystem
- [ ] Hidden files (`.bashrc`, `.bash_history`) only visible with `ls -a`
- [ ] Player can navigate to `/opt/neo-kyoto/services/power-grid/error.log`
- [ ] Cat-ing the crash report triggers completion

**Cross-cutting**:
- [ ] Contract board shows all 5 contracts with correct lock/unlock/complete states
- [ ] God-view camera with pan/zoom works
- [ ] Jack-in transition between world view and coding interface
- [ ] Feature gates enforce progression (can't use while True until C1 is done)
- [ ] Save/load persists progress between sessions
- [ ] Runs in both standalone PC build and WebGL

### Polish Targets (demo)

- Smooth camera transitions on jack-in/jack-out
- At least one district visually rendered with 2-3 systems visible
- Ambient audio for one district
- Code execution feedback sounds

### Stretch Goals (demo)

- Tab completion in terminal
- Animated briefing text (typewriter effect)
- Picture-in-picture world view during coding
- Multiple city districts visible (even if only one is interactive)

---

## Appendix A: Beyond the Demo

### Prototyped Contracts (C6-C8)

These are implemented in the prototype and ready to port:

| # | Title | Type | Teaches | Notes |
|---|-------|------|---------|-------|
| C6 | Log Analysis | Terminal | grep, chmod, hidden files | Breach investigation — Act 2 trigger |
| C7 | Server Migration | Terminal | cp, mv, file reorganization | Encryption key subplot |
| C8 | Grid Restoration | Combined | for loops, lists | Randomized failures, first combined contract |

### Planned Future Phases

| Phase | Technology | Narrative Purpose | Key Mechanic Shift |
|-------|-----------|-------------------|-------------------|
| Functions | Python `def` | Systems too complex for one-off scripts | Player writes reusable tools instead of calling ours |
| Investigation | SQL | Hunting the saboteur through city data | Querying structured databases for clues |
| Forensics | Git | Tracing tampered configs and hidden changes | `git log`, `git show`, `git diff` as detective tools |
| SQL + Python | Combined scripting | Automating large-scale investigation | Python scripts that query DBs, loop through results, build reports |
| Field work | All skills + locations | Physical access to restricted systems | Player travels to locations, jacks in on-premises |
| Advanced combined | All skills | Converging on the Architect mystery | Open-ended contracts, player-built solutions |
| Other | TBD | As narrative demands | Any skill that serves the story |

### Archived Contract Concepts

In `archive/`: assembly line automation, warehouse management, elevator control, network monitoring (ps/kill). These can be adapted for future phases or branching paths.

## Appendix B: Design Principles Summary

1. **Feel the limitation first** — never introduce a tool before the player needs it
2. **The world is the feedback** — code runs visually in the city, not just in a console
3. **Teach through mechanics, not text** — briefings set the scene; errors, files, and system behavior do the teaching
4. **Story serves progression** — new technologies arrive when the narrative demands them
5. **Linear to branching** — hold the player's hand early, then let go
6. **Player growth is real** — from calling our functions to writing their own
7. **Immersion over abstraction** — the player is in Neo-Kyoto, not in a classroom
