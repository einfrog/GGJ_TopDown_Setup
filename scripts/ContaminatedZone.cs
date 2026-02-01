using Godot;

namespace GGJ_2026.scripts;

[GlobalClass]
public partial class ContaminatedZone : Area2D
{

    [Export]
    public float DamagePerHit { get; set; }

    [Export]
    public Timer DamageTimer { get; set; }

    public override void _Ready()
    {
        DamageTimer.Timeout += () => Player.Instance.Hurt(DamagePerHit);
        DamageTimer.Paused = true;
        DamageTimer.Start();
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    private void OnBodyEntered(Node body)
    {
        if (body is Player)
        {
            DamageTimer.Paused = false;
            // Player.Instance.RegenerationTimer.Paused = true;
        }
    }

    private void OnBodyExited(Node body)
    {
        if (body is Player)
        {
            DamageTimer.Paused = true;
            // Player.Instance.RegenerationTimer.Paused = false;
        }
    }

}