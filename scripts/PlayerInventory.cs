using System;
using System.Collections.Generic;
using System.Linq;

namespace GGJ_2026.scripts;

public class PlayerInventory
{

    private static readonly Dictionary<InventoryItem, int> RadioTransceiverRecipe = new()
    {
        [InventoryItem.Antenna] = 1,
        [InventoryItem.Battery] = 1,
        [InventoryItem.ScrapMetal] = 3,
    };

    private readonly Dictionary<InventoryItem, int> _itemCount = Enum.GetValues<InventoryItem>()
        .ToDictionary(item => item, _ => 0);

    public bool CanCraftRadioTransceiver()
    {
        return RadioTransceiverRecipe.Keys.All(HasEnoughForRadioTransceiver);
    }

    public bool HasEnoughForRadioTransceiver(InventoryItem item)
    {
        return _itemCount[item] >= RadioTransceiverRecipe[item];
    }

    public void Collect(InventoryItem item)
    {
        _itemCount[item]++;
    }

}