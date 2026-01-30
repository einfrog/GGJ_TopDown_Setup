using Godot;
using System;
using GGJ_2026.scripts;

public partial class PuzzleHost : Control
{
	// Called when the node enters the scene tree for the first time.
	[Export] public PackedScene ChimpPuzzleScene;

	[Export] public NodePath PlayerPath;

	private Node _player;
	private Node _currentPuzzle;

	public override void _Ready()
	{
		_player = PlayerPath != null && !PlayerPath.IsEmpty ? GetNode(PlayerPath) : null;
		Hide();
	}

	public void OpenChimpPuzzle(Action<bool> onFinished)
	{
		if (_currentPuzzle != null) return;
		var puzzle = ChimpPuzzleScene.Instantiate();
		_currentPuzzle = puzzle;
		AddChild(puzzle);

		// SetPlayerEnabled(false);
		puzzle.Connect("PuzzleFinished", Callable.From<bool>((success) =>
		{
			_currentPuzzle?.QueueFree();
			_currentPuzzle = null;

			// SetPlayerEnabled(true);

			onFinished?.Invoke(success);
		}));
		GD.Print("Puzzle scene is null? ", ChimpPuzzleScene == null);
	}

	private void SetPlayerEnabled(bool enabled)
	{
		if (_player == null) return;

		if (_player.HasMethod("SetInputEnabled"))
			_player.Call("SetInputEnabled, enabled");
	}
}
// Called every frame. 'delta' is the elapsed time since the previous frame.

