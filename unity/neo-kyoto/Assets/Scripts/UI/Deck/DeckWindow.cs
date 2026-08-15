using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NeoKyoto.UI.Deck
{
    /// <summary>
    /// One floating deck window: title bar, back, minimise, close, drag, resize, focus.
    /// Per docs/DECK_SPEC.md §3.
    ///
    /// The window owns its chrome but not its content — callers fill <see cref="Content"/>.
    /// It never destroys itself on close; the shell decides, because minimising and closing
    /// must both keep state (§10: "Minimising never discards anything").
    /// </summary>
    public class DeckWindow : MonoBehaviour, IPointerDownHandler
    {
        public string Id { get; private set; }
        public RectTransform Rect { get; private set; }
        public RectTransform Content { get; private set; }
        public bool IsMinimised { get; private set; }

        /// <summary>Raised when the player closes the window, so the shell can update the rail.</summary>
        public event Action<DeckWindow> Closed;
        public event Action<DeckWindow> Minimised;
        public event Action<DeckWindow> FocusRequested;
        public event Action BackPressed;

        private DeckShell _shell;
        private DeckLayoutSettings _cfg;

        private Image _border, _background, _scrim, _titleBarImage;
        private TextMeshProUGUI _titleLabel;
        private GameObject _backButton;

        internal void Build(DeckShell shell, DeckLayoutSettings cfg, string id, string title,
                            Vector2 size, Vector2 position, bool navigable)
        {
            _shell = shell;
            _cfg = cfg;
            Id = id;

            Rect = GetComponent<RectTransform>();
            // Top-left anchored so positions stay meaningful when the field resizes.
            Rect.anchorMin = new Vector2(0f, 1f);
            Rect.anchorMax = new Vector2(0f, 1f);
            Rect.pivot = new Vector2(0f, 1f);
            Rect.sizeDelta = Vector2.Max(size, _cfg.minWindowSize);
            Rect.anchoredPosition = position;

            // Scrim first so it sits behind everything, and larger than the window —
            // DECK_SPEC §4 layer 2. Without this, window edges dissolve into neon.
            _scrim = UITheme.Box("Scrim", transform, new Color(0f, 0f, 0f, _cfg.scrimDarken));
            float s = _cfg.scrimSpread;
            UITheme.Stretch(_scrim.rectTransform, -s, -s, -s, -s);
            _scrim.raycastTarget = false;

            _border = UITheme.Box("Border", transform, UITheme.Border);
            UITheme.Stretch(_border.rectTransform);

            _background = UITheme.Box("Background", _border.transform, OpaqueEnough(UITheme.Panel));
            UITheme.Stretch(_background.rectTransform, 1.5f, 1.5f, 1.5f, 1.5f);

            BuildTitleBar(title, navigable);

            Content = UITheme.Node("Content", _background.transform).GetComponent<RectTransform>();
            UITheme.Stretch(Content, 0f, 0f, 0f, _cfg.titleBarHeight);

            BuildResizeGrip();
            SetFocused(false);
        }

        private void BuildTitleBar(string title, bool navigable)
        {
            _titleBarImage = UITheme.Box("TitleBar", _background.transform, UITheme.Field);
            var bar = _titleBarImage.rectTransform;
            bar.anchorMin = new Vector2(0f, 1f);
            bar.anchorMax = new Vector2(1f, 1f);
            bar.pivot = new Vector2(0.5f, 1f);
            bar.offsetMin = new Vector2(0f, -_cfg.titleBarHeight);
            bar.offsetMax = Vector2.zero;

            var drag = _titleBarImage.gameObject.AddComponent<DeckDragHandle>();
            drag.Init(this);

            float x = 6f;
            if (navigable)
            {
                // The reference app needs history; moving between linked entries without
                // a back button is miserable. DECK_SPEC §3.
                _backButton = ChromeButton("Back", "<", x, () => { if (BackPressed != null) BackPressed(); });
                x += 24f;
            }

            _titleLabel = UITheme.Label("Title", _titleBarImage.transform, title,
                                        UITheme.SmallSize, UITheme.Text, TextAlignmentOptions.Left);
            var trt = _titleLabel.rectTransform;
            trt.anchorMin = new Vector2(0f, 0f);
            trt.anchorMax = new Vector2(1f, 1f);
            trt.offsetMin = new Vector2(x, 0f);
            trt.offsetMax = new Vector2(-56f, 0f);
            _titleLabel.raycastTarget = false;

            ChromeButtonRight("Minimise", "_", 30f, () => SetMinimised(true));
            ChromeButtonRight("Close", "x", 6f, Close);
        }

        private GameObject ChromeButton(string name, string glyph, float fromLeft, Action onClick)
        {
            var btn = MakeGlyphButton(name, glyph, onClick);
            var rt = btn.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0.5f);
            rt.anchorMax = new Vector2(0f, 0.5f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.sizeDelta = new Vector2(20f, 20f);
            rt.anchoredPosition = new Vector2(fromLeft, 0f);
            return btn;
        }

        private void ChromeButtonRight(string name, string glyph, float fromRight, Action onClick)
        {
            var btn = MakeGlyphButton(name, glyph, onClick);
            var rt = btn.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 0.5f);
            rt.anchorMax = new Vector2(1f, 0.5f);
            rt.pivot = new Vector2(1f, 0.5f);
            rt.sizeDelta = new Vector2(20f, 20f);
            rt.anchoredPosition = new Vector2(-fromRight, 0f);
        }

        private GameObject MakeGlyphButton(string name, string glyph, Action onClick)
        {
            var img = UITheme.Box(name, _titleBarImage.transform, new Color(0f, 0f, 0f, 0f));
            var btn = img.gameObject.AddComponent<Button>();
            var label = UITheme.Label("Glyph", img.transform, glyph, UITheme.SmallSize,
                                      UITheme.TextDim, TextAlignmentOptions.Center);
            UITheme.Stretch(label.rectTransform);
            label.raycastTarget = false;
            btn.onClick.AddListener(() =>
            {
                if (Core.GameAudio.Instance != null) Core.GameAudio.Instance.Play(Core.Sfx.Click);
                if (onClick != null) onClick();
            });
            return img.gameObject;
        }

        private void BuildResizeGrip()
        {
            var grip = UITheme.Box("ResizeGrip", transform, new Color(1f, 1f, 1f, 0.06f));
            var rt = grip.rectTransform;
            rt.anchorMin = new Vector2(1f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(1f, 0f);
            rt.sizeDelta = new Vector2(16f, 16f);
            rt.anchoredPosition = Vector2.zero;
            grip.gameObject.AddComponent<DeckResizeHandle>().Init(this);
        }

        /// <summary>
        /// DECK_SPEC §4 layer 1: the opacity floor is enforced here rather than trusted to
        /// callers, so no future transparency setting can push a window below legibility.
        /// </summary>
        private Color OpaqueEnough(Color c)
        {
            return new Color(c.r, c.g, c.b, Mathf.Max(c.a, _cfg.opacityFloor));
        }

        public void SetTitle(string title)
        {
            if (_titleLabel != null) _titleLabel.text = title;
        }

        public void SetFocused(bool focused)
        {
            // Two text surfaces, one keyboard — focus has to be unmistakable. DECK_SPEC §3.
            if (_border != null)
                _border.color = focused ? UITheme.Accent : UITheme.Border;
            if (_titleBarImage != null)
                _titleBarImage.color = focused ? UITheme.CodeBg : UITheme.Field;
            if (_titleLabel != null)
                _titleLabel.color = focused ? UITheme.Text : UITheme.TextDim;
        }

        public void SetMinimised(bool minimised)
        {
            IsMinimised = minimised;
            // Hide the chrome, keep the object alive: all internal state, caret and scroll
            // included, must survive minimising.
            gameObject.SetActive(!minimised);
            if (minimised && Minimised != null) Minimised(this);
        }

        public void Close()
        {
            gameObject.SetActive(false);
            if (Closed != null) Closed(this);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (FocusRequested != null) FocusRequested(this);
        }

        internal void MoveBy(Vector2 delta)
        {
            Rect.anchoredPosition += delta;
            _shell.SnapAndClamp(this);
        }

        internal void ResizeBy(Vector2 delta)
        {
            var size = Rect.sizeDelta + new Vector2(delta.x, -delta.y);
            Rect.sizeDelta = Vector2.Max(size, _cfg.minWindowSize);
            _shell.SnapAndClamp(this);
        }
    }

    /// <summary>Title-bar drag. Delta is divided by canvas scale so dragging tracks the cursor
    /// at any resolution.</summary>
    public class DeckDragHandle : MonoBehaviour, IDragHandler, IPointerDownHandler
    {
        private DeckWindow _window;
        private Canvas _canvas;

        public void Init(DeckWindow window)
        {
            _window = window;
            _canvas = window.GetComponentInParent<Canvas>();
        }

        public void OnPointerDown(PointerEventData e) { _window.OnPointerDown(e); }

        public void OnDrag(PointerEventData e)
        {
            float scale = _canvas != null ? _canvas.scaleFactor : 1f;
            if (scale <= 0f) scale = 1f;
            _window.MoveBy(e.delta / scale);
        }
    }

    /// <summary>Corner resize grip.</summary>
    public class DeckResizeHandle : MonoBehaviour, IDragHandler, IPointerDownHandler
    {
        private DeckWindow _window;
        private Canvas _canvas;

        public void Init(DeckWindow window)
        {
            _window = window;
            _canvas = window.GetComponentInParent<Canvas>();
        }

        public void OnPointerDown(PointerEventData e) { _window.OnPointerDown(e); }

        public void OnDrag(PointerEventData e)
        {
            float scale = _canvas != null ? _canvas.scaleFactor : 1f;
            if (scale <= 0f) scale = 1f;
            _window.ResizeBy(e.delta / scale);
        }
    }
}
