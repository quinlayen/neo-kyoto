using NeoKyoto.Core;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace NeoKyoto.World
{
    /// <summary>
    /// Builds the city once, then swaps in the site for whichever contract is
    /// open and frames it with the camera.
    /// </summary>
    public class WorldController : MonoBehaviour
    {
        public Camera worldCamera;

        private Transform _siteRoot;
        private ContractSiteView _activeSite;
        private string _activeContractId;

        /// <summary>Left share of the screen the world occupies; the panel takes the rest.</summary>
        public float worldViewportWidth = 0.58f;

        private void Awake()
        {
            BuildEnvironment();

            _siteRoot = new GameObject("Site").transform;
            _siteRoot.SetParent(transform, false);
        }

        private void Start()
        {
            // The camera is wired up after Awake, so opt it into post-processing here.
            if (worldCamera != null)
            {
                var data = worldCamera.GetUniversalAdditionalCameraData();
                if (data != null) data.renderPostProcessing = true;
            }

            var gm = GameManager.Instance;
            if (gm != null)
            {
                gm.ScreenChanged += OnScreenChanged;
                OnScreenChanged();
            }
        }

        private void OnDestroy()
        {
            var gm = GameManager.Instance;
            if (gm != null) gm.ScreenChanged -= OnScreenChanged;
        }

        private void BuildEnvironment()
        {
            var env = new GameObject("Environment").transform;
            env.SetParent(transform, false);

            WorldBuilder.Ground(env);
            WorldBuilder.BuildSkyline(env);

            var sunGo = new GameObject("KeyLight");
            sunGo.transform.SetParent(env, false);
            sunGo.transform.rotation = Quaternion.Euler(38f, -35f, 0f);
            var sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(0.55f, 0.65f, 0.95f);
            sun.intensity = 0.55f;

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.15f, 0.17f, 0.24f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.04f, 0.05f, 0.09f);
            RenderSettings.fogDensity = 0.012f;

            BuildPostFx(env);
        }

        /// <summary>
        /// Bloom plus neutral tonemapping: status lights glow instead of clipping
        /// to white, so their colour still reads as the state signal.
        /// </summary>
        private void BuildPostFx(Transform parent)
        {
            var volGo = new GameObject("PostFX");
            volGo.transform.SetParent(parent, false);
            var volume = volGo.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 1f;

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();

            var bloom = profile.Add<Bloom>(true);
            bloom.intensity.Override(1.1f);
            bloom.threshold.Override(0.85f);
            bloom.scatter.Override(0.72f);

            var tonemapping = profile.Add<Tonemapping>(true);
            tonemapping.mode.Override(TonemappingMode.Neutral);

            volume.profile = profile;
        }

        private void OnScreenChanged()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            // The debrief is docked to the same side panel, so the site stays visible.
            bool inContract = gm.ActiveContract != null &&
                              (gm.CurrentScreen == GameScreen.Workspace ||
                               gm.CurrentScreen == GameScreen.Briefing ||
                               gm.CurrentScreen == GameScreen.Debrief);

            if (!inContract)
            {
                ClearSite();
                ApplyViewport(false);
                FrameOverview();
                return;
            }

            if (_activeContractId != gm.ActiveDef.Id)
            {
                ClearSite();
                _activeSite = CreateSite(gm.ActiveDef.Id);
                if (_activeSite != null) _activeSite.Bind(gm.ActiveContract);
                _activeContractId = gm.ActiveDef.Id;
            }
            else if (_activeSite != null)
            {
                // Same site, fresh contract instance (replay).
                _activeSite.Bind(gm.ActiveContract);
            }

            // The side panel covers the right of the screen on both of these
            // screens, so the world renders into the left strip either way.
            ApplyViewport(true);
            FrameSite();
        }

        private ContractSiteView CreateSite(string contractId)
        {
            var go = new GameObject("Site_" + contractId);
            go.transform.SetParent(_siteRoot, false);

            switch (contractId)
            {
                case "contract_01": return go.AddComponent<PowerNodeSite>();
                case "contract_02": return go.AddComponent<DroneRouterSite>();
                case "contract_03": return go.AddComponent<DroneDispatchSite>();
                case "contract_04": return go.AddComponent<TransitSignalsSite>();
                case "contract_05": return go.AddComponent<DataCenterSite>();
            }
            Destroy(go);
            return null;
        }

        private void ClearSite()
        {
            _activeSite = null;
            _activeContractId = null;
            if (_siteRoot == null) return;
            for (int i = _siteRoot.childCount - 1; i >= 0; i--) Destroy(_siteRoot.GetChild(i).gameObject);
        }

        [Tooltip("Deck model: the world fills the frame and the deck's windows float over it, " +
                 "rather than the world being letterboxed into a band. The protected focal " +
                 "region becomes a composition rule for where the failing system is framed, " +
                 "not a camera rect. See docs/DECK_SPEC.md §2.")]
        public bool fullFrameWorld = true;

        /// <summary>
        /// Hides the game's own world without tearing it down, so something else can own
        /// the view for a while.
        ///
        /// The placeholder Ground is a 200x200m plane whose top face sits at exactly y=0 —
        /// which is also where a real city kit puts its pavement. Left visible under a
        /// loaded location the two are coplanar across the whole street, and it reads as
        /// flickering sidewalks. Renderers only: colliders, state and the build itself all
        /// stay, so restoring is a single call and costs no rebuild.
        /// </summary>
        public void SetWorldVisible(bool visible)
        {
            bool wasLentOut = _viewLentOut;
            _viewLentOut = !visible;

            foreach (var r in GetComponentsInChildren<Renderer>(true)) r.enabled = visible;

            // Taking the view back means re-taking the camera. CityView releases a frame
            // before it actually tears down, so by the time this runs the screen change
            // that prompted it has already been and gone — if this doesn't re-frame the
            // shot, nothing will, and the contract site arrives unframed.
            if (wasLentOut && visible) OnScreenChanged();
        }

        /// <summary>True while CityView owns the camera — the splash, and the overmap.</summary>
        private bool _viewLentOut;

        private void ApplyViewport(bool docked)
        {
            if (worldCamera == null) return;
            worldCamera.rect = docked && !fullFrameWorld
                ? new Rect(0f, 0f, worldViewportWidth, 1f)
                : new Rect(0f, 0f, 1f, 1f);
        }

        private void FrameSite()
        {
            if (worldCamera == null || _activeSite == null) return;
            Frame(_activeSite.FocusPoint, _activeSite.FocusDistance, _activeSite.CameraYaw);
        }

        private void FrameOverview()
        {
            if (worldCamera == null) return;
            Frame(new Vector3(0f, 3f, 0f), 34f, 34f);
        }

        private void Frame(Vector3 focus, float distance, float yaw)
        {
            // Stand down while the view is lent out. CityView frames the same camera for
            // the splash and the overmap, and both handlers run off the same ScreenChanged
            // — without this the placeholder world's shot silently wins whichever runs last,
            // and the overmap ends up framed 34 m off the origin looking at nothing.
            if (_viewLentOut) return;

            var dir = Quaternion.Euler(24f, yaw, 0f) * Vector3.back;
            worldCamera.transform.position = focus + dir * distance;
            worldCamera.transform.rotation = Quaternion.LookRotation(focus - worldCamera.transform.position);
        }
    }
}
