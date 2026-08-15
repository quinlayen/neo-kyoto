using System.Collections;
using System.Collections.Generic;
using NeoKyoto.Contracts;
using NeoKyoto.Core;
using NeoKyoto.UI.Deck;
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

        /// <summary>Splash choreography. Set by Bootstrap before this object activates.</summary>
        public SplashTiming splashTiming = new SplashTiming();

        /// <summary>Deck frame geometry. Set by Bootstrap before this object activates.</summary>
        public DeckLayoutSettings deckLayout = new DeckLayoutSettings();

        // The workspace screen is the deck: a shell plus its windows, over the live world.
        private DeckShell _deck;
        private DeckWindow _editorWindow, _terminalWindow, _readoutWindow, _briefingWindow;
        private TextMeshProUGUI _briefingWindowText;
        private ScrollRect _briefingWindowScroll;

        private GameManager _gm;

        private GameObject _titlePanel, _boardPanel, _briefingPanel, _workspacePanel, _debriefPanel;
        private Transform _boardList;

        // Splash backdrop, swapped for the live city when the kit scene is available.
        private Image _titleBackground, _splashPanorama;
        private SplashSequence _splashSequence;

        private TextMeshProUGUI _debriefHeader;
        private TextMeshProUGUI _wsStatus, _consoleText, _briefingText, _debriefText, _hintText;
        private TMP_InputField _codeInput, _terminalInput;
        private ScrollRect _consoleScroll, _briefingScroll, _debriefScroll;
        private GameObject _editorSection, _terminalRow, _runRow;
        private RunLineHighlight _runLine;
        private Button _runButton, _debriefButton;
        private TextMeshProUGUI _runButtonLabel;
        private TextMeshProUGUI _runMeter;
        private LayoutElement _statusLayout;

        private TextMeshProUGUI _debriefScore;
        private TextMeshProUGUI _rankLabel, _creditsLabel, _starsLabel, _nextRankLabel;
        private RectTransform _rankBarFill;
        private Button _resetButton;
        private TextMeshProUGUI _resetLabel, _saveHint;
        private bool _resetArmed;

        private string[] _briefingPages;
        private int _briefingPage;
        private Button _briefingPrev, _briefingNext;
        private TextMeshProUGUI _briefingNextLabel, _briefingCounter;

        private string[] _debriefPages;
        private int _debriefPage;
        private Button _debriefPrev, _debriefNext;
        private TextMeshProUGUI _debriefNextLabel, _debriefCounter;

        private const float StatusMinHeight = 96f;
        private const float BoardWidth = 1180f;

        /// <summary>
        /// Splash logo width at the 1920 reference. The mark carries its own subtitle,
        /// so it is sized to leave clear air above the tagline rather than to fill the
        /// frame — the two stacked wordmarks read as one block if they crowd.
        /// </summary>
        private const float LogoWidth = 700f;

        /// <summary>
        /// Sits the mark up in the dark sky band of the panorama rather than over the
        /// lit street, where the neon signage competes with it.
        /// </summary>
        private const float LogoY = 355f;

        /// <summary>Clip name under Resources/Audio. Swap here to change the track.</summary>
        private const string SplashTrack = "ADarkTime";

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
            CanvasRoot = canvasGo.transform;

            BuildTitle(canvasGo.transform);
            BuildBoard(canvasGo.transform);
            BuildBriefing(canvasGo.transform);
            BuildWorkspace(canvasGo.transform);
            BuildDebrief(canvasGo.transform);
        }

        /// <summary>
        /// The splash. Per ART_BRIEF_SPLASH.md this is the one moment the player sees
        /// the city on their own screen, before the camera pulls back to the in-world
        /// workstation — so it is art-led rather than panel-led.
        /// </summary>
        private void BuildTitle(Transform parent)
        {
            _titleBackground = UITheme.Box("TitlePanel", parent, UITheme.PanelSolid);
            _titlePanel = _titleBackground.gameObject;
            UITheme.Stretch(_titlePanel.GetComponent<RectTransform>());

            // Backdrop art rides in one group so it fades and rises together. It is
            // oversized by the rise distance, otherwise sliding it up would expose a
            // strip of empty panel along the bottom edge.
            var backdrop = UITheme.Node("Backdrop", _titlePanel.transform);
            const float over = SplashSequence.BackdropOversize;
            UITheme.Stretch(backdrop.GetComponent<RectTransform>(), -over, -over, -over, -over);
            var backdropGroup = backdrop.AddComponent<CanvasGroup>();

            _splashPanorama = UITheme.CoverImage("Panorama", backdrop.transform, UITheme.Art("ONCALL_CityPanorama"));

            // The panorama is dense and brightly lit, and the logo and copy sit dead
            // centre where it is busiest. A flat scrim under the vignette pushes the
            // city back so it frames the mark instead of competing with it.
            var scrim = UITheme.Box("Scrim", backdrop.transform, new Color(0.02f, 0.03f, 0.05f, 0.45f));
            UITheme.Stretch(scrim.rectTransform);
            scrim.raycastTarget = false;

            UITheme.CoverImage("Vignette", backdrop.transform, UITheme.Art("SplashVignette"));

            SplashLogo splashLogo = null;
            var logoSprite = UITheme.Art("ONCALL_Logo");
            if (logoSprite != null)
            {
                // A bare container: SplashLogo slices the sprite and adds one Image per
                // glyph, so drawing the whole mark here too would double it up.
                var logo = UITheme.Node("Logo", _titlePanel.transform);
                Place(logo.GetComponent<RectTransform>(), 0.5f, 0.5f, new Vector2(0, LogoY),
                      new Vector2(LogoWidth, LogoWidth * logoSprite.rect.height / logoSprite.rect.width));
                splashLogo = logo.AddComponent<SplashLogo>();
                splashLogo.Build(logoSprite);
            }
            else
            {
                // The art is optional; the game still has to boot without it.
                var title = UITheme.Label("Title", _titlePanel.transform,
                    "ONCALL://\nSYSTEMS CONTRACTOR", 64f, UITheme.Accent, TextAlignmentOptions.Center);
                Place(title.rectTransform, 0.5f, 0.5f, new Vector2(0, 170), new Vector2(1200, 220));
            }

            var tag = UITheme.Label("Tagline", _titlePanel.transform,
                "\"The city doesn't sleep. Neither do its systems.\n" +
                " When they break, you get the call.\"", 22f, UITheme.Text,
                TextAlignmentOptions.Center);
            // Tucked under the mark rather than floating mid-frame, where it sat over
            // the busiest part of the street and had to fight the signage to be read.
            Place(tag.rectTransform, 0.5f, 0.5f, new Vector2(0, 150), new Vector2(1200, 120));
            var tagGroup = tag.gameObject.AddComponent<CanvasGroup>();

            // Button and its hint share a group: the call to action arrives as one beat.
            var action = UITheme.Node("Action", _titlePanel.transform);
            UITheme.Stretch(action.GetComponent<RectTransform>());
            var actionGroup = action.AddComponent<CanvasGroup>();

            var btn = UITheme.Button("Connect", action.transform,
                "CONNECT TO ONCALL TERMINAL", UITheme.Accent, () => _gm.StartGame());
            Place(btn.GetComponent<RectTransform>(), 0.5f, 0.5f, new Vector2(0, -180), new Vector2(520, 54));

            var hint = UITheme.Label("TitleHint", action.transform,
                "Progress saves automatically.", UITheme.MicroSize, UITheme.TextDim,
                TextAlignmentOptions.Center);
            Place(hint.rectTransform, 0.5f, 0.5f, new Vector2(0, -240), new Vector2(600, 24));

            _splashSequence = _titlePanel.AddComponent<SplashSequence>();
            _splashSequence.timing = splashTiming;
            _splashSequence.Bind(backdropGroup, splashLogo, tagGroup, actionGroup);
        }

        /// <summary>The splash choreography, so the live city can ride the same clock.</summary>
        public SplashSequence SplashSequence { get { return _splashSequence; } }

        /// <summary>The screen-space canvas every panel is built under. The deck shell
        /// mounts here too, on top of the panels.</summary>
        public Transform CanvasRoot { get; private set; }

        /// <summary>
        /// Clears the way for the real city to show through: the solid panel behind the
        /// splash and the painted panorama both go transparent. The scrim and vignette
        /// stay — they were always what keeps the mark readable over a busy city, and
        /// the live one is busier than the painting.
        ///
        /// Only called once the kit scene has actually loaded, so the painted backdrop
        /// remains the fallback on a clone without the purchased assets.
        /// </summary>
        public void UseLiveCityBackdrop()
        {
            if (_titleBackground != null) _titleBackground.color = Color.clear;
            if (_splashPanorama != null) _splashPanorama.enabled = false;
            if (_splashSequence != null) _splashSequence.ClearBackdropRise();
        }

        private void BuildBoard(Transform parent)
        {
            _boardPanel = UITheme.Box("BoardPanel", parent, UITheme.Backdrop).gameObject;
            UITheme.Stretch(_boardPanel.GetComponent<RectTransform>());

            var header = UITheme.Label("Header", _boardPanel.transform,
                "ONCALL > CONTRACTOR TERMINAL", UITheme.TitleSize, UITheme.Accent,
                TextAlignmentOptions.Center);
            var hrt = header.rectTransform;
            hrt.anchorMin = new Vector2(0.5f, 1f);
            hrt.anchorMax = new Vector2(0.5f, 1f);
            hrt.pivot = new Vector2(0.5f, 1f);
            hrt.sizeDelta = new Vector2(BoardWidth, 52);
            hrt.anchoredPosition = new Vector2(0, -56);

            BuildRankPanel(_boardPanel.transform);

            var sub = UITheme.Label("Sub", _boardPanel.transform,
                "AVAILABLE CONTRACTS", UITheme.SmallSize, UITheme.TextDim);
            var srt = sub.rectTransform;
            srt.anchorMin = new Vector2(0.5f, 1f);
            srt.anchorMax = new Vector2(0.5f, 1f);
            srt.pivot = new Vector2(0.5f, 1f);
            srt.sizeDelta = new Vector2(BoardWidth, 26);
            srt.anchoredPosition = new Vector2(0, -290);

            var listGo = UITheme.Node("List", _boardPanel.transform);
            var lrt = listGo.GetComponent<RectTransform>();
            lrt.anchorMin = new Vector2(0.5f, 1f);
            lrt.anchorMax = new Vector2(0.5f, 1f);
            lrt.pivot = new Vector2(0.5f, 1f);
            lrt.sizeDelta = new Vector2(BoardWidth, 600);
            lrt.anchoredPosition = new Vector2(0, -322);

            var layout = listGo.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 10f;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            _boardList = listGo.transform;

            // Playtesters need a way back to a clean slate. Two-step, so it is
            // never a single misclick away.
            _resetButton = UITheme.Button("ResetProgress", _boardPanel.transform,
                "RESET PROGRESS", UITheme.TextDim, OnResetProgressClicked);
            var rrt = _resetButton.GetComponent<RectTransform>();
            rrt.anchorMin = new Vector2(0.5f, 0f);
            rrt.anchorMax = new Vector2(0.5f, 0f);
            rrt.pivot = new Vector2(0.5f, 0f);
            rrt.sizeDelta = new Vector2(300, 40);
            rrt.anchoredPosition = new Vector2(0, 40);
            _resetLabel = _resetButton.GetComponentInChildren<TextMeshProUGUI>();

            _saveHint = UITheme.Label("SaveHint", _boardPanel.transform,
                "Progress saves automatically.", UITheme.MicroSize, UITheme.TextDim,
                TextAlignmentOptions.Center);
            var shrt = _saveHint.rectTransform;
            shrt.anchorMin = new Vector2(0.5f, 0f);
            shrt.anchorMax = new Vector2(0.5f, 0f);
            shrt.pivot = new Vector2(0.5f, 0f);
            shrt.sizeDelta = new Vector2(600, 24);
            shrt.anchoredPosition = new Vector2(0, 92);
        }

        /// <summary>
        /// Rank, credits and progress toward the next promotion. The board is where
        /// the player picks what to do next, so the reason to care sits above the choice.
        /// </summary>
        private void BuildRankPanel(Transform parent)
        {
            var frame = UITheme.Framed("RankPanel", parent, UITheme.Border);
            var frt = frame.parent.GetComponent<RectTransform>();
            Place(frt, 0.5f, 1f, new Vector2(0, -122), new Vector2(BoardWidth, 150));

            _rankLabel = UITheme.Label("Rank", frame, "", UITheme.SectionSize, UITheme.Accent);
            Place(_rankLabel.rectTransform, 0f, 1f, new Vector2(24, -18), new Vector2(760, 30));

            _creditsLabel = UITheme.Label("Credits", frame, "", UITheme.SectionSize, UITheme.Good,
                TextAlignmentOptions.TopRight);
            Place(_creditsLabel.rectTransform, 1f, 1f, new Vector2(-24, -18), new Vector2(380, 30));

            _starsLabel = UITheme.Label("Stars", frame, "", UITheme.BodySize, UITheme.Text);
            _starsLabel.richText = true;
            Place(_starsLabel.rectTransform, 0f, 1f, new Vector2(24, -60), new Vector2(BoardWidth - 48, 26));

            var barBg = UITheme.Box("RankBarBg", frame, UITheme.Border);
            Place(barBg.rectTransform, 0f, 1f, new Vector2(24, -96), new Vector2(BoardWidth - 48, 10));

            var fill = UITheme.Box("Fill", barBg.transform, UITheme.Accent);
            _rankBarFill = fill.rectTransform;
            _rankBarFill.anchorMin = Vector2.zero;
            _rankBarFill.anchorMax = new Vector2(0f, 1f);
            _rankBarFill.pivot = new Vector2(0f, 0.5f);
            _rankBarFill.offsetMin = Vector2.zero;
            _rankBarFill.offsetMax = Vector2.zero;

            _nextRankLabel = UITheme.Label("NextRank", frame, "", UITheme.MicroSize, UITheme.TextDim);
            Place(_nextRankLabel.rectTransform, 0f, 1f, new Vector2(24, -114), new Vector2(BoardWidth - 48, 22));
        }

        /// <summary>Anchors a rect to one corner of its parent at a fixed size.</summary>
        private static void Place(RectTransform rt, float anchorX, float anchorY,
                                  Vector2 position, Vector2 size)
        {
            rt.anchorMin = new Vector2(anchorX, anchorY);
            rt.anchorMax = new Vector2(anchorX, anchorY);
            rt.pivot = new Vector2(anchorX, anchorY);
            rt.sizeDelta = size;
            rt.anchoredPosition = position;
        }

        private void RefreshRankPanel()
        {
            if (_rankLabel == null) return;

            int total = _gm.State.TotalStars;
            int max = ContractRegistry.MaxTotalStars;

            _rankLabel.text = _gm.State.Rank.ToUpperInvariant();
            _creditsLabel.text = _gm.State.Credits.ToString("N0") + " cr";

            var sb = new System.Text.StringBuilder();
            sb.Append("<color=").Append(UITheme.Hex(UITheme.Good)).Append('>')
              .Append('◆', total).Append("</color>")
              .Append("<color=").Append(UITheme.Hex(UITheme.TextDim)).Append('>')
              .Append('◇', Mathf.Max(0, max - total)).Append("</color>")
              .Append("   ").Append(total).Append(" / ").Append(max);
            _starsLabel.text = sb.ToString();

            _rankBarFill.anchorMax = new Vector2(Mathf.Clamp01(Scoring.RankProgress(total)), 1f);
            _rankBarFill.offsetMin = Vector2.zero;
            _rankBarFill.offsetMax = Vector2.zero;

            string next = Scoring.NextRankTitle(total);
            _nextRankLabel.text = next == null
                ? "Top rank reached."
                : Scoring.StarsToNextRank(total) + "◆ to " + next;
        }

        private void OnResetProgressClicked()
        {
            if (!_resetArmed)
            {
                _resetArmed = true;
                _resetLabel.text = "CONFIRM — ERASE ALL PROGRESS?";
                _resetLabel.color = UITheme.Fault;
                return;
            }
            DisarmReset();
            _gm.ResetProgress();
        }

        private void DisarmReset()
        {
            _resetArmed = false;
            if (_resetLabel == null) return;
            _resetLabel.text = "RESET PROGRESS";
            _resetLabel.color = UITheme.TextDim;
        }

        private void BuildBriefing(Transform parent)
        {
            _briefingPanel = MakeRightPanel("BriefingPanel", parent, out var content);

            var header = UITheme.Label("Header", content, "INCOMING TRANSMISSION",
                UITheme.SectionSize, UITheme.Accent);
            AddLayout(header.gameObject, 34f, 0f);

            _briefingText = UITheme.ScrollText("BriefingScroll", content, out _briefingScroll, true);
            _briefingText.richText = true;
            _briefingText.lineSpacing = 6f;
            _briefingText.paragraphSpacing = 10f;
            AddLayout(_briefingScroll.gameObject, 0f, 1f);

            // Paged rather than one long scroll.
            var pageRow = MakeRow(content, 44f);
            _briefingPrev = UITheme.Button("Prev", pageRow, "◂ BACK", UITheme.TextDim,
                () => ShowBriefingPage(_briefingPage - 1));
            _briefingCounter = UITheme.Label("Count", pageRow, "", UITheme.MicroSize,
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
            _briefingText.text = TextMarkup.Format(_briefingPages[_briefingPage]);

            bool last = _briefingPage == _briefingPages.Length - 1;
            _briefingNextLabel.text = last ? "JACK IN ▸" : "NEXT ▸";
            _briefingNextLabel.color = last ? UITheme.Good : UITheme.Accent;
            _briefingPrev.interactable = _briefingPage > 0;
            _briefingCounter.text = (_briefingPage + 1) + " / " + _briefingPages.Length;

            StartCoroutine(LayoutPage(_briefingScroll, _briefingText));
        }

        /// <summary>
        /// The workspace is the deck. A full-screen node holds the shell so RefreshScreen
        /// can still show and hide the whole screen with one SetActive, but there is no
        /// docked panel any more — the world is full-frame and the windows float over it.
        /// See docs/DECK_SPEC.md §2.
        /// </summary>
        private void BuildWorkspace(Transform parent)
        {
            _workspacePanel = UITheme.Node("WorkspacePanel", parent);
            UITheme.Stretch(_workspacePanel.GetComponent<RectTransform>());

            _deck = _workspacePanel.AddComponent<DeckShell>();
            _deck.settings = deckLayout;
            _deck.Build(_workspacePanel.transform);

            // Navigation moves into the rail: with no docked panel there is nowhere else
            // for it to live, and the rail is the one thing that is never occluded.
            // Briefing opens in place rather than leaving the deck. DECK_SPEC §6: it is a
            // window, and "briefings should never be one-shot".
            _deck.AddTool("briefing", "!", true, OpenBriefingWindow);
            _debriefButton = _deck.AddTool("debrief", "*", true, () => _gm.GoTo(GameScreen.Debrief));
            _deck.AddTool("board", "#", true, () => _gm.BackToBoard());
            _deck.AddTool("reference", "?", false, null);   // locked but visible, on purpose
            _deck.AddTool("store", "$", false, null);

            BuildEditorWindow();
            BuildTerminalWindow();
            BuildReadoutWindow();
            BuildBriefingWindow();
        }

        /// <summary>A padded vertical stack filling a window's content area.</summary>
        private static Transform WindowColumn(DeckWindow window)
        {
            var col = UITheme.Node("Column", window.Content).GetComponent<RectTransform>();
            UITheme.Stretch(col, 8f, 8f, 8f, 8f);
            var v = col.gameObject.AddComponent<VerticalLayoutGroup>();
            v.spacing = 6f;
            v.childControlHeight = true;
            v.childControlWidth = true;
            v.childForceExpandHeight = false;
            v.childForceExpandWidth = true;
            return col;
        }

        private void BuildEditorWindow()
        {
            // Explicit default layout rather than a cascade: DECK_SPEC.md §14 asks what is
            // open when the boot surface clears, and three windows landing on top of each
            // other answers it badly. Editor top-left, output beneath it, readout beside.
            // The player can rearrange; this is only where they start.
            _editorWindow = _deck.Open("editor", "main.py", new Vector2(620f, 420f),
                                       false, new Vector2(16f, -16f));
            var col = WindowColumn(_editorWindow);
            _editorSection = col.gameObject;

            _codeInput = MakeInputField("CodeInput", col, true, UITheme.CodeSize);
            AddLayout(_codeInput.gameObject, 0f, 1f);
            _codeInput.onValueChanged.AddListener(code => _gm.SetScript(code));
            _codeInput.gameObject.AddComponent<CodeEditorBehaviour>();
            _runLine = RunLineHighlight.Attach(_codeInput, UITheme.RunLine);

            _runRow = MakeRow(col, 42f).gameObject;
            // One button: starts a run, and stops it while one is in progress.
            _runButton = UITheme.Button("Run", _runRow.transform, "▶ RUN", UITheme.Good, ToggleRun);
            _runButtonLabel = _runButton.GetComponentInChildren<TextMeshProUGUI>();
            UITheme.Button("Reset", _runRow.transform, "RESET SCRIPT", UITheme.Warn, ResetScript);

            // Call meter. The star rating is otherwise only revealed at the debrief, by
            // which point the script is closed and the player can no longer act on it.
            // Putting the count here is what turns "I finished it" into "I could do that
            // in fewer".
            _runMeter = UITheme.Label("RunMeter", col, "", UITheme.SmallSize, UITheme.TextDim);
            AddLayout(_runMeter.gameObject, 20f, 0f);
        }

        /// <summary>
        /// Output for every contract, plus the input row for the ones that take commands.
        /// Python contracts still need somewhere for print() to land, so this window is
        /// always open — only the input row and the title change.
        /// </summary>
        private void BuildTerminalWindow()
        {
            _terminalWindow = _deck.Open("terminal", "output", new Vector2(620f, 330f),
                                         false, new Vector2(16f, -456f));
            var col = WindowColumn(_terminalWindow);

            var consoleFrame = UITheme.Framed("ConsoleFrame", col, UITheme.Border);
            AddLayout(consoleFrame.parent.gameObject, 0f, 1f);
            _consoleText = UITheme.ScrollText("ConsoleScroll", consoleFrame, out _consoleScroll);
            UITheme.Stretch(_consoleScroll.GetComponent<RectTransform>(), 4, 4, 4, 4);
            _consoleText.fontSize = UITheme.SmallSize;

            _terminalRow = UITheme.Node("TerminalRow", col);
            AddLayout(_terminalRow, 40f, 0f);
            var termLayout = _terminalRow.AddComponent<HorizontalLayoutGroup>();
            termLayout.spacing = 6f;
            termLayout.childControlHeight = true;
            termLayout.childControlWidth = true;
            termLayout.childForceExpandWidth = true;

            _terminalInput = MakeInputField("TerminalInput", _terminalRow.transform, false,
                UITheme.CodeSize);
            _terminalInput.onSubmit.AddListener(OnTerminalSubmit);

            var sendBtn = UITheme.Button("Send", _terminalRow.transform, "ENTER", UITheme.Accent,
                () => OnTerminalSubmit(_terminalInput.text));
            var sendLayout = sendBtn.gameObject.AddComponent<LayoutElement>();
            sendLayout.preferredWidth = 110f;
            sendLayout.flexibleWidth = 0f;
        }

        /// <summary>
        /// The dispatcher's message, re-openable from the rail at any point during work —
        /// DECK_SPEC §6, "briefings should never be one-shot". Starts closed, because the
        /// player has just read it on the way in.
        ///
        /// Full text rather than the paged first-read: this window is for going back to
        /// check something, where scanning the whole transmission beats stepping through
        /// it again. The paced, paged version stays on the pre-work screen.
        /// </summary>
        private void BuildBriefingWindow()
        {
            _briefingWindow = _deck.Open("briefing", "transmission", new Vector2(560f, 430f),
                                         false, new Vector2(120f, -90f));
            var col = WindowColumn(_briefingWindow);

            var frame = UITheme.Framed("BriefFrame", col, UITheme.Border);
            AddLayout(frame.parent.gameObject, 0f, 1f);
            _briefingWindowText = UITheme.ScrollText("BriefWindowScroll", frame,
                                                     out _briefingWindowScroll, true);
            _briefingWindowText.richText = true;
            _briefingWindowText.lineSpacing = 6f;
            _briefingWindowText.paragraphSpacing = 10f;
            UITheme.Stretch(_briefingWindowScroll.GetComponent<RectTransform>(), 4, 4, 4, 4);

            _briefingWindow.gameObject.SetActive(false);
        }

        private void OpenBriefingWindow()
        {
            if (_briefingWindow == null || _gm.ActiveContract == null) return;
            _briefingWindowText.text = TextMarkup.Format(_gm.ActiveContract.GetBriefing());
            _briefingWindow.gameObject.SetActive(true);
            _deck.Focus(_briefingWindow);
            StartCoroutine(ScrollToTop(_briefingWindowScroll));
        }

        /// <summary>Live system state — the numeric companion to the world. Always open.</summary>
        private void BuildReadoutWindow()
        {
            // Sized to fit the window field beside the editor: 656 + 420 = 1076, inside the
            // ~1094 the 35/57/8 split leaves. Wider slides under the rail and clips.
            _readoutWindow = _deck.Open("readout", "system", new Vector2(420f, 340f),
                                        false, new Vector2(656f, -16f));
            var col = WindowColumn(_readoutWindow);

            var statusFrame = UITheme.Framed("StatusFrame", col, UITheme.Border);
            _statusLayout = AddLayout(statusFrame.parent.gameObject, StatusMinHeight, 1f);
            // Mono-small, not body size: status lines are column-aligned so they must not
            // wrap, and the longest a contract prints has to fit the readout's width.
            _wsStatus = UITheme.Label("Status", statusFrame, "", UITheme.SmallSize, UITheme.Text);
            UITheme.Stretch(_wsStatus.rectTransform, 10, 6, 10, 6);

            // The hint lives here rather than in the editor, because terminal contracts
            // close the editor and would otherwise lose it. No fixed height: it wraps to
            // two lines for terminal contracts and a forced 22px clips the second one.
            _hintText = UITheme.Label("Hint", col, "", UITheme.MicroSize, UITheme.TextDim,
                TextAlignmentOptions.TopLeft, true);
        }

        private void BuildDebrief(Transform parent)
        {
            // Docked right like the briefing, so the site the player just fixed stays
            // on screen behind it instead of the world vanishing at the payoff moment.
            Transform col;
            _debriefPanel = MakeRightPanel("DebriefPanel", parent, out col);

            _debriefHeader = UITheme.Label("Header", col, "CONTRACT COMPLETE",
                UITheme.SectionSize, UITheme.Good);
            AddLayout(_debriefHeader.gameObject, 34f, 0f);

            var scoreFrame = UITheme.Framed("ScoreFrame", col, UITheme.Border);
            AddLayout(scoreFrame.parent.gameObject, 96f, 0f);
            _debriefScore = UITheme.Label("Score", scoreFrame, "", UITheme.BodySize, UITheme.Good);
            UITheme.Stretch(_debriefScore.rectTransform, 12, 8, 12, 8);

            _debriefText = UITheme.ScrollText("DebriefScroll", col, out _debriefScroll, true);
            _debriefText.richText = true;
            _debriefText.lineSpacing = 6f;
            _debriefText.paragraphSpacing = 10f;
            AddLayout(_debriefScroll.gameObject, 0f, 1f);

            var row = MakeRow(col, 46f);
            _debriefPrev = UITheme.Button("Prev", row, "◂ BACK", UITheme.TextDim,
                () => ShowDebriefPage(_debriefPage - 1));
            _debriefCounter = UITheme.Label("Count", row, "", UITheme.MicroSize,
                UITheme.TextDim, TextAlignmentOptions.Center);
            _debriefNext = UITheme.Button("Next", row, "NEXT ▸", UITheme.Accent, DebriefAdvance);
            _debriefNextLabel = _debriefNext.GetComponentInChildren<TextMeshProUGUI>();

            var row2 = MakeRow(col, 40f);
            UITheme.Button("Board", row2, "CONTRACT BOARD", UITheme.TextDim, () => _gm.BackToBoard());
        }

        /// <summary>Stars, credits and efficiency for the run that just finished.</summary>
        private void RefreshDebriefScore()
        {
            if (_debriefScore == null || _gm.ActiveContract == null) return;

            var sb = new System.Text.StringBuilder();
            sb.Append(Scoring.FormatStars(_gm.LastStars)).Append("   ");
            sb.Append(_gm.LastCreditsEarned > 0
                ? "+" + _gm.LastCreditsEarned + " cr"
                : "no new credits — already rated this well");
            sb.AppendLine();

            if (_gm.ActiveContract.Kind != ContractKind.Terminal && _gm.LastCallsToGoal > 0)
            {
                sb.Append("Calls to goal: ").Append(_gm.LastCallsToGoal);
                int target = _gm.ActiveContract.ThreeStarCalls;
                if (target > 0 && _gm.LastStars < 3) sb.Append("   (").Append(target).Append(" for ◆◆◆)");
                sb.AppendLine();
            }
            else if (_gm.ActiveContract.HasBonus)
            {
                sb.AppendLine(_gm.LastBonusFound
                    ? "Bonus found."
                    : "Bonus missed — " + _gm.ActiveContract.BonusHint);
            }

            sb.Append(_gm.State.Rank).Append("   ·   ")
              .Append(_gm.State.TotalStars).Append("◆   ·   ")
              .Append(_gm.State.Credits).Append(" cr total");

            _debriefScore.text = sb.ToString();
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
            _debriefText.text = TextMarkup.Format(_debriefPages[_debriefPage]);

            bool last = _debriefPage == _debriefPages.Length - 1;
            // On the last page the button becomes the action the debrief was leading to.
            _debriefNextLabel.text = last
                ? (_gm.DebriefInvitesRetry ? "TRY IT ▸" : "CONTRACT BOARD ▸")
                : "NEXT ▸";
            _debriefNextLabel.color = last ? UITheme.Good : UITheme.Accent;
            _debriefPrev.interactable = _debriefPage > 0;
            _debriefCounter.text = (_debriefPage + 1) + " / " + _debriefPages.Length;

            StartCoroutine(LayoutPage(_debriefScroll, _debriefText));
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

        private static TMP_InputField MakeInputField(string name, Transform parent, bool multiline,
                                                     float size)
        {
            var border = UITheme.Box(name, parent, UITheme.Border);
            var bg = UITheme.Box("Bg", border.transform, UITheme.Field);
            UITheme.Stretch(bg.rectTransform, 1, 1, 1, 1);

            var input = border.gameObject.AddComponent<TMP_InputField>();

            var textArea = UITheme.Node("TextArea", bg.transform);
            var tart = textArea.GetComponent<RectTransform>();
            UITheme.Stretch(tart, 8, 6, 8, 6);
            textArea.AddComponent<RectMask2D>();

            var text = UITheme.Label("Text", textArea.transform, "", size, UITheme.Text);
            UITheme.Stretch(text.rectTransform);
            text.overflowMode = TextOverflowModes.Overflow;

            var placeholder = UITheme.Label("Placeholder", textArea.transform,
                multiline ? "" : "type a command…", size, UITheme.TextDim);
            UITheme.Stretch(placeholder.rectTransform);

            input.textViewport = tart;
            input.textComponent = text;
            input.placeholder = placeholder;
            input.fontAsset = UITheme.Mono;
            input.pointSize = size;
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

            // Starts on the splash and carries through; the track is atmospheric enough
            // to underscore the whole session rather than cutting at the board.
            if (GameAudio.Instance != null)
            {
                GameAudio.Instance.PlayMusic(SplashTrack);
                // Rain and grid hum sit under everything — it is the same city whether
                // the player is on the splash or inside a contract.
                GameAudio.Instance.PlayAmbience();
            }

            if (s == GameScreen.Board) RebuildBoard();
            else DisarmReset();

            if (s == GameScreen.Briefing && _gm.ActiveContract != null)
            {
                _briefingPages = Contract.Paginate(_gm.ActiveContract.GetBriefing());
                ShowBriefingPage(0);
            }

            if (s == GameScreen.Debrief && _gm.ActiveContract != null)
            {
                // The payoff moment. Scaled by rating, so three stars sounds like more
                // than one — the world-as-feedback principle applied to audio.
                if (GameAudio.Instance != null)
                    GameAudio.Instance.Play(Sfx.Complete, 0.6f + 0.2f * Mathf.Max(1, _gm.LastStars));

                string text = _gm.CurrentDebriefText;
                if (string.IsNullOrEmpty(text)) text = _gm.ActiveContract.GetCompletionMessage();
                _debriefPages = Contract.Paginate(text);
                ShowDebriefPage(0);
                RefreshDebriefScore();
            }

            if (s == GameScreen.Workspace && _gm.ActiveContract != null) SetupWorkspace();
        }

        private void SetupWorkspace()
        {
            var contract = _gm.ActiveContract;
            bool takesCode = contract.Kind != ContractKind.Terminal;
            bool takesCommands = contract.Kind == ContractKind.Terminal
                              || contract.Kind == ContractKind.Combined;

            // The rail names what you are plugged into — DECK_SPEC §2, Link zone. This
            // replaces the docked panel's header, which no longer has anywhere to sit.
            if (_deck != null)
                _deck.SetLink(contract.Title.ToUpperInvariant() + "\n"
                              + contract.Location.ToUpperInvariant(), true);

            // Combined contracts get an editor and a terminal at once. The docked panel
            // structurally could not do that — it only ever asked "is this terminal?" and
            // gave combined contracts the editor alone. Windows are what fix it.
            if (_editorWindow != null) _editorWindow.gameObject.SetActive(takesCode);
            if (_terminalWindow != null)
                _terminalWindow.SetTitle(takesCommands ? "terminal" : "output");
            _terminalRow.SetActive(takesCommands);

            if (takesCode) _codeInput.SetTextWithoutNotify(_gm.GetScript());

            if (takesCode && takesCommands)
                _hintText.text = "Editor and terminal both live · Alt+1/2 to switch";
            else if (takesCode)
                _hintText.text = "Tab indents · commands: " + string.Join(", ", CommandNames(contract));
            else
                _hintText.text = "Type commands directly · 'reset' restores the filesystem";

            if (takesCommands) StartCoroutine(FocusTerminal());

            RefreshStatus();
            RefreshConsole();
        }

        private static string[] CommandNames(Contract contract)
        {
            var keys = new List<string>(contract.GetCommands().Keys);
            keys.Sort();
            return keys.ToArray();
        }

        /// <summary>
        /// Feeds the rail's live zones. Contracts expose only IsGoalMet/GetStatusText — there
        /// is no per-contract objective list in the model yet — so the checklist is the one
        /// real objective the contract has. A proper multi-item checklist is backlog A5 and
        /// needs a Contract-level API first; this is deliberately not faked with dummy rows.
        /// </summary>
        private void RefreshRail()
        {
            if (_deck == null) return;

            _deck.SetStatus(_gm.State.Credits.ToString("N0") + " cr · " + _gm.State.Rank);

            _deck.ClearObjectives();
            var c = _gm.ActiveContract;
            if (c != null) _deck.AddObjective(c.Title, c.Completed);
        }

        private void RefreshStatus()
        {
            RefreshRail();
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

            if (_runLine != null) _runLine.SetLine(_gm.IsRunning ? _gm.CurrentLine : 0);

            bool done = _gm.ActiveContract.Completed;
            _wsStatus.color = done ? UITheme.Good : UITheme.Text;
            if (_debriefButton != null) _debriefButton.gameObject.SetActive(done);

            if (_runButtonLabel != null)
            {
                _runButtonLabel.text = _gm.IsRunning ? "■ STOP" : "▶ RUN";
                _runButtonLabel.color = _gm.IsRunning ? UITheme.Fault : UITheme.Good;
            }

            RefreshRunMeter();
        }

        /// <summary>
        /// Shows what the last run cost, next to the button that starts the next one.
        ///
        /// The three-star target stays hidden until the contract has been solved once.
        /// The player should reach their own answer first and only then learn there was
        /// a tighter one — showing the target up front turns a discovery into a chore.
        /// </summary>
        private void RefreshRunMeter()
        {
            if (_runMeter == null) return;

            var c = _gm.ActiveContract;
            bool scripted = c != null && c.Kind != ContractKind.Terminal;

            if (!scripted || _gm.IsRunning)
            {
                _runMeter.text = "";
                return;
            }

            // Before the first run of a session, show the standing best instead.
            // Solving the contract jumps to the debrief, so a rating shown only after
            // a successful run is a rating nobody reads. Coming back to improve one is
            // exactly when the number matters, and this is what the player sees then.
            if (!_gm.HasRunThisSession)
            {
                var best = _gm.State.ScoreFor(_gm.ActiveDef.Id);
                if (best == null || best.CallsToGoal <= 0) { _runMeter.text = ""; return; }

                string bestLine = "best · " + best.CallsToGoal + " calls · " +
                                  Scoring.FormatStars(best.Stars);
                if (best.Stars < 3 && c.ThreeStarCalls > 0)
                    bestLine += "    " + c.ThreeStarCalls + " = " + Scoring.FormatStars(3);

                _runMeter.text = bestLine;
                _runMeter.color = best.Stars == 3 ? UITheme.Good : UITheme.TextDim;
                return;
            }

            int calls = _gm.LastRunCallsToGoal;

            if (calls <= 0)
            {
                _runMeter.text = "ran · " + _gm.LastRunTotalCalls + " calls · goal not met";
                _runMeter.color = UITheme.TextDim;
                return;
            }

            int stars = Scoring.RateContract(calls, c.ThreeStarCalls, c.TwoStarCalls);
            if (stars == 3 && !_gm.LastRunLoopDidWork) stars = 2;

            string line = "ran · " + calls + " calls · " + Scoring.FormatStars(stars);

            // Only once they have a completion behind them.
            var prior = _gm.State.ScoreFor(_gm.ActiveDef.Id);
            if (prior != null && stars < 3 && c.ThreeStarCalls > 0)
                line += "    " + c.ThreeStarCalls + " = " + Scoring.FormatStars(3);

            _runMeter.text = line;
            _runMeter.color = stars == 3 ? UITheme.Good : UITheme.TextDim;
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

        private const int PagePadding = 8;

        /// <summary>
        /// Briefing pages are written one idea at a time, so a short page would
        /// otherwise sit in the top corner of a tall panel. Pages that fit are pushed
        /// down to the middle; pages that overflow stay top-aligned and scroll.
        /// </summary>
        private IEnumerator LayoutPage(ScrollRect scroll, TextMeshProUGUI text)
        {
            if (scroll == null || text == null) yield break;

            var layout = text.transform.parent.GetComponent<VerticalLayoutGroup>();
            if (layout == null) { yield return ScrollToTop(scroll); yield break; }

            layout.padding = new RectOffset(PagePadding, PagePadding, PagePadding, PagePadding);

            yield return null;
            Canvas.ForceUpdateCanvases();

            // Centred, but capped: a one-paragraph page floating in the middle of a
            // tall panel reads as a mistake rather than as composition.
            float slack = scroll.viewport.rect.height - scroll.content.rect.height;
            if (slack > 1f)
            {
                int top = PagePadding + Mathf.RoundToInt(Mathf.Min(slack * 0.5f, 140f));
                layout.padding = new RectOffset(PagePadding, PagePadding, top, PagePadding);
                LayoutRebuilder.MarkLayoutForRebuild(scroll.content);
            }

            yield return null;
            Canvas.ForceUpdateCanvases();
            scroll.verticalNormalizedPosition = 1f;
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
            else
            {
                if (GameAudio.Instance != null) GameAudio.Instance.Play(Sfx.Run);
                _gm.RunScript();
            }
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
            RefreshRankPanel();

            for (int i = _boardList.childCount - 1; i >= 0; i--) Destroy(_boardList.GetChild(i).gameObject);

            for (int i = 0; i < ContractRegistry.All.Count; i++)
            {
                var def = ContractRegistry.All[i];
                bool completed = _gm.State.IsContractCompleted(def.Id);
                bool available = _gm.IsAvailable(i);
                int stars = _gm.State.StarsFor(def.Id);

                Color color = completed ? UITheme.Good : (available ? UITheme.Accent : UITheme.TextDim);

                var captured = def;
                var btn = UITheme.Button("Contract" + i, _boardList, "", color,
                    available ? (UnityEngine.Events.UnityAction)(() => _gm.OpenContract(captured)) : null);
                btn.interactable = available;

                var le = btn.gameObject.AddComponent<LayoutElement>();
                le.preferredHeight = 58f;

                // Three anchored columns rather than one padded string, so the row stays
                // aligned regardless of title length or a later change of font.
                var title = btn.GetComponentInChildren<TextMeshProUGUI>();
                title.text = "[" + (i + 1) + "]  " + def.Title;
                title.fontSize = UITheme.BodySize;
                title.color = color;
                title.alignment = TextAlignmentOptions.Left;
                title.overflowMode = TextOverflowModes.Ellipsis;
                title.margin = new Vector4(20, 0, 8, 0);
                Span(title.rectTransform, 0f, 0.46f);

                var inner = title.transform.parent;

                var loc = UITheme.Label("Location", inner, def.Location,
                    UITheme.SmallSize, UITheme.TextDim, TextAlignmentOptions.Left);
                loc.overflowMode = TextOverflowModes.Ellipsis;
                loc.raycastTarget = false;
                Span(loc.rectTransform, 0.46f, 0.72f);

                var status = UITheme.Label("Status", inner,
                    StatusMarkup(def, completed, available, stars),
                    UITheme.BodySize, color, TextAlignmentOptions.Right);
                status.richText = true;
                status.margin = new Vector4(8, 0, 20, 0);
                status.raycastTarget = false;
                Span(status.rectTransform, 0.72f, 1f);
            }
        }

        /// <summary>Stretches a rect between two horizontal fractions of its parent.</summary>
        private static void Span(RectTransform rt, float min, float max)
        {
            rt.anchorMin = new Vector2(min, 0f);
            rt.anchorMax = new Vector2(max, 1f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        /// <summary>Earned rating and payout, or why the row cannot be clicked yet.</summary>
        private static string StatusMarkup(ContractDef def, bool completed, bool available, int stars)
        {
            if (!completed) return available ? "[AVAILABLE]" : "[LOCKED]";

            int earned = Scoring.CreditsFor(stars, ContractRegistry.BaseCreditsFor(def.Id));
            return "<color=" + UITheme.Hex(UITheme.Good) + ">" + new string('◆', stars) + "</color>"
                 + "<color=" + UITheme.Hex(UITheme.TextDim) + ">"
                 + new string('◇', Scoring.MaxStars - stars) + "</color>"
                 + "   <color=" + UITheme.Hex(UITheme.TextDim) + ">" + earned + " cr</color>";
        }
    }
}
