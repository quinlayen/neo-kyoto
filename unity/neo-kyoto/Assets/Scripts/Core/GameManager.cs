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

        [Tooltip("Wipe the save before loading. Saved scripts otherwise persist across " +
                 "editor sessions, which is right for players and tedious while iterating.")]
        public bool resetSaveOnPlay;

        [Tooltip("Jump straight into this contract on play, 1-based. 0 for the normal " +
                 "title flow. Implies unlocking, so the contract is reachable.")]
        public int startAtContract;

        [Tooltip("Seconds between observable script events, so code runs visibly in the world.")]
        public float stepDelay = 0.12f;

        public GameState State { get; private set; }
        public GameScreen CurrentScreen { get; private set; }
        public Contract ActiveContract { get; private set; }
        public ContractDef ActiveDef { get; private set; }
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Source line of the step the script is on, or 0 when nothing is running.
        /// Only calls and prints emit events, so this tracks the lines that actually
        /// do something rather than every line the interpreter touches.
        /// </summary>
        public int CurrentLine { get; private set; }

        private readonly List<string> _console = new List<string>();
        private readonly Dictionary<string, string> _scripts = new Dictionary<string, string>();
        private readonly HashSet<string> _debriefed = new HashSet<string>();
        private readonly HashSet<string> _followUpDebriefed = new HashSet<string>();
        private ScriptRunner _runner;
        private Coroutine _runRoutine;
        private bool _goalWasMet;
        private int _callsToGoal;

        public IList<string> ConsoleLines { get { return _console; } }

        public event Action ScreenChanged;
        public event Action ConsoleChanged;
        public event Action StatusChanged;

        private const int MaxConsoleLines = 400;

        private void Awake()
        {
            Instance = this;
            State = new GameState();
            if (resetSaveOnPlay) SaveSystem.Clear();
            LoadProgress();
            if (unlockAllForTesting || startAtContract > 0)
                State.UnlockAll(ContractRegistry.AllIds());
            CurrentScreen = GameScreen.Title;
        }

        private void Start()
        {
            if (startAtContract > 0) StartCoroutine(JumpToContract(startAtContract));
        }

        /// <summary>
        /// Development shortcut past the title and board. Waits a frame so every
        /// listener has finished subscribing in its own Start before events fire.
        /// </summary>
        private IEnumerator JumpToContract(int oneBased)
        {
            yield return null;

            int index = Mathf.Clamp(oneBased - 1, 0, ContractRegistry.All.Count - 1);
            OpenContract(ContractRegistry.All[index]);
            BeginWork();
        }

        // ─── Persistence ───

        private bool _saveDirty;
        private float _saveTimer;
        private const float SaveDebounceSeconds = 2f;

        public bool HasSavedProgress { get { return SaveSystem.HasSave; } }

        private void LoadProgress()
        {
            var data = SaveSystem.Load();
            if (data == null) return;

            var features = new List<Feature>();
            foreach (var name in data.unlockedFeatures)
            {
                try { features.Add((Feature)Enum.Parse(typeof(Feature), name)); }
                catch { /* a feature that no longer exists is simply dropped */ }
            }

            State.Restore(data.completedContracts, features, data.retiredCommands);

            foreach (var id in data.debriefed) _debriefed.Add(id);
            foreach (var id in data.followUpDebriefed) _followUpDebriefed.Add(id);
            foreach (var entry in data.scripts)
            {
                if (!string.IsNullOrEmpty(entry.contractId)) _scripts[entry.contractId] = entry.code;
            }

            var scores = new Dictionary<string, ContractScore>();
            foreach (var e in data.scores)
            {
                if (string.IsNullOrEmpty(e.contractId)) continue;
                scores[e.contractId] = new ContractScore {
                    Stars = e.stars, CallsToGoal = e.callsToGoal, BonusFound = e.bonusFound };
            }
            State.RestoreScores(scores, data.credits);
        }

        private SaveData BuildSaveData()
        {
            var data = new SaveData();
            foreach (var id in State.CompletedContracts) data.completedContracts.Add(id);
            foreach (var f in State.UnlockedFeatures) data.unlockedFeatures.Add(f.ToString());
            foreach (var c in State.RetiredCommands) data.retiredCommands.Add(c);
            foreach (var id in _debriefed) data.debriefed.Add(id);
            foreach (var id in _followUpDebriefed) data.followUpDebriefed.Add(id);
            foreach (var kv in _scripts)
                data.scripts.Add(new ScriptEntry { contractId = kv.Key, code = kv.Value });
            foreach (var kv in State.Scores)
                data.scores.Add(new ScoreEntry {
                    contractId = kv.Key, stars = kv.Value.Stars,
                    callsToGoal = kv.Value.CallsToGoal, bonusFound = kv.Value.BonusFound });
            data.credits = State.Credits;
            return data;
        }

        /// <summary>Queues a save. Typing should not hit storage on every keystroke.</summary>
        private void MarkDirty()
        {
            _saveDirty = true;
            _saveTimer = SaveDebounceSeconds;
        }

        /// <summary>Writes immediately — used at moments the player could vanish.</summary>
        public void FlushSave()
        {
            if (!_saveDirty) return;
            _saveDirty = false;
            SaveSystem.Save(BuildSaveData());
        }

        private void Update()
        {
            if (!_saveDirty) return;
            _saveTimer -= Time.unscaledDeltaTime;
            if (_saveTimer <= 0f) FlushSave();
        }

        private void OnApplicationPause(bool paused) { if (paused) FlushSave(); }
        private void OnApplicationFocus(bool focused) { if (!focused) FlushSave(); }
        private void OnApplicationQuit() { FlushSave(); }

        public void ResetProgress()
        {
            SaveSystem.Clear();
            State.Reset();
            _scripts.Clear();
            _debriefed.Clear();
            _followUpDebriefed.Clear();
            _saveDirty = false;
            ActiveContract = null;
            ActiveDef = null;
            GoTo(GameScreen.Title);
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
            _goalWasMet = false;

            var terminal = ActiveContract as TerminalContract;
            if (terminal != null) AppendConsole(terminal.GetPrompt());

            GoTo(GameScreen.Briefing);
        }

        public void BeginWork() { GoTo(GameScreen.Workspace); }

        public void BackToBoard()
        {
            FlushSave();
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
            MarkDirty();
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
            _runRoutine = StartCoroutine(RunScriptRoutine());
        }

        private IEnumerator RunScriptRoutine()
        {
            IsRunning = true;
            CurrentLine = 0;
            ClearConsole();

            // Each run starts from a clean system, matching the prototype.
            ActiveContract.ResetSystem();
            _goalWasMet = false;
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

            // Calls are counted only up to the moment the goal is reached. A
            // `while True` loop cannot stop itself and always burns the call cap,
            // so scoring the whole run would rate the loop worse than writing the
            // command out by hand — the opposite of what the contract teaches.
            _callsToGoal = 0;

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
                    if (GameAudio.Instance != null) GameAudio.Instance.Play(Sfx.Error);
                    break;
                }
                if (!moved) break;

                var ev = it.Current;
                if (ev.Line > 0) CurrentLine = ev.Line;
                if (ev.Kind == ExecEventKind.Print) AppendConsole(PyValue.Str(ev.Text));

                // A run was previously text only. One tick per command gives it a second
                // feedback channel and makes the pacing of a loop audible.
                if (ev.Kind == ExecEventKind.Call && GameAudio.Instance != null)
                    GameAudio.Instance.Play(Sfx.Tick, 0.45f, UnityEngine.Random.Range(0.96f, 1.05f));
                if (_callsToGoal == 0 && ActiveContract.IsGoalMet()) _callsToGoal = _runner.CallCount;
                RaiseStatus();

                if (stepDelay > 0f) yield return new WaitForSeconds(stepDelay);
                else yield return null;
            }

            // Clear the running flag before the final refresh, otherwise the UI's
            // last status update still shows the run in progress.
            IsRunning = false;
            CurrentLine = 0;
            _runRoutine = null;

            AppendConsole(endMessage);
            AppendConsole("└──────────────────────────────────────────");
            RaiseStatus();

            CheckCompletion();
        }

        /// <summary>Halts a run in progress. The system keeps whatever state it reached.</summary>
        public void StopScript()
        {
            if (!IsRunning) return;

            if (_runRoutine != null) StopCoroutine(_runRoutine);
            _runRoutine = null;
            IsRunning = false;
            CurrentLine = 0;

            AppendConsole("Stopped.");
            AppendConsole("└──────────────────────────────────────────");
            RaiseStatus();
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
                _goalWasMet = false;
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

            // React to the moment the goal is reached, not to it merely being true —
            // otherwise every later terminal command would re-report success.
            bool met = ActiveContract.IsGoalMet();
            if (!met) { _goalWasMet = false; return; }
            if (_goalWasMet) return;
            _goalWasMet = true;

            ActiveContract.ConsumeCompletionAnnouncement();
            State.MarkCompleted(ActiveDef.Id, ActiveDef.UnlockIndex);
            AwardScore();
            MarkDirty();

            // Finished systems keep their commands callable but inert.
            if (ActiveContract.Kind != ContractKind.Terminal)
                State.RetireCommands(new List<string>(ActiveContract.GetCommands().Keys));

            RaiseStatus();

            if (!_debriefed.Contains(ActiveDef.Id))
            {
                _debriefed.Add(ActiveDef.Id);
                MarkDirty();
                CurrentDebriefText = ActiveContract.GetCompletionMessage();
                ShowingFollowUpDebrief = false;
                GoTo(GameScreen.Debrief);
                return;
            }

            // Solving it again with the tool the debrief just handed over earns a
            // proper follow-up — but only once, and only if they actually used it.
            string followUp = ActiveContract.GetLoopCompletionMessage();
            if (followUp != null && !_followUpDebriefed.Contains(ActiveDef.Id) &&
                _runner != null && _runner.LoopDidTheWork(_callsToGoal))
            {
                _followUpDebriefed.Add(ActiveDef.Id);
                MarkDirty();
                CurrentDebriefText = followUp;
                ShowingFollowUpDebrief = true;
                GoTo(GameScreen.Debrief);
                return;
            }

            AppendConsole("");
            AppendConsole("◆ " + ActiveContract.GetSolvedAgainMessage());
        }

        /// <summary>Summary of the run just scored, shown above the debrief.</summary>
        public int LastStars { get; private set; }
        public int LastCreditsEarned { get; private set; }
        public int LastCallsToGoal { get; private set; }
        public bool LastBonusFound { get; private set; }

        private void AwardScore()
        {
            var c = ActiveContract;

            if (c.Kind == ContractKind.Terminal)
            {
                // Exploration is the skill here, so finishing is worth two stars and
                // turning up the hidden extra is worth the third.
                LastStars = c.HasBonus && c.BonusFound ? 3 : 2;
                LastCallsToGoal = 0;
            }
            else
            {
                LastCallsToGoal = _callsToGoal;
                LastStars = Scoring.RateContract(_callsToGoal, c.ThreeStarCalls, c.TwoStarCalls);

                // Call count alone cannot tell a loop from the same command typed out
                // twelve times — both make twelve calls. The lesson is the loop, so the
                // third star asks for it. This is what makes the design doc's
                // "1-2★ with basic tools, 3★ on replay with better tools" actually happen.
                //
                // It asks whether the loop did the work, not whether one is present:
                // a decorative `while False:` above repeated calls contains a loop but
                // runs nothing inside it.
                if (LastStars == 3 && !_runner.LoopDidTheWork(_callsToGoal)) LastStars = 2;
            }

            LastBonusFound = c.HasBonus && c.BonusFound;
            LastCreditsEarned = State.RecordScore(
                ActiveDef.Id, LastStars, LastCallsToGoal, LastBonusFound, c.BaseCredits);
        }

        /// <summary>Text the debrief screen should show — first pass or follow-up.</summary>
        public string CurrentDebriefText { get; private set; }

        public bool ShowingFollowUpDebrief { get; private set; }

        /// <summary>
        /// True when this contract's debrief handed the player a new tool, so the
        /// natural next step is going back to the same job to try it.
        /// </summary>
        public bool DebriefInvitesRetry
        {
            get
            {
                return ActiveContract != null
                       && ActiveContract.DebriefInvitesRetry
                       && !ShowingFollowUpDebrief;
            }
        }

        public bool HasNextContract()
        {
            if (ActiveDef == null) return false;
            int i = ContractRegistry.IndexOf(ActiveDef.Id);
            return i >= 0 && i + 1 < ContractRegistry.All.Count;
        }

        public void ContinueAfterDebrief()
        {
            // A debrief that unlocked something sends you back to the job to use it;
            // otherwise the board is the natural next stop.
            GoTo(DebriefInvitesRetry ? GameScreen.Workspace : GameScreen.Board);
        }
    }
}
