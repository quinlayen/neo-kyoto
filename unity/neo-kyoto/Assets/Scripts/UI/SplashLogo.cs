using System.Collections.Generic;
using NeoKyoto.Core;
using UnityEngine;
using UnityEngine.UI;

namespace NeoKyoto.UI
{
    /// <summary>
    /// The splash mark, driven as a failing neon sign.
    ///
    /// The sprite is sliced into its glyphs at runtime by reading the alpha channel:
    /// rows of clear pixels separate the title from the subtitle, columns of clear
    /// pixels separate the letters. Deriving the rects rather than hard-coding them
    /// means swapping the logo art keeps working.
    ///
    /// Each glyph is its own Image, so they strike on in sequence and fail
    /// independently. One gives out entirely — the ONCALL network runs on the same
    /// infrastructure the player is being paid to repair.
    ///
    /// Flicker is short bursts of dips, never a sustained strobe: repetitive flashing
    /// between roughly 3 and 30 Hz is a photosensitivity risk. Bursts are brief and
    /// far apart, a dead glyph stays dark rather than pulsing, and
    /// <see cref="flicker"/> disables the behaviour outright.
    /// </summary>
    public class SplashLogo : MonoBehaviour
    {
        [Tooltip("Seconds for the whole mark to light up, glyph by glyph.")]
        public float revealSeconds = 2.6f;

        [Tooltip("Failing-power behaviour. Turn off for photosensitivity.")]
        public bool flicker = true;

        [Tooltip("Off when SplashSequence cues the reveal instead.")]
        public bool autoStart = true;

        /// <summary>
        /// Multiplies the sprite's own hue. The raw art is a very blue cyan; this pulls
        /// it toward the brand teal and takes the brightness down.
        /// </summary>
        public Color tint = new Color(0.50f, 0.85f, 0.72f, 1f);

        // Starting values.
        private const float GlyphStagger = 0.80f;   // share of the reveal spent staggering
        private const float StrikeTime = 0.22f;     // per-glyph strike-on, seconds
        private const float MinGap = 1.4f, MaxGap = 4.5f;
        private const float MinDip = 0.05f, MaxDip = 0.11f;
        private const float DeadLevel = 0.06f;      // an unlit tube, not truly absent
        private const float ClearThreshold = 0.12f;

        private class Glyph
        {
            public Image Image;
            public float Order;      // 0..1, drives strike-on sequence
            public bool IsTitle;     // only title glyphs are candidates to fail
            public bool Doomed, Dead;
            public float NextEventAt, EventEndsAt, EventLevel = 1f;
            public int BurstLeft;
        }

        private readonly List<Glyph> _glyphs = new List<Glyph>();
        private float _elapsed;
        private bool _started, _revealed, _built;
        private float _instability = 1f;
        private float _failAt;

        // ─── Build ───

        /// <summary>Slices the sprite into glyph children. Idempotent.</summary>
        public void Build(Sprite sprite)
        {
            if (_built || sprite == null) return;
            _built = true;

            var tex = sprite.texture;
            if (!tex.isReadable) { AddPart(tex, sprite, new RectInt(0, 0, tex.width, tex.height), false, 0f); return; }

            int w = tex.width, h = tex.height;
            var px = tex.GetPixels();

            var bands = Runs(h, y =>
            {
                for (int x = 0; x < w; x++) if (px[y * w + x].a > ClearThreshold) return true;
                return false;
            });
            if (bands.Count == 0) { AddPart(tex, sprite, new RectInt(0, 0, w, h), false, 0f); return; }

            // The tallest band is the wordmark; anything else (the subtitle) stays whole
            // and arrives last so the mark reads before its qualifier.
            int title = 0;
            for (int i = 1; i < bands.Count; i++)
                if (bands[i].Length > bands[title].Length) title = i;

            for (int i = 0; i < bands.Count; i++)
            {
                var band = bands[i];
                if (i != title)
                {
                    AddPart(tex, sprite, new RectInt(0, band.Start, w, band.Length), false, 1f);
                    continue;
                }

                var cols = Runs(w, x =>
                {
                    for (int y = band.Start; y < band.Start + band.Length; y++)
                        if (px[y * w + x].a > ClearThreshold) return true;
                    return false;
                });
                for (int c = 0; c < cols.Count; c++)
                {
                    float order = cols.Count > 1 ? (float)c / (cols.Count - 1) : 0f;
                    AddPart(tex, sprite, new RectInt(cols[c].Start, band.Start, cols[c].Length, band.Length),
                            true, order * GlyphStagger);
                }
            }
        }

        private struct Run { public int Start, Length; }

        private static List<Run> Runs(int count, System.Func<int, bool> lit)
        {
            var runs = new List<Run>();
            bool on = false; int start = 0;
            for (int i = 0; i < count; i++)
            {
                bool v = lit(i);
                if (v && !on) { on = true; start = i; }
                else if (!v && on) { on = false; runs.Add(new Run { Start = start, Length = i - start }); }
            }
            if (on) runs.Add(new Run { Start = start, Length = count - start });
            return runs;
        }

        private void AddPart(Texture2D tex, Sprite source, RectInt r, bool isTitle, float order)
        {
            var go = UITheme.Node("Part" + _glyphs.Count, transform);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2((float)r.x / tex.width, (float)r.y / tex.height);
            rt.anchorMax = new Vector2((float)(r.x + r.width) / tex.width,
                                       (float)(r.y + r.height) / tex.height);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var img = go.AddComponent<Image>();
            img.sprite = Sprite.Create(tex, new Rect(r.x, r.y, r.width, r.height),
                                       new Vector2(0.5f, 0.5f), source.pixelsPerUnit);
            img.raycastTarget = false;

            var glyph = new Glyph { Image = img, Order = order, IsTitle = isTitle };
            // Build runs after OnEnable, so a new glyph has to start dark itself.
            SetLevel(glyph, 0f);
            _glyphs.Add(glyph);
        }

        // ─── Cueing ───

        public void PrepareForCue()
        {
            autoStart = false;
            ResetState();
        }

        public void Begin() { _started = true; }

        public void CompleteReveal()
        {
            _started = true;
            _revealed = true;
            _elapsed = revealSeconds;
            foreach (var g in _glyphs) SetLevel(g, g.Dead ? DeadLevel : 1f);
        }

        private void OnEnable() { ResetState(); _started = autoStart; }

        private void ResetState()
        {
            _elapsed = 0f;
            _started = false;
            _revealed = false;

            // The mark is only as steady as the grid behind it.
            _instability = 1f;
            var gm = GameManager.Instance;
            if (gm != null && gm.State != null)
            {
                int max = ContractRegistry.MaxTotalStars;
                if (max > 0) _instability = 1f - Mathf.Clamp01((float)gm.State.TotalStars / max);
            }

            foreach (var g in _glyphs)
            {
                g.Dead = false; g.Doomed = false; g.BurstLeft = 0;
                g.NextEventAt = 0f; g.EventEndsAt = 0f;
                SetLevel(g, 0f);
            }

            // One letter gives out, chosen from the middle so the mark still reads.
            // A healthy grid keeps every tube alight.
            if (_instability > 0.25f)
            {
                var titles = _glyphs.FindAll(g => g.IsTitle);
                if (titles.Count >= 4) titles[titles.Count / 2].Doomed = true;
            }
            _failAt = revealSeconds + Random.Range(1.5f, 3f);
        }

        // ─── Drive ───

        private void Update()
        {
            if (!_started || _glyphs.Count == 0) return;
            if (!_revealed) { Reveal(); return; }
            if (flicker) Failing();
        }

        /// <summary>Glyphs strike on left to right, like a sign warming up.</summary>
        private void Reveal()
        {
            _elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(_elapsed / revealSeconds);
            float strike = Mathf.Max(0.0001f, StrikeTime / revealSeconds);

            foreach (var g in _glyphs)
                SetLevel(g, Mathf.Clamp01((t - g.Order) / strike));

            if (t >= 1f) { _revealed = true; _elapsed = revealSeconds; }
        }

        private void Failing()
        {
            _elapsed += Time.unscaledDeltaTime;
            float now = Time.unscaledTime;

            foreach (var g in _glyphs)
            {
                if (now < g.EventEndsAt) { SetLevel(g, g.EventLevel); continue; }

                SetLevel(g, g.Dead ? DeadLevel : 1f);
                if (_instability <= 0.02f || now < g.NextEventAt) continue;

                if (g.Doomed && !g.Dead && g.BurstLeft == 0 && _elapsed >= _failAt)
                {
                    g.BurstLeft = Random.Range(5, 8);
                    if (GameAudio.Instance != null)
                        GameAudio.Instance.Play(Sfx.Crackle, 0.55f, Random.Range(0.9f, 1.1f));
                }

                if (g.BurstLeft > 0)
                {
                    // The stutter as a tube gives out, then it stays dark.
                    g.BurstLeft--;
                    g.EventEndsAt = now + Random.Range(0.05f, 0.14f);
                    g.EventLevel = g.BurstLeft % 2 == 0 ? DeadLevel : Random.Range(0.35f, 0.9f);
                    g.NextEventAt = g.EventEndsAt + Random.Range(0.04f, 0.12f);
                    if (g.BurstLeft == 0) g.Dead = true;
                    continue;
                }

                if (g.Dead)
                {
                    // A rare, weak attempt to relight.
                    g.EventEndsAt = now + Random.Range(0.04f, 0.09f);
                    g.EventLevel = Random.Range(0.20f, 0.50f);
                    g.NextEventAt = g.EventEndsAt + Random.Range(4f, 11f);
                    if (GameAudio.Instance != null)
                        GameAudio.Instance.Play(Sfx.Crackle, 0.22f, Random.Range(1.05f, 1.25f));
                    continue;
                }

                g.EventEndsAt = now + Random.Range(MinDip, MaxDip);
                g.EventLevel = Mathf.Lerp(1f, Random.Range(0.30f, 0.75f), _instability);
                g.NextEventAt = g.EventEndsAt + Random.Range(MinGap, MaxGap) / Mathf.Max(0.05f, _instability);
            }
        }

        private void SetLevel(Glyph g, float level)
        {
            g.Image.color = new Color(tint.r, tint.g, tint.b, Mathf.Clamp01(level) * tint.a);
        }
    }
}
