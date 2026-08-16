using System.Collections.Generic;
using NeoKyoto.Contracts;
using UnityEngine;

namespace NeoKyoto.World
{
    /// <summary>
    /// How the city's own lighting reacts to a repair. Lives on Bootstrap so it can be
    /// found live in play mode and then set in edit mode to keep it, the same way the
    /// audio mix, the splash timing and the overmap framing already work.
    ///
    /// Every value is read every frame, so dragging a slider mid-contract shows
    /// immediately. Use <see cref="previewOverride"/> to scrub the repair without
    /// actually writing any code.
    /// </summary>
    [System.Serializable]
    public class WorkSiteLightSettings
    {
        [Header("Preview — for tuning, not for play")]
        [Tooltip("Ignore the contract and drive the street from the slider below. Lets the " +
                 "broken and fixed ends be judged side by side without running twelve " +
                 "rebalances each time. Leave OFF for real play.")]
        public bool previewOverride;

        [Range(0f, 1f)]
        [Tooltip("0 = the moment the player arrives. 1 = node stable, block back on.")]
        public float previewFraction;

        [Header("How dark it gets")]
        [Range(0f, 1f)]
        [Tooltip("Share of the block that is fully out at load. Straight out of Voss's " +
                 "briefing — \"about a third of them have no light tonight\" — so this is " +
                 "copy-driven, and changing it means changing the copy too.")]
        public float darkShareAtLoad = 0.34f;

        [Range(0f, 1f)]
        [Tooltip("How dim the still-lit ones sit at load, before flicker. 1 = normal " +
                 "brightness. Starting value 0.7. Test: the street reads as browned-out " +
                 "rather than merely patchy.")]
        public float dimAtLoad = 0.7f;

        [Range(0.5f, 3f)]
        [Tooltip("Brightness once the node is stable, as a multiple of the street's normal " +
                 "level. 1 = exactly as the kit ships it. Above 1 makes the fix read as an " +
                 "improvement rather than a restoration — which is a claim about the story, " +
                 "so raise it deliberately.")]
        public float litLevel = 1f;

        [Header("Flicker")]
        [Range(0f, 1f)]
        [Tooltip("How hard the lit ones flicker at load, as a share of their brightness. " +
                 "Starting value 0.55. Test: reads as a failing supply from the kerb, not " +
                 "as a strobe. This is a photosensitivity surface — when in doubt, come down.")]
        public float flickerAmount = 0.55f;

        [Range(0.5f, 25f)]
        [Tooltip("Flicker speed. Starting value 9. Test: unsteady, not buzzing.")]
        public float flickerSpeed = 9f;

        [Header("Reach")]
        [Tooltip("Metres around the work site. 45 catches the street spots, the shopfronts " +
                 "and the upper-floor windows without reaching the next junction. Test: the " +
                 "affected area reads as \"this block\", not \"the district\". " +
                 "Changing this re-scans, which costs a frame — fine while tuning.")]
        public float radius = 45f;

        [Tooltip("Ignore anything below this height. The metro sits four metres under the " +
                 "pavement; its lights are inside a station nobody can see from the kerb.")]
        public float minHeight;
    }

    /// <summary>
    /// Drives the city's own lights from a contract's progress, so a repair shows up as
    /// the block coming back on rather than as a prop changing colour.
    ///
    /// This is `DESIGN_DIRECTION.md` principle 4 at the smallest scale: the world is the
    /// primary feedback channel. C1's briefing already promises it — *"about a third of
    /// them have no light tonight"*, and *"you'll know when it's stable. So will they."*
    /// A placeholder box on the pavement cannot pay that off; the street's actual lights can.
    ///
    /// Everything it touches is recorded and put back. These are the kit's own lights and
    /// materials, shared with the rest of the city, and leaving one dimmed would quietly
    /// darken that street for every later contract.
    /// </summary>
    public class WorkSiteLights : MonoBehaviour
    {
        private struct Captured
        {
            public Light Light;
            public float Intensity;   // what it was before we touched it
            public float Phase;       // so they do not all flicker in step
        }

        private struct CapturedGlow
        {
            public Renderer Renderer;

            /// <summary>
            /// One entry per material slot, black where that slot does not emit.
            /// Per-slot on purpose: a MaterialPropertyBlock set without a material index
            /// applies to the whole renderer, so pushing a neon sign's emission across one
            /// also lit every wall submesh it shared — which blew the street out to solid
            /// white the first time this ran.
            /// </summary>
            public Color[] Emission;
            public float Phase;
        }

        private readonly List<Captured> _lights = new List<Captured>();
        private readonly List<CapturedGlow> _glows = new List<CapturedGlow>();

        private Contract _contract;
        private MaterialPropertyBlock _block;
        private WorkSiteLightSettings _settings = new WorkSiteLightSettings();

        private Vector3 _centre;
        private float _capturedRadius = -1f;
        private float _capturedMinHeight = float.NaN;

        /// <summary>
        /// Emissive materials that are *not* on the block's supply. A dark taxi or a dark
        /// traffic light is not "the power is out", it is a different and more alarming
        /// message — and the metro runs on its own feed.
        /// </summary>
        private static readonly string[] NotOnThisSupply =
            { "CP_Taxi", "CP_Traffic_Light", "CP_Metro_Train", "CP_Metro_Signage" };

        public void Configure(Vector3 centre, Contract contract, WorkSiteLightSettings settings)
        {
            _centre = centre;
            _contract = contract;
            if (settings != null) _settings = settings;
            Capture();
        }

        // ─── Capture ───

        private void Capture()
        {
            Restore();
            _lights.Clear();
            _capturedRadius = _settings.radius;
            _capturedMinHeight = _settings.minHeight;

            var found = new List<KeyValuePair<float, Light>>();

            foreach (var light in FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if (light.type == LightType.Directional) continue;
                if (light.transform.position.y < _settings.minHeight) continue;
                if (light.GetComponentInParent<WorkSiteLights>() != null) continue;

                float d = PlanarDistance(light.transform.position);
                if (d > _settings.radius) continue;

                found.Add(new KeyValuePair<float, Light>(d, light));
            }

            // Nearest first. The dark ones are taken from the far end, so as the node
            // settles the darkness recedes outward from where the player is standing —
            // the fix reads as spreading from their hands rather than arriving everywhere.
            found.Sort((a, b) => a.Key.CompareTo(b.Key));

            for (int i = 0; i < found.Count; i++)
                _lights.Add(new Captured {
                    Light = found[i].Value,
                    Intensity = found[i].Value.intensity,
                    Phase = i * 7.13f,      // an irrational-ish stride, so no two share a beat
                });

            CaptureGlows();
        }

        private float PlanarDistance(Vector3 p)
        {
            return Vector2.Distance(new Vector2(p.x, p.z), new Vector2(_centre.x, _centre.z));
        }

        /// <summary>
        /// The emissive materials — neon, shopfronts, lit windows. These are what actually
        /// carry the look of a lit street: driving only the Light components moved the
        /// measured intensities but barely changed the frame, because the kit lights its
        /// city with emission and ambient rather than with lamps.
        ///
        /// Written through a MaterialPropertyBlock, never onto the material. These are
        /// shared assets — writing the material would darken every copy of that shopfront
        /// in Neo-Kyoto and would persist into the asset on disk.
        /// </summary>
        private void CaptureGlows()
        {
            _glows.Clear();
            var found = new List<KeyValuePair<float, Renderer>>();

            foreach (var r in FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
            {
                if (PlanarDistance(r.bounds.center) > _settings.radius) continue;

                bool emits = false, excluded = false;
                foreach (var m in r.sharedMaterials)
                {
                    if (m == null) continue;
                    foreach (var name in NotOnThisSupply)
                        if (m.name.StartsWith(name)) { excluded = true; break; }
                    if (excluded) break;

                    if (m.HasProperty("_EmissionColor")
                        && m.GetColor("_EmissionColor").maxColorComponent > 0.01f) emits = true;
                }
                if (excluded || !emits) continue;

                found.Add(new KeyValuePair<float, Renderer>(PlanarDistance(r.bounds.center), r));
            }

            found.Sort((a, b) => a.Key.CompareTo(b.Key));

            for (int i = 0; i < found.Count; i++)
            {
                var r = found[i].Value;
                var mats = r.sharedMaterials;
                var emission = new Color[mats.Length];

                for (int s = 0; s < mats.Length; s++)
                    emission[s] = (mats[s] != null && mats[s].HasProperty("_EmissionColor"))
                        ? mats[s].GetColor("_EmissionColor")
                        : Color.black;

                _glows.Add(new CapturedGlow { Renderer = r, Emission = emission, Phase = i * 3.77f });
            }
        }

        // ─── Drive ───

        private void LateUpdate()
        {
            if (_lights.Count == 0 && _glows.Count == 0) return;
            if (_block == null) _block = new MaterialPropertyBlock();

            // Re-scan if the reach was changed in the Inspector, so tuning radius is live
            // rather than needing the contract reopened.
            if (!Mathf.Approximately(_capturedRadius, _settings.radius) ||
                !Mathf.Approximately(_capturedMinHeight, _settings.minHeight))
            {
                Capture();
                return;
            }

            float f = _settings.previewOverride
                ? Mathf.Clamp01(_settings.previewFraction)
                : (_contract != null ? Mathf.Clamp01(_contract.ProgressFraction) : 1f);

            float darkShare = Mathf.Lerp(_settings.darkShareAtLoad, 0f, f);
            float level = Mathf.Lerp(_settings.dimAtLoad, _settings.litLevel, f);
            float flicker = Mathf.Lerp(_settings.flickerAmount, 0f, f);

            int litLights = Mathf.CeilToInt(_lights.Count * (1f - darkShare));
            for (int i = 0; i < _lights.Count; i++)
            {
                var c = _lights[i];
                if (c.Light == null) continue;
                c.Light.intensity = i >= litLights ? 0f : c.Intensity * Scale(level, flicker, c.Phase);
            }

            int litGlows = Mathf.CeilToInt(_glows.Count * (1f - darkShare));
            for (int i = 0; i < _glows.Count; i++)
            {
                var g = _glows[i];
                if (g.Renderer == null) continue;
                ApplyGlow(g, i >= litGlows ? 0f : Scale(level, flicker, g.Phase));
            }
        }

        /// <summary>
        /// Perlin rather than Random: it wanders instead of strobing, which is what a
        /// failing supply looks like and what a seizure risk does not.
        /// </summary>
        private float Scale(float level, float flicker, float phase)
        {
            if (flicker <= 0f) return level;
            float noise = Mathf.PerlinNoise(Time.time * _settings.flickerSpeed + phase, 0f);
            return level * (1f - flicker * noise);
        }

        /// <summary>
        /// Writes a glow's emission at some fraction of its original, per material slot.
        /// Slots that never emitted are left untouched — setting them at all would make
        /// walls and pavement glow.
        /// </summary>
        private void ApplyGlow(CapturedGlow g, float scale)
        {
            for (int s = 0; s < g.Emission.Length; s++)
            {
                if (g.Emission[s].maxColorComponent <= 0.01f) continue;
                g.Renderer.GetPropertyBlock(_block, s);
                _block.SetColor("_EmissionColor", g.Emission[s] * scale);
                g.Renderer.SetPropertyBlock(_block, s);
            }
        }

        /// <summary>Puts every light and every glow back exactly as it was found.</summary>
        public void Restore()
        {
            foreach (var c in _lights)
                if (c.Light != null) c.Light.intensity = c.Intensity;

            if (_block == null) _block = new MaterialPropertyBlock();
            foreach (var g in _glows)
                if (g.Renderer != null) ApplyGlow(g, 1f);
        }

        private void OnDisable() { Restore(); }
        private void OnDestroy() { Restore(); }
    }
}
