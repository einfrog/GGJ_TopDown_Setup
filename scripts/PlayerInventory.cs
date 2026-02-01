using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace GGJ_2026.scripts;

public class PlayerInventory
{

    private static readonly Dictionary<InventoryItem, int> RadioTransceiverRecipe = new()
    {
        [InventoryItem.Antenna] = 1,
        [InventoryItem.Battery] = 1,
        [InventoryItem.ScrapMetal] = 3,
    };

    private readonly Dictionary<InventoryItem, int> _itemCounts = Enum.GetValues<InventoryItem>()
        .ToDictionary(item => item, _ => 0);

    public event Action<InventoryItemCollectible> ItemCollected;

    public int this[InventoryItem item] => _itemCounts[item];

    public void Collect(InventoryItemCollectible item)
    {
        _itemCounts[item.Item]++;
        ItemCollected?.Invoke(item);
    }

    public void UpgradeMask()
    {
        _itemCounts[InventoryItem.MaskUpgradePart]--;
    }

    public bool CanUpgradeMask()
    {
        return _itemCounts[InventoryItem.MaskUpgradePart] > 0;
    }

    public void CraftRadioTransceiver()
    {
        foreach (var itemCount in RadioTransceiverRecipe)
        {
            _itemCounts[itemCount.Key] -= itemCount.Value;
        }
    }

    public bool CanCraftRadioTransceiver()
    {
        return RadioTransceiverRecipe.Keys.All(HasEnoughForRadioTransceiver);
    }

    public bool HasEnoughForRadioTransceiver(InventoryItem item)
    {
        return _itemCounts[item] >= RadioTransceiverRecipe[item];
    }

}