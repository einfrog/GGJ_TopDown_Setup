using Godot;

namespace GGJ_2026.scripts;

public partial class Menu : Control
{

	[Export]
	private AnimatedSprite2D _aliveBackgroundSprite;

	[Export]
	private AnimatedSprite2D _deadBackgroundSprite;

	[Export]
	private AnimatedSprite2D _aliveMaksSprite;
	
	[Export]
	private AnimatedSprite2D _deadMaksSprite;

	[Export] private AnimatedSprite2D _gameWon;

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
		
		PlayButton.Pressed += () =>
		{
			Player.RunEndedInDeath = false;
			GetTree().ChangeSceneToPacked(GameScene);
		};
		QuitButton.Pressed += () => GetTree().Quit();
		CreditsButton.Pressed += () => GetTree().ChangeSceneToFile("res://scenes/credits.tscn");

		var player = Player.Instance;
		if (player != null)
		{
			player.Died += () => GetTree().ChangeSceneToFile("res://scenes/menu.tscn");
		}

		bool alive = !Player.RunEndedInDeath;

		var titleShade = GetNode<Label>("Title/Shade");
		var titleLight = GetNode<Label>("Title/White");

		if (!alive)
		{
			titleShade.Text = "'You died.'";
			titleLight.Text = "'You died.'";
		}
		else if (PlayerInventory.GameWon)
		{
			titleShade.Text = "'You won.'";
			titleLight.Text = "'You won.'";
		}
		else
		{
			titleShade.Text = "'Just breathe'";
			titleLight.Text = "'Just breathe'";
		}

		if (!PlayerInventory.GameWon)
		{
			_aliveMaksSprite.Visible = alive;
			_aliveBackgroundSprite.Visible = alive;
			_deadMaksSprite.Visible = !alive;
			_deadBackgroundSprite.Visible = !alive;
			_gameWon.Visible = false;
		} else 
		{
			_aliveBackgroundSprite.Visible = alive;
			_gameWon.Visible = true;
		}
	}

}