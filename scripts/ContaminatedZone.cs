using System;
using Godot;

namespace GGJ_2026.scripts;

[GlobalClass]
public partial class ContaminatedZone : Area2D
{

    private int _playerHurtCount;

    [Export]
    public Timer DamageTimer { get; set; }

    [Export]
    public Curve DamageCurve { get; set; }

    public override void _Ready()
    {
        DamageTimer.Timeout += () => Player.Instance.Hurt(DamageCurve.Sample(++_playerHurtCount));
        DamageTimer.Paused = true;
        DamageTimer.Start();
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    private void OnBodyEntered(Node body)
    {
        if (body == Player.Instance)
        {
            DamageTimer.Paused = false;
        }
    }

    private void OnBodyExited(Node body)
    {
        if (body == Player.Instance)
        {
            DamageTimer.Paused = true;
        }
    }

}