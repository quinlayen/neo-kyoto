# UI Rebuild: status and next steps

**Last updated**: 2026-08-14
**Specs**: `DECK_SPEC.md` (frame, windows, legibility), `DECK_APPS.md` (per-app content),
`ONSITE_PIVOT.md` (view model, the plug-in), `OVERMAP.md` (the board)

The UI is being **rebuilt completely** onto the deck model: a full-frame world with floating
deck windows over it, replacing docked panels everywhere. This file is the running state of
that work — what exists, what doesn't, and what to do next.

---

## Done

### The frame — `Assets/Scripts/UI/Deck/`

| File | What |
|---|---|
| `DeckLayout.cs` | `DeckLayoutSettings`: band split, window sizes, legibility values. Every number is a DECK_SPEC §12 **starting value**, with its test and failure direction in the tooltip |
| `DeckWindow.cs` | Title bar, back/minimise/close, drag, resize, focus visuals, scrim, opacity floor |
| `DeckShell.cs` | Rail, window field, focus + z-order, `Alt`+1–9, `Ctrl`+`Tab`, snapping, clamping, tool launchers, objectives |

Built at **35 / 57 / 8** (world / window field / rail). The world is **full-frame**; the
protected focal region is a composition rule, not a camera rect. `WorldController.fullFrameWorld`
ignores the old `worldViewportWidth`.

Legibility (§4) is enforced *inside* `DeckWindow` where the background colour is set, so no later
transparency setting can push a window under the floor.

### The workspace — the docked panel is gone

Editor, output/terminal and system readout are deck windows. Re-hosted, not rewritten: the code
input, `RunLineHighlight`, run meter, console scrollback and every callback are the same objects
in a new parent, so the contract and interpreter layers were never touched.

Contract header, credits and rank moved to the rail. Rail launchers are glyph-over-caption,
matching DECK_SPEC §2 and the reference games.

**Combined contracts fixed on the way past.** `SetupWorkspace` only ever asked "is this terminal?",
so `ContractKind.Combined` fell through to the editor alone and never got a terminal. It now opens
both at once — the thing the docked panel structurally could not do.

The briefing is a re-openable window (§6), opened in place from the rail. The paced, paged
first-read stays on the pre-work screen.

### The splash

The live Megapolis city is the title backdrop, held until the player leaves the title screen.
Camera adopts the demo scene's own framing and drifts across the beats. The kit's free-fly
controller is disabled by type name, never referenced — the kit is gitignored and a hard
reference would stop a fresh clone compiling.

---

## Not done

| Screen | State | Notes |
|---|---|---|
| **Board** | ⚠ Still a docked right panel | **Not a reskin.** `OVERMAP.md` wants a district map with travel and district state where there is currently a list. Biggest remaining piece |
| **Debrief** | ⚠ Still a docked right panel | Sequencing constraint: `ONSITE_PIVOT.md` §4 says the world reveal comes **before** the star summary. Tangled with the jack-out beats, so do it with Stage 3 |
| **Reference** | Not built | Backlog A4. Multi-instance, navigable — `DeckWindow` already supports a back button for it |
| **Store** | Not built | Backlog B2. Rail launcher exists, greyed |
| **Plug-in** | Not built | Stage 3. CONNECTING state, five beats, boot surface, skip-at-any-frame, jack-out reveal |
| **Toasts** | Not built | Stage 4, DECK_SPEC §7 |
| **Layout persistence** | Not built | Stage 4. Per-location window layout + code buffer. §3 calls losing written code "unforgivable" |
| **Text scale** | Not built | Stage 4, §8 accessibility |

---

## Suggested order

1. **Overmap / Board** — biggest, and it gates the travel and district-state work
2. **Plug-in (Stage 3) + Debrief together** — they share the jack-out reveal ordering
3. **Reference window** — `DeckWindow` navigation already supports it
4. **Stage 4** — toasts, persistence, text scale

---

## Open questions

1. ~~Rail-and-windows, or a full desktop?~~ **Resolved 2026-08-14 — rail-and-windows, confirmed
   by the designer.** The intent is: the location sits behind as a diorama, the screens the player
   jacks into float over it, and they are **movable specifically so the player can shift them
   aside and watch the scene**. That is what is built, and it is what DECK_SPEC §2 specifies.

   **The Farmer Was Replaced is the closest reference of the three** — the farm stays visible
   behind the code UI. `reference/HackHub*.png` and `reference/HackersJourney*.png` are the
   outliers: both are opaque desktops that hide the world entirely, which is exactly why §2
   rejects that pattern. Take rail and window chrome from them; take the *world-behind-UI*
   relationship from TFWR.

   Trajectory, consistent with `ENVIRONMENT_BRIEF.md`: fixed-camera dioramas now, first-person
   walkable at locations later. Nothing in the deck frame assumes a fixed camera, so this does
   not need revisiting when that happens.
2. **Objectives need a Contract-level API.** `Contract` exposes only `IsGoalMet()` and
   `GetStatusText()`; there is no per-contract objective list. The rail currently shows the one
   real objective rather than dummy rows. A real checklist (backlog A5) needs the model first.
3. **Does the rail persist in SITE view**, before the plug-in? Still open from DECK_SPEC §14.
4. **Default window set per contract type** — partly answered (editor/output/readout with explicit
   positions), but untested against combined contracts on a small display.

---

## Gotchas worth keeping

- **Hide the placeholder world before loading a location scene.** `WorldController` builds a
  200×200 m `Ground` whose top face is at exactly y=0, where a city kit puts its pavement.
  Coplanar, and it reads as flickering sidewalks. `WorldController.SetWorldVisible(false)`.
  Applies to every district scene. Also in `CLAUDE.md`.
- **Anything that swaps a backdrop must swap it back.** The player returns to the title more
  than once — after a progress reset, or from the board. `UseLiveCityBackdrop` / `UsePaintedBackdrop`
  and `SplashCityView.Acquire` / `Release` are deliberately symmetric. A one-way swap left the
  title transparent over whatever 3D was framed behind it, which read as "it flashes the contract
  site then restarts".
- **Window widths must fit the field.** At 35/57/8 the field is ~1094 px at 1080p. A window at
  x=656 may be at most ~420 wide or it slides under the rail and clips.
- **Unity does not pick up new script files written outside the editor** until
  `AssetDatabase.Refresh` + `RequestScriptCompilation`. `unity_get_compilation_errors` will
  cheerfully report 0 errors against the stale assembly.
- **Ripgrep respects `.gitignore`**, so it silently finds nothing inside the purchased kit
  folders. Use plain `grep` there.
- **The AE/Grunge emission patch reverts on kit reimport** — see
  `.claude/memory/project-vendor-shader-patch.md`.
