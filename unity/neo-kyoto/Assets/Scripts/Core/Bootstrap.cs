using NeoKyoto.UI;
using NeoKyoto.UI.Deck;
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

        [Tooltip("Puts the real city behind the splash instead of the painted panorama. " +
                 "Falls back to the painting if the kit scene is not available.")]
        public SplashCitySettings splashCity = new SplashCitySettings();

        [Tooltip("Deck frame geometry and legibility. Every value is a starting value from " +
                 "docs/DECK_SPEC.md §12 with a documented test — read it before changing them.")]
        public DeckLayoutSettings deckLayout = new DeckLayoutSettings();

        [Tooltip("Development only. Builds the deck frame with sample windows on top of " +
                 "whatever is on screen, so the frame can be judged over the live city " +
                 "before any real surface is ported into it.")]
        public bool deckPreview;

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
            var ui = uiGo.AddComponent<UIController>();
            ui.splashTiming = splashTiming;
            ui.deckLayout = deckLayout;
            uiGo.SetActive(true);

            // After the UI, because it needs the splash sequence the UIController builds
            // in Awake — and it only swaps out the painted backdrop once the city scene
            // has actually loaded.
            var cityGo = new GameObject("SplashCity");
            cityGo.transform.SetParent(transform, false);
            var cityView = cityGo.AddComponent<SplashCityView>();
            cityView.settings = splashCity;
            cityView.Begin(cam, ui, ui.SplashSequence, gm, world);

            if (deckPreview) BuildDeckPreview(ui);
        }

        /// <summary>
        /// Stage-1 scaffold for the deck frame. The real surfaces still live in the docked
        /// work panel; this exists so the frame itself — band split, rail, window chrome and
        /// above all legibility over neon — can be judged before anything is ported into it.
        /// </summary>
        private void BuildDeckPreview(UIController ui)
        {
            if (ui.CanvasRoot == null) return;

            var shellGo = new GameObject("Deck");
            shellGo.transform.SetParent(transform, false);
            var shell = shellGo.AddComponent<DeckShell>();
            shell.settings = deckLayout;
            shell.Build(ui.CanvasRoot);

            shell.SetLink("BLOCK 7 · substation\nlink established", true);
            shell.SetStatus("1,240 cr · Contractor");

            shell.AddTool("editor", "{}", true, null);
            shell.AddTool("terminal", ">_", true, null);
            shell.AddTool("reference", "?", true, null);
            shell.AddTool("store", "$", false, null);   // locked but visible, on purpose

            shell.AddObjective("Restore power to Block 7", false);
            shell.AddObjective("Read the fault log", true);

            var editor = shell.Open("editor", "main.py", new Vector2(520f, 340f));
            SampleText(editor, UITheme.CodeSize, UITheme.Text,
                       "1  for node in grid.nodes:\n" +
                       "2      if node.offline:\n" +
                       "3          node.restart()\n" +
                       "4  \n" +
                       "5  print(grid.status())");

            var readout = shell.Open("readout", "grid status", new Vector2(360f, 220f));
            SampleText(readout, UITheme.SmallSize, UITheme.TextDim,
                       "nodes     12\nonline     8\nfaulted    4\nload     73%");
        }

        private static void SampleText(DeckWindow window, float size, Color colour, string text)
        {
            var label = UITheme.Label("Sample", window.Content, text, size, colour);
            UITheme.Stretch(label.rectTransform, 10f, 8f, 10f, 8f);
            label.raycastTarget = false;
        }
    }
}
