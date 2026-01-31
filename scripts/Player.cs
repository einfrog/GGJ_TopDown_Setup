using System;
using Godot;

namespace GGJ_2026.scripts;

public partial class Player : CharacterBody2D
{

    private Vector2 _previousPosition;

    [Export]
    public AnimatedSprite2D Sprite { get; set; }

    [Export]
    public PlayerInteractor Interactor { get; set; }

    [Export]
    public float MovementSpeed { get; set; } = 500;

    [Export]
    public float MaxHealth { get; set; } = 100;

    public float Health { get; set; }

    public bool InputEnabled { get; set; } = true;

    public GasMask Mask { get; set; }

    public static Player Instance { get; private set; }

    public event Action Died;
    
    public event Action<float> HealthChanged;

    public override void _EnterTree()
    {
        if (Instance is not null)
        {
            GD.PushError("There can only be one player");
            return;
        }

        Instance = this;
    }

    public override void _Ready()
    {
        _previousPosition = Position;
        Health = MaxHealth;
    }

    public override void _PhysicsProcess(double delta)
    {
        // If input is disabled (puzzle open), stop movement
        if (!InputEnabled)
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
            Velocity = Vector2.Zero;
            Sprite.Animation = "Maks_Idle";
        }
        else
        {
            Velocity = direction * MovementSpeed;

            Sprite.Animation = direction.Y switch
            {
                < 0 => "Maks_Backward",
                > 0 => "Maks_Forward",
                _ => "Maks_Walk_Left_Right"
            };
        }

        Sprite.FlipH = direction.X > 0;
        MoveAndSlide();
    }

    public void Hurt(float damage)
    {
        if (Mask is not null)
        {
            damage = Mask.Filter(damage);
        }

        Health -= damage;
        Health = Mathf.Max(Health, 0);

        HealthChanged?.Invoke(Health);

        if (Health <= 0)
        {
            Died?.Invoke();
        }

    }
    

}