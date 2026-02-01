using Godot;

namespace GGJ_2026.scripts;

public partial class HealthBar : Control
{

    [Export]
    public TextureProgressBar Bar;

    public override void _Ready()
    {
        if (Player.Instance == null)
            return;

        Bar.MaxValue = Player.Instance.MaxHealth;
        Bar.Value = Player.Instance.Health;
        Player.Instance.HealthChanged += UpdateHealth;
    }

    public override void _ExitTree()
    {
        if (Player.Instance != null)
            Player.Instance.HealthChanged -= UpdateHealth;
    }

    public void UpdateHealth(float currentHealth)
    {
        Bar.Value = currentHealth;
    }

}