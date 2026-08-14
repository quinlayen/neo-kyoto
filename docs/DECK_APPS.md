# Deck Apps: Terminal, Editor, Reference, Objectives, Toasts

**Date**: 2026-08-14
**Status**: Current thinking
**Backlog items**: A2, A3, A4, A5, A6 — and D1, D4, D5 folded in where they belong
**Depends on**: `DECK_SPEC.md` (window system, frame, legibility, numbers)

---

## Shared Rules

All three text surfaces inherit from `DECK_SPEC.md`: floating windows, per-location persistence, player-settable text scale, 92% opacity floor, focus lighting.

Two rules specific to text:

**1. Two voices, visually separated.** Every text surface carries both a *simulated tool* speaking and *the deck* helping. These must never be confusable.

| Voice | Register | Treatment |
|-------|----------|-----------|
| **Tool** | Exactly what the real thing would say | Full brightness, standard colours, no marker |
| **Deck** | The game, helping | Dimmed, italic, `▸` prefix, indented |

**2. Colour is never the only carrier.** Errors are coloured *and* marked. Completed objectives are struck through *and* ticked. Grep matches are coloured *and* inverse.

---

# A2 · Terminal

The surface the reference games do best, and the one to get right.

## Colour taxonomy

| Element | Colour | Notes |
|---------|--------|-------|
| Prompt | Cyan | `contractor@neo-kyoto:~$` |
| Typed command | Bright white | Echoes what the player wrote |
| Standard output | Light grey | The bulk |
| Directories | Blue | `ls` output |
| Executables | Green | `ls` output |
| Hidden files | Dim grey | `ls -a` — visibly *lesser*, which is the joke |
| Grep matches | Amber, inverse | The match, not the line |
| Permissions denied / errors | **Warm amber** | Not red |
| Success confirmations | Green | Sparingly |
| Deck hints | Dim, italic, `▸` | Never coloured like tool output |

**Warm amber, not red**, per `DESIGN_SYSTEMS.md` §Tone: *"errors are diagnostics, not punishments."* Red is for things that are actually going wrong in the world, not for the player being wrong.

## Real terminal behaviour

The specific things the reference games get right and most game terminals don't:

- Output **appends and scrolls** as it arrives, not all at once on completion
- Long output scrolls *during* the command, not after
- Scrollback is deep and mouse-wheel scrollable, and auto-scroll resumes when the player returns to the bottom
- `↑`/`↓` walk command history; history persists per location
- Cursor blinks; the prompt redraws correctly on resize
- Text reflows on window resize rather than clipping

## D5 · Errors that look real

The tool speaks first, in its own voice. The deck helps underneath.

```
$ cat /var/log/grid/error.log
cat: /var/log/grid/error.log: Permission denied
   ▸ the file is there — you just can't read it yet.
     try  ls -l  to see who can.

$ apt install subfinder nuclei namp
E: Unable to locate package subfinder nuclei namp
   ▸ apt took that whole line as one package name.
     install them one at a time.
```

Hacker's Journey does exactly this and it teaches better than any tutorial box, because the player learns what the *real* error looks like. When they hit `Permission denied` outside the game, they'll recognise it.

The `DESIGN_SYSTEMS.md` error matrix stays intact — it just moves into the deck voice. Cross-context errors (`rebalance()` typed at a shell prompt) are pure deck voice, since no real tool would produce them.

## Progressive hints

`DESIGN_SYSTEMS.md` already escalates hints on repeated errors. One change: move the `ref` pointer earlier.

| Attempt | Response |
|---------|----------|
| 1st | Tool error + standard deck hint |
| 2nd | `▸ same issue as before. look at line N.` |
| 3rd | **`▸ ref grep` — opens the reference entry directly** |
| 4th+ | Corrected form shown |

The original had the reference pointer at 4th. A player who has failed the same thing three times should be handed the documentation, not a fourth nudge.

---

# A3 · Editor

## Core

- Syntax highlighting for the restricted subset only — highlighting a keyword the player hasn't unlocked would spoil the gate
- Line numbers, current-line highlight
- **No autocomplete.** `GDD.md` §9 is explicit: the player should type and learn the commands. TFWR offers Tab completion; we deliberately don't
- Auto-indent after `:` — this is not autocomplete, it's removing a papercut that punishes learners for something they already understood
- Copy/paste, undo/redo

## Errors

Inline, at the offending line, in warm amber. The player's actual line shown above the corrected form, per the existing tone guidelines.

```
  3   whille True:
      ▸ not quite — check the spelling.
        whille  →  while
```

## RUN

| State | Treatment |
|-------|-----------|
| Idle | `[▶ RUN]`, lit |
| Running | `[■ STOP]`, pulsing. **Always stoppable** |
| Complete | Brief flash, then idle |
| Error | Amber pulse, error surfaces inline at the line |

RUN is a **global binding** (`DECK_SPEC.md` §11) — it fires whether or not the editor has focus.

## The call counter

New, and it matters.

Star ratings are scored on function-call count (`DESIGN_DIRECTION.md` §Star Ratings), but the player currently discovers their count on the summary screen, after the contract is over. That's feedback arriving too late to act on.

Show it in the editor footer after each run:

```
────────────────────────────────────
 ran · 18 calls · ★★☆        13 = ★★★
```

This makes the optimisation loop visible while the player is still in a position to do something about it, and it's what turns "I finished it" into "I could do that in fewer." It is the single cheapest support for the replay loop the design depends on.

**Do not show a target before the first run.** The player should solve it first, then learn there was a better solve — feel the limitation, then get the tool.

---

# A4 · Reference

## Behaviour

- **Multi-instance.** Two entries open side by side, TFWR-style. Comparing `while` and `for` is a real need
- **Navigation history** — the back button in the window chrome (`DECK_SPEC.md` §3)
- **Pinnable** — a pinned entry stays open across jack-outs and travel
- Openable from the rail, from `ref <topic>` in the terminal, and from deck hints
- Cross-links between entries are clickable

## Visibility rules

The `GDD.md` rule was "only unlocked skills appear — no spoilers." Split it on the axis that matters:

| Kind | Locked state | Why |
|------|-------------|-----|
| **Tools** (SQL, Git, terminal utilities) | **Visible, named, greyed** | Concrete and desirable. Answers "what will I be able to do?" — creates wanting, like HackHub's store |
| **Language features** (`def`, `for`, comprehensions) | **Hidden entirely** | Abstract and meaningless before you need them. Showing them contradicts feel-the-limitation-first |

`DESIGN_SYSTEMS.md` already lists `SQL [LOCKED]` and `GIT [LOCKED]` in the contents — this formalises that instinct and stops it leaking to language features.

## D1 · Entry template — the fifth part

The current template is **What / Syntax / Example / Watch Out**. It's missing the part that carries the project's central philosophy into the moment the player actually reads.

Add **Why you wanted this** — and put it *first*.

```
─── WHILE TRUE ─────────────────────────
WHY YOU WANTED THIS
  On Block 7 you typed  rebalance()  a dozen
  times. It worked. It also would not have
  worked for a hundred. A loop runs the same
  line as many times as needed.

WHAT IT DOES
  Repeats the indented block forever, until
  something stops it.

SYNTAX
  while True:
      <do something>

EXAMPLE
  while True:
      rebalance()

WATCH OUT
  • The colon is required
  • Everything indented under it repeats
  • Nothing after the loop will ever run
```

TFWR does this in a section headed "For Beginners" that reconstructs the exact failure you just experienced. It's the difference between documentation and teaching, and it costs one paragraph per entry.

## D2 · Fear defusal

Belongs here and in the unlock message. The sandbox has a 5-second timeout and a MAX_CALLS ceiling; the player will hit both, and fear of an infinite loop stops people pressing RUN. That's a **Response** failure, which outranks everything.

Say it before they can be afraid of it:

```
  ▸ You cannot break anything with this.
    A runaway script stops on its own and hands
    the node back to you untouched. Nothing you
    write can damage Block 7. Go and see what
    happens.
```

## D4 · Worked transcripts

Terminal entries get a full session, not a snippet. Hacker's Journey's tool pages carry a complete example session and it's followable in a way that `grep <pattern> <file>` never is.

```
EXAMPLE SESSION
  $ ls
  system.log  archive/  notes.txt

  $ grep ERROR system.log
  [02:14:07] ERROR grid-sync timeout
  [02:14:09] ERROR node-7 unreachable

  $ grep -n ERROR system.log
  1841:[02:14:07] ERROR grid-sync timeout
  1843:[02:14:09] ERROR node-7 unreachable
```

## D3 · Analogy before syntax

First-encounter entries lead with a concrete comparison before any code. Hacker's Journey: *"Think of subdomains as smaller parts of a big website. Like, if the main site is a hotel, subdomains are the different rooms."*

Applies to genuinely new concepts — loops, conditionals, functions, permissions, version history. Not to every entry; an analogy for `ls` is padding.

## Other rules

- `[NEW]` badge on recently unlocked entries
- Every example uses game commands and scenarios the player has seen — never `fruits = ["apple", "banana"]`
- Search bar filtering by keyword (stretch)

---

# A5 · Objectives

Rail-docked, always visible. Critical-tier HUD.

```
 OBJECTIVES
 ─────────────
 ✓  p̶l̶u̶g̶ ̶i̶n̶
 ✓  i̶d̶e̶n̶t̶i̶f̶y̶ ̶f̶a̶u̶l̶t̶
 ☐  stabilise node
    ▓▓▓▓▓▓░░░░  62%
 ☐  ─ ─ ─ ─ ─
```

## Rules

- **Struck through and ticked** on completion, never removed. Seeing what you've done is half the value
- **Progress where progress is meaningful** — `10/15 sectors` beats a spinner
- **Undiscovered objectives shown as blanks**, not hidden. The player knows there is more without knowing what
- **Multiple objective groups** supported, for Act 2 when contracts overlap

## The line HackHub crosses and we don't

HackHub's tracker names the tool in every step: *"Find the IP address of the target domain. (Command: nslookup)"*. That's right for a tool-driven hacking sim and corrosive here.

**Track objectives, never methods.**

| ✓ Do | ✗ Don't |
|------|---------|
| `stabilise node` | `call rebalance() 12 times` |
| `6 signals synchronised: 3/6` | `use a while loop` |
| `find the crash report` | `cd to /opt/neo-kyoto/services` |

The whole design rests on the player working out *how*. An objective list that answers it is the game solving itself.

---

# A6 · Toasts

Bottom-left, above the rail, stacking upward (`DECK_SPEC.md` §7).

## Triggers

| Event | Toast | Why |
|-------|-------|-----|
| Bonus objective discovered | `HIDDEN FILE RECOVERED · +50cr` | **The gap this closes.** Today a hidden file is read and nothing happens until the summary minutes later — one delayed channel, failing the two-channel minimum |
| Tool unlocked | `NEW TOOL · grep` | |
| Rank change | `SENIOR CONTRACTOR` | Rare, so it lands |
| Reference entry unlocked | `REF · while True [NEW]` | |
| Skipped transmission | `TRANSMISSION UNREAD` | Per `TRAVELING.md` §4 |
| Credits awarded | folded into the event that earned them | Never a bare toast |

Toasts never block input and never require dismissal. Two channels minimum on every one: visual toast **plus** a distinct audio cue.

---

## Playtest Scenarios

1. **New player** — C5, first terminal contract, no instruction. *Pass:* distinguishes tool output from deck hints without being told, 8/10.
2. **Stress** — 2000-line `cat` output; resize mid-scroll; spam `↑`; open six reference windows; RUN with the editor unfocused. *Pass:* no clipping, no lost scrollback, no dropped input.
3. **Skill** — replay C1 after unlocking `while True`. *Pass:* the player cites the call counter as the reason they knew to optimise, and the "Why you wanted this" entry as the reason they knew how.
4. **Abuse** — read the objective list and attempt to derive the solution method from it. *Pass:* impossible. Objectives state ends, never means.
5. **Readability** — observer watches a bonus discovery. *Pass:* 8/10 can say what was found and what it was worth, from the toast alone.

---

## Numbers

Inherits `DECK_SPEC.md` §12. Additional:

| Value | Starting | Test / Pass | If it fails |
|---|---|---|---|
| Terminal scrollback | 5000 lines | C6's 200-line logs never truncate | Truncating → 10000 |
| Output stream rate | 60 lines/sec | Long output reads as streaming, not instant | Feels laggy → 120, or instant above a threshold |
| Hint escalation | 1 / 2 / 3(ref) / 4+ | ≥70% of stuck players self-recover by step 3 | Below 70% → move `ref` to step 2 |
| Call-counter display | after every run | Player attempts optimisation unprompted on ≥1 contract | No attempts → surface the 3★ target after first completion |
| Toast audio | distinct cue per category | Player identifies category without looking | Confusable → reduce to two cues, discovery and progression |

---

## Open

- **Terminal themes** as a store item (`ONSITE_PIVOT.md` §3 lists them as a candidate cosmetic). Requires the colour taxonomy to be data-driven from the start — cheap now, expensive later.
- **`ref` search** — stretch, but the entry count grows fast once SQL and Git land.
- **Does the editor support multiple files?** TFWR does, and it matters once `def` unlocks and players want reusable tools. Probably yes, from the functions milestone onward.
