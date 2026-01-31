using Godot;

namespace GGJ_2026.scripts.puzzles.core;

public partial class PuzzleTerminal2D : Area2D
{
	// Called when the node enters the scene tree for the first time.
	[Export] public NodePath PuzzleHostPath;
	[Export] public string HintText = "Press E";

	[Export] public bool OneTime = true;

	private PuzzleHost _host;
	private Label _hintLabel;

	private bool _playerInside;
	private bool _busy;
	private bool _solved;

	public override void _Ready()
	{
		_host = GetNode<PuzzleHost>(PuzzleHostPath);
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
		if (_hintLabel != null) _hintLabel.Visible = true;

		var interactor = body.GetNodeOrNull<Node>("Interactor");
		if (interactor != null && interactor.HasMethod("SetCurrentInteractable"))
		{
			interactor.Call("SetCurrentInteractable", this);
		}
		GD.Print(body.Name, " is player: ", body.IsInGroup("player"));

	}

	private void OnBodyExited(Node body)
	{
		if (!body.IsInGroup("player"))
			return;

		_playerInside = false;
		if (_hintLabel != null) _hintLabel.Visible = false;

		if (body.HasMethod("ClearCurrentInteractable"))
			body.Call("ClearCurrentInteractable", this);
	}
	public void Interact()
	{
		if (!_playerInside) return;
		if (_busy) return;
		if (OneTime && _solved) return;
		GD.Print("interacted with the terminal");
		_busy = true;
		if (_hintLabel != null) _hintLabel.Visible = false;
		_host.OpenChimpPuzzle(success =>
		{
			_busy = false;
			if (success)
			{
				_solved = true;
				GD.Print("Puzzle solved!");
				EmitSignal(SignalName.Solved);
			}
			else
			{
				if (_playerInside && _hintLabel != null)
					_hintLabel.Visible = true;
			}
		});
	}
	[Signal]
	public delegate void SolvedEventHandler();

}