using System;
using System.Collections.Generic;
using NeoKyoto.Core;

namespace NeoKyoto.Interpreter
{
    /// <summary>
    /// The sandbox the player's script runs inside. Mirrors RestrictedInterpreter
    /// from the Python prototype: feature gates checked before execution, a call
    /// cap that safely ends `while True` loops, and player-facing error messages.
    /// </summary>
    public class ScriptRunner
    {
        public int MaxCalls = 20;

        private readonly GameState _state;
        private Dictionary<string, CommandFunc> _commands = new Dictionary<string, CommandFunc>();
        private List<Stmt> _program;
        private Evaluator _evaluator;

        public ScriptRunner(GameState state, int maxCalls)
        {
            _state = state;
            MaxCalls = maxCalls;
        }

        private static readonly Dictionary<Feature, string> FeatureNames = new Dictionary<Feature, string>
        {
            { Feature.Loops, "while loops" },
            { Feature.Conditionals, "if/else conditionals" },
            { Feature.ForLoops, "for loops" },
            { Feature.Functions, "function definitions (def)" },
        };

        /// <summary>
        /// Active contract commands plus no-op stand-ins for commands from
        /// contracts already finished.
        /// </summary>
        public void SetCommands(Dictionary<string, CommandFunc> active, Action<string> retiredOutput)
        {
            var merged = new Dictionary<string, CommandFunc>();

            foreach (var name in _state.RetiredCommands)
            {
                if (active != null && active.ContainsKey(name)) continue;
                string captured = name;
                merged[captured] = delegate (List<object> args)
                {
                    if (retiredOutput != null)
                        retiredOutput("  [" + captured + "] System already stable. No action needed.");
                    return null;
                };
            }

            if (active != null)
                foreach (var kv in active) merged[kv.Key] = kv.Value;

            _commands = merged;
        }

        public IEnumerable<string> AvailableCommands { get { return _commands.Keys; } }

        /// <summary>
        /// Lexes, parses and gate-checks the source. Returns false with a
        /// player-facing message when the script cannot run.
        /// </summary>
        public bool Prepare(string code, out string error)
        {
            error = null;
            try
            {
                var tokens = new Lexer(code).Tokenize();
                _program = new Parser(tokens).ParseProgram();
            }
            catch (ScriptSyntaxException e)
            {
                error = "Syntax error on line " + e.Line + ": " + e.Message + "\n\nCheck your code for typos.";
                return false;
            }

            string gate = CheckFeatureGates(_program);
            if (gate != null) { error = gate; return false; }

            LastProgramUsedLoop = ContainsWhile(_program);

            _evaluator = new Evaluator(_commands) { MaxCalls = MaxCalls };
            return true;
        }

        /// <summary>
        /// Whether the script just prepared contains a loop. Lets the game react to
        /// the player actually adopting a new tool rather than merely unlocking it.
        /// </summary>
        public bool LastProgramUsedLoop { get; private set; }

        private static bool ContainsWhile(List<Stmt> body)
        {
            foreach (var stmt in body)
            {
                if (stmt is WhileStmt) return true;

                var ifStmt = stmt as IfStmt;
                if (ifStmt != null)
                {
                    if (ContainsWhile(ifStmt.Body)) return true;
                    if (ifStmt.Else != null && ContainsWhile(ifStmt.Else)) return true;
                }
            }
            return false;
        }

        public IEnumerator<ExecEvent> Execute()
        {
            if (_program == null) return null;
            return _evaluator.Run(_program).GetEnumerator();
        }

        public int CallCount { get { return _evaluator != null ? _evaluator.CallCount : 0; } }

        /// <summary>Calls the script made from inside a loop body.</summary>
        public int CallsInsideLoop { get { return _evaluator != null ? _evaluator.CallsInsideLoop : 0; } }

        /// <summary>
        /// True when a loop actually carried the work, rather than merely appearing
        /// in the source. A decorative `while False:` above a block of repeated calls
        /// contains a loop but performs nothing inside it.
        /// </summary>
        public bool LoopDidTheWork(int callsToGoal)
        {
            if (callsToGoal <= 0) return false;
            return CallsInsideLoop * 2 >= callsToGoal;
        }

        private string CheckFeatureGates(List<Stmt> body)
        {
            foreach (var stmt in body)
            {
                var whileStmt = stmt as WhileStmt;
                if (whileStmt != null)
                {
                    if (!_state.IsUnlocked(Feature.Loops)) return GateMessage(Feature.Loops);
                    string nested = CheckFeatureGates(whileStmt.Body);
                    if (nested != null) return nested;
                    continue;
                }

                var ifStmt = stmt as IfStmt;
                if (ifStmt != null)
                {
                    if (!_state.IsUnlocked(Feature.Conditionals)) return GateMessage(Feature.Conditionals);
                    string nested = CheckFeatureGates(ifStmt.Body);
                    if (nested != null) return nested;
                    if (ifStmt.Else != null)
                    {
                        nested = CheckFeatureGates(ifStmt.Else);
                        if (nested != null) return nested;
                    }
                }
            }
            return null;
        }

        private string GateMessage(Feature feature)
        {
            return "Error: " + FeatureNames[feature] + " are not available yet.\n" +
                   "Complete the current contract to unlock new tools.";
        }

        /// <summary>Converts an exception thrown mid-execution into player-facing text.</summary>
        public string DescribeException(Exception e)
        {
            var stop = e as SandboxStopException;
            if (stop != null) return stop.Message;

            var nameErr = e as ScriptNameException;
            if (nameErr != null)
            {
                var names = new List<string>(_commands.Keys);
                names.Add("print");
                names.Sort();
                return "Error: name '" + nameErr.Name + "' is not defined\n\n" +
                       "That command does not exist yet.\n" +
                       "Available commands: " + string.Join(", ", names.ToArray());
            }

            var runtime = e as ScriptRuntimeException;
            if (runtime != null) return "Error on line " + runtime.Line + ": " + runtime.Message;

            var syntax = e as ScriptSyntaxException;
            if (syntax != null) return "Syntax error on line " + syntax.Line + ": " + syntax.Message;

            return "Error: " + e.Message;
        }
    }
}
