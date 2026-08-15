---
name: project-unity-setup
description: "Unity project location, version, and MCP connection details for Neo-Kyoto"
metadata:
  type: project
---

## Unity Project Setup (as of 2026-08-12, updated 2026-08-14)

- **Repo moved 2026-08-14**: `C:\Users\Peter\Documents\neo-kyoto` → **`D:\neo-kyoto`**. C: had hit 0 bytes free, which was silently failing Unity asset imports and locking the editor. The C: copy is deleted; `git fsck` clean, all 22 LFS objects intact. `Library/` was deleted so Unity rebuilt clean.
- **Location**: `unity/neo-kyoto` inside the main repo (not a separate directory)
- **Unity version**: 6000.5.8f1
- **Render pipeline**: URP (confirmed, matches WebGL compat decision in [[project-unity-decisions]])
- **MCP**: AnkleBreaker unity-mcp-server, connected via `.mcp.json` at repo root. Plugin package `com.anklebreaker.unity-mcp` installed in the Unity project's manifest.
- **MCP server install path**: `C:\Users\Peter\.local\share\unity-mcp-server` (Windows). Note: this path is machine-specific — `.mcp.json` in the repo hardcodes it, so switching machines (e.g. back to the Linux box) requires editing `.mcp.json`'s `args` path and re-running `npm install` in the server dir if `node_modules` doesn't carry over cleanly.
- **Instance selection**: MCP supports multiple Unity instances; this project must be selected explicitly via `unity_select_instance({projectName: "neo-kyoto"})` and `port: 7890` included on subsequent calls.
- Default packages present: Input System, AI Navigation, Timeline, Test Framework, standard module set.
- **Purchased asset kits are now imported** (Cyberpunk Megapolis, Rolling Balls Sci-fi Pack, Cyber Box) and are gitignored — a fresh clone must re-import them. See [[project-asset-kit-status]] for the two-step URP import trap.

**How to apply:** Unity implementation work happens in `unity/neo-kyoto`. Use the unity_* MCP tools (never raw HTTP to the bridge) for all editor operations — scene setup, GameObjects, scripts, builds.

Related: [[project-unity-decisions]] [[project-prototype-status]]
