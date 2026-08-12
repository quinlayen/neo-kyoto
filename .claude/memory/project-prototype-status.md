---
name: project-prototype-status
description: "Current prototype state: 8 contracts (4 Python, 3 terminal, 1 combined), ready for Unity game design phase"
metadata: 
  node_type: memory
  type: project
  originSessionId: 95c6bdc1-46af-4303-ba42-a2e49afc13aa
---

## Prototype Status (as of 2026-08-11)

**8 contracts implemented and working:**

| # | Title | Type | Location | Teaches |
|---|-------|------|----------|---------|
| C1 | Keep the Lights On | python | Block 7 | Commands, print() → while True |
| C2 | Drone Route Cleanup | python | Sector 12 | While True, variables → if/else |
| C3 | Drone Dispatch | python | Sector 14 | If/else in loops → controlled while |
| C4 | Signal Interference | python | Transit Hub | Controlled while, counters, args → Linux |
| C5 | System Recovery | terminal | Data Center | pwd, ls, cd, cat (navigation) |
| C6 | Log Analysis | terminal | Network Ops | grep, ls -la, chmod, hidden files |
| C7 | Server Migration | terminal | Server Farm | cp, mv, file reorganization |
| C8 | Grid Restoration | combined | Central Grid | for loops + lists (Python Phase 2) |

**Architecture:**
- `interpreter.py` — restricted Python sandbox with AST feature gates
- `systems/virtual_fs.py` — in-memory filesystem for terminal contracts
- `systems/terminal.py` — terminal interpreter (pwd, ls, cd, cat, grep, chmod, mkdir, touch, rm, cp, mv, ps, kill, head, tail, echo)
- `contracts/base.py` — Python contract base
- `contracts/base_terminal.py` — terminal contract base
- `contracts/base_combined.py` — combined Python+terminal base
- `main.py` — game loop with type-based dispatch (python/terminal/combined)

**Key design decisions:**
- Briefings are short (scene + commands + goal) — teaching happens in-game through filesystem files and error messages
- "Breaks old code" mechanics force learning new tools
- Randomized failures in C8 force for loops (can't hardcode)
- Terminal contracts use realistic home directories
- Hidden files and permissions are obstacles that test previous skills

**Archived for later:**
- Network monitoring (ps/kill contract) in `archive/contracts/`
- Elevator, assembly, warehouse contracts in `archive/`
- Pipes and redirection (planned but deferred — too complex for now)

**Next phase:** Unity game design — visuals, mechanics, deployment

**How to apply:** The prototype contracts define the teaching progression and mechanics. Unity implementation should preserve these patterns while adding the visual layer (god-view, jack-in, system animations).
