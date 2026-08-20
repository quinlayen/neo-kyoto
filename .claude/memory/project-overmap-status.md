---
name: project-overmap-status
description: "Overmap + district model built on feat/district-model-overmap; the city is one persistent scene and contracts are worked in it"
metadata:
  type: project
---

## Overmap and district model (as of 2026-08-16, branch `feat/district-model-overmap`)

The board is no longer a list. **The overmap is the live city seen from above**, districts are
markers pinned to real world positions, and a contract is worked at street level *in that city*
rather than over the placeholder ground. Full reasoning and numbers: `docs/OVERMAP.md`.

### The shape of it

| Piece | Owns |
|---|---|
| `DistrictRegistry` | `District`: world `Anchor` + `MapFraming` for the map shot, `WorkSite` + `WorkFraming` for the kerbside shot |
| `CityView` | The city scene. **Reference-counted** — title and overmap both hold it, so moving between them never unloads it |
| `OvermapView` | Overview framing, dispatch zoom, fog/far-clip override for altitude |
| `DistrictMarkers` | Diamonds projected from `District.Anchor` each frame, hover/click popup |
| `WorkSiteLights` | Drives the block's real lights and emissive materials from `Contract.ProgressFraction` |

`Assets/Scenes/NeoKyotoCity.unity` is **our copy** of the kit's `CP_Demo`. Never edit `CP_Demo`.

### Traps that cost time here, in order of how much

1. **A serialized scene value always beats a class default.** Bit three separate times —
   `sceneName`, `fogDensityScale`, `overviewDistance`. Changing a default in code does nothing if
   Bootstrap already has the field serialized in `NeoKyoto.unity`. Change both.
2. **The map anchor is not a work site.** Block 7's anchor sits on a rooftop at y=35. Work sites
   must be found by raycasting for ground below y=1 **with nothing overhead for 40 m** — the
   ground test alone finds dead-end courtyards.
3. **Frame the feedback, not the prop.** The first kerbside shot pitched down at the pavement and
   clipped every window off the top, so there was nothing to watch come on.
4. **Light components barely matter; emissive materials carry the look.** Driving lights moved the
   numbers and not the picture. Emission must go through a `MaterialPropertyBlock`, **per material
   slot** — a block set without a slot index lights every submesh and blows the street to white.
5. **`_EMISSION` is a shader keyword, so a property block cannot switch a dark window on.** The kit
   ships `_NoEm` twins; swap the material instead.

### Open, not bugs

- The HOP MORE sign renders brighter with `WorkSiteLights` running than without, even where the rig
  writes back the material's own authored emission and should be a no-op. Ruled out: kit scripts
  animating it. Next suspect is gamma/linear between `Material.GetColor` and
  `MaterialPropertyBlock.SetColor`. Settle before tuning `flickerAmount`.
- URP shadow atlas is saturated at street level — 175–256 maps in a 2048 atlas, URP dropping some.
  Turn shadow casting off on decorative lights first.
- Only Block 7 has a work site. The other four districts fall back to the placeholder ground by
  design.

**Why:** these are all "measured it, don't re-derive it" facts. Each cost a diagnostic round trip.

**How to apply:** read `docs/OVERMAP.md` before touching the overmap; tune lighting live with
**F1** in play mode (`TuningOverlay`), which has a preview scrub.

Related: [[project-ui-rebuild-status]] [[project-vendor-shader-patch]] [[project-asset-kit-status]]
