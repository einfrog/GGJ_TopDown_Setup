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

    [Export] public NodePath BreathingPlayerPath = "Breathing";
    [Export] public AudioStream MaskBreathingLoop;
    [Export] public float BreathingFadeSeconds = 0.15f;
    
    private AudioStreamPlayer2D _breathing;
    private Tween _breathingTween;
    private bool _wasMasked;
    public float Health { get; set; }

    public bool InputDisabled { get; set; }

    public GasMaskResource MaskResource { get; set; }

    private bool Masked => MaskResource != null;
    
    public PlayerInventory Inventory { get; set; }

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

        _breathing = GetNodeOrNull<AudioStreamPlayer2D>(BreathingPlayerPath);
        if (_breathing == null)
        {
            GD.PushWarning("Player: Breathing AudioStreamPlayer not found. Add a child node named 'Breathing' or set BreathingPlayerPath.");
        }
        else
        {
            if (MaskBreathingLoop != null)
                _breathing.Stream = MaskBreathingLoop;
            _breathing.Autoplay = false;
            _breathing.VolumeDb = -80f;
            _breathing.ProcessMode = ProcessModeEnum.Inherit;
        }

        _wasMasked = Masked;
        UpdateBreathing(force: true);
    }


    public override void _PhysicsProcess(double delta)
    {
        var deltaPosition = Position - _previousPosition;
        GD.Print(deltaPosition);
        Sprite.FlipH = deltaPosition.X > 0;
        _previousPosition = Position;
        
        if (deltaPosition.IsZeroApprox())
        {
            Sprite.Animation = Masked ? "Maks_Idle_Masked" : "Maks_Idle";
        }
        else
        {
            Sprite.Animation = deltaPosition.Y switch
            {
                < 0 => Masked ? "Maks_Backward_Masked" : "Maks_Backward",
                > 0 => Masked ? "Maks_Forward_Masked" : "Maks_Forward",
                _   => Masked ? "Maks_Walk_Left_Right_Masked" : "Maks_Walk_Left_Right"
            };
        }

        // If input is disabled (puzzle open), stop movement
        if (InputDisabled)
        {
            Velocity = Vector2.Zero;
            MoveAndSlide();
            return;
        }

        UpdateBreathing();
        float x = Input.GetAxis("move_left", "move_right");
        float y = Input.GetAxis("move_up", "move_down");
        Velocity = MovementSpeed * new Vector2(x, y).Normalized();
        MoveAndSlide();
    }

    public void Hurt(float damage)
    {
        if (MaskResource is not null)
        {
            damage = MaskResource.Filter(damage);
        }

        if (Health - damage <= 0 && Health > 0)
        {
            Died?.Invoke();
        }

        Health = Mathf.Max(0, Health - damage);
        HealthChanged?.Invoke(Health);
    }
    private void UpdateBreathing(bool force = false)
    {
        if (_breathing == null)
            return;

        bool nowMasked = Masked;

        if (!force && nowMasked == _wasMasked)
            return;

        _wasMasked = nowMasked;

        if (MaskBreathingLoop != null && _breathing.Stream != MaskBreathingLoop)
            _breathing.Stream = MaskBreathingLoop;

        // Kill previous tween to avoid fighting fades
        _breathingTween?.Kill();
        _breathingTween = null;

        if (nowMasked)
        {
            // Start loop and fade in
            if (!_breathing.Playing)
                _breathing.Play();

            _breathingTween = CreateTween();
            _breathingTween.TweenProperty(_breathing, "volume_db", 0f, BreathingFadeSeconds);
        }
        else
        {
            // Fade out then stop
            _breathingTween = CreateTween();
            _breathingTween.TweenProperty(_breathing, "volume_db", -80f, BreathingFadeSeconds);
            _breathingTween.TweenCallback(Callable.From(() =>
            {
                _breathing?.Stop();
            }));
        }
    }


}