# Art Brief: Splash Screen & Logo

**Date**: 2026-08-13
**Status**: Active brief
**Wireframe**: `docs/wireframes/06-splash-screen.excalidraw`
**Companion to**: `docs/ART_DIRECTION.md`, `docs/DESIGN_DIRECTION.md`

---

## Context

ONCALL: Systems Contractor is a cyberpunk programming game. The player is a
freelance infrastructure engineer who jacks into city systems and fixes them by
writing real code. The splash screen is the player's first impression — it sets
the tone before any gameplay begins.

ONCALL is the in-universe contractor dispatch system — the network the player
logs into to receive contracts and connect to city infrastructure. The game is
named after the system, the way Hacknet is named after its network.

The in-game UI lives on a **diegetic monitor** (see `ART_DIRECTION.md`). The
splash screen is the one moment the player sees the city as *themselves*, on
their own screen, before the camera pulls back and reveals the in-world
workstation. This makes the splash special: it's the threshold between the real
world and the game world.

### Asset Budget

Free / CC0 / AI-generated assets are the current constraint. This is a
**prototype-phase budget limitation**, not a design choice. Professional assets
will replace placeholders as investment allows. All assets should be designed
at production quality targets and produced at whatever fidelity is currently
achievable.

---

## Brand System

The game has two marks for different contexts:

| Context | Mark | Used where |
|---------|------|------------|
| **External** | `ONCALL://` | Splash screen, store pages, marketing, loading screens |
| **In-game** | `ONCALL >` | Contract board header, terminal UI, corner badge |

Both use the same typeface, color, and effects. The difference is the
separator: `://` reads as a connection string (you're dialing in), `>` reads
as a terminal prompt (you're inside). The subtitle **SYSTEMS CONTRACTOR**
appears with both marks.

---

## Deliverables

Five separate assets. They must be produced independently — never baked
together — because they composite at runtime and several are reused across
the game.

---

### Asset 1: LOGO

The logo is typographic. No icon, no emblem, no glyph. The typeface and the
`://` separator *are* the identity.

**Text layout (external mark):**
```
ONCALL://
SYSTEMS CONTRACTOR
```

**Text layout (in-game mark):**
```
ONCALL >
SYSTEMS CONTRACTOR
```

**"ONCALL://"** is the primary mark. Large, dominant, the name people remember.
The `://` is integral to the logo — it's the design hook that signals "this is
a connection, a live system." It must not be separated or omitted.

**"SYSTEMS CONTRACTOR"** is the subtitle. Smaller, dimmer, subordinate.

**Typography:**
- Monospace typeface with period character — something that reads as
  "terminal from 2189", not "modern IDE." See font candidates below.
- Medium weight. Not thin (fragile), not bold (aggressive). The contractor
  is competent, not flashy.
- Generous letter-spacing. The logo should breathe. The shorter primary mark
  (6 characters + separator) gives more room for spacing than a longer name.
- The title and subtitle should be the same typeface at different sizes and
  opacities, not two different fonts.

**Font candidates** (from `ART_DIRECTION.md`, atmospheric role):
- **Departure Mono** — genuine terminal character, geometric
- **VT323** — CRT-era, more retro
- A custom monospace with cyberpunk character, if AI-generating

The code editor uses a different, maximally legible font (IBM Plex Mono /
Iosevka). The logo should NOT use the code font. The split is deliberate:
the logo sets atmosphere, the editor prioritises function.

**Color:**
- Primary: cyan-teal (#00D4AA range) — matches the game's accent color
- The `://` can be the same color or slightly dimmer — experiment with both
- Subtitle: dimmer teal (#008877 range)
- On black / transparent background

**Effects:**
- Subtle CRT phosphor bloom — the letters glow slightly, as if displayed on
  a high-quality monitor. Not a heavy neon glow. Understated.
- Faint horizontal scan-line texture across the letterforms. Barely visible.
  Present enough to feel like a screen, not enough to impair readability.
- No drop shadow, no bevel, no outline stroke. Clean.

**Dynamic behavior** (implemented in Unity, not baked into the asset):
- Early game: the logo has a very subtle flicker / signal instability —
  the ONCALL system itself is running on degraded infrastructure
- Late game: rock solid, no flicker — the player has stabilized the city
  and the system they connect through reflects that
- This reinforces "your work matters" — even the logo reflects progress

**Deliverable formats:**
- PNG with alpha (transparent background), minimum 1920px wide
- Layered source (PSD / SVG) if possible, so title, separator, and subtitle
  can be separated and animated independently
- Additional sizes: 512x512 (store icon / social media crop), 256x128
  (corner UI badge)
- Both marks (`:// ` and `>`) rendered separately

**Quality test:** Does it look like it belongs on a loading screen beside
Shenzhen I/O, Hacknet, or Exapunks? If it looks like a tutorial app, it's
wrong. If it looks like a AAA action game, it's also wrong. The sweet spot
is: competent, atmospheric, professional.

---

### Asset 2: CITY PANORAMA — EARLY GAME (degraded)

A top-down / slight isometric view of the city at night, in a state of
widespread failure. This is the splash screen background. The player sees a
broken city and implicitly understands: "this is where I'll be working."

**Perspective:**
- Bird's-eye / elevated isometric angle (not pure top-down)
- High enough to see multiple city blocks / districts
- Dense urban layout — this is a megacity, not suburbs

> **Note (2026-08-14):** this asset predates the on-site pivot, and the shipped splash is already street-level (`cb39e31`), which is the correct read. An elevated panorama still has a home — it is what the **overmap** should look like. Gameplay itself is never viewed from here.

**Art style:**
- Stylised, not photorealistic. Low-poly or painterly is fine.
- The style should match the game's art direction: "stylized-primitive
  elevated by lighting, emissive trim, fog" (from `ART_DIRECTION.md`)
- Think: a beautiful painting of a city in distress, not a GIS satellite photo

**Color and mood:**
- Dominant: dark navy / deep indigo (#1a1a2e range) — the city is mostly dark
- Accent: warm orange / amber (#ff8844) on roughly 30-40% of structures —
  these are the broken, flickering systems. Unstable light.
- Some structures are completely dark (dead zones — no power at all)
- Very few cool-colored lights — the city hasn't been fixed yet
- Atmospheric: rain suggested (streaks, reflections on surfaces), fog / haze,
  light pollution glow where systems are still active

**Composition:**
- The center-upper-third should be slightly sparser or darker. This is where
  the logo sits. The city should frame the logo, not compete with it. A
  natural dark area (park, river, wide avenue) works better than artificially
  clearing buildings.
- Visual interest should be distributed across the frame — no single focal
  point, because the logo is the focal point.
- Edges should be darker (the runtime vignette will reinforce this, but the
  art should support it).

**What NOT to include:**
- No text, UI elements, or HUD
- No characters or vehicles (the city feels empty — that's the point)
- No specific recognizable real-world landmarks

**Resolution:** 2560x1440 minimum (allows cropping for different aspect
ratios). Wider is better for safe-area flexibility.

**Licensing:** Must be usable commercially without attribution requirements.
If AI-generated, verify the tool's output licensing terms. If sourced, must
be CC0 or equivalent.

---

### Asset 3: CITY PANORAMA — LATE GAME (restored)

The same city, same composition and perspective, but restored by the player's
work. This is the bookend — the visual proof that progress mattered.

**Identical to Asset 2 except:**
- 80-90% of districts are lit with cool colors: cyan (#00D4AA), teal, soft
  blue. Smooth, steady light — systems running correctly.
- A few remaining dark spots (not everything is fixed yet, even late game)
- The orange/amber is mostly gone — only 5-10% of structures, the last
  holdouts
- The rain and atmosphere are the same, but the city feels *alive* rather
  than dying. The difference is entirely in the lighting, not the geometry.
- Overall impression: "I did this."

**Critical:** The composition must match Asset 2 closely enough that a runtime
crossfade or blend between them looks natural. Same buildings, same layout,
different lighting state. If AI-generating, use the same seed / base image
and adjust the lighting pass.

**Intermediate states:** Between early and late game, the splash screen should
show partial progress. This can be handled in two ways:
- **Ideal:** Generate 3-5 intermediate panorama states (20%, 40%, 60%, 80%
  restored). The game selects the closest match to the player's completion
  percentage.
- **Acceptable:** Generate only the two bookends (Assets 2 and 3). The game
  crossfades between them based on completion percentage. This works if the
  compositions are close enough.

---

### Asset 4: VIGNETTE OVERLAY

**Not AI-generated.** A simple radial gradient: black at edges, transparent in
center. Applied as a layer on top of the city panorama in Unity.

This is built as a shader or a pre-made transparent PNG. Its purpose is to
darken the frame edges, push focus to the center where the logo sits, and
create depth.

**Spec:** 1920x1080 PNG, radial gradient from center (alpha 0) to edges
(alpha 200-230). Subtle — it should feel like atmosphere, not a filter.

---

### Asset 5: BUTTON & UI CHROME

**Not AI-generated.** Built in Unity with TextMeshPro and the game's UI
system. Same monospace font as the logo (atmospheric font, not code font).

- **"CONNECT TO ONCALL TERMINAL"** — thin stroke outline in accent color
  (#00D4AA), no fill, slight rounded corners
- **"Progress saves automatically."** — small dim text below the button

These are code, not art assets. Included here for completeness so the art
brief accounts for every element visible on the splash screen.

---

## Animation Sequence

The splash screen is not a static image. It's a 6-8 second intro that
establishes the world before gameplay. Skippable after first view (or by
user preference).

| Time | Event | Duration |
|------|-------|----------|
| 0.0s | Black screen. Low ambient hum fades in — distant city, electrical buzz. Not music. Environment. | — |
| 0.5s | City panorama fades up from below. The player sees the city at night — broken, beautiful, waiting. | 2.0s |
| 2.5s | Logo resolves. `ONCALL://` locks on like a signal finding its frequency — characters appear left to right or resolve from noise to sharp. The `://` lands last. | 2.0s |
| 4.5s | Tagline fades in below the logo. Dim, understated. | 1.0s |
| 5.5s | "CONNECT TO ONCALL TERMINAL" button appears. The player can now act. | 0.5s |
| — | Rain ambience + electrical hum persist throughout. No music on the splash. | continuous |

**After first playthrough:** The splash should be skippable (press any key
to jump to the connect button). The animation is for first impressions, not
repeated friction.

**Dynamic logo behavior:** The logo resolves with slight instability early
game (signal interference — the ONCALL system runs on the same degraded
infrastructure the player is fixing). Late game, it locks on instantly and
holds steady. Small touch, big payoff.

---

## Splash-to-Game Transition

The splash screen is the player's real screen. The game world has a diegetic
monitor. The transition between them is a key moment:

1. Player clicks "CONNECT TO ONCALL TERMINAL"
2. Screen whites out briefly (connecting to the dispatch system...)
3. Camera fades in on the in-world workstation at REST pose — monitor, desk,
   the district visible beyond
4. First-time players go directly into C1's briefing on the monitor
5. Returning players see the contract board on the monitor

This transition is designed but not part of the art brief — it's a Unity
camera and shader task. Noted here so the splash art is designed with the
transition in mind: the panorama shouldn't feel like a standalone painting
but like a view the camera could pull back from.

---

## AI Art Generation — Prompt Guidance

If using Grok, Midjourney, DALL-E, or similar tools:

### Logo prompt (starting point)

> Minimalist typographic game logo on a transparent background. The text
> "ONCALL://" in a clean monospace terminal font, glowing cyan-teal color,
> with very subtle CRT phosphor bloom and barely visible horizontal
> scan-lines. The "://" is part of the logo, like a URL connection string.
> Below it, "SYSTEMS CONTRACTOR" in the same font, smaller and dimmer. No
> icon, no emblem, no decoration. Engineering aesthetic, not hacker aesthetic.
> 4K resolution. IMPORTANT: pure transparent background (alpha channel),
> no background color, no gradient, no glow halo extending into the
> background — the logo must composite cleanly over any image.

**Iterate on:** glow intensity (less is more), letter spacing (wider), scan
line visibility (barely there), font weight (medium, not thin or bold), how
the `://` reads relative to `ONCALL` (same weight or slightly lighter).

### City panorama prompt — early game (starting point)

> Bird's-eye view of a cyberpunk megacity at night, top-down with slight
> isometric angle. Dense city blocks, many buildings completely dark, some
> with flickering warm orange-amber lights suggesting failing systems. Dark
> navy atmosphere, rain, fog, light pollution haze. Stylized low-poly art
> direction, not photorealistic. The upper-center area should be naturally
> darker or sparser — a wide avenue, park, or river — to leave space for a
> logo overlay. No text, no UI, no characters. The city feels broken and
> empty but beautiful. Game background asset, 2560x1440.

### City panorama prompt — late game (starting point)

> Same composition as [reference the early game image]. Bird's-eye view of
> the same cyberpunk megacity at night, but now most buildings glow with
> steady cool cyan-teal lights. Systems are operational, the city is alive.
> A few dark spots remain. Same rain and atmosphere. The mood is hopeful
> and accomplished, not dark and broken. Game background asset, 2560x1440.

**Key iteration notes:**
- Generate the early and late game versions from the same base if possible
- The center should be naturally darker/sparser for logo placement
- Reject outputs that are too busy — the panorama is a backdrop, not the
  star. The logo is the star.
- Reject outputs that look photorealistic — the game is stylized
- Reject outputs with recognizable real-world architecture

---

## Licensing Checklist

Before using any generated or sourced asset:

- [ ] Verify output licensing of the AI tool used (commercial use allowed?)
- [ ] Confirm no attribution requirement, or document required attribution
- [ ] Check for style-transfer concerns (does it closely replicate a specific
      artist's work?)
- [ ] Save the generation prompt and parameters for reproducibility
- [ ] Keep the layered / high-res source for future iteration

**Note:** CC0/free sourcing is a current budget constraint. These assets are
designed at production quality targets and will be replaced with professional
art as investment allows. Design the pipeline for swapability.

---

## Reference

- **Wireframe mockup**: `docs/wireframes/06-splash-screen.excalidraw`
- **Art direction**: `docs/ART_DIRECTION.md`
- **Design direction**: `docs/DESIGN_DIRECTION.md`
- **Game design framework**: `.claude/skills/game-design/`
- **UI theme code**: `Assets/Scripts/UI/UITheme.cs`
