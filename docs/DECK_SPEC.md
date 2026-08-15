# Deck Specification: OS Chrome & Window System

**Date**: 2026-08-14
**Status**: Current thinking
**Backlog item**: A1
**Depends on**: `ONSITE_PIVOT.md` (the deck, the plug-in, the seven generated requirements)

---

## 1. Player Goal & Context

The player is on site with a deck plugged into a failing system. They need to read the problem, write a fix, run it, and watch the world respond — without ever losing sight of the world.

Everything here serves one constraint: **the deck must never become a full-screen application.** The moment the UI owns the whole frame, the game is a code editor with a wallpaper, and the entire on-site pivot was pointless.

---

## 2. The Frame

Three vertical bands. This is the load-bearing decision.

**Orientation follows the existing build.** `WorldController.worldViewportWidth = 0.58f` already places the world on the **left** and the work panel on the **right**, and the C1–C5 site cameras are framed against it. Flipping the spec is free; flipping the build and re-framing every site camera is not.

```
┌──────────────────┬───────────────────────────────────┬────┐
│                  │                                   │    │
│   PROTECTED      │        WINDOW FIELD               │ R  │
│   FOCAL          │        windows spawn and          │ A  │
│   REGION         │        drag freely here           │ I  │
│                  │                                   │ L  │
│   the failing    │            ┌──────────────┐       │    │
│   system is      │            │ main.py  ▁ ✕ │       │ □  │
│   framed here    │            │──────────────│       │ □  │
│                  │            │ 1 rebalance()│       │ □  │
│   windows never  │            │ 2 ▏          │       │ □  │
│   spawn here     │            │              │       │    │
│   and snapping   │            │    [▶ RUN]   │       │ ▤  │
│   avoids it      │            └──────────────┘       │ ▤  │
│                  │                                   │    │
│                  │   ┌────────┐                      │ ◈  │
│                  │   │ toast  │                      │    │
└──────────────────┴───────────────────────────────────┴────┘
       ~35%                     ~57%                     8%
```

| Band | Contents |
|------|----------|
| **Protected focal region** (left) | The world. Reserved by the camera authoring rule in `ENVIRONMENT_BRIEF.md` |
| **Window field** (centre) | Free-floating windows. The player's workspace |
| **Rail** (right edge) | Persistent deck chrome. Never occluded, never moves |

The rail sits on the far right because it must be on the *same side as the UI* — a rail on the left would overlay the world.

**Toasts move with it:** bottom of the window field, left-adjacent to the rail, stacking upward. Still clear of the world.

### Why a rail and not a desktop

The two hacker-sim references (HackHub, Hacker's Journey) both use a full opaque desktop — taskbar, wallpaper, icon rail. That works because in those games the interface *is* the world. Ours isn't; ours sits in front of one.

**The Farmer Was Replaced is the closer reference**, and the one to reach for when this is ambiguous: the farm stays visible behind the code UI, and watching it respond is the point. Same relationship here — the location is a diorama behind, the screens float over it, and they move **so the player can shift them aside and watch the scene**. Take rail and window chrome from the two hacker sims; take the world-behind-UI relationship from TFWR.

The rail is the synthesis: Hacker's Journey's icon rail without its wallpaper. It gives the deck somewhere to live — tools, objectives, status — at a cost of 8% of the frame instead of 100%.

**Confirmed by the designer, 2026-08-14.** This is settled, not a working assumption.

### Rail contents (top to bottom)

| Zone | Content | HUD priority |
|------|---------|--------------|
| **Link** | What you're plugged into, connection state | Critical |
| **Tools** | Launcher icons: editor, terminal, reference, briefing, store. Locked tools shown greyed, not hidden | Important |
| **Objectives** | Live checklist, always visible (backlog A5) | Critical |
| **Status** | Credits, rank | Important |

Locked-but-visible tools are deliberate — see `ONSITE_PIVOT.md` §3. Showing a named tool you can't afford yet creates wanting; hiding it doesn't.

---

## 3. Window System

### Chrome

Every window has: **title · back (where content is navigable) · minimise · close**. Drag by title bar, resize from any corner or edge.

```
┌─────────────────────────────────┐
│ ↩  main.py              ▁   ✕  │   ← 28px title bar
├─────────────────────────────────┤
│                                 │
│  content                        │
│                                 │
└─────────────────────────────────┘
                                 ◢  ← resize grip
```

The back button exists because the reference app needs navigation history — TFWR's docs have it, and moving between linked entries without it is miserable.

### Focus

Only one window receives keystrokes. This matters most in combined contracts, where a terminal and an editor are open together and typing into the wrong one is a real failure.

- **Focused**: lit border, full-opacity title bar, bright prompt/caret
- **Unfocused**: dimmed border, muted title bar, hollow caret
- **Click** anywhere in a window to focus it
- Focused window rises to the top of the z-order

**Keyboard focus switching: `Alt`+`1…9` for direct selection, `Ctrl`+`Tab` to cycle.**

Note: **`Tab` is not available** — it belongs to the editor for indentation, and to the terminal for completion. Any window-switching binding on plain `Tab` will collide.

### Snapping

Windows snap to the rail edge, to the protected region boundary, and to each other. Snap is a magnetic assist, never a constraint — the player can always place a window anywhere, including over the world if they choose to.

### Multiple concurrent windows

Required, not optional. Two reference entries side by side is a TFWR behaviour worth copying directly (their `While Loop` and `Continue` docs open together so you can compare). Combined contracts need terminal + editor + status simultaneously.

### Persistence

Saved per location, restored on re-plug:
- Which windows are open
- Position, size, z-order of each
- Code buffer contents
- Terminal scrollback and command history
- Reference navigation position

Losing written code on a jack-out would be unforgivable. This is a save-system requirement, already noted in `GDD.md` §9.

---

## 4. Legibility Over Live 3D

Windows sit in front of a neon-lit city. Bright emissive signage behind a code editor is the single most likely way this UI becomes unreadable.

Three layers of defence:

1. **Opacity floor.** Window backgrounds never drop below the floor value, regardless of any transparency setting.
2. **Backdrop treatment.** A darkening scrim behind each window, slightly larger than the window itself, softening whatever is behind it.
3. **Contrast lock.** Text colour is chosen against the window background, never against the world. No "frosted glass" effect that lets world colour bleed into text contrast.

Transparency is a **look**, not a feature. If a setting lets the player make windows see-through enough to hurt legibility, the setting is wrong.

---

## 5. The Boot Surface

Plug-in beat 4 (`ONSITE_PIVOT.md` §4) needs a deck state that exists *before* any window opens.

The boot surface fills the window field — not the rail, not the protected region — and shows the handshake: deck identity, port negotiation, the system it has found. Then windows unfold out of it.

This is a screen mode that wouldn't otherwise have been designed, and it does real work: it names the system the player just connected to, which is the Clarity payload of the entire plug-in sequence.

---

## 6. Window Types

| Window | Purpose | Notes |
|--------|---------|-------|
| **Editor** | Write and run code | Backlog A3 |
| **Terminal** | Shell contracts | Backlog A2 |
| **Readout** | Live system state, per-contract | The numeric companion to the world |
| **Reference** | Unlocked skills documentation | Backlog A4. Multi-instance, navigable |
| **Briefing** | The dispatcher's message | Re-openable; briefings should never be one-shot |
| **Store** | Deck tools | Backlog B2 |

Objectives are **not** a window — they live in the rail, always visible.

---

## 7. Toasts

**Bottom of the window field, left-adjacent to the rail, stacking upward.**

Hacker's Journey puts them bottom-right. Ours sit at the bottom of the centre band — clear of the world on the left, clear of the rail on the right.

Toasts fire on: bonus objective discovered, tool unlocked, credits awarded, rank change. They are the second feedback channel for events that currently have none — a hidden file found today produces nothing until the summary screen minutes later, which fails the two-channel minimum.

Toasts never block input and never require dismissal.

---

## 8. Text & Accessibility

- **Player-settable text scale**, affecting all deck text. Not a global UI scale — the *text* specifically.
- Monospace for editor, terminal, and code samples in the reference. Proportional for prose.
- Colour is never the sole carrier of meaning. Terminal error lines are coloured **and** marked; objective completion is struck through **and** ticked.
- Remappable window-management bindings.

The domain guide lists remappable controls and colourblind modes as high-priority accessibility baselines. Colour-coded terminal output is a core feature here, which makes the redundancy rule load-bearing rather than nice-to-have.

---

## 9. Five-Component Evaluation

| Component | Rating | Notes |
|---|---|---|
| **Response** | Strong | Nothing modal. Every window closeable, every action reversible. Keyboard focus switching |
| **Clarity** | Needs care | Focus state is the risk — two text surfaces, one keyboard. Hence the lit/dimmed treatment |
| **Satisfaction** | Adequate | Windows unfolding from the deck is the main flourish; toasts carry event feedback |
| **Fit** | Strong | A rugged tradesperson's tool. Utilitarian chrome, not sci-fi glass |
| **Motivation** | Indirect | Locked tools visible in the rail create wanting |

---

## 10. Window Focus State Machine

**FOCUSED**
- *Entry:* click within window; `Alt`+n; `Ctrl`+`Tab`; window opened; previously focused window closed
- *Exit:* another window focused; this window minimised or closed
- *During:* receives all keystrokes except global bindings; rendered top of z-order; lit border

**UNFOCUSED**
- *During:* receives no keystrokes; dimmed border and title bar; retains all internal state including caret position and scroll

**MINIMISED**
- *Entry:* minimise button
- *Exit:* click its rail/taskbar entry, or the tool icon that owns it
- *During:* retains all state. Minimising never discards anything

---

## 11. Edge Cases

| Condition | Behaviour |
|---|---|
| All windows closed | Rail persists. World fully visible. This is a legitimate state — the player is looking at the problem |
| Window dragged into the protected region | Allowed. Player's choice. Snapping resists it; nothing forbids it |
| Window dragged off-screen | Clamped so the title bar always remains grabbable |
| RUN pressed while editor unfocused | Works. RUN is a global binding, not window-scoped |
| Two reference windows on the same entry | Allowed. Independent scroll and history |
| Resolution or aspect change | Layout re-anchored proportionally; protected region recomputed; windows clamped back on-screen |
| Text scale increased past window size | Window grows to its minimum legible size rather than clipping text |
| Jack-out with unsaved code | No such state — the buffer is continuously persisted |

---

## 12. Numbers

Option B throughout — starting values with test plans. No sourced benchmarks; none of these is claimed as standard practice.

| Value | Starting | Test / Pass | If it fails |
|---|---|---|---|
| Rail width | 8% of viewport width | All rail content legible without truncation | Truncating → 10%; if still bad, move status to a flyout |
| Protected focal region | **Left** 35% (build currently 58%) | Observer can watch the system respond during RUN without moving a window, 8/10 | → 45% before considering auto-hide. Auto-hide steals control; Response outranks Clarity |
| Window background opacity floor | 92% | Code readable with a neon sign directly behind, 10/10 | Anything below 10/10 → raise to 96%, then opaque |
| Backdrop scrim | 40% darken, 24px beyond window edge | Window edges read as distinct from the world | Edges lost → increase darken before increasing spread |
| Title bar height | 28px at 1080p, scaling with text scale | Grabbable without precision aiming | Mis-grabs → 32px |
| Minimum window size | 320 × 200px | Terminal shows ≥8 lines; editor ≥8 lines | Too cramped → 380 × 240 |
| Window unfold stagger | 80ms between windows | Player tracks each to its landing spot | Disorienting → 120ms |
| Toast duration | 4s | Observer states what was earned and why, 8/10 | Below → +1s steps to 7s max; if still failing the copy is the problem |
| Toast stagger | 400ms | Stacked toasts read as separate | Read as one block → 700ms |
| Snap threshold | 12px | Snapping feels helpful, not grabby | Fighting the player → 8px |
| Text scale range | 80%–160% of default | Legible at both ends without layout breakage | Breakage → clamp the range and fix layout first |

---

## 13. Playtest Scenarios

1. **New player** — C1, editor and readout open by default. *Pass:* writes and runs code without opening the window-management options, 8/10.
2. **Stress** — open every window type at once; drag them all into a pile; resize to minimum; spam focus switching during a RUN; change resolution mid-session. *Pass:* no lost state, no unreachable windows, no unrecoverable layouts.
3. **Skill** — combined contract (C8). *Pass:* an experienced player switches terminal↔editor by keyboard without looking, and never types into the wrong surface twice in a session.
4. **Abuse** — cover the entire protected region with windows, then RUN. *Pass:* allowed, and the player can recover the view in one action.
5. **Readability** — observer watches over the shoulder during a RUN. *Pass:* 8/10 can say which window has focus and what the world is doing.

---

## 14. Open

- ~~Rail on the left or the right?~~ **Resolved 2026-08-14** — rail right, world left, following the existing build (`worldViewportWidth = 0.58f`).
- ~~Does the world band stay at 58%, or shrink toward 35%?~~ **Resolved 2026-08-14, after the kit import.** Built at the spec's **35 / 57 / 8**. The deciding argument was the window field: at 58% world it drops to ~653px, barely two minimum-size (320px) windows, which starves exactly the combined contracts that need a terminal and an editor side by side. Tunable live on `Bootstrap.deckLayout`.
- ~~Is the world letterboxed into a band, or full-frame behind the windows?~~ **Resolved 2026-08-14 — full-frame.** `WorldController.fullFrameWorld` now ignores `worldViewportWidth`, the camera fills the screen, and the protected focal region is a **composition rule for where the failing system is framed, not a camera rect**. This is what §3 and §11 already implied: windows sit "over the location, which keeps animating behind them", and dragging a window into the protected region is explicitly allowed — neither reads sensibly against a letterboxed world.
- **Does the rail persist in SITE view**, before the plug-in, or appear only in DECK view?
- **Default window set per contract type** — what's open when the boot surface clears.
- **A "reset layout" affordance** — cheap, and prevents the abuse case above from becoming a support issue.
