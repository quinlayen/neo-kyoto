# On-Site Pivot: View Model, The Deck, and The Plug-In

**Date**: 2026-08-14
**Status**: Current thinking
**Companion**: `ENVIRONMENT_BRIEF.md` (locations, kit requirements, camera authoring)

Design here is live. Where this conflicts with the GDD, this document reflects the newer thinking.

---

## 1. The Pivot

The player no longer jacks in remotely from a top-down god view. **They travel to the site and work there.**

Every contract is a place. The player selects a district from an overmap, arrives, finds the access point on the failing system, plugs a physical deck into it, and works with the broken thing live in front of them.

### Why this replaces the god view

The GDD carried an unresolved contradiction. `GDD.md:344` promised the live world would take the majority of screen space, but the combined-contract wireframe at `GDD.md:383-393` was `TERMINAL | CODE EDITOR | SYSTEM STATUS` — the world vanished entirely in exactly the contracts (C8, C10, C11) whose payoff was described as a city-wide cascade of light.

Being on site dissolves the contradiction. The world and the interface stop competing for the same rectangle and become **two things you look at**: the failing hardware in front of you, and the device you plugged into it. That's a view state, not a split screen.

It also does real work for Motivation — the documented weakest component (`DESIGN_DIRECTION.md:23-30`). Standing at a failing substation while traffic gridlocks around you carries stakes a top-down grid view structurally cannot.

---

## 2. View Model

```
OVERMAP ──select──▶ TRAVELING ──▶ SITE ──port──▶ CONNECTING
                                    ▲                 │
                                    │                 ▼
                              DISCONNECTING ◀──── DECK ⇄ RUNNING
                                    │
                                    ▼
                          SITE(resolved) ──▶ DEBRIEF ──▶ OVERMAP
```

| State | What the player sees | Purpose |
|-------|---------------------|---------|
| **OVERMAP** | Stylised district map of Neo-Kyoto. Available contracts marked. Rank, credits, stars. | Contract board + travel. Replaces the "contractor's workspace screen" |
| **TRAVELING** | Transit sequence | **This is where the real loading happens.** Diegetic cover for district scene load |
| **SITE** | Fixed-camera view of the location, system visibly broken | Survey. Read the problem before reading any text |
| **CONNECTING** | The plug-in (§4) | Ritual. Converts looking-at into hands-on |
| **DECK** | Floating deck windows over the live location | Work |
| **RUNNING** | Same, with the world responding | Payoff |
| **DISCONNECTING** | Reverse plug-in, camera pulls back | **The reveal shot** |
| **DEBRIEF** | Stars, credits, rank, what you learned | Scoreboard — *after* the world reveal, never before |

### Explicitly not building

**A large explorable contiguous city.** Districts are isolated scenes loaded on selection. They never need to connect, share scale, or stream. This is the single biggest technical win of the overmap: one district can be built to a high bar and the next one cheaply, with no seams and nobody able to tell.

### Deck view: windowed over live world

Floating, draggable, resizable windows over the location, which **keeps animating behind them**. TFWR's model. The player chooses what to occlude.

Consequence: see the camera authoring rule in `ENVIRONMENT_BRIEF.md` — every location's hero camera reserves a protected focal region that windows avoid.

### Text rendering

**Screen-space UI, always.** World-space monitors exist as set dressing and ambient state, but nothing the player reads or types is ever rendered as a texture in world space. Clarity outranks Fit, and world-space text is the most common way this fantasy gets wrecked. The *framing* sells "I'm on site at a device"; the *rendering* stays crisp.

---

## 3. The Deck

The contractor carries a physical deck — a rugged portable terminal. The floating windows are its OS.

This is the single most load-bearing object in the game, because it resolves four open problems at once:

| Problem | How the deck solves it |
|---------|----------------------|
| The plug-in needs a physical origin | Windows unfold from the deck's screen |
| Credits have no sink (`DESIGN_DIRECTION.md:188`) | **You buy tools for your deck** |
| The reference system needs a diegetic home | It's an app on the deck |
| A nameless, faceless protagonist has no characterisation | The deck is theirs. Scuffed, customised, upgraded |

### The deck as the credits sink

This collapses HackHub's App Store pattern into the fantasy. Locked-but-visible tools create wanting; the store is browsable before you can afford anything.

**Guard rail:** story-critical technologies (SQL, Git) are **granted by the narrative**, never purchased. The store sells conveniences and alternatives — terminal themes, a faster scanner, a diagnostic that reveals one hidden file per contract, extra window slots. If SQL costs credits, players grind old contracts for money and the Motivation win becomes a chore.

### Fit constraint

`GDD.md:17`: *"You are not a hacker or a hero. You are the person who shows up when everything is broken and makes it work again."*

The deck is a tool of the trade, not a sci-fi gadget. Weathered, practical, repaired. It should look like it's been dropped.

---

## 4. Feature: The Plug-In

### Player Goal & Context

The player has arrived at a district and can see something is wrong — conduits arcing, drones circling, traffic backed up. They want to *start working on it*. The plug-in is the ritual converting "I am looking at a broken thing" into "I have my hands on it."

It fires dozens of times across the game. It must feel significant the first time and cost nothing by the twentieth.

### The Five Beats

| Beat | What happens | Duration |
|---|---|---|
| **1. Survey** | SITE camera holds. Broken state readable before any text. No UI but an objective chip | Player-controlled |
| **2. Acquire** | Access point marked — junction box, cabinet, port housing. Diorama: hover-highlight + label. FP later: proximity prompt | Player-controlled |
| **3. Seat** | Camera pushes toward the port. Deck comes up into frame. Connector goes in, physically, with weight | ~1.2s |
| **4. Handshake** | Deck screen wakes. Boot lines resolve. Port identifies the system. Ambient ducks | ~1.0s |
| **5. Resolve** | Deck screen fills frame; windows unfold out of it into layout. Location stays live behind | ~0.6s |

### Jack-out is the payoff shot

Beats reverse 5→3: windows collapse into the deck, connector releases, **camera retreats to reveal the location now working**. Ambient swells to the fixed-state soundscape (`GDD.md:563-567`).

This changes an ordering in the current design. `DESIGN_DIRECTION.md:126-132` puts the star/credits performance screen at completion. It should come **after** the world reveal, not instead of it. Satisfaction first, scoreboard second.

### The fantasy constraint

Not a neural jack. No glowing cable into a skull. A grubby physical connector into a weathered port, seated with the practiced motion of someone who has done it ten thousand times. Competence, not spectacle — which is exactly why it can be short.

Optional flavour: on old infrastructure the housing is filthy and the contractor wipes the port before seating. **Purely cosmetic. Never a mechanical failure** — a connector that randomly fails to seat is a Response violation, and Response outranks everything.

### Five-Component Evaluation

| Component | Rating | Notes |
|---|---|---|
| **Response** | Needs care | Must be skippable at any frame |
| **Clarity** | Strong | One marked, unambiguous interactable. Handshake names the system |
| **Satisfaction** | Strong | Four channels: connector seat (audio), deck wake (visual), ambient duck (audio), camera push (motion) |
| **Fit** | Load-bearing | This beat is where "competent tradesperson" lands or doesn't |
| **Motivation** | Indirect | Survey beat shows how bad it is before you commit |

**Response rules, non-negotiable:**
- Any input during beats 3–5 snaps immediately to the end state
- First visit to a location plays full; return visits abbreviated (beat 3 truncated, beat 4 skipped)
- No modal blocking at any point; the player can always back out to SITE

### State Machine

**CONNECTING**
- *Entry:* from SITE only, by interacting with a port
- *Exit:* to DECK on completion; to SITE on cancel input
- *Interruptible by:* any input (snaps to DECK); cancel input (returns to SITE)
- *Consumes:* nothing. Plugging in is free and always reversible

**DECK**
- *Entry:* from CONNECTING, or from RUNNING on halt
- *Exit:* to RUNNING on RUN; to DISCONNECTING on jack-out
- *Player can:* write, run, read reference, open terminal, look at the world behind, jack out
- *Player cannot:* travel, change contract

### Edge Cases

| Condition | Behaviour |
|---|---|
| Jack out mid-RUN | Script halts. **System state persists** — four repaired drones stay repaired |
| Jack out, contract incomplete | Allowed, no penalty. Contract stays available |
| Re-plug at a known port | Abbreviated animation. **Deck restores previous code buffer and window layout.** Losing written code here would be unforgivable |
| Quit during DECK | Autosave code buffer and window layout |
| Contract completes mid-RUN | Script runs to its natural end, *then* auto-jack-out. Never cut the player's script off at the moment of success |
| Player never finds the port | After 45s in SITE with no interaction, the port pulses. Escalates every 30s |

### Risks & Abuse

- **Twentieth-time fatigue.** Mitigated by abbreviation + skip. Most likely failure mode; watch for it.
- **Transition becomes a loading-hitch dumping ground.** Tempting, but the real load is OVERMAP→SITE. Put diegetic loading in TRAVELING and keep the plug-in honest and short.
- **Skippability undermines first-time impact.** Accepted trade. Response wins.
- **Ports as pixel-hunts.** In a detailed 2.7 GB kit a junction box is visual noise among a hundred props. Marking must be unambiguous, not subtle.

### Playtest Scenarios

1. **New player** — C1, no instruction. *Pass:* finds and uses the port within 30s, 8/10
2. **Stress** — spam click through all five beats; cancel at each; jack out during RUN; re-plug immediately. *Pass:* no stuck states, code buffer never lost
3. **Skill** — twentieth plug-in. *Pass:* expert reaches DECK in under 1.5s, doesn't report the animation as an obstacle
4. **Abuse** — plug/unplug repeatedly mid-script to game the call counter. *Pass:* call count persists across disconnects
5. **Readability** — observer watches a jack-out. *Pass:* 8/10 can say what got fixed from the pull-back reveal alone, before the summary appears

### Numbers

All Option B — starting values with test plans. No sourced benchmarks; none of these are claimed as standard practice.

| Value | Starting | Test / Pass | If it fails |
|---|---|---|---|
| Beat 3 (seat) | 1.2s | Reads deliberate, not slow, 8/10 | Reported slow → 0.9s, then 0.7s |
| Beat 4 (handshake) | 1.0s | Player reads the system name | Unread → keep duration, raise contrast |
| Beat 5 (resolve) | 0.6s | Windows tracked to landing spots | Disorienting → 0.8s, stagger arrival 80ms apart |
| **Full sequence** | **2.8s** | First-timers don't reach for skip | >30% skip on first play → cut to 2.0s |
| Abbreviated | 0.8s | Not perceived as a wait at visit 20 | Still noted → 0.5s |
| Skip response | ≤1 frame | Input→end-state instant | Any perceptible lag is a bug, not a tuning value |
| Port pulse (idle) | 45s, then every 30s | Nobody strands in SITE >2min | Stranding → 30s / 20s |
| Ambient duck on connect | −6dB over 300ms | Deck audio reads as foreground | Muddy → −9dB |

---

## 5. Requirements This Generates For The Screens

The plug-in doesn't depend on the screen design; it constrains it.

1. **Windows unfold from a point** — the deck screen's centre. They need an origin transform and a stagger order.
2. **Layout is saved per location** and restored on re-plug.
3. **The deck's aspect ratio caps the window field.** Windows can't imply a bigger surface than the physical device.
4. **A boot/handshake surface exists** — the deck has a state before any window opens. A screen mode you'd otherwise never have designed.
5. **Windows must be legible over live 3D**, including bright neon. Opacity floor and a backing treatment are requirements, not preferences.
6. **Protected focal region** (right 35%) constrains default spawn positions.
7. **The deck has OS-level chrome** — a persistent frame around the windows, because it's a device you'll later install tools onto. **This is the one that's expensive to retrofit.** Decide it early even though the store itself is far off.

---

## 6. What This Changes Elsewhere

| Doc | Section | Change |
|-----|---------|--------|
| `GDD.md` | §1 Platform | PC native primary; WebGL best-effort only |
| `GDD.md` | §5 Camera | God view → OVERMAP + on-site fixed cameras |
| `GDD.md` | §5 Art Direction | Low-poly-with-upgrade-path reopened; buy target fidelity directly |
| `GDD.md` | §6 UI wireframes | Docked panels → floating deck windows over live world |
| `GDD.md` | §7 Field Work | No longer late-game; on-site is the default. Act 3 escalation becomes *getting inside* |
| `GDD.md` | §9 WebGL | Downgraded from platform target to convenience build |
| `GDD.md` | §10 Demo criteria | God-view/jack-in criteria replaced with overmap/plug-in |
| `DESIGN_DIRECTION.md` | Performance display | World reveal precedes the star summary |

---

## 7. Open

- **What does TRAVELING look like?** It's the real loading cover and currently undesigned.
- **Deck upgrade taxonomy** — what's purchasable, what's granted, what's cosmetic.
- **Does the deck appear in SITE view** before the plug-in, or only come up into frame during beat 3?
