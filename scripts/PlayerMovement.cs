using Godot;

namespace GGJ_2026.scripts;

public partial class PlayerMovement : CharacterBody2D
{

    [Export]
    private AnimatedSprite2D Sprite { get; set; }

    [Export]
    public float MovementSpeed { get; set; } = 500;

    public override void _PhysicsProcess(double delta)
    {
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

}