using Godot;

namespace GGJ_2026.scripts.puzzles.core;

public partial class PuzzleTerminal2D : Area2D
{
    [Export] public NodePath PuzzleHostPath;
    [Export] public string PuzzleId = "chimp"; // e.g. "chimp" or "hanoi"
    [Export] public string HintText = "Press E";
    [Export] public bool OneTime = true;

    private PuzzleHost _host;
    private Label _hintLabel;

    private bool _playerInside;
    private bool _busy;
    private bool _solved;

    public override void _Ready()
    {
        _host = GetNodeOrNull<PuzzleHost>(PuzzleHostPath);
        if (_host == null)
            GD.PushError($"PuzzleTerminal2D: PuzzleHost not found at path '{PuzzleHostPath}'.");

        _hintLabel = GetNodeOrNull<Label>("HintLabel");
        if (_hintLabel != null)
        {
            _hintLabel.Text = HintText;
            _hintLabel.Visible = false;
        }

        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    private void OnBodyEntered(Node body)
    {
        if (!body.IsInGroup("player"))
            return;

        _playerInside = true;

        // Don't show hint if already solved in one-time mode
        if (! (OneTime && _solved))
        {
            if (_hintLabel != null) _hintLabel.Visible = true;
        }

        // Tell the player's Interactor child that this is the current interactable
        var interactor = body.GetNodeOrNull<Node>("Interactor");
        if (interactor != null && interactor.HasMethod("SetCurrentInteractable"))
            interactor.Call("SetCurrentInteractable", this);
    }

    private void OnBodyExited(Node body)
    {
        if (!body.IsInGroup("player"))
            return;

        _playerInside = false;
        if (_hintLabel != null) _hintLabel.Visible = false;

        // IMPORTANT: clear via Interactor (same pattern as OnBodyEntered)
        var interactor = body.GetNodeOrNull<Node>("Interactor");
        if (interactor != null && interactor.HasMethod("ClearCurrentInteractable"))
            interactor.Call("ClearCurrentInteractable", this);
    }

    public void Interact()
    {
        if (!_playerInside) return;
        if (_busy) return;
        if (OneTime && _solved) return;
        if (_host == null) return;

        _busy = true;
        if (_hintLabel != null) _hintLabel.Visible = false;

        // Open puzzle by ID using the new host
        bool opened = _host.OpenPuzzle(PuzzleId, success =>
        {
            _busy = false;

            if (success)
            {
                _solved = true;
                SetSolvedState();
                EmitSignal(SignalName.Solved);
            }
            else
            {
                // If player still inside and not solved, show hint again
                if (_playerInside && !(OneTime && _solved) && _hintLabel != null)
                    _hintLabel.Visible = true;
            }
        });

        if (!opened)
        {
            _busy = false;
            // If it failed to open, restore hint if player is still inside
            if (_playerInside && _hintLabel != null) _hintLabel.Visible = true;
        }
    }

    private void SetSolvedState()
    {
        // Hide hint permanently and stop reacting to bodies if one-time
        if (_hintLabel != null)
            _hintLabel.Visible = false;

        if (OneTime)
        {
            Monitoring = false;
            Monitorable = false;
        }
    }

    [Signal]
    public delegate void SolvedEventHandler();
}
