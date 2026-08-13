# Art Direction: The Jack-In Fantasy

**Date**: 2026-08-13
**Status**: Active brief — supersedes ad-hoc visual decisions
**Companion to**: `docs/DESIGN_DIRECTION.md`

---

## Player Goal & Context

The player is a contractor who jacks into Neo-Kyoto's infrastructure and repairs it by
writing code. The current build does not deliver that fantasy: the console is a
screen-space overlay on the player's real monitor, and the district is untextured
primitives under a single unlit-ish material.

**Diagnosis** (5-Component Filter): this is a **Fit** failure with a secondary
**Satisfaction** failure. Clarity and Response are currently *strong* and must not
regress in service of Fit.

**Root cause finding**: `Assets/Scenes/NeoKyoto.unity` contains **zero Volume
components**. URP 17.5 is installed and `Settings/DefaultVolumeProfile.asset` exists,
but no post-processing is active in the scene. No bloom, vignette, chromatic
aberration, film grain, or tonemapping. This is the single highest-leverage gap and it
requires no new art assets.

---

## Direction Decisions (locked)

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Console framing | **Diegetic monitor** — RenderTexture on a world-space quad | Literalises the jack-in fantasy |
| Asset licensing | **Free / CC0 only** | No budget; forces lighting-led art |
| District style | Stylized-primitive elevated by lighting, emissive trim, fog, decals | CC0 cyberpunk model coverage is thin; lighting scales better than geometry |

---

## System Rules

### The Two-Pose Monitor

The monitor exists as physical geometry in the scene and has exactly two camera poses.

**REST pose** — the player is looking at a workstation.
- Monitor angled off-axis; desk, keyboard, and the district visible past it
- Full screen-shader treatment: curvature, scanlines, chroma, bloom, glass reflection
- Text on screen is *atmospheric*, not required reading (status glyphs, idle output)

**FOCUS pose** — the player is working.
- Camera dollies to straight-on; quad fills the frame and maps ~1:1 to screen pixels
- Curvature, scanlines, and chroma dial **down**, bloom stays
- Text is authoritative and must be pixel-legible

**Transition** is the fantasy beat. The dolly *is* the jack-in.

### Non-Negotiables

1. **Code text is never read through curvature.** If a glyph is distorted while the
   player is authoring or debugging, the effect is wrong regardless of how it looks.
2. **The RenderTexture is never upscaled at FOCUS.** Sub-1:1 sampling on SDF monospace
   produces shimmer that reads as broken, not retro.
3. **Post-processing must be toggleable at runtime.** Motion sensitivity and
   readability are accessibility baselines in `references/domain-guide.md`.
4. **World feedback scales with star rating.** Per `DESIGN_DIRECTION.md`, 1★ = lights
   on, 3★ = the cascade. Emissive intensity and particle budget are the knobs.

---

## 5-Component Evaluation

| Component | Rating | Notes |
|-----------|--------|-------|
| **Clarity** | ⚠️ At risk | The whole reason for the two-pose design. Curvature + scanlines over code is the failure mode. Mitigated by FOCUS pose, must be verified by playtest. |
| **Motivation** | ➖ Unchanged | Art doesn't fix the credit-sink gap already flagged in `DESIGN_DIRECTION.md` #3. |
| **Response** | ⚠️ At risk | A camera transition that blocks input is a Response regression. Transition must be input-interruptible and typing must be accepted during it. |
| **Satisfaction** | ✅ Large gain | Currently one feedback channel (text). Adds emissive response, bloom pulse, screen reaction, particles. |
| **Fit** | ✅ Large gain | This is the point. |

**Conflict resolution applied**: Fit wanted a permanently angled, heavily filtered
screen. Clarity and Response override. Fit is preserved in the REST pose and the
transition rather than at the expense of authoring.

---

## Implementation Order

Deliberately sequenced so the cheapest, most reversible wins land first — and so asset
decisions are made *after* lighting reveals what geometry actually needs to do.

### Phase 1 — Post-processing (no assets, hours)
Add a global Volume. Tonemapping (Neutral), Bloom, Vignette, Film Grain, Color
Adjustments. Set emissive on the power-node sphere and any active system so bloom has
something to grab. **Expect this alone to change the read of the whole game.**

### Phase 2 — The monitor (no assets, days)
Retarget the UI Canvas to a RenderTexture. Build the monitor as primitives first —
bezel, glass quad, desk. Screen shader with curvature/scanline/chroma parameters
exposed and driven by pose. Camera rig with REST/FOCUS poses.

### Phase 3 — Type (CC0 fonts)
Split the type system. Code editor stays maximally legible; chrome and headers carry
the period character. See Font Candidates below.

### Phase 4 — District (CC0 assets + materials)
Only now, with lighting known. CC0 PBR materials on existing geometry likely beats new
models. Modular kit only where silhouette genuinely fails.

### Phase 5 — Dynamic response
Wire world visuals to contract state and star rating. This is where "the district
changes with the player's input" gets real.

---

## Numbers (Starting Values)

Per the Numbers Policy, every value below is a **starting value with a test plan** —
none are sourced claims, and none should be treated as final.

| Value | Starting | Test / Adjust |
|-------|----------|---------------|
| RenderTexture resolution | 1920×1080 | At FOCUS, capture and compare glyph edges against the current overlay build. If softer, the quad isn't 1:1 — fix framing before raising resolution. |
| Monitor yaw / pitch at REST | 12° / 6° | Observer test: does it read as a physical object on a desk? If it reads flat, increase yaw in 4° steps. If code is tempting to read at REST, increase further. |
| REST → FOCUS transition | 350ms, ease-out | Must feel like intent, not a cutscene. If players report lag, reduce by 50ms steps. Hard floor: input accepted throughout. |
| Screen curvature (REST → FOCUS) | 1.0 → 0.25 | Readability test at FOCUS: can a player read 20 lines of code without leaning in? If no, drive to 0. |
| Scanline opacity (REST → FOCUS) | 0.18 → 0.06 | Same test. Scanlines are the most common legibility killer — cut first. |
| Chromatic aberration (REST → FOCUS) | 0.15 → 0.04 | If colored fringing is visible on glyph edges at FOCUS, set to 0. |
| Bloom threshold / intensity | 0.9 / 0.35 | Emissive systems should glow; UI text should not smear. If text blooms, raise threshold in 0.1 steps. |
| Emissive intensity by star | 1★ 1.0 · 2★ 2.0 · 3★ 4.0 | Readability test: can an observer tell 1★ from 3★ from a screenshot alone, without reading UI? If not, widen the gap. |

---

## Risks & Abuse Cases

- **Legibility regression** — the primary risk. Every effect must have a FOCUS-pose
  reduction and a global off switch.
- **Motion sickness** — curvature plus camera dolly plus grain is a known trigger
  combination. Accessibility toggles are required, not optional.
- **WebGL cost** — the project ships a WebGL build (`Builds/WebGL`, `publish-webgl.sh`).
  Post-processing plus an extra RenderTexture pass is real GPU cost. Profile before
  committing; the Mobile_Renderer path may need a reduced profile.
- **Effect creep** — "noise soup" is called out in the domain guide. More effects is
  not more atmosphere. Each addition must survive the Readability test.
- **CC0 style drift** — assets from many sources will not cohere. Mitigation: a single
  material/palette pass over everything imported, driven by `WorldPalette.cs`.

---

## Playtest Scenarios

1. **New player** — Sit someone at REST pose. Do they understand they can interact with
   the monitor without being told? Pass: 8/10 attempt it unprompted.
2. **Stress** — Spam the focus/unfocus input during transition. Type during transition.
   Pass: no dropped keystrokes, no stuck camera, no re-entrant transition.
3. **Skill** — Can an experienced player work as fast at FOCUS as they did with the flat
   overlay? Pass: no measurable slowdown in time-to-first-run.
4. **Abuse** — Can the player leave the camera between poses and edit code at an angle?
   Pass: state machine forbids it, or the pose snaps.
5. **Readability** — Show an observer a screenshot of a completed contract. Pass: they
   can tell 1★ from 3★ 8/10 times without reading the UI.

---

## CC0 Asset Sources

Verified-permissive sources. **Confirm the license on each individual asset** — these
sites host mixed licenses and the per-asset terms govern.

| Source | Best for | License note |
|--------|----------|--------------|
| **ambientCG** | PBR materials — concrete, metal, grime, painted steel | CC0. The highest-value source for this project: materials on existing geometry. |
| **Poly Haven** | HDRIs, PBR textures, some models | CC0. HDRI for reflections on the monitor glass. |
| **Kenney.nl** | Modular city/space kits, UI, audio | CC0. Low-poly, cohesive within a kit. |
| **Quaternius** | Modular sci-fi / city model packs | CC0. Good silhouette coverage. |
| **OpenGameArt** | Misc; filter by CC0 | Mixed licensing — filter carefully. |
| **Sketchfab** | One-off props; filter by CC0 | Mixed licensing — filter carefully. |

### Font Candidates (all OFL / free)

Split the type system rather than picking one face.

| Role | Candidate | Why |
|------|-----------|-----|
| Code editor | **IBM Plex Mono** or **Iosevka** | Maximum legibility at small sizes, unambiguous `0/O` and `1/l/I`. Iosevka is condensed — more code per line. |
| Chrome / headers / status | **Departure Mono** or **VT323** | Genuine terminal-CRT character. Too rough for authoring code, correct for atmosphere. |

Current `CascadiaMono` is a fine *code* face but carries no period character — it reads
as a modern IDE, which is precisely the Fit problem.

---

## Tuning Priority

If it still doesn't feel right, adjust in this order:

1. **Lighting and emissive** — before anything else. Most "it looks cheap" is lighting.
2. **Bloom and tonemapping** — the difference between flat and cinematic.
3. **Monitor framing** — angle, distance, how much district is visible past it.
4. **Fog and atmospheric depth** — sells scale and hides primitive geometry.
5. **Materials on existing geometry** — CC0 PBR before new models.
6. **New models** — last, and only where silhouette genuinely fails.

---

## Reference

- **Design direction**: `docs/DESIGN_DIRECTION.md`
- **Game design framework**: `.claude/skills/game-design/`
- **Current theme code**: `Assets/Scripts/UI/UITheme.cs`, `Assets/Scripts/World/WorldPalette.cs`
