using Godot;
using GGJ_2026.scripts.puzzles.core;

namespace GGJ_2026.scripts;

public partial class Crafting : Control
{
    private bool _craftingTabSelected;

    [Export] private VBoxContainer _craftingInput;
    [Export] private TextureRect _craftingOutput;

    [Export] public TextureButton UpgradeTab { get; set; }
    [Export] public TextureButton CraftTabButton { get; set; }
    [Export] public TextureButton UpgradeTabButton { get; set; }
    [Export] public TextureButton ActionButton { get; set; }

    [Export] public Texture2D PressedButtonTexture { get; set; }
    [Export] public Texture2D DisabledButtonTexture { get; set; }

    // NEW: hook to puzzles
    // [Export] public NodePath PuzzleHostPath;
    [Export] public string CraftPuzzleId = "hanoi"; // must match PuzzleHost catalog id

    private PuzzleHost _puzzleHost;
    private bool _openingPuzzle;

    public override void _Ready()
    {
        // Cache PuzzleHost (optional but needed for crafting gate)
        _puzzleHost = GetTree().GetFirstNodeInGroup("puzzle_host") as PuzzleHost;
        if (_puzzleHost == null)
            GD.PushWarning("Crafting: PuzzleHost not found (group 'puzzle_host'). Add PuzzleHost to that group.");

        SelectTab(false);
        CraftTabButton.Pressed += () =>
        {
            GD.Print("Craft tab button pressed");
            SelectTab(true);
        };
        UpgradeTabButton.Pressed += () =>
        {
            GD.Print("Upgrade tab button pressed");
            SelectTab(false);
        };

        ActionButton.Pressed += OnActionPressed;
    }

    private void OnActionPressed()
    {
        if (_openingPuzzle)
            return;

        if (_craftingTabSelected)
        {
            TryCraftRadioViaHanoi();
        }
        else
        {
            // Upgrade stays as-is (no puzzle gate for now)
            if (Player.Instance.Inventory.CanUpgradeMask())
                Player.Instance.Inventory.UpgradeMask();

            Exit();
        }
    }

    public override void _UnhandledKeyInput(InputEvent @event)
    {
        if (@event is InputEventKey { KeyLabel: Key.Escape, Pressed: true })
        {
            Exit();
        }
    }

    private void TryCraftRadioViaHanoi()
    {
        // Must have ingredients
        if (!Player.Instance.Inventory.CanCraftRadioTransceiver())
            return;

        // If host is missing, fail safely (no craft) OR fallback craft.
        // With <1h left, I recommend failing safely so puzzle is required.
        if (_puzzleHost == null)
        {
            GD.PushWarning("Crafting: Cannot open Hanoi puzzle (PuzzleHost missing). Crafting aborted.");
            return;
        }

        _openingPuzzle = true;
        ActionButton.Disabled = true;

        // Optional: hide the crafting UI while puzzle is open so it doesn't overlap.
        // This does NOT free it; it will come back on fail.
        Visible = false;
        MouseFilter = MouseFilterEnum.Ignore;

        // Disable player movement while puzzle is open (consistent with your puzzles)
        Player.Instance.InputDisabled = true;

        bool opened = _puzzleHost.OpenPuzzle(CraftPuzzleId, success =>
        {
            // Puzzle closed, restore player input
            Player.Instance.InputDisabled = false;

            _openingPuzzle = false;

            if (success)
            {
                // Only now do we craft
                Player.Instance.Inventory.CraftRadioTransceiver();
                Exit();
                return;
            }

            // Failed/cancelled: bring crafting UI back so they can try again
            Visible = true;
            MouseFilter = MouseFilterEnum.Stop;

            // Refresh tab state + button enabled state
            SelectTab(true);
        });

        if (!opened)
        {
            // Failed to even open puzzle -> restore UI
            GD.PushWarning($"Crafting: OpenPuzzle('{CraftPuzzleId}') failed.");
            Player.Instance.InputDisabled = false;
            _openingPuzzle = false;

            Visible = true;
            MouseFilter = MouseFilterEnum.Stop;
            SelectTab(true);
        }
    }

    private void SelectTab(bool craftingTab)
    {
        _craftingTabSelected = craftingTab;

        if (craftingTab)
        {
            CraftTabButton.TextureNormal = PressedButtonTexture;
            UpgradeTabButton.TextureNormal = DisabledButtonTexture;
            ActionButton.Disabled = _openingPuzzle || !Player.Instance.Inventory.CanCraftRadioTransceiver();
        }
        else
        {
            CraftTabButton.TextureNormal = DisabledButtonTexture;
            UpgradeTabButton.TextureNormal = PressedButtonTexture;
            ActionButton.Disabled = _openingPuzzle || !Player.Instance.Inventory.CanUpgradeMask();
        }
    }

    private void Exit()
    {
        Player.Instance.InputDisabled = false;
        QueueFree();
    }
}
