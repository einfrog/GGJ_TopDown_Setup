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
            UpdateTexture(Texture, value);
            UpdateTexture(NextLevelTexture, value + 1);
            LevelChanged?.Invoke(value);
        }
    } = 1;

    [Export]
    public int MaxLevel { get; set; } = 5;

    [Export]
    public AtlasTexture Texture
    {
        get;
        private set
        {
            field = value;
            UpdateTexture(value, Level);
        }
    }

    [Export]
    public AtlasTexture NextLevelTexture
    {
        get;
        private set
        {
            field = value;
            UpdateTexture(value, Level + 1);
        }
    }

    private void UpdateTexture(AtlasTexture texture, int level)
    {
        level %= MaxLevel + 1;
        texture?.Region = new Rect2(32 * (level % 3), 24 * (level / 3), 32, 24);
    }

    public event Action<int> LevelChanged;

    public float Strength => Level;

    public float Filter(float damage) => damage * Mathf.Exp(-Strength);

}