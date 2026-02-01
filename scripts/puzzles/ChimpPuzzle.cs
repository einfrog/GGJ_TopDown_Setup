using System.Collections.Generic;
using System.Linq;
using Godot;

namespace GGJ_2026.scripts.puzzles;

/// <summary>
/// Chimp test puzzle logic.
/// Handles round progression, grid generation, input validation,
/// and reports success/failure to the main game.
/// </summary>
public partial class ChimpPuzzle : Control
{
    // ============================================================
    // SIGNALS
    // ============================================================

    [Signal]
    public delegate void PuzzleFinishedEventHandler(bool success);

    // ============================================================
    // DIFFICULTY / PROGRESSION SETTINGS
    // ============================================================

    [Export] public int StartN =2 ;   // First round size
    [Export] public int MaxN = 2;     // Final round size

    // ============================================================
    // GRID / VISUAL SETTINGS
    // ============================================================

    [Export] public int GridColumns = 5;   // Columns in GridContainer
    [Export] public int CellSize = 72;     // Button size (px)

    // ============================================================
    // NODE REFERENCES (ASSIGNED IN EDITOR)
    // ============================================================

    [Export] public NodePath GridPath;
    [Export] public NodePath StatusLabelPath;
    [Export] public NodePath RetryButtonPath;
    [Export] public NodePath CloseButtonPath;

    // Result overlay (same pattern as Hanoi)
    [Export] public NodePath ResultOverlayPath;
    [Export] public NodePath ResultTitlePath;
    [Export] public NodePath ResultBodyPath;
    [Export] public NodePath RestartButtonPath;
    [Export] public NodePath ContinueButtonPath;

    protected GridContainer _grid;
    protected Label _statusLabel;
    protected Button _retryButton;
    protected Button _closeButton;

    // Overlay cached nodes
    private ColorRect _resultOverlay;
    private Label _resultTitle;
    private Label _resultBody;
    private Button _restartButton;
    private Button _continueButton;

    // If set, Continue emits this. If null, Continue closes as failure.
    private bool? _pendingResult = null;

    // ============================================================
    // INTERNAL STATE
    // ============================================================

    protected int _currentN;
    protected int _expectedNext;

    protected List<Button> _cells = new();
    protected Dictionary<Button, int> _numberByButton = new();

    protected enum PuzzleState
    {
        Idle,
        ShowingNumbers,
        WaitingFirstClick,
        InSequence,
        Failed,
        Completed
    }

    protected PuzzleState _state = PuzzleState.Idle;

    // ============================================================
    // GODOT LIFECYCLE
    // ============================================================

    public override void _Ready()
    {
        GD.Print($"Chimp overlay null? {_resultOverlay == null}, continue null? {_continueButton == null}");

        _grid = GetNode<GridContainer>(GridPath);
        _statusLabel = GetNode<Label>(StatusLabelPath);
        _retryButton = GetNode<Button>(RetryButtonPath);
        _closeButton = GetNode<Button>(CloseButtonPath);

        _grid.Columns = Mathf.Max(1, GridColumns);

        _retryButton.Pressed += OnRetryPressed;
        _closeButton.Pressed += OnClosePressed;

        // Optional: hide retry until needed
        _retryButton.Visible = false;

        // Overlay nodes
        _resultOverlay = GetNodeOrNull<ColorRect>(ResultOverlayPath);
        _resultTitle = GetNodeOrNull<Label>(ResultTitlePath);
        _resultBody = GetNodeOrNull<Label>(ResultBodyPath);
        _restartButton = GetNodeOrNull<Button>(RestartButtonPath);
        _continueButton = GetNodeOrNull<Button>(ContinueButtonPath);

        // Wire overlay buttons (null-safe)
        if (_restartButton != null)
        {
            _restartButton.Pressed += () =>
            {
                HideResultOverlay();
                StartFromBeginning();
            };
        }

        if (_continueButton != null)
        {
            _continueButton.Pressed += () =>
            {
                EmitSignal(SignalName.PuzzleFinished, _pendingResult ?? false);
            };
        }


        // Ensure overlay hidden on start
        HideResultOverlay();

        StartFromBeginning();
    }

    // ============================================================
    // PUZZLE FLOW
    // ============================================================

    public void StartFromBeginning()
    {
        HideResultOverlay();

        _currentN = Mathf.Max(2, StartN);
        _expectedNext = 1;
        _retryButton.Visible = false;

        StartRound(_currentN);
    }

    protected void StartRound(int n)
    {
        _currentN = Mathf.Clamp(n, StartN, MaxN);
        _expectedNext = 1;

        ClearGrid();
        BuildGrid(_currentN);

        // Build-and-validate: avoid softlocks
        const int maxAttempts = 5;
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            if (ValidateRoundMapping(_currentN, out string err))
                break;

            GD.PushWarning($"ChimpPuzzle: invalid round mapping (attempt {attempt}/{maxAttempts}): {err}. Rebuilding...");

            ClearGrid();
            BuildGrid(_currentN);

            if (attempt == maxAttempts)
            {
                FailRound("Internal error generating tiles.");
                return;
            }
        }

        ShowNumbers(true);
        SetNumberedButtonsEnabled(true);

        _state = PuzzleState.WaitingFirstClick;
        _statusLabel.Text = $"Memorize 1-{_currentN} and click 1.";
    }

    // ============================================================
    // GRID GENERATION
    // ============================================================

    protected void BuildGrid(int n)
    {
        int cols = Mathf.Max(1, GridColumns);

        // NOTE: you currently hardcode rows=5. Keep it if you want.
        int rows = 5;
        int totalCells = rows * cols;

        for (int i = 0; i < totalCells; i++)
        {
            var btn = new Button
            {
                CustomMinimumSize = new Vector2(CellSize, CellSize),
                FocusMode = FocusModeEnum.None,
                Text = "",
                Disabled = true,

            };

            Button captured = btn;
            btn.Pressed += () => OnCellPressed(captured);

            _grid.AddChild(btn);
            _cells.Add(btn);
        }

        var rng = new RandomNumberGenerator();
        rng.Randomize();

        List<int> indices = Enumerable.Range(0, totalCells).ToList();
        Shuffle(indices, rng);

        for (int number = 1; number <= n; number++)
        {
            var cell = _cells[indices[number - 1]];
            _numberByButton[cell] = number;
            cell.Disabled = false;
        }
    }

    protected void ClearGrid()
    {
        foreach (var btn in _cells)
            btn.QueueFree();

        _cells.Clear();
        _numberByButton.Clear();
    }

    // ============================================================
    // INPUT HANDLING
    // ============================================================

    protected void OnCellPressed(Button pressedButton)
    {
        // Block interaction while overlay is visible
        if (_resultOverlay != null && _resultOverlay.Visible)
            return;

        if (_state is PuzzleState.Failed or PuzzleState.Completed)
            return;

        if (!_numberByButton.TryGetValue(pressedButton, out int value))
            return;

        if (_state == PuzzleState.WaitingFirstClick)
        {
            if (value != 1)
            {
                FailRound("Wrong start! You must click 1.");
                return;
            }
            //this commented code makes the revealed buttons state 
            // pressedButton.Disabled = true;
            // _expectedNext = 2;
            //
            // foreach (var kv in _numberByButton)
            // {
            //     if (kv.Key != pressedButton)
            //         kv.Key.Text = "";
            // }
            ConsumeCell(pressedButton);
            _expectedNext = 2;

            foreach (var kv in _numberByButton)
            {
                if (kv.Key != pressedButton)
                    kv.Key.Text = "";
            }
            foreach (var kv in _numberByButton)
            {
                if (kv.Key != pressedButton)
                    kv.Key.Text = "";
            }



            _state = PuzzleState.InSequence;
            _statusLabel.Text = $"Good. Now click {_expectedNext}.";
            return;
        }

        if (_state == PuzzleState.InSequence)
        {
            if (value != _expectedNext)
            {
                FailRound($"Wrong! Expected {_expectedNext}.");
                return;
            }
            //old code to reveal buttons
            // pressedButton.Disabled = true;
            // pressedButton.Text = value.ToString();
  
            ConsumeCell(pressedButton);

            _expectedNext++;
            if (_expectedNext > _currentN)
                CompleteRound();
            else
                _statusLabel.Text = $"Nice. Now click {_expectedNext}.";
        }
    }
    private void ConsumeCell(Button btn)
    {
        // Make it behave like an empty/non-playable cell:
        btn.Text = "";
        btn.Disabled = true;

        // Ensure we didn't tint it earlier
        btn.Modulate = Colors.White;

        // Optional: avoid focus outline
        btn.FocusMode = FocusModeEnum.None;
    }
    // ============================================================
    // VISUAL HELPERS
    // ============================================================

    protected void ShowNumbers(bool visible)
    {
        foreach (var kv in _numberByButton)
            kv.Key.Text = visible ? kv.Value.ToString() : "";
    }

    protected void SetNumberedButtonsEnabled(bool enabled)
    {
        foreach (var btn in _numberByButton.Keys)
            btn.Disabled = !enabled;
    }

    // ============================================================
    // ROUND RESULTS
    // ============================================================

    protected void CompleteRound()
    {
        ShowNumbers(true);

        if (_currentN >= MaxN)
        {
            WinPuzzle();
            return;
        }

        StartRound(_currentN + 1);
    }

    protected void FailRound(string reason)
    {
        _state = PuzzleState.Failed;
        SetNumberedButtonsEnabled(false);

        // You can keep the old retry button, but overlay is clearer.
        _retryButton.Visible = false;

        ShowResultOverlay(
            "Failed",
            $"{reason}\n\nPress Restart to try again.",
            pendingResult: null // Restart required; Continue closes as failure
        );
    }

    // ============================================================
    // PUZZLE COMPLETION / EXIT
    // ============================================================

    protected void WinPuzzle()
    {
        _state = PuzzleState.Completed;
        SetNumberedButtonsEnabled(false);

        if (_retryButton != null)
            _retryButton.Visible = false;

        if (_closeButton != null)
        {
            _closeButton.Visible = false;
            _closeButton.Disabled = true;
        }

        ShowResultOverlay(
            "Success",
            "You managed to open the door!",
            pendingResult: true
        );

        // DO NOT emit PuzzleFinished here
    }

    protected void CancelPuzzle()
    {
        bool success = (_state == PuzzleState.Completed) || (_pendingResult == true);
        EmitSignal(SignalName.PuzzleFinished, success);
    }


    // ============================================================
    // OVERLAY HELPERS
    // ============================================================

    private void ShowResultOverlay(string title, string body, bool? pendingResult)
    {
        _pendingResult = pendingResult;

        // If overlay is missing, fall back to OLD UI — but DO NOT auto-finish
        if (_resultOverlay == null || _resultTitle == null || _resultBody == null)
        {
            _statusLabel.Text = body;

            // On failure, allow retry
            if (pendingResult != true && _retryButton != null)
                _retryButton.Visible = true;

            // Always allow close
            if (_closeButton != null)
            {
                _closeButton.Visible = true;
                _closeButton.Disabled = false;
            }

            return;
        }

        // Overlay exists → show it
        _resultTitle.Text = title;
        _resultBody.Text = body;
        _resultOverlay.Visible = true;

        if (_restartButton != null)
            _restartButton.Visible = (pendingResult != true);

        if (_continueButton != null)
        {
            _continueButton.Visible = true;
            _continueButton.Disabled = false;
        }
    }


    private void HideResultOverlay()
    {
        _pendingResult = null;
        if (_resultOverlay != null)
            _resultOverlay.Visible = false;
    }

    // ============================================================
    // UI CALLBACKS
    // ============================================================

    protected void OnRetryPressed()
    {
        StartFromBeginning();
    }

    protected void OnClosePressed()
    {
        CancelPuzzle();
    }

    // ============================================================
    // UTIL
    // ============================================================

    private static void Shuffle<T>(IList<T> list, RandomNumberGenerator rng)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = (int)rng.RandiRange(0, i);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private bool ValidateRoundMapping(int n, out string error)
    {
        if (_numberByButton.Count != n)
        {
            error = $"Mapping count mismatch: expected {n}, got {_numberByButton.Count}.";
            return false;
        }

        for (int i = 1; i <= n; i++)
        {
            if (!_numberByButton.Values.Contains(i))
            {
                error = $"Missing number {i} in mapping.";
                return false;
            }
        }

        error = "";
        return true;
    }
}
