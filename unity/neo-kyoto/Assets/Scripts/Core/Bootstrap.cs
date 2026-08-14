using NeoKyoto.UI;
using NeoKyoto.World;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace NeoKyoto.Core
{
    /// <summary>
    /// Single entry point for the scene: creates the manager, camera, world and
    /// UI at runtime so the whole game is defined in code rather than scene data.
    /// </summary>
    public class Bootstrap : MonoBehaviour
    {
        public bool unlockAllForTesting;

        [Tooltip("Wipe saved progress and scripts on play. Development only.")]
        public bool resetSaveOnPlay;

        [Tooltip("Jump straight into this contract, 1-based. 0 for the normal flow.")]
        public int startAtContract;

        [Tooltip("Volume sliders. Editable during play — find a level live, then set it " +
                 "in edit mode to keep it.")]
        public AudioMix audioMix = new AudioMix();

        [Tooltip("Splash intro beats. 'At' is when something starts, 'For' is how long " +
                 "it takes once it does.")]
        public SplashTiming splashTiming = new SplashTiming();

        private void Awake()
        {
            // Created inactive on purpose: AddComponent runs Awake immediately, so a
            // live object would read these flags before they were assigned. That is why
            // unlockAllForTesting had never taken effect.
            var gmGo = new GameObject("GameManager");
            gmGo.SetActive(false);
            gmGo.transform.SetParent(transform, false);
            var gm = gmGo.AddComponent<GameManager>();
            gm.unlockAllForTesting = unlockAllForTesting;
            gm.resetSaveOnPlay = resetSaveOnPlay;
            gm.startAtContract = startAtContract;
            gmGo.SetActive(true);

            var camGo = new GameObject("WorldCamera");
            camGo.transform.SetParent(transform, false);
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.02f, 0.03f, 0.05f);
            cam.fieldOfView = 55f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 400f;
            camGo.tag = "MainCamera";
            // Without a listener in the scene nothing is audible at all.
            camGo.AddComponent<AudioListener>();

            // Inactive first, so Awake does not run before the mix is handed over —
            // the same ordering trap that silently disabled unlockAllForTesting.
            var audioGo = new GameObject("Audio");
            audioGo.SetActive(false);
            audioGo.transform.SetParent(transform, false);
            audioGo.AddComponent<GameAudio>().mix = audioMix;
            audioGo.SetActive(true);

            if (FindFirstObjectByType<EventSystem>() == null)
            {
                var esGo = new GameObject("EventSystem");
                esGo.transform.SetParent(transform, false);
                esGo.AddComponent<EventSystem>();
                esGo.AddComponent<InputSystemUIInputModule>();
            }

            var worldGo = new GameObject("World");
            worldGo.transform.SetParent(transform, false);
            var world = worldGo.AddComponent<WorldController>();
            world.worldCamera = cam;

            // Inactive first: UIController builds the whole splash in Awake, so the
            // timing has to be in place before it runs.
            var uiGo = new GameObject("UI");
            uiGo.SetActive(false);
            uiGo.transform.SetParent(transform, false);
            uiGo.AddComponent<UIController>().splashTiming = splashTiming;
            uiGo.SetActive(true);
        }
    }
}
