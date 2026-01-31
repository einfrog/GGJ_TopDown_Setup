using Godot;

namespace GGJ_2026.scripts;

public partial class DiskUi : Panel
{
	[Export] public int SizeValue = 1;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var label = GetNodeOrNull<Label>("Label");
		if (label != null)
			label.Text = SizeValue.ToString();

	}


}
