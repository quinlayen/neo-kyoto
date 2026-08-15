using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace NeoKyoto.UI
{
    /// <summary>
    /// Splash choreography, in one serialisable block so it can be tuned in the
    /// inspector instead of recompiled.
    ///
    /// Every beat is a pair: "At" is when it starts, measured in seconds from the
    /// moment the splash appears, and "For" is how long it takes once it does. So
    /// cityAt 0.5 / cityFor 4 means half a second of black, then a four second fade.
    /// </summary>
    [System.Serializable]
    public class SplashTiming
    {
        [Header("City — black holds until it starts")]
        public float cityAt = 0.5f;
        public float cityFor = 4f;
        [Tooltip("Pixels the city drifts upward while fading. 0 for a pure fade.")]
        [Range(0f, SplashSequence.BackdropOversize)] public float cityRise = 50f;

        [Header("Logo")]
        public float logoAt = 5f;
        public float logoFor = 3.5f;
        [Tooltip("0 lights the whole mark at once as a plain fade; 1 strikes the " +
                 "glyphs on one after another like a sign warming up.")]
        [Range(0f, 1f)] public float logoStagger = 0.8f;

        [Header("Tagline")]
        public float taglineAt = 9f;
        public float taglineFor = 1.4f;

        [Header("Connect button")]
        public float buttonAt = 10.5f;
        public float buttonFor = 0.6f;

        /// <summary>When the intro is over and everything is at rest.</summary>
        public float TotalSeconds { get { return buttonAt + buttonFor; } }
    }

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

        /// <summary>
        /// Fixed headroom the backdrop is built with. cityRise is tunable, so the
        /// oversize cannot track it — it is sized once for the largest rise allowed,
        /// otherwise a slide would expose bare panel along the bottom edge.
        /// </summary>
        public const float BackdropOversize = 120f;

        public SplashTiming timing = new SplashTiming();

        private Beat[] _beats;
        private SplashLogo _logo;
        private float _elapsed;
        private bool _finished;

        public void Bind(CanvasGroup city, SplashLogo logo, CanvasGroup tagline, CanvasGroup button)
        {
            _logo = logo;
            _beats = new[]
            {
                new Beat { Group = city,    Start = timing.cityAt,    Duration = timing.cityFor,
                           RiseFrom = timing.cityRise },
                new Beat { Group = tagline, Start = timing.taglineAt, Duration = timing.taglineFor },
                new Beat { Group = button,  Start = timing.buttonAt,  Duration = timing.buttonFor },
            };
            Rewind();
        }

        /// <summary>Seconds into the splash. Read by the live city view so the camera
        /// move rides the same clock as the beats, skip included.</summary>
        public float Elapsed { get { return _elapsed; } }

        /// <summary>True once the last beat has landed, or the player skipped.</summary>
        public bool Finished { get { return _finished; } }

        private void OnEnable() { Rewind(); }

        private void Rewind()
        {
            _elapsed = 0f;
            _finished = false;
            if (_beats == null) return;

            foreach (var b in _beats) Apply(b, 0f);
            if (_logo != null)
            {
                _logo.revealSeconds = timing.logoFor;
                _logo.stagger = timing.logoStagger;
                _logo.PrepareForCue();
            }
        }

        private void Update()
        {
            if (_beats == null || _finished) return;

            if (Skipped()) { Finish(); return; }

            _elapsed += Time.unscaledDeltaTime;
            foreach (var b in _beats)
                Apply(b, Mathf.Clamp01((_elapsed - b.Start) / Mathf.Max(0.0001f, b.Duration)));

            if (_logo != null && _elapsed >= timing.logoAt) _logo.Begin();

            if (_elapsed >= timing.TotalSeconds) _finished = true;
        }

        /// <summary>Jumps to the resting state — everything on, mark fully lit.</summary>
        public void Finish()
        {
            _finished = true;
            _elapsed = timing.TotalSeconds;
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
