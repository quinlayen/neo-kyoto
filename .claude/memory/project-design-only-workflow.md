---
name: project-design-only-workflow
description: "Working on Neo-Kyoto game design without Unity (e.g. the office machine) — what is available and what to expect"
metadata:
  type: project
---

## Design-only sessions (no Unity installed)

Peter works on this repo from more than one machine. On at least one of them
(the office) there is **no Unity** and the work is **game design only** — writing
and tuning contract content, progression, and mechanics, not building the game.

**Expect this and do not try to fix it:** `.mcp.json` points at a Unity MCP server
with a machine-specific absolute path. Without Unity that server fails to connect.
That is harmless for design work — do not attempt to install Unity or repair the
MCP connection unless asked. (Longer-term fix: have `setup.sh` generate `.mcp.json`
locally and gitignore it.)

**What design work does not need Unity:**
- Contract text lives in `unity/neo-kyoto/Assets/Scripts/Contracts/Contract0*.cs`,
  inside `GetBriefing()` and `GetCompletionMessage()` as C# verbatim strings.
  Editing prose there is plain text editing — no Unity required. Escape a literal
  quote as `""`, and put `" + PageBreak + @"` on its own line for a page break.
- The original Python prototype is still at `contracts/contract_0*.py` in the repo
  root and is the reference for teaching progression.
- `docs/GDD.md` and the UI/UX wireframes hold the design intent.
- The `game-design` skill in `.claude/skills/` provides the evaluation framework.

**What does need Unity (defer to the home machine):** compiling, play-testing,
WebGL builds, and publishing to itch. Text changes made without Unity are safe but
unverified until built — flag them as needing a compile check, since an unescaped
quote in a verbatim string is a compile error.

Related: [[project-unity-demo-status]] [[reference-itch-page]] [[project-unity-setup]]
