using Godot;
using GGJ_2026.scripts.puzzles.core;

namespace GGJ_2026.scripts.world;

public partial class Door : StaticBody2D
{
    [Export] public NodePath TerminalPath;

    [Export] public AnimatedSprite2D Sprite;
    [Export] public CollisionShape2D Collision;

    // ---- NEW: door sound ----
    [Export] public AudioStreamPlayer2D DoorSfx;
    [Export] public string OpenAnimation = "open";

    // If true, collision disappears when the open anim finishes (recommended)
    // If false, collision disappears immediately when opening starts
    [Export] public bool DisableCollisionOnAnimEnd = true;

    private PuzzleTerminal2D _terminal;
    private bool _isOpen;

    public override void _Ready()
    {
        Sprite ??= GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
        Collision ??= GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
        DoorSfx ??= GetNodeOrNull<AudioStreamPlayer2D>("DoorSfx");

        // Listen for anim finish if we want collision to disable at the end
        if (Sprite != null)
            Sprite.AnimationFinished += OnSpriteAnimationFinished;

        if (!TerminalPath.IsEmpty)
        {
            _terminal = GetNodeOrNull<PuzzleTerminal2D>(TerminalPath);
            if (_terminal != null)
            {
                _terminal.Solved += OnTerminalSolved;
            }
            else
            {
                GD.PushWarning($"Door '{Name}': Terminal not found at {TerminalPath}");
            }
        }
    }

    public override void _ExitTree()
    {
        if (_terminal != null)
            _terminal.Solved -= OnTerminalSolved;

        if (Sprite != null)
            Sprite.AnimationFinished -= OnSpriteAnimationFinished;
    }

    private void OnTerminalSolved()
    {
        Open();
    }

    public void Open()
    {
        if (_isOpen)
            return;

        _isOpen = true;

        // 1) Play sound
        float soundLen = 0f;
        if (DoorSfx != null)
        {
            // Ensure stream is assigned in inspector to DoorSfx.Stream
            var stream = DoorSfx.Stream;
            if (stream != null)
                soundLen = (float)stream.GetLength();

            DoorSfx.Stop();
            DoorSfx.Play();
        }

        // 2) Play animation (optionally match its duration to the sound)
        if (Sprite != null)
        {
            if (soundLen > 0.01f)
                MatchAnimToDuration(Sprite, OpenAnimation, soundLen);
            else
                Sprite.SpeedScale = 1f; // fallback

            Sprite.Play(OpenAnimation);
        }

        // 3) Collision handling
        if (!DisableCollisionOnAnimEnd)
        {
            if (Collision != null)
                Collision.Disabled = true;
        }
        // else: collision disabled in OnSpriteAnimationFinished
    }

    private void OnSpriteAnimationFinished()
    {
        if (!DisableCollisionOnAnimEnd)
            return;

        if (Sprite == null)
            return;

        // Only disable collision after the OPEN animation finishes
        if (Sprite.Animation != OpenAnimation)
            return;

        if (Collision != null)
            Collision.Disabled = true;
    }

    private static void MatchAnimToDuration(AnimatedSprite2D sprite, string animName, float desiredSeconds)
    {
        if (sprite.SpriteFrames == null)
            return;

        if (!sprite.SpriteFrames.HasAnimation(animName))
            return;

        int frameCount = sprite.SpriteFrames.GetFrameCount(animName);
        float fps = (float)sprite.SpriteFrames.GetAnimationSpeed(animName);

        if (frameCount <= 0 || fps <= 0f)
            return;

        float currentDuration = frameCount / fps; // seconds at SpeedScale = 1
        if (currentDuration <= 0.001f)
            return;

        // newDuration = currentDuration / SpeedScale
        // => SpeedScale = currentDuration / desiredDuration
        sprite.SpeedScale = currentDuration / desiredSeconds;
    }
}
