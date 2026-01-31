using Godot;

namespace GGJ_2026.scripts;

[GlobalClass]
public partial class GasMask : Area2D, IInteractable
{

    [Export]
    public float Strength { get; set; }

    public float Filter(float damage) => damage * Mathf.Exp(-Strength);

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    private void OnBodyEntered(Node body)
    {
        if (body is not Player player)
            return;
		
        player.Interactor.SetCurrentInteractable(this);
		
    }
	
    private void OnBodyExited(Node body)
    {
        if (body is not Player player)
            return;

        player.Interactor.ClearCurrentInteractable(this);
    }

    public void Interact()
    {
        Player.Instance.Mask = this;
        Visible = false;
        GD.Print("interact with mask");
    }
    
}