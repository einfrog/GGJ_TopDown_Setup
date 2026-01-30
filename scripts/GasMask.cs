using Godot;

namespace GGJ_2026.scripts;

[GlobalClass]
public partial class GasMask : Node2D
{

    [Export]
    public GasFilter GasFilter { get; set; }

    public float Filter(float damage) => GasFilter.Filter(damage);

}