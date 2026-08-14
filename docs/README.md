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
| **`ENVIRONMENT_BRIEF.md`** | Exterior-first location plan, 11 contracts → 6 reusable sets, kit requirements, Cyberpunk Megapolis assessment |
| **`ART_DIRECTION.md`** | Post-processing, lighting, visual fidelity. Partially superseded — read the 2026-08-14 note at the top |
| **`ART_BRIEF_SPLASH.md`** | Splash art. Its "city panorama" asset is now the overmap |

## Player-facing systems

| Doc | Backlog | What it covers |
|-----|---------|----------------|
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

1. **Asset kit not yet purchased.** Cyberpunk Megapolis assessed and recommended with caveats — `ENVIRONMENT_BRIEF.md`
2. **Star metric may be wrong, not just mistuned** — `ECONOMY.md` Defect 2. Needs a playtest decision
3. **Deck OS chrome is expensive to retrofit** — decide before Unity UI work begins. `DECK_SPEC.md` §14
4. **Rail on the left or the right?** — `DECK_SPEC.md` §14
5. **Late-game credit surplus has nowhere to go** — `ECONOMY.md`
6. **Player character's name**, needed for Voss's Act 3 ID-to-name shift — `DISPATCHER.md`
7. **Interior art spend** deferred to Act 3 — hold the line on the six-set consolidation
