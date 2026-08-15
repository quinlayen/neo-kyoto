---
name: project-vendor-shader-patch
description: "AE/Grunge emission patch — required for the colour language, lives outside git, reverts on kit reimport"
metadata:
  type: project
---

## AE/Grunge Emission Patch (as of 2026-08-14)

The game's whole state language is broken-amber → fixed-cyan. Stock `AE/Grunge` **cannot glow**: it hardcodes `float3 Emission = 0;` in all three passes, and `_EmissionColor` is a dead Amplify artifact — declared `[HideInInspector]` but absent from every `CBUFFER`.

### The patch (applied, verified)

13 lines against `unity/neo-kyoto/Assets/Cyberpunk_Megapolis/Other/CP_Grunge.shader`:

| Count | Change |
|---|---|
| 1 | Property `[HideInInspector] … = (1,1,1,1)` → `[HDR] … = (0,0,0,1)` — visible, HDR, **defaults to black** |
| 9 | `float4 _EmissionColor;` as first member of every `CBUFFER_START(UnityPerMaterial)` |
| 3 | `float3 Emission = 0;` → `float3 Emission = _EmissionColor.rgb;` |

All nine CBUFFERs or none — the SRP Batcher needs identical layout across passes. Default black means nothing changes until something drives it.

### Acceptance test — PASSED

Sphere with no lights and black ambient, so brightness could only be emission:

| Condition | Sampled pixel | Luminance |
|---|---|---|
| Emission black | r0.000 g0.000 b0.000 | 0.000 |
| Emission cyan | r0.246 g1.258 b1.268 | 1.043 |
| `_Tint` red, emission off | r0.246 g0.000 b0.000 | — |

Emission writes **HDR** (>1.0, so bloom responds). `_Tint` is confirmed runtime-drivable by measurement — `material.SetColor("_Tint", c)`, and a `MaterialPropertyBlock` gives per-object tinting without instancing materials.

### ⚠ Two traps

1. **Reimporting the kit silently reverts the patch and emission dies.** Purchased kits are gitignored, so there is **no git safety net**.
2. Do **not** use the publisher's `URP_Grunge_Unity_6.0.shader` from the Google Drive zip. It is an older Amplify build (1.9.8.1, ten CBUFFERs) than the kit's shipped shader (1.9.9.4, nine). Both declare `Shader "AE/Grunge"` and Unity resolves by that **name**, not filename — two files means it picks one arbitrarily and the fix silently appears not to work. Overwrite, never add alongside.

### ⚠ Machine-local, outside git

Pristine original, patched drop-in, the rescued vendor shader zip contents and a full README live at:

```
D:\assets-staging\ae-shaders-unity6\
```

This is the **only** copy — the vendor's source zip (a Google Drive drop) is gone, and the extracted tree was rescued from volatile `%TEMP%` on the drive that hit 0 bytes free. Re-obtaining it means contacting `art_equilibrium.studio@mail.ru`.

**How to apply:** if emission stops working, or materials come back pink after a reimport, copy `D:\assets-staging\ae-shaders-unity6\DROP-IN\CP_Grunge.shader` over the kit's copy under that exact filename. Do not re-derive from scratch.

Related: [[project-asset-kit-status]] [[project-unity-setup]]
