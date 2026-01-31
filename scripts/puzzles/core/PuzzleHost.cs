using System;
using Godot;

namespace GGJ_2026.scripts.puzzles.core;

public partial class PuzzleHost : Control
{

    private Node _currentPuzzle;

    private Node _player;

    // Called when the node enters the scene tree for the first time.
    [Export]
    public PackedScene ChimpPuzzleScene;

    [Export]
    public NodePath PlayerPath;

    public void OpenChimpPuzzle(Action<bool> onFinished)
    {
        if (_currentPuzzle != null)
        {
            return;
        }

        Player.Instance.InputEnabled = false;
        var puzzle = ChimpPuzzleScene.Instantiate();
        _currentPuzzle = puzzle;
        AddChild(puzzle);

        puzzle.Connect("PuzzleFinished", Callable.From<bool>(success =>
        {
            Player.Instance.InputEnabled = true;
            _currentPuzzle?.QueueFree();
            _currentPuzzle = null;
            onFinished?.Invoke(success);
        }));

        GD.Print("Puzzle scene is null? ", ChimpPuzzleScene == null);
    }

}