using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace NeoKyoto.UI
{
    /// <summary>
    /// Turns briefing and debrief text into TMP rich text.
    ///
    /// The source is written for an 80-column terminal: prose is hard-wrapped at
    /// roughly 45 characters and headers are drawn with box-art. Rendering that
    /// verbatim leaves most of the work panel empty, so paragraphs are unwrapped
    /// back into single lines and handed to TMP, which wraps them to whatever width
    /// the panel actually is. Box-art becomes a real header; sample code becomes a
    /// backed block instead of differently-coloured prose.
    ///
    /// The source text also contains literal angle brackets (`while &lt;condition&gt;:`),
    /// which TMP would otherwise swallow as malformed tags, so everything is escaped
    /// first and only our own tags are added afterwards.
    /// </summary>
    public static class TextMarkup
    {
        private static readonly string Header = ToHex(UITheme.Accent);
        private static readonly string Code = ToHex(UITheme.Good);
        private static readonly string Emphasis = ToHex(UITheme.Warn);
        private static readonly string Dim = ToHex(UITheme.TextDim);
        private static readonly string CodeBg = ToHex(UITheme.CodeBg) + "FF";

        private static readonly Regex SectionLine =
            new Regex(@"^\s*(─── .* ───)\s*$", RegexOptions.Compiled);

        // The generated header box: ╔══╗ / ║ text ║ / ╚══╝.
        private static readonly Regex BoxBorder =
            new Regex(@"^\s*[╔╚][═]*[╗╝]\s*$", RegexOptions.Compiled);
        private static readonly Regex BoxContent =
            new Regex(@"^\s*║(.*)║\s*$", RegexOptions.Compiled);

        // A call like reroute_next() or check_signal(1) mentioned mid-sentence.
        private static readonly Regex InlineCall =
            new Regex(@"\b([a-z_][a-z0-9_]*)\((.*?)\)", RegexOptions.Compiled);

        // *emphasis* for terms worth flagging in prose.
        private static readonly Regex Emphasised =
            new Regex(@"\*([^*\n]+)\*", RegexOptions.Compiled);

        public static string Format(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return raw;

            var lines = raw.Replace("\r\n", "\n").Split('\n');
            var outLines = new List<string>();

            var prose = new List<string>();
            var code = new List<string>();
            var box = new List<string>();

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                if (BoxBorder.IsMatch(line)) { FlushProse(prose, outLines); FlushCode(code, outLines); continue; }

                var boxed = BoxContent.Match(line);
                if (boxed.Success)
                {
                    FlushProse(prose, outLines);
                    FlushCode(code, outLines);
                    string inner = boxed.Groups[1].Value.Trim();
                    if (inner.Length > 0) box.Add(inner);
                    continue;
                }
                FlushBox(box, outLines);

                var section = SectionLine.Match(line);
                if (section.Success)
                {
                    FlushProse(prose, outLines);
                    FlushCode(code, outLines);
                    outLines.Add("<b><color=" + Header + ">" +
                                 Escape(section.Groups[1].Value) + "</color></b>");
                    continue;
                }

                if (IsCodeBlock(line))
                {
                    FlushProse(prose, outLines);
                    code.Add(line);
                    continue;
                }

                if (line.Trim().Length == 0)
                {
                    FlushProse(prose, outLines);
                    FlushCode(code, outLines);
                    outLines.Add("");
                    continue;
                }

                FlushCode(code, outLines);
                prose.Add(line.Trim());
            }

            FlushProse(prose, outLines);
            FlushCode(code, outLines);
            FlushBox(box, outLines);

            return string.Join("\n", Collapse(outLines).ToArray());
        }

        /// <summary>
        /// Joins the hard-wrapped source lines of one paragraph back into a single
        /// line so the panel can wrap it to its own width.
        /// </summary>
        private static void FlushProse(List<string> prose, List<string> outLines)
        {
            if (prose.Count == 0) return;

            string joined = Escape(string.Join(" ", prose.ToArray()));
            prose.Clear();

            string styled = Emphasised.Replace(joined,
                m => "<b><color=" + Emphasis + ">" + m.Groups[1].Value + "</color></b>");
            styled = InlineCall.Replace(styled,
                m => "<color=" + Code + ">" + m.Value + "</color>");

            outLines.Add(styled);
        }

        /// <summary>
        /// Renders sample code as a backed block. Every line in the block is padded
        /// to the same width so the highlight forms a rectangle rather than a ragged
        /// edge, which only works because the whole UI is monospaced.
        /// </summary>
        private static void FlushCode(List<string> code, List<string> outLines)
        {
            if (code.Count == 0) return;

            // Re-indent to the block's own left edge; the panel already has padding.
            int common = int.MaxValue;
            foreach (var line in code)
            {
                if (line.Trim().Length == 0) continue;
                int n = 0;
                while (n < line.Length && line[n] == ' ') n++;
                if (n < common) common = n;
            }
            if (common == int.MaxValue) common = 0;

            var trimmed = new List<string>();
            int width = 0;
            foreach (var line in code)
            {
                string s = line.Length > common ? line.Substring(common) : line.Trim();
                s = s.TrimEnd();
                trimmed.Add(s);
                if (s.Length > width) width = s.Length;
            }
            code.Clear();

            // No blank rows top and bottom: TMP gives a glyphless line no highlight
            // geometry, so they cost vertical space and draw nothing. The blank source
            // lines either side of the block already separate it from the prose.
            foreach (var line in trimmed) outLines.Add(MarkedRow(line, width));
        }

        /// <summary>
        /// TMP trims ordinary trailing spaces and drops whitespace-only lines, which
        /// would leave the highlight ragged and swallow the block's top and bottom
        /// padding — so the padding is non-breaking spaces, which survive.
        /// </summary>
        private const char Pad = ' ';

        private static string MarkedRow(string content, int width)
        {
            var padded = new StringBuilder();
            padded.Append(Pad, 2).Append(content).Append(Pad, width - content.Length + 2);
            return "<nobr><mark=" + CodeBg + "><color=" + Code + ">" +
                   Escape(padded.ToString()) + "</color></mark></nobr>";
        }

        /// <summary>The box-art header becomes a title and a subtitle.</summary>
        private static void FlushBox(List<string> box, List<string> outLines)
        {
            if (box.Count == 0) return;

            outLines.Add("<b><color=" + Header + "><size=125%>" +
                         Escape(box[0]) + "</size></color></b>");
            for (int i = 1; i < box.Count; i++)
                outLines.Add("<color=" + Dim + ">" + Escape(box[i]) + "</color>");
            outLines.Add("");
            box.Clear();
        }

        /// <summary>Trims leading/trailing blanks and never leaves two in a row.</summary>
        private static List<string> Collapse(List<string> lines)
        {
            var result = new List<string>();
            foreach (var line in lines)
            {
                bool blank = line.Length == 0;
                if (blank && (result.Count == 0 || result[result.Count - 1].Length == 0)) continue;
                result.Add(line);
            }
            while (result.Count > 0 && result[result.Count - 1].Length == 0)
                result.RemoveAt(result.Count - 1);
            return result;
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
