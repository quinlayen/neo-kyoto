using System;

namespace NeoKyoto.Interpreter
{
    /// <summary>Raised for malformed source. Reported with a line number.</summary>
    public class ScriptSyntaxException : Exception
    {
        public readonly int Line;
        public ScriptSyntaxException(string message, int line) : base(message) { Line = line; }
    }

    /// <summary>Raised when the sandbox halts a script (call cap or runaway loop). Not a player error.</summary>
    public class SandboxStopException : Exception
    {
        public SandboxStopException(string message) : base(message) { }
    }

    /// <summary>Raised when a name is used that does not exist in the current scope.</summary>
    public class ScriptNameException : Exception
    {
        public readonly string Name;
        public ScriptNameException(string name)
            : base("name '" + name + "' is not defined") { Name = name; }
    }

    /// <summary>Raised for runtime faults (bad types, bad argument counts).</summary>
    public class ScriptRuntimeException : Exception
    {
        public readonly int Line;
        public ScriptRuntimeException(string message, int line) : base(message) { Line = line; }
    }
}
