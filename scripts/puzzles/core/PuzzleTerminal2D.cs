using System;
using Godot;

namespace GGJ_2026.scripts.puzzles.core;

public partial class PuzzleTerminal2D : Area2D, IInteractable
{

    private bool _busy;

    private bool _playerInside;

    private bool _solved;

    [Export]
    public PuzzleHost PuzzleHost;

    [Export]
    public Label HintLabel;

    [Export]
    public string HintText = "Press E";

    [Export]
    public bool OneTime = true;
    
    public event Action Solved;

    public void Interact()
    {
        if (!_playerInside) return;
        if (_busy) return;
        if (OneTime && _solved) return;

        _busy = true;
        HintLabel?.Visible = false;

        PuzzleHost.OpenChimpPuzzle(success =>
        {
            _busy = false;

            if (success)
            {
                _solved = true;
                Solved?.Invoke();
            }
            else if (_playerInside)
            {
                HintLabel?.Visible = true;
            }
        });
    }

    public override void _Ready()
    {
        HintLabel?.Visible = false;
        HintLabel?.Text = HintText;
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    private void OnBodyEntered(Node body)
    {
        if (body is not Player player)
        {
            return;
        }

        _playerInside = true;
        HintLabel?.Visible = true;
        player.Interactor.SetCurrentInteractable(this);
    }

    private void OnBodyExited(Node body)
    {
        if (body is not Player player)
        {
            return;
        }

        _playerInside = false;
        HintLabel?.Visible = false;
        player.Interactor.ClearCurrentInteractable(this);
    }

}