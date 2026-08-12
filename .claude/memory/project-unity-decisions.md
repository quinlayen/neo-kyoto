---
name: project-unity-decisions
description: "Key Unity decisions: PC+Web, low-poly 3D (upgradeable), story arc, branching progression, world-as-feedback, player-defined functions"
metadata: 
  node_type: memory
  type: project
  originSessionId: 998148da-ad78-490b-8553-b723091270f6
---

## Unity Build Decisions (2026-08-11)

- **Platform**: PC (Steam) primary + Web (WebGL) secondary
- **Art style**: Low-poly 3D to start, with planned upgrade path to higher-fidelity graphics for greater immersion. Art pipeline should be modular for asset replacement.
- **Narrative**: Full story arc — mystery of why Neo-Kyoto's systems are failing. The "Architect Protocol" as the throughline.
- **Demo scope**: C1-C5 (Python Phase 1 + first Linux terminal contract)
- **Interpreter approach**: C# port (not embedded Python) for WebGL compatibility and control
- **Rendering**: URP for WebGL compat
- **Game scope**: Significantly larger than the 8 prototype contracts. Full-length game, not a tutorial.
- **Progression**: Linear early game → branching tree mid-game → convergence late game. If a contract is too hard, player can try others.
- **Functions milestone**: After `def` unlocks, players write their own functions instead of relying on our pre-built commands. This is the key growth arc: using tools → building tools.
- **SQL introduction**: Tied to narrative — arrives when the player discovers someone is behind the failures and needs to search databases for clues.
- **Git**: Confirmed. Player uses git log/show/diff as forensic tools to find who tampered with configs. Reinforces terminal skills (navigating to repos, grep on git output).
- **SQL inside Python**: Late-game contracts where players write Python scripts that call a simplified query() function to search databases. Skills combine: terminal + SQL + Python in one script.
- **Field work**: Later-game contracts where the player physically travels to locations (corporate HQ, data centers, infrastructure sites) and jacks in on-premises. Adds spatial/exploration dimension.
- **Visual immersion**: The world is the primary feedback channel, not the console. TFWR-style: code runs visibly in the city. Editor is docked to one side; the live world is always visible and dominant.

**Why:** These decisions expand the GDD from a demo plan to a full game vision. The world-as-feedback principle and the functions milestone are the two most important design pillars.

**How to apply:** All Unity implementation work should align with these decisions. The GDD is at `docs/GDD.md`.

Related: [[project-visual-game-vision]] [[project-prototype-status]]
