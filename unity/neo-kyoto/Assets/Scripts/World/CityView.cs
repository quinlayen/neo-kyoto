using System;
using System.Collections;
using System.Collections.Generic;
using NeoKyoto.Core;
using NeoKyoto.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NeoKyoto.World
{
    /// <summary>
    /// Owns the live city scene: loading it, making it safe to look at, lending it the
    /// game camera, and putting everything back afterwards.
    ///
    /// **Reference-counted, not screen-driven.** More than one thing wants the city now
    /// — the title screen behind the splash, and the overmap. A single owner would
    /// unload the city on the way from one to the other and reload it half a second
    /// later, which is both a stall and a visible flash.
    ///
    /// Every failure path falls back silently to the painted art rather than erroring.
    /// The scene file is in git but everything it references lives in a gitignored kit,
    /// so a fresh clone must still get a working title screen.
    /// </summary>
    public class CityView : MonoBehaviour
    {
        private string _sceneName;
        private bool _enabled;

        private Camera _camera;
        private UIController _ui;
        private GameManager _gm;
        private WorldController _world;

        private Scene _city;
        private Scene _previousActive;
        private bool _loading;

        /// <summary>
        /// False when we adopted a scene somebody else had already opened. We hand that
        /// one back untouched rather than unloading it — it is not ours to close, and in
        /// the editor it is the scene the developer is working in.
        /// </summary>
        private bool _weLoadedIt;

        private readonly HashSet<object> _holders = new HashSet<object>();
        private Coroutine _flight;

        /// <summary>True once the city is loaded, framed and safe to look at.</summary>
        public bool IsUp { get; private set; }

        public Camera Camera { get { return _camera; } }

        /// <summary>Fires when the city becomes live. Consumers frame their shot here.</summary>
        public event Action CityUp;

        // The framing the kit's own scene camera arrived with. The vendor's default shot
        // is the one that reads well, so it beats hand-entered numbers that go stale the
        // moment the kit is updated.
        public Vector3 AdoptedPosition { get; private set; }
        public Quaternion AdoptedRotation { get; private set; }
        public float AdoptedFieldOfView { get; private set; }

        // The camera is the game's, only borrowed. Everything changed on it is recorded
        // here and put back on the last release, or gameplay inherits a city framing and
        // nothing else ever repositions it.
        private CameraClearFlags _clearFlagsWas;
        private Vector3 _camPosWas;
        private Quaternion _camRotWas;
        private float _fovWas;

        public void Configure(string sceneName, bool enabled, Camera worldCamera,
                              UIController ui, GameManager gm, WorldController world)
        {
            _sceneName = sceneName;
            _enabled = enabled;
            _camera = worldCamera;
            _ui = ui;
            _gm = gm;
            _world = world;
        }

        // ─── Reference counting ───

        /// <summary>
        /// Registers a holder and brings the city up if it isn't already. Safe to call
        /// repeatedly — the player returns to the title more than once, and to the
        /// overmap after every contract.
        /// </summary>
        public void Acquire(object holder)
        {
            if (holder == null) return;
            _holders.Add(holder);

            if (IsUp || _loading || !_enabled || _camera == null) return;

            _loading = true;
            StartCoroutine(LoadRoutine());
        }

        /// <summary>
        /// Drops a holder, and tears the city down once nobody wants it — but not until
        /// the next frame.
        ///
        /// A handoff between two holders arrives as release-then-acquire inside a single
        /// ScreenChanged, and the order those handlers run in is not ours to choose. Going
        /// title → overmap, the splash lets go before the overmap takes hold; tearing down
        /// on the spot unloads the city and reloads it a moment later, which is a stall, a
        /// visible flash, and a load/unload race on the same scene that the city does not
        /// come back from. Waiting a frame and re-checking is the whole point of counting
        /// holders in the first place.
        /// </summary>
        public void Release(object holder)
        {
            if (holder == null) return;
            _holders.Remove(holder);

            if (_holders.Count != 0 || _pendingTearDown != null) return;
            if (!IsUp && !_loading) return;

            _pendingTearDown = StartCoroutine(TearDownWhenNobodyWantsIt());
        }

        private Coroutine _pendingTearDown;

        private IEnumerator TearDownWhenNobodyWantsIt()
        {
            yield return null;
            _pendingTearDown = null;
            if (_holders.Count == 0) TearDown();
        }

        private IEnumerator LoadRoutine()
        {
            // One frame before deciding anything. Scenes the editor had open additively
            // are still settling during Awake, so asking "is one already loaded?" then
            // reliably answers no — and we would load a second copy of the city on top
            // of the developer's.
            yield return null;

            // Adopt rather than duplicate. Two overlapping cities means two audio
            // listeners, a second camera running the kit's free-fly controller we never
            // neutralised, and doubled geometry.
            var existing = SceneManager.GetSceneByName(_sceneName);
            if (existing.IsValid() && existing.isLoaded)
            {
                Debug.Log("[CityView] '" + _sceneName + "' was already open — adopting it "
                        + "rather than loading a second copy.");
                _loading = false;
                _city = existing;
                _weLoadedIt = false;
                Activate();
                yield break;
            }

            if (!Application.CanStreamedLevelBeLoaded(_sceneName))
            {
                // Never added to Build Settings. Not an error.
                Debug.Log("[CityView] Scene '" + _sceneName + "' unavailable — keeping the painted art.");
                _loading = false;
                yield break;
            }

            // Capture the scene the loader actually produced. GetSceneByName returns the
            // *first* match, which is the wrong one the moment a second copy exists — it
            // had us holding, and preparing to unload, somebody else's scene.
            var loaded = default(Scene);
            UnityEngine.Events.UnityAction<Scene, LoadSceneMode> onLoaded =
                (s, mode) => { if (s.name == _sceneName) loaded = s; };

            SceneManager.sceneLoaded += onLoaded;
            var op = SceneManager.LoadSceneAsync(_sceneName, LoadSceneMode.Additive);
            if (op == null) { SceneManager.sceneLoaded -= onLoaded; _loading = false; yield break; }
            yield return op;
            SceneManager.sceneLoaded -= onLoaded;
            _loading = false;

            _city = loaded;
            _weLoadedIt = true;
            if (!_city.IsValid() || !_city.isLoaded) yield break;

            // Everyone may have let go while the scene was streaming in. Bringing a city
            // up behind the workspace would be worse than not bringing one up at all.
            if (_holders.Count == 0)
            {
                SceneManager.UnloadSceneAsync(_city);
                yield break;
            }

            // The city scene used to live inside the kit, so "can the scene be loaded?"
            // doubled as "is the kit imported?". It doesn't any more — our copy is in git
            // and loads perfectly on a fresh clone, arriving completely empty because
            // every prefab reference points into gitignored assets. Check for geometry
            // instead, or the graceful fallback silently stops being graceful.
            if (CountRenderers(_city) < MinimumCityRenderers)
            {
                Debug.Log("[CityView] '" + _sceneName + "' loaded but is empty — the asset kit " +
                          "is not imported. Keeping the painted art.");
                SceneManager.UnloadSceneAsync(_city);
                yield break;
            }

            _weLoadedIt = true;
            Activate();
        }

        /// <summary>
        /// Makes a city scene ours: framed, neutralised, lit, and with the placeholder
        /// world out of the way. Shared by the load path and the adopt path, so a scene
        /// the editor left open gets exactly the same treatment as one we loaded.
        /// </summary>
        private void Activate()
        {
            if (CountRenderers(_city) < MinimumCityRenderers)
            {
                Debug.Log("[CityView] '" + _sceneName + "' is empty — the asset kit is not "
                        + "imported. Keeping the painted art.");
                return;
            }

            IsUp = true;

            AdoptFraming();
            Neutralise();

            // Lighting and skybox are per-scene, and an additive scene's settings are
            // ignored unless it is the active one. Without this the city renders under
            // NeoKyoto's flat ambient and looks nothing like the demo.
            _previousActive = SceneManager.GetActiveScene();
            if (_previousActive != _city) SceneManager.SetActiveScene(_city);

            _clearFlagsWas = _camera.clearFlags;
            _camera.clearFlags = CameraClearFlags.Skybox;

            // Only once the real city is definitely up. Hiding the placeholder world on a
            // load that then failed would leave the player looking at nothing.
            if (_world != null) _world.SetWorldVisible(false);
            if (_ui != null) _ui.UseLiveCityBackdrop();

            if (CityUp != null) CityUp();
        }

        /// <summary>
        /// Measured: the city produces ~4,100 renderers with the kit imported, and none
        /// without it. The threshold sits in the middle of that gap — a presence check,
        /// not a tuned value.
        /// </summary>
        private const int MinimumCityRenderers = 100;

        private static int CountRenderers(Scene scene)
        {
            int n = 0;
            foreach (var root in scene.GetRootGameObjects())
                n += root.GetComponentsInChildren<Renderer>(true).Length;
            return n;
        }

        private void AdoptFraming()
        {
            _camPosWas = _camera.transform.position;
            _camRotWas = _camera.transform.rotation;
            _fovWas = _camera.fieldOfView;

            AdoptedPosition = _camPosWas;
            AdoptedRotation = _camRotWas;
            AdoptedFieldOfView = _fovWas;

            foreach (var go in _city.GetRootGameObjects())
            {
                var source = go.GetComponentInChildren<Camera>(true);
                if (source == null) continue;
                AdoptedPosition = source.transform.position;
                AdoptedRotation = source.transform.rotation;
                AdoptedFieldOfView = source.fieldOfView;
                break;
            }

            Frame(AdoptedPosition, AdoptedRotation, AdoptedFieldOfView);
        }

        /// <summary>
        /// Switches off anything in the loaded scene that would fight us: its cameras,
        /// its audio listener, and the kit's free-fly controller — the player must never
        /// be able to steer the city.
        ///
        /// The controller is matched by type name rather than referenced directly,
        /// because it lives in a gitignored kit and a hard reference would stop the
        /// project compiling on a fresh clone.
        /// </summary>
        private void Neutralise()
        {
            foreach (var root in _city.GetRootGameObjects())
            {
                foreach (var cam in root.GetComponentsInChildren<Camera>(true))
                    cam.enabled = false;

                foreach (var listener in root.GetComponentsInChildren<AudioListener>(true))
                    listener.enabled = false;

                foreach (var mb in root.GetComponentsInChildren<MonoBehaviour>(true))
                {
                    if (mb == null) continue;
                    if (mb.GetType().Name.IndexOf("CameraController",
                            StringComparison.OrdinalIgnoreCase) >= 0)
                        mb.enabled = false;
                }
            }
        }

        // ─── Framing ───

        /// <summary>Puts the camera somewhere immediately, cancelling any flight.</summary>
        public void Frame(Vector3 position, Quaternion rotation, float fieldOfView)
        {
            StopFlight();
            if (_camera == null) return;
            _camera.transform.SetPositionAndRotation(position, rotation);
            _camera.fieldOfView = fieldOfView;
        }

        /// <summary>
        /// Moves the camera to a new framing over time. Smoothstepped rather than linear
        /// so it eases out of one district and into the next — a linear move between two
        /// aerial shots reads as a slide, not a flight.
        /// </summary>
        public void FlyTo(Vector3 position, Quaternion rotation, float fieldOfView,
                          float seconds, Action onArrived = null)
        {
            if (_camera == null) return;
            StopFlight();

            if (seconds <= 0f)
            {
                Frame(position, rotation, fieldOfView);
                if (onArrived != null) onArrived();
                return;
            }

            _flight = StartCoroutine(FlyRoutine(position, rotation, fieldOfView, seconds, onArrived));
        }

        public bool IsFlying { get { return _flight != null; } }

        private void StopFlight()
        {
            if (_flight == null) return;
            StopCoroutine(_flight);
            _flight = null;
        }

        private IEnumerator FlyRoutine(Vector3 to, Quaternion toRot, float toFov,
                                       float seconds, Action onArrived)
        {
            var fromPos = _camera.transform.position;
            var fromRot = _camera.transform.rotation;
            float fromFov = _camera.fieldOfView;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / seconds;
                float e = Mathf.Clamp01(t);
                e = e * e * (3f - 2f * e);   // smoothstep, matching the UI beats

                _camera.transform.SetPositionAndRotation(
                    Vector3.Lerp(fromPos, to, e), Quaternion.Slerp(fromRot, toRot, e));
                _camera.fieldOfView = Mathf.Lerp(fromFov, toFov, e);
                yield return null;
            }

            _camera.transform.SetPositionAndRotation(to, toRot);
            _camera.fieldOfView = toFov;
            _flight = null;
            if (onArrived != null) onArrived();
        }

        // ─── Teardown ───

        private void TearDown()
        {
            if (!IsUp) return;
            IsUp = false;
            StopFlight();

            if (_camera != null)
            {
                _camera.clearFlags = _clearFlagsWas;
                _camera.transform.SetPositionAndRotation(_camPosWas, _camRotWas);
                _camera.fieldOfView = _fovWas;
            }

            if (_world != null) _world.SetWorldVisible(true);

            // Hand the painted backdrop back at the same time as the world, or the title
            // is left transparent over whatever 3D happens to be framed behind it.
            if (_ui != null) _ui.UsePaintedBackdrop();

            if (_previousActive.IsValid() && _previousActive.isLoaded && _previousActive != _city)
                SceneManager.SetActiveScene(_previousActive);

            // Only unload what we loaded. An adopted scene belongs to whoever opened it.
            if (_weLoadedIt && _city.IsValid() && _city.isLoaded)
                SceneManager.UnloadSceneAsync(_city);
            _weLoadedIt = false;
        }
    }
}
