using Godot;

namespace GGJ_2026.scripts;

public partial class InventoryItemUi : Control
{

    [Export]
    private TextureRect _textureRect;

    [Export]
    private Label _label;

    public Texture2D Texture
    {
        get;
        set
        {
            field = value;
            _textureRect.Texture = value;
        }
    }

    public int ItemCount
    {
        get;
        set
        {
            field = value;
            _label.Text = $"{value} / {PlayerInventory.GetNecessaryAmountForCraftingRadioTransceiver(Item)}";
        }
    }

    public InventoryItem Item
    {
        get;
        set
        {
            field = value;
            _label.Text = $"{ItemCount} / {PlayerInventory.GetNecessaryAmountForCraftingRadioTransceiver(value)}";
        }
    }

}