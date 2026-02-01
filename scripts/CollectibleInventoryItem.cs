using Godot;

namespace GGJ_2026.scripts;

public partial class CollectibleInventoryItem : Interactable
{
    
    [Export]
    public InventoryItem Item { get; private set; }

    public override void Interact()
    {
        Player.Instance.Inventory.Collect(Item);
        QueueFree();
    }

}