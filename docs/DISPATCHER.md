# The Dispatcher

**Date**: 2026-08-14
**Status**: Current thinking
**Backlog item**: B1
**Addresses**: Motivation — the documented weakest component (`DESIGN_DIRECTION.md` §Analysis)

---

## The Problem

Every briefing in the game opens the same way:

```
─── INCOMING TRANSMISSION ───

Contractor,
```

From nobody. Sent by no one. Nobody in Neo-Kyoto cares whether the player succeeds.

The gamification layer — stars, credits, rank — patched Motivation with *abstract scoreboards*. Hacker's Journey gets more motivational lift out of a single email signed by a friend than any of that, because a person wanting something from you is the cheapest stake there is.

This costs nothing to fix. Same briefing length, same channel, same information. It just needs a sender.

---

## Who

**VOSS.** Contractor dispatch. Surname only.

```
─── INCOMING TRANSMISSION ───
FROM: VOSS // CONTRACTOR DISPATCH
TO:   CONTRACTOR #4471
RE:   #2477 — Block 7 power node
```

Voss sits at a desk somewhere in the city routing jobs to freelancers. They have been doing it a long time. They know the infrastructure better than the people who own it, they know which corporations file genuine emergencies as "non-urgent," and they are professionally forbidden from saying so.

**Surname only, no portrait, no stated gender.** This matches the game's existing convention — the player character has no name and no face either. It also means the player projects rather than receives.

Voss is not a friend and not a quest-giver. Voss is **a colleague on the other end of a radio**: the person who sends you the job, tells you what they know, and occasionally tells you something they shouldn't.

---

## Hard Constraints

Naming a character invites scope creep toward dialogue trees, portraits, and voice acting. It stops here:

| Rule | |
|------|--|
| **Text only** | No portrait, no VO, no animation |
| **One sender** | Voss is the only recurring voice in the entire game |
| **No replies** | The player never chooses a response. Ever. This is a one-way radio |
| **No new channel** | Voss appears in briefings and debriefs — surfaces that already exist |
| **Voss does not teach** | See below. This is the one that matters |

### Voss does not teach programming

Currently the C1 briefing contains a section headed **"WHAT IS A PROGRAM?"** That's a tutorial wearing a transmission's clothes, and it contradicts the project's own third design principle: *teach through mechanics, not text.*

**Split the briefing in two:**

| Surface | Voice | Content |
|---------|-------|---------|
| **TRANSMISSION** | Voss. Human, short, 8–15 lines | Who's affected, what's broken, what Voss knows, what it costs |
| **BRIEF** | Deck-generated. Technical, impersonal | Available commands, syntax, objectives, constraints |

Two tabs in the briefing window, or two windows. The player can reopen either at any time.

This is a structural improvement independent of the naming: it stops the human voice and the reference material fighting for the same space, and it lets Voss be brief.

---

## Voice

Dry. Efficient. Competent. Has opinions and mostly keeps them.

**Do**
- Lead with the human cost, in numbers. *"Four hundred units up there, and about a third of them have no light."*
- Let technical guidance arrive as knowledge, not instruction. *"It won't take a single big correction — it'll trip."*
- Allow one dry aside per transmission, maximum. *"Management filed it as non-urgent. Make of that what you will."*
- Sign off short. `— V`

**Don't**
- Explain syntax. That's the BRIEF's job.
- Praise the player generically. "Great work!" is worth nothing. *"Node's holding. Block 7 has lights."* is worth something.
- Emote. Voss is at work.
- Exposit the plot. Voss knows what a dispatcher would know, and no more.

---

## Tone Arc

The GDD already plans a tonal shift across the acts (`GDD.md` §7). It only lands if there's a person to attach it to.

| Act | Register | Marker |
|-----|----------|--------|
| **1 — The Repair Jobs** | Professional, terse. Occasional dry aside | Routine sign-offs. Voss is doing a job |
| **2 — The Pattern** | Questioning. Voss starts noticing | *"That's the third grid fault this month. I pulled the logs. You should look at them."* Voss begins sending unrequested things |
| **3 — The Architect** | Off-book. Voss is exposed too | Transmissions arrive outside the contract flow. Some are warnings. Voss starts using the player's name instead of their ID |

That last beat — **ID to name** — is the whole arc in one substitution, and it costs one line of copy.

---

## Rewrite: C1 Briefing

### Current

> Contractor,
>
> Welcome to Neo-Kyoto. The year is 2189.
>
> This city runs on thousands of automated systems — power grids, cargo drones, water recyclers, transit networks. When those systems break, people like you get the call.
>
> You are an engineer. You write small programs that tell machines what to do. Right now, Block 7's power node is flickering and the residents are losing power. You need to stabilize it by writing a short program.
>
> *(followed by "WHAT IS A PROGRAM?", "YOUR COMMAND", "YOUR GOAL")*

### Proposed — TRANSMISSION

```
─── INCOMING TRANSMISSION ───
FROM: VOSS // CONTRACTOR DISPATCH
TO:   CONTRACTOR #4471
RE:   #2477 — Block 7 power node

New ID on my list, so I'll assume you're new to
this. Welcome to Neo-Kyoto. Try not to get
attached.

Block 7's node has been flickering six hours.
Four hundred units up there. About a third of
them have no light tonight.

Management filed it non-urgent. Make of that
what you will.

The node's old. It won't take one big correction
— it'll trip and you'll be starting over. You
have to walk it down. Small steps, as many as it
takes, until it settles.

You'll know when it's stable. So will they.

— V
```

### Proposed — BRIEF (deck-generated, separate tab)

```
CONTRACT #2477 · BLOCK 7 · POWER NODE
─────────────────────────────────────
OBJECTIVE   Node state → STABLE

COMMANDS    rebalance()   reduce node load
            print(text)   display text

NOTES       A program is a list of instructions,
            executed top to bottom, one per line.
            A command is a name followed by ().

SCRIPT      block7.py
```

### What changed

Nothing was lost. The teaching content moved to the BRIEF; the fiction moved to Voss.

What was gained: **stakes** (400 units, a third dark, tonight), **a person** who told you, **a political undertone** seeded in the first sixty seconds of the game, and — critically — the mechanic taught diegetically. *"Walk it down. Small steps, as many as it takes"* teaches repeated calls without ever saying "type this line several times."

---

## Rewrite: C1 Completion

### Current

> Power restored. The lights in Block 7 are steady again. District management has logged your work. Payment processed.
>
> ─── WHAT YOU JUST DID ───
> You wrote a program — a real one...
>
> ─── THE LIMITATION ───
> Look at the program you just wrote. You probably typed the same line several times in a row...

### Proposed

```
─── INCOMING TRANSMISSION ───
FROM: VOSS // CONTRACTOR DISPATCH

Node's holding. Block 7 has lights.

Logged it, pushed your payment. That's a real
program you wrote — instructions in order, machine
followed them. That's all software is, all the way
up.

Between us: you typed that same line a lot of
times, didn't you.

There's a better way. It'll be on your deck by
morning.

— V
```

This is the "feel the limitation first" beat delivered by a person who noticed, rather than by a system reporting on itself. *"You typed that same line a lot of times, didn't you"* does the work of the entire **THE LIMITATION** section in one line, and the unlock tease lands as a favour rather than a curriculum step.

---

## Implementation Notes

**C1 is done** — `Contract01.cs`, briefing and completion message, verified in-game 2026-08-14. Page count dropped 5 → 3. Ten briefings and ten completion messages remain (C2–C5 in Unity; C2–C11 in the Python prototype).

### TextMarkup gotchas

`UI/TextMarkup.cs` reflows the source text for the panel, and two rules will bite anyone writing new briefings:

| Rule | Where | Consequence |
|------|-------|-------------|
| **≥8 leading spaces = preformatted block** | `IsCodeBlock`, line 210 | Line breaks preserved, green code styling, background fill |
| **Consecutive prose lines are joined with a space** | `FlushProse`, line 111 | 4-space-indented lines merge into one paragraph |

So:

- **Any aligned or tabular content must be indented ≥8 spaces** — `FROM:/TO:/RE:` headers, `OBJECTIVE/SCRIPT` pairs, command tables. At 4 spaces they collapse onto one line.
- **Never right-align a sign-off with padding.** `— V` pushed right with ~46 spaces reads as a code block and renders with a background box. Keep it at 4 spaces, left-aligned.
- Blank lines separate paragraphs; single newlines inside a paragraph are soft and will be re-wrapped to panel width. This is correct behaviour — the source is written for 45 columns and the panel is wider.

Both bugs were hit and fixed during the C1 rewrite. The preformatted treatment for the transmission header turned out to be an improvement, not just a workaround — the metadata reads as distinct from the message.

---

## Where Voss Appears

| Surface | Act 1 | Act 2 | Act 3 |
|---------|-------|-------|-------|
| Briefing TRANSMISSION | ✓ | ✓ | ✓ |
| Completion transmission | ✓ | ✓ | ✓ |
| Unsolicited messages | — | ✓ | ✓ |
| Overmap contract listings | flavour line | flavour line | flavour line |
| Warnings / off-book | — | — | ✓ |

Unsolicited messages arriving on the deck without a contract attached is the Act 2 signal that the relationship has changed. It needs no new UI — it's the briefing window with a notification toast.

---

## Five-Component Evaluation

| Component | Effect |
|---|---|
| **Motivation** | The point. Someone now wants this fixed, and tells you who's in the dark |
| **Clarity** | Improved — splitting TRANSMISSION from BRIEF stops fiction and syntax competing |
| **Fit** | Strong. A dispatcher is exactly who would exist in this world |
| **Satisfaction** | Completion messages land harder from a person who noticed |
| **Response** | Unaffected. One-way, skippable, re-openable |

---

## Cost

Copy only. No new systems, no new UI beyond a second tab on a window that already exists. Eleven briefings and eleven completion messages to rewrite, plus a style guide entry.

**This is the highest value-per-hour item in the backlog.**

---

## Open

- The name is swappable. **VOSS** is chosen for being short, sayable, gender-unspecified, and plausibly of anywhere. Alternatives worth trying if it doesn't sit: **MERCH**, **SASAKI**, **KESTREL** (too spy), **DESK 9** (no person at all — a deliberate refusal).
- Does the player's contractor ID (`#4471`) get chosen, or assigned? Assigned is more in keeping.
- Act 3's ID-to-name substitution requires the player to *have* a name. Where does it come from — chosen at start, discovered in a database, or never quite stated?
