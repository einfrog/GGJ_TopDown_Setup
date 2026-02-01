using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace GGJ_2026.scripts;

public class PlayerInventory
{

    public static bool GameWon { get; set; }

    private static readonly Dictionary<InventoryItem, int> RadioTransceiverRecipe = new()
    {
        [InventoryItem.Antenna] = 1,
        [InventoryItem.Battery] = 1,
        [InventoryItem.ScrapMetal] = 3,
    };

    private readonly Dictionary<InventoryItem, int> _itemCounts = Enum.GetValues<InventoryItem>()
        .ToDictionary(item => item, _ => 0);

    public event Action<InventoryItemCollectible> ItemCollected;

    public event Action<InventoryItem, int> ItemConsumed;

    public void Collect(InventoryItemCollectible item)
    {
        _itemCounts[item.Item]++;
        ItemCollected?.Invoke(item);
    }

    public void UpgradeMask()
    {
        _itemCounts[InventoryItem.MaskUpgradePart]--;
        Player.Instance.MaskResource.Level++;
        ItemConsumed?.Invoke(InventoryItem.MaskUpgradePart, 1);
    }

    public bool CanUpgradeMask()
    {
        return Player.Instance.MaskResource?.Level < Player.Instance.MaskResource?.MaxLevel
               && _itemCounts[InventoryItem.MaskUpgradePart] > 0;
    }

    public void CraftRadioTransceiver()
    {
        foreach (var itemCount in RadioTransceiverRecipe)
        {
            _itemCounts[itemCount.Key] -= itemCount.Value;
            ItemConsumed?.Invoke(itemCount.Key, itemCount.Value);
        }

        GameWon = true;
        GD.Print(GameWon);
        
         Player.Instance.GetTree().ChangeSceneToFile("res://scenes/menu.tscn");
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