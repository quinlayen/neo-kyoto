using System.Collections;
using System.Collections.Generic;
using NeoKyoto.Contracts;
using NeoKyoto.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NeoKyoto.UI
{
    /// <summary>
    /// Builds and drives every screen. The world stays visible on the left; the
    /// editor and console are docked to the right.
    /// </summary>
    public class UIController : MonoBehaviour
    {
        [Tooltip("Right-hand share of the screen taken by the work panel.")]
        public float panelWidth = 0.42f;

        private GameManager _gm;

        private GameObject _titlePanel, _boardPanel, _briefingPanel, _workspacePanel, _debriefPanel;
        private Transform _boardList;

        private TextMeshProUGUI _wsHeader, _wsStatus, _consoleText, _briefingText, _debriefText, _hintText;
        private TMP_InputField _codeInput, _terminalInput;
        private ScrollRect _consoleScroll, _briefingScroll, _debriefScroll;
        private GameObject _editorSection, _terminalRow, _runRow;
        private Button _runButton, _debriefButton;
        private TextMeshProUGUI _runButtonLabel;
        private LayoutElement _statusLayout;

        private string[] _briefingPages;
        private int _briefingPage;
        private Button _briefingPrev, _briefingNext;
        private TextMeshProUGUI _briefingNextLabel, _briefingCounter;

        private string[] _debriefPages;
        private int _debriefPage;
        private Button _debriefPrev, _debriefNext;
        private TextMeshProUGUI _debriefNextLabel, _debriefCounter;

        private const float StatusMinHeight = 96f;

        private void Awake()
        {
            _gm = GameManager.Instance;
            BuildCanvas();
        }

        private void Start()
        {
            _gm.ScreenChanged += RefreshScreen;
            _gm.ConsoleChanged += RefreshConsole;
            _gm.StatusChanged += RefreshStatus;
            RefreshScreen();
        }

        private void OnDestroy()
        {
            if (_gm == null) return;
            _gm.ScreenChanged -= RefreshScreen;
            _gm.ConsoleChanged -= RefreshConsole;
            _gm.StatusChanged -= RefreshStatus;
        }

        // ─── Construction ───

        private void BuildCanvas()
        {
            var canvasGo = new GameObject("Canvas", typeof(RectTransform));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            BuildTitle(canvasGo.transform);
            BuildBoard(canvasGo.transform);
            BuildBriefing(canvasGo.transform);
            BuildWorkspace(canvasGo.transform);
            BuildDebrief(canvasGo.transform);
        }

        private void BuildTitle(Transform parent)
        {
            _titlePanel = UITheme.Box("TitlePanel", parent, UITheme.PanelSolid).gameObject;
            UITheme.Stretch(_titlePanel.GetComponent<RectTransform>());

            var title = UITheme.Label("Title", _titlePanel.transform,
                "NEO-KYOTO\nSYSTEMS CONTRACTOR", 64f, UITheme.Accent, TextAlignmentOptions.Center);
            var rt = title.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(1200, 220);
            rt.anchoredPosition = new Vector2(0, 170);

            var tag = UITheme.Label("Tagline", _titlePanel.transform,
                "\"The city doesn't sleep. Neither do its systems.\n" +
                " When they break, you get the call.\"", 20f, UITheme.TextDim,
                TextAlignmentOptions.Center);
            var trt = tag.rectTransform;
            trt.anchorMin = new Vector2(0.5f, 0.5f);
            trt.anchorMax = new Vector2(0.5f, 0.5f);
            trt.sizeDelta = new Vector2(1200, 120);
            trt.anchoredPosition = new Vector2(0, 20);

            var btn = UITheme.Button("Connect", _titlePanel.transform,
                "CONNECT TO CONTRACTOR TERMINAL", UITheme.Accent, () => _gm.StartGame());
            var brt = btn.GetComponent<RectTransform>();
            brt.anchorMin = new Vector2(0.5f, 0.5f);
            brt.anchorMax = new Vector2(0.5f, 0.5f);
            brt.sizeDelta = new Vector2(520, 54);
            brt.anchoredPosition = new Vector2(0, -140);
        }

        private void BuildBoard(Transform parent)
        {
            _boardPanel = UITheme.Box("BoardPanel", parent, UITheme.Backdrop).gameObject;
            UITheme.Stretch(_boardPanel.GetComponent<RectTransform>());

            var header = UITheme.Label("Header", _boardPanel.transform,
                "NEO-KYOTO — CONTRACTOR TERMINAL", 30f, UITheme.Accent, TextAlignmentOptions.Center);
            var hrt = header.rectTransform;
            hrt.anchorMin = new Vector2(0.5f, 1f);
            hrt.anchorMax = new Vector2(0.5f, 1f);
            hrt.pivot = new Vector2(0.5f, 1f);
            hrt.sizeDelta = new Vector2(1200, 50);
            hrt.anchoredPosition = new Vector2(0, -70);

            var sub = UITheme.Label("Sub", _boardPanel.transform,
                "AVAILABLE CONTRACTS", 16f, UITheme.TextDim, TextAlignmentOptions.Center);
            var srt = sub.rectTransform;
            srt.anchorMin = new Vector2(0.5f, 1f);
            srt.anchorMax = new Vector2(0.5f, 1f);
            srt.pivot = new Vector2(0.5f, 1f);
            srt.sizeDelta = new Vector2(1200, 30);
            srt.anchoredPosition = new Vector2(0, -124);

            var listGo = UITheme.Node("List", _boardPanel.transform);
            var lrt = listGo.GetComponent<RectTransform>();
            lrt.anchorMin = new Vector2(0.5f, 1f);
            lrt.anchorMax = new Vector2(0.5f, 1f);
            lrt.pivot = new Vector2(0.5f, 1f);
            lrt.sizeDelta = new Vector2(900, 520);
            lrt.anchoredPosition = new Vector2(0, -180);

            var layout = listGo.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 10f;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            _boardList = listGo.transform;
        }

        private void BuildBriefing(Transform parent)
        {
            _briefingPanel = MakeRightPanel("BriefingPanel", parent, out var content);

            var header = UITheme.Label("Header", content, "INCOMING TRANSMISSION", 18f, UITheme.Accent);
            AddLayout(header.gameObject, 28f, 0f);

            _briefingText = UITheme.ScrollText("BriefingScroll", content, out _briefingScroll);
            AddLayout(_briefingScroll.gameObject, 0f, 1f);

            // Paged rather than one long scroll.
            var pageRow = MakeRow(content, 40f);
            _briefingPrev = UITheme.Button("Prev", pageRow, "◂ BACK", UITheme.TextDim,
                () => ShowBriefingPage(_briefingPage - 1));
            _briefingCounter = UITheme.Label("Count", pageRow, "", UITheme.SmallSize,
                UITheme.TextDim, TextAlignmentOptions.Center);
            _briefingNext = UITheme.Button("Next", pageRow, "NEXT ▸", UITheme.Accent, BriefingAdvance);
            _briefingNextLabel = _briefingNext.GetComponentInChildren<TextMeshProUGUI>();

            var row = MakeRow(content, 40f);
            UITheme.Button("Back", row, "CONTRACT BOARD", UITheme.TextDim, () => _gm.BackToBoard());
        }

        /// <summary>Next page, or jack in once the briefing has been read through.</summary>
        private void BriefingAdvance()
        {
            if (_briefingPage < _briefingPages.Length - 1) ShowBriefingPage(_briefingPage + 1);
            else _gm.BeginWork();
        }

        private void ShowBriefingPage(int index)
        {
            if (_briefingPages == null || _briefingPages.Length == 0) return;
            _briefingPage = Mathf.Clamp(index, 0, _briefingPages.Length - 1);
            _briefingText.text = _briefingPages[_briefingPage];

            bool last = _briefingPage == _briefingPages.Length - 1;
            _briefingNextLabel.text = last ? "JACK IN ▸" : "NEXT ▸";
            _briefingNextLabel.color = last ? UITheme.Good : UITheme.Accent;
            _briefingPrev.interactable = _briefingPage > 0;
            _briefingCounter.text = (_briefingPage + 1) + " / " + _briefingPages.Length;

            StartCoroutine(ScrollToTop(_briefingScroll));
        }

        private void BuildWorkspace(Transform parent)
        {
            _workspacePanel = MakeRightPanel("WorkspacePanel", parent, out var content);

            _wsHeader = UITheme.Label("Header", content, "", 18f, UITheme.Accent);
            AddLayout(_wsHeader.gameObject, 28f, 0f);

            var statusFrame = UITheme.Framed("StatusFrame", content, UITheme.Border);
            _statusLayout = AddLayout(statusFrame.parent.gameObject, StatusMinHeight, 0f);
            _wsStatus = UITheme.Label("Status", statusFrame, "", UITheme.BodySize, UITheme.Text);
            UITheme.Stretch(_wsStatus.rectTransform, 10, 6, 10, 6);

            // ── Script editor (Python contracts) ──
            _editorSection = UITheme.Node("EditorSection", content);
            AddLayout(_editorSection, 0f, 1.25f);
            var editorLayout = _editorSection.AddComponent<VerticalLayoutGroup>();
            editorLayout.spacing = 6f;
            editorLayout.childControlHeight = true;
            editorLayout.childControlWidth = true;
            editorLayout.childForceExpandHeight = false;
            editorLayout.childForceExpandWidth = true;

            var editorLabel = UITheme.Label("EditorLabel", _editorSection.transform,
                "SCRIPT", 14f, UITheme.TextDim);
            AddLayout(editorLabel.gameObject, 20f, 0f);

            _codeInput = MakeInputField("CodeInput", _editorSection.transform, true);
            AddLayout(_codeInput.gameObject, 0f, 1f);
            _codeInput.onValueChanged.AddListener(code => _gm.SetScript(code));
            _codeInput.gameObject.AddComponent<CodeEditorBehaviour>();

            _runRow = MakeRow(content, 42f).gameObject;
            // One button: starts a run, and stops it while one is in progress.
            _runButton = UITheme.Button("Run", _runRow.transform, "▶ RUN", UITheme.Good, ToggleRun);
            _runButtonLabel = _runButton.GetComponentInChildren<TextMeshProUGUI>();
            UITheme.Button("Reset", _runRow.transform, "RESET SCRIPT", UITheme.Warn, ResetScript);
            UITheme.Button("Brief", _runRow.transform, "BRIEFING", UITheme.TextDim,
                () => _gm.GoTo(GameScreen.Briefing));

            // ── Console (both kinds of contract) ──
            var consoleLabel = UITheme.Label("ConsoleLabel", content, "OUTPUT", 14f, UITheme.TextDim);
            AddLayout(consoleLabel.gameObject, 20f, 0f);

            var consoleFrame = UITheme.Framed("ConsoleFrame", content, UITheme.Border);
            AddLayout(consoleFrame.parent.gameObject, 0f, 1f);
            _consoleText = UITheme.ScrollText("ConsoleScroll", consoleFrame, out _consoleScroll);
            UITheme.Stretch(_consoleScroll.GetComponent<RectTransform>(), 4, 4, 4, 4);
            _consoleText.fontSize = UITheme.SmallSize;

            // ── Terminal input (terminal contracts) ──
            _terminalRow = UITheme.Node("TerminalRow", content);
            AddLayout(_terminalRow, 40f, 0f);
            var termLayout = _terminalRow.AddComponent<HorizontalLayoutGroup>();
            termLayout.spacing = 6f;
            termLayout.childControlHeight = true;
            termLayout.childControlWidth = true;
            termLayout.childForceExpandWidth = true;

            _terminalInput = MakeInputField("TerminalInput", _terminalRow.transform, false);
            _terminalInput.onSubmit.AddListener(OnTerminalSubmit);

            var sendBtn = UITheme.Button("Send", _terminalRow.transform, "ENTER", UITheme.Accent,
                () => OnTerminalSubmit(_terminalInput.text));
            var sendLayout = sendBtn.gameObject.AddComponent<LayoutElement>();
            sendLayout.preferredWidth = 110f;
            sendLayout.flexibleWidth = 0f;

            _hintText = UITheme.Label("Hint", content, "", 13f, UITheme.TextDim);
            AddLayout(_hintText.gameObject, 20f, 0f);

            var bottomRow = MakeRow(content, 40f);
            _debriefButton = UITheme.Button("Debrief", bottomRow, "DEBRIEF", UITheme.Accent,
                () => _gm.GoTo(GameScreen.Debrief));
            UITheme.Button("Board", bottomRow, "CONTRACT BOARD", UITheme.TextDim, () => _gm.BackToBoard());
        }

        private void BuildDebrief(Transform parent)
        {
            _debriefPanel = UITheme.Box("DebriefPanel", parent, UITheme.Backdrop).gameObject;
            UITheme.Stretch(_debriefPanel.GetComponent<RectTransform>());

            var frameBorder = UITheme.Box("Frame", _debriefPanel.transform, UITheme.Border);
            var frt = frameBorder.rectTransform;
            frt.anchorMin = new Vector2(0.5f, 0.5f);
            frt.anchorMax = new Vector2(0.5f, 0.5f);
            // Sized to the longest page rather than the screen, so short pages
            // do not leave a large empty panel.
            frt.sizeDelta = new Vector2(1120, 720);

            var inner = UITheme.Box("Inner", frameBorder.transform, UITheme.PanelSolid);
            UITheme.Stretch(inner.rectTransform, 2, 2, 2, 2);

            var col = UITheme.Node("Col", inner.transform);
            UITheme.Stretch(col.GetComponent<RectTransform>(), 18, 18, 18, 18);
            var layout = col.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 10f;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            _debriefText = UITheme.ScrollText("DebriefScroll", col.transform, out _debriefScroll);
            AddLayout(_debriefScroll.gameObject, 0f, 1f);

            var row = MakeRow(col.transform, 46f);
            _debriefPrev = UITheme.Button("Prev", row, "◂ BACK", UITheme.TextDim,
                () => ShowDebriefPage(_debriefPage - 1));
            _debriefCounter = UITheme.Label("Count", row, "", UITheme.SmallSize,
                UITheme.TextDim, TextAlignmentOptions.Center);
            _debriefNext = UITheme.Button("Next", row, "NEXT ▸", UITheme.Accent, DebriefAdvance);
            _debriefNextLabel = _debriefNext.GetComponentInChildren<TextMeshProUGUI>();

            var row2 = MakeRow(col.transform, 40f);
            UITheme.Button("Board", row2, "CONTRACT BOARD", UITheme.TextDim, () => _gm.BackToBoard());
        }

        private void DebriefAdvance()
        {
            if (_debriefPage < _debriefPages.Length - 1) ShowDebriefPage(_debriefPage + 1);
            else _gm.ContinueAfterDebrief();
        }

        private void ShowDebriefPage(int index)
        {
            if (_debriefPages == null || _debriefPages.Length == 0) return;
            _debriefPage = Mathf.Clamp(index, 0, _debriefPages.Length - 1);
            _debriefText.text = _debriefPages[_debriefPage];

            bool last = _debriefPage == _debriefPages.Length - 1;
            // On the last page the button becomes the action the debrief was leading to.
            _debriefNextLabel.text = last
                ? (_gm.DebriefInvitesRetry ? "TRY IT ▸" : "CONTRACT BOARD ▸")
                : "NEXT ▸";
            _debriefNextLabel.color = last ? UITheme.Good : UITheme.Accent;
            _debriefPrev.interactable = _debriefPage > 0;
            _debriefCounter.text = (_debriefPage + 1) + " / " + _debriefPages.Length;

            StartCoroutine(ScrollToTop(_debriefScroll));
        }

        // ─── Small builders ───

        private GameObject MakeRightPanel(string name, Transform parent, out Transform content)
        {
            var panel = UITheme.Box(name, parent, UITheme.Panel).gameObject;
            var rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f - panelWidth, 0f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var edge = UITheme.Box("Edge", panel.transform, UITheme.Accent);
            var ert = edge.rectTransform;
            ert.anchorMin = new Vector2(0f, 0f);
            ert.anchorMax = new Vector2(0f, 1f);
            ert.pivot = new Vector2(0f, 0.5f);
            ert.sizeDelta = new Vector2(2f, 0f);
            edge.color = new Color(UITheme.Accent.r, UITheme.Accent.g, UITheme.Accent.b, 0.5f);

            var col = UITheme.Node("Content", panel.transform);
            UITheme.Stretch(col.GetComponent<RectTransform>(), 20, 18, 18, 18);
            var layout = col.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            content = col.transform;
            return panel;
        }

        private static Transform MakeRow(Transform parent, float height)
        {
            var row = UITheme.Node("Row", parent);
            AddLayout(row, height, 0f);
            var layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8f;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = true;
            layout.childForceExpandWidth = true;
            return row.transform;
        }

        private static LayoutElement AddLayout(GameObject go, float preferredHeight, float flexibleHeight)
        {
            var le = go.AddComponent<LayoutElement>();
            if (preferredHeight > 0f) le.preferredHeight = preferredHeight;
            le.flexibleHeight = flexibleHeight;
            le.minHeight = preferredHeight > 0f ? preferredHeight : 40f;
            return le;
        }

        private static TMP_InputField MakeInputField(string name, Transform parent, bool multiline)
        {
            var border = UITheme.Box(name, parent, UITheme.Border);
            var bg = UITheme.Box("Bg", border.transform, UITheme.Field);
            UITheme.Stretch(bg.rectTransform, 1, 1, 1, 1);

            var input = border.gameObject.AddComponent<TMP_InputField>();

            var textArea = UITheme.Node("TextArea", bg.transform);
            var tart = textArea.GetComponent<RectTransform>();
            UITheme.Stretch(tart, 8, 6, 8, 6);
            textArea.AddComponent<RectMask2D>();

            var text = UITheme.Label("Text", textArea.transform, "", UITheme.BodySize, UITheme.Text);
            UITheme.Stretch(text.rectTransform);
            text.enableWordWrapping = false;
            text.overflowMode = TextOverflowModes.Overflow;

            var placeholder = UITheme.Label("Placeholder", textArea.transform,
                multiline ? "" : "type a command…", UITheme.BodySize, UITheme.TextDim);
            UITheme.Stretch(placeholder.rectTransform);

            input.textViewport = tart;
            input.textComponent = text;
            input.placeholder = placeholder;
            input.fontAsset = UITheme.Mono;
            input.pointSize = UITheme.BodySize;
            input.caretColor = UITheme.Accent;
            input.customCaretColor = true;
            input.selectionColor = new Color(UITheme.Accent.r, UITheme.Accent.g, UITheme.Accent.b, 0.3f);
            input.lineType = multiline ? TMP_InputField.LineType.MultiLineNewline : TMP_InputField.LineType.SingleLine;
            input.richText = false;
            input.restoreOriginalTextOnEscape = false;

            if (multiline)
            {
                text.alignment = TextAlignmentOptions.TopLeft;
                placeholder.alignment = TextAlignmentOptions.TopLeft;
            }
            else
            {
                text.alignment = TextAlignmentOptions.Left;
                placeholder.alignment = TextAlignmentOptions.Left;
            }

            return input;
        }

        // ─── Refresh ───

        private void RefreshScreen()
        {
            var s = _gm.CurrentScreen;
            _titlePanel.SetActive(s == GameScreen.Title);
            _boardPanel.SetActive(s == GameScreen.Board);
            _briefingPanel.SetActive(s == GameScreen.Briefing);
            _workspacePanel.SetActive(s == GameScreen.Workspace);
            _debriefPanel.SetActive(s == GameScreen.Debrief);

            if (s == GameScreen.Board) RebuildBoard();

            if (s == GameScreen.Briefing && _gm.ActiveContract != null)
            {
                _briefingPages = Contract.Paginate(_gm.ActiveContract.GetBriefing());
                ShowBriefingPage(0);
            }

            if (s == GameScreen.Debrief && _gm.ActiveContract != null)
            {
                string text = _gm.CurrentDebriefText;
                if (string.IsNullOrEmpty(text)) text = _gm.ActiveContract.GetCompletionMessage();
                _debriefPages = Contract.Paginate(text);
                ShowDebriefPage(0);
            }

            if (s == GameScreen.Workspace && _gm.ActiveContract != null) SetupWorkspace();
        }

        private void SetupWorkspace()
        {
            var contract = _gm.ActiveContract;
            bool isTerminal = contract.Kind == ContractKind.Terminal;

            _wsHeader.text = contract.Title.ToUpperInvariant() + " — " + contract.Location.ToUpperInvariant();

            _editorSection.SetActive(!isTerminal);
            _runRow.SetActive(!isTerminal);
            _terminalRow.SetActive(isTerminal);

            if (!isTerminal)
            {
                _codeInput.SetTextWithoutNotify(_gm.GetScript());
                _hintText.text = "Tab indents · commands: " + string.Join(", ", CommandNames(contract));
            }
            else
            {
                _hintText.text = "Type commands directly · 'reset' restores the filesystem";
                StartCoroutine(FocusTerminal());
            }

            RefreshStatus();
            RefreshConsole();
        }

        private static string[] CommandNames(Contract contract)
        {
            var keys = new List<string>(contract.GetCommands().Keys);
            keys.Sort();
            return keys.ToArray();
        }

        private void RefreshStatus()
        {
            if (_gm.ActiveContract == null) return;

            string text = _gm.ActiveContract.GetStatusText();
            if (_gm.ActiveContract.Completed)
                text += "\n" + _gm.ActiveContract.GetCompletedBanner();
            _wsStatus.text = text;

            // The completion banner adds lines, so grow the box instead of overlapping.
            if (_statusLayout != null)
            {
                float needed = _wsStatus.GetPreferredValues(text, _wsStatus.rectTransform.rect.width, 0f).y + 18f;
                _statusLayout.preferredHeight = Mathf.Max(StatusMinHeight, needed);
                _statusLayout.minHeight = _statusLayout.preferredHeight;
            }

            bool done = _gm.ActiveContract.Completed;
            _wsStatus.color = done ? UITheme.Good : UITheme.Text;
            if (_debriefButton != null) _debriefButton.gameObject.SetActive(done);

            if (_runButtonLabel != null)
            {
                _runButtonLabel.text = _gm.IsRunning ? "■ STOP" : "▶ RUN";
                _runButtonLabel.color = _gm.IsRunning ? UITheme.Fault : UITheme.Good;
            }
        }

        private void RefreshConsole()
        {
            if (_consoleText == null) return;
            var lines = _gm.ConsoleLines;
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < lines.Count; i++) sb.AppendLine(lines[i]);
            _consoleText.text = sb.ToString();
            StartCoroutine(ScrollToBottom(_consoleScroll));
        }

        private IEnumerator ScrollToBottom(ScrollRect scroll)
        {
            yield return null;
            Canvas.ForceUpdateCanvases();
            if (scroll != null) scroll.verticalNormalizedPosition = 0f;
        }

        private IEnumerator ScrollToTop(ScrollRect scroll)
        {
            yield return null;
            Canvas.ForceUpdateCanvases();
            if (scroll != null) scroll.verticalNormalizedPosition = 1f;
        }

        private IEnumerator FocusTerminal()
        {
            yield return null;
            if (_terminalInput != null)
            {
                _terminalInput.ActivateInputField();
                _terminalInput.Select();
            }
        }

        private void OnTerminalSubmit(string value)
        {
            if (_gm.ActiveContract == null) return;
            _gm.SubmitTerminalCommand(value);
            _terminalInput.SetTextWithoutNotify("");
            StartCoroutine(FocusTerminal());
        }

        private void ToggleRun()
        {
            if (_gm.IsRunning) _gm.StopScript();
            else _gm.RunScript();
        }

        private void ResetScript()
        {
            if (_gm.ActiveContract == null) return;
            // Clearing the script mid-run would leave the run going against code
            // that no longer exists, so stop it first.
            _gm.StopScript();
            _gm.SetScript(_gm.ActiveContract.StarterScript);
            _codeInput.SetTextWithoutNotify(_gm.ActiveContract.StarterScript);
        }

        private void RebuildBoard()
        {
            for (int i = _boardList.childCount - 1; i >= 0; i--) Destroy(_boardList.GetChild(i).gameObject);

            for (int i = 0; i < ContractRegistry.All.Count; i++)
            {
                var def = ContractRegistry.All[i];
                bool completed = _gm.State.IsContractCompleted(def.Id);
                bool available = _gm.IsAvailable(i);

                string status = completed ? "[DONE] ◆" : (available ? "[AVAILABLE]" : "[LOCKED]");
                Color color = completed ? UITheme.Good : (available ? UITheme.Accent : UITheme.TextDim);

                string label = "[" + (i + 1) + "]  " +
                               (def.Title + " — " + def.Location).PadRight(42) + status;

                var captured = def;
                var btn = UITheme.Button("Contract" + i, _boardList, label, color,
                    available ? (UnityEngine.Events.UnityAction)(() => _gm.OpenContract(captured)) : null);
                btn.interactable = available;

                var le = btn.gameObject.AddComponent<LayoutElement>();
                le.preferredHeight = 52f;

                var text = btn.GetComponentInChildren<TextMeshProUGUI>();
                text.alignment = TextAlignmentOptions.Left;
                text.margin = new Vector4(16, 0, 16, 0);
            }
        }
    }
}
