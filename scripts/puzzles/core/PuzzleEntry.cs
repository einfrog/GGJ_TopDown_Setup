using Godot;

namespace GGJ_2026.scripts.puzzles.core
{
    [GlobalClass]
    public partial class PuzzleEntry : Resource
    {
        [Export] public string Id = "";
        [Export] public PackedScene Scene;
    }
}