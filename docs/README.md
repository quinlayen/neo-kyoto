# ONCALL: Systems Contractor — Design Docs

**Last updated**: 2026-08-14

---

## Read first

| Doc | What it covers |
|-----|----------------|
| **`GDD.md`** | The whole game. Identity, loop, progression, systems, narrative, technical. Start here |
| **`ONSITE_PIVOT.md`** | The on-site pivot: view model, the deck, the plug-in moment. **Supersedes GDD §5's god-view camera** |
| **`DESIGN_DIRECTION.md`** | Fun-first reframing, stars/credits/rank. ⚠ Its star table is superseded by `ECONOMY.md` |

## Environments & assets

| Doc | What it covers |
|-----|----------------|
| **`ENVIRONMENT_BRIEF.md`** | Exterior-first location plan, 11 contracts → 6 reusable sets, kit requirements, Cyberpunk Megapolis assessment. **Kit now purchased, imported and verified — see its Post-Purchase Verification section, which supersedes the pre-purchase reasoning above it** |
| **`ART_DIRECTION.md`** | Post-processing, lighting, visual fidelity. Partially superseded — read the 2026-08-14 note at the top |
| **`ART_BRIEF_SPLASH.md`** | Splash art. Its "city panorama" asset is now the overmap |

## Player-facing systems

| Doc | Backlog | What it covers |
|-----|---------|----------------|
| **`UI_REBUILD.md`** | — | **Running state of the UI rebuild.** What's built, what isn't, what's next, open questions. Start here for UI work |
| **`DECK_SPEC.md`** | A1 | Deck OS chrome, the three-band frame, window system, legibility over live 3D |
| **`DECK_APPS.md`** | A2–A6, D1–D5 | Terminal, editor, reference, objectives, toasts. Colour taxonomy, teaching content |
| **`TRAVELING.md`** | C1 | The journey to site. Diegetic loading, Voss's transmission en route |
| **`OVERMAP.md`** | C2, C3, B4 | Contract board, district state, the debrief sequence |
| **`DISPATCHER.md`** | B1 | Voss. Voice, tone arc, briefing rewrites |
| **`ECONOMY.md`** | B2, B3 | Star rating audit (two confirmed defects), deck store taxonomy |
| **`DESIGN_SYSTEMS.md`** | — | Onboarding, branching, error messages, reference system |

## Reference & history

| Doc | What it covers |
|-----|----------------|
| **`HANDOFF.md`** | Moving machine or Claude account. What's not in git and must be restored by hand |
| **`LINUX_TERMINAL_PLAN.md`** | Terminal command scope and implementation plan |
| **`PROJECT_SCOPE.md`** | ⚠ Historical. Describes the pre-Unity Python prototype |

---

## Current state

**Platform**: PC native. WebGL is a best-effort share build, never a design constraint.

**The pivot**: the player travels to a district, arrives on site, plugs a physical deck into the failing system, and works with the world live in front of them. There is no top-down god view of gameplay.

**Locations**: exterior-first. The contractor works from the *outside* of systems — kerbside junction boxes, access vaults, tower-base cabinets. Getting *inside* is the Act 3 escalation.

**Embodiment**: fixed-camera dioramas now, architected so a district can become first-person walkable later.

---

## Known open questions

Carried across the docs, in rough priority:

1. **Is the kit's geometry genuinely modular?** Grid unit unmeasured, and it's unclear whether the demo is modular pieces or pre-assembled hero streets. Decides how cheaply 11 contracts redress into 6 sets — `ENVIRONMENT_BRIEF.md`
   <br>*(Resolved: kit purchased, imported, verified. Scale, LODs, collision and emission all pass; facade texel density is the one weak spot and the fixed cameras hide it.)*
2. **Star metric may be wrong, not just mistuned** — `ECONOMY.md` Defect 2. Needs a playtest decision
3. **Is the overmap rendered on the deck, full-frame, with no windows?** Decide before the panorama is placed — it also settles `DECK_SPEC.md` §14's rail question. `OVERMAP.md` §Open
4. **Deck OS chrome is expensive to retrofit** — decide before Unity UI work begins. `DECK_SPEC.md` §14
5. **Rail on the left or the right?** — `DECK_SPEC.md` §14
6. **Late-game credit surplus has nowhere to go** — `ECONOMY.md`
7. **Player character's name**, needed for Voss's Act 3 ID-to-name shift — `DISPATCHER.md`
8. **Interior art spend** deferred to Act 3 — hold the line on the six-set consolidation
