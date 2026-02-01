using System;
using System.Collections.Generic;
using System.Linq;

namespace GGJ_2026.scripts;

public class PlayerInventory
{
    
    public static bool GameWon = false;

    private static readonly Dictionary<InventoryItem, int> RadioTransceiverRecipe = new()
    {
        [InventoryItem.Antenna] = 1,
        [InventoryItem.Battery] = 1,
        [InventoryItem.ScrapMetal] = 3,
    };

    private readonly Dictionary<InventoryItem, int> _itemCounts = Enum.GetValues<InventoryItem>()
        .ToDictionary(item => item, _ => 0);

    public event Action<InventoryItemCollectible> ItemCollected;

    public void Collect(InventoryItemCollectible item)
    {
        _itemCounts[item.Item]++;
        ItemCollected?.Invoke(item);
    }

    public void UpgradeMask()
    {
        _itemCounts[InventoryItem.MaskUpgradePart]--;
        Player.Instance.MaskResource.Level++;
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
        
        GameWon = true;

        // TODO: make Radio Trainsceiver appear large on screen
    }

    public bool CanCraftRadioTransceiver()
    {
        return RadioTransceiverRecipe.Keys.All(HasEnoughForRadioTransceiver);
    }

    public bool HasEnoughForRadioTransceiver(InventoryItem item)
    {
        return _itemCounts[item] >= RadioTransceiverRecipe.GetValueOrDefault(item, 0);
    }

    public static int GetNecessaryAmountForCraftingRadioTransceiver(InventoryItem item)
    {
        return RadioTransceiverRecipe.GetValueOrDefault(item, 0);
    }

}