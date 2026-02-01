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
    // Breathing
    [Export] public NodePath BreathingPlayerPath = "Breathing";
    [Export] public AudioStream MaskBreathingLoop;
    [Export] public float BreathingFadeSeconds = 0.15f;
    
    // Footsteps
    [Export] public NodePath FootstepsPlayerPath = "Footsteps";
    [Export] public AudioStream[] FootstepClips = Array.Empty<AudioStream>();

    [Export] public float StepIntervalSeconds = 0.32f;

    [Export] public float FootstepPitchMin = 0.95f;
    [Export] public float FootstepPitchMax = 1.05f;

    [Export] public float FootstepVolumeJitterDb = 1.5f; // +/- dB
    [Export] public float FootstepBaseVolumeDb = -6f;
    
    
    private AudioStreamPlayer2D _breathing;
    private Tween _breathingTween;
    private bool _wasMasked;
    
    
    private AudioStreamPlayer2D _footsteps;
    private double _stepTimer;
    private readonly RandomNumberGenerator _stepRng = new();
    private int _lastFootstepIndex = -1;
    
    public float Health { get; set; }

    public bool InputDisabled { get; set; }

    public GasMaskResource MaskResource
    {
        get;
        set
        {
            field = value;
            MaskEquipped?.Invoke(value);
        }
    }

    private bool Masked => MaskResource != null;

    private float MovementSpeedThreshold => MovementSpeed / 2;

    public PlayerInventory Inventory { get; } = new();

    public static Player Instance { get; private set; }

    public event Action Died;

    public event Action<GasMaskResource> MaskEquipped;

    public event Action<float> HealthChanged;
    
    [Export]
    public Timer RegenerationTimer { get; set; }
    
    public static bool RunEndedInDeath { get; set; }

    public override void _EnterTree()
    {
        if (Instance is not null)
        {
            GD.PushError("There can only be one player");
            return;
        }

        Instance = this;
    }
    
    public override void _ExitTree()
    {
        if (Instance == this)
            Instance = null;
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
        
        _stepRng.Randomize();

        _footsteps = GetNodeOrNull<AudioStreamPlayer2D>(FootstepsPlayerPath);
        if (_footsteps == null)
        {
            GD.PushWarning("Player: Footsteps AudioStreamPlayer not found. Add a child named 'Footsteps' or set FootstepsPlayerPath.");
        }
        else
        {
            _footsteps.Autoplay = false;
            _footsteps.VolumeDb = FootstepBaseVolumeDb;
        }

        RegenerationTimer.Paused = true;
        RegenerationTimer.Timeout += () =>
        {
            Health = Mathf.Min(MaxHealth, Health + 1);
            HealthChanged?.Invoke(Health);
        };
        RegenerationTimer.Start();

        _stepTimer = 0;
    }


    public override void _PhysicsProcess(double delta)
    {
        string animation;
        var deltaPosition = (Position - _previousPosition) / (float) delta;
        _previousPosition = Position;

        if (deltaPosition.IsZeroApprox())
        {
            animation = "Maks_Idle";
        }
        else
        {
            if (deltaPosition.Y < -MovementSpeedThreshold)
                animation = "Maks_Backward";
            else if (deltaPosition.Y > MovementSpeedThreshold)
                animation = "Maks_Forward";
            else
                animation = "Maks_Walk_Left_Right";
        }

        Sprite.Animation = (Masked) ? animation + "_Masked" : animation;
        Sprite.FlipH = deltaPosition.X > MovementSpeedThreshold;
        UpdateBreathing();
        
        float x = 0f;
        float y = 0f;

        if (!InputDisabled)
        {
            x = Input.GetAxis("move_left", "move_right");
            y = Input.GetAxis("move_up", "move_down");
        }

        Vector2 inputDir = new Vector2(x, y);
        bool wantsToMove = !InputDisabled && !inputDir.IsZeroApprox();

        // --- footsteps: use input intent (stable), not deltaPosition (jittery) ---
        UpdateFootsteps(delta, wantsToMove);

        // --- existing movement code ---
        if (InputDisabled)
        {
            Velocity = Vector2.Zero;
        }
        else
        {
            Velocity = MovementSpeed * inputDir.Normalized();
        }
        
        MoveAndSlide();
    }

    public void Hurt(float damage)
    {
        if (MaskResource is not null)
        {
            damage = MaskResource.Filter(damage);
        }
        
        Health = Mathf.Max(0, Health - damage);
        HealthChanged?.Invoke(Health);

        if (Health <= 0)
        {
            RunEndedInDeath = true;
            Died?.Invoke();
            GetTree().ChangeSceneToFile("res://scenes/menu.tscn");
        }
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
            _breathingTween.TweenCallback(Callable.From(() => _breathing?.Stop()));
        }
    }

    private void UpdateFootsteps(double delta, bool wantFootsteps)
    {
        if (_footsteps == null)
            return;

        if (!wantFootsteps)
        {
            _stepTimer = 0;
            return;
        }

        if (FootstepClips == null || FootstepClips.Length == 0)
            return;

        _stepTimer -= delta;
        if (_stepTimer > 0)
            return;

        PlayRandomFootstep();
        _stepTimer = StepIntervalSeconds;
    }



    private void PlayRandomFootstep()
    {
        int count = FootstepClips.Length;
        int index = _stepRng.RandiRange(0, count - 1);
        if (count > 1 && index == _lastFootstepIndex)
            index = (index + 1) % count;

        _lastFootstepIndex = index;

        var clip = FootstepClips[index];
        if (clip == null)
            return;

        _footsteps.Stream = clip;

        _footsteps.PitchScale = _stepRng.RandfRange(FootstepPitchMin, FootstepPitchMax);

        float volJitter = _stepRng.RandfRange(-FootstepVolumeJitterDb, FootstepVolumeJitterDb);
        _footsteps.VolumeDb = FootstepBaseVolumeDb + volJitter;

        _footsteps.Play();
    }

}