using System;
using System.Collections.Generic;

namespace NeoKyoto.Interpreter
{
    public enum ExecEventKind { Call, Print }

    /// <summary>
    /// One observable thing the script did. The host consumes these to pace
    /// execution and animate the world, so code runs visibly rather than instantly.
    /// </summary>
    public class ExecEvent
    {
        public ExecEventKind Kind;
        public string Name;     // command name for Call
        public object Result;   // return value for Call
        public string Text;     // printed text for Print
        public int Line;
    }

    public delegate object CommandFunc(List<object> args);

    /// <summary>
    /// Walks the AST. Statement execution is an iterator so the host can step it
    /// one observable event at a time; expression evaluation is synchronous and
    /// queues its events, which the statement loop drains in order.
    /// </summary>
    public class Evaluator
    {
        /// <summary>Backstop for loops that never call a command (e.g. `while True: x = 1`).</summary>
        public int MaxSteps = 200000;
        public int MaxCalls = 20;

        private readonly Dictionary<string, CommandFunc> _commands;
        private readonly Dictionary<string, object> _env = new Dictionary<string, object>();
        private readonly List<ExecEvent> _pending = new List<ExecEvent>();

        private int _steps;
        private int _calls;

        public int CallCount { get { return _calls; } }

        public Evaluator(Dictionary<string, CommandFunc> commands)
        {
            _commands = commands ?? new Dictionary<string, CommandFunc>();
        }

        public IEnumerable<ExecEvent> Run(List<Stmt> program)
        {
            foreach (var ev in ExecBlock(program)) yield return ev;
        }

        private IEnumerable<ExecEvent> ExecBlock(List<Stmt> body)
        {
            for (int i = 0; i < body.Count; i++)
                foreach (var ev in ExecStmt(body[i]))
                    yield return ev;
        }

        private IEnumerable<ExecEvent> ExecStmt(Stmt stmt)
        {
            Tick();

            var exprStmt = stmt as ExprStmt;
            if (exprStmt != null)
            {
                Eval(exprStmt.Expression);
                foreach (var ev in Drain()) yield return ev;
                yield break;
            }

            var assign = stmt as AssignStmt;
            if (assign != null)
            {
                object value = Eval(assign.Value);
                _env[assign.Target] = value;
                foreach (var ev in Drain()) yield return ev;
                yield break;
            }

            var whileStmt = stmt as WhileStmt;
            if (whileStmt != null)
            {
                while (true)
                {
                    Tick();
                    bool go = PyValue.Truthy(Eval(whileStmt.Condition));
                    foreach (var ev in Drain()) yield return ev;
                    if (!go) break;
                    foreach (var ev in ExecBlock(whileStmt.Body)) yield return ev;
                }
                yield break;
            }

            var ifStmt = stmt as IfStmt;
            if (ifStmt != null)
            {
                bool go = PyValue.Truthy(Eval(ifStmt.Condition));
                foreach (var ev in Drain()) yield return ev;
                if (go)
                {
                    foreach (var ev in ExecBlock(ifStmt.Body)) yield return ev;
                }
                else if (ifStmt.Else != null)
                {
                    foreach (var ev in ExecBlock(ifStmt.Else)) yield return ev;
                }
                yield break;
            }

            throw new ScriptRuntimeException("unsupported statement", stmt.Line);
        }

        private List<ExecEvent> Drain()
        {
            if (_pending.Count == 0) return EmptyEvents;
            var copy = new List<ExecEvent>(_pending);
            _pending.Clear();
            return copy;
        }

        private static readonly List<ExecEvent> EmptyEvents = new List<ExecEvent>();

        private void Tick()
        {
            _steps++;
            if (_steps > MaxSteps)
            {
                throw new SandboxStopException(
                    "Sandbox auto-stopped: the script ran too long without finishing.\n" +
                    "Check for a loop that never ends and never calls a command.");
            }
        }

        // ─── Expressions ───

        private object Eval(Expr expr)
        {
            var lit = expr as LiteralExpr;
            if (lit != null) return lit.Value;

            var name = expr as NameExpr;
            if (name != null)
            {
                object v;
                if (_env.TryGetValue(name.Name, out v)) return v;
                throw new ScriptNameException(name.Name);
            }

            var call = expr as CallExpr;
            if (call != null) return EvalCall(call);

            var unary = expr as UnaryExpr;
            if (unary != null)
            {
                object operand = Eval(unary.Operand);
                if (unary.Op == TokenType.Not) return !PyValue.Truthy(operand);
                if (unary.Op == TokenType.Minus)
                {
                    if (operand is long) return -(long)operand;
                    if (operand is double) return -(double)operand;
                    throw new ScriptRuntimeException(
                        "cannot negate a " + PyValue.TypeName(operand), unary.Line);
                }
            }

            var bin = expr as BinaryExpr;
            if (bin != null) return EvalBinary(bin);

            throw new ScriptRuntimeException("unsupported expression", expr.Line);
        }

        private object EvalBinary(BinaryExpr bin)
        {
            // Short-circuit operators evaluate the right side only when needed.
            if (bin.Op == TokenType.And)
            {
                object left = Eval(bin.Left);
                return PyValue.Truthy(left) ? Eval(bin.Right) : left;
            }
            if (bin.Op == TokenType.Or)
            {
                object left = Eval(bin.Left);
                return PyValue.Truthy(left) ? left : Eval(bin.Right);
            }

            object a = Eval(bin.Left);
            object b = Eval(bin.Right);

            switch (bin.Op)
            {
                case TokenType.EqualEqual: return PyValue.AreEqual(a, b);
                case TokenType.NotEqual: return !PyValue.AreEqual(a, b);
            }

            if (bin.Op == TokenType.Plus && (a is string || b is string))
            {
                if (a is string && b is string) return (string)a + (string)b;
                throw new ScriptRuntimeException(
                    "cannot add a " + PyValue.TypeName(a) + " and a " + PyValue.TypeName(b), bin.Line);
            }

            if (!PyValue.IsNumber(a) || !PyValue.IsNumber(b))
            {
                throw new ScriptRuntimeException(
                    "cannot compare or combine a " + PyValue.TypeName(a) +
                    " and a " + PyValue.TypeName(b), bin.Line);
            }

            bool bothInts = a is long && b is long;
            double x = PyValue.ToDouble(a);
            double y = PyValue.ToDouble(b);

            switch (bin.Op)
            {
                case TokenType.Less: return x < y;
                case TokenType.Greater: return x > y;
                case TokenType.LessEqual: return x <= y;
                case TokenType.GreaterEqual: return x >= y;
                case TokenType.Plus: return bothInts ? (object)((long)a + (long)b) : (object)(x + y);
                case TokenType.Minus: return bothInts ? (object)((long)a - (long)b) : (object)(x - y);
                case TokenType.Star: return bothInts ? (object)((long)a * (long)b) : (object)(x * y);
                case TokenType.Slash:
                    if (y == 0) throw new ScriptRuntimeException("division by zero", bin.Line);
                    return x / y;
            }

            throw new ScriptRuntimeException("unsupported operator", bin.Line);
        }

        private object EvalCall(CallExpr call)
        {
            var args = new List<object>();
            for (int i = 0; i < call.Args.Count; i++) args.Add(Eval(call.Args[i]));

            if (call.Callee == "print")
            {
                var parts = new string[args.Count];
                for (int i = 0; i < args.Count; i++) parts[i] = PyValue.Str(args[i]);
                string text = string.Join(" ", parts);
                _pending.Add(new ExecEvent
                {
                    Kind = ExecEventKind.Print,
                    Text = text,
                    Line = call.Line
                });
                return null;
            }

            CommandFunc fn;
            if (!_commands.TryGetValue(call.Callee, out fn))
                throw new ScriptNameException(call.Callee);

            _calls++;
            if (_calls > MaxCalls)
            {
                throw new SandboxStopException(
                    "Sandbox auto-stopped after " + MaxCalls + " calls (loop safety limit).");
            }

            object result;
            try
            {
                result = fn(args);
            }
            catch (SandboxStopException) { throw; }
            catch (ScriptRuntimeException) { throw; }
            catch (ArgumentException e)
            {
                throw new ScriptRuntimeException(e.Message, call.Line);
            }

            _pending.Add(new ExecEvent
            {
                Kind = ExecEventKind.Call,
                Name = call.Callee,
                Result = result,
                Line = call.Line
            });
            return result;
        }
    }
}
