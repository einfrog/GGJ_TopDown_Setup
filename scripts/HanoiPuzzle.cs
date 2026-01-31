using Godot;
using System;
using System.Collections.Generic;
using System.Data;

public partial class HanoiPuzzle : Control
{

	[Signal]
	public delegate void PuzzleFinishedEventHandler(bool success);
	// Called when the node enters the scene tree for the first time.
	[Export] public int DiskCount = 5;
	[Export] public int MinDiskCount = 5;
	[Export] public int MaxDiskCount = 8;

	[Export] public int DiskHeight = 22;
	[Export] public int DiskGap = 4;
	[Export] public int DiskMinWidth = 70;
	[Export] public int DiskWidthStep = 25;

	[Export] public PackedScene DiskScene;

	[Export] public NodePath StatusLabelPath;
	[Export] public NodePath ResetButtonPath;
	[Export] public NodePath CloseButtonPath;

	[Export] public NodePath PegAPath;
	[Export] public NodePath PegBPath;
	[Export] public NodePath PegCPath;

	[Export] public NodePath StackAPath;
	[Export] public NodePath StackBPath;
	[Export] public NodePath StackCPath;

	private Label _status;
	private Button _resetButton;
	private Button _closeButton;

	private Control _pegA;
	private Control _pegB;
	private Control _pegC;

	private Control _stackA;
	private Control _stackB;
	private Control _stackC;
	
	private enum PuzzleState {Idle, Playing, Completed, Failed}

	private PuzzleState _state = PuzzleState.Idle;

	private int _moveCount;
	private int _minMoves;

	private readonly Stack<int>[] _pegs =
	{
		new Stack<int>(), new Stack<int>(), new Stack<int>()
	};

	private readonly Dictionary<int, Control> _diskNodeBySize = new();

	private bool _hasPickedDisk;
	private int _pickedDiskSize;
	private int _pickedFromPegIndex;

	public override void _Ready()
	{
		CacheNode();
		WireUi();
		StartPuzzle();
	}

	private void CacheNodes()
	{
		_status = GetNode<Label>(StatusLabelPath);
		_resetButton = GetNode<Button>(ResetButtonPath);
		_closeButton = GetNode<Button>(CloseButtonPath);

		_pegA = GetNode<Control>(PegAPath);
		_pegB = GetNode<Control>(PegBPath);
		_pegC = GetNode<Control>(PegCPath);

		_stackA = GetNode<Control>(StackAPath);
		_stackB = GetNode<Control>(StackBPath);
		_stackC = GetNode<Control>(StackCPath);
	}

	private void CacheNode()
	{
		_status = GetNode<Label>(StatusLabelPath);
		_resetButton = GetNode<Button>(ResetButtonPath);
		_closeButton = GetNode<Button>(CloseButtonPath);

		_pegA = GetNode<Control>(PegAPath);
		_pegB = GetNode<Control>(PegBPath);
		_pegC = GetNode<Control>(PegCPath);

		_stackA = GetNode<Control>(StackAPath);
		_stackB = GetNode<Control>(StackBPath);
		_stackC = GetNode<Control>(StackCPath);
	}

	private void WireUi()
	{
		_resetButton.Pressed += ResetPuzzle;
		_closeButton.Pressed += CancelPuzzle;

		_pegA.GuiInput += (e) => OnPegGuiInput(0, e);
		_pegB.GuiInput += (e) => OnPegGuiInput(1, e);
		_pegC.GuiInput += (e) => OnPegGuiInput(2, e);
	}

	public void StartPuzzle()
	{
		DiskCount = Mathf.Clamp(DiskCount, MinDiskCount, MaxDiskCount);
		_state = PuzzleState.Playing;
		_moveCount = 0;
		_minMoves = CalculateMinMoves(DiskCount);

		_hasPickedDisk = false;
		_pickedDiskSize = 0;
		_pickedFromPegIndex = -1;

		ClearAll();

		BuildInitialState();
		BuildVisuals();
		LayoutAllPegs();
		UpdateStatus();
	}

	public void ResetPuzzle()
	{
		StartPuzzle();
	}

	private void CancelPuzzle()
	{
		EmitSignal(SignalName.PuzzleFinished, false);
		QueueFree();
	}

	private void CompletePuzzle()
	{
		_state = PuzzleState.Completed;
		EmitSignal(SignalName.PuzzleFinished, true);
		QueueFree();
	}

	private void OnPegGuiInput(int pegIndex, InputEvent e)
	{
		
	}

	private void BuildInitialState()
	{
		
	}

	private bool TryMove(int fromPeg, int toPeg)
	{
		// Validate rules:
		// - from peg not empty
		// - moving top disk only
		// - destination empty OR top disk larger
		// If valid:
		// - pop from stack, push to target
		// - increment move count
		// - return true
		// else return false
		return false;
	}

	private bool IsSolved()
	{
		// Solved means all disks moved to peg C (index 2)
		// AND moveCount == minMoves (your win condition)
		return false;
	}

	private int CalculateMinMoves(int n)
	{
		return (1 << n) - 1;
	}

	private void BuildVisuals()
	{
			// Instantiate DiskScene for each size 1..DiskCount
			// Set width based on size, set height, store in _diskNodeBySize
			// Parent all initially under stack A
	}

	private void LayoutAllPegs()
	{
		LayoutPeg(0);
		LayoutPeg(1);
		LayoutPeg(2);
	}

	private void LayoutPeg(int pegIndex)
	{
		// Position disk nodes inside the peg's StackRoot so they stack from bottom
		// Use DiskHeight and DiskGap, center horizontally
	}

	private Control GetStackRoot(int pegIndex)
	{
		return pegIndex switch
		{
			0 => _stackA,
			1 => _stackB,
			2 => _stackC,
			_ => throw new ArgumentOutOfRangeException(nameof(pegIndex))
		};
	}

	private void ClearAll()
	{
		_pegs[0].Clear();
		_pegs[1].Clear();
		_pegs[2].Clear();

		foreach (var kv in _diskNodeBySize)
			kv.Value.QueueFree();
		_diskNodeBySize.Clear();
	}

	private void UpdateStatus()
	{
		_status.Text = $"Movex: {_moveCount} / Target: {_minMoves}";
	}
}

