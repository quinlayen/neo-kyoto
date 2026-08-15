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

### The UI model is settled — do not reopen it

Confirmed by the designer 2026-08-14: **the location sits behind as a diorama, the screens the
player jacks into float over it, and they are movable specifically so the player can shift them
aside and watch the scene.** That is the rail-and-windows model, it is what DECK_SPEC §2
specifies, and it is what is built.

**The Farmer Was Replaced is the closest of the three references** — the farm stays visible
behind the code UI. The two in `reference/` (HackHub, Hacker's Journey) are opaque desktops that
hide the world, which is precisely the pattern §2 rejects. Take rail and window chrome from them;
take the world-behind-UI relationship from TFWR.

**Why:** the band split rests on this, and it is the expensive retrofit `docs/README.md` open
question #3 warns about. It is decided now.

**How to apply:** start with the overmap; it gates travel and district state. Fixed-camera
dioramas now, walkable locations later — the deck frame assumes nothing about the camera.

Related: [[project-asset-kit-status]] [[project-vendor-shader-patch]] [[project-unity-setup]]
