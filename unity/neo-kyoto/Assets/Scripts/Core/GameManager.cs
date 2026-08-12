using System;
using System.Collections;
using System.Collections.Generic;
using NeoKyoto.Contracts;
using NeoKyoto.Interpreter;
using UnityEngine;

namespace NeoKyoto.Core
{
    public enum GameScreen { Title, Board, Briefing, Workspace, Debrief }

    /// <summary>
    /// Owns game flow and script execution. Presentation lives in UIController and
    /// the world views; both listen to the events here rather than polling.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Tooltip("Unlock every contract and language feature — for testing.")]
        public bool unlockAllForTesting;

        [Tooltip("Seconds between observable script events, so code runs visibly in the world.")]
        public float stepDelay = 0.12f;

        public GameState State { get; private set; }
        public GameScreen CurrentScreen { get; private set; }
        public Contract ActiveContract { get; private set; }
        public ContractDef ActiveDef { get; private set; }
        public bool IsRunning { get; private set; }

        private readonly List<string> _console = new List<string>();
        private readonly Dictionary<string, string> _scripts = new Dictionary<string, string>();
        private ScriptRunner _runner;

        public IList<string> ConsoleLines { get { return _console; } }

        public event Action ScreenChanged;
        public event Action ConsoleChanged;
        public event Action StatusChanged;

        private const int MaxConsoleLines = 400;

        private void Awake()
        {
            Instance = this;
            State = new GameState();
            if (unlockAllForTesting) State.UnlockAll(ContractRegistry.AllIds());
            CurrentScreen = GameScreen.Title;
        }

        // ─── Navigation ───

        public void GoTo(GameScreen screen)
        {
            CurrentScreen = screen;
            if (ScreenChanged != null) ScreenChanged();
        }

        public void StartGame()
        {
            // First run drops straight into C1; afterwards the board is the hub.
            if (!State.IsContractCompleted("contract_01")) OpenContract(ContractRegistry.All[0]);
            else GoTo(GameScreen.Board);
        }

        public bool IsAvailable(int index)
        {
            if (index == 0) return true;
            return State.IsContractCompleted(ContractRegistry.All[index - 1].Id);
        }

        public void OpenContract(ContractDef def)
        {
            ActiveDef = def;
            ActiveContract = def.Create();
            ActiveContract.Output = AppendConsole;
            // A replayed contract starts unsolved: its system is rebuilt from scratch,
            // and solving it again simply shows the debrief again.

            if (!_scripts.ContainsKey(def.Id)) _scripts[def.Id] = ActiveContract.StarterScript;

            _runner = new ScriptRunner(State, ActiveContract.MaxCalls);
            _console.Clear();

            var terminal = ActiveContract as TerminalContract;
            if (terminal != null) AppendConsole(terminal.GetPrompt());

            GoTo(GameScreen.Briefing);
        }

        public void BeginWork() { GoTo(GameScreen.Workspace); }

        public void BackToBoard()
        {
            ActiveContract = null;
            ActiveDef = null;
            GoTo(GameScreen.Board);
        }

        // ─── Script editing ───

        public string GetScript()
        {
            if (ActiveDef == null) return "";
            string code;
            return _scripts.TryGetValue(ActiveDef.Id, out code) ? code : "";
        }

        public void SetScript(string code)
        {
            if (ActiveDef == null) return;
            _scripts[ActiveDef.Id] = code;
        }

        // ─── Console ───

        public void AppendConsole(string line)
        {
            if (line == null) return;
            foreach (var part in line.Replace("\r\n", "\n").Split('\n')) _console.Add(part);
            while (_console.Count > MaxConsoleLines) _console.RemoveAt(0);
            if (ConsoleChanged != null) ConsoleChanged();
        }

        public void ClearConsole()
        {
            _console.Clear();
            if (ConsoleChanged != null) ConsoleChanged();
        }

        private void RaiseStatus() { if (StatusChanged != null) StatusChanged(); }

        // ─── Running the player's script ───

        public void RunScript()
        {
            if (IsRunning || ActiveContract == null) return;
            if (ActiveContract.Kind == ContractKind.Terminal) return;
            StartCoroutine(RunScriptRoutine());
        }

        private IEnumerator RunScriptRoutine()
        {
            IsRunning = true;
            ClearConsole();

            // Each run starts from a clean system, matching the prototype.
            ActiveContract.ResetSystem();
            RaiseStatus();

            _runner.SetCommands(ActiveContract.GetCommands(), AppendConsole);

            AppendConsole("┌─ Running script ─────────────────────────");

            string error;
            if (!_runner.Prepare(GetScript(), out error))
            {
                AppendConsole(error);
                AppendConsole("└──────────────────────────────────────────");
                IsRunning = false;
                RaiseStatus();
                yield break;
            }

            var it = _runner.Execute();
            string endMessage = "Script executed successfully.";

            while (true)
            {
                bool moved;
                try
                {
                    moved = it.MoveNext();
                }
                catch (Exception e)
                {
                    endMessage = _runner.DescribeException(e);
                    break;
                }
                if (!moved) break;

                var ev = it.Current;
                if (ev.Kind == ExecEventKind.Print) AppendConsole(PyValue.Str(ev.Text));
                RaiseStatus();

                if (stepDelay > 0f) yield return new WaitForSeconds(stepDelay);
                else yield return null;
            }

            AppendConsole(endMessage);
            AppendConsole("└──────────────────────────────────────────");
            RaiseStatus();

            CheckCompletion();
            IsRunning = false;
        }

        // ─── Terminal contracts ───

        public void SubmitTerminalCommand(string commandLine)
        {
            var terminal = ActiveContract as TerminalContract;
            if (terminal == null) return;

            commandLine = (commandLine ?? "").Trim();
            AppendConsole(terminal.GetPrompt() + commandLine);
            if (commandLine.Length == 0) return;

            if (commandLine == "clear") { ClearConsole(); return; }

            if (commandLine == "reset")
            {
                terminal.ResetSystem();
                AppendConsole("  System reset. Filesystem restored to initial state.");
                RaiseStatus();
                return;
            }

            string output = terminal.OnCommand(commandLine);
            if (!string.IsNullOrEmpty(output)) AppendConsole(output);
            RaiseStatus();

            CheckCompletion();
        }

        // ─── Completion ───

        private void CheckCompletion()
        {
            if (ActiveContract == null) return;
            if (!ActiveContract.ConsumeCompletionAnnouncement()) return;

            State.MarkCompleted(ActiveDef.Id, ActiveDef.UnlockIndex);

            // Finished systems keep their commands callable but inert.
            if (ActiveContract.Kind != ContractKind.Terminal)
                State.RetireCommands(new List<string>(ActiveContract.GetCommands().Keys));

            RaiseStatus();
            GoTo(GameScreen.Debrief);
        }

        public bool HasNextContract()
        {
            if (ActiveDef == null) return false;
            int i = ContractRegistry.IndexOf(ActiveDef.Id);
            return i >= 0 && i + 1 < ContractRegistry.All.Count;
        }

        public void ContinueAfterDebrief()
        {
            GoTo(GameScreen.Board);
        }
    }
}
