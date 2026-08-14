# TRAVELING: The Journey to Site

**Date**: 2026-08-14
**Status**: Current thinking
**Backlog item**: C1
**Depends on**: `ONSITE_PIVOT.md` (view model), `DISPATCHER.md` (Voss), `DECK_SPEC.md` (the deck UI)

---

## 1. Three Jobs

TRAVELING sits between OVERMAP and SITE. It has to do three things at once:

1. **Cover the district scene load.** This is where the real loading happens — districts are isolated scenes loaded on selection. A progress bar here would be the only undisguised loading screen in the game.
2. **Deliver Voss's transmission.** The dispatcher briefs you *en route*, because that is what a dispatcher would do.
3. **Establish distance.** Districts feel far apart only if getting there takes something. This is what makes the overmap mean anything.

Job 2 is the one that makes the other two work.

---

## 2. The Elastic Problem

The plug-in could be fixed at 2.8 seconds because nothing external governed it. TRAVELING can't: load time varies by hardware, by district complexity, and by whether the scene is warm in cache. A fixed-length animation either cuts off mid-load or leaves dead air.

**Solution: the player exits TRAVELING, not the loader.**

The transmission arrives and the player reads it. When they're done, they tap to arrive. The loader only has to finish before they do — and on any machine that can run the game, it will.

This inverts the usual relationship. Instead of the player waiting on the loader, the loader waits on the player, and the wait is spent reading something they want to read.

If the load *is* slower than the read, the sequence holds gracefully (§6). If it's faster, nothing is rushed.

---

## 3. Visual Treatment

**You are in transit, reading your deck.**

Foreground: the deck, held. The transmission types out on it.
Background: the city passing — out a transit window, from an elevated line, through rain.

This costs almost no new art. The deck UI is already being built (`DECK_SPEC.md`), and the backdrop is a moving suggestion of city, not a modelled interior. If the kit's `Metro` set can supply a window frame and a passing skyline, that's the whole scene.

### It also introduces the deck before the plug-in

This answers an open question in `DECK_SPEC.md` §14 and gives the object a clean progression:

| State | The deck is… |
|-------|-------------|
| TRAVELING | in your hands, reading |
| SITE | stowed — you're looking at the problem |
| DECK | plugged in, working |

The deck's presence tracks the player's engagement, and by the time they first plug it into something they already know what it is.

---

## 4. Beats

| Beat | What happens | Duration |
|------|--------------|----------|
| **1. Depart** | Overmap recedes. Motion begins. Ambient shifts to transit | ~0.8s |
| **2. Transmission** | Voss's message types onto the deck | Player-paced |
| **3. Hold** | Message complete. City continues passing. Arrival prompt available | Player-controlled |
| **4. Arrive** | Deck stows. Camera lifts to the location. SITE view | ~1.0s |

Beat 4 is the reveal of the broken system, and it should land as one continuous move from deck to world — the same gesture as looking up.

### Skipping

Skippable at any point. Skipping jumps to beat 4 (or holds on a minimal transit shot if the scene isn't loaded yet — see §6).

**A skipped transmission is never lost.** It goes to the briefing window, marked unread, with a toast. The player can read it at the site, mid-contract, or never.

---

## 5. Repeat Visits and Replays

Same fatigue problem as the plug-in, same shape of answer.

| Situation | Treatment |
|-----------|-----------|
| **First visit to a district** | Full sequence |
| **Return, new contract** | Full sequence — there's a new transmission to deliver |
| **Return, same contract (replay for stars)** | Abbreviated. No transmission; Voss has nothing new to say. Straight to arrival |
| **Immediately re-entering after jack-out** | Minimal. Near-instant |

Replaying C1 for a third star should not make the player sit through a welcome-to-Neo-Kyoto message again.

---

## 6. Edge Cases

| Condition | Behaviour |
|---|---|
| **Scene loads instantly** (warm cache) | Minimum dwell still applies. Never flash through — a 0.2s travel reads as a glitch, not speed |
| **Load slower than the read** | After beat 3, hold indefinitely on the transit shot. Ambient continues. Optional low-priority system chatter fills the space. **Never** show a progress bar |
| **Player skips before load completes** | Hold on a minimal transit frame until ready, then arrive. Skipping is a request to stop reading, not a promise of instant arrival |
| **Load fails** | Return to overmap with a diegetic failure — a transit fault, a rerouted line — and a plain-language error underneath. Never a silent bounce |
| **Player skips every time** | Legitimate. Transmissions accumulate in the briefing window. No nagging |
| **Very short transmission** (late-game Voss is terse) | Minimum dwell covers it |
| **Alt-tab during travel** | Pauses nothing; loading continues. Sequence waits at beat 3 |

---

## 7. State Machine

**TRAVELING**
- *Entry:* district selected from OVERMAP
- *Exit:* to SITE on arrival (player-initiated at beat 3, or automatic if skipped and loaded); to OVERMAP on load failure
- *Interruptible by:* skip input (advances to beat 4); cancel input (returns to OVERMAP, aborts load)
- *Consumes:* nothing. Travel is free and always cancellable

**Cancel matters.** A player who picks the wrong district must be able to back out without completing the journey.

---

## 8. Numbers

Option B throughout — starting values with test plans.

| Value | Starting | Test / Pass | If it fails |
|---|---|---|---|
| Beat 1 (depart) | 0.8s | Reads as departure, not a cut | Too abrupt → 1.1s |
| Transmission type speed | 45 chars/sec | Comfortable read pace; faster readers use skip | Too slow to bear → 60 cps, and check the copy is short enough |
| **Minimum dwell** | 2.5s total | A warm-cache load never reads as a glitch | Flashing → 3.0s |
| Beat 4 (arrive) | 1.0s | The look-up from deck to world feels continuous | Disjointed → tune the camera curve, not the duration |
| Abbreviated (replay) | 1.2s | Not perceived as a wait on a third replay | Still noted → 0.8s |
| Skip response | ≤1 frame | Input registers instantly | Any perceptible lag is a bug, not a value |
| Hold-state chatter interval | every 8s | Fills long loads without becoming noise | Annoying → 15s, or silence |

**Note on minimum dwell:** it is the only value here that deliberately makes the game *slower*. It exists because a transition that sometimes takes 3s and sometimes 0.2s feels broken, and consistency reads as quality.

---

## 9. Five-Component Evaluation

| Component | Rating | Notes |
|---|---|---|
| **Response** | Strong | Skippable, cancellable, player-paced. The loader waits on the player |
| **Clarity** | Strong | The transmission states the job before arrival, so SITE view is pure survey |
| **Motivation** | **This is where it lands** | Voss delivers stakes en route. The player arrives already caring |
| **Fit** | Strong | Reading your assignment on the way to a job is what the work actually looks like |
| **Satisfaction** | Modest | Beat 4's look-up is the flourish |

---

## 10. Playtest Scenarios

1. **New player** — first travel to Block 7. *Pass:* reads the transmission to the end without skipping, 8/10; can state who is affected on arrival.
2. **Stress** — skip at every beat; cancel mid-travel; alt-tab during load; travel on a cold cache and a warm one. *Pass:* no stuck states, no lost transmissions, no visible progress bars.
3. **Skill** — twentieth travel. *Pass:* an experienced player reaches SITE in under 2s and doesn't report travel as an obstacle.
4. **Abuse** — repeatedly enter and cancel travel. *Pass:* no resource leak, no state corruption, no penalty.
5. **Readability** — observer watches a first travel. *Pass:* 8/10 can say where the player is going and why.

---

## 11. What This Unblocks

Scene architecture. `GDD.md` §9 says districts are isolated scenes loaded on selection, but until now nothing specified what covers the load. TRAVELING is that cover, and its requirements are concrete:

- Async scene load, initiated at beat 1
- A hold state that can extend indefinitely without degrading
- Load-failure path back to OVERMAP
- Travel scene is lightweight and always resident — it can't itself need loading

---

## 12. Open

- **Does the transit vehicle matter?** Metro, ground car, on foot for adjacent districts? Varying it per district is cheap flavour and makes the map feel physical.
- **Is travel ever interrupted?** An Act 2 beat where the transmission arrives *during* travel and changes the job is available for free once this exists.
- **Return journeys.** Currently jack-out → DEBRIEF → OVERMAP with no travel home. Probably correct — one journey per contract is enough — but worth naming as a deliberate omission.
