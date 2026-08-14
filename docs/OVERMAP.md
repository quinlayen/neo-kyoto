# The Overmap & The Debrief

**Date**: 2026-08-14
**Status**: Current thinking
**Backlog items**: C2 (overmap), C3 (debrief sequencing), B4 (district state)
**Depends on**: `ONSITE_PIVOT.md` (view model), `TRAVELING.md`, `ECONOMY.md`, `DISPATCHER.md`

---

# Part 1 · The Overmap (C2)

## What it is

The overmap replaces three things the GDD previously kept separate: the contract board, the travel layer, and the progression display. It is the game's only hub.

**It is a stylised map, not a playable city.** Neo-Kyoto is never explorable at this scale — districts are isolated scenes (`GDD.md` §9), and the map is the connective tissue between them. This is a deliberate scope refusal: an explorable megacity is not the game.

## Visual treatment

An elevated night panorama of Neo-Kyoto, rendered once, with districts picked out as interactive regions. `ART_BRIEF_SPLASH.md` §Asset 2 already briefs almost exactly this image — it was scoped as a splash background before the pivot, and it now has a functional home.

Districts read at a glance through the game's existing colour language: warm and unstable, or cool and steady.

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
- *Autosaves on entry*

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

---

## Playtest Scenarios

1. **New player** — first completion of C1. *Pass:* on returning to the map, 8/10 notice Block 7 has changed.
2. **Stress** — skip every debrief beat; jack out incomplete repeatedly; replay a completed contract; dispatch and cancel. *Pass:* no stuck states, no double-awarded credits, no lost progress.
3. **Skill** — a player at ★14/33. *Pass:* they can identify their weakest district from the map alone, without opening a menu.
4. **Abuse** — replay a 3★ contract repeatedly for credits. *Pass:* pays the difference only, which is zero. Grinding is worthless by construction.
5. **Readability** — observer watches a debrief. *Pass:* 8/10 can say what got fixed and how well it went, in that order.

---

## Open

- **Does the map show districts before they unlock?** Silhouettes assumed here — same logic as visible locked tools. Wanting requires seeing.
- **Where does Act 3 field work sit?** Interior locations may not be map-selectable in the same way if the story routes the player to them.
- **Does the map have weather or time of day?** Free atmosphere if the panorama supports it; a trap if it needs a second asset.
