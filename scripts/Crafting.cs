using Godot;

namespace GGJ_2026.scripts;

public partial class Crafting : Control
{

    private bool _craftingTabSelected;

    [Export]
    private VBoxContainer _craftingInput;

    [Export]
    private TextureRect _craftingOutput;

    [Export]
    public TextureButton UpgradeTab { get; set; }

    [Export]
    public TextureButton CraftTabButton { get; set; }

    [Export]
    public TextureButton UpgradeTabButton { get; set; }

    [Export]
    public TextureButton ActionButton { get; set; }

    [Export]
    public Texture2D PressedButtonTexture { get; set; }

    [Export]
    public Texture2D DisabledButtonTexture { get; set; }

    public override void _Ready()
    {
        SelectTab(true);
        CraftTabButton.Pressed += () => SelectTab(true);
        UpgradeTabButton.Pressed += () => SelectTab(false);

        ActionButton.Pressed += () =>
        {
            if (_craftingTabSelected)
                Player.Instance.Inventory.CraftRadioTransceiver();
            else
                Player.Instance.Inventory.UpgradeMask();

            QueueFree();
        };
    }

    public override void _UnhandledKeyInput(InputEvent @event)
    {
        if (@event is InputEventKey { KeyLabel: Key.Escape, Pressed: true })
        {
            QueueFree();
        }
    }

    private void SelectTab(bool craftingTab)
    {
        _craftingTabSelected = craftingTab;

        if (craftingTab)
        {
            CraftTabButton.TextureNormal = PressedButtonTexture;
            UpgradeTabButton.TextureNormal = DisabledButtonTexture;
            ActionButton.Disabled = !Player.Instance.Inventory.CanCraftRadioTransceiver();
        }
        else
        {
            CraftTabButton.TextureNormal = DisabledButtonTexture;
            UpgradeTabButton.TextureNormal = PressedButtonTexture;
            ActionButton.Disabled = !Player.Instance.Inventory.CanUpgradeMask();
        }
    }

}