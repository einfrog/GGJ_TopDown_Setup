using Godot;

namespace GGJ_2026.scripts;

public partial class DiskUi : Panel
{

    [Export]
    public int SizeValue = 1;

    public override void _Ready()
    {
        var label = GetNodeOrNull<Label>("Label");
        label?.Text = SizeValue.ToString();
    }


}
