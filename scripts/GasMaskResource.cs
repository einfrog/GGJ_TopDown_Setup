using Godot;

namespace GGJ_2026.scripts;

[GlobalClass]
public partial class GasMaskResource : Resource
{

    [Export]
    public float Strength { get; set; }

    public float Filter(float damage) => damage * Mathf.Exp(-Strength);
    
}