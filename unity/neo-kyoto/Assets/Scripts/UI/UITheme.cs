using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NeoKyoto.UI
{
    /// <summary>Colours, fonts and small builders shared by every panel.</summary>
    public static class UITheme
    {
        public static readonly Color Backdrop = new Color32(0x08, 0x0A, 0x0F, 0xF2);
        public static readonly Color Panel = new Color32(0x0D, 0x11, 0x18, 0xFA);
        public static readonly Color PanelSolid = new Color32(0x0B, 0x0E, 0x14, 0xFF);
        public static readonly Color Field = new Color32(0x07, 0x09, 0x0D, 0xFF);
        public static readonly Color Border = new Color32(0x1D, 0x26, 0x33, 0xFF);

        public static readonly Color Text = new Color32(0xC8, 0xD3, 0xE0, 0xFF);
        public static readonly Color TextDim = new Color32(0x7A, 0x86, 0x98, 0xFF);
        public static readonly Color Accent = new Color32(0x35, 0xD6, 0xFF, 0xFF);
        public static readonly Color Good = new Color32(0x33, 0xF0, 0x8C, 0xFF);
        public static readonly Color Warn = new Color32(0xFF, 0xAE, 0x26, 0xFF);
        public static readonly Color Fault = new Color32(0xF2, 0x40, 0x34, 0xFF);

        /// <summary>Backing panel for sample code, so it reads as a block and not as prose.</summary>
        public static readonly Color CodeBg = new Color32(0x1E, 0x2C, 0x3C, 0xFF);

        /// <summary>Bar behind the line a run is currently executing.</summary>
        public static readonly Color RunLine = new Color32(0x1B, 0x3A, 0x4A, 0xFF);

        // Type scale. Starting values for the 1920x1080 reference canvas; at BodySize
        // the ~768px work panel measures roughly 65 characters, which is inside the
        // comfortable range for prose. See docs/ART_DIRECTION.md before changing these.
        public const float MicroSize = 14f;   // page counters, hints
        public const float SmallSize = 16f;   // field labels, console output
        public const float CodeSize = 18f;    // sample code and the script editor
        public const float BodySize = 19f;    // prose and status readouts
        public const float SectionSize = 22f; // panel headers
        public const float TitleSize = 34f;   // screen headers

        private static TMP_FontAsset _mono;

        public static TMP_FontAsset Mono
        {
            get
            {
                if (_mono == null) _mono = Resources.Load<TMP_FontAsset>("CascadiaMono SDF");
                if (_mono == null) _mono = TMP_Settings.defaultFontAsset;
                return _mono;
            }
        }

        /// <summary>A sprite from Resources/Splash, or null if it is missing.</summary>
        public static Sprite Art(string name)
        {
            return Resources.Load<Sprite>("Splash/" + name);
        }

        /// <summary>
        /// A full-bleed image that covers its parent without distorting. Envelope
        /// scaling crops the overflow instead of letterboxing, so the art fills any
        /// aspect ratio the player's window happens to be.
        /// </summary>
        public static Image CoverImage(string name, Transform parent, Sprite sprite)
        {
            var go = Node(name, parent);
            var img = go.AddComponent<Image>();
            img.sprite = sprite;
            img.raycastTarget = false;
            img.preserveAspect = false;
            Stretch(go.GetComponent<RectTransform>());

            if (sprite != null)
            {
                var fitter = go.AddComponent<AspectRatioFitter>();
                fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
                fitter.aspectRatio = sprite.rect.width / sprite.rect.height;
            }
            return img;
        }

        /// <summary>Theme colour as a TMP rich-text hex, e.g. "#35D6FF".</summary>
        public static string Hex(Color c)
        {
            return "#" + ColorUtility.ToHtmlStringRGB(c);
        }

        public static GameObject Node(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        public static RectTransform Stretch(RectTransform rt, float left = 0, float bottom = 0,
                                            float right = 0, float top = 0)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
            return rt;
        }

        public static Image Box(string name, Transform parent, Color color)
        {
            var go = Node(name, parent);
            var img = go.AddComponent<Image>();
            img.color = color;
            return img;
        }

        /// <summary>
        /// Wrapping is opt-in: terminal output, code and column-aligned rows rely on
        /// their own line breaks, while prose needs the panel to decide where lines end.
        /// </summary>
        public static TextMeshProUGUI Label(string name, Transform parent, string text,
                                            float size, Color color,
                                            TextAlignmentOptions align = TextAlignmentOptions.TopLeft,
                                            bool wrap = false)
        {
            var go = Node(name, parent);
            var t = go.AddComponent<TextMeshProUGUI>();
            t.font = Mono;
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.richText = false;
            t.enableWordWrapping = wrap;
            t.overflowMode = TextOverflowModes.Overflow;
            return t;
        }

        public static Button Button(string name, Transform parent, string label, Color accent,
                                    UnityEngine.Events.UnityAction onClick)
        {
            var img = Box(name, parent, UITheme.Field);
            var btn = img.gameObject.AddComponent<Button>();

            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.35f, 1.35f, 1.35f, 1f);
            colors.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
            colors.selectedColor = Color.white;
            btn.colors = colors;

            var outline = Box("Border", img.transform, accent);
            Stretch(outline.rectTransform);
            outline.type = Image.Type.Sliced;
            outline.color = new Color(accent.r, accent.g, accent.b, 0.35f);
            outline.raycastTarget = false;

            var inner = Box("Inner", outline.transform, UITheme.Field);
            Stretch(inner.rectTransform, 1.5f, 1.5f, 1.5f, 1.5f);
            inner.raycastTarget = false;

            var t = Label("Label", inner.transform, label, SmallSize, accent, TextAlignmentOptions.Center);
            Stretch(t.rectTransform, 8, 0, 8, 0);
            t.raycastTarget = false;

            if (onClick != null) btn.onClick.AddListener(onClick);
            return btn;
        }

        /// <summary>A scrollable text area. Returns the text component; scroll is the parent ScrollRect.</summary>
        public static TextMeshProUGUI ScrollText(string name, Transform parent, out ScrollRect scroll,
                                                 bool wrap = false)
        {
            var viewportImg = Box(name, parent, new Color(0, 0, 0, 0));
            scroll = viewportImg.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRectMovementType();
            scroll.scrollSensitivity = 28f;

            var viewport = Box("Viewport", viewportImg.transform, new Color(0, 0, 0, 0.001f));
            Stretch(viewport.rectTransform);
            viewport.gameObject.AddComponent<RectMask2D>();
            scroll.viewport = viewport.rectTransform;

            var content = Node("Content", viewport.transform);
            var contentRt = content.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0, 1);
            contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot = new Vector2(0, 1);
            contentRt.offsetMin = new Vector2(0, 0);
            contentRt.offsetMax = new Vector2(0, 0);
            scroll.content = contentRt;

            var fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var text = Label("Text", content.transform, "", BodySize, Text,
                             TextAlignmentOptions.TopLeft, wrap);
            var trt = text.rectTransform;
            trt.anchorMin = new Vector2(0, 1);
            trt.anchorMax = new Vector2(1, 1);
            trt.pivot = new Vector2(0, 1);

            var textFitter = content.AddComponent<VerticalLayoutGroup>();
            textFitter.childControlHeight = true;
            textFitter.childControlWidth = true;
            textFitter.childForceExpandHeight = false;
            textFitter.childForceExpandWidth = true;
            textFitter.padding = new RectOffset(8, 8, 6, 6);

            return text;
        }

        private static ScrollRect.MovementType ScrollRectMovementType()
        {
            return ScrollRect.MovementType.Clamped;
        }

        /// <summary>Framed container: border box with an inset background.</summary>
        public static RectTransform Framed(string name, Transform parent, Color borderColor)
        {
            var border = Box(name, parent, borderColor);
            var inner = Box("Inner", border.transform, Field);
            Stretch(inner.rectTransform, 1, 1, 1, 1);
            return inner.rectTransform;
        }
    }
}
