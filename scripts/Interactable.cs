using Godot;

namespace GGJ_2026.scripts;

public abstract partial class Interactable : Area2D
{

    public abstract void Interact();

    public override void _Ready()
    {
        BodyEntered += body => { if (body is Player) OnPlayerEntered(); };
        BodyExited += body => { if (body is Player) OnPlayerExited(); };
    }

    protected virtual void OnPlayerEntered()
    {
        Player.Instance.Interactor.SetCurrentInteractable(this);
    }

    protected virtual void OnPlayerExited()
    {
        Player.Instance.Interactor.ClearCurrentInteractable(this);
    }

}