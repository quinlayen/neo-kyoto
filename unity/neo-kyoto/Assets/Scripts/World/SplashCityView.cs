using System.Collections;
using NeoKyoto.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        [Tooltip("Scene loaded additively behind the splash. Lives in a purchased kit, " +
                 "so it is absent on a fresh clone until the kit is re-imported.")]
        public string sceneName = "CP_Demo";

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

        [Tooltip("Unload the city once the splash is done. Off keeps it resident — useful " +
                 "later if the hub reuses the same view.")]
        public bool unloadWhenDone = true;
    }

    /// <summary>
    /// Puts the real city behind the splash instead of the painted panorama.
    ///
    /// The scene it loads ships inside a purchased kit, and purchased kits are
    /// gitignored — so every failure path here is a silent fall back to the painted
    /// art rather than an error. A fresh clone still gets a splash.
    /// </summary>
    public class SplashCityView : MonoBehaviour
    {
        public SplashCitySettings settings = new SplashCitySettings();

        private Camera _camera;
        private UIController _ui;
        private SplashSequence _sequence;

        private Scene _city;
        private bool _loaded;
        private Scene _previousActive;

        private Vector3 _startPos;
        private Quaternion _startRot;

        // The camera is the game's, only borrowed for the splash. Everything we change
        // on it is recorded here and put back in Release — otherwise gameplay inherits
        // the splash framing, and nothing else repositions the camera afterwards.
        private CameraClearFlags _clearFlagsWas;
        private Vector3 _camPosWas;
        private Quaternion _camRotWas;
        private float _fovWas;

        public void Begin(Camera worldCamera, UIController ui, SplashSequence sequence)
        {
            _camera = worldCamera;
            _ui = ui;
            _sequence = sequence;

            if (!settings.enabled || _camera == null) return;

            if (!Application.CanStreamedLevelBeLoaded(settings.sceneName))
            {
                // Expected on a fresh clone: the kit has not been re-imported, or the
                // scene was never added to Build Settings. Not an error.
                Debug.Log("[SplashCityView] Scene '" + settings.sceneName +
                          "' unavailable — keeping the painted panorama.");
                return;
            }

            StartCoroutine(LoadRoutine());
        }

        private IEnumerator LoadRoutine()
        {
            var op = SceneManager.LoadSceneAsync(settings.sceneName, LoadSceneMode.Additive);
            if (op == null) yield break;
            yield return op;

            _city = SceneManager.GetSceneByName(settings.sceneName);
            if (!_city.IsValid() || !_city.isLoaded) yield break;
            _loaded = true;

            AdoptFraming();
            Neutralise();

            // Lighting and skybox are per-scene, and the additive scene's settings are
            // ignored unless it is the active one. Without this the city renders under
            // NeoKyoto's flat ambient and looks nothing like the demo.
            _previousActive = SceneManager.GetActiveScene();
            SceneManager.SetActiveScene(_city);

            _clearFlagsWas = _camera.clearFlags;
            _camera.clearFlags = CameraClearFlags.Skybox;

            if (_ui != null) _ui.UseLiveCityBackdrop();
        }

        /// <summary>
        /// Takes the shot from the scene's own camera. The kit's default framing is the
        /// one that reads well, so copying it beats hand-entered numbers that drift out
        /// of date the moment the vendor changes the demo.
        /// </summary>
        private void AdoptFraming()
        {
            _camPosWas = _camera.transform.position;
            _camRotWas = _camera.transform.rotation;
            _fovWas = _camera.fieldOfView;

            if (settings.overrideFraming)
            {
                _camera.transform.SetPositionAndRotation(settings.position, Quaternion.Euler(settings.euler));
                _camera.fieldOfView = settings.fieldOfView;
            }
            else
            {
                Camera source = null;
                foreach (var go in _city.GetRootGameObjects())
                {
                    source = go.GetComponentInChildren<Camera>(true);
                    if (source != null) break;
                }

                if (source != null)
                {
                    _camera.transform.SetPositionAndRotation(source.transform.position, source.transform.rotation);
                    _camera.fieldOfView = source.fieldOfView;
                }
            }

            _startPos = _camera.transform.position;
            _startRot = _camera.transform.rotation;
        }

        /// <summary>
        /// Switches off anything in the loaded scene that would fight us: its cameras,
        /// its audio listener, and the kit's free-fly controller — the player must not
        /// be able to steer the splash.
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
                    var n = mb.GetType().Name;
                    if (n.IndexOf("CameraController", System.StringComparison.OrdinalIgnoreCase) >= 0)
                        mb.enabled = false;
                }
            }
        }

        private void Update()
        {
            if (!_loaded || _camera == null || _sequence == null) return;

            // Driven off the sequence's own clock so the move stays locked to the beats,
            // including when the player skips and Finish() jumps the clock to the end.
            float total = Mathf.Max(0.0001f, _sequence.timing.TotalSeconds);
            float t = Mathf.Clamp01(_sequence.Elapsed / total);
            float eased = t * t * (3f - 2f * t);   // smoothstep, matching the UI beats

            _camera.transform.position = _startPos + _startRot * settings.drift * eased;
            _camera.transform.rotation = _startRot * Quaternion.Euler(settings.turn * eased);

            if (_sequence.Finished && settings.unloadWhenDone) Release();
        }

        /// <summary>Hands the camera back to the game and drops the city.</summary>
        public void Release()
        {
            if (!_loaded) return;
            _loaded = false;

            if (_camera != null)
            {
                _camera.clearFlags = _clearFlagsWas;
                _camera.transform.SetPositionAndRotation(_camPosWas, _camRotWas);
                _camera.fieldOfView = _fovWas;
            }

            if (_previousActive.IsValid() && _previousActive.isLoaded)
                SceneManager.SetActiveScene(_previousActive);

            if (_city.IsValid() && _city.isLoaded) SceneManager.UnloadSceneAsync(_city);
        }
    }
}
