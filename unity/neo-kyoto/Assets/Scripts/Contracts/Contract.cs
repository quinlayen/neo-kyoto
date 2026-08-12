using System;
using System.Collections.Generic;
using NeoKyoto.Interpreter;
using NeoKyoto.Systems;

namespace NeoKyoto.Contracts
{
    public enum ContractKind { Python, Terminal, Combined }

    /// <summary>
    /// One job on the contract board. Subclasses own their backing system and
    /// decide what "done" means.
    /// </summary>
    public abstract class Contract
    {
        public abstract string Id { get; }
        public abstract string Title { get; }
        public abstract string Location { get; }

        public virtual ContractKind Kind { get { return ContractKind.Python; } }
        public virtual int MaxCalls { get { return 20; } }
        public virtual string ScriptName { get { return ""; } }
        public virtual string StarterScript { get { return ""; } }

        public bool Completed;
        private bool _completionAnnounced;

        /// <summary>Where system chatter (the old print output) is written.</summary>
        public Action<string> Output;

        /// <summary>Raised when the backing system changes, so the world view can react.</summary>
        public event Action SystemChanged;

        protected void RaiseSystemChanged() { if (SystemChanged != null) SystemChanged(); }

        public abstract string GetBriefing();
        public abstract string GetCompletionMessage();
        public abstract string GetCompletedBanner();
        public abstract bool IsGoalMet();
        public abstract string GetStatusText();
        public abstract void ResetSystem();

        public virtual Dictionary<string, CommandFunc> GetCommands()
        {
            return new Dictionary<string, CommandFunc>();
        }

        public bool UpdateCompletion()
        {
            if (!Completed && IsGoalMet()) Completed = true;
            return Completed;
        }

        /// <summary>
        /// Marks a replayed contract as already announced, so re-solving it does
        /// not fire the debrief a second time.
        /// </summary>
        public void SuppressCompletionAnnouncement() { _completionAnnounced = true; }

        public bool ConsumeCompletionAnnouncement()
        {
            UpdateCompletion();
            if (Completed && !_completionAnnounced)
            {
                _completionAnnounced = true;
                return true;
            }
            return false;
        }

        protected void Print(string text) { if (Output != null) Output(text); }

        /// <summary>
        /// Reads a whole-number argument. Throws ArgumentException, which the
        /// evaluator turns into a player-facing runtime error.
        /// </summary>
        protected static int ArgInt(List<object> args, int index, string usage)
        {
            if (args == null || args.Count <= index)
                throw new ArgumentException("this command needs a number, for example " + usage);
            object v = args[index];
            if (v is long) return (int)(long)v;
            if (v is double) return (int)(double)v;
            throw new ArgumentException(
                "expected a number but got a " + PyValue.TypeName(v) + ", for example " + usage);
        }
    }

    /// <summary>A contract the player solves by typing shell commands.</summary>
    public abstract class TerminalContract : Contract
    {
        public override ContractKind Kind { get { return ContractKind.Terminal; } }

        public VirtualFilesystem Fs;
        public TerminalInterpreter Terminal;

        protected TerminalContract() { ResetSystem(); }

        public abstract VirtualFilesystem BuildFilesystem();

        public override void ResetSystem()
        {
            Fs = BuildFilesystem();
            Terminal = new TerminalInterpreter(Fs);
            RaiseSystemChanged();
        }

        public virtual string OnCommand(string commandLine)
        {
            string output = Terminal.Execute(commandLine);
            UpdateCompletion();
            RaiseSystemChanged();
            return output;
        }

        public string GetPrompt() { return Terminal.GetPrompt(); }
    }
}
