using NeoKyoto.Core;
using UnityEngine;

namespace NeoKyoto.World
{
    /// <summary>
    /// Splash camera-move settings. Exposed on Bootstrap so the shot can be found
    /// live in play mode and then set in edit mode to keep it, the same way the
    /// audio mix and splash timing already work.
    /// </summary>
    [System.Serializable]
    public class SplashCitySettings
    {
        [Tooltip("Off falls back to the painted panorama.")]
        public bool enabled = true;

        [Tooltip("Scene loaded additively behind the splash. Our own copy of the kit's " +
                 "demo city, so we can dress it without touching the vendor scene. The " +
                 "scene file is in git; the assets it references are not.")]
        public string sceneName = "NeoKyotoCity";

        [Tooltip("Metres the camera travels across the whole splash, relative to its own " +
                 "facing: x right, y up, z forward. Small numbers. This is a drift, not a fly-through.")]
        public Vector3 drift = new Vector3(0f, 0.6f, 2.5f);

        [Tooltip("Degrees the camera turns across the whole splash. Also small.")]
        public Vector3 turn = new Vector3(-1.5f, 3f, 0f);

        [Tooltip("Override the framing the scene's own camera arrives with. Off keeps the " +
                 "kit's shot, which is the one that already looks right.")]
        public bool overrideFraming;

        public Vector3 position;
        public Vector3 euler;
        [Range(20f, 110f)] public float fieldOfView = 80f;

        [Tooltip("Obsolete. The city is reference-counted now — it stays up while anything " +
                 "still wants it, and the overmap wants it too.")]
        public bool unloadWhenDone = true;
    }

    /// <summary>
    /// Drives the slow camera drift behind the title screen, over the live city.
    ///
    /// It no longer owns the city scene — <see cref="CityView"/> does, because the
    /// overmap needs the same city and a single owner would unload it in between. This
    /// is now purely a holder that says "the title wants the city" and then moves the
    /// borrowed camera while the title is up.
    /// </summary>
    public class SplashCityView : MonoBehaviour
    {
        public SplashCitySettings settings = new SplashCitySettings();

        private CityView _city;
        private UI.SplashSequence _sequence;
        private GameManager _gm;

        private Vector3 _startPos;
        private Quaternion _startRot;
        private bool _framed;

        public void Begin(CityView city, UI.SplashSequence sequence, GameManager gm)
        {
            _city = city;
            _sequence = sequence;
            _gm = gm;

            if (_city != null) _city.CityUp += OnCityUp;

            // The city is the title screen's backdrop, not the splash animation's. It
            // stays up for as long as the player is looking at the title, and is let go
            // when they leave it — not when the beats happen to finish.
            if (_gm != null) _gm.ScreenChanged += OnScreenChanged;

            if (settings.enabled && _city != null) _city.Acquire(this);
        }

        private void OnScreenChanged()
        {
            if (_gm == null || _city == null) return;

            if (_gm.CurrentScreen == GameScreen.Title)
            {
                _framed = false;
                _city.Acquire(this);
                if (_city.IsUp) OnCityUp();
            }
            else
            {
                _city.Release(this);
            }
        }

        /// <summary>
        /// Takes the shot the moment the city is live. The kit's own framing is the one
        /// that reads well; the override exists for finding a better one by hand.
        /// </summary>
        private void OnCityUp()
        {
            if (_city == null || !_city.IsUp) return;

            if (settings.overrideFraming)
                _city.Frame(settings.position, Quaternion.Euler(settings.euler), settings.fieldOfView);
            else
                _city.Frame(_city.AdoptedPosition, _city.AdoptedRotation, _city.AdoptedFieldOfView);

            _startPos = _city.Camera.transform.position;
            _startRot = _city.Camera.transform.rotation;
            _framed = true;
        }

        private void Update()
        {
            if (!_framed || _sequence == null || _city == null || !_city.IsUp) return;

            // Only while the title is actually up — the overmap borrows the same camera
            // and a drift still running underneath it would fight the district framing.
            if (_gm != null && _gm.CurrentScreen != GameScreen.Title) return;
            if (_city.IsFlying) return;

            // Driven off the sequence's own clock so the move stays locked to the beats,
            // including when the player skips and Finish() jumps the clock to the end.
            float total = Mathf.Max(0.0001f, _sequence.timing.TotalSeconds);
            float t = Mathf.Clamp01(_sequence.Elapsed / total);
            float eased = t * t * (3f - 2f * t);   // smoothstep, matching the UI beats

            _city.Camera.transform.position = _startPos + _startRot * settings.drift * eased;
            _city.Camera.transform.rotation = _startRot * Quaternion.Euler(settings.turn * eased);

            // The move settles at the end of its travel and the city stays live behind
            // the title — traffic, trains and fog keep running, so a held camera still
            // reads as a place rather than a screenshot.
        }

        private void OnDestroy()
        {
            if (_gm != null) _gm.ScreenChanged -= OnScreenChanged;
            if (_city != null) _city.CityUp -= OnCityUp;
        }
    }
}
