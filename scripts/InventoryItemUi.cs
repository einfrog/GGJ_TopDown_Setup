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

    public int ItemCount { get; set; }

    public InventoryItem Item { get; set; }

    public void UpdateUi()
    {
        int necessary = PlayerInventory.GetNecessaryAmountForCraftingRadioTransceiver(Item);
        _label.Text = (necessary == 0) ? ItemCount.ToString() : $"{ItemCount} / {necessary}";
    }

}