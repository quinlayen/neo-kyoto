using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace NeoKyoto.Interpreter
{
    /// <summary>
    /// Turns Python-subset source into tokens. Indentation is significant: the lexer
    /// works line-by-line and emits Indent/Dedent tokens, which is enough for the
    /// subset we support (no line continuations, no multi-line expressions).
    /// </summary>
    public class Lexer
    {
        private readonly string[] _lines;
        private readonly List<Token> _tokens = new List<Token>();
        private readonly List<int> _indents = new List<int> { 0 };

        public Lexer(string source)
        {
            source = (source ?? string.Empty).Replace("\r\n", "\n").Replace("\r", "\n");
            _lines = source.Split('\n');
        }

        public List<Token> Tokenize()
        {
            for (int i = 0; i < _lines.Length; i++)
            {
                int lineNo = i + 1;
                string expanded = _lines[i].Replace("\t", "    ");
                string trimmed = expanded.Trim();

                // Blank lines and comment-only lines carry no structure.
                if (trimmed.Length == 0 || trimmed[0] == '#') continue;

                int indent = 0;
                while (indent < expanded.Length && expanded[indent] == ' ') indent++;

                if (indent > _indents[_indents.Count - 1])
                {
                    _indents.Add(indent);
                    Add(TokenType.Indent, "", null, lineNo);
                }
                else
                {
                    while (indent < _indents[_indents.Count - 1])
                    {
                        _indents.RemoveAt(_indents.Count - 1);
                        Add(TokenType.Dedent, "", null, lineNo);
                    }
                    if (indent != _indents[_indents.Count - 1])
                    {
                        throw new ScriptSyntaxException(
                            "unindent does not match any outer indentation level", lineNo);
                    }
                }

                TokenizeLine(expanded, indent, lineNo);
                Add(TokenType.Newline, "", null, lineNo);
            }

            int lastLine = _lines.Length;
            while (_indents.Count > 1)
            {
                _indents.RemoveAt(_indents.Count - 1);
                Add(TokenType.Dedent, "", null, lastLine);
            }
            Add(TokenType.EndOfFile, "", null, lastLine);
            return _tokens;
        }

        private void TokenizeLine(string line, int start, int lineNo)
        {
            int i = start;
            while (i < line.Length)
            {
                char c = line[i];

                if (c == ' ') { i++; continue; }
                if (c == '#') return;

                if (char.IsLetter(c) || c == '_')
                {
                    int begin = i;
                    while (i < line.Length && (char.IsLetterOrDigit(line[i]) || line[i] == '_')) i++;
                    string word = line.Substring(begin, i - begin);
                    TokenType kw;
                    if (Token.Keywords.TryGetValue(word, out kw)) Add(kw, word, null, lineNo);
                    else Add(TokenType.Name, word, null, lineNo);
                    continue;
                }

                if (char.IsDigit(c))
                {
                    int begin = i;
                    bool isFloat = false;
                    while (i < line.Length && (char.IsDigit(line[i]) || line[i] == '.'))
                    {
                        if (line[i] == '.')
                        {
                            if (isFloat) break;
                            isFloat = true;
                        }
                        i++;
                    }
                    string num = line.Substring(begin, i - begin);
                    object value;
                    if (isFloat) value = double.Parse(num, CultureInfo.InvariantCulture);
                    else value = long.Parse(num, CultureInfo.InvariantCulture);
                    Add(TokenType.Number, num, value, lineNo);
                    continue;
                }

                if (c == '"' || c == '\'')
                {
                    char quote = c;
                    i++;
                    var sb = new StringBuilder();
                    bool closed = false;
                    while (i < line.Length)
                    {
                        if (line[i] == '\\' && i + 1 < line.Length)
                        {
                            char esc = line[i + 1];
                            if (esc == 'n') sb.Append('\n');
                            else if (esc == 't') sb.Append('\t');
                            else sb.Append(esc);
                            i += 2;
                            continue;
                        }
                        if (line[i] == quote) { closed = true; i++; break; }
                        sb.Append(line[i]);
                        i++;
                    }
                    if (!closed) throw new ScriptSyntaxException("unterminated string literal", lineNo);
                    Add(TokenType.String, sb.ToString(), sb.ToString(), lineNo);
                    continue;
                }

                // Two-character operators first.
                if (i + 1 < line.Length)
                {
                    string two = line.Substring(i, 2);
                    if (two == "==") { Add(TokenType.EqualEqual, two, null, lineNo); i += 2; continue; }
                    if (two == "!=") { Add(TokenType.NotEqual, two, null, lineNo); i += 2; continue; }
                    if (two == "<=") { Add(TokenType.LessEqual, two, null, lineNo); i += 2; continue; }
                    if (two == ">=") { Add(TokenType.GreaterEqual, two, null, lineNo); i += 2; continue; }
                }

                switch (c)
                {
                    case '=': Add(TokenType.Assign, "=", null, lineNo); break;
                    case '<': Add(TokenType.Less, "<", null, lineNo); break;
                    case '>': Add(TokenType.Greater, ">", null, lineNo); break;
                    case '+': Add(TokenType.Plus, "+", null, lineNo); break;
                    case '-': Add(TokenType.Minus, "-", null, lineNo); break;
                    case '*': Add(TokenType.Star, "*", null, lineNo); break;
                    case '/': Add(TokenType.Slash, "/", null, lineNo); break;
                    case '(': Add(TokenType.LParen, "(", null, lineNo); break;
                    case ')': Add(TokenType.RParen, ")", null, lineNo); break;
                    case ':': Add(TokenType.Colon, ":", null, lineNo); break;
                    case ',': Add(TokenType.Comma, ",", null, lineNo); break;
                    default:
                        throw new ScriptSyntaxException("invalid character '" + c + "'", lineNo);
                }
                i++;
            }
        }

        private void Add(TokenType type, string text, object value, int line)
        {
            _tokens.Add(new Token(type, text, value, line));
        }
    }
}
