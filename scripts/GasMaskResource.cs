using System;
using Godot;

namespace GGJ_2026.scripts;

[GlobalClass]
public partial class GasMaskResource : Resource
{

    [Export]
    public int Level
    {
        get;
        set
        {
            field = value;
            Texture?.Region = new Rect2(32 * (Level % 3), 24 * (Level / 3), 32, 24);
            LevelChanged?.Invoke(value);
        }
    }

    [Export]
    public AtlasTexture Texture
    {
        get;
        private set
        {
            field = value;
            field?.Region = new Rect2(32 * (Level % 3), 24 * (Level / 3), 32, 24);
        }
    }

    public event Action<int> LevelChanged;

    public float Strength => Level;

    public float Filter(float damage) => damage * Mathf.Exp(-Strength);

}