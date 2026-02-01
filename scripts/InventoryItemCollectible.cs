using Godot;

namespace GGJ_2026.scripts;

public partial class InventoryItemCollectible : Interactable
{
    [Export]
    public InventoryItem Item { get; private set; }

    [Export]
    public AudioStream PickupSound;


    public Sprite2D Sprite { get; private set; }
    

    public override void _Ready()
    {
        base._Ready();
        Sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
    }

    public override void Interact()
    {
        Player.Instance.Inventory.Collect(this);

        // Hide immediately to avoid double pickup
        if (Sprite != null)
            Sprite.Visible = false;

        PlayPickupSound();

        QueueFree();
    }

    private void PlayPickupSound()
    {
        if (PickupSound == null)
            return;

        var player = new AudioStreamPlayer2D
        {
            Stream = PickupSound,
            GlobalPosition = GlobalPosition,
            VolumeDb = -6f,          // tweak if needed
            PitchScale = 1f
        };

        GetTree().CurrentScene.AddChild(player);
        player.Finished += () => player.QueueFree();
        player.Play();
    }
}