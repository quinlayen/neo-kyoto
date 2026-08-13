---
name: reference-itch-page
description: "Live itch.io page for the Neo-Kyoto WebGL demo, and how to publish updates to it"
metadata:
  type: reference
---

## itch.io — Neo-Kyoto

**Public page:** https://quinlayen.itch.io/neo-kyoto
Went public 2026-08-12. HTML5, plays in browser, no account needed. Status "Prototype".

**Publishing updates:**
```
# 1. Unity menu: Neo-Kyoto > Build WebGL
# 2. Stop any local test server first — it locks Builds/WebGL and the build
#    fails in ~15s with no console error.
cd unity && ./publish-webgl.sh --push
```
Pushes to channel `quinlayen/neo-kyoto:webgl` via butler (installed at
`~/.local/bin/butler.exe`, authenticated via `~/.config/itch/butler_creds`).
butler patches against the previous build, so updates are far smaller than the
16 MB first push. It assigns build numbers itself.

**Things butler cannot do** — these are manual on the itch page: creating the
page, visibility, ticking "played in browser", and embed viewport (set to
1920x1080; the UI docks the editor to the right 42% and gets cramped smaller).

**Telling testers:** desktop browser with a keyboard only (it is a code-typing
game — phones do not work), Chrome/Edge/Firefox, WebGL2, ~16 MB first load.
Progress saves per-browser via IndexedDB, so another device or incognito starts fresh.

Related: [[project-unity-demo-status]] [[project-unity-setup]]
