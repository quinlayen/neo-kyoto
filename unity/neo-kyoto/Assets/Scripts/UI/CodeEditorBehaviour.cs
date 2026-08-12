using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace NeoKyoto.UI
{
    /// <summary>
    /// Makes a TMP_InputField usable for Python: Tab inserts four spaces and
    /// Enter keeps the current indentation, adding a level after a ':'.
    /// Indentation is the lesson, so the editor should not fight the player.
    /// </summary>
    [RequireComponent(typeof(TMP_InputField))]
    public class CodeEditorBehaviour : MonoBehaviour
    {
        public const string Indent = "    ";

        private TMP_InputField _field;
        private string _last = "";
        private bool _suppress;

        private void Awake()
        {
            _field = GetComponent<TMP_InputField>();
            _field.onValueChanged.AddListener(OnValueChanged);
            _last = _field.text;

            // Stop Tab from moving focus to another control.
            var nav = _field.navigation;
            nav.mode = Navigation.Mode.None;
            _field.navigation = nav;
        }

        private void Update()
        {
            if (!_field.isFocused) return;
            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.tabKey.wasPressedThisFrame) InsertAtCaret(Indent);
        }

        private void OnValueChanged(string text)
        {
            if (_suppress) { _last = text; return; }

            // Detect a newline that was just typed, then match the previous line.
            if (text.Length == _last.Length + 1)
            {
                int caret = Mathf.Clamp(_field.stringPosition, 0, text.Length);
                if (caret > 0 && text[caret - 1] == '\n')
                {
                    string indent = IndentForLineBefore(text, caret - 1);
                    if (indent.Length > 0) InsertAtCaret(indent);
                }
            }
            _last = _field.text;
        }

        /// <summary>Indentation to apply after the newline at <paramref name="newlineIndex"/>.</summary>
        private static string IndentForLineBefore(string text, int newlineIndex)
        {
            int lineStart = text.LastIndexOf('\n', Mathf.Max(0, newlineIndex - 1));
            lineStart = lineStart < 0 ? 0 : lineStart + 1;
            if (lineStart > newlineIndex) return "";

            string line = text.Substring(lineStart, newlineIndex - lineStart);

            int spaces = 0;
            while (spaces < line.Length && line[spaces] == ' ') spaces++;

            string trimmed = line.Trim();
            // A block opener earns an extra level.
            if (trimmed.EndsWith(":")) spaces += Indent.Length;

            return new string(' ', spaces);
        }

        private void InsertAtCaret(string insert)
        {
            int pos = Mathf.Clamp(_field.stringPosition, 0, _field.text.Length);
            _suppress = true;
            _field.text = _field.text.Insert(pos, insert);
            _suppress = false;
            _last = _field.text;

            int newPos = pos + insert.Length;
            _field.stringPosition = newPos;
            _field.caretPosition = newPos;
            _field.selectionAnchorPosition = newPos;
            _field.selectionFocusPosition = newPos;
        }
    }
}
