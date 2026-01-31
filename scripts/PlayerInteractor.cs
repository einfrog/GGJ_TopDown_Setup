using Godot;

namespace GGJ_2026.scripts;

public partial class PlayerInteractor : Node2D
{

    private IInteractable _currentInteractable;

    [Export]
    public string InteractAction = "interact";

    public override void _UnhandledInput(InputEvent e)
    {
        if (!e.IsActionPressed(InteractAction))
        {
            return;
        }

        _currentInteractable?.Interact();
    }

    public void SetCurrentInteractable(IInteractable interactable)
    {
        _currentInteractable = interactable;
    }

    public void ClearCurrentInteractable(IInteractable interactable)
    {
        if (_currentInteractable == interactable)
        {
            _currentInteractable = null;
        }
    }

}