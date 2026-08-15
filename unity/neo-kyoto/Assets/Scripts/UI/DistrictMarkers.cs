using System.Collections.Generic;
using NeoKyoto.Core;
using NeoKyoto.World;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NeoKyoto.UI
{
    /// <summary>
    /// Pointer callbacks for one marker. A small component rather than an EventTrigger,
    /// so the hit area, the visuals and the callbacks stay a single object.
    /// </summary>
    public class MarkerPointer : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public System.Action Entered, Exited, Clicked;
        public void OnPointerEnter(PointerEventData e) { if (Entered != null) Entered(); }
        public void OnPointerExit(PointerEventData e) { if (Exited != null) Exited(); }
        public void OnPointerClick(PointerEventData e) { if (Clicked != null) Clicked(); }
    }

    /// <summary>
    /// District markers pinned to their real positions over the live city — a map, not a
    /// list with a background. Each district's world <see cref="District.Anchor"/> is
    /// projected to screen space every frame, so the markers stay on their places while
    /// the camera flies.
    ///
    /// The marker carries the minimum that reads at a glance: a diamond in the district's
    /// state colour, its name, and star pips once it is finished. Everything else — the
    /// contracts, the payouts, the gate line — waits for a hover or a click.
    /// </summary>
    public class DistrictMarkers : MonoBehaviour
    {
        private GameManager _gm;
        private CityView _city;
        private UIController _ui;
        private RectTransform _layer;

        private readonly List<Marker> _markers = new List<Marker>();
        private RectTransform _popup;
        private Transform _popupBody;
        private District _hovered, _pinned;

        // WCAG 2.1 SC 2.5.5 Target Size (Enhanced), 44x44. Taken over the 2.2 AA minimum
        // of 24x24 because these sit over a busy night city — exactly the low-contrast,
        // high-clutter case the criterion exists for. The diamond stays small; the hit
        // rect is what grows.
        private const float HitSize = 44f;
        private const float DiamondSize = 18f;

        private const float PopupWidth = 380f;
        private const float PopupPadding = 14f;
        private const float RowSpacing = 6f;
        private const float Margin = 12f;

        /// <summary>Running total of the rows added, so the popup can size itself to fit.</summary>
        private float _popupContentHeight;

        private class Marker
        {
            public District District;
            public RectTransform Root;
            public Image Diamond;
            public TextMeshProUGUI Name;
            public TextMeshProUGUI Pips;
            public Image Plate;
        }

        public void Begin(GameManager gm, CityView city, UIController ui, RectTransform layer)
        {
            _gm = gm;
            _city = city;
            _ui = ui;
            _layer = layer;

            Build();
            if (_gm != null) _gm.DistrictStateChanged += (d, s) => Refresh();
        }

        // ─── Construction ───

        private void Build()
        {
            foreach (var district in DistrictRegistry.All)
            {
                var rootGo = UITheme.Node("Marker_" + district.Id, _layer);
                var root = rootGo.GetComponent<RectTransform>();
                root.sizeDelta = new Vector2(180f, 78f);

                // A plate behind the text. The §4 legibility floor is enforced inside
                // DeckWindow and nothing out here inherits it, so bare labels over a lit
                // city are unreadable — which is exactly how the list version failed.
                var plate = UITheme.Box("Plate", rootGo.transform, new Color(0.02f, 0.03f, 0.05f, 0.72f));
                var plateRt = plate.rectTransform;
                plateRt.anchorMin = new Vector2(0.5f, 0f);
                plateRt.anchorMax = new Vector2(0.5f, 0f);
                plateRt.pivot = new Vector2(0.5f, 0f);
                plateRt.sizeDelta = new Vector2(150f, 40f);
                plateRt.anchoredPosition = Vector2.zero;
                plate.raycastTarget = false;

                var name = UITheme.Label("Name", rootGo.transform, district.Name.ToUpperInvariant(),
                    UITheme.SmallSize, UITheme.Text, TextAlignmentOptions.Center);
                Anchor(name.rectTransform, new Vector2(160f, 22f), new Vector2(0f, 18f));
                name.raycastTarget = false;

                // SmallSize, not MicroSize: at micro the mastered pips render as a single
                // unreadable dot, and the stabilised/mastered gap is the whole reason the
                // pips exist.
                var pips = UITheme.Label("Pips", rootGo.transform, "",
                    UITheme.SmallSize, UITheme.TextDim, TextAlignmentOptions.Center);
                pips.richText = true;
                Anchor(pips.rectTransform, new Vector2(160f, 18f), new Vector2(0f, 2f));
                pips.raycastTarget = false;

                // Diamond: a square turned 45°, so there is no glyph to go missing if the
                // font changes.
                var diamond = UITheme.Box("Diamond", rootGo.transform, UITheme.Accent);
                var drt = diamond.rectTransform;
                drt.anchorMin = drt.anchorMax = new Vector2(0.5f, 0f);
                drt.pivot = new Vector2(0.5f, 0.5f);
                drt.sizeDelta = new Vector2(DiamondSize, DiamondSize);
                drt.anchoredPosition = new Vector2(0f, 56f);
                drt.localRotation = Quaternion.Euler(0f, 0f, 45f);
                diamond.raycastTarget = false;

                // Invisible, and bigger than the diamond it stands in for.
                var hit = UITheme.Box("Hit", rootGo.transform, new Color(0f, 0f, 0f, 0f));
                var hrt = hit.rectTransform;
                hrt.anchorMin = hrt.anchorMax = new Vector2(0.5f, 0f);
                hrt.pivot = new Vector2(0.5f, 0.5f);
                hrt.sizeDelta = new Vector2(HitSize, HitSize);
                hrt.anchoredPosition = new Vector2(0f, 56f);

                var captured = district;
                var pointer = hit.gameObject.AddComponent<MarkerPointer>();
                pointer.Entered = () => { _hovered = captured; ShowPopup(captured); };
                pointer.Exited = () => { _hovered = null; if (_pinned == null) HidePopup(); };
                pointer.Clicked = () => OnMarkerClicked(captured);

                _markers.Add(new Marker {
                    District = district, Root = root, Diamond = diamond,
                    Name = name, Pips = pips, Plate = plate });
            }

            BuildPopup();
            Refresh();
        }

        private static void Anchor(RectTransform rt, Vector2 size, Vector2 position)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = size;
            rt.anchoredPosition = position;
        }

        private void BuildPopup()
        {
            // Framed returns the inner box; its parent is the border.
            var inner = UITheme.Framed("DistrictPopup", _layer, UITheme.Border);
            _popup = inner.parent.GetComponent<RectTransform>();

            // Centre-anchored, to match the markers. Marker positions come out of
            // ScreenPointToLocalPointInRectangle relative to the layer's centre pivot, so
            // a bottom-left-anchored popup reads those numbers in the wrong space and
            // lands in a corner.
            _popup.anchorMin = _popup.anchorMax = new Vector2(0.5f, 0.5f);
            _popup.pivot = Vector2.zero;
            _popup.sizeDelta = new Vector2(PopupWidth, 200f);

            var bg = inner.GetComponent<Image>();
            if (bg != null) bg.color = new Color(0.02f, 0.03f, 0.05f, 0.94f);

            var body = UITheme.Node("Body", inner);
            UITheme.Stretch(body.GetComponent<RectTransform>(),
                PopupPadding, PopupPadding, PopupPadding, PopupPadding);

            var layout = body.AddComponent<VerticalLayoutGroup>();
            layout.spacing = RowSpacing;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            _popupBody = body.transform;
            _popup.gameObject.SetActive(false);
        }

        // ─── State ───

        /// <summary>Recolours every marker from the district's current state.</summary>
        public void Refresh()
        {
            if (_gm == null) return;

            foreach (var m in _markers)
            {
                var state = _gm.StateOf(m.District);
                var colour = UIController.DistrictColor(state);

                m.Diamond.color = colour;
                m.Name.color = state == DistrictState.Locked ? UITheme.TextDim : UITheme.Text;

                // Pips only once the place is finished — that is the only time the
                // stabilised/mastered gap needs reading at a glance, and it is the gap
                // that turns "I finished it" into "I want to go back".
                int total = m.District.Contracts.Count;
                if (state == DistrictState.Stabilised || state == DistrictState.Mastered)
                {
                    int mastered = DistrictRegistry.MasteredCount(m.District, _gm.State);
                    m.Pips.text = "<color=" + UITheme.Hex(colour) + ">"
                                + new string('◆', mastered) + "</color><color="
                                + UITheme.Hex(UITheme.TextDim) + ">"
                                + new string('◇', Mathf.Max(0, total - mastered)) + "</color>";
                }
                else if (state == DistrictState.Failing)
                {
                    int open = DistrictRegistry.OpenCount(m.District, _gm.State);
                    m.Pips.text = "<color=" + UITheme.Hex(colour) + ">"
                                + open + (open == 1 ? " JOB" : " JOBS") + "</color>";
                }
                else
                {
                    m.Pips.text = "<color=" + UITheme.Hex(UITheme.TextDim) + ">LOCKED</color>";
                }

                // Locked places recede rather than shouting. Still legible, still there.
                var plateColour = m.Plate.color;
                plateColour.a = state == DistrictState.Locked ? 0.5f : 0.72f;
                m.Plate.color = plateColour;

                float scale = state == DistrictState.Locked ? 0.85f : 1f;
                m.Root.localScale = new Vector3(scale, scale, 1f);
            }
        }

        // ─── Projection ───

        private void LateUpdate()
        {
            bool live = _gm != null && _gm.CurrentScreen == GameScreen.Board
                     && _city != null && _city.IsUp && _city.Camera != null;

            if (!live)
            {
                foreach (var m in _markers) m.Root.gameObject.SetActive(false);
                if (_popup != null && _popup.gameObject.activeSelf) HidePopup();
                return;
            }

            var cam = _city.Camera;
            foreach (var m in _markers)
            {
                var screen = cam.WorldToScreenPoint(m.District.Anchor);

                // Negative z means the district is behind the camera, where the projection
                // mirrors it to the wrong side of the screen.
                if (screen.z <= 0f) { m.Root.gameObject.SetActive(false); continue; }

                Vector2 local;
                if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        _layer, screen, null, out local))
                {
                    m.Root.gameObject.SetActive(false);
                    continue;
                }

                m.Root.gameObject.SetActive(true);
                m.Root.anchoredPosition = local;
            }

            if (_pinned != null) PositionPopup(_pinned);
            else if (_hovered != null) PositionPopup(_hovered);
        }

        // ─── Popup ───

        private void OnMarkerClicked(District district)
        {
            _pinned = district;
            ShowPopup(district);

            var overmap = FindFirstObjectByType<OvermapView>();
            if (overmap != null) overmap.FlyToDistrict(district);
        }

        private void ShowPopup(District district)
        {
            if (_popup == null || _gm == null) return;

            for (int i = _popupBody.childCount - 1; i >= 0; i--)
            {
                var child = _popupBody.GetChild(i);
                child.SetParent(null, false);
                Destroy(child.gameObject);
            }
            _popupContentHeight = 0f;

            var state = _gm.StateOf(district);
            var colour = UIController.DistrictColor(state);

            Row(district.Name.ToUpperInvariant(), UITheme.SectionSize, colour, 28f);

            if (state == DistrictState.Locked)
            {
                Row(string.IsNullOrEmpty(district.LockedLine)
                        ? "Nothing routed to you there yet."
                        : district.LockedLine,
                    UITheme.SmallSize, UITheme.TextDim, 46f, true);
            }
            else
            {
                foreach (var def in district.Contracts)
                {
                    bool completed = _gm.State.IsContractCompleted(def.Id);
                    int stars = _gm.State.StarsFor(def.Id);
                    bool available = _gm.IsAvailable(def);

                    int baseCredits = ContractRegistry.BaseCreditsFor(def.Id);
                    string right = completed
                        ? new string('◆', stars) + new string('◇', Scoring.MaxStars - stars)
                          + "   " + Scoring.CreditsFor(stars, baseCredits) + " cr"
                        : (available ? "AVAILABLE" : "LOCKED");

                    var captured = def;
                    var btn = UITheme.Button("Open_" + def.Id, _popupBody, "",
                        completed ? UITheme.Good : UITheme.Accent,
                        available ? (UnityEngine.Events.UnityAction)(() => _gm.OpenContract(captured)) : null);
                    btn.interactable = available;
                    btn.gameObject.AddComponent<LayoutElement>().preferredHeight = 44f;
                    _popupContentHeight += 44f + RowSpacing;

                    var title = btn.GetComponentInChildren<TextMeshProUGUI>();
                    title.text = def.Title;
                    title.fontSize = UITheme.SmallSize;
                    title.alignment = TextAlignmentOptions.Left;
                    title.margin = new Vector4(12, 0, 8, 0);
                    title.overflowMode = TextOverflowModes.Ellipsis;

                    var status = UITheme.Label("Status", title.transform.parent, right,
                        UITheme.SmallSize, completed ? UITheme.Good : UITheme.TextDim,
                        TextAlignmentOptions.Right);
                    status.margin = new Vector4(8, 0, 12, 0);
                    status.raycastTarget = false;
                    var srt = status.rectTransform;
                    srt.anchorMin = new Vector2(0.45f, 0f);
                    srt.anchorMax = Vector2.one;
                    srt.offsetMin = srt.offsetMax = Vector2.zero;
                }
            }

            if (_pinned != null)
            {
                var back = UITheme.Button("BackToCity", _popupBody, "◀  BACK TO THE CITY",
                    UITheme.TextDim, () => { _pinned = null; HidePopup(); ReturnToOverview(); });
                back.gameObject.AddComponent<LayoutElement>().preferredHeight = 32f;
                back.GetComponentInChildren<TextMeshProUGUI>().fontSize = UITheme.MicroSize;
                _popupContentHeight += 32f + RowSpacing;
            }

            _popup.sizeDelta = new Vector2(PopupWidth,
                _popupContentHeight + PopupPadding * 2f);

            _popup.gameObject.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_popup);
            PositionPopup(district);
        }

        private void Row(string text, float size, Color colour, float height, bool wrap = false)
        {
            var label = UITheme.Label("Row", _popupBody, text, size, colour,
                TextAlignmentOptions.TopLeft, wrap);
            label.raycastTarget = false;

            // Wrapped rows have to measure themselves. Voss's gate lines vary in length
            // and a fixed height clips the last one and lets the button overlap it.
            if (wrap)
            {
                float available = PopupWidth - PopupPadding * 2f;
                height = Mathf.Max(height, label.GetPreferredValues(text, available, 0f).y + 6f);
            }

            label.gameObject.AddComponent<LayoutElement>().preferredHeight = height;
            _popupContentHeight += height + RowSpacing;
        }

        /// <summary>
        /// Sits the popup beside its marker and clamps it inside the screen, so a district
        /// near an edge does not open a panel half off it.
        /// </summary>
        private void PositionPopup(District district)
        {
            var marker = _markers.Find(m => m.District == district);
            if (marker == null || _popup == null) return;

            var size = _popup.rect.size;
            float halfW = _layer.rect.width * 0.5f;
            float halfH = _layer.rect.height * 0.5f;

            var p = marker.Root.anchoredPosition + new Vector2(26f, 20f);
            p.x = Mathf.Clamp(p.x, -halfW + Margin, Mathf.Max(-halfW + Margin, halfW - size.x - Margin));
            p.y = Mathf.Clamp(p.y, -halfH + Margin, Mathf.Max(-halfH + Margin, halfH - size.y - Margin));
            _popup.anchoredPosition = p;
        }

        private void HidePopup()
        {
            if (_popup != null) _popup.gameObject.SetActive(false);
        }

        private void ReturnToOverview()
        {
            var overmap = FindFirstObjectByType<OvermapView>();
            if (overmap != null) overmap.ShowOverview(false);
        }
    }
}
