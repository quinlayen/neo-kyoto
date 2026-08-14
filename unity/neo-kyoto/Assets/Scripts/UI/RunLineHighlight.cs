using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NeoKyoto.UI
{
    /// <summary>
    /// A bar behind the script editor marking the line the run is currently on.
    ///
    /// The editor is a TMP_InputField with rich text disabled — an editable buffer
    /// mangles colour tags — so the line cannot be recoloured. A bar drawn behind the
    /// text gives the same read without touching what the player typed.
    /// </summary>
    public class RunLineHighlight : MonoBehaviour
    {
        private TextMeshProUGUI _text;
        private RectTransform _bar;
        private int _line;

        public static RunLineHighlight Attach(TMP_InputField field, Color color)
        {
            var text = field.textComponent as TextMeshProUGUI;
            if (text == null) return null;

            var barImage = UITheme.Box("RunLine", text.transform.parent, color);
            var bar = barImage.rectTransform;
            bar.anchorMin = new Vector2(0f, 0.5f);
            bar.anchorMax = new Vector2(1f, 0.5f);
            bar.pivot = new Vector2(0.5f, 0.5f);
            bar.sizeDelta = new Vector2(0f, 0f);
            barImage.raycastTarget = false;

            // Behind the text, which is a later sibling and so draws on top.
            bar.SetAsFirstSibling();
            barImage.enabled = false;

            var highlight = field.gameObject.AddComponent<RunLineHighlight>();
            highlight._text = text;
            highlight._bar = bar;
            return highlight;
        }

        /// <summary>1-based source line, or 0 to hide the bar.</summary>
        public void SetLine(int line)
        {
            _line = line;
            Reposition();
        }

        private void LateUpdate()
        {
            // The input field scrolls by moving the text rect, so the bar has to
            // follow it rather than being placed once.
            if (_line > 0) Reposition();
        }

        private void Reposition()
        {
            if (_bar == null || _text == null) return;

            var image = _bar.GetComponent<Image>();
            var info = _text.textInfo;

            if (_line <= 0 || info == null || _line > info.lineCount)
            {
                if (image != null) image.enabled = false;
                return;
            }

            var lineInfo = info.lineInfo[_line - 1];
            float top = lineInfo.ascender;
            float bottom = lineInfo.descender;
            float height = top - bottom;
            if (height <= 0f)
            {
                if (image != null) image.enabled = false;
                return;
            }

            // Text and bar share a parent, so the only difference is the offset the
            // input field applies to the text rect when it scrolls.
            float scroll = _text.rectTransform.anchoredPosition.y;

            _bar.sizeDelta = new Vector2(_bar.sizeDelta.x, height);
            _bar.anchoredPosition = new Vector2(0f, (top + bottom) * 0.5f + scroll);
            if (image != null) image.enabled = true;
        }
    }
}
