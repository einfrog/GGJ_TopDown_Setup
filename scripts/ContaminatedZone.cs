using System;
using Godot;

namespace GGJ_2026.scripts;

[GlobalClass]
public partial class ContaminatedZone : Area2D
{

    private Action _playerHurtAction;

    private int _playerHurtCount;

    [Export]
    public Timer DamageTimer { get; set; }

    [Export]
    public Curve DamageCurve { get; set; }

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    private void OnBodyEntered(Node body)
    {
        if (body is Player player)
        {
            _playerHurtAction = () => player.Hurt(DamageCurve.Sample(_playerHurtCount++));
            DamageTimer.Timeout += _playerHurtAction;
            DamageTimer.Start();
        }
    }

    private void OnBodyExited(Node body)
    {
        if (body is Player player)
        {
            DamageTimer.Stop();
            DamageTimer.Timeout -= _playerHurtAction;
            _playerHurtAction = null;
        }
    }

}