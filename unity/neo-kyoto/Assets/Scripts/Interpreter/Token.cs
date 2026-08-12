using System.Collections.Generic;

namespace NeoKyoto.Interpreter
{
    public enum TokenType
    {
        Name, Number, String,
        Newline, Indent, Dedent, EndOfFile,
        Assign, EqualEqual, NotEqual, Less, Greater, LessEqual, GreaterEqual,
        Plus, Minus, Star, Slash, LParen, RParen, Colon, Comma,
        True, False, None, While, If, Elif, Else, And, Or, Not
    }

    public struct Token
    {
        public readonly TokenType Type;
        public readonly string Text;
        public readonly object Value;
        public readonly int Line;

        public Token(TokenType type, string text, object value, int line)
        {
            Type = type;
            Text = text;
            Value = value;
            Line = line;
        }

        public override string ToString() { return Type + ":" + Text; }

        public static readonly Dictionary<string, TokenType> Keywords = new Dictionary<string, TokenType>
        {
            { "True", TokenType.True },
            { "False", TokenType.False },
            { "None", TokenType.None },
            { "while", TokenType.While },
            { "if", TokenType.If },
            { "elif", TokenType.Elif },
            { "else", TokenType.Else },
            { "and", TokenType.And },
            { "or", TokenType.Or },
            { "not", TokenType.Not },
        };
    }
}
