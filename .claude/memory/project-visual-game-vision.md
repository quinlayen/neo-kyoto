---
name: project-visual-game-vision
description: "End-game vision: god-view UI with visual feedback, jack-in coding interface, world responds to player's code in real-time"
metadata: 
  node_type: memory
  type: project
  originSessionId: 95c6bdc1-46af-4303-ba42-a2e49afc13aa
---

The current text-based prototype will evolve into a visual game with:

**Primary view**: Farmer-style god view (top-down/slight isometric) of district blocks. The world itself is the primary feedback channel — systems glow, flicker, and respond to the player's code.

**Secondary view**: "Jack in" to a dual-pane coding interface (editor + terminal + live data) when working on a contract. World stays visible as background/picture-in-picture.

**Visual language**:
- Unstable/broken → warm colors (orange, red, amber), flickering/stuttering motion
- Working/stable → cool colors (cyan, blue, soft green), smooth continuous flow
- When player's code runs, world reacts within 1–2 seconds (tight feedback loop)

**Example — Power Node (C1)**: Central junction with glowing conduits. Flickering state = buildings pulse unevenly, conduits show unstable orange/red energy. Each rebalance() call sends a visible surge through conduits. Stable state = steady cool-blue glow, even lighting.

**Example — Drones**: Drone paths shown as thin glowing lines. Broken = paths cross, drones pause, detours, warning orange. Fixed = clean paths, smooth movement, cyan/green.

**Design principles**:
- Color is the fastest signal
- Motion tells the story
- Avoid clutter — few readable systems per district
- Camera can pan/zoom freely
- Clicking a system focuses camera and opens relevant context

**Why:** The visual payoff of watching systems respond to code is the core pleasure. Gives clarity of a proper coding interface when needed, room for adventure elements later.

**How to apply:** System designs should have clear, distinct visual states (STUCK/SCRAMBLED/FIXED, FLICKERING/STABLE, etc.) that map to future visual representations. Keep state names readable and animation-friendly. [[reference-tfwr-progression]]
