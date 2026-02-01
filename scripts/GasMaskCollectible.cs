using Godot;

namespace GGJ_2026.scripts;

[GlobalClass]
public partial class GasMaskCollectible : Interactable
{
    [Export] public GasMaskResource MaskResource { get; set; }
    [Export] public AudioStream EquipSound; // assign directly in inspector

    public override void Interact()
    {
        Player.Instance.MaskResource = MaskResource;

        if (EquipSound != null)
        {
            var p = new AudioStreamPlayer2D
            {
                Stream = EquipSound,
                GlobalPosition = GlobalPosition,
                VolumeDb = -6f
            };

            GetTree().CurrentScene.AddChild(p);
            p.Finished += () => p.QueueFree();
            p.Play();
        }

        QueueFree();
    }
}