#if UNITY_EDITOR || DEVELOPMENT_BUILD
using NeoKyoto.Core;
using NeoKyoto.World;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NeoKyoto.UI
{
    /// <summary>
    /// A live tuning panel, on **F1**.
    ///
    /// The settings it drives live on Bootstrap, which is the right home for them — but
    /// Maximize On Play fills the editor with the game, so the Inspector is exactly what
    /// you cannot reach at the moment you most want to drag a slider. This puts the
    /// sliders on top of the thing they change.
    ///
    /// Editor and development builds only. It compiles out of a release build entirely,
    /// so there is no shipping surface to secure or hide.
    /// </summary>
    public class TuningOverlay : MonoBehaviour
    {
        public WorkSiteLightSettings lights;

        /// <summary>Any constant will do; there is only ever one of these.</summary>
        private const int WindowId = 0x7ADF;

        private bool _open;
        private Rect _window = new Rect(24f, 24f, 360f, 0f);
        private GUIStyle _label, _header, _hint;

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb != null && kb.f1Key.wasPressedThisFrame) _open = !_open;
        }

        private void OnGUI()
        {
            if (!_open)
            {
                // A quiet reminder rather than nothing, so the panel is discoverable
                // without having to remember it exists.
                GUI.color = new Color(1f, 1f, 1f, 0.35f);
                GUI.Label(new Rect(12f, Screen.height - 24f, 300f, 20f), "F1  tuning");
                GUI.color = Color.white;
                return;
            }

            EnsureStyles();
            _window = GUILayout.Window(WindowId, _window, Draw, GUIContent.none);
        }

        private void EnsureStyles()
        {
            if (_label != null) return;
            _label = new GUIStyle(GUI.skin.label) { fontSize = 12 };
            _header = new GUIStyle(GUI.skin.label) { fontSize = 13, fontStyle = FontStyle.Bold };
            _hint = new GUIStyle(GUI.skin.label) { fontSize = 11, wordWrap = true };
            _hint.normal.textColor = new Color(0.65f, 0.7f, 0.78f);
        }

        private void Draw(int id)
        {
            if (lights == null) { GUILayout.Label("No settings wired.", _label); return; }

            GUILayout.Label("BLOCK LIGHTING   —   F1 to close", _header);
            GUILayout.Space(4f);

            // What the contract itself is doing, so the preview override is never
            // confused for real progress.
            var gm = GameManager.Instance;
            float actual = gm != null && gm.ActiveContract != null
                ? gm.ActiveContract.ProgressFraction : -1f;
            GUILayout.Label(actual < 0f
                ? "No contract open — preview is the only thing driving the street."
                : "Contract progress: " + actual.ToString("F2"), _hint);

            GUILayout.Space(6f);
            lights.previewOverride = GUILayout.Toggle(lights.previewOverride,
                " Preview override (ignore the contract)");
            GUI.enabled = lights.previewOverride;
            lights.previewFraction = Slider("Preview  broken → fixed", lights.previewFraction, 0f, 1f);
            GUI.enabled = true;

            GUILayout.Space(8f);
            GUILayout.Label("HOW DARK IT GETS", _header);
            lights.darkShareAtLoad = Slider("Share fully out at load", lights.darkShareAtLoad, 0f, 1f);
            lights.dimAtLoad       = Slider("Dim of the rest at load", lights.dimAtLoad, 0f, 1f);
            lights.litLevel        = Slider("Level once stable", lights.litLevel, 0.5f, 3f);

            GUILayout.Space(8f);
            GUILayout.Label("FLICKER", _header);
            lights.flickerAmount = Slider("Amount", lights.flickerAmount, 0f, 1f);
            lights.flickerSpeed  = Slider("Speed", lights.flickerSpeed, 0.5f, 25f);
            GUILayout.Label("A photosensitivity surface. When in doubt, come down.", _hint);

            GUILayout.Space(8f);
            GUILayout.Label("WINDOWS & REACH", _header);
            lights.lightDarkWindows = GUILayout.Toggle(lights.lightDarkWindows, " Light the dark windows");
            lights.windowLitShare = Slider("Share of windows lit", lights.windowLitShare, 0f, 1f);
            lights.radius = Slider("Radius (m)", lights.radius, 10f, 120f);

            GUILayout.Space(10f);
            if (GUILayout.Button("LOG VALUES TO CONSOLE"))
                Debug.Log(Summary());

            GUILayout.Label("Play mode discards these. Log them, or right-click the Bootstrap "
                          + "component → Copy Component, stop, → Paste Component Values.", _hint);

            GUI.DragWindow(new Rect(0f, 0f, 10000f, 20f));
        }

        private float Slider(string label, float value, float min, float max)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, _label, GUILayout.Width(190f));
            value = GUILayout.HorizontalSlider(value, min, max);
            GUILayout.Label(value.ToString("F2"), _label, GUILayout.Width(40f));
            GUILayout.EndHorizontal();
            return value;
        }

        /// <summary>
        /// Formatted so it can be read straight off the console and typed back into the
        /// Inspector, in Inspector field order.
        /// </summary>
        private string Summary()
        {
            return "[Tuning] Bootstrap → Work Site Lights\n"
                 + "  darkShareAtLoad  " + lights.darkShareAtLoad.ToString("F2") + "\n"
                 + "  dimAtLoad        " + lights.dimAtLoad.ToString("F2") + "\n"
                 + "  litLevel         " + lights.litLevel.ToString("F2") + "\n"
                 + "  flickerAmount    " + lights.flickerAmount.ToString("F2") + "\n"
                 + "  flickerSpeed     " + lights.flickerSpeed.ToString("F1") + "\n"
                 + "  lightDarkWindows " + lights.lightDarkWindows + "\n"
                 + "  windowLitShare   " + lights.windowLitShare.ToString("F2") + "\n"
                 + "  radius           " + lights.radius.ToString("F0");
        }
    }
}
#endif
