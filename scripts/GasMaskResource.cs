using Godot;

namespace GGJ_2026.scripts;

[GlobalClass]
public partial class GasMaskResource : Resource
{

    [Export]
    public int Level { get; set; }

    public float Strength => Level;

    public float Filter(float damage) => damage * Mathf.Exp(-Strength);

}