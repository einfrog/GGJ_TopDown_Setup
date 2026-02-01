using System.Collections.Generic;
using Godot;

namespace GGJ_2026.scripts;

public partial class InventoryUi : Control
{

    private readonly Dictionary<InventoryItem, InventoryItemUi> _itemUis = [];

    [Export]
    private PackedScene _itemUiScene;

    [Export]
    private Node _itemUiParent;

    public override void _Ready()
    {
        Player.Instance.Inventory.ItemCollected += item =>
        {
            if (!_itemUis.TryGetValue(item.Item, out var ui))
            {
                ui = _itemUiScene.Instantiate<InventoryItemUi>();
                _itemUiParent.AddChild(ui);
                _itemUis[item.Item] = ui;
            }

            ui.ItemCount++;
            ui.Texture = item.Sprite.Texture;
        };
    }

}