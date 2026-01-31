using System;
using Godot;

namespace GGJ_2026.scripts.puzzles.core;

public partial class PuzzleTerminal2D : Area2D, IInteractable
{

    [Export]
    public NodePath PuzzleHostPath;

    [Export]
    public string PuzzleId = "chimp"; // e.g. "chimp" or "hanoi"

    [Export]
    public string HintText = "Press E";

    [Export]
    public bool OneTime = true;

    private PuzzleHost _host;
    private Label _hintLabel;

    private bool _playerInside;
    private bool _busy;
    private bool _solved;

    public event Action Solved;

    public override void _Ready()
    {
        _host = GetNodeOrNull<PuzzleHost>(PuzzleHostPath);

        if (_host == null)
            GD.PushError($"PuzzleTerminal2D: PuzzleHost not found at path '{PuzzleHostPath}'.");

        _hintLabel = GetNodeOrNull<Label>("HintLabel");
        _hintLabel?.Text = HintText;
        _hintLabel?.Visible = false;

        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    private void OnBodyEntered(Node body)
    {
        if (body is not Player player)
            return;

        _playerInside = true;

        // Don't show hint if already solved in one-time mode
        if (!(OneTime && _solved))
        {
            _hintLabel?.Visible = true;
        }

        player.Interactor.SetCurrentInteractable(this);
    }

    private void OnBodyExited(Node body)
    {
        if (body is not Player player)
            return;

        _playerInside = false;
        _hintLabel?.Visible = false;

        // IMPORTANT: clear via Interactor (same pattern as OnBodyEntered)
        player.Interactor.ClearCurrentInteractable(this);
    }

    public void Interact()
    {
        if (!_playerInside) return;
        if (_busy) return;
        if (OneTime && _solved) return;
        if (_host == null) return;

        _busy = true;
        _hintLabel?.Visible = false;

        // Open puzzle by ID using the new host
        bool opened = _host.OpenPuzzle(PuzzleId, success =>
        {
            _busy = false;

            if (success)
            {
                SetSolvedState();
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
        _hintLabel?.Visible = false;
        _solved = true;
        Solved?.Invoke();

        if (OneTime)
        {
            Monitoring = false;
            Monitorable = false;
        }
    }

}