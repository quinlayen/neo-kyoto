# ONCALL: Systems Contractor — working notes for Claude

Cyberpunk programming-puzzle game. The player is a freelance systems contractor in Neo-Kyoto,
2189: they travel to a district, plug a deck into a failing system, write real Python or terminal
commands, and watch the city respond.

**Read `docs/README.md` first.** It indexes the design docs and carries the live open questions.
Design is live — where docs conflict, the newer one wins and says so at the top.

**Doing UI work? Read `docs/UI_REBUILD.md` before anything else.** The UI is mid-rebuild onto the
deck model — full-frame world, floating windows, no docked panels — and that file is the running
state of it, including one question to confirm before the overmap gets built.

## Where things are

| Path | What |
|---|---|
| `unity/neo-kyoto/` | **The game.** Unity 6000.5.8f1, URP 17.5.0. This is the current implementation |
| `docs/` | Design docs. `docs/README.md` is the index |
| `.claude/memory/` | Project memory, **tracked in git** so it survives a re-clone |
| `contracts/`, `main.py`, `interpreter.py` | ⚠ **Historical.** The pre-Unity Python prototype. See `docs/PROJECT_SCOPE.md` |
| `docs/HANDOFF.md` | Machine / Claude-account migration runbook |

The Python files at the repo root are not the game any more. Don't extend them without asking.

## Unity

- Always use the `unity_*` MCP tools. **Never** call the bridge over HTTP (`127.0.0.1:7890/api/...`)
  — that bypasses the queue and safety layer.
- Select the instance first; pass `port: 7890` on subsequent calls.
- Script layout: `Assets/Scripts/{Core,Contracts,Interpreter,Systems,UI,World}`.

## Gotchas that have cost real time

**Purchased asset kits are gitignored** (licence + LFS quota) — `Cyberpunk_Megapolis`,
`Rolling_Balls-Sci-fi_Pack`, `Cyber_Box`. A fresh clone needs them re-imported before their scenes
resolve. Unity references by GUID, so re-importing restores the links. Never commit them.

**Importing Cyberpunk Megapolis is two steps.** The Asset Store import installs the *Built-In*
variant and everything renders pink in URP. You must then run
`Cyberpunk_Megapolis_URP.unitypackage` from *inside* the imported kit folder as a separate step.

**The AE/Grunge emission patch reverts on kit reimport.** The stock shader hardcodes
`Emission = 0`, which would kill the broken-amber → fixed-cyan state language. Details and the
restore path: `.claude/memory/project-vendor-shader-patch.md`.

**Loading a location scene additively? Hide the game's own world first.** `WorldController`
builds a placeholder `Ground` plane, 200 × 200 m, whose top face is at exactly **y = 0** — which
is where a real city kit puts its pavement. Left visible underneath, the two are coplanar across
the whole street and it reads as flickering sidewalks. Call `WorldController.SetWorldVisible(false)`
once the location is definitely loaded, and restore it on the way out. This applies to every
district scene, not just the splash.

**`UI/TextMarkup.cs`**: eight or more leading spaces renders as a preformatted block, and
consecutive prose lines get joined with a space. Aligned content needs 8-space indentation.
Documented in `docs/DISPATCHER.md`.

**Ripgrep respects `.gitignore`.** Searching inside the gitignored kit folders silently returns
nothing — use plain `grep` there, or you will conclude a file doesn't exist when it does.

## Design work

`.claude/skills/game-design/` holds a 5-component filter (Clarity, Motivation, Response,
Satisfaction, Fit) plus a numbers policy: never claim "industry standard" without a source —
either cite one or label the value a starting value with a test plan.
