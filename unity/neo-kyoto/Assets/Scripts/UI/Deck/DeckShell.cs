using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace NeoKyoto.UI.Deck
{
    /// <summary>
    /// The deck frame: a persistent rail on the right, a free window field in the middle,
    /// and the world untouched on the left. Per docs/DECK_SPEC.md §2.
    ///
    /// The one rule everything here serves: **the deck must never become a full-screen
    /// application.** The moment the UI owns the whole frame, the on-site pivot was
    /// pointless. So the shell owns no background of its own — the world shows through
    /// everywhere a window isn't.
    /// </summary>
    public class DeckShell : MonoBehaviour
    {
        public DeckLayoutSettings settings = new DeckLayoutSettings();

        private RectTransform _field;
        private RectTransform _rail;
        private Transform _railTools, _railObjectives;
        private TextMeshProUGUI _linkLabel, _statusLabel;

        private readonly List<DeckWindow> _windows = new List<DeckWindow>();
        private DeckWindow _focused;

        public RectTransform Field { get { return _field; } }
        public IList<DeckWindow> Windows { get { return _windows; } }

        /// <summary>Builds the frame under a full-screen parent, typically the main canvas.</summary>
        public void Build(Transform parent)
        {
            var root = UITheme.Node("DeckShell", parent).GetComponent<RectTransform>();
            UITheme.Stretch(root);

            // Window field: from the protected region's right edge to the rail's left edge.
            // Anchored in normalised space so a resolution change re-anchors it proportionally,
            // which DECK_SPEC §11 requires.
            var fieldRect = settings.FieldViewport;
            _field = UITheme.Node("WindowField", root).GetComponent<RectTransform>();
            _field.anchorMin = new Vector2(fieldRect.xMin, 0f);
            _field.anchorMax = new Vector2(fieldRect.xMax, 1f);
            _field.offsetMin = Vector2.zero;
            _field.offsetMax = Vector2.zero;

            BuildRail(root);
        }

        private void BuildRail(RectTransform root)
        {
            // The rail is the only opaque deck chrome. It sits hard right because it must be
            // on the same side as the UI — a rail on the left would overlay the world.
            var railBg = UITheme.Box("Rail", root, UITheme.PanelSolid);
            _rail = railBg.rectTransform;
            _rail.anchorMin = new Vector2(1f - settings.RailViewport, 0f);
            _rail.anchorMax = new Vector2(1f, 1f);
            _rail.offsetMin = Vector2.zero;
            _rail.offsetMax = Vector2.zero;

            var edge = UITheme.Box("Edge", _rail, UITheme.Border);
            edge.rectTransform.anchorMin = new Vector2(0f, 0f);
            edge.rectTransform.anchorMax = new Vector2(0f, 1f);
            edge.rectTransform.offsetMin = Vector2.zero;
            edge.rectTransform.offsetMax = new Vector2(1.5f, 0f);
            edge.raycastTarget = false;

            var column = UITheme.Node("Column", _rail).GetComponent<RectTransform>();
            UITheme.Stretch(column, 6f, 8f, 6f, 8f);
            // childControlHeight drives every row from its own reported preferred height.
            // TextMeshProUGUI and nested layout groups both report one, so wrapped
            // objectives grow instead of clipping and nothing has to guess a fixed size.
            var layout = column.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 10f;

            // Zones top to bottom, in HUD priority order — DECK_SPEC §2.
            _linkLabel = RailZone(column, "LINK", "— no link —", UITheme.TextDim);
            _railTools = RailGroup(column, "TOOLS");
            _railObjectives = RailGroup(column, "OBJECTIVES");
            _statusLabel = RailZone(column, "STATUS", "0 cr", UITheme.Text);
        }

        private TextMeshProUGUI RailZone(Transform parent, string heading, string value, Color colour)
        {
            RailHeading(parent, heading);
            // No LayoutElement: TMP reports its own preferred height, so a two-line link
            // status takes two lines instead of being clipped to a guessed number.
            return UITheme.Label(heading + "Value", parent, value, UITheme.MicroSize, colour,
                                 TextAlignmentOptions.TopLeft, true);
        }

        private Transform RailGroup(Transform parent, string heading)
        {
            RailHeading(parent, heading);
            var group = UITheme.Node(heading + "Group", parent);
            var v = group.AddComponent<VerticalLayoutGroup>();
            v.childControlWidth = true;
            v.childControlHeight = true;
            v.childForceExpandWidth = true;
            v.childForceExpandHeight = false;
            v.spacing = 4f;
            return group.transform;
        }

        private void RailHeading(Transform parent, string text)
        {
            UITheme.Label(text + "Heading", parent, text, UITheme.MicroSize, UITheme.Accent);
        }

        /// <summary>Fixed height, for the things that genuinely have one — buttons.</summary>
        private static void AddHeight(GameObject go, float h)
        {
            var le = go.AddComponent<LayoutElement>();
            le.minHeight = h;
            le.preferredHeight = h;
            le.flexibleHeight = 0f;
        }

        // ── Windows ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Opens a window, or re-focuses and un-minimises the existing one with this id.
        /// Re-opening never discards state — DECK_SPEC §10.
        /// </summary>
        public DeckWindow Open(string id, string title, Vector2 size, bool navigable = false)
        {
            var existing = Find(id);
            if (existing != null)
            {
                existing.gameObject.SetActive(true);
                existing.SetMinimised(false);
                Focus(existing);
                return existing;
            }

            var go = UITheme.Node("Window_" + id, _field);
            var win = go.AddComponent<DeckWindow>();
            win.Build(this, settings, id, title, size, NextSpawn(size), navigable);

            win.FocusRequested += Focus;
            win.Closed += OnWindowClosed;

            _windows.Add(win);
            Focus(win);
            return win;
        }

        public DeckWindow Find(string id)
        {
            foreach (var w in _windows) if (w != null && w.Id == id) return w;
            return null;
        }

        private void OnWindowClosed(DeckWindow w)
        {
            _windows.Remove(w);
            if (_focused == w) _focused = null;
            Destroy(w.gameObject);

            // Focus falls to whatever is topmost, so the keyboard is never orphaned.
            for (int i = _windows.Count - 1; i >= 0; i--)
                if (_windows[i] != null && !_windows[i].IsMinimised) { Focus(_windows[i]); break; }
        }

        public void Focus(DeckWindow w)
        {
            if (w == null) return;
            _focused = w;
            w.Rect.SetAsLastSibling();          // focused window rises to the top of z-order
            foreach (var other in _windows)
                if (other != null) other.SetFocused(other == w);
        }

        public DeckWindow Focused { get { return _focused; } }

        /// <summary>Staggered cascade, biased away from the rail so nothing opens under it.</summary>
        private Vector2 NextSpawn(Vector2 size)
        {
            // Step deliberately larger than the title bar, so a cascaded window never lands
            // exactly on the one beneath it and leaves its title bar ungrabbable.
            int n = _windows.Count;
            float step = settings.titleBarHeight + 8f;
            return new Vector2(40f + step * (n % 6), -40f - step * (n % 6));
        }

        /// <summary>
        /// Magnetic assist, never a constraint. Windows snap to the field edges and to each
        /// other, but the player can still put a window anywhere — including over the world,
        /// which DECK_SPEC §11 explicitly allows.
        /// </summary>
        internal void SnapAndClamp(DeckWindow win)
        {
            var rt = win.Rect;
            var pos = rt.anchoredPosition;
            var size = rt.sizeDelta;
            float fieldW = _field.rect.width;
            float fieldH = _field.rect.height;
            float t = settings.snapThreshold;

            if (Mathf.Abs(pos.x) < t) pos.x = 0f;
            if (Mathf.Abs(pos.x + size.x - fieldW) < t) pos.x = fieldW - size.x;
            if (Mathf.Abs(pos.y) < t) pos.y = 0f;
            if (Mathf.Abs(-pos.y + size.y - fieldH) < t) pos.y = -(fieldH - size.y);

            foreach (var other in _windows)
            {
                if (other == null || other == win || other.IsMinimised) continue;
                var op = other.Rect.anchoredPosition;
                var os = other.Rect.sizeDelta;
                if (Mathf.Abs(pos.x - (op.x + os.x)) < t) pos.x = op.x + os.x;
                if (Mathf.Abs(pos.x + size.x - op.x) < t) pos.x = op.x - size.x;
                if (Mathf.Abs(pos.y - (op.y - os.y)) < t) pos.y = op.y - os.y;
            }

            // Clamped so the title bar always remains grabbable — DECK_SPEC §11. Losing a
            // window off-screen with no way to retrieve it is the unrecoverable-layout case.
            const float grabMargin = 80f;
            pos.x = Mathf.Clamp(pos.x, -(size.x - grabMargin), fieldW - grabMargin);
            pos.y = Mathf.Clamp(pos.y, -(fieldH - settings.titleBarHeight), 0f);

            rt.anchoredPosition = pos;
        }

        // ── Rail content ─────────────────────────────────────────────────────────

        public void SetLink(string text, bool connected)
        {
            if (_linkLabel == null) return;
            _linkLabel.text = text;
            _linkLabel.color = connected ? UITheme.Good : UITheme.TextDim;
        }

        public void SetStatus(string text)
        {
            if (_statusLabel != null) _statusLabel.text = text;
        }

        /// <summary>
        /// Locked tools are shown greyed rather than hidden — showing a named tool you
        /// cannot afford yet creates wanting; hiding it doesn't. ONSITE_PIVOT.md §3.
        /// </summary>
        public Button AddTool(string label, bool unlocked, UnityEngine.Events.UnityAction onClick)
        {
            var colour = unlocked ? UITheme.Accent : UITheme.TextDim;
            var btn = UITheme.Button("Tool_" + label, _railTools, label, colour,
                                     unlocked ? onClick : null);
            AddHeight(btn.gameObject, 28f);
            btn.interactable = unlocked;
            return btn;
        }

        public void ClearObjectives()
        {
            if (_railObjectives == null) return;
            for (int i = _railObjectives.childCount - 1; i >= 0; i--)
                Destroy(_railObjectives.GetChild(i).gameObject);
        }

        /// <summary>Ticked *and* struck through: colour is never the sole carrier. §8.</summary>
        public void AddObjective(string text, bool done)
        {
            if (_railObjectives == null) return;
            string mark = done ? "[x] " : "[ ] ";
            var t = UITheme.Label("Objective", _railObjectives, mark + text, UITheme.MicroSize,
                                  done ? UITheme.TextDim : UITheme.Text,
                                  TextAlignmentOptions.TopLeft, true);
            t.fontStyle = done ? FontStyles.Strikethrough : FontStyles.Normal;
        }

        // ── Keyboard focus switching ─────────────────────────────────────────────

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null || _windows.Count == 0) return;

            // Tab is deliberately not bound: it belongs to the editor for indentation and
            // the terminal for completion. DECK_SPEC §3.
            bool ctrl = kb.leftCtrlKey.isPressed || kb.rightCtrlKey.isPressed;
            if (ctrl && kb.tabKey.wasPressedThisFrame) { CycleFocus(); return; }

            bool alt = kb.leftAltKey.isPressed || kb.rightAltKey.isPressed;
            if (!alt) return;
            for (int i = 0; i < 9 && i < _windows.Count; i++)
            {
                var key = kb[(Key)((int)Key.Digit1 + i)];
                if (key != null && key.wasPressedThisFrame) { Focus(_windows[i]); return; }
            }
        }

        private void CycleFocus()
        {
            if (_windows.Count == 0) return;
            int start = _focused != null ? _windows.IndexOf(_focused) : -1;
            for (int step = 1; step <= _windows.Count; step++)
            {
                var candidate = _windows[(start + step + _windows.Count) % _windows.Count];
                if (candidate != null && !candidate.IsMinimised) { Focus(candidate); return; }
            }
        }
    }
}
