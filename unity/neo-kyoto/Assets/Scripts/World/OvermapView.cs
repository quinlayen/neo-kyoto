using System;
using NeoKyoto.Core;
using NeoKyoto.UI;
using UnityEngine;

namespace NeoKyoto.World
{
    /// <summary>
    /// Overmap camera and atmosphere. Starting values per `OVERMAP.md`; every one has
    /// its test in the tooltip.
    /// </summary>
    [System.Serializable]
    public class OvermapSettings
    {
        [Tooltip("Off leaves the board as a flat panel with no city behind it.")]
        public bool enabled = true;

        [Header("Overview shot — the whole core in one frame")]
        public Vector3 overviewAnchor = Vector3.zero;

        [Tooltip("Starting value 40°. Test: the four quadrants are all distinguishable. " +
                 "Too flat and the far two hide behind the near two; too steep and it " +
                 "stops reading as a city and starts reading as a floor plan.")]
        public float overviewPitch = 40f;

        public float overviewYaw = 35f;

        [Tooltip("Starting value 340 m, revised down from 520. 520 framed the city " +
                 "beautifully but the district anchors only span about 215 x 150 m, so all " +
                 "five markers clumped into the middle of the frame and their plates " +
                 "overlapped. Pull the camera in rather than spreading the anchors — the " +
                 "anchors are real places and moving them for composition breaks the premise. " +
                 "Test: the five markers are separable at a glance and no two plates touch.")]
        public float overviewDistance = 340f;

        public float overviewAimHeight = 30f;
        [Range(20f, 110f)] public float overviewFieldOfView = 55f;

        [Header("Flight")]
        [Tooltip("Starting value 1.4s. Test: reads as travelling to a place, not as a " +
                 "menu transition. Too slow and it becomes a wait on the twentieth visit — " +
                 "Response outranks everything, so cut this before cutting anything else.")]
        public float flightSeconds = 1.4f;

        [Header("Atmosphere")]
        [Tooltip("The kit's fog is authored for 100-200 m of street-level visibility. " +
                 "Above ~300 m it turns the city to grey soup, so the overmap pushes it " +
                 "back while it is showing and restores it on the way out.")]
        public bool overrideAtmosphere = true;

        [Tooltip("The game camera is set up for street-level work and its far plane clips " +
                 "most of the city away at overmap altitude — the overview camera sits 520 m " +
                 "out. Starting value 3000 m: the dense core is 500 m across, the skyline " +
                 "runs to about 3 km, and the kit's own demo camera used 2000. " +
                 "Test: the far towers are present rather than cut off mid-air.")]
        public float farClipPlane = 3000f;

        [Tooltip("Exponential fog. Starting value 0.25 of the scene's own density, chosen " +
                 "by comparison against 0.35 — at 0.35 the mid-ground greys out and the lit " +
                 "windows stop reading. Test: the far quadrants are visible but still read " +
                 "as further away. If the city goes flat, raise it rather than turning fog " +
                 "off, because the depth is doing real work.")]
        [Range(0.05f, 1f)] public float fogDensityScale = 0.25f;

        [Tooltip("Linear fog only, for if the scene's fog mode ever changes. Starting value 1400 m.")]
        public float fogEndDistance = 1400f;
    }

    /// <summary>
    /// The overmap: the live city seen from above, with the board over it.
    ///
    /// This is the replacement for the painted panorama in `OVERMAP.md`. Districts are
    /// real places in the city scene rather than markers on an image, so selecting one
    /// flies the camera down to it and that same corner becomes the diorama the contract
    /// is worked in front of.
    ///
    /// It holds the city through <see cref="CityView"/> rather than loading it, so
    /// moving between the title and the board never unloads and reloads the city.
    /// </summary>
    public class OvermapView : MonoBehaviour
    {
        public OvermapSettings settings = new OvermapSettings();

        private CityView _city;
        private GameManager _gm;
        private UIController _ui;

        private bool _atmosphereOverridden;
        private bool _fogWas;
        private float _fogEndWas;
        private float _fogDensityWas;
        private float _farClipWas;

        /// <summary>The district the camera is currently looking at, or null for the overview.</summary>
        public District Focused { get; private set; }

        public void Begin(CityView city, GameManager gm, UIController ui)
        {
            _city = city;
            _gm = gm;
            _ui = ui;

            if (_city != null) _city.CityUp += OnCityUp;
            if (_gm != null) _gm.ScreenChanged += OnScreenChanged;

            OnScreenChanged();
        }

        private void OnScreenChanged()
        {
            if (_gm == null || _city == null || !settings.enabled) return;

            if (_gm.CurrentScreen == GameScreen.Board)
            {
                _city.Acquire(this);
                if (_city.IsUp) OnCityUp();
            }
            else
            {
                RestoreAtmosphere();
                if (_ui != null) _ui.UseOpaqueBoard();
                _city.Release(this);
            }
        }

        private void OnCityUp()
        {
            if (_gm == null || _gm.CurrentScreen != GameScreen.Board) return;
            if (_city == null || !_city.IsUp) return;

            ApplyAtmosphere();
            if (_ui != null) _ui.UseLiveCityBoard();
            ShowOverview(true);
        }

        // ─── Framing ───

        /// <summary>The whole core in one frame. Where the player arrives, and returns to.</summary>
        public void ShowOverview(bool instant)
        {
            if (_city == null || !_city.IsUp) return;
            Focused = null;

            Vector3 pos; Quaternion rot;
            OverviewCamera(out pos, out rot);

            if (instant) _city.Frame(pos, rot, settings.overviewFieldOfView);
            else _city.FlyTo(pos, rot, settings.overviewFieldOfView, settings.flightSeconds);
        }

        private void OverviewCamera(out Vector3 position, out Quaternion rotation)
        {
            rotation = Quaternion.Euler(settings.overviewPitch, settings.overviewYaw, 0f);
            var aim = settings.overviewAnchor + Vector3.up * settings.overviewAimHeight;
            position = aim - rotation * Vector3.forward * settings.overviewDistance;
        }

        /// <summary>
        /// Flies down to a district. Same orbit maths the framings were authored with
        /// (<see cref="DistrictRegistry.CameraFor"/>), so a shot found by flying around
        /// the scene view transfers exactly.
        /// </summary>
        public void FlyToDistrict(District district, Action onArrived = null)
        {
            if (_city == null || !_city.IsUp || district == null) return;
            Focused = district;

            Vector3 pos; Quaternion rot;
            DistrictRegistry.CameraFor(district, out pos, out rot);
            _city.FlyTo(pos, rot, district.MapFraming.fieldOfView, settings.flightSeconds, onArrived);
        }

        // ─── Atmosphere ───

        /// <summary>
        /// Opens the camera's range and pushes the kit's street-level fog back, so the
        /// elevated view is neither clipped nor soup.
        ///
        /// Both are recorded and restored. RenderSettings is per-scene global state and
        /// the camera is borrowed — the contract diorama wants the street-level values
        /// back, and leaving either changed would quietly alter every later shot.
        /// </summary>
        private void ApplyAtmosphere()
        {
            if (!settings.overrideAtmosphere || _atmosphereOverridden) return;
            _atmosphereOverridden = true;

            _fogWas = RenderSettings.fog;
            _fogEndWas = RenderSettings.fogEndDistance;
            _fogDensityWas = RenderSettings.fogDensity;

            if (RenderSettings.fogMode == FogMode.Linear)
                RenderSettings.fogEndDistance = Mathf.Max(_fogEndWas, settings.fogEndDistance);
            else
                RenderSettings.fogDensity = _fogDensityWas * settings.fogDensityScale;

            var cam = _city != null ? _city.Camera : null;
            if (cam != null)
            {
                _farClipWas = cam.farClipPlane;
                cam.farClipPlane = Mathf.Max(_farClipWas, settings.farClipPlane);
            }
        }

        private void RestoreAtmosphere()
        {
            if (!_atmosphereOverridden) return;
            _atmosphereOverridden = false;

            RenderSettings.fog = _fogWas;
            RenderSettings.fogEndDistance = _fogEndWas;
            RenderSettings.fogDensity = _fogDensityWas;

            var cam = _city != null ? _city.Camera : null;
            if (cam != null) cam.farClipPlane = _farClipWas;
        }

        private void OnDestroy()
        {
            RestoreAtmosphere();
            if (_gm != null) _gm.ScreenChanged -= OnScreenChanged;
            if (_city != null) _city.CityUp -= OnCityUp;
        }
    }
}
