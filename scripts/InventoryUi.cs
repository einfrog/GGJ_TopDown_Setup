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
            ui.Item = item.Item;
            ui.Texture = item.Sprite.Texture;
            ui.UpdateUi();
        };

        Player.Instance.Inventory.ItemConsumed += (item, count) =>
        {
            if (!_itemUis.TryGetValue(item, out var ui))
            {
                GD.PushWarning($"Consumed an item that wasn't in inventory ({count}x{item})");
                return;
            }

            int newCount = ui.ItemCount - count;

            switch (newCount)
            {
                case 0:
                    _itemUis.Remove(item);
                    ui.QueueFree();
                    break;
                case > 0:
                    ui.ItemCount = newCount;
                    break;
                case < 0:
                    GD.PushWarning($"Consumed more items than in inventory ({count}x{item})");
                    break;
            }

            ui.UpdateUi();
        };
    }

}