using Godot;

namespace GGJ_2026.scripts;

public partial class GasMaskLevel : Control
{

    [Export]
    private TextureRect _textureRect;

    public override void _Ready()
    {
        Player.Instance.MaskEquipped += mask => _textureRect.Texture = mask.Texture;
    }

}