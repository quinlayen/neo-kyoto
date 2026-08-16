using System.Collections.Generic;
using NeoKyoto.Contracts;
using UnityEngine;

namespace NeoKyoto.World
{
    /// <summary>
    /// Drives the city's own lights from a contract's progress, so a repair shows up as
    /// the block coming back on rather than as a prop changing colour.
    ///
    /// This is `DESIGN_DIRECTION.md` principle 4 at the smallest scale: the world is the
    /// primary feedback channel. C1's briefing already promises it — *"about a third of
    /// them have no light tonight"*, and *"you'll know when it's stable. So will they."*
    /// A placeholder box on the pavement cannot pay that off; the street's actual lights can.
    ///
    /// Every light it touches is recorded and put back. These are the kit's lights, shared
    /// with the rest of the city, and leaving one dimmed would quietly darken that street
    /// for every later contract.
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
            /// applies to the whole renderer, so pushing a neon sign's emission onto a
            /// renderer also lit every wall submesh it shared — which blew the street out
            /// to white the first time this ran.
            /// </summary>
            public Color[] Emission;
            public float Phase;
        }

        private readonly List<Captured> _lights = new List<Captured>();
        private readonly List<CapturedGlow> _glows = new List<CapturedGlow>();
        private Contract _contract;
        private MaterialPropertyBlock _block;

        /// <summary>
        /// Emissive materials that are *not* on the block's supply. A dark taxi or a dark
        /// traffic light is not "the power is out", it is a different and more alarming
        /// message — and the metro runs on its own feed.
        /// </summary>
        private static readonly string[] NotOnThisSupply =
            { "CP_Taxi", "CP_Traffic_Light", "CP_Metro_Train", "CP_Metro_Signage" };

        // ─── Starting values, all with their test ───

        /// <summary>
        /// Metres around the work site. 45 m catches the street spots, the shopfront
        /// points and the upper-floor window lights at y=17.5 without reaching the next
        /// junction. Test: the lit area reads as "this block" and not "the whole district".
        /// </summary>
        private const float Radius = 45f;

        /// <summary>
        /// Below this is the metro, four metres under the pavement. Its lights are inside
        /// a station nobody can see from the kerb, and flickering them is wasted work.
        /// </summary>
        private const float MinHeight = 0f;

        /// <summary>
        /// Share of the block that is dark at load. Straight out of Voss's briefing —
        /// "about a third of them have no light tonight" — so this is copy-driven rather
        /// than a tuning value, and it should change only if the copy does.
        /// </summary>
        private const float DarkShareAtStart = 0.34f;

        /// <summary>
        /// How hard the still-lit ones flicker before any work is done, as a share of
        /// their normal brightness. Starting value 0.55. Test: reads as a failing supply
        /// from the kerb, not as a strobe. Too high and it becomes a hazard for
        /// photosensitivity; if in doubt, come down.
        /// </summary>
        private const float FlickerAtStart = 0.55f;

        /// <summary>Flicker speed. Starting value 9. Test: unsteady, not buzzing.</summary>
        private const float FlickerSpeed = 9f;

        public void Configure(Vector3 centre, Contract contract)
        {
            _contract = contract;
            Capture(centre);
        }

        private void Capture(Vector3 centre)
        {
            Restore();
            _lights.Clear();
            if (_contract == null) return;

            var found = new List<KeyValuePair<float, Light>>();

            foreach (var light in FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if (light.type == LightType.Directional) continue;
                if (light.transform.position.y < MinHeight) continue;

                var p = light.transform.position;
                float d = Vector2.Distance(new Vector2(p.x, p.z), new Vector2(centre.x, centre.z));
                if (d > Radius) continue;

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

            CaptureGlows(centre);
        }

        /// <summary>
        /// The emissive materials — neon, shopfronts, lit windows. These are what actually
        /// carry the look of a lit street: driving only the Light components moved the
        /// measured intensities but barely changed the frame, because the kit lights its
        /// city with emission and ambient rather than with lamps.
        ///
        /// Written through a MaterialPropertyBlock, never onto the material. These are
        /// shared assets — writing to the material would darken every copy of that shopfront
        /// in Neo-Kyoto and would persist into the asset on disk.
        /// </summary>
        private void CaptureGlows(Vector3 centre)
        {
            _glows.Clear();
            var found = new List<KeyValuePair<float, Renderer>>();

            foreach (var r in FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
            {
                var c = r.bounds.center;
                float d = Vector2.Distance(new Vector2(c.x, c.z), new Vector2(centre.x, centre.z));
                if (d > Radius) continue;

                Color emission = Color.black;
                bool skip = false;
                foreach (var m in r.sharedMaterials)
                {
                    if (m == null) continue;
                    foreach (var excluded in NotOnThisSupply)
                        if (m.name.StartsWith(excluded)) { skip = true; break; }
                    if (skip) break;

                    if (!m.HasProperty("_EmissionColor")) continue;
                    var e = m.GetColor("_EmissionColor");
                    if (e.maxColorComponent > emission.maxColorComponent) emission = e;
                }
                if (skip || emission.maxColorComponent <= 0.01f) continue;

                found.Add(new KeyValuePair<float, Renderer>(d, r));
            }

            found.Sort((a, b) => a.Key.CompareTo(b.Key));

            for (int i = 0; i < found.Count; i++)
            {
                var r = found[i].Value;
                var mats = r.sharedMaterials;
                var emission = new Color[mats.Length];

                for (int s = 0; s < mats.Length; s++)
                {
                    var m = mats[s];
                    emission[s] = (m != null && m.HasProperty("_EmissionColor"))
                        ? m.GetColor("_EmissionColor")
                        : Color.black;
                }

                _glows.Add(new CapturedGlow { Renderer = r, Emission = emission, Phase = i * 3.77f });
            }
        }

        private void LateUpdate()
        {
            if (_contract == null) return;
            if (_lights.Count == 0 && _glows.Count == 0) return;
            if (_block == null) _block = new MaterialPropertyBlock();

            float f = Mathf.Clamp01(_contract.ProgressFraction);
            float darkShare = Mathf.Lerp(DarkShareAtStart, 0f, f);
            float flicker = Mathf.Lerp(FlickerAtStart, 0f, f);

            int litLights = Mathf.CeilToInt(_lights.Count * (1f - darkShare));
            for (int i = 0; i < _lights.Count; i++)
            {
                var c = _lights[i];
                if (c.Light == null) continue;
                if (i >= litLights) { c.Light.intensity = 0f; continue; }

                // Perlin rather than Random: it wanders instead of strobing, which is what
                // a failing supply looks like and what a seizure risk does not.
                float noise = Mathf.PerlinNoise(Time.time * FlickerSpeed + c.Phase, 0f);
                c.Light.intensity = c.Intensity * (1f - flicker * noise);
            }

            int litGlows = Mathf.CeilToInt(_glows.Count * (1f - darkShare));
            for (int i = 0; i < _glows.Count; i++)
            {
                var g = _glows[i];
                if (g.Renderer == null) continue;

                float scale = 0f;
                if (i < litGlows)
                {
                    float noise = Mathf.PerlinNoise(Time.time * FlickerSpeed + g.Phase, 0f);
                    scale = 1f - flicker * noise;
                }

                ApplyGlow(g, scale);
            }
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
