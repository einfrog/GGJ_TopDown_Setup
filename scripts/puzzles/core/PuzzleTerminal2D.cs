using System;
using Godot;

namespace GGJ_2026.scripts.puzzles.core;

public partial class PuzzleTerminal2D : Area2D, IInteractable
{
    [Export] public NodePath PuzzleHostPath;
    [Export] public string PuzzleId = "chimp";
    [Export] public string HintText = "Press E";
    [Export] public bool OneTime = true;

    // Visuals
    [Export] public Texture2D TerminalTexture;
    [Export] public Texture2D SolvedTexture;

    private Sprite2D _sprite;

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
        if (_hintLabel != null)
        {
            _hintLabel.Text = HintText;
            _hintLabel.Visible = false;
        }

        _sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
        ApplyVisual(false);

        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    private void ApplyVisual(bool solved)
    {
        if (_sprite == null) return;

        if (solved && SolvedTexture != null)
            _sprite.Texture = SolvedTexture;
        else if (!solved && TerminalTexture != null)
            _sprite.Texture = TerminalTexture;
    }

    private void OnBodyEntered(Node body)
    {
        if (body is not Player player) return;

        _playerInside = true;
        if (!(OneTime && _solved))
            _hintLabel?.Show();

        player.Interactor.SetCurrentInteractable(this);
    }

    private void OnBodyExited(Node body)
    {
        if (body is not Player player) return;

        _playerInside = false;
        _hintLabel?.Hide();
        player.Interactor.ClearCurrentInteractable(this);
    }

    public void Interact()
    {
        if (!_playerInside) return;
        if (_busy) return;
        if (OneTime && _solved) return;
        if (_host == null) return;

        _busy = true;
        _hintLabel?.Hide();

        bool opened = _host.OpenPuzzle(PuzzleId, success =>
        {
            _busy = false;

            if (success)
                SetSolvedState();
            else
            {
                if (_playerInside && !(OneTime && _solved))
                    _hintLabel?.Show();
            }
        });

        if (!opened)
        {
            _busy = false;
            if (_playerInside && !(OneTime && _solved))
                _hintLabel?.Show();
        }
    }

    private void SetSolvedState()
    {
        _solved = true;
        _hintLabel?.Hide();
        ApplyVisual(true);

        Solved?.Invoke();

        if (OneTime)
        {
            Monitoring = false;
            Monitorable = false;
        }
    }
}
