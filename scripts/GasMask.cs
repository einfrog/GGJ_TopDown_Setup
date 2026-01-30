using Godot;

namespace GGJ_2026.scripts;

public partial class GasMask : Node2D
{

    public GasMaskStrength Strength { get; set; }

    public float Mitigate(float damage)
    {
        // TODO: mitigate
        return damage;
    }

}