using UnityEngine;

namespace NeoKyoto.UI.Deck
{
    /// <summary>
    /// Geometry and legibility values for the deck frame, per docs/DECK_SPEC.md §2 and §12.
    ///
    /// Every number here is a **starting value** from the spec, not a sourced benchmark.
    /// Each has a documented test and a direction to move if it fails — see DECK_SPEC.md
    /// §12 before changing any of them. They are exposed on Bootstrap so a value can be
    /// found live in play mode and then set in edit mode to keep it.
    /// </summary>
    [System.Serializable]
    public class DeckLayoutSettings
    {
        [Header("Bands — fractions of viewport width, left to right")]
        [Tooltip("The world. Windows never spawn here and snapping avoids it. Spec start: 0.35. " +
                 "Test: an observer can watch the system respond during RUN without moving a " +
                 "window, 8/10. If it fails, widen to 0.45 before considering auto-hide.")]
        [Range(0.20f, 0.60f)] public float protectedRegion = 0.35f;

        [Tooltip("Persistent deck chrome. Never occluded, never moves. Spec start: 0.08. " +
                 "Test: all rail content legible without truncation. If it truncates, 0.10.")]
        [Range(0.05f, 0.15f)] public float rail = 0.08f;

        [Header("Windows")]
        [Tooltip("Spec start: 28px at 1080p. Test: grabbable without precision aiming. " +
                 "Mis-grabs → 32.")]
        public float titleBarHeight = 28f;

        [Tooltip("Spec start: 320x200. Test: terminal and editor both show at least 8 lines. " +
                 "Too cramped → 380x240.")]
        public Vector2 minWindowSize = new Vector2(320f, 200f);

        [Tooltip("Spec start: 12px. Test: snapping feels helpful, not grabby. Fighting the " +
                 "player → 8.")]
        public float snapThreshold = 12f;

        [Tooltip("Seconds between windows arriving when they unfold from the deck. " +
                 "Spec start: 0.08. Disorienting → 0.12.")]
        public float unfoldStagger = 0.08f;

        [Header("Legibility over live 3D — DECK_SPEC.md §4")]
        [Tooltip("Window backgrounds never drop below this, whatever any transparency setting " +
                 "says. Spec start: 0.92. Test: code readable with a neon sign directly behind, " +
                 "10/10. Below that → 0.96, then fully opaque.")]
        [Range(0.5f, 1f)] public float opacityFloor = 0.92f;

        [Tooltip("Darkening scrim behind each window. Spec start: 0.40. If window edges get " +
                 "lost against the world, increase this before increasing spread.")]
        [Range(0f, 1f)] public float scrimDarken = 0.40f;

        [Tooltip("How far the scrim extends beyond the window edge. Spec start: 24px.")]
        public float scrimSpread = 24f;

        /// <summary>
        /// The window field: everything between the protected region and the rail.
        /// Returned in normalised viewport space so it survives resolution changes,
        /// which DECK_SPEC.md §11 requires.
        /// </summary>
        public Rect FieldViewport
        {
            get
            {
                float x = Mathf.Clamp01(protectedRegion);
                float w = Mathf.Max(0.05f, 1f - x - Mathf.Clamp01(rail));
                return new Rect(x, 0f, w, 1f);
            }
        }

        /// <summary>Fraction of the width taken by the rail, clamped to something sane.</summary>
        public float RailViewport { get { return Mathf.Clamp(rail, 0.02f, 0.2f); } }
    }
}
