using Godot;

namespace GGJ_2026.scripts;

public partial class Player : CharacterBody2D
{
    private bool _inputEnabled = true;
    [Export]
    private AnimatedSprite2D Sprite { get; set; }

    [Export]
    public float MovementSpeed { get; set; } = 500;

    [Export]
    public float MaxHealth { get; set; } = 100;

    public float Health { get; set; }

    public GasMask Mask { get; set; }

    public override void _Ready()
    {
        Health = MaxHealth;
    }

    public override void _PhysicsProcess(double delta)
    {
        // If input is disabled (puzzle open), stop movement
        if (!_inputEnabled)
        {
            Velocity = Vector2.Zero;
            MoveAndSlide();
            return;
        }

        float x = Input.GetAxis("move_left", "move_right");
        float y = Input.GetAxis("move_up", "move_down");
        var direction = new Vector2(x, y).Normalized();

        if (direction.IsZeroApprox())
        {
            Sprite.Animation = "Idle";
            Velocity = Velocity.MoveToward(Vector2.Zero, MovementSpeed);
        }
        else
        {
            Sprite.Animation = "Walking";
            Velocity = direction * MovementSpeed;
        }

        Sprite.FlipH = direction.X < 0;
        MoveAndSlide();
    }

    public void Hurt(float damage)
    {
        Health -= Mask.Filter(damage);
    }

    public void SetInputEnabled(bool enabled)
    {
        _inputEnabled = enabled;
    }

}