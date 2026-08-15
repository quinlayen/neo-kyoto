---
name: project-asset-kit-status
description: "Cyberpunk Megapolis purchased, imported and verified — emission test passed, kit is a keeper"
metadata:
  type: project
---

## Asset Kit Status (as of 2026-08-14)

**The refund gate is cleared. Keep the kit.** Verified on the real import, Unity 6000.5.8f1 / URP 17.5.0.

- **Kit**: Cyberpunk Megapolis (Art Equilibrium), $44.99, Unity Asset Store version
- **Also imported**: `Rolling_Balls-Sci-fi_Pack` (the sphere pack, intended to replace the C1 power node primitive) and `Cyber_Box`

### The import gotcha that cost real time

The base Asset Store import silently installs the **Built-In** variant — materials land on Unity's Standard shader and render **pink** in URP. The fix is **not** the publisher's Google Drive shader set. It is `Cyberpunk_Megapolis_URP.unitypackage`, shipped *inside* the imported kit folder, which must be run as a **separate second step**. Easy to miss; missing it is what makes the kit look broken.

### Measured quality (not vendor claims)

| Surface class | px/m |
|---|---|
| Kerbside props, small signage | 800–2470 — walkable grade |
| Ground (sidewalk, asphalt) | 256 |
| **Building facades** | **68–120 — soft, background only** |

Scale is correct: train doors measure 2.17 m × 0.90 m. LODs (2,754 LODGroups) and collision (2,191 colliders) are present. The listing's "texel density for close-up and background" is half true — true for props, false for facades. That maps well onto the on-site pivot, which never puts the eye on a facade.

### The connection point is no longer unsourced

`CP_Electric_Charging_01/02` is a 1.65 m kerbside unit **with a working emissive display panel**, legible at 1.15 m. That is very nearly the Set A junction box already, so the separate Fab props purchase dropped from necessary to optional.

### Known vendor defect (harmless)

`CP_High_Renderer` and `CP_High_ScreenRenderer` reference a `GlobalVolumeFeature` script that ships nowhere — console errors on import, zero compile errors. Nothing references those renderer assets. For the kit's look, use a normal Global Volume with `CP_HighQualityVolumeProfile`.

**How to apply:** full measured detail lives in `docs/ENVIRONMENT_BRIEF.md` under *Post-Purchase Verification*, which supersedes the pre-purchase reasoning above it in that file. Emission only works because of a patch — see [[project-vendor-shader-patch]] before assuming it still applies.

Related: [[project-vendor-shader-patch]] [[project-unity-setup]] [[project-unity-decisions]]
