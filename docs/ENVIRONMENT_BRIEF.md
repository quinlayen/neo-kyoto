# Environment Brief: Locations & Asset Kit Requirements

**Date**: 2026-08-14
**Status**: Kit **purchased, imported and verified**. Sections below written *before* purchase are kept for the reasoning trail — where they conflict with **Post-Purchase Verification**, that section wins
**Companion**: `ONSITE_PIVOT.md` (view model, the deck, the plug-in moment)

Design here is live. Where this conflicts with the GDD, this document reflects the newer thinking. Both change as the game finds its shape.

---

## Three Decisions That Drive Everything Below

| Decision | Choice |
|----------|--------|
| **Platform** | **PC native primary.** WebGL is an occasional convenience build for sharing with a playtester — best-effort, never a design constraint. Fidelity, bundle size and poly budget are no longer gating. |
| **Locations** | **Exterior-first.** Contracts are reworked so the player almost always works outdoors. Interiors are an Act 3 purchase, not a launch blocker. |
| **Embodiment** | Fixed-camera dioramas now, architected so a district can become first-person walkable later without anything else changing. |

---

## The Reframe: The Contractor Works From The Outside

You are freelance. Nobody hands you a badge to walk into a corporate server hall. You get dispatched to the **access point** — the street-level junction box, the utility vault under the pavement, the external maintenance panel bolted to the side of the building, the cabinet at the base of the tower. You squat on the kerb in the rain, pop the housing, and plug in.

This is not a budget compromise dressed up as fiction. It is better characterisation than the alternative, and it does three things at once:

1. It fits an exterior-heavy asset kit natively.
2. It's truer to `GDD.md:17` — *"You are not a hacker or a hero. You are the person who shows up."* Executive access to a data centre is a hero's privilege. A kerbside junction box is a tradesperson's reality.
3. **It gives Act 3 a real threshold.** Field work stops meaning "you go on site" — you always did — and starts meaning *you finally get inside*. Corporate HQ. The sealed data centre. The interior art spend arrives exactly when the story has earned it.

---

## Location Table (Exterior-First)

| # | Contract | Location | Exterior framing | Set |
|---|----------|----------|------------------|-----|
| C1 | Keep the Lights On | Block 7 | Street substation; pavement junction box, conduits running up the building face | **A** |
| C2 | Drone Route Cleanup | Sector 12 | Rooftop or street-level drone pads, path lights overhead | **B** |
| C3 | Drone Dispatch | Sector 14 | Same depot; grounded drones dark on their pads | **B** |
| C4 | Signal Interference | Transit Hub | Elevated metro platform + trackside signal cabinet | **C** |
| C5 | System Recovery | Data Center | External access vault — alley, racks behind a grille, rain | **D** |
| C6 | Log Analysis | Network Ops | Street-level relay cabinet / NOC access panel | **D** |
| C7 | Server Migration | Server Farm | Loading dock; crated racks stacked outside mid-migration | **D** |
| C8 | Grid Restoration | Central Grid | Elevated overlook above the district | **E** |
| C9 | Process Lockdown | Comms Tower | Tower base equipment cabinet, antenna array overhead | **C** |
| C10 | Water Treatment | Undercity Plant | **Undercity level** — exterior street beneath the elevated city, open pipework and tanks | **F** |
| C11 | Sector Sweep | Industrial Zone | Factory yard; exterior conveyor lines and production gantries | **F** |

**11 of 11 exterior.** C10's move to an undercity level is worth calling out: a lower street deck beneath the elevated city is a stock cyberpunk environment, reads as "underground" without being an interior, and most megapolis kits ship undercity/canal/lower-level pieces.

### Consolidation: 11 contracts → 6 sets

| Set | Covers | Redress strategy |
|-----|--------|------------------|
| **A** | C1 | Standalone. First location the player ever sees — highest polish priority in the game |
| **B** | C2, C3 | One depot. C3 kills the path lights and puts drones dark on pads |
| **C** | C4, C9 | Shared "infrastructure with a signal cabinet" vocabulary. Different silhouette, same prop kit |
| **D** | C5, C6, C7 | One alley/service-access set, three dressings. C5 = one live rack behind a grille. C6 = relay cabinet, wall of status lights. C7 = loading dock, half the racks crated |
| **E** | C8 | Reuses A/B/C/D as the *view*, not as playable space. Cheapest set in the game |
| **F** | C10, C11 | One industrial shell. C10 = undercity, pipes, wet, dripping. C11 = factory yard, conveyors, dry, sparks |

**Demo build order (C1–C5): A → D → B.** Three sets covers five contracts.

---

## Candidate Kit: Cyberpunk Megapolis (Art Equilibrium)

Listed in two places, and **the difference matters**:

| | Unity Asset Store | Fab |
|---|---|---|
| URL | `.../cyberpunk-megapolis-376952` | `fab.com/listings/308aa2a1-…` |
| **Included formats** | Unity package | **Unreal Engine only** |
| Price / size | $44.99 / 2.7 GB | not shown when logged out |
| Pipelines | Built-in, URP and HDRP | n/a |
| Unity version | 2022.3.62 — matches our LTS | n/a |
| Traction | 24 ratings | 1 rating (5.0) |

**Buy the Unity Asset Store version.** The Fab listing ships Unreal format only — no Unity package, no raw FBX.

### Confirmed contents (from the Fab description, which actually rendered)

- Preassembled buildings **and** modular parts: facades, walls, rooftops, streets, roads, **elevated structures**, city blocks, background buildings
- Props: street objects, signs, vending machines, **cables**, lights, barriers, trash, decals, vehicles
- **"Some buildings include interior modules, allowing you to create simple interior spaces"**
- LODs "included where needed"
- "Game-ready texel density for both close-up and background use"
- Tags: Metro, Train, Car, Neon, Skyscraper, Modular, City

### Read

Three things land well. **Metro and train** are a direct hit for Set C. **Elevated structures and elevated roads** imply an under-level, which is exactly what Set F's undercity needs. And the claim of texel density for close-up use is the right claim for walkable-later — though it is a claim, not evidence.

Two gaps against our plan. The prop list has cables, barriers, signs and vending machines but **no junction boxes, access panels, or utility cabinets** — the connection point remains unsourced, and it is the single most important prop in the game. And "simple interior spaces" is a partial answer for Act 3 at best.

> **✅ First gap closed.** The listing undersold itself: `CP_Electric_Charging_01/02` is a 1.65 m kerbside unit with an emissive display panel. See **Post-Purchase Verification**.

### ⚠ Confirmed: generated with AI

Fab's metadata states plainly: **`Generated with AI: Yes`**. The Unity listing carries the same disclosure; Fab is where it's legible.

This is not disqualifying, but it bears directly on our two hard requirements. AI-generated environment kits fail most often at precisely the things we need:

| Our must-have | Why AI generation threatens it |
|---|---|
| Real-world scale | Scale consistency *between* pieces is a common failure; looks fine in a hero shot, wrong at eye height |
| Closed, thick-walled geometry | Non-manifold, open, or single-sided meshes are frequent; fine for a camera that never goes inside |
| Consistent modular grid | Generated modules often don't snap cleanly to one unit |
| Collision-ready | Messy topology makes auto-generated collision unreliable |

**The gallery checklist below is therefore not optional.** Scale consistency and close-up quality are the two failure modes most likely here, and they are exactly the two things that decide whether this kit survives the walkable phase.

### Publisher README findings

The Art Equilibrium setup guide is **boilerplate across their catalogue** — its screenshots are from their Miami Beach asset (`MB_` material prefixes) and the folder tree references `Readme_Havana_Street.pdf`. It contains no Megapolis-specific specs. It does, however, settle three things.

**✅ Corroborates walkable.** The project tree shows **`StarterAssets`** (Unity's official first/third-person character controller) in the Assets root, and §7 documents a door script whose first setup step is *"Assign the Player tag to your character object."* A kit shipping a character controller and interactive doors is built to be walked around in — structural evidence, stronger than a video.

**✅ Contents manifest.** Prefab folders: `Metro` (Set C), `Street`, `Modules`, `Facade_Details`, `Decals`, `Combined_Building`, `Background` (Set E), `Car`, `Environment`.

**✅ Genuine URP support** — separate UnityPackages per pipeline, not a single "compatible" build.

### AE/Grunge: source-level analysis

Read directly from the publisher's Unity 6.0 shader set (`URP_Grunge_Unity_6.0.shader`, Amplify Shader Editor v1.9.8.1 output, 159 KB). This is **confirmed from source**, not inferred.

**❌ Emission is hardcoded off.** All three passes contain:

```hlsl
float3 Emission = 0;          // lines 648 forward, 1859 meta, 3056 gbuffer
surfaceData.emission = Emission,
```

`_EmissionColor` is declared as a property (line 7) but is `[HideInInspector]` and **absent from all ten `CBUFFER_START(UnityPerMaterial)` blocks** — a dead Amplify artifact, never read. The HDRP variant is identical (3 sites).

**✅ `_Tint` is real and runtime-drivable.** Line 646:

```hlsl
float3 BaseColor = ( ( _Tint * tex2D( _Base_Color, texCoord94 ) ) * lerpResult108 ).rgb;
```

`_Tint` is a straight multiply into albedo and **is** in the CBUFFER (line 325). So `material.SetColor("_Tint", c)` works, and a `MaterialPropertyBlock` gives per-object tinting without instancing materials.

**Consequence for the colour language:** the broken-amber → fixed-cyan sweep works on bulk city surfaces *today*, unmodified. It's a tint rather than a glow, but paired with real lights it reads correctly. This is the C8 grid-restoration payoff shot, and it survives without emission.

**✅ Emission is a four-line fix.** Because the property already exists:

1. Remove `[HideInInspector]` from line 7
2. Add `float4 _EmissionColor;` to the CBUFFER blocks feeding the three passes
3. Change all three `float3 Emission = 0;` → `float3 Emission = _EmissionColor.rgb;`
4. Optionally mask by the grunge or metallic channel so only intended areas glow

That turns the main city shader fully emissive on every surface, rather than restricting the colour language to bespoke hero materials.

**Caveat:** the analysed zip is the **Unity 6.0** fix set. Megapolis was authored against 2022.3.62, so the shader shipped in the package may differ. The patch shape holds; line numbers will not.

**Revised risk:** downgraded from significant to minor. Tint covers the bulk case unmodified, and the emission gap has a known, cheap remedy.

> **✅ Resolved on import.** The caveat was right — the shipped shader *did* differ. See **Post-Purchase Verification**: the patch is 13 lines against `CP_Grunge.shader` (Amplify 1.9.9.4, nine CBUFFERs), not 14 against the vendor zip's file (1.9.8.1, ten). It has been applied and **passed its acceptance test**. Do not use the vendor zip's shader — it is an older build.

### ⚠ Maintenance signal

README §6, "Pink Material / Shader Fix", documents that the publisher's shaders break on Unity API changes and the remedy is **downloading replacement shaders from Google Drive** — not a package update. Two manual repair procedures are given, including deleting the broken shader and reassigning it across all affected materials. A known, recurring, unpatched breakage with an out-of-band fix. Not fatal at $45, but indicative of the support level.

### Still unverified

Poly counts, texture resolutions, modular grid unit, real-world scale, collision meshes, unique mesh count. Absent from both listings and from the README. (Comparative shopping against alternative kits was not possible — web search is unavailable in this environment; a 300-listing Fab sweep found no competitive Unity alternative.)

> **✅ Now measured.** Everything in this list except modular grid unit was resolved by direct measurement after import. See **Post-Purchase Verification**.

### Publisher videos

| Video | Engine | Length | Views |
|---|---|---|---|
| `b0LNB4LKfDc` Cyberpunk Megapolis **Unity** | Unity | 1:19 | 255 |
| `vThTp7R0kGs` Cyberpunk Megapolis UE | Unreal | 1:11 | 130 |
| `_WB_F9S46q0` Environment **Walkthrough** UE | Unreal | 2:18 | 135 |

A Unity-specific showcase exists, so the Unity package is not merely an unverified port. Note that the **detailed walkthrough is the Unreal one** — the Unity video is promo-length. Judgements about ground-level quality should be traced back to which video they came from.

None of the three has captions, so no automated inspection is possible; assessment requires watching, or screenshots.

### Verify before buying — watch the Environment Walkthrough video first

The Fab listing has a **promo video and an environment walkthrough video**. The walkthrough is worth more than all 29 stills combined, because a moving camera exposes exactly what an AI-generated kit hides.

Watch for:

1. **Does the camera ever drop to eye height (~1.7m)?** If it stays aerial or sweeping, the kit was built to be seen from distance and will fall apart at 2m.
2. **Scale consistency between pieces.** Compare doors, railings, steps, vehicles across different buildings. Inconsistent human-scale reference is the most common AI-kit tell and the hardest to fix.
3. **Close-ups.** First-person lives at arm's length; distance-built kits hide poor normals there.
4. **A "prefab spread" shot** — modules laid out separately on a grey floor. Single best evidence of genuine modularity versus a handful of pre-built hero streets.
5. **Flat modular pavement, or one sculpted city mesh?** Sculpted can't be rearranged.
6. **Do the elevated roads have a usable under-level?** That's Set F.
7. **Interior modules** — how simple is "simple"?

### Ask the publisher

`art_equilibrium.studio@mail.ru`

> "What is the modular grid unit? Is the kit built to real-world scale (door and storey heights)? Are collision meshes included? Do the elevated road sections have a usable lower level?"

Four questions, all decisive. A publisher with one Fab rating will usually answer within a day.

### Verdict

**Low risk for phase 1, unproven for phase 2.** At $44.99 this is cheap enough to buy purely to validate the on-site feel across three diorama sets — that alone is worth the money and the fixed cameras will hide most of what could be wrong with it. What it has *not* demonstrated is that it survives a first-person camera later. Buy it with that split in mind, and treat a future walkable phase as possibly needing a different foundation.

> **Superseded.** The phase-2 doubt was largely wrong. Props and signage measure at walkable grade; only building facades don't. See below.

---

## Post-Purchase Verification (2026-08-14)

Kit purchased, imported into Unity 6000.5.8f1 / URP 17.5.0, and measured directly in `CP_Demo`. Everything here is measured, not claimed.

### Import: the base import silently installs the wrong pipeline

**This is the single most important operational note in this document.** Importing the kit from the Asset Store installs the **Built-In** variant — `CP_Grunge.shader` arrives as `#pragma surface surf Standard` with zero `CBUFFER_START`, and materials sit on Unity's Standard shader. In a URP project that renders **pink**.

The fix is not the publisher's Google Drive shader set. It is `Cyberpunk_Megapolis_URP.unitypackage`, shipped *inside* the imported kit folder, which must be run as a **separate second step**. Missing it is what makes the kit look broken on arrival, and it is easy to miss.

This also reframes the maintenance signal above: the Google Drive shader route was never needed here.

### Emission: patched and passing

The pre-purchase source analysis was correct in substance and wrong in detail. The shipped URP shader is `CP_Grunge.shader` (Amplify **1.9.9.4**, **nine** CBUFFER blocks) — not the vendor zip's `URP_Grunge_Unity_6.0.shader` (1.9.8.1, ten). Both declare `Shader "AE/Grunge"` and must never coexist; Unity resolves by shader name, not filename.

Patch is **13 lines**: 1 property (`[HideInInspector]`→`[HDR]`, default black), 9 CBUFFER declarations, 3 emission sites.

Acceptance test — sphere lit only by emission, no lights, black ambient:

| Condition | Sampled pixel | Luminance |
|---|---|---|
| Emission black | r0.000 g0.000 b0.000 | 0.000 |
| Emission cyan | r0.246 g1.258 b1.268 | **1.043** |
| `_Tint` red, emission off | r0.246 g0.000 b0.000 | — |

**Emission works and writes HDR** (>1.0, so bloom responds). **`_Tint` confirmed runtime-drivable by measurement**, not just from source. The broken-amber → fixed-cyan language is viable through either channel.

⚠ **The patch lives in a gitignored vendor file. Reimporting the kit silently reverts it and emission dies.** Pristine and patched copies are staged at `D:\assets-staging\ae-shaders-unity6\`.

### Texel density: strong where it matters, weak where it doesn't

| Surface class | px/m | Verdict |
|---|---|---|
| Kerbside props (charging units, vending) | **1240–1265** | Walkable grade |
| Small signage (metro, shop, road) | **800–2470** | Walkable grade |
| Ground (sidewalk, asphalt) | **256** | Good |
| Large signboards (skyscraper) | 122–340 | Fine — distance-viewed |
| **Building facades (concrete)** | **68–120** | **Soft. Background only** |

242 textures, 236 at 2048² and 5 at 4096² (metro train) — unusually uniform authoring.

The listing's "game-ready texel density for both close-up and background use" is **half true**: true for props and signage, false for facades. This maps almost perfectly onto the on-site pivot, which puts the player at a kerbside cabinet with buildings as backdrop. The weak class never lands where the eye rests.

**Implication for walkable-later:** the phase-2 doubt narrows to one problem. A first-person camera is fine among props and signage; it degrades against building walls. That is a facade-retexture problem, not a re-buy.

### Scale: sound

| Reference | Measured | Expected |
|---|---|---|
| Train doors | **2.17 m × 0.90 m** | 2.0–2.1 m — textbook |
| Metro station doors | 2.93 m | Oversized, but correct for transit |

The most-feared AI-kit failure mode did not materialise. Hard-checklist scale requirement **passes**.

### The connection point is no longer unsourced

The pre-purchase read flagged the absence of junction boxes as the kit's worst gap — *"the single most important prop in the game"*. The kit ships **`CP_Electric_Charging_01/02`**: a **1.65 m kerbside unit with a working emissive display panel**, at ~1240 px/m, legible at 1.15 m viewing distance (Japanese UI text, voltage readouts and hazard decals all read cleanly).

That is very nearly the Set A junction box already. The separate Fab props purchase drops from necessary to optional — evaluate this prop before spending, and keep the free utility-box pack as a fallback for silhouette variety.

### Scene scale and LODs

`CP_Demo`: 4,840 GameObjects, 7.1M verts, 4.5M tris, 3,963 renderers, **2,754 LODGroups**, 69 lights, 2,191 colliders. LODs and collision are present and extensive — both "strongly want" items satisfied.

### Known vendor defect (harmless)

`CP_High_Renderer` and `CP_High_ScreenRenderer` both reference a renderer feature script `GlobalVolumeFeature` (guid `a0ec52cecc795714f93f274c2e71e87b`) that ships nowhere. Console errors on import; zero compile errors. Neither the project pipeline nor `CP_Demo` references those assets, so it is cosmetic. For the kit's post-processing look, apply `CP_HighQualityVolumeProfile` via a normal Global Volume GameObject.

### Revised verdict

**Keep the kit. It clears phase 1 outright and most of phase 2.** Scale is correct, LODs and collision are there, emission works after a known 13-line patch, and the hero connection-point prop turned out to be included. The one real limitation is facade texel density, which the fixed-camera diorama design never exposes and a future walkable phase could address by retexturing facades alone.

Two things still unmeasured: **modular grid unit**, and whether the demo scene's geometry is genuinely modular versus pre-assembled hero streets. Both matter for redressing six sets, neither blocks the demo build order.

---

## Marketplace Notes

### Fab is the wrong store for the city kit

A sweep of 300 unique Fab listings across ten cyberpunk/sci-fi/modular-city queries:

| Format availability | Count |
|---|---|
| **Unreal-only** (unusable for us) | **186** |
| FBX / OBJ / glTF (importable, no prefabs or materials) | 91 |
| **Native Unity package** | **14** |

Of those 14 native-Unity listings, none is a competitive alternative to Megapolis — they are low-poly packs, single interiors, or audio. The FBX pool is mostly zero-review single models rather than game-ready modular kits.

**Conclusion:** Fab is Epic's store and is overwhelmingly Unreal. Shop for the *city kit* on the Unity Asset Store.

### But Fab is the right store for props — and it solves the connection point

The single most important prop in the game — the port the player's cable goes into — is abundant on Fab in FBX, at **$0–$2**:

| Asset | Price | Formats |
|---|---|---|
| Retro Urban Utility Box Pack | **Free** | blender, fbx, obj |
| Modular Industrial Electrical Switchboard | **Free** | fbx |
| Junction box for wiring | **Free** | fbx |
| Vintage Industrial Electrical Cabinet | **Free** | fbx |
| Industrial Electrical Utility Cabinet Set | $1.99 | blender, fbx, glb |
| Electrical Utility Cabinet Box | $1.99 | fbx |

FBX is a much smaller handicap for a single hero prop than for a modular kit: you lose material setup, but a hero prop wants bespoke materials anyway. **Budget roughly nothing for the connection point.** Pick one, author its materials to match the city kit, and treat it as a signature object — the player will look at it hundreds of times.

`Cyberpunk street terminal` ($9.99, fbx/blender/obj) is also worth a look as a deck or wall-terminal reference.

### Research workflow for future kits

The Unity Asset Store cannot be surveyed programmatically here — its search API is CSRF-protected and listing pages lazy-load their technical details, so a fetched page yields only marketing boilerplate.

**Workable process:** browse candidates on the Unity Asset Store, then check whether the same asset has a **Fab twin** (many publishers list both, as Art Equilibrium did). Fab's listing JSON exposes what Unity's hides — unique mesh counts, collision, LODs, vertex ranges, texture resolutions, and the AI-generated flag. That's how the AI disclosure on Megapolis was confirmed.

**Caveat on this survey:** ten queries against one marketplace is not an exhaustive review of the field. Web search is blocked by org policy in this environment (`constraints/vertexai.allowedPartnerModelFeatures`), so no broader comparison was possible.

---

## Kit Requirements (Hard Checklist)

Because we chose *diorama now, walkable later*, the kit must be walkable-grade from day one. A diorama-grade kit means re-buying later.

Status against Megapolis as imported. Unticked items are unmeasured, not failed.

### Must have

- [x] **Real-world scale.** Doorways ~2.0–2.1m, pavements ≥1.5m, storey height ~3m. Stylised kits with 3m doors read fine in a fixed shot and feel wrong the instant you're in first person. — *measured 2.17 m × 0.90 m*
- [ ] **Closed, thick-walled geometry.** No single-sided planes, no hollow facade shells.
- [ ] **Consistent modular grid.** Pieces snap on one unit (1m / 2m / 4m). Mixed-grid kits cost days. — *still unmeasured*
- [x] **Emissive channels drivable at runtime.** Colour is our primary state signal — broken = warm/orange, fixed = cool/cyan. Baked-only emissives kill the entire visual language. — *after the 13-line patch; also `_Tint` unmodified*

### Strongly want

- [x] **Collision meshes**, or geometry clean enough to auto-generate. — *2,191 colliders in `CP_Demo`*
- [x] **Street-level service props**: junction boxes, wall cabinets, cable runs, conduit, grilles, access panels. **The single most important prop in the game is a connection point** — a port the player's cable physically goes into. Cityscape kits rarely include these; budget a separate industrial/props pack. — *`CP_Electric_Charging_01/02`; separate pack now optional*
- [ ] **Undercity or lower-level modules** for Set F.
- [ ] **Wet/rain-capable surfaces.** Half these locations want rain, and it's the cheapest atmosphere multiplier available.
- [x] **LODs** — still worth it for framerate, no longer existential. — *2,754 LODGroups*

### Deal-breakers

- Baked lighting only, no runtime-drivable emissives
- Single-sided geometry
- A pipeline that doesn't match the other kits you buy

### Pipeline

URP remains the pick — not for WebGL any more, but because **most cyberpunk kits ship URP** and mixing pipelines across two or three purchases is the expensive mistake. HDRP is technically viable on PC native and would look better, but only if *every* kit supports it. Choose one and filter all purchases by it.

---

## Camera Authoring Rule

Floating deck windows occlude the world. The world is our primary feedback channel. These fight.

**Rule:** every location's hero camera must compose the failing system inside a **protected focal region** that windows never spawn into and snap-arrange avoids.

- **Starting value:** protected region = the **right 35%** of frame; windows spawn left-of-centre.
- **Test:** during a RUN, can an observer watch the system respond without moving a window? Pass at 8/10.
- **If it fails:** widen to 45% before considering auto-hiding windows during execution. Auto-hide is a last resort — it steals control, and Response outranks Clarity.

Framing is therefore a design constraint on every set, not a per-location art decision made afterwards.

---

## Fidelity: Reopened

`GDD.md:240-242` specifies low-poly flat/cel-shaded as the *starting* style with an "upgrade path" to higher fidelity later. That hedge existed largely to protect the WebGL build. With PC native as the target the hedge costs more than it saves — an upgrade path means building the game twice.

**Recommendation:** buy at the fidelity you want the finished game to have and skip the migration. The kit sets the look; the art direction document follows the kit.

One thing to preserve from the low-poly direction regardless of fidelity: **strong silhouettes and few readable systems per location.** That was never a technical constraint. It's what makes a system's state legible at a glance, and it matters *more* at high fidelity, because detail competes with signal.

---

## Scope Risk

**Field work was deliberately late-game.** `GDD.md:555` scheduled on-site work for Act 3, partly to defer environment cost. Moving it to C1 front-loads that cost. The six-set consolidation is the mitigation — without it this is eleven bespoke environments for a prototype. Hold the line on redressing.

A secondary practical note: at 2.7 GB per kit, three purchases is ~8 GB of source assets. Fine for PC native, but expect slow imports and a heavy `Library/`.
