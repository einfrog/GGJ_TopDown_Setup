using Godot;

namespace GGJ_2026.scripts;

public partial class CraftingTable : Interactable
{
    
    [Export]
    private Node _craftingSceneParent;
    
    [Export]
    private PackedScene _craftingScene;

    public override void Interact()
    {
        _craftingSceneParent.AddChild(_craftingScene.Instantiate());
    }

}