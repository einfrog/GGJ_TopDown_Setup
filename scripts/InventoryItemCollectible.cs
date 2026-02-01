using Godot;

namespace GGJ_2026.scripts;

public partial class InventoryItemCollectible : Interactable
{

    [Export]
    public InventoryItem Item { get; private set; }

    public Sprite2D Sprite { get; private set; }

    public override void _Ready()
    {
        base._Ready();
        Sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
    }

    public override void Interact()
    {
        Player.Instance.Inventory.Collect(this);
        QueueFree();
    }

}