using Godot;

namespace GGJ_2026.scripts;

public partial class Credits : Control
{

    [Export]
    public Button BackButton { get; set; }

    public override void _Ready()
    {
        BackButton.Pressed += () => GetTree().ChangeSceneToFile("res://scenes/menu.tscn");
    }

}