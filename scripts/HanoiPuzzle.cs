using System;
using System.Collections.Generic;
using Godot;

namespace GGJ_2026.scripts;

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
	[Export] public NodePath UndoButtonPath;

	[Export] public NodePath PegAPath;
	[Export] public NodePath PegBPath;
	[Export] public NodePath PegCPath;

	[Export] public NodePath StackAPath;
	[Export] public NodePath StackBPath;
	[Export] public NodePath StackCPath;

	[Export] public NodePath ResultOverlayPath;
	[Export] public NodePath ResultTitlePath;
	[Export] public NodePath ResultBodyPath;
	[Export] public NodePath RestartButtonPath;
	[Export] public NodePath ContinueButtonPath;

	private Label _status;
	private Button _resetButton;
	private Button _closeButton;
	private Button _undoButton;

	private Control _pegA;
	private Control _pegB;
	private Control _pegC;

	private Control _stackA;
	private Control _stackB;
	private Control _stackC;
	
	private ColorRect _resultOverlay;
	private Label _resultTitle;
	private Label _resultBody;
	private Button _restartButton;
	private Button _continueButton;

	private enum PuzzleState {Idle, Playing, Completed, Failed}

	private PuzzleState _state = PuzzleState.Idle;

	private int _moveCount;
	private int _minMoves;

	private readonly Stack<int>[] _pegs =
	{
		new Stack<int>(), new Stack<int>(), new Stack<int>()
	};
	private struct MoveRecord
	{
		public int From;
		public int To;
		public int Disk;
	}

	private readonly Stack<MoveRecord> _history = new();
	private readonly Dictionary<int, Control> _diskNodeBySize = new();

	private bool _hasPickedDisk;
	private int _pickedDiskSize;
	private int _pickedFromPegIndex;

	public override void _Ready()
	{
		GD.Print("HanoiPuzzle _Ready running: ", SceneFilePath, " node=", Name);

		GD.Print("Calling CacheNodes...");
		CacheNodes();
		GD.Print("CacheNodes finished.");

		GD.Print("Calling WireUi...");
		WireUi();
		GD.Print("WireUi finished.");

		StartPuzzle();
	}

	private void CacheNodes()
	{
		GD.Print("CacheNodes START");

		try
		{
			// Helper to avoid passing empty NodePaths (common silent killer)
			T GetSafe<T>(NodePath path, string name) where T : Node
			{
				if (path.IsEmpty)
				{
					GD.PushWarning($"CacheNodes: '{name}' NodePath is empty (not set in inspector).");
					return null;
				}

				var node = GetNodeOrNull<T>(path);
				if (node == null)
					GD.PushWarning($"CacheNodes: '{name}' not found at path: {path}");

				return node;
			}

			_status = GetSafe<Label>(StatusLabelPath, nameof(StatusLabelPath));
			_resetButton = GetSafe<Button>(ResetButtonPath, nameof(ResetButtonPath));
			_closeButton = GetSafe<Button>(CloseButtonPath, nameof(CloseButtonPath));
			_undoButton = GetSafe<Button>(UndoButtonPath, nameof(UndoButtonPath));

			_pegA = GetSafe<Control>(PegAPath, nameof(PegAPath));
			_pegB = GetSafe<Control>(PegBPath, nameof(PegBPath));
			_pegC = GetSafe<Control>(PegCPath, nameof(PegCPath));

			_stackA = GetSafe<Control>(StackAPath, nameof(StackAPath));
			_stackB = GetSafe<Control>(StackBPath, nameof(StackBPath));
			_stackC = GetSafe<Control>(StackCPath, nameof(StackCPath));

			_resultOverlay = GetSafe<ColorRect>(ResultOverlayPath, nameof(ResultOverlayPath));
			_resultTitle = GetSafe<Label>(ResultTitlePath, nameof(ResultTitlePath));
			_resultBody = GetSafe<Label>(ResultBodyPath, nameof(ResultBodyPath));
			_restartButton = GetSafe<Button>(RestartButtonPath, nameof(RestartButtonPath));
			_continueButton = GetSafe<Button>(ContinueButtonPath, nameof(ContinueButtonPath));

			GD.Print("CacheNodes END (no exception)");
		}
		catch (Exception ex)
		{
			GD.PushError("CacheNodes EXCEPTION: " + ex);
			throw; // rethrow so you still see the stack trace
		}
	}


	private void WireUi()
	{
		_resetButton.Pressed += ResetPuzzle;
		_closeButton.Pressed += CancelPuzzle;
		_undoButton.Pressed += UndoLastMove;

		_pegA.GuiInput += (e) => OnPegGuiInput(0, e);
		_pegB.GuiInput += (e) => OnPegGuiInput(1, e);
		_pegC.GuiInput += (e) => OnPegGuiInput(2, e);
		
		if (_restartButton != null)
		{
			_restartButton.Pressed += () =>
			{
				HideResultOverlay();
				StartPuzzle();
			};
		}
		else
		{
			GD.PushWarning("HanoiPuzzle: RestartButtonPath not set or node not found.");
		}
		if (_continueButton != null)
		{
			_continueButton.Pressed += () =>
			{
				CancelPuzzle();
			};
		}
		else
		{
			GD.PushWarning("HanoiPuzzle: ContinueButtonPath not set or node not found.");
		}

	}

	private void ShowResultOverlay(string title, string body)
	{
		_resultTitle.Text = title;
		_resultBody.Text = body;
		_resultOverlay.Visible = true;
		_state = title == "Victory" ? PuzzleState.Completed : PuzzleState.Failed;
		
		UpdateUndoButton();
	}
	private void HideResultOverlay()
	{
		_resultOverlay.Visible = false;

		// Back to playing state after restart
		_state = PuzzleState.Playing;
		UpdateUndoButton();
	}

	public void StartPuzzle()
	{
		_resultOverlay.Visible = false;
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
		if (_state != PuzzleState.Playing)
			return;

		// ONLY react to left mouse button PRESS
		if (e is not InputEventMouseButton mb)
			return;

		if (mb.ButtonIndex != MouseButton.Left)
			return;

		if (!mb.Pressed)
			return;

		GD.Print($"Peg {pegIndex} CLICKED");
		if (!_hasPickedDisk)
		{
			if (_pegs[pegIndex].Count == 0)
			{
				ShowInfo("That peg is empty.");
				return;
			}

			int topDisk = _pegs[pegIndex].Peek();

			_hasPickedDisk = true;
			_pickedDiskSize = topDisk;
			_pickedFromPegIndex = pegIndex;
			
			SetDiskPickedVisual(_pickedDiskSize, true);
			ShowInfo($"Picked disk {_pickedDiskSize}. Choose a peg go place it.");
			return;
		}

		if (pegIndex == _pickedFromPegIndex)
		{
			SetDiskPickedVisual(_pickedDiskSize, false);
			_hasPickedDisk = false;
			_pickedDiskSize = 0;
			_pickedFromPegIndex = -1;
			
			UpdateStatus();
			return;
		}
		bool moved = TryMove(_pickedFromPegIndex, pegIndex);
		
		SetDiskPickedVisual(_pickedDiskSize, false);
		_hasPickedDisk = false;
		_pickedDiskSize = 0;
		_pickedFromPegIndex = -1;

		if (!moved)
		{
			return;
		}
		LayoutPeg(0);
		LayoutPeg(1);
		LayoutPeg(2);
		// if (IsSolved())
		// {
		// 	CompletePuzzle();
		// 	return;
		// }
		LayoutAllPegs();
		UpdateStatus();
		CheckEndConditionAfterMove();
		UpdateStatus();
	}

	private void BuildInitialState()
	{
		for (int size = DiskCount; size >= 1; size--)
		{
			_pegs[0].Push(size);
		}
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
		
		if(fromPeg < 0 || fromPeg > 2 || toPeg < 0 || toPeg >2)
			return false;
		if (_pegs[fromPeg].Count == 0)
		{
			ShowInfo("No disk to move.");
			return false;
		}

		int movingDisk = _pegs[fromPeg].Peek();

		if (_pegs[toPeg].Count > 0)
		{
			int destTop = _pegs[toPeg].Peek();
			if (destTop < movingDisk)
			{
				ShowInfo($"Illegal move: disk {movingDisk} can't go on disk {destTop}.");
				return false;
			}
		}

		_pegs[fromPeg].Pop();
		_pegs[toPeg].Push(movingDisk);
		_moveCount++;
		_history.Push(new MoveRecord{From = fromPeg, To = toPeg, Disk = movingDisk});
		if(_diskNodeBySize.TryGetValue(movingDisk, out var diskNode))
		{
			var newRoot = GetStackRoot(toPeg);
			if (diskNode.GetParent() != newRoot)
			{
				diskNode.GetParent()?.RemoveChild(diskNode);
				newRoot.AddChild(diskNode);
			}
		}
		ShowInfo($"Moved disk {movingDisk} to peg {PegName(toPeg)}.");
		UpdateUndoButton();
		return true;
	}

	private string PegName(int pegIndex) => pegIndex switch
	{
		0 => "A",
		1 => "B",
		2 => "C",
		_ => "?"
	};
	private bool IsSolved()
	{
		if (_pegs[2].Count != DiskCount)
			return false;

		// Optional extra validation: peg C must be correctly ordered (top smallest)
		// Stack enumerates top->bottom; we want strictly increasing sizes as we go down.
		int prev = 0;
		foreach (int size in _pegs[2]) // top -> bottom
		{
			if (size <= prev) return false;
			prev = size;
		}

		// "Perfect-only" rule:
		if (_moveCount == _minMoves)
			return true;

		// Not perfect: give feedback but do not complete
		ShowInfo($"All disks reached C, but moves = {_moveCount}. Minimum is {_minMoves}. Undo/Reset to try again.");
		return false;
	}

	private int CalculateMinMoves(int n)
	{
		return (1 << n) - 1;
	}

	private void BuildVisuals()
	{
		if (DiskScene == null)
		{
			GD.PushError("HanoiPuzzle: DiskScene is not assigned. Drag DiskUI.tscn into DiskScene export.");
			return;
		}

		for (int size = 1; size <= DiskCount; size++)
		{
			var disk = DiskScene.Instantiate<Control>();

			// Set disk size label/value BEFORE adding to the tree
			if (disk is DiskUi diskUi)
				diskUi.SizeValue = size;

			// Size it
			int width = DiskMinWidth + (size - 1) * DiskWidthStep;
			disk.CustomMinimumSize = new Vector2(width, DiskHeight);
			disk.Size = disk.CustomMinimumSize;

			// Make it visible (style)
			if (disk is Panel panel)
			{
				var style = new StyleBoxFlat();
				style.BgColor = new Color(0.8f, 0.8f, 0.8f, 1f);
				style.CornerRadiusTopLeft = 6;
				style.CornerRadiusTopRight = 6;
				style.CornerRadiusBottomLeft = 6;
				style.CornerRadiusBottomRight = 6;
				panel.AddThemeStyleboxOverride("panel", style);
			}

			_stackA.AddChild(disk);
			_diskNodeBySize[size] = disk;
		}
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
		var stackRoot = GetStackRoot(pegIndex);

		var sizes = _pegs[pegIndex].ToArray();
		Array.Reverse(sizes);

		float bottomY = stackRoot.Size.Y;

		for (int i = 0; i < sizes.Length; i++)
		{
			int size = sizes[i];
			if(!_diskNodeBySize.TryGetValue(size, out var diskNode))
				continue;
			if (diskNode.GetParent() != stackRoot)
			{
				diskNode.GetParent()?.RemoveChild(diskNode);
				stackRoot.AddChild(diskNode);
			}

			float x = (stackRoot.Size.X - diskNode.Size.X) * 0.5f;

			float y = bottomY - (i + 1) * DiskHeight - i * DiskGap;

			diskNode.Position = new Vector2(x, y);
		}
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
		_history.Clear();
		foreach (var kv in _diskNodeBySize)
			kv.Value.QueueFree();
		_diskNodeBySize.Clear();
	}

	private void UndoLastMove()
	{
		if (_state != PuzzleState.Playing)
			return;
		if (_history.Count == 0)
		{
			ShowInfo("Nothing to undo.");
			return;
		}

		if (_hasPickedDisk)
		{
			SetDiskPickedVisual(_pickedDiskSize, false);
			_hasPickedDisk = false;
			_pickedDiskSize = 0;
			_pickedFromPegIndex = -1;
		}

		var last = _history.Pop();

		if (_pegs[last.To].Count == 0 || _pegs[last.To].Peek() != last.Disk)
		{
			ShowInfo("Undo failed (state mismatch). Reset recommended.");
			_history.Clear();
			UpdateUndoButton();
			return;
		}

		_pegs[last.To].Pop();
		_pegs[last.From].Push(last.Disk);

		_moveCount = Mathf.Max(0, _moveCount - 1);

		if (_diskNodeBySize.TryGetValue(last.Disk, out var diskNode))
		{
			var root = GetStackRoot(last.From);
			if (diskNode.GetParent() != root)
			{
				diskNode.GetParent()?.RemoveChild(diskNode);
				root.AddChild(diskNode);
			}
		}
		LayoutAllPegs();
		ShowInfo($"Undid move: disk {last.Disk} back to {PegName(last.From)}.");
		UpdateUndoButton();
	}
	private void UpdateUndoButton()
	{
		if (_undoButton != null)
		{
			_undoButton.Disabled = _history.Count == 0 || _state != PuzzleState.Playing;
		}
	}
	private void UpdateStatus()
	{
		string extra = _hasPickedDisk
			? $"Picked: {_pickedDiskSize} (from {PegName(_pickedFromPegIndex)})"
			: "Click a peg to pick the top disk.";

		_status.Text = $"Moves: {_moveCount} / Target: {_minMoves}\n{extra}";
		UpdateUndoButton();
	}

	private void SetDiskPickedVisual(int diskSize, bool picked)
	{
		if (_diskNodeBySize.TryGetValue(diskSize, out var node) && node is CanvasItem ci)
		{
			ci.Modulate = picked ? new Color(1f, 1f, 0.75f, 1f) : new Color(1f, 1f, 1f, 1f);
		}
	}

	private void ShowInfo(string message)
	{
		_status.Text = $"Moves: {_moveCount} / Target: {_minMoves}\n{message}";
	}

	private void CheckEndConditionAfterMove()
	{
		if(_pegs[2].Count != DiskCount)
			return;
		if (_moveCount == _minMoves)
		{
			ShowResultOverlay(
				"Victory",
				$"Perfect! You solved it in {_moveCount} moves (minimum for {DiskCount} disks." )
				;
			EmitSignal(SignalName.PuzzleFinished, true);
		}
		else
		{
			ShowResultOverlay(
				"Game Over",
				$"You solved it in {_moveCount}, but the minimum is {_minMoves}. \n"+
				"Use Undo to optimize, or press Restart to try again.");
		}
		
	}
}

