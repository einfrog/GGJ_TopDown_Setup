using Godot;

namespace GGJ_2026.scripts;

[GlobalClass]
public partial class GasMaskCollectible : Interactable
{

    [Export]
    public GasMaskResource MaskResource { get; set; }

    public override void Interact()
    {
        Player.Instance.MaskResource = MaskResource;
        QueueFree();
    }
    
}