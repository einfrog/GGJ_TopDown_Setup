using Godot;

namespace GGJ_2026.scripts;

public partial class Menu : Control
{

	[Export]
	public Button PlayButton { get; set; }

	[Export]
	public Button QuitButton { get; set; }

	[Export]
	public Button CreditsButton { get; set; }

	[Export]
	public PackedScene GameScene { get; set; }

	public override void _Ready()
	{
		PlayButton.Pressed += () => GetTree().ChangeSceneToPacked(GameScene);
		QuitButton.Pressed += () => GetTree().Quit();
		CreditsButton.Pressed += () => GetTree().ChangeSceneToFile("res://scenes/credits.tscn");
	}

}