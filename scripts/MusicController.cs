using Godot;
using System;

public partial class MusicController : AudioStreamPlayer2D
{
	[Export] public float MusicVolumeDb = -18f;
	[Export] public float FadeSeconds = 0.3f;
	[Export] public bool StartEnabled = true;

	private Tween _tween;
	public override void _Ready()
	{
		VolumeDb = -80f;

		if (Stream is AudioStreamOggVorbis ogg)
			ogg.Loop = true;

		if (StartEnabled)
			EnableMusic(true, instant: true);
		
	}

	public void ToggleMusic()
	{
		EnableMusic(!Playing);
	}

	public void EnableMusic(bool enabled, bool instant = false)
	{
		_tween?.Kill();
		_tween = null;

		if (enabled)
		{
			if(!Playing)
				Play();
			if (instant)
			{
				VolumeDb = MusicVolumeDb;
				return;
			}

			_tween = CreateTween();
			_tween.TweenProperty(this, "volume_db", MusicVolumeDb, FadeSeconds);
		}
		else
		{
			if (!Playing)
				return;
			if (instant)
			{
				Stop();
				VolumeDb = -80f;
				return;
			}

			_tween = CreateTween();
			_tween.TweenProperty(this, "volume_db", -80f, FadeSeconds);
			_tween.TweenCallback(Callable.From(() =>
			{
				Stop();
			}));
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
