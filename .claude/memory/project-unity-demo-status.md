---
name: project-unity-demo-status
description: "First playable Unity demo (C1-C5) is complete and verified as of 2026-08-12; architecture and known polish gaps"
metadata:
  type: project
---

## First Playable Demo — COMPLETE (2026-08-12)

C1–C5 are playable end to end in Unity and verified: each contract completes with its
canonical solution, feature gates fire correctly, and a StandaloneWindows64 build
succeeds with 0 errors.

**Architecture (all under `unity/neo-kyoto/Assets/Scripts/`):**
- `Interpreter/` — hand-written C# Python-subset interpreter (Lexer → Parser → Evaluator).
  Supports print, calls with args, variables, while (True and conditioned), if/elif/else,
  comparisons, arithmetic. Execution is an **iterator yielding ExecEvent**, so the host
  paces it and the world animates as code runs. Sandbox limits: per-contract call cap
  (ends `while True` safely) plus a step budget for loops that call nothing.
- `Core/` — GameState (feature unlocks, completed contracts, retired commands),
  ContractRegistry, GameManager (flow + coroutine script execution), Bootstrap.
- `Systems/`, `Contracts/` — direct ports of the Python prototype; briefing text is verbatim.
- `UI/` — the entire UI is **built in code at runtime** (UIController + UITheme), no scene
  wiring. CodeEditorBehaviour adds Tab-indent and auto-indent to TMP_InputField.
- `World/` — sites are generated from primitives per contract and animate from system state.

**Scene:** `Assets/Scenes/NeoKyoto.unity` holds a single `Bootstrap` GameObject that
creates everything else. This is deliberate — the game is defined in code, not scene data.

**Non-obvious gotchas hit (don't re-discover these):**
- Project is **New Input System only** (`activeInputHandler: 1`) — `UnityEngine.Input` throws;
  use `UnityEngine.InputSystem.Keyboard.current`, and `InputSystemUIInputModule` on EventSystem.
- Don't name anything `Screen` — collides with `UnityEngine.Screen`. The enum is `GameScreen`.
- `unity_execute_code` runs as a **method body**: no `using` directives, use fully-qualified names.
- An **unfocused editor freezes the player loop**, so coroutines stall mid-run and
  screenshots silently fail. Set `Application.runInBackground = true` **after entering play
  mode** — it resets on every domain reload, so setting PlayerSettings alone is not enough.
  A stalled coroutine here is an editor artifact, not a game bug.
- Font is **Cascadia Mono** (SIL OFL, safe to ship) at `Assets/Resources/CascadiaMono SDF.asset`,
  loaded via `Resources.Load`. It covers all box-drawing glyphs but **lacks ★ (U+2605)** —
  all contract text uses ◆ instead, because a proportional fallback glyph breaks monospace
  alignment of the ASCII box art.
- Emissive lights need the URP bloom + Neutral tonemapping volume (built in WorldController),
  otherwise intensities clip to white and the colour-coded status is lost.
- **Never build runtime materials with `Shader.Find`.** Builds strip any shader no asset
  references, so it returns null in a player and nothing renders — while the editor looks
  fine, so this is invisible until you test an actual build. World materials clone
  `Assets/Resources/WorldLit.mat` instead (emission enabled there, so that variant ships too).
  Same trap applies to any shader/variant only reached from code.

**Since then:** briefing text was restored to the fuller pre-trim versions and
paginated, and the gamification layer was ported on a branch — see
[[project-gamification-status]] and [[feedback-briefing-style]].

**Known polish gaps (not blockers):**
- No audio.
- Nobody has played the scoring layer; it is only programmatically verified.
- Interactive typing (Tab/Enter/terminal submit) was verified by unit-testing the indent
  logic and driving the API, not by simulated keystrokes.
- Camera framing on the wide drone lanes (C2/C3) leaves dead space at the bottom.

Related: [[project-unity-setup]] [[project-unity-decisions]] [[project-prototype-status]]
