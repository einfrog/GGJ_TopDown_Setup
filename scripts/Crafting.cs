using Godot;

namespace GGJ_2026.scripts;

public partial class Crafting : Control
{

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
        CraftTabButton.Pressed += () =>
        {
            CraftTabButton.TextureNormal = PressedButtonTexture;
            UpgradeTabButton.TextureNormal = DisabledButtonTexture;
            ActionButton.Disabled = !Player.Instance.Inventory.CanCraftRadioTransceiver();
        };

        UpgradeTabButton.Pressed += () =>
        {
            CraftTabButton.TextureNormal = DisabledButtonTexture;
            UpgradeTabButton.TextureNormal = PressedButtonTexture;
            ActionButton.Disabled = !Player.Instance.Inventory.CanCraftRadioTransceiver();
        };

        ActionButton.Pressed += () => { };
    }

}