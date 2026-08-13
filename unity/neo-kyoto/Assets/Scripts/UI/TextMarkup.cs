using System.Text;
using System.Text.RegularExpressions;

namespace NeoKyoto.UI
{
    /// <summary>
    /// Turns briefing and debrief text into TMP rich text: section headers stand
    /// out, code blocks read as code, and command names are highlighted inline.
    ///
    /// The source text contains literal angle brackets (`while &lt;condition&gt;:`),
    /// which TMP would otherwise swallow as malformed tags, so everything is escaped
    /// first and only our own tags are added afterwards.
    /// </summary>
    public static class TextMarkup
    {
        private static readonly string Header = ToHex(UITheme.Accent);
        private static readonly string Code = ToHex(UITheme.Good);
        private static readonly string Emphasis = ToHex(UITheme.Warn);

        private static readonly Regex SectionLine =
            new Regex(@"^(\s*)(─── .* ───)\s*$", RegexOptions.Compiled);

        // A call like reroute_next() or check_signal(1) mentioned mid-sentence.
        private static readonly Regex InlineCall =
            new Regex(@"\b([a-z_][a-z0-9_]*)\((.*?)\)", RegexOptions.Compiled);

        // *emphasis* for terms worth flagging in prose.
        private static readonly Regex Emphasised =
            new Regex(@"\*([^*\n]+)\*", RegexOptions.Compiled);

        public static string Format(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return raw;

            var sb = new StringBuilder();
            var lines = raw.Replace("\r\n", "\n").Split('\n');

            foreach (var line in lines)
            {
                string escaped = Escape(line);

                var section = SectionLine.Match(line);
                if (section.Success)
                {
                    sb.Append(section.Groups[1].Value)
                      .Append("<b><color=").Append(Header).Append('>')
                      .Append(Escape(section.Groups[2].Value))
                      .Append("</color></b>");
                }
                else if (IsCodeBlock(line))
                {
                    // Indented sample code: colour the whole line as code.
                    sb.Append("<color=").Append(Code).Append('>').Append(escaped).Append("</color>");
                }
                else
                {
                    string styled = Emphasised.Replace(escaped,
                        m => "<b><color=" + Emphasis + ">" + m.Groups[1].Value + "</color></b>");
                    styled = InlineCall.Replace(styled,
                        m => "<color=" + Code + ">" + m.Value + "</color>");
                    sb.Append(styled);
                }
                sb.Append('\n');
            }

            if (sb.Length > 0) sb.Length -= 1;
            return sb.ToString();
        }

        /// <summary>Sample code in the briefings is indented eight spaces or more.</summary>
        private static bool IsCodeBlock(string line)
        {
            if (line.Trim().Length == 0) return false;
            int spaces = 0;
            while (spaces < line.Length && line[spaces] == ' ') spaces++;
            return spaces >= 8;
        }

        /// <summary>
        /// TMP has no entity escapes, so a literal '&lt;' has to be wrapped in
        /// noparse to survive.
        /// </summary>
        private static string Escape(string text)
        {
            return text.IndexOf('<') < 0 ? text : text.Replace("<", "<noparse><</noparse>");
        }

        private static string ToHex(UnityEngine.Color c)
        {
            return "#" + UnityEngine.ColorUtility.ToHtmlStringRGB(c);
        }
    }
}
