using System.Collections.Generic;

namespace NeoKyoto.Interpreter
{
    public abstract class Node { public int Line; }

    public abstract class Expr : Node { }

    public class LiteralExpr : Expr { public object Value; }

    public class NameExpr : Expr { public string Name; }

    public class CallExpr : Expr
    {
        public string Callee;
        public List<Expr> Args = new List<Expr>();
    }

    public class BinaryExpr : Expr
    {
        public TokenType Op;
        public Expr Left;
        public Expr Right;
    }

    public class UnaryExpr : Expr
    {
        public TokenType Op;
        public Expr Operand;
    }

    public abstract class Stmt : Node { }

    public class ExprStmt : Stmt { public Expr Expression; }

    public class AssignStmt : Stmt
    {
        public string Target;
        public Expr Value;
    }

    public class WhileStmt : Stmt
    {
        public Expr Condition;
        public List<Stmt> Body = new List<Stmt>();
    }

    public class IfStmt : Stmt
    {
        public Expr Condition;
        public List<Stmt> Body = new List<Stmt>();
        public List<Stmt> Else; // null when there is no else/elif branch
    }
}
