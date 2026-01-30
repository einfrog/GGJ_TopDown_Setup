using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

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

    /// <summary>
    /// Emitted when the puzzle is finished.
    /// success = true  -> puzzle fully completed
    /// success = false -> failed or cancelled
    /// </summary>
    [Signal]
    public delegate void PuzzleFinishedEventHandler(bool success);


    // ============================================================
    // DIFFICULTY / PROGRESSION SETTINGS
    // ============================================================

    [Export] public int StartN = 4;   // First round size
    [Export] public int MaxN = 7;     // Final round size


    // ============================================================
    // GRID / VISUAL SETTINGS
    // ============================================================

    [Export] public int GridColumns = 5;   // Columns in GridContainer
    [Export] public int CellSize = 72;     // Button size (px)


    // ============================================================
    // NODE REFERENCES (ASSIGNED IN EDITOR)
    // ============================================================

    [Export] public NodePath GridPath ;
    [Export] public NodePath StatusLabelPath ;
    [Export] public NodePath RetryButtonPath ;
    [Export] public NodePath CloseButtonPath ;

    protected GridContainer _grid;
    protected Label _statusLabel;
    protected Button _retryButton;
    protected Button _closeButton;


    // ============================================================
    // INTERNAL STATE
    // ============================================================

    /// <summary>
    /// Current number of tiles in this round
    /// </summary>
    protected int _currentN;

    /// <summary>
    /// The next number the player must click
    /// </summary>
    protected int _expectedNext;

    /// <summary>
    /// All generated buttons for the current grid
    /// </summary>
    protected List<Button> _cells = new();

    /// <summary>
    /// Maps each numbered button to its assigned value
    /// </summary>
    protected Dictionary<Button, int> _numberByButton = new();


    // ============================================================
    // PUZZLE STATE MACHINE
    // ============================================================

    protected enum PuzzleState
    {
        Idle,               // Puzzle not yet started
        ShowingNumbers,     // Numbers visible for memorization
        WaitingFirstClick,  // Waiting for player to click "1"
        InSequence,         // Numbers hidden, clicking in order
        Failed,             // Wrong click
        Completed            // Puzzle fully completed
    }

    protected PuzzleState _state = PuzzleState.Idle;


    // ============================================================
    // GODOT LIFECYCLE
    // ============================================================

    public override void _Ready()
    {
        _grid = GetNode<GridContainer>(GridPath);
        _statusLabel = GetNode<Label>(StatusLabelPath);
        _retryButton = GetNode<Button>(RetryButtonPath);
        _closeButton = GetNode<Button>(CloseButtonPath);

        // 2) Configure the grid visually.
        _grid.Columns = Mathf.Max(1, GridColumns);

        // 3) Wire up UI button presses to our functions.
        _retryButton.Pressed += OnRetryPressed;
        _closeButton.Pressed += OnClosePressed;

        // Optional: hide retry until needed
        _retryButton.Visible = false;

        // 4) Start the puzzle immediately (or call StartFromBeginning() from outside instead).
        StartFromBeginning();
    }


    // ============================================================
    // PUZZLE FLOW (HIGH LEVEL)
    // ============================================================

    /// <summary>
    /// Resets the puzzle to the starting round (StartN).
    /// Used when puzzle opens or when player retries.
    /// </summary>
    public void StartFromBeginning()
    {
 
        // Set _currentN to StartN
        _currentN = Mathf.Max(2, StartN);
        // Reset expected number to 1
        _expectedNext = 1;
        // Start first round
        _retryButton.Visible = false;
        StartRound(_currentN);
    }

    /// <summary>
    /// Starts a new round with N numbers.
    /// Builds a new grid and shows numbers to the player.
    /// </summary>
    protected void StartRound(int n)
    {
        // Set current round size
        _currentN = Mathf.Clamp(n, StartN, MaxN);
        // Reset expected number
        _expectedNext = 1;
        // Clear previous grid
        ClearGrid();
        // Build new randomized grid
        BuildGrid(_currentN);
        // Build-and-validate: if something went wrong, rebuild a few times to avoid softlocks.
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
                FailRound("Internal error generating tiles. Please Retry.");
                return;
            }
        }
        // Show numbers
        ShowNumbers(true);
        // Update state and UI text
        
        SetNumberedButtonsEnabled(true);

        _state = PuzzleState.WaitingFirstClick;
        _statusLabel.Text = $"Memorize 1-{_currentN} and click 1.";
    }


    // ============================================================
    // GRID GENERATION
    // ============================================================

    /// <summary>
    /// Creates grid buttons and assigns numbers randomly.
    /// </summary>
    protected void BuildGrid(int n)
    {
        // Decide grid size (rows/columns)
        int cols = Mathf.Max(1, GridColumns);

        // int rows = Mathf.CeilToInt((float)n * n / cols);
        int rows = 5;
        int totalCells = rows * cols;


        // Create button instances

        for (int i = 0; i < totalCells; i++)
        {
            var btn = new Button
            {
                CustomMinimumSize = new Vector2(CellSize, CellSize),
                FocusMode = FocusModeEnum.None,
                Text = "",
                Disabled = true
            };
            Button captured = btn;
            btn.Pressed += () => OnCellPressed(captured);
            
            _grid.AddChild(btn);
            _cells.Add(btn);
        }

        var rng = new RandomNumberGenerator();
        rng.Randomize();
        // Assign numbers 1..n to selected buttons
        List<int> indices = Enumerable.Range(0, totalCells).ToList();
        Shuffle(indices, rng);
        for (int number = 1; number <= n; number++)
        {
            var cell = _cells[indices[number - 1]];
            _numberByButton[cell] = number;
            cell.Disabled = false;
            // cell.Text = number.ToString();

        }
        for (int i = 1; i <= n; i++)
        {
            bool exists = _numberByButton.Values.Contains(i);
            GD.Print($"Number {i} exists: {exists}");
        }
        GD.Print($"BuildGrid: n={n}, cells={_cells.Count}, mapping={_numberByButton.Count}");
        GD.Print("Numbers present: " + string.Join(",", _numberByButton.Values.OrderBy(v => v)));
    }

    /// <summary>
    /// Removes all buttons from the grid and clears data structures.
    /// </summary>
    protected void ClearGrid()
    {
        // QueueFree all buttons
        foreach (var btn in _cells)
        {  
            btn.QueueFree();
        }
        // Clear _cells list
        _cells.Clear();
        // Clear _numberByButton dictionary
        _numberByButton.Clear();
    }


    // ============================================================
    // INPUT HANDLING
    // ============================================================

    /// <summary>
    /// Called when a grid cell is pressed.
    /// Handles validation depending on current puzzle state.
    /// </summary>
    protected void OnCellPressed(Button pressedButton)
    {
        if (_state is PuzzleState.Failed or PuzzleState.Completed)
            return;
        // Determine which number (if any) this button represents   
        if(!_numberByButton.TryGetValue(pressedButton, out int value))
            return;

        if (_state == PuzzleState.WaitingFirstClick)
        {
            if (value != 1)
            {
                FailRound("Wrong start! You must click 1.");
                return;
            }

            pressedButton.Disabled = true;
            _expectedNext = 2;
            foreach (var kv in _numberByButton)
            {
                if (kv.Key != pressedButton)
                    kv.Key.Text = "";
            }

            _state = PuzzleState.InSequence;
            _statusLabel.Text = $"Good. now click {_expectedNext}";
            return;
        }

        if (_state == PuzzleState.InSequence)
        {
            if (value != _expectedNext)
            {   
                FailRound($"Wrong! Expected {_expectedNext}");
                return;
            }

            pressedButton.Disabled = true;
            pressedButton.Text = value.ToString(); // or make it disappear instead

            _expectedNext++;
            if (_expectedNext > _currentN)
            {
                CompleteRound();
            }
            else
            {
                _statusLabel.Text = $"Nice. Now click {_expectedNext}.";
            }
        }

        // Branch logic based on current PuzzleState
        // Validate correct or incorrect input
    }


    // ============================================================
    // VISUAL HELPERS
    // ============================================================

    /// <summary>
    /// Shows or hides all numbers on the grid.
    /// </summary>
    protected void ShowNumbers(bool visible)
    {
        // Loop through _numberByButton
        foreach (var kv in _numberByButton)
        {
            // Set button text to number or empty string
            kv.Key.Text = visible ? kv.Value.ToString() : "";
        }

    }

    /// <summary>
    /// Enables or disables all numbered buttons.
    /// </summary>
    protected void SetNumberedButtonsEnabled(bool enabled)
    {
        // Enable or disable buttons that have assigned numbers
        foreach (var btn in _numberByButton.Keys)
            btn.Disabled = !enabled;
    }


    // ============================================================
    // ROUND RESULTS
    // ============================================================

    /// <summary>
    /// Called when the current round is completed successfully.
    /// </summary>
    protected void CompleteRound()
    {
        ShowNumbers(true);
        // Check if this was the final round
        // If yes, complete the puzzle
        if (_currentN >= MaxN)
        {
            WinPuzzle();
            return;
        }
        // If not, start next round
        int nextN = _currentN + 1;
        StartRound(nextN);
    }

    /// <summary>
    /// Called when the player makes a mistake.
    /// </summary>
    protected void FailRound(string reason)
    {
        // Set state to Failed
        _state = PuzzleState.Failed;
        // Disable input
        SetNumberedButtonsEnabled(false);
        // Show failure message
        _statusLabel.Text = $"{reason} Retry resto to {StartN}";
        // Enable Retry option
        _retryButton.Visible = true;
    }


    // ============================================================
    // PUZZLE COMPLETION / EXIT
    // ============================================================

    /// <summary>
    /// Called when the player completes all rounds successfully.
    /// </summary>
    protected void WinPuzzle()
    {
        // Set state to Completed
        _state = PuzzleState.Completed;
        // Emit PuzzleFinished(true)
        _statusLabel.Text = "Success!";
        EmitSignal(SignalName.PuzzleFinished, true);
        // Close or free puzzle UI
        QueueFree();
    }

    /// <summary>
    /// Called when the player cancels or closes the puzzle.
    /// </summary>
    protected void CancelPuzzle()
    {
        // Emit PuzzleFinished(false)
        EmitSignal(SignalName.PuzzleFinished, false);
        // Close or free puzzle UI
        QueueFree();
    }


    // ============================================================
    // UI CALLBACKS
    // ============================================================

    protected void OnRetryPressed()
    {
        // Reset puzzle to starting state
        StartFromBeginning();
    }

    protected void OnClosePressed()
    {
        // Cancel puzzle
        CancelPuzzle();
    }
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
        // Must have exactly n numbered buttons.
        if (_numberByButton.Count != n)
        {
            error = $"Mapping count mismatch: expected {n}, got {_numberByButton.Count}.";
            return false;
        }

        // Must contain all numbers 1..n.
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
