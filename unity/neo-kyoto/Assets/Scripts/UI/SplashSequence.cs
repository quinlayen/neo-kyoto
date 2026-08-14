using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace NeoKyoto.UI
{
    /// <summary>
    /// Times the splash intro: black, city, mark, tagline, button. Beats and
    /// durations come from the table in docs/ART_BRIEF_SPLASH.md.
    ///
    /// Any input skips straight to the end state. The brief only asked for the
    /// intro to be skippable after the first view, but a six-second gate in front
    /// of someone who has already decided to play is charming exactly once —
    /// Response outranks Fit, so the skip is always available.
    ///
    /// Everything runs on unscaled time; the splash must not care what Time.timeScale
    /// happens to be.
    /// </summary>
    public class SplashSequence : MonoBehaviour
    {
        private struct Beat
        {
            public CanvasGroup Group;
            public float Start, Duration, RiseFrom;
        }

        // docs/ART_BRIEF_SPLASH.md — Animation Sequence.
        private const float CityAt = 0.5f, CityFor = 2.0f;
        private const float LogoAt = 2.5f;
        private const float TaglineAt = 4.5f, TaglineFor = 1.0f;
        private const float ButtonAt = 5.5f, ButtonFor = 0.5f;

        /// <summary>
        /// How far the city rises as it fades up, in reference pixels. The backdrop
        /// must be oversized by at least this much or the slide exposes bare panel.
        /// </summary>
        public const float CityRise = 50f;

        private Beat[] _beats;
        private SplashLogo _logo;
        private float _elapsed;
        private bool _finished;

        public void Bind(CanvasGroup city, SplashLogo logo, CanvasGroup tagline, CanvasGroup button)
        {
            _logo = logo;
            _beats = new[]
            {
                new Beat { Group = city,    Start = CityAt,    Duration = CityFor,    RiseFrom = CityRise },
                new Beat { Group = tagline, Start = TaglineAt, Duration = TaglineFor },
                new Beat { Group = button,  Start = ButtonAt,  Duration = ButtonFor },
            };
            Rewind();
        }

        private void OnEnable() { Rewind(); }

        private void Rewind()
        {
            _elapsed = 0f;
            _finished = false;
            if (_beats == null) return;

            foreach (var b in _beats) Apply(b, 0f);
            if (_logo != null) _logo.PrepareForCue();
        }

        private void Update()
        {
            if (_beats == null || _finished) return;

            if (Skipped()) { Finish(); return; }

            _elapsed += Time.unscaledDeltaTime;
            foreach (var b in _beats)
                Apply(b, Mathf.Clamp01((_elapsed - b.Start) / b.Duration));

            if (_logo != null && _elapsed >= LogoAt) _logo.Begin();

            if (_elapsed >= ButtonAt + ButtonFor) _finished = true;
        }

        /// <summary>Jumps to the resting state — everything on, mark fully lit.</summary>
        public void Finish()
        {
            _finished = true;
            _elapsed = ButtonAt + ButtonFor;
            if (_beats != null) foreach (var b in _beats) Apply(b, 1f);
            if (_logo != null) _logo.CompleteReveal();
        }

        private static void Apply(Beat b, float t)
        {
            if (b.Group == null) return;

            float eased = t * t * (3f - 2f * t);   // smoothstep
            b.Group.alpha = eased;

            // The button must not be clickable before it has faded in.
            bool live = t >= 1f;
            b.Group.interactable = live;
            b.Group.blocksRaycasts = live;

            if (b.RiseFrom > 0f)
            {
                var rt = b.Group.transform as RectTransform;
                if (rt != null)
                {
                    var p = rt.anchoredPosition;
                    rt.anchoredPosition = new Vector2(p.x, Mathf.Lerp(-b.RiseFrom, 0f, eased));
                }
            }
        }

        private static bool Skipped()
        {
            var kb = Keyboard.current;
            if (kb != null && kb.anyKey.wasPressedThisFrame) return true;
            var mouse = Mouse.current;
            return mouse != null && mouse.leftButton.wasPressedThisFrame;
        }
    }
}
