# The Overmap & The Debrief

**Date**: 2026-08-14
**Status**: Current thinking
**Backlog items**: C2 (overmap), C3 (debrief sequencing), B4 (district state)
**Depends on**: `ONSITE_PIVOT.md` (view model), `TRAVELING.md`, `ECONOMY.md`, `DISPATCHER.md`
**Blocked on**: a district model in code — see *What has to exist first*. And one decision:
whether the map lives on the deck (bottom of this file)

---

# Part 1 · The Overmap (C2)

## What it is

The overmap replaces three things the GDD previously kept separate: the contract board, the travel layer, and the progression display. It is the game's only hub.

**It is a stylised map, not a playable city.** Neo-Kyoto is never explorable at this scale — districts are isolated scenes (`GDD.md` §9), and the map is the connective tissue between them. This is a deliberate scope refusal: an explorable megacity is not the game.

## Visual treatment

> ### ⚠ Superseded 2026-08-14 — the map is the live city, not a painting
>
> The panorama below is **not** what is being built. The overmap is a camera flying over the
> **real, live city scene**, and selecting a district flies down into it — that same corner then
> becomes the diorama the contract is worked in front of. Measured and shot before committing;
> see *The city, measured* and *District anchors* below.
>
> What this buys, beyond looking better than a painting: **district state stops being an overlay
> and becomes the actual lights.** A repaired district is repaired in the world, watched from
> above, and it stays that way because the state is derived from saved progress.

An elevated night panorama of Neo-Kyoto, rendered once, with districts picked out as interactive regions. `ART_BRIEF_SPLASH.md` §Asset 2 already briefs almost exactly this image — it was scoped as a splash background before the pivot, and it now has a functional home.

Districts read at a glance through the game's existing colour language: warm and unstable, or cool and steady.

## The city, measured

`Assets/Scenes/NeoKyotoCity.unity` — **our copy** of the kit's `CP_Demo`, so the vendor scene is
never edited. Numbers taken from the scene, not estimated:

| | |
|---|---|
| Renderers | 3,947, of which **3,695 sit under 2,754 LODGroups** (94% LOD-managed) |
| Unique materials | 77 — SRP-batcher friendly |
| Triangles | 4.5 M across all LOD levels; ~790 k at LOD0 in the core |
| Dense core | **90% of renderers within 250 m of origin** |
| Beyond that | sparse but real — 82 renderers carrying 1.25 M tris in the 2–3 km ring. Detailed hero towers with no street grid between them. This is the skyline, and the space to build into |
| Lights | **69, all realtime, zero baked lightmaps** |
| Underground | `Metro` sits at y −8 to +6 — a complete subway station: platforms, escalators, boards, maps |

**Zero baked lighting cuts the right way twice.** District state means changing lights at runtime,
which baked lighting cannot do; and there is no bake step per district variant.

⚠ **The kit's atmosphere is authored for ~100–200 m of street-level visibility.** At district
altitude it is beautiful. Above ~300 m it turns the city to grey soup. The overmap camera needs a
volume override — not a scene change, but not free either.

⚠ **The shadow atlas is already saturated at street level.** Running the splash logs a continuous
stream of URP warnings: **175–256 shadow maps competing for a 2048×2048 atlas**, resolution cut by
8×, and at peak *"URP removed 4 shadow maps"* outright. That is the 69 realtime lights — point
lights cost six maps each. It will get worse at overmap altitude, where the whole core is in
frustum at once.

This is not a blocker but it is on the critical path, because **lights are the state language** —
if the atlas is thrashing, the amber→cyan transition is competing for the same budget. Three levers,
cheapest first: turn shadow casting *off* on the decorative neon and window lights (most of the 69
are not shadow-relevant), raise the atlas size in the URP asset, or convert point lights to spots.
Do the first before touching the other two.

## District anchors

A district is a **world-space anchor plus a camera framing**, not a marker on a painting
(`DistrictRegistry.cs`). `DistrictRegistry.CameraFor` orbits the framing around the anchor using the
same maths as the editor's scene view, so a shot found by flying around and reading off
pitch/yaw/distance transfers exactly — verified by driving the scene view from the model and
matching the reference screenshot pixel for pixel.

| District | Anchor | Reads as | Evidence |
|---|---|---|---|
| **Block 7** | (−75, 0, −75) | Residential — dense mid-rise, rooftop five-a-side pitch, 1,264 renderers, tallest 133 m | shot |
| **Sector 12** | (75, 0, 75) | Utility/plant — low-rise, rooftop machinery, clustered tanks, cable runs, tallest only 64 m | shot |
| **Sector 14** | (140, 0, 20) | Shares Sector 12's quadrant *by design* — adjacent sectors, same drone fleet (C2 and C3 are both drone work) | ⚠ **framing unverified** |
| **Transit Hub** | (75, 0, −75) | Transit — the monorail S-curve dominates, twin dark towers behind, tallest 112 m | shot |
| **Data Center** | (−75, 0, 75) | Corporate — neon canyon, CITYNET / ARC HORIZON / NEO HORIZON DISTRICT, tallest 264 m | shot |

The four shot framings answer the Clarity/Fit risk: **four distinct places out of one 300 m core,
with no redress.** The silhouettes alone carry it — 64 m against 264 m registers before anything
else does. Sector 14 is the open one.

## How it is wired

| Component | Owns |
|---|---|
| `CityView` | The city scene. **Reference-counted**, because both the title and the overmap want it — a single owner unloads it on the way from one to the other and reloads it half a second later. Also borrows and restores the camera, swaps the active scene for lighting, and hides the placeholder world |
| `SplashCityView` | Just the title drift now. A holder, not an owner |
| `OvermapView` | Overview framing, `FlyToDistrict`, and the atmosphere override |

Three integration bugs came out of building it, all of the same shape — **two things owning one
resource** — and all three are worth remembering rather than rediscovering:

1. **Release-then-acquire tore the city down mid-handoff.** Both holders answer the same
   `ScreenChanged` and the order is not ours to pick. On title → overmap the splash let go before
   the overmap took hold, holders hit zero, and the city unloaded and immediately reloaded — which
   races on the same scene and never recovers. `CityView` now defers teardown by one frame and
   re-checks.
2. **`WorldController` was framing the same camera.** Its `FrameOverview()` puts the camera 34 m
   from the origin looking at the placeholder ground, and it won whichever handler ran last, so the
   overmap ended up framed at `(-17, 17, -26)` looking at nothing. It now stands down while the view
   is lent out, and re-frames when it gets it back — which it must do itself, because the deferred
   teardown means the screen change is long gone by then.
3. **The camera's far plane clipped the city away.** It is set for street work at 400 m; the
   overview camera sits 520 m out.

**What this costs.** `GDD.md` §9's "districts are isolated scenes loaded on selection" no longer
holds for these districts — travel is a camera move. That deletes `TRAVELING`'s job 1 (cover the
load) outright, leaving job 2, Voss's transmission, to carry the sequence alone. `TRAVELING.md` §1
already says job 2 is the one that makes the other two work, so it survives — but as a deliberate
beat rather than a disguised loading screen. Decide it, don't discover it.

## Layout

```
┌──────────────────────────────────────────────────┐
│ VOSS // DISPATCH          ▤ 2 unread             │
│                                                  │
│         ░░▒▒▓ NEO-KYOTO ▓▒▒░░                    │
│                                                  │
│      ◈ BLOCK 7          ◈ SECTOR 12              │
│        ★★★ complete       ★☆☆  available         │
│                                                  │
│              ◉ TRANSIT HUB      ◈ DATA CENTER    │
│                !! available       locked         │
│                                                  │
│           ▁▁▁ UNDERCITY ▁▁▁                      │
│                 locked                           │
│                                                  │
├──────────────────────────────────────────────────┤
│ CONTRACTOR #4471 · SENIOR · ★ 14/33 · 2,150 cr  │
└──────────────────────────────────────────────────┘
```

| Element | Purpose |
|---------|---------|
| District markers | Selectable. State shown by colour and icon |
| Voss channel | Unread transmissions surface here, not just in-contract |
| Status bar | ID, rank, stars, credits — the progression at a glance |
| Store access | Rail-consistent; the deck is reachable from the map |

## Selecting a contract

Click a district → panel showing available contracts, Voss's one-line flavour, star state, credits on offer, and a **DISPATCH** button. Dispatch begins `TRAVELING`.

Completed contracts show **REPLAY** with the current rating and what improving it would pay — `★☆☆ → ★★★ (+200cr)` — per `DESIGN_DIRECTION.md`.

## Act structure on the map

The branching tree in `GDD.md` §3 becomes geography:

| Act | Map behaviour |
|-----|---------------|
| **1** | One district available at a time. The next lights up on completion. Linear, and the map teaches itself |
| **2** | Multiple districts available simultaneously. The player chooses. Stuck on one? Take another and come back |
| **3** | Convergence — late districts require prerequisites from several branches |

This is why the map is worth building rather than a list: Act 2's "set it aside and try something else" relief only reads as freedom if the alternatives are *visible*.

## What has to exist first

**There is no district in the code.** This is the blocker under C2, B4 and the act structure alike,
and it is not a UI problem:

| What the map needs | What exists today |
|---|---|
| A district entity with contracts hanging off it | `ContractDef.Location` — a display string (`"Block 7"`) |
| Per-district star aggregation | `GameState` keys completion and stars by **contract id** only |
| Several districts open at once | `GameManager.IsAvailable(i)` is `i == 0 \|\| previous completed` — strictly linear |
| Prerequisites from several branches (Act 3) | nothing |

So the first unit of work is a `District` record — id, display name, map position, its contracts, an
unlock predicate — and an availability rule that takes **prerequisites** rather than an index.

**Build the map against a list-rendered debug view of that model before the panorama exists.** If
the model is wrong, the art is wasted; the panorama is the most expensive asset in the feature and
the last thing that should be committed to.

⚠ This is also why the overmap is **three features, not one**. C2 (selection surface) needs the
district model. B4 (district state) needs C2. Debrief beat 6 needs B4. Sequence them in that order
and each one is small.

---

# Part 2 · District State (B4)

`DESIGN_DIRECTION.md` principle 4: *"The world should reflect mastery. A district where the player has 3★ on all contracts should look and feel different from one with 1★ completions."*

Currently mastery is a number on a summary screen. Put it on the map.

| State | Map treatment | On-site treatment |
|-------|--------------|-------------------|
| **Locked** | Dark. Silhouette only | — |
| **Failing** | Warm amber, flickering. Fault icon | Systems broken, the contract state |
| **Stabilised** (complete, 1–2★) | Cool, steady, but dim. Partial illumination | Fixed, functional, unremarkable |
| **Mastered** (all 3★) | Fully lit, saturated cyan, traffic and drone movement visible | Fixed, and *thriving* — extra activity, more light |

Three properties make this work:

1. **It reuses the existing colour language.** Broken is warm, fixed is cool — already the game's core signal (`GDD.md` §5). Nothing new to teach.
2. **It's the only persistent visible record of quality.** Stars are a number; a lit district is a place you made better.
3. **It motivates replay without nagging.** A dim district among bright ones is an invitation, not a quest marker.

The gap between *stabilised* and *mastered* must be **clearly visible from the map at a glance** — that's what converts "I finished it" into "I want to go back."

## Districts holding more than one contract

The table above quietly assumes **district ≡ contract**. That is *nearly* true today — five
contracts across five locations, `Sector 12` and `Sector 14` the only near-collision — and the
six-set consolidation in `ENVIRONMENT_BRIEF.md` guarantees it stops being true.

A district with one 3★ contract and one never-attempted contract is neither Failing, Stabilised
nor Mastered. The table has no row for it.

**Rule: the marker takes the state of the district's *worst* contract.** Amber the moment anything
in there is unfixed; cool-but-dim while everything is done but not all 3★; fully lit only at all
3★. Worst-case is the only aggregation that keeps the promise *amber means there is work here*.

**The caption carries the count** — `2/3 ★★★` — so the player can tell "one job left" from "one
star left" without opening the panel.

## The lights are the record, and persistence is free

District state is **always derived from saved progress and never stored separately**
(`DistrictRegistry.StateOf` reads completion and stars off `GameState`). So the city coming back
exactly as the player left it costs nothing — there is no second thing to save or to get out of sync.

Two hooks carry it (`GameManager`):

| | |
|---|---|
| `DistrictStateChanged` | Fires the moment a repair changes how a district reads. The city view listens, so the lights come up **while the player is watching** rather than on the next visit |
| `PublishDistrictStates()` | Replays the whole set once the city view is up, so the player arrives to a city that already shows their history instead of animating into it |

The event fires on the *delta*, compared before and after the repair lands — so a replay that earns
no new star does not re-fire the lights coming on.

## Mastery is monotonic

`GameState.RecordScore` only ever raises `existing.Stars`, and pays the difference in credits.
A district therefore **can never darken once lit.**

Worth stating rather than leaving implicit: it kills a whole class of bad feeling ("I replayed it
and made it worse"), and it means the state transition only ever has to animate in one direction.

## Locked districts

Silhouettes are assumed (wanting requires seeing) — but a silhouette that does nothing when
selected generates curiosity and then punishes it, which is worse than not drawing it.

**A locked district is selectable and answers in Voss's voice**, naming the gate:

>     Undercity's sealed. Municipal won't route you there until Transit Hub is signed off.

Never a grey `LOCKED` chip. That is exactly the current board's failure mode and the map should
leave it behind.

---

# Part 3 · The Debrief (C3)

## The rule

**World reveal first. Scoreboard second.**

Established in `ONSITE_PIVOT.md` §4 and `DESIGN_DIRECTION.md`. If the numbers land first, the player reads the numbers and never looks at the city they just repaired — which discards the game's primary feedback channel at the exact moment it pays off.

## Beats

| Beat | What happens | Duration |
|------|--------------|----------|
| **1. Jack-out** | Windows collapse into the deck. Connector releases | ~1.0s |
| **2. Reveal** | Camera pulls back. The location is **working**. Ambient swells to its fixed-state soundscape | ~1.5s |
| **3. Breathe** | Hold on the fixed location. No UI at all | ~3.0s, skippable |
| **4. Voss** | Completion transmission arrives on the deck | Player-paced |
| **5. Summary** | Stars, call count vs target, credits, bonus status, rank progress | Player-dismissed |
| **6. Return** | Overmap. **The district visibly changes state** | ~1.2s |

Beat 3 is the one that will be cut under schedule pressure and shouldn't be. Three seconds of no UI on a city you just fixed is the entire emotional payload of the contract.

Beat 6 closes the loop: you leave a district amber and return to see it cool. Do the state change *after* the map is on screen, not before — the player should watch it happen.

**Beat 6 needs a second feedback channel.** It is the map's only payoff moment and it is specced
visually only. The audio half: the district's **amber fault hum drops out and its steady tone comes
up** across the same 1.0s. Free if district ambience already exists per-scene, and it is the audio
half of the warm/cool language rather than a new sound to invent. A UI chime here would be the
wrong texture — the *city* should make the noise, not the interface.

## Incomplete jack-out

No debrief. Straight to overmap, contract still available, progress persisted, no penalty, no scolding. Voss says nothing.

Leaving a job half-done is a legitimate choice, and `DESIGN_SYSTEMS.md` already identifies branching-away as the intended relief valve for frustration. Punishing it would close the valve.

---

## State Machine

```
OVERMAP ──dispatch──▶ TRAVELING ──▶ SITE ──▶ CONNECTING ──▶ DECK ⇄ RUNNING
   ▲                      │                                      │
   │                   cancel                              jack out
   │                      │                                      ▼
   │◀─────────────────────┘                              DISCONNECTING
   │                                                             │
   │                              ┌──── incomplete ──────────────┤
   │                              │                              ▼
   └──────── DEBRIEF ◀────────────┴──── complete ──── SITE(resolved)
```

**OVERMAP**
- *Entry:* game start; from DEBRIEF; cancel from TRAVELING
- *Exit:* to TRAVELING on dispatch
- *Player can:* select districts, read transmissions, open the store, replay contracts, quit
- *Interruptible by:* nothing. The map is the rest state — it has no timers and nothing arrives
  that takes control away
- *Consumes:* nothing. Selection, deselection and travel are all free
- *Autosaves on entry*
- *Edge:* with **no contracts available** — everything current is done and the next act is gated —
  the map must not be a dead screen. Voss's transmission becomes the only interactive element and
  says what is being waited on. An empty map with no affordance is a soft lock in everything but
  name

---

## Numbers

Option B — starting values with test plans.

| Value | Starting | Test / Pass | If it fails |
|---|---|---|---|
| Debrief beat 2 (reveal) | 1.5s | Player's eye goes to the world, not the UI, 8/10 | Eye stays on UI → the UI is arriving too early; delay beat 5 |
| Debrief beat 3 (breathe) | 3.0s | Not reported as a wait on first completion | Reported → 2.0s. **Do not remove** |
| Return to overmap | 1.2s | District state change is *witnessed*, not discovered | Missed → hold 0.5s before the change fires |
| District state transition | 1.0s | Reads as a change, not a pop | Popping → 1.5s with a light sweep |
| Map districts, Act 2 | 3–5 simultaneously available | Player reports choice, not paralysis | Paralysis → 3; indifference → 5 |
| **District marker hit target** | **44 × 44 px** at 1080p — *sourced, see below* | No mis-selects across a 20-selection run | Enlarge the hit rect, **not** the glyph — the marker can stay small and pretty |
| **Marker label legibility** | The DECK_SPEC §4 floor — *derived, not a new number* | Labels readable over the brightest region of the panorama | Raise the floor for map labels specifically |
| **Map → dispatch, experienced player** | ≤ 2 inputs | Player at ★14+ reaches TRAVELING in ≤2 inputs, 10/10 | Add direct-dispatch on the marker before touching any duration |
| **Newly-unlocked marker emphasis** | Decays after 1 viewing | Player names the new district unprompted on first Act 2 entry, 8/10 | Missed → persist until the district is *entered*, not until it is *seen* |

**Source for the hit target:** WCAG 2.1 SC 2.5.5 *Target Size (Enhanced)*, AAA — 44 × 44 CSS px.
The WCAG 2.2 AA minimum (SC 2.5.8, *Target Size (Minimum)*) is 24 × 24. Take the enhanced value:
these markers sit over a busy night panorama, which is exactly the low-contrast, high-clutter case
the criterion exists to protect.

**On marker legibility:** the §4 legibility floor is currently enforced *inside* `DeckWindow`,
where the background colour is set. A full-frame map is not a `DeckWindow` and inherits none of it,
so district labels need their own scrim — and the constant should be lifted out of `DeckWindow`
into somewhere both can reach.

---

## Five-Component Evaluation

| Component | Rating | Notes |
|---|---|---|
| **Motivation** | **This is the whole reason to build it** | A dim district among bright ones is the only unnagging replay invitation the game has. Stars are a number; a lit district is a place you made better |
| **Fit** | Strong, with one open question | Reading a dispatch board off your deck is what the job looks like. Open: is the map *on* the deck? |
| **Clarity** | Was weak; two gaps now closed | Multi-contract aggregation and locked-district voice, above. Both were rules, not numbers |
| **Response** | **Weakened by the pivot — the thing to protect** | Below |
| **Satisfaction** | Modest | Beat 6 is the map's only payoff; it now has two channels |

### Response: count the cost honestly

```
Today:    board row click ───────────────────────────────▶ editor      1 click, ~0s
Proposed: district ▶ panel ▶ DISPATCH ▶ TRAVELING ▶ SITE ▶ CONNECTING ▶ deck
                                        2.5s min   ~1.0s     ~2.8s      2 clicks, ~6.3s
```

That is the right trade on a first visit — TRAVELING is where Motivation is delivered. It is the
wrong trade on the twentieth. `TRAVELING.md` §5 already abbreviates replays to 1.2s, which covers
most of it. Two things the **map itself** owes, since Response outranks everything else:

- **Keyboard parity.** Districts cycle and dispatch from the keyboard, matching `DeckShell`'s
  `Alt`+1–9 idiom. The list has this for free today; a map loses it unless deliberately built in.
- **Skip the panel when there is nothing to choose.** A district with exactly one available
  contract dispatches on confirm. The panel exists to disambiguate; with no ambiguity it is a tax.

The one thing the current list does better than the map will is **get out of the way**. Don't lose
that on the way to something prettier.

---

## Playtest Scenarios

1. **New player** — first completion of C1. *Pass:* on returning to the map, 8/10 notice Block 7 has changed.
2. **Stress** — skip every debrief beat; jack out incomplete repeatedly; replay a completed contract; dispatch and cancel. *Pass:* no stuck states, no double-awarded credits, no lost progress.
3. **Skill** — a player at ★14/33. *Pass:* they can identify their weakest district from the map alone, without opening a menu.
4. **Abuse** — replay a 3★ contract repeatedly for credits. *Pass:* pays the difference only, which is zero. Grinding is worthless by construction. ✅ *Verified against `GameState.RecordScore`, not just asserted.*
5. **Readability** — observer watches a debrief. *Pass:* 8/10 can say what got fixed and how well it went, in that order.
6. **The week away** — player returns after ≥5 days, mid-Act 2, cold. *Pass:* within 15 seconds and **without opening a panel**, they can say where they were last, what is open, and what they left unfinished. This is the test the map exists to pass and the list can never pass.
7. **The two-contract district** — a district holding one 3★ and one unattempted contract, observer looking at the marker only. *Pass:* 8/10 correctly say there is still work there, *and* distinguish it from a district that is merely un-mastered.

---

## Open

- **Is the overmap rendered *on the deck*, full-frame, with no windows?** ⚠ **Decide before code.**
  `TRAVELING.md` §3 gives the deck a clean progression — *in your hands* (travelling) → *stowed*
  (site) → *plugged in* (working) — and the overmap has no slot in it.

  *Recommendation: on the deck, full-frame, no windows.* Window chrome currently means "you are
  inside a system"; spending that vocabulary on a hub screen dilutes the one signal that makes the
  plug-in land. It also settles `DECK_SPEC` §14's rail question — the rail persists at the map and
  at SITE as the deck's own chrome, and the **windows** are what appear on plug-in.

  It is the cheapest decision now and the most expensive one after the panorama is placed.
- **Two colour languages are now on the same screen.** Found by rendering the grouped board, not
  by reading the spec. District lines use the world's warm/cool language — amber *there is work
  here*, cyan *fixed*. Contract rows use the older UI language — `Good` green *completed*, `Accent`
  cyan *available*. So Sector 14 reads amber while the job inside it reads cyan, and Block 7 reads
  cyan while the job inside it reads green. Nothing is wrong, but a player can reasonably read the
  difference as meaning something.

  Resolving it means touching the `DECK_APPS.md` colour taxonomy, so it is deliberately **not**
  changed here. Worth settling before the map, since the map is all colour.
- **How does the player tell *newly unlocked* from merely *available*?** Act 2 opens 3–5 districts
  at once; with no recency marker that is where paralysis actually comes from, not from the count.
- **Does the map show districts before they unlock?** Silhouettes assumed here — same logic as visible locked tools. Wanting requires seeing. *(Partly answered above: they are selectable and Voss names the gate.)*
- **Where does Act 3 field work sit?** Interior locations may not be map-selectable in the same way if the story routes the player to them. *Do not let this block C2.*
- **Does the map have weather or time of day?** Free atmosphere if the panorama supports it; a trap if it needs a second asset.
