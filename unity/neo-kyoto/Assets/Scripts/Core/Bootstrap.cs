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

        private void Awake()
        {
            var gmGo = new GameObject("GameManager");
            gmGo.transform.SetParent(transform, false);
            var gm = gmGo.AddComponent<GameManager>();
            gm.unlockAllForTesting = unlockAllForTesting;

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

            var audioGo = new GameObject("Audio");
            audioGo.transform.SetParent(transform, false);
            audioGo.AddComponent<GameAudio>();

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

            var uiGo = new GameObject("UI");
            uiGo.transform.SetParent(transform, false);
            uiGo.AddComponent<UIController>();
        }
    }
}
