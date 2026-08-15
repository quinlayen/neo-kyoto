---
name: project-ui-rebuild-status
description: "The UI is being rebuilt completely onto the deck model — what is done, what is next, and the one question to confirm first"
metadata:
  type: project
---

## UI Rebuild (from 2026-08-14)

**The UI is being rebuilt completely.** Docked panels are being replaced everywhere by the deck
model: a full-frame world with floating windows over it, a persistent rail, and no screen that is
"a code editor with a wallpaper".

**Read `docs/UI_REBUILD.md` first.** It is the running state — done, not done, suggested order,
open questions and gotchas — and it is kept current. This memory is only the pointer and the
headline.

### Where it stands

**Done:** the deck frame (`Assets/Scripts/UI/Deck/` — layout, window, shell) at the spec's
35/57/8 split with a full-frame world; the workspace ported off the docked panel into editor /
output / readout windows; glyph rail launchers; the briefing as a re-openable window; the live
Megapolis city as the splash backdrop.

**Not done:** the Board (needs the overmap from `OVERMAP.md` — a district map, not a reskin of a
list), the Debrief (its world-reveal-before-scoreboard ordering ties it to the plug-in work), the
Reference and Store windows, the plug-in sequence, toasts, layout persistence, text scale.

### ⚠ Confirm before building the overmap

Both reference games in `reference/` are **full opaque desktops with wallpaper**. `DECK_SPEC.md`
§2 deliberately rejects that model — *"ours sits in front of one"* — and everything built so far
follows the spec, treating the references as source material for the rail and window chrome only.
If the intent is actually closer to a full desktop, that is a spec change and it invalidates the
35/57/8 band split before the overmap is built on top of it.

**Why:** the band split is the load-bearing decision. Getting it wrong is the expensive retrofit
`docs/README.md` open question #3 warns about.

**How to apply:** ask, then start with the overmap; it gates travel and district state.

Related: [[project-asset-kit-status]] [[project-vendor-shader-patch]] [[project-unity-setup]]
