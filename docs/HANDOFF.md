# Handoff: moving machine or Claude account

**Last updated**: 2026-08-14

Everything needed to pick this project up somewhere else, or under a different Claude account.
Written because the parts that break are the parts that are deliberately *not* in git.

---

## What travels in the repo (nothing to do)

| Thing | Where |
|---|---|
| The game | `unity/neo-kyoto/` |
| Design docs | `docs/` — index at `docs/README.md` |
| **Project memory** | `.claude/memory/` — tracked in git on purpose |
| Project permissions | `.claude/settings.local.json` — tracked |
| Global settings template | `.claude/global-settings.json` |
| game-design skill | `.claude/skills/game-design/` |
| Session orientation | `CLAUDE.md` at the repo root, auto-loaded |
| Setup script | `.claude/setup.sh` |

Memory lives in the repo rather than the default
`~/.claude/projects/<sanitized-path>/memory/`, because that per-machine location does not survive
a machine move *or* a re-clone to a different path. `autoMemoryDirectory` in
`.claude/settings.local.json` points Claude at the in-repo copy.

> If a future session writes memory to the user-profile path instead, it is in the wrong place and
> will be lost. Move it into `.claude/memory/` and add a line to that folder's `MEMORY.md`.

---

## Changing Claude account

The project has been running Claude through **Google Vertex AI**, not a direct Anthropic account.
That is configured *globally*, in `~/.claude/settings.json`, which is **not** in this repo:

```json
"env": {
  "CLAUDE_CODE_USE_VERTEX": "1",
  "ANTHROPIC_VERTEX_PROJECT_ID": "dev-tools-496118",
  "CLOUD_ML_REGION": "global"
},
"model": "claude-opus-5[1m]"
```

### Option A — different Google account, still Vertex

Update `ANTHROPIC_VERTEX_PROJECT_ID` to the new GCP project, then:

```bash
gcloud auth login
gcloud auth application-default login
gcloud config set project <new-project-id>
```

The new project needs the Claude models enabled in Vertex AI Model Garden, and the account needs
Vertex AI User on it. The `model` pin must be a model id that project can actually serve.

### Option B — moving to a direct Anthropic / claude.ai account

1. **Remove all three Vertex env vars** from `~/.claude/settings.json`. Leaving them set makes
   Claude Code fail to start — it will keep trying to reach Vertex.
2. Drop the `"model": "claude-opus-5[1m]"` pin unless that id is available on the new plan.
3. Start Claude Code and run `/login`.

### Either way

- `.claude/global-settings.json` in this repo is a **template of the old Vertex setup**. `setup.sh`
  installs it only when `~/.claude/settings.json` does not already exist. On Option B, edit it or
  skip it — do not install it verbatim.
- The `sage` plugin comes from `github.com/gendigitalinc/sage.git`. If the new account can't reach
  that repo, remove `enabledPlugins` and `extraKnownMarketplaces` from global settings.
- The old global settings also carried a `statusLine` pointing at a versioned path inside the sage
  plugin cache. It is deliberately **not** in the template, because that path breaks on any version
  change. Re-add it from the plugin if you want it back.

Changing Claude account does **not** affect the Unity Asset Store purchases — those belong to the
Unity account.

---

## Changing machine

Run `.claude/setup.sh` after cloning. It installs global settings, the Unity MCP server, and
rewrites `.mcp.json` with the local path. Then, by hand:

1. **Unity Hub** → add `unity/neo-kyoto`, Editor **6000.5.8f1**.
2. **Re-import the purchased kits** from the Unity Asset Store account (see below).
3. Re-run gcloud auth or `/login` per the section above.
4. Restart Claude Code so it picks up `.mcp.json`.

`.mcp.json` hardcodes the MCP server path (`C:/Users/Peter/.local/share/unity-mcp-server/...`).
`setup.sh` regenerates it; if you skip the script, edit it by hand.

---

## What is NOT in git and must be restored by hand

### 1. Purchased Unity kits

Gitignored on purpose — licence terms and LFS quota; the city kit alone is ~2.7 GB.

- **Cyberpunk Megapolis** (Art Equilibrium, Unity Asset Store, $44.99)
- **Rolling Balls Sci-fi Pack** — the sphere meant to replace the C1 power node primitive
- **Cyber Box**

> ⚠ **Megapolis is a two-step import.** The Asset Store import silently installs the **Built-In**
> variant; in URP every material renders **pink**. Afterwards you must double-click
> `Assets/Cyberpunk_Megapolis/Cyberpunk_Megapolis_URP.unitypackage` — inside the imported folder —
> and Import All. This step is easy to miss and is the single most common way to conclude the kit
> is broken.

Full measured assessment: `docs/ENVIRONMENT_BRIEF.md` → *Post-Purchase Verification*.

### 2. The AE/Grunge emission patch

**Reimporting the kit reverts it, and emission silently dies.** Since the kit is gitignored there
is no git safety net.

The stock shader hardcodes `float3 Emission = 0;` in all three passes. The 13-line patch is what
makes the broken-amber → fixed-cyan colour language possible.

Restore by copying `D:\assets-staging\ae-shaders-unity6\DROP-IN\CP_Grunge.shader` over
`unity/neo-kyoto/Assets/Cyberpunk_Megapolis/Other/CP_Grunge.shader`, keeping that exact filename.
Details: `.claude/memory/project-vendor-shader-patch.md`.

### 3. `D:\assets-staging\ae-shaders-unity6\` — machine-local, single copy

Contains the pristine kit shader, the patched drop-in, the rescued publisher shader zip contents,
the Grunge textures, and its own README.

**This is the only copy.** The publisher's source zip was a Google Drive drop that is gone, and the
extracted tree was rescued from volatile `%TEMP%` on a drive that had hit 0 bytes free. Re-obtaining
it means emailing `art_equilibrium.studio@mail.ru`.

> **Copy this folder before wiping or leaving the machine.** It is not in git, not in the Asset
> Store, and not re-downloadable.

---

## Quick verification after a move

```bash
git fsck                       # repo intact
git lfs ls-files | wc -l       # expect 22 LFS objects
```

In Unity: open `Assets/Cyberpunk_Megapolis/Scenes/CP_Demo.unity`. Materials should render normally
— **pink means the URP unitypackage step was missed**. Then drive `_EmissionColor` on any kit
material and confirm it glows; if it doesn't, the shader patch needs reapplying.
