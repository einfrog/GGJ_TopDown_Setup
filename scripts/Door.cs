using Godot;
using GGJ_2026.scripts.puzzles.core;

namespace GGJ_2026.scripts.world;

public partial class Door : StaticBody2D
{
    [Export] public NodePath TerminalPath;

    [Export] public AnimatedSprite2D Sprite;
    [Export] public CollisionShape2D Collision;

    private PuzzleTerminal2D _terminal;
    private bool _isOpen;

    public override void _Ready()
    {
        if (Sprite == null)
            Sprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
        if (Collision == null)
            Collision = GetNodeOrNull<CollisionShape2D>("CollisionShape2D");

        if (!TerminalPath.IsEmpty)
        {
            _terminal = GetNodeOrNull<PuzzleTerminal2D>(TerminalPath);
            if (_terminal != null)
            {
                _terminal.Solved += OnTerminalSolved;
            }
            else
            {
                GD.PushWarning($"Door '{Name}': Terminal not found at {TerminalPath}");
            }
        }
    }

    public override void _ExitTree()
    {
        // IMPORTANT: unsubscribe to avoid dangling references
        if (_terminal != null)
            _terminal.Solved -= OnTerminalSolved;
    }

    private void OnTerminalSolved()
    {
        Open();
    }

    public void Open()
    {
        if (_isOpen)
            return;

        _isOpen = true;

        if (Sprite != null)
            Sprite.Play("open");

        if (Collision != null)
            Collision.Disabled = true;
    }
}