using Godot;

namespace GGJ_2026.scripts;

public partial class PlayerInteract2D : Node2D
{
    [Export] public string InteractAction = "interact";

    private Node _currentInteractable;

    public override void _UnhandledInput(InputEvent e)
    {
        if (!e.IsActionPressed(InteractAction))
            return;

        if (_currentInteractable == null)
            return;

        if (_currentInteractable.HasMethod("Interact"))
            _currentInteractable.Call("Interact");
    }

    public void SetCurrentInteractable(Node interactable)
    {
        _currentInteractable = interactable;
    }

    public void ClearCurrentInteractable(Node interactable)
    {
        if (_currentInteractable == interactable)
            _currentInteractable = null;
    }
}
