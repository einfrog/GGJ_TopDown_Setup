using Godot;
using System;
using System.Collections.Generic;

namespace GGJ_2026.scripts.puzzles.core
{
    public partial class PuzzleHost : Control
    {
        [Export] public Godot.Collections.Array<PuzzleEntry> PuzzleCatalog = new();

        [Export] public NodePath PuzzleLayerPath;
        [Export] public NodePath PlayerPath;

        private Node _player;
        private Node _puzzleLayer;
        private Node _currentPuzzle;
        private Action<bool> _onFinished;

        private readonly Dictionary<string, PackedScene> _sceneById =
            new(StringComparer.OrdinalIgnoreCase);

        public override void _Ready()
        {
            _player = !PlayerPath.IsEmpty ? GetNodeOrNull(PlayerPath) : null;
            _puzzleLayer = !PuzzleLayerPath.IsEmpty ? GetNodeOrNull(PuzzleLayerPath) : this;

            BuildCatalog();
            MouseFilter = MouseFilterEnum.Ignore;
        }

        private void BuildCatalog()
        {
            _sceneById.Clear();

            foreach (var entry in PuzzleCatalog)
            {
                if (entry == null) continue;

                var id = (entry.Id ?? "").Trim();
                if (string.IsNullOrEmpty(id))
                {
                    GD.PushWarning("PuzzleHost: PuzzleEntry has empty Id.");
                    continue;
                }

                if (entry.Scene == null)
                {
                    GD.PushWarning($"PuzzleHost: PuzzleEntry '{id}' has no Scene assigned.");
                    continue;
                }

                if (_sceneById.ContainsKey(id))
                {
                    GD.PushWarning($"PuzzleHost: Duplicate puzzle id '{id}'. Using the first one.");
                    continue;
                }

                _sceneById[id] = entry.Scene;
            }
        }

        public bool IsPuzzleOpen => _currentPuzzle != null;

        public bool OpenPuzzle(string id, Action<bool> onFinished = null)
        {
            if (IsPuzzleOpen) return false;
            if (string.IsNullOrWhiteSpace(id)) return false;

            if (!_sceneById.TryGetValue(id.Trim(), out var scene) || scene == null)
            {
                GD.PushWarning($"PuzzleHost: No puzzle registered for id '{id}'.");
                return false;
            }

            var layer = _puzzleLayer ?? this;
            var puzzle = scene.Instantiate();

            _currentPuzzle = puzzle;
            _onFinished = onFinished;

            layer.AddChild(puzzle);
            SetPlayerEnabled(false);

            if (puzzle.HasSignal("PuzzleFinished"))
                puzzle.Connect("PuzzleFinished", Callable.From<bool>(OnPuzzleFinished));
            else
            {
                GD.PushWarning($"PuzzleHost: Puzzle '{id}' has no signal 'PuzzleFinished(bool)'.");
                ClosePuzzle(false);
                return false;
            }

            return true;
        }

        public void ClosePuzzle(bool success)
        {
            if (_currentPuzzle == null) return;

            _currentPuzzle.QueueFree();
            _currentPuzzle = null;

            SetPlayerEnabled(true);

            var cb = _onFinished;
            _onFinished = null;
            cb?.Invoke(success);
        }

        private void OnPuzzleFinished(bool success) => ClosePuzzle(success);

        private void SetPlayerEnabled(bool enabled)
        {
            if (_player == null) return;
            if (_player.HasMethod("SetInputEnabled"))
                _player.Call("SetInputEnabled", enabled);
        }
    }
}
