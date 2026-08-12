---
name: reference-tfwr-progression
description: "The Farmer Was Replaced progression research — unlock order, design patterns, and how it compares to Neo-Kyoto's approach"
metadata: 
  node_type: memory
  type: reference
  originSessionId: 95c6bdc1-46af-4303-ba42-a2e49afc13aa
---

## The Farmer Was Replaced — Skill Progression

**Stage 1: Sequential calls** — Start with `harvest()` and `do_a_flip()` on a 1x1 farm. Hay is the first currency. Teaching: what a function call is, `()` syntax.

**Stage 2: While loops** (5 hay) — Unlocks `while True:` and booleans. Motivation: typing harvest() repeatedly is tedious. Teaching: repetition, indentation, infinite loops (safe due to built-in delay).

**Stage 3: Speed** (20 hay) — Drone gets faster but now harvests before grass regrows. This *breaks* the naive `while True: harvest()` approach, motivating conditionals.

**Stage 4: If/else + can_harvest()** — Speed upgrade creates empty harvests. Unlocks `if`, `else`, and `can_harvest()` together. Teaching: conditionals, return values as booleans, branching.

**Stage 5: Plant** (50 hay) — Unlocks `plant()` with entity arguments. Bushes are the first plantable crop. Teaching: function arguments, entity types.

**Stage 6: Expand** (30 hay, tiered) — Farm grows beyond 1x1. Unlocks `move(North/East/etc)`. Teaching: function arguments with constants, spatial thinking.

**Stage 7: Senses** (100 hay) — Unlocks `get_pos_x()`, `get_pos_y()`, `get_entity_type()`, `get_ground_type()`, `num_items()`. Introduces `None`. Teaching: reading world data, combining conditionals with sensors.

**Stage 8: Operators** (150 hay + 10 wood) — Arithmetic, comparison, and logic operators. Teaching: expressions, combining conditions.

**Stage 9: For loops + range()** — Unlocked when farm expands to a square grid. `while` can't easily traverse 2D — `for` solves this. `range(n)` and `get_world_size()` introduced together. Teaching: fixed-count repetition, nested loops.

**Stage 10: Carrots** (50 wood) — First crop requiring `till()` (ground prep) before planting. Costs resources. Teaching: multi-step processes.

**Stage 11: Variables** (35 carrot) — Named containers, `=` vs `==`, `+=` shorthand. Teaching: storing and reusing data.

**Stage 12: Functions (def)** (40 carrot) — Define reusable code blocks. Teaching: modularity, abstraction.

**Stage 13+: Branching unlocks** — Trees/Watering/Sunflowers, Lists, Pumpkins, Import (multi-file), Dictionaries, Cactus, Mazes, Polyculture, Dinosaurs, Megafarm (multiple drones), Leaderboard.

## Key Design Patterns

1. **Mechanics create the need, not the tutorial.** Speed doubling *breaks* naive code, forcing the player to want if/else. Farm expanding *breaks* single-tile loops, forcing for. The game never says "now learn this" — it makes the old approach fail.

2. **Extremely gradual.** if/else comes bundled with its own sensor function (can_harvest()), so the player learns one thing at a time. Variables don't unlock until stage 11 — much later than expected.

3. **Currencies gate progression naturally.** Can't unlock the next feature until you've farmed enough with current tools, meaning you've practiced them thoroughly.

4. **Continuous, not level-based.** No discrete "contract complete" moments — the farm is always running and you incrementally upgrade it.

## Comparison to Neo-Kyoto

Neo-Kyoto's current order: calls → loops → variables → conditionals → for loops → functions.
TFWR's order: calls → loops → conditionals → movement/args → sensors → operators → for loops → variables → functions.

Key differences:
- Neo-Kyoto introduces variables early (after contract 2). This is a deliberate choice — it lets the player work with data sooner.
- TFWR delays variables until stage 11 because sensor functions and crop mechanics provide enough to work with.
- TFWR uses a single continuous system (the farm) while Neo-Kyoto uses discrete contracts with different themed systems.
- Both share the "feel the limitation first" philosophy.
