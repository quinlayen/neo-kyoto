using System.Collections.Generic;

namespace NeoKyoto.Interpreter
{
    /// <summary>
    /// Recursive-descent parser for the Python subset the game teaches:
    /// calls, assignment, while, if/elif/else, comparisons and arithmetic.
    /// </summary>
    public class Parser
    {
        private readonly List<Token> _tokens;
        private int _pos;

        public Parser(List<Token> tokens) { _tokens = tokens; }

        private Token Current { get { return _tokens[_pos]; } }
        private bool Check(TokenType t) { return Current.Type == t; }

        private bool Match(TokenType t)
        {
            if (!Check(t)) return false;
            _pos++;
            return true;
        }

        private Token Expect(TokenType t, string what)
        {
            if (!Check(t)) throw new ScriptSyntaxException("expected " + what, Current.Line);
            Token tok = Current;
            _pos++;
            return tok;
        }

        public List<Stmt> ParseProgram()
        {
            var stmts = new List<Stmt>();
            while (!Check(TokenType.EndOfFile))
            {
                if (Match(TokenType.Newline)) continue;
                stmts.Add(ParseStatement());
            }
            return stmts;
        }

        private List<Stmt> ParseBlock()
        {
            Expect(TokenType.Colon, "':'");
            Expect(TokenType.Newline, "a new line after ':'");
            if (!Check(TokenType.Indent))
            {
                throw new ScriptSyntaxException(
                    "expected an indented block (add 4 spaces to the lines inside)", Current.Line);
            }
            Expect(TokenType.Indent, "an indented block");

            var body = new List<Stmt>();
            while (!Check(TokenType.Dedent) && !Check(TokenType.EndOfFile))
            {
                if (Match(TokenType.Newline)) continue;
                body.Add(ParseStatement());
            }
            Match(TokenType.Dedent);

            if (body.Count == 0)
                throw new ScriptSyntaxException("this block is empty", Current.Line);
            return body;
        }

        private Stmt ParseStatement()
        {
            if (Check(TokenType.While)) return ParseWhile();
            if (Check(TokenType.If)) return ParseIf();
            return ParseSimpleStatement();
        }

        private Stmt ParseWhile()
        {
            int line = Current.Line;
            Expect(TokenType.While, "'while'");
            Expr cond = ParseExpression();
            var body = ParseBlock();
            return new WhileStmt { Condition = cond, Body = body, Line = line };
        }

        private Stmt ParseIf()
        {
            int line = Current.Line;
            _pos++; // consume 'if' or 'elif'
            Expr cond = ParseExpression();
            var body = ParseBlock();
            var node = new IfStmt { Condition = cond, Body = body, Line = line };

            if (Check(TokenType.Elif))
            {
                node.Else = new List<Stmt> { ParseIf() };
            }
            else if (Match(TokenType.Else))
            {
                node.Else = ParseBlock();
            }
            return node;
        }

        private Stmt ParseSimpleStatement()
        {
            int line = Current.Line;

            // Assignment: NAME '=' expr  (lookahead distinguishes it from a bare expression)
            if (Check(TokenType.Name) && _tokens[_pos + 1].Type == TokenType.Assign)
            {
                string target = Current.Text;
                _pos += 2;
                Expr value = ParseExpression();
                ExpectEndOfStatement();
                return new AssignStmt { Target = target, Value = value, Line = line };
            }

            Expr expr = ParseExpression();
            ExpectEndOfStatement();
            return new ExprStmt { Expression = expr, Line = line };
        }

        private void ExpectEndOfStatement()
        {
            if (Check(TokenType.EndOfFile) || Check(TokenType.Dedent)) return;
            if (Match(TokenType.Newline)) return;

            if (Check(TokenType.Assign))
            {
                throw new ScriptSyntaxException(
                    "unexpected '='. Use '==' to compare two values, '=' to store one", Current.Line);
            }
            throw new ScriptSyntaxException("unexpected '" + Current.Text + "'", Current.Line);
        }

        // ─── Expressions, lowest precedence first ───

        private Expr ParseExpression() { return ParseOr(); }

        private Expr ParseOr()
        {
            Expr left = ParseAnd();
            while (Check(TokenType.Or))
            {
                int line = Current.Line;
                _pos++;
                Expr right = ParseAnd();
                left = new BinaryExpr { Op = TokenType.Or, Left = left, Right = right, Line = line };
            }
            return left;
        }

        private Expr ParseAnd()
        {
            Expr left = ParseNot();
            while (Check(TokenType.And))
            {
                int line = Current.Line;
                _pos++;
                Expr right = ParseNot();
                left = new BinaryExpr { Op = TokenType.And, Left = left, Right = right, Line = line };
            }
            return left;
        }

        private Expr ParseNot()
        {
            if (Check(TokenType.Not))
            {
                int line = Current.Line;
                _pos++;
                return new UnaryExpr { Op = TokenType.Not, Operand = ParseNot(), Line = line };
            }
            return ParseComparison();
        }

        private Expr ParseComparison()
        {
            Expr left = ParseArithmetic();
            while (Check(TokenType.EqualEqual) || Check(TokenType.NotEqual) ||
                   Check(TokenType.Less) || Check(TokenType.Greater) ||
                   Check(TokenType.LessEqual) || Check(TokenType.GreaterEqual))
            {
                TokenType op = Current.Type;
                int line = Current.Line;
                _pos++;
                Expr right = ParseArithmetic();
                left = new BinaryExpr { Op = op, Left = left, Right = right, Line = line };
            }
            return left;
        }

        private Expr ParseArithmetic()
        {
            Expr left = ParseTerm();
            while (Check(TokenType.Plus) || Check(TokenType.Minus))
            {
                TokenType op = Current.Type;
                int line = Current.Line;
                _pos++;
                Expr right = ParseTerm();
                left = new BinaryExpr { Op = op, Left = left, Right = right, Line = line };
            }
            return left;
        }

        private Expr ParseTerm()
        {
            Expr left = ParseUnary();
            while (Check(TokenType.Star) || Check(TokenType.Slash))
            {
                TokenType op = Current.Type;
                int line = Current.Line;
                _pos++;
                Expr right = ParseUnary();
                left = new BinaryExpr { Op = op, Left = left, Right = right, Line = line };
            }
            return left;
        }

        private Expr ParseUnary()
        {
            if (Check(TokenType.Minus))
            {
                int line = Current.Line;
                _pos++;
                return new UnaryExpr { Op = TokenType.Minus, Operand = ParseUnary(), Line = line };
            }
            return ParsePrimary();
        }

        private Expr ParsePrimary()
        {
            Token tok = Current;

            switch (tok.Type)
            {
                case TokenType.Number:
                    _pos++;
                    return new LiteralExpr { Value = tok.Value, Line = tok.Line };

                case TokenType.String:
                    _pos++;
                    return new LiteralExpr { Value = tok.Value, Line = tok.Line };

                case TokenType.True:
                    _pos++;
                    return new LiteralExpr { Value = true, Line = tok.Line };

                case TokenType.False:
                    _pos++;
                    return new LiteralExpr { Value = false, Line = tok.Line };

                case TokenType.None:
                    _pos++;
                    return new LiteralExpr { Value = null, Line = tok.Line };

                case TokenType.LParen:
                {
                    _pos++;
                    Expr inner = ParseExpression();
                    Expect(TokenType.RParen, "')'");
                    return inner;
                }

                case TokenType.Name:
                {
                    _pos++;
                    if (Check(TokenType.LParen))
                    {
                        _pos++;
                        var call = new CallExpr { Callee = tok.Text, Line = tok.Line };
                        if (!Check(TokenType.RParen))
                        {
                            call.Args.Add(ParseExpression());
                            while (Match(TokenType.Comma)) call.Args.Add(ParseExpression());
                        }
                        Expect(TokenType.RParen, "')' to close the command");
                        return call;
                    }
                    return new NameExpr { Name = tok.Text, Line = tok.Line };
                }
            }

            if (tok.Type == TokenType.Newline || tok.Type == TokenType.EndOfFile)
                throw new ScriptSyntaxException("this line is incomplete", tok.Line);

            throw new ScriptSyntaxException("unexpected '" + tok.Text + "'", tok.Line);
        }
    }
}
